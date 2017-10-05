using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace VeeamZipper
{    
    class ZipProcessor
    {
        public static readonly AutoResetEvent zipperEvent = new AutoResetEvent(false);
        public static readonly AutoResetEvent queueDepletedEvent = new AutoResetEvent(false);
        Compressor compressor;

        public ZipProcessor(Compressor comp)
        {
            compressor = comp;
        }

        public void Start()
        {
            ZipQueue.ZipBlock zb = null;
            try
            {
                while (!Program.IsCancelled)
                {
                    if (Compressor.IsReadingComplete && compressor.blocksQueue.CountLock() == 0)
                        break;
                    else
                    {
                        if (compressor.blocksQueue.CountLock() == 0)
                        {
                            SourceReader.blockReadEvent.WaitOne(20);
                            continue;
                        }
                        zb = compressor.blocksQueue.DequeueLock();
                        if (compressor.blocksQueue.CountLock() < Compressor.MAX_BLOCKS_COUNT)
                        {
                            queueDepletedEvent.Set();
                        }
                        if (zb == null) continue;
                    }
                    //Console.WriteLine("zipThread " + Thread.CurrentThread.Name + " read buffer block [" + zb.number + "]");
                    var b = ZipUtil.Compress(zb.data);
                    lock (compressor.zippedBlocks)
                    {
                        compressor.zippedBlocks.Add(zb.number, b);
                        zipperEvent.Set();
                    }
                }
            }
            catch (OutOfMemoryException ex)
            {
                /*
                in this place we wait out of memory exception,
                so catch it, return block in queue and continue to perform,
                also, we may add here counter to catch situation when threads will crush one by one,
                mean than os couldnt allock more memory for our proccess and we cant zip
                */
                GC.Collect();
                Logger.error(ex.Message);
                compressor.blocksQueue.Enqueue(zb);
            }
        }
    }
}
