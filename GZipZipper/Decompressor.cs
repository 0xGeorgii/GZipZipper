using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;

namespace VeeamZipper
{
    class Decompressor : IZipProcessor
    {                       
        private const int BLOCK_SIZE = 4096 * 1024;

        public bool Perform(string source, string destination)
        {
            var sw = new Stopwatch();
            using (var sourceStream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read, BLOCK_SIZE))
            {
                using(var destStream = File.Create(destination))
                {
                    var blockNumber = 1;
                    while (!Program.IsCancelled)
                    {
                        byte[] lenAr = new byte[4];                    
                        var l = sourceStream.Read(lenAr, 0, 4);
                        if (l <= 0) break;
                        var len = BitConverter.ToInt32(lenAr, 0);
                        var buffer = new byte[len];
                        var count = sourceStream.Read(buffer, 0, len);
                        if (count <= 0) break;
                        Console.WriteLine("Blocks lenght: " + len + " number: " + blockNumber++);
                        var b = ZipUtil.Decompress(buffer);
                        destStream.Write(b, 0, b.Length);                        
                    }                    
                }                
            }
            Logger.debug("decompress [" + source + "] elaspsed time: " + (sw.ElapsedMilliseconds / 1000));
            return true;
        }
    }
}
