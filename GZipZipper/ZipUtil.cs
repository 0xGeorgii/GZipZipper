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
        private const int BUFF_SIZE = 65536;
        public static int CoresCount = Environment.ProcessorCount;
        private static object _ziplock = new object();
        
        public static byte[] Compress(byte[] buffer)
        {
            using (var ms = new MemoryStream())
            using (var gzip = new GZipStream(ms, CompressionMode.Compress))
            {
                gzip.Write(buffer, 0, buffer.Length);
                return ms.ToArray();
            }
        }

        public static byte[] Decompress(byte[] content)
        {
            using (var ms = new MemoryStream())
            {
                var zip = new GZipStream(new MemoryStream(content), CompressionMode.Decompress);
                CopyStream(zip, ms);
                return ms.ToArray();
            }            
        }

        public static void CopyStream(Stream instream, Stream outstream)
        {
            const long mb = 10 * 1024 * 1024;
            var buffer = new byte[BUFF_SIZE];
            long pos = 0;
            long c = 0;
            while (true)
            {
                var k = instream.Read(buffer, 0, BUFF_SIZE);
                pos += k;
                if (k == 0) break;
                outstream.Write(buffer, 0, k);
                var c1 = pos / mb;
                if (c1 > c)
                {
                    c = c1;
                }
            }
            outstream.Flush();
        }
    }
}
