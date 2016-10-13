using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace VeeamZipper
{
    class LockedQueue<T> : Queue<T>
    {
        private object o = new object();        

        public void syncPush(T item)
        {
            lock(o)
            {
                base.Enqueue(item);
            }
        }

        public T syncPop()
        {
            T item;
            lock (o)
            {
                item = base.Dequeue();
            }
            return item;
        }
    }
}
