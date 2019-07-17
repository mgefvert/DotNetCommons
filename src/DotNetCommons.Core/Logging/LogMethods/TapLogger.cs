using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Core.Logging.LogMethods
{
    public class TapLoggerArgs : EventArgs
    {
        public List<LogEntry> Entries { get; }

        public TapLoggerArgs(List<LogEntry> entries)
        {
            Entries = entries;
        }
    }

    public class TapLogger : ILogMethod
    {
        public delegate void TapLoggerDelegate(object sender, TapLoggerArgs args);
        public event TapLoggerDelegate DataAvailable;

        public IReadOnlyList<LogEntry> Handle(IReadOnlyList<LogEntry> entries, bool flush)
        {
            if (DataAvailable != null && entries.Any())
                ThreadPool.QueueUserWorkItem(CallTap, entries.ToList());

            return entries;
        }

        private void CallTap(object state)
        {
            if (DataAvailable == null)
                return;

            try
            {
                DataAvailable(this, new TapLoggerArgs((List<LogEntry>)state));
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("TapLogger: " + e.Message);
            }
        }
    }
}
