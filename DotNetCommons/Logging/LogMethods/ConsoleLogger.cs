using System;
using System.Collections.Generic;

namespace DotNetCommons.Logging.LogMethods
{
    public class ConsoleLogger : ILogMethod
    {
        private static readonly object Lock = new object();

        public IReadOnlyList<LogEntry> Handle(IReadOnlyList<LogEntry> entries, bool flush)
        {
            var color = Console.ForegroundColor;
            foreach (var entry in entries)
            {
                if (entry.Severity >= LogSeverity.Error)
                    Console.ForegroundColor = ConsoleColor.Red;
                else if (entry.Severity >= LogSeverity.Warning)
                    Console.ForegroundColor = ConsoleColor.Yellow;
                else if (entry.Severity >= LogSeverity.Notice)
                    Console.ForegroundColor = ConsoleColor.Green;

                Console.WriteLine(entry.ToString(LogFormat.Short));
                Console.ForegroundColor = color;
            }

            return entries;
        }
    }
}
