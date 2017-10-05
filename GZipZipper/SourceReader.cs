using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace VeeamZipper
{
    class SourceReader
    {
        public static readonly AutoResetEvent blockReadEvent = new AutoResetEvent(false);
        Compressor compressor;

        public SourceReader(Compressor comp)
        {
            compressor = comp;
        }

        public void Start()
        {
            try
            {
                _Start();
            }
            catch (Exception ex)
            {
                Logger.error(ex.Message);
                Program.IsCancelled = true;
            }
        }
        private void _Start()
        {
            int count;
            byte[] buffer = new byte[Compressor.READ_BLOCKS_SIZE];
            while ((count = compressor.sourceStream.Read(buffer, 0, buffer.Length)) > 0 && !Program.IsCancelled)
            {
                if (compressor.blocksQueue.CountLock() >= Compressor.MAX_BLOCKS_COUNT)
                {
                    while (compressor.blocksQueue.CountLock() >= Compressor.MAX_BLOCKS_COUNT)
                    {
                        ZipProcessor.zipperEvent.WaitOne(50);
                    }
                }
                if (count < Compressor.READ_BLOCKS_SIZE)
                {
                    var b = new byte[count];
                    Array.Copy(buffer, b, count);
                    buffer = b;
                }
                compressor.blocksQueue.Enqueue(buffer);
                blockReadEvent.Set();
            }
            Compressor.IsReadingComplete = true;
        }
    }
}
