using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VeeamZipper
{
    class Program
    {
        const Int32 SUCCESS_EXIT_CODE = 0;
        const Int32 ERROR_EXIT_CODE = 1;
        const String COMPRESS_COMMAND = "compress";
        const String DECOMPRESS_COMMAND = "decompress";
        const String INCORRECT_COMMAND_ERROR = "Use one of command: " + COMPRESS_COMMAND + " | " + DECOMPRESS_COMMAND;
        const String INCORRECT_FORMAT_ERROR = "Use command line query in format: \"[command] [source file name] [archive name]\"";

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
                showError(INCORRECT_COMMAND_ERROR);
            else if(args.Length != 3)            
                showError(INCORRECT_FORMAT_ERROR);
            if (args[0].Equals(COMPRESS_COMMAND, StringComparison.OrdinalIgnoreCase))
                processor = new Compressor();
            else if (args[0].Equals(DECOMPRESS_COMMAND, StringComparison.OrdinalIgnoreCase))
                processor = new Decompressor();
            else
                showError("Unindefined command " + args[0] + " (expect one of: " + COMPRESS_COMMAND + "/" + DECOMPRESS_COMMAND + ")");
            if(!File.Exists(args[1]))
                showError("File " + args[1] + " is not found");
            if (new FileInfo(args[1]).Length < 10)
                showError("File is too short");
            try
            {
                if (processor.perform(args[1], args[2]))            
                    showSuccess("File was successfully created: " + args[2]);      
                else
                    Console.ReadKey();
                    Environment.Exit(ERROR_EXIT_CODE);
            }
            catch(Exception ex)
            {
                Logger.error("Error in process", ex);
            }
        }

        static void showError(String text)
        {
            Console.WriteLine(text);
            Logger.error(text);
            Console.ReadKey();
            Environment.Exit(ERROR_EXIT_CODE);
        }

        static void showSuccess(String text)
        {
            Console.WriteLine(text);
            Console.ReadKey();
            Environment.Exit(ERROR_EXIT_CODE);
        }
    }
}
