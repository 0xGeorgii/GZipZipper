using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VeeamZipper
{
    class Program
    {
        private const int SUCCESS_EXIT_CODE = 0;
        private const int ERROR_EXIT_CODE = 1;
        private const string COMPRESS_COMMAND = "compress";
        private const string DECOMPRESS_COMMAND = "decompress";
        private const string INCORRECT_COMMAND_ERROR = "Use one of command: " + COMPRESS_COMMAND + " | " + DECOMPRESS_COMMAND;
        private const string INCORRECT_FORMAT_ERROR = "Use command line query in format: \"[command] [source file name] [archive name]\"";

        static IZipProcessor processor = null;
        public static bool IsCancelled = false;
        static void Main(string[] args)
        {
            //Logger.AlertLevel = Logger.DEBUG; //если нужно, включить необходимый уровень логгера
            Console.CancelKeyPress += (sender, a) =>
            {
                IsCancelled = true;
                Console.WriteLine("App was shutted down");
                Console.ReadKey();
                Environment.Exit(ERROR_EXIT_CODE);
            };

            if(args.Length == 0)            
                ShowError(INCORRECT_COMMAND_ERROR);
            else if(args.Length != 3)            
                ShowError(INCORRECT_FORMAT_ERROR);
            if (args[0].Equals(COMPRESS_COMMAND, StringComparison.OrdinalIgnoreCase))
                processor = new Compressor();
            else if (args[0].Equals(DECOMPRESS_COMMAND, StringComparison.OrdinalIgnoreCase))
                processor = new Decompressor();
            else
                ShowError("Unindefined command " + args[0] + " (expect one of: " + COMPRESS_COMMAND + "/" + DECOMPRESS_COMMAND + ")");
            if(!File.Exists(args[1]))
                ShowError("File " + args[1] + " is not found");
            if (new FileInfo(args[1]).Length < 10)
                ShowError("File is too short");
            try
            {
                var processorResult = false;
                if (processor is IDisposable)
                {
                    using (processor as IDisposable)
                    {
                        processorResult = processor.Perform(args[1], args[2]);
                    }
                }
                else
                {
                    processorResult = processor.Perform(args[1], args[2]);
                }
                if (processorResult)
                    ShowSuccess("File was successfully created: " + args[2]);
                else
                {
                    Console.ReadKey();
                    Environment.Exit(ERROR_EXIT_CODE);
                }
            }
            catch(Exception ex)
            {
                Logger.error("Error in process", ex);
            }
        }

        static void ShowError(string text)
        {
            Console.WriteLine(text);
            Logger.error(text);
            Console.ReadKey();
            Environment.Exit(ERROR_EXIT_CODE);
        }

        static void ShowSuccess(string text)
        {
            Console.WriteLine(text);
            Console.ReadKey();
            Environment.Exit(ERROR_EXIT_CODE);
        }
    }
}
