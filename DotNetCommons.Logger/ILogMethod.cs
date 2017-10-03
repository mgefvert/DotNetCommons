using System;
using System.Collections.Generic;

namespace DotNetCommons.Logger
{
    public interface ILogMethod
    {
        List<LogEntry> Handle(List<LogEntry> entries, bool flush);
    }
}
