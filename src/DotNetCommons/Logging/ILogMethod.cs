using System;
using System.Collections.Generic;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Logging
{
    public interface ILogMethod
    {
        IReadOnlyList<LogEntry> Handle(IReadOnlyList<LogEntry> entries, bool flush);
    }
}
