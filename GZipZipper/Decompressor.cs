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
        const int blockSize = 4096 * 1024;

        public bool perform(string source, string destination)
        {
            var sw = new Stopwatch();
            using (var sourceStream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read, blockSize))
            {
                using(var destStream = File.Create(destination))
                {
                    int count;
                    int blockNumber = 1;
                    int len = 0;
                    while (!Program.IsCancelled)
                    {
                        byte[] lenAr = new byte[4];                    
                        var l = sourceStream.Read(lenAr, 0, 4);
                        if (l <= 0) break;
                        len = BitConverter.ToInt32(lenAr, 0);
                        var buffer = new byte[len];
                        count = sourceStream.Read(buffer, 0, len);
                        if (count <= 0) break;
                        Console.WriteLine("Blocks lenght: " + len + " number: " + blockNumber++);
                        var b = ZipUtil.decompress(buffer);
                        destStream.Write(b, 0, b.Length);                        
                    }                    
                }                
            }
            Logger.debug("decompress [" + source + "] elaspsed time: " + (sw.ElapsedMilliseconds / 1000));
            return true;
        }
    }
}
