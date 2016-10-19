// ETW POC for detecting Ransomware. Works on both live captures and ETL capture files

using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using Microsoft.Diagnostics.Tracing.Session;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NDesk.Options;

namespace ETW_POC
{

    // Main logic for detection:
    //
    // When a write event occurs we check if there is a corresponding read event for the same pid
    // if so we check the delta in time stamps and see if it's under the allowable threshold
    // Lastly we check if the change in file size (delta) is within threshold
    // 
    // For more information see our slides:
    // https://ruxcon.org.au/
    //
    // check for correlation:
    // Filename exists in readList (same file name usually means encryption is done in-place)
    // Filname minus extension exists in readList (File extension changed)
    //
    public class DirectoryEventTracker
    {
        // CONSTANTS - Threasholds. See slides for details
        /////////////////////////////////////////////
        public static double IO_DELTA_THRESHOLD = 80;
        //Biggest seen so far is 544 (nano)
        public static int READ_WRITE_SIZE_DIFF_THRESHOLD = 1024;
        //Suspcious event count per Directory
        public static int SUSPICOUS_EVENTS_THRESHOLD = 3;

        //Write all debug output here
        public static TextWriter Out;
        
        public int pid;
        public int suspiciousEventCount;
        public ArrayList readEvents;
        public ArrayList writeEvents;
        public ArrayList suspiciousWriteEvents;
        public double firstReadTime;
        public double lastReadTime;
        public double firstWriteTime;
        public double lastWriteTime;
        public string dirName;
        public string processName;

        public DirectoryEventTracker(int pid, double time, string dirName, string processName, StreamWriter output)
        {
            readEvents = new ArrayList();
            writeEvents = new ArrayList();
            suspiciousWriteEvents = new ArrayList();
            this.dirName = dirName;
            this.pid = pid;
            this.processName = processName;
            Out = output;  
        }
        public void addReadEvent(FileIOReadWriteTraceData readEvent)
        {
            //first check for correlation
            string currFileName = Path.GetFileNameWithoutExtension(readEvent.FileName);

            ReadEventTracker existingReadEvent = null;
            foreach (ReadEventTracker ret in readEvents)
            {
                if (Path.GetFileNameWithoutExtension(ret.fileName).ToLower() == currFileName.ToLower())
                {
                    existingReadEvent = ret;
                    break;
                }
            }

            //new read event
            if (existingReadEvent == null)
            {
                ReadEventTracker ret = new ReadEventTracker(readEvent);
                readEvents.Add(ret);
            }
            else
            {
                readEvents.Remove(existingReadEvent);
                existingReadEvent.updateEvent(readEvent);
                readEvents.Add(existingReadEvent);
            }
        }
        
