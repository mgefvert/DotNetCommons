using System;
using System.Collections.Generic;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Logging.LogMethods
{
    public class ConsoleLogger : ILogMethod
    {
        private static readonly object Lock = new object();
        private static readonly ConsoleColor DefaultColor = Console.ForegroundColor;

        public IReadOnlyList<LogEntry> Handle(IReadOnlyList<LogEntry> entries, bool flush)
        {
            lock (Lock)
            {
                foreach (var entry in entries)
                {
                    if (entry.Severity >= LogSeverity.Error)
                        Console.ForegroundColor = ConsoleColor.Red;
                    else if (entry.Severity >= LogSeverity.Warning)
                        Console.ForegroundColor = ConsoleColor.Yellow;
                    else if (entry.Severity >= LogSeverity.Notice)
                        Console.ForegroundColor = ConsoleColor.Green;

                    Console.WriteLine(entry.ToString(LogFormat.Short));
                    Console.ForegroundColor = DefaultColor;
                }
            }

            return entries;
        }
    }
}
