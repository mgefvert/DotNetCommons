using System;

namespace DotNetCommons.Logger
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
            Parameters["duration"] = DateTime.Now - _start;
            _logger?.Write(this);
        }
    }
}