        public void addWriteEvent(FileIOReadWriteTraceData writeEvent)
        {
            //first check for correlation
            string currFileName = Path.GetFileNameWithoutExtension(writeEvent.FileName);

            ReadEventTracker correspondingReadEvent = null;
            int corrReadEvent = -1;
            int i = 0;
            foreach (ReadEventTracker ret in readEvents)
            {
                if (Path.GetFileNameWithoutExtension(ret.fileName).ToLower() == currFileName.ToLower() )
                {
                    corrReadEvent = i;
                    correspondingReadEvent = ret;
                    break;
                }
                i++;
            }
            // There are times when we receive writes before reads are detected. The obvious case is
            // when the file written to has changed it's name drastically and we fail to recognize it.
            // For now these events are dropped.
            if (corrReadEvent == -1)
            {
                return;
            }
            
            // check timestamp
            // doesn't matter if the writeEvent is new or old it still needs to be under the threshold
            if (writeEvent.TimeStampRelativeMSec - correspondingReadEvent.eventTime > IO_DELTA_THRESHOLD)
            {
                Out.WriteLine("Timestamp for incoming writeEvent is outside of threshold [" + (writeEvent.TimeStampRelativeMSec - correspondingReadEvent.eventTime).ToString() + "]. Disregarding: " + writeEvent.FileName);
                return;
            }

            //////////////////////////////////////
            // at this point we know there has been a Read and a write to the same file, from the same process
            // and it was within the timing threshold. All that's left is to check the size of the read first 
            // the file write.
            //////////////////////////////////////

            // Check if previous event exist
            WriteEventTracker tempWE = null;
            foreach (WriteEventTracker wet in writeEvents)
            {
                if (writeEvent.FileName == wet.fileName)
                {
                    tempWE = wet;
                    break;
                }
            }

            //if it's new
            if (tempWE == null)
            {   
                //update time stamps
                firstWriteTime = writeEvent.TimeStampRelativeMSec;
                lastWriteTime = writeEvent.TimeStampRelativeMSec;

                bool suspicious = false;
                if (writeEvent.IoSize - correspondingReadEvent.fileSize >= 0 && writeEvent.IoSize - correspondingReadEvent.fileSize <= READ_WRITE_SIZE_DIFF_THRESHOLD)
                {
                    suspicious = true;
                    Out.WriteLine("[!] Suspicious write event detected! " + writeEvent.FileName);
                    Out.WriteLine("\tSize of file write(s): " + writeEvent.IoSize.ToString() + " Size of file read(s): " + correspondingReadEvent.fileSize.ToString() + ". PID: " + writeEvent.ProcessID.ToString() + ". Process name: " + writeEvent.ProcessName);

                    //if we encounter a certain number of suspicious events we call it ransomware!
                    if (suspiciousWriteEvents.Count >= SUSPICOUS_EVENTS_THRESHOLD)
                    {
                        Out.WriteLine("[!!] Ransomware detected! - PID[" + writeEvent.ProcessID.ToString() + "] Process Name: " + writeEvent.ProcessName);
                    }

                    //don't add duplicates
                    bool dup = false;
                    foreach (WriteEventTracker swe in suspiciousWriteEvents)
                    {
                        if (swe.fileName == writeEvent.FileName)
                        {
                            dup = true;
                            Out.WriteLine("\t Already detected entry, not added");
                        }
                    }
                    if (!dup)
                    {
                        suspiciousWriteEvents.Add(new WriteEventTracker(writeEvent, false));
                    }
                }
                WriteEventTracker wet = new WriteEventTracker(writeEvent, suspicious);
                writeEvents.Add(wet);
            }
            //update existing write event
            else
            {
                //remove old before we add in our updated version
                writeEvents.Remove(tempWE);

                //update time stamps
                firstWriteTime = writeEvent.TimeStampRelativeMSec;
                lastWriteTime = writeEvent.TimeStampRelativeMSec;

                //update time, file size with current time then check
                tempWE.updateEvent(writeEvent);
                
                if (tempWE.fileSize - correspondingReadEvent.fileSize >= 0 && tempWE.fileSize - correspondingReadEvent.fileSize <= READ_WRITE_SIZE_DIFF_THRESHOLD)
                {
                    Out.WriteLine("[!] Suspicious write event on existing file: " + writeEvent.FileName);
                    Out.WriteLine("\tSize of file write(s): " + tempWE.fileSize.ToString() + " Size of file read(s): " + correspondingReadEvent.fileSize.ToString() +". PID: " + correspondingReadEvent.pid.ToString() + ". Process name: " + writeEvent.ProcessName);

                    //if we encounter a certain number of suspicious events we call it cryptolocker
                    if (suspiciousWriteEvents.Count >= SUSPICOUS_EVENTS_THRESHOLD)
                    {
                        Out.WriteLine("[!!] Ransomware detected! - PID[" + writeEvent.ProcessID.ToString() + "] Process Name: " + writeEvent.ProcessName);
                    }

                    //don't add duplicates
                    bool dup = false;
                    foreach (WriteEventTracker swe in suspiciousWriteEvents)
                    {
                        if (swe.fileName == tempWE.fileName)
                        {
                            dup = true;
                            Out.WriteLine("\tAlready detected entry, not added");
                        }
                    }
                    if (!dup)
                    {
                        suspiciousWriteEvents.Add(tempWE);
                    }
                }
                writeEvents.Add(tempWE);
            }
        }
    }

    // Track File reads.
    public class ReadEventTracker
    {
        public bool isCorrelated;
        public int pid;
        public double eventTime;
        public string fileName;
        public long fileSize;
        public ReadEventTracker(FileIOReadWriteTraceData data)
        {
            isCorrelated = false;
            fileSize = data.IoSize;
            eventTime = data.TimeStampRelativeMSec;
            fileName = data.FileName;
            pid = data.ProcessID;
        }

        public void updateEvent(FileIOReadWriteTraceData data)
        {
            if (data.IoSize >= fileSize)
            {
                fileSize = data.IoSize;
                eventTime = data.TimeStampRelativeMSec;
            }
        }
    }

