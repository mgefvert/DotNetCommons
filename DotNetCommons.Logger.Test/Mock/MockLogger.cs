using System;
using System.Collections.Generic;

namespace DotNetCommons.Logger.Test.Mock
{
    public class MockLogger : ILogMethod
    {
        public List<LogEntry> Entries { get; } = new List<LogEntry>();

        public List<LogEntry> Handle(List<LogEntry> entries, bool flush)
        {
            Entries.AddRange(entries);
            return entries;
        }
    }
}
