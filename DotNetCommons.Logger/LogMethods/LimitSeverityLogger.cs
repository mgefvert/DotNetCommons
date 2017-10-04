using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetCommons.Logger.LogMethods
{
    public class LimitSeverityLogger : ILogMethod
    {
        private readonly LogSeverity[] _allowed;

        public LimitSeverityLogger(LogSeverity minSeverity)
        {
            _allowed = Enum.GetValues(typeof(LogSeverity))
                .Cast<LogSeverity>()
                .Where(item => item >= minSeverity)
                .ToArray();
        }

        public LimitSeverityLogger(LogSeverity[] allowed)
        {
            _allowed = allowed;
        }

        public List<LogEntry> Handle(List<LogEntry> entries, bool flush)
        {
            return entries.Where(x => _allowed.Contains(x.Severity)).ToList();
        }
    }
}