    // class for tracking write events as well as updating the list time a file was written.
    public class WriteEventTracker
    {
        public bool isCorrelated;
        public int correlatedReadEvent;
        public bool isSuspiciousEvent;
        public int pid;
        public double firstWrite;
        public double lastWrite;
        public string fileName;
        public int fileSize;
        public WriteEventTracker(FileIOReadWriteTraceData data, bool isSuspicious=false)
        {
            isCorrelated = false;
            correlatedReadEvent = -1;
            isSuspiciousEvent = isSuspicious;

            pid = data.ProcessID;
            fileName = data.FileName;           
        }

        //update time and file size.
        public void updateEvent(FileIOReadWriteTraceData data, bool isSuspicious = false)
        {
            if (data.Offset + data.IoSize > fileSize)
            {
                fileSize = (int)data.Offset + data.IoSize;
                lastWrite = data.TimeStampRelativeMSec;
            }
        }
    }

    class Program
    {
        private static bool DO_READ_WRITE = true;

        public static TextWriter Out = new StreamWriter("debug.log");
        static TraceEventSession s_kernelSession;
        static bool s_stopping = false;

        // Table of read/write operations that occur. Key is directory name as returned by Path.GetDirectoryName()
        // There needs to be one for each PID responsible for the IO
        public static Hashtable directoryOperations = new Hashtable();
        
        // Array List of of the PIDs insids the ArrayList of direcotryOperations. This is purely for convience
        public static ArrayList pidList = new ArrayList();

        //when a file is written check to see if it's the same PID that read it then check time stamp
        public static void fileWriteEvent(FileIOReadWriteTraceData writeEvent)
        {
            string currDir = Path.GetDirectoryName(writeEvent.FileName);

            if (pidList.Contains(writeEvent.ProcessID))
            {
                if (directoryOperations.ContainsKey(currDir))
                {
                    //add our write event to the existing entry
                    DirectoryEventTracker tempDirEvent = (DirectoryEventTracker)directoryOperations[currDir];
                    tempDirEvent.addWriteEvent(writeEvent);
                    directoryOperations[currDir] = tempDirEvent;
                }
                else
                {
                    //existing pid, new dir. Make new entry
                    DirectoryEventTracker tempDirEvent = new DirectoryEventTracker(writeEvent.ProcessID, writeEvent.TimeStampRelativeMSec, currDir, writeEvent.ProcessName, (StreamWriter)Out);
                    tempDirEvent.addWriteEvent(writeEvent);
                    directoryOperations[currDir] = tempDirEvent;
                }
            }
            else
            {
                //otherwise add it to the list and create a new entry in dirOps
                pidList.Add(writeEvent.ProcessID);
                DirectoryEventTracker tempDirEvent = new DirectoryEventTracker(writeEvent.ProcessID, writeEvent.TimeStampRelativeMSec, currDir, writeEvent.ProcessName, (StreamWriter)Out);
                tempDirEvent.addWriteEvent(writeEvent);
                directoryOperations[currDir] = tempDirEvent;
            }
        }

        //For each fileRead event that occurs we must check if we have a directory entry for it
        public static void fileReadEvent(FileIOReadWriteTraceData readEvent)
        {
            //Avoid cached files for some apps (see IE_plus.zip_test.etl.zip)
            if (!Path.HasExtension(readEvent.FileName))
            {
                Out.WriteLine("No extension, disregarding file read for: " + readEvent.FileName);
                return;
            }

            string currDir = Path.GetDirectoryName(readEvent.FileName);
            //first check if we have a directory entry for the incoming pid 
            if (pidList.Contains(readEvent.ProcessID))
            {
                //if PID exists append check for previos ops in the dir
                if (directoryOperations.ContainsKey(currDir))
                {
                    //if there's allready a read event for this Dir and PID just add the new one to the list
                    //and update the hashtable
                    DirectoryEventTracker tempDirEvent = (DirectoryEventTracker)directoryOperations[currDir];
                    tempDirEvent.addReadEvent(readEvent);
                    directoryOperations[currDir] = tempDirEvent;
                }
                else
                {
                    //otherwise make a new entry for it and add in the read event
                    DirectoryEventTracker tempDirEvent = new DirectoryEventTracker(readEvent.ProcessID, readEvent.TimeStampRelativeMSec, currDir, readEvent.ProcessName, (StreamWriter)Out);
                    tempDirEvent.addReadEvent(readEvent);
                    directoryOperations[currDir] = tempDirEvent;
                }

            }
            else
            {
                //otherwise add it to the list and create a new entry in dirOps
                pidList.Add(readEvent.ProcessID);
                DirectoryEventTracker tempDirEvent = new DirectoryEventTracker(readEvent.ProcessID, readEvent.TimeStampRelativeMSec, currDir, readEvent.ProcessName, (StreamWriter)Out);
                tempDirEvent.addReadEvent(readEvent);
                directoryOperations[currDir] = tempDirEvent;
            }

        }

