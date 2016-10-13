using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VeeamZipper
{
    static class Logger
    {
        public const int TRACE = 1;
        public const int DEBUG = 2;
        public const int INFO = 3;
        public const int WARN = 4;
        public const int ERROR = 4;
        public const int FATAL = 5;

        public static int AlertLevel = INFO;
        public static Encoding encoding = Encoding.GetEncoding(1251);
        const String LOG_FILENAME = "log.txt";
        static StreamWriter logWriter;

        static Logger()
        {
            var dir = Path.Combine(Environment.CurrentDirectory, "logs");
            Directory.CreateDirectory(dir);
            String fname = dir + Path.DirectorySeparatorChar + LOG_FILENAME;
            logWriter = new StreamWriter(fname, true, encoding);
        }
        
        private static void writeLine(String alertLevel, String msg)
        {
            if (logWriter == null) return;
            String s = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "][" + alertLevel + "]" + msg;
            lock (logWriter)
            {
                logWriter.WriteLine(s);
                logWriter.Flush();
            }
        }

        public static void trace(String msg)
        {
            if (AlertLevel > TRACE) return;
            writeLine("[TRACE]", msg);
        }

        public static void debug(String msg)
        {
            if (AlertLevel > DEBUG) return;
            writeLine("DEBUG", msg);
        }

        public static void info(String msg)
        {
            if (AlertLevel > INFO) return;
            writeLine("INFO", msg);
        }

        public static void warn(String msg)
        {
            if (AlertLevel > WARN) return;
            writeLine("WARN", msg);
        }

        public static void error(String msg)
        {
            if (AlertLevel > ERROR) return;
            writeLine("ERROR", msg);
        }

        public static void error(String msg, Exception ex)
        {
            if (AlertLevel > ERROR) return;
            writeLine("ERROR", msg + "\r\n" + ex.Message + "\r\n" + ex.StackTrace);
            Exception inner = ex.InnerException;
            while (inner != null)
            {
                writeLine("ERROR", "Inner exception: " + inner.Message + "\r\nв " +
                    inner.Source + "\r\n" + inner.StackTrace);
                inner = inner.InnerException;
            }
        }

        public static void fatal(String msg)
        {
            if (AlertLevel > FATAL) return;
            writeLine("FATAL", msg);
        }

        public static void fatal(String msg, Exception ex)
        {
            if (AlertLevel > FATAL) return;
            writeLine("FATAL", msg + "\r\n" + ex.Message + "\r\n" + ex.StackTrace);
            Exception inner = ex.InnerException;
            while (inner != null)
            {
                writeLine("FATAL", "Inner exception: " + inner.Message + "\r\nв " +
                    inner.Source + "\r\n" + inner.StackTrace);
                inner = inner.InnerException;
            }
        }
    }
}
