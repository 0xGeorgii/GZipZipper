using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace VeeamZipper
{
    class ResultWriter
    {
        Compressor compressor;

        public ResultWriter(Compressor comp)
        {
            compressor = comp;
        }

        public void start()
        {
            try
            {
                _start();
            }
            catch(Exception ex)
            {
                Logger.error(ex.Message);
                Program.IsCancelled = true;
            }
        }
        private  void _start()
        {
            byte[] item;
            int nextBlockNumber = 1;
            while (!Program.IsCancelled)
            {
                item = null;
                lock (compressor.blocksQueue)
                {
                    if (compressor.zippedBlocks.ContainsKey(nextBlockNumber))
                    {
                        item = compressor.zippedBlocks[nextBlockNumber++];
                        compressor.zippedBlocks.Remove(nextBlockNumber - 1);
                    }
                }
                if (item == null)
                {
                    ZipProcessor.zipperEvent.WaitOne(20);
                    continue;
                }

                //Console.WriteLine("writeThread: writing zipped blocks [" + (blockNumber - 1) + "]");
                int len = item.Length;
                compressor.destStream.WriteByte((byte)(len & 0xff));
                compressor.destStream.WriteByte((byte)((len >> 8) & 0xff));
                compressor.destStream.WriteByte((byte)((len >> 16) & 0xff));
                compressor.destStream.WriteByte((byte)((len >> 24) & 0xff));
                //Console.WriteLine("block length [" + len + "]");
                compressor.destStream.Write(item, 0, len);
                Console.Write(".");
                if (Compressor.isGZipComplete && compressor.zippedBlocks.Count == 0)
                {
                    Console.Write("\r\n");
                    break;
                }
            }
        }
    }
}
