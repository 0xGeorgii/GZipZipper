using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace VeeamZipper
{
    class ZipQueue : Queue<ZipQueue.ZipBlock>
    {
        public class ZipBlock
        {
            private static int counter = 0;
            public int number;
            public byte[] data;
            
            public ZipBlock(byte[] d)
            {
                Interlocked.Increment(ref counter);
                number = counter;
                data = new byte[d.Length];
                Array.Copy(d, data, d.Length);
            }
        }

        readonly object _locker = new object();

        //
        // Summary:
        //     Adds an object to the end of the System.Collections.Generic.Queue`1.
        //     Locks on
        // Parameters:
        //   item:
        //     The object to add to the System.Collections.Generic.Queue`1. The value can be
        //     null for reference types.
        public void Enqueue(byte[] d)
        {
            lock(_locker)
            {
                base.Enqueue(new ZipBlock(d));
            }
        }

        new public void Enqueue(ZipBlock zipBlock)
        {
            lock(_locker)
            {
                base.Enqueue(zipBlock);
            }
        }

        public ZipBlock DequeueLock()
        {
            ZipBlock item;
            lock (_locker)
            {
                if (Count == 0) return null;
                item = base.Dequeue();
            }
            return item;
        }

        public int CountLock()
        {
            int i;
            lock(_locker)
            {
                i = base.Count;
            }
            return i;
        }

    }
}
