using System;
using System.Collections.Generic;

namespace DotNetCommons.Logging
{
    public interface ILogMethod
    {
        IReadOnlyList<LogEntry> Handle(IReadOnlyList<LogEntry> entries, bool flush);
    }
}
