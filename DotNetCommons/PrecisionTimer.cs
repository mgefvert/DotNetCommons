using System;
using System.Threading;

namespace DotNetCommons
{
    public class PrecisionTimer
    {
        private readonly object _lock = new object();
        private readonly Action _callback;
        private readonly int _millis;
        protected Thread WaitThread;

        public PrecisionTimer(Action callback, int millis)
        {
            _callback = callback;
            _millis = millis;
        }

        public bool Enabled
        {
            get { return WaitThread != null; }
            set
            {
                if (value)
                    Start();
                else
                    Stop();
            }
        }

        public void Start()
        {
            if (WaitThread != null)
                return;

            lock (_lock)
            {
                WaitThread = new Thread(WaitLoop) { IsBackground = true };
                WaitThread.Start();
            }
        }

        public void Stop()
        {
            lock (_lock)
            {
                WaitThread?.Abort();
                WaitThread = null;
            }
        }

        private static void WaitDelta(DateTime start, int millis)
        {
            for (;;)
            {
                var delta = (int)(DateTime.Now - start).TotalMilliseconds;
                if (delta >= millis)
                    return;

                Thread.Sleep(1);
            }
        }

        private void WaitLoop()
        {
            try
            {
                var start = DateTime.Now;
                var wait = _millis - _millis / 20;
                if (wait == 0 || wait == _millis)
                    wait = 1;

                for (;;)
                {
                    Thread.Sleep(wait);
                    WaitDelta(start, _millis);
                    _callback();

                    do
                    {
                        start = start.AddMilliseconds(_millis);
                    } while ((DateTime.Now - start).TotalMilliseconds > _millis);
                }
            }
            catch (ThreadAbortException)
            {
                //
            }
        }
    }
}
