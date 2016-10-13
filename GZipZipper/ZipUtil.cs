using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace VeeamZipper
{
    public static class ZipUtil
    {
        public static int coresCount = Environment.ProcessorCount;
        private static object ziplock = new object();
        
        public static byte[] compress(byte[] buffer)
        {
            using (MemoryStream ms = new MemoryStream())
            using (var gzip = new GZipStream(ms, CompressionMode.Compress))
            {
                gzip.Write(buffer, 0, buffer.Length);
                gzip.Close();
                return ms.ToArray();
            }
        }

        public static byte[] decompress(byte[] content)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                GZipStream zip = new GZipStream(new MemoryStream(content), CompressionMode.Decompress);
                copyStream(zip, ms);
                return ms.ToArray();
            }            
        }

        public static void copyStream(Stream instream, Stream outstream)
        {
            long MB = 10 * 1024 * 1024;
            byte[] buffer = new byte[65536];
            long pos = 0;
            long c = 0;
            while (true)
            {
                int k = instream.Read(buffer, 0, 65536);
                pos += k;
                if (k == 0) break;
                outstream.Write(buffer, 0, k);
                var c1 = pos / MB;
                if (c1 > c)
                {
                    c = c1;
                }
            }
            outstream.Flush();
        }
    }
}