        public static void Run()
        {
            if (TraceEventSession.IsElevated() != true)
            {
                Console.WriteLine("Must be elevated (Admin) to run this program.");
                Debugger.Break();
                return;
            }
            
            // Set up Ctrl-C to stop both user mode and kernel mode sessions
            Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs cancelArgs) =>
            {
                StopSessions();
                cancelArgs.Cancel = true;
            };
            Console.WriteLine("Start monitoring for ransomware file activity");
            var task1 = Task.Run(() =>
            {
                using (s_kernelSession = new TraceEventSession(KernelTraceEventParser.KernelSessionName))
                {
                    //This is the best spot to examine for efficiency improvments
                    //As you might expect, listening to large amounts of kernel events can be slow
                    s_kernelSession.EnableKernelProvider(
                        KernelTraceEventParser.Keywords.All
                        );
                    s_kernelSession.Source.Kernel.All += FilterIOEvents;
                    // process events until Ctrl-C is pressed
                    s_kernelSession.Source.Process();
                }
                Console.WriteLine("Thread 1 dieing");
            });
            Task.WaitAll(task1);
        }

        static void StopSessions()
        {
            s_stopping = true;
            Out.WriteLine("Insuring all ETW sessions are stopped.");
            if (s_kernelSession != null)
                s_kernelSession.Dispose();
        }

        // To improve performace we drop IO events that are multiples of 4096
        // This helps filter out large IO events with the obvious disadvantage
        // of possibly overlooking needed data. In testing we found that rarely
        // happened and the benefit far outweighed (small) risk.
        static void FilterIOEvents(TraceEvent data)
        {
            // Ctrl-C will stop the sessions, but buffered events may still come in, ignore these.  
            if (s_stopping)        
                return;

            if (data.GetType().Name == "FileIOReadWriteTraceData")
            {
                FileIOReadWriteTraceData parsedData = (FileIOReadWriteTraceData)data;

                // Sometimes filenames can be blank. Not exactly sure why
                // See nano sample for example
                if (parsedData.FileName.Length > 0)
                {
                    if (DO_READ_WRITE)
                    {
                        if (parsedData.OpcodeName == "Read")
                        {
                            if (parsedData.IoSize % 4096 == 0)
                            {
                                return;
                            }
                            else
                            {
                                fileReadEvent(parsedData);
                            }
                        }
                        if (parsedData.OpcodeName == "Write")
                        {
                            if (parsedData.IoSize % 4096 == 0)
                            {
                                return;
                            }
                            else
                            {
                                fileWriteEvent(parsedData);
                            }    
                        }
                    }
                        
                }
            }
        }
        
        // This is borrowed from the ETW example code provided by MS. Very little is changed
        static void DataProcessing(string dataFileName, string outFileName)
        {
            Out.Flush();
            Out = new StreamWriter(outFileName);

            Out.WriteLine("Opening the output file and printing the results.");
            Out.WriteLine("The list is filtered quite a bit...");
            using (var source = new ETWTraceEventSource(dataFileName))
            {
                if (source.EventsLost != 0)
                    Out.WriteLine("WARNING: there were {0} lost events", source.EventsLost);

                // Set up callbacks to 
                source.Kernel.All += FilterIOEvents;
                
                var symbolParser = new SymbolTraceEventParser(source);
                symbolParser.All += FilterIOEvents;

                //Begin blocking thread for ETW processing.
                source.Process();
                Out.WriteLine("Done Processing.");
            }
        }
        
        // Loop through data and print summary. 
        public static List<Tuple<string, int>> printSummary()
        {
            Out.WriteLine("");
            Out.WriteLine("");
            Out.WriteLine("======================================================");
            Out.WriteLine("\t\tSummary");
            Out.WriteLine("======================================================");

            List<string> ransomeWareProcesses = new List<string>();
            List<Tuple<string, int>> summaryProcess = new List<Tuple<string, int>>();
            foreach (DictionaryEntry de in directoryOperations)
            {
                DirectoryEventTracker det = (DirectoryEventTracker)directoryOperations[de.Key];
                if (det.suspiciousWriteEvents.Count >= DirectoryEventTracker.SUSPICOUS_EVENTS_THRESHOLD)
                {
                    //only print the new processes, shouldn't have more than one
                    if (!ransomeWareProcesses.Contains(det.processName))
                    {
                        Out.WriteLine("Ransomware behavior detected in process : [" + det.processName + "] PID [" + det.pid.ToString() +"]");
                        ransomeWareProcesses.Add(det.processName);
                        summaryProcess.Add(Tuple.Create(det.processName, det.pid));
                    }
                }
            }
            return summaryProcess;
        }

