using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetCommons.Logger.LogMethods
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
        private static readonly object Lock = new object();

        public delegate void TapLoggerDelegate(object sender, TapLoggerArgs args);
        public event TapLoggerDelegate DataAvailable;

        public List<LogEntry> Handle(List<LogEntry> entries, bool flush)
        {
            lock (Lock)
            {
                if (entries.Any())
                    DataAvailable?.Invoke(this, new TapLoggerArgs(entries));

                return entries;
            }
        }
    }
}
