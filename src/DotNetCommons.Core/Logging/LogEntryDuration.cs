using System;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Core.Logging
{
    [Serializable]
    public class LogEntryDuration : LogEntry, IDisposable
    {
        [NonSerialized]
        private readonly DateTime _start = DateTime.Now;

        [NonSerialized]
        private readonly LogChannel _logger;

        public LogEntryDuration()
        {
        }

        public LogEntryDuration(LogChannel logger)
        {
            _logger = logger;
        }

        public void Dispose()
        {
            ExtraValues["duration"] = ((long)(DateTime.Now - _start).TotalMilliseconds).ToString();
            _logger?.Write(this);
        }
    }
}