        static void ShowHelp(OptionSet p)
        {
            Console.Error.WriteLine("Usage: " + System.AppDomain.CurrentDomain.FriendlyName + " [OPTIONS]");
            Console.Error.WriteLine("Use ETW to detect ransomware");
            Console.Error.WriteLine();
            Console.Error.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Error);
        }

        static void Main(string[] args)
        {
            #region arg processing
            bool show_help = false;
            bool DO_DYNAMIC_CAPTURE = false;
            string DO_ETL_FILE = "";
            string DO_BATCH_CAPTURE = "";

            var p = new OptionSet() {
                { "d|dynamic", "Capture live ransomware events", v => DO_DYNAMIC_CAPTURE = v != null},
                { "etlFile=", "Process ETL capture file", f => DO_ETL_FILE = f},
                { "batchDir=", "Batch process ETLs. Should be a directory with ETL files", f => DO_BATCH_CAPTURE = f},
                { "h|help",  "show this message and exit", v => show_help = v != null },
            };
            List<string> extra;
            try
            {
                extra = p.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write(System.AppDomain.CurrentDomain.FriendlyName + ": ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `" + System.AppDomain.CurrentDomain.FriendlyName + " --help` for more information.");
                return;
            }
            if (show_help)
            {
                ShowHelp(p);
                return;
            }
            #endregion arg processing

            //Caputure events dynamically
            if (DO_DYNAMIC_CAPTURE)
            {
                Console.WriteLine("Doing Live Capture. Watch out for malware!");
                Run();
            }
            //Check if ransomware included in ETL
            else if (DO_ETL_FILE.Length != 0)
            {
                if (File.Exists(DO_ETL_FILE))
                {
                    Console.WriteLine("Parsing ETL for ransomware");
                    DataProcessing(DO_ETL_FILE, "dev_test.log");
                    
                    List <Tuple<string, int>> procList = new List<Tuple<string, int>>();
                    TextWriter summaryFile = new StreamWriter("summary.log");
                    procList = printSummary();
                    Out.Flush();

                    //add entry to summary
                    summaryFile.WriteLine("==============================================");
                    summaryFile.WriteLine("Results for trace file: " + DO_ETL_FILE);
                    summaryFile.WriteLine("==============================================");
                    foreach (Tuple<string, int> proc in procList)
                    {
                        summaryFile.WriteLine("\t Process name: " + proc.Item1 + " PID [" + proc.Item2.ToString() + "]");
                    }
                    summaryFile.Flush();
                }   
            }
            else if (DO_BATCH_CAPTURE.Length != 0)
            {
                if (Directory.Exists(DO_BATCH_CAPTURE))
                {
                    Console.WriteLine("Doing batch capture parsing");
                    string dir = DO_BATCH_CAPTURE;
                    var ext = new List<string> { ".etl" };
                    TextWriter summaryFile = new StreamWriter("summary.log");
                    List<Tuple<string, int>> procList = new List<Tuple<string, int>>();
                    var myFiles = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories).Where(s => ext.Any(e => s.EndsWith(e)));
                    foreach (string file in myFiles)
                    {
                        Console.Out.WriteLine("Parsing capture: " + file);
                        DataProcessing(file, file + ".log");
                        procList = printSummary();
                        Out.Flush();

                        //add entry to summary
                        summaryFile.WriteLine("==============================================");
                        summaryFile.WriteLine("Results for trace file: " + file);
                        summaryFile.WriteLine("==============================================");
                        foreach (Tuple<string, int> proc in procList)
                        {
                            summaryFile.WriteLine("\t Process name: " + proc.Item1 + " PID [" + proc.Item2.ToString() + "]");
                        }
                        //reset the tracking structures each time
                        procList = new List<Tuple<string, int>>();
                        directoryOperations = new Hashtable();
                        pidList = new ArrayList();
                        summaryFile.Flush();
                    }
                }
                else
                {
                    Console.WriteLine("Directory doesn't exist!");
                    ShowHelp(p);
                    return;
                }
            }
            Console.Out.WriteLine("done");
            Out.Flush();
        }
    }
}
