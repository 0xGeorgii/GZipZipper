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
        private readonly Compressor _compressor;

        public ResultWriter(Compressor comp)
        {
            _compressor = comp;
        }

        public void Start()
        {
            try
            {
                _Start();
            }
            catch(Exception ex)
            {
                Logger.error(ex.Message);
                Program.IsCancelled = true;
            }
        }
        private  void _Start()
        {
            byte[] item;
            int nextBlockNumber = 1;
            while (!Program.IsCancelled)
            {
                item = null;
                lock (_compressor.blocksQueue)
                {
                    if (_compressor.zippedBlocks.ContainsKey(nextBlockNumber))
                    {
                        item = _compressor.zippedBlocks[nextBlockNumber++];
                        _compressor.zippedBlocks.Remove(nextBlockNumber - 1);
                    }
                }
                if (item == null)
                {
                    ZipProcessor.zipperEvent.WaitOne(20);
                    continue;
                }

                //Console.WriteLine("writeThread: writing zipped blocks [" + (blockNumber - 1) + "]");
                int len = item.Length;
                _compressor.destStream.WriteByte((byte)(len & 0xff));
                _compressor.destStream.WriteByte((byte)((len >> 8) & 0xff));
                _compressor.destStream.WriteByte((byte)((len >> 16) & 0xff));
                _compressor.destStream.WriteByte((byte)((len >> 24) & 0xff));
                //Console.WriteLine("block length [" + len + "]");
                _compressor.destStream.Write(item, 0, len);
                Console.Write(".");
                if (Compressor.IsGZipComplete && _compressor.zippedBlocks.Count == 0)
                {
                    Console.Write("\r\n");
                    break;
                }
            }
        }
    }
}
