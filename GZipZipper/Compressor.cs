using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Linq;

namespace VeeamZipper
{
    internal class Compressor : IZipProcessor, IDisposable
    {
        public static int CORES_COUNT = ZipUtil.CoresCount;
        public static int READ_BLOCKS_SIZE = 4096 * 1024;
        public static int MAX_BLOCKS_COUNT = CORES_COUNT;
        public FileStream destStream = null;
        public Stream sourceStream = null;
        public ZipQueue blocksQueue = new ZipQueue();
        public Dictionary<int, byte[]> zippedBlocks = new Dictionary<int, byte[]>();
                
        public static bool IsReadingComplete = false;
        public static bool IsGZipComplete = false;

        public bool Perform(string source, string destination)
        {
            var sw = new Stopwatch();
            using (sourceStream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read, READ_BLOCKS_SIZE))
            {
                sw.Start();                
                using (destStream = File.Create(destination))
                {
                    try
                    {
                        if (sourceStream.Length < 50 * 1024 * 1024) //if < 50mb read in single thread
                        {
                            using (var gzip = new GZipStream(destStream, CompressionMode.Compress))
                            {
                                int count;
                                var buffer = new byte[READ_BLOCKS_SIZE];
                                while ((count = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    gzip.Write(buffer, 0, buffer.Length);
                                }
                            }
                        }
                        else
                            ParallelCompress();
                    }
                    catch(Exception ex)
                    {
                        Logger.error("Error while archivating", ex);
                        return false;
                    }
                }
            }            
            sw.Stop();
            Logger.debug("compress [" + source + "] elaspsed time: " + sw.ElapsedMilliseconds);            
            return !Program.IsCancelled;
        }

      
        private void ParallelCompress()
        {
            var sr = new SourceReader(this);
            var rw = new ResultWriter(this);

            var readThread = new Thread(sr.Start);
            var writeThread = new Thread(rw.Start);

            var zipThreads = new Thread[CORES_COUNT];
            for (var i = 0; i < zipThreads.Length; i++)
                zipThreads[i] = new Thread((new ZipProcessor(this)).Start);
            
            readThread.Start();
            for (var i = 0; i < zipThreads.Length; i++)            
                zipThreads[i].Start();
            
            writeThread.Start();


            for (var i = 0; i < zipThreads.Length; i++)
            {
                zipThreads[i].Join();
            }            
            IsGZipComplete = true;
            writeThread.Join();
        }

        #region IDisposable Support
        private bool _isDisposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;
            if (disposing)
            {
                ((IDisposable)destStream).Dispose();
                ((IDisposable)sourceStream).Dispose();
            }
            zippedBlocks = null;
            _isDisposed = true;
        }
        
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
