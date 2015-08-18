using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace FxAdvisorCore.Util
{
    internal class BlockingQueue<T> 
    {
        private object _sync = new object();

        private T queuedObj;
        private bool isEnqueued;
        private bool enabled = true;

        public bool Enabled
        {
            get { return enabled; }
            set 
            {
                lock (_sync)
                {
                    enabled = value;
                    if(!enabled)
                        Monitor.PulseAll(_sync);
                }
            }
        }

        public T Dequeue()
        {
            lock (_sync)
            {
                while (!isEnqueued && enabled)
                    Monitor.Wait(_sync);
                if (enabled)
                {
                    isEnqueued = false;
                    return queuedObj;
                }
                else
                    return default(T);
            }
        }

        public void Enqueue(T obj)
        {
            lock (_sync)
            {
                while (isEnqueued && enabled)
                    Monitor.Wait(_sync);
                if (enabled)
                {
                    this.queuedObj = obj;
                    this.isEnqueued = true;
                    Monitor.Pulse(_sync);
                }
            }
        }
    }
}
