using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TestServer
{
    class SemaphoreEx
    {
        Semaphore semaphore;
        volatile int counter = 0;
        object obj = new object();
 
        public SemaphoreEx(int initialCount, int maximumCount)
        {
            semaphore = new Semaphore(initialCount, maximumCount);
        }

        public void WaitOne()
        {
            lock (obj)
            {
                counter++;
            }
            semaphore.WaitOne();
        }

        public void Release()
        {
            lock (obj)
            {
                counter--;
            }
            semaphore.Release();
        }

        public void WaitJobFinished()
        {
            while (counter != 0)
                Thread.Sleep(100);
        }
    }
}
