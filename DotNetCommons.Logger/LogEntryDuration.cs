using System;

namespace DotNetCommons.Logger
{
    public class LogEntryDuration : LogEntry, IDisposable
    {
        private readonly DateTime _start = DateTime.Now;
        private readonly LogChannel _logger;

        public LogEntryDuration(LogChannel logger)
        {
            _logger = logger;
        }

        public void Dispose()
        {
            (Options ?? (Options = new LogEntryOptions())).Duration = DateTime.Now - _start;
            _logger?.Write(this);
        }
    }
}
