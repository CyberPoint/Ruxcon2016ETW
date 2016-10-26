using ETW_IE_InfoLeak_Demo_Parser;
using Microsoft.Diagnostics.Tracing;
using NDesk.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETW_IE_InfoLeak_Demo_CLI
{
    class Program
    {
        static bool show_help = false;
        static string outputFilename = null;
        static string debugFilename = null;
        static string etlFilename = null;
        static bool prettyPrint = false;
        static TextWriter outputFile = null;
        static JsonTextWriter outputWriter = null;

        static void Main(string[] args)
        {

            var p = new OptionSet() {
                { "o|output-file=", "the {FILENAME} to write extracted event data to. Outputs to STDOUT if not specified.", f => outputFilename = f },
                { "d|debug-file=", "the {FILENAME} to write all parsed events to.", f => debugFilename = f },
                { "p|pretty-print", "enable pretty printing JSON output.", v => prettyPrint = v != null },
                { "i|input-etl-file=", "the {FILENAME} of an ETL file to read instead of consuming events in real time.", f => etlFilename = f },
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

            IE_Demo_Parser parser = new IE_Demo_Parser();

            Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e) { parser.Stop(); };

            if (etlFilename == null)
                parser.CreateSession();
            else
                parser.CreateSource(etlFilename);

            if (debugFilename != null)
            {
                StreamWriter debugFile = new StreamWriter(debugFilename);
                parser.EventCallback += delegate (IE_Demo_Parser.EventData data)
                {
                    debugFile.WriteLine(data.InfoString + "\r\n");
                };
            }

            if (outputFilename != null)
            {
                outputFile = new StreamWriter(outputFilename);
                Console.Error.WriteLine("Starting to process events. Press Control-C to stop.");
            }
            else
            {
                outputFile = Console.Out;
            }

            outputWriter = new JsonTextWriter(outputFile);

            if (prettyPrint)
                outputWriter.Formatting = Formatting.Indented;
            else
                outputWriter.Formatting = Formatting.None;

            outputWriter.WriteStartArray();

            parser.ExtractedDataCallback += delegate (JObject data, int type)
            {
                if (!prettyPrint)
                    outputWriter.WriteWhitespace("\r\n");
                data.WriteTo(outputWriter);
                outputWriter.Flush();
            };

            if (etlFilename == null)
                parser.EnableProvider(TraceEventLevel.Verbose);

            parser.Process();

            if (!prettyPrint)
                outputWriter.WriteWhitespace("\r\n");

            outputWriter.WriteEndArray();
            outputWriter.Flush();
            outputWriter.Close();
            Console.Error.WriteLine();
            Console.Error.WriteLine(parser.NumParsedEvents + " events processed");
            Console.Error.WriteLine(parser.NumMissedEvents + " events failed parsing");
        }

        static void ShowHelp(OptionSet p)
        {
            Console.Error.WriteLine("Usage: " + System.AppDomain.CurrentDomain.FriendlyName + " [OPTIONS]");
            Console.Error.WriteLine("Demonstrates the use of ETW to collect private information from Internet Explorer without modifying the browser.");
            Console.Error.WriteLine();
            Console.Error.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Error);
        }
    }
}
