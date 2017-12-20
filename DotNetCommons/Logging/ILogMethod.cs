using System;
using System.Collections.Generic;

namespace DotNetCommons.Logging
{
    public interface ILogMethod
    {
        List<LogEntry> Handle(List<LogEntry> entries, bool flush);
    }
}
