using System;
using System.Collections.Generic;
using DotNetCommons.Logging;

namespace DotNetCommons.Test.Logging.Mock
{
    public class MockLogger : ILogMethod
    {
        public List<LogEntry> Entries { get; } = new List<LogEntry>();

        public IReadOnlyList<LogEntry> Handle(IReadOnlyList<LogEntry> entries, bool flush)
        {
            Entries.AddRange(entries);
            return entries;
        }
    }
}
