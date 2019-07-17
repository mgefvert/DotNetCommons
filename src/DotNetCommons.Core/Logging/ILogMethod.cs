using System;
using System.Collections.Generic;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Core.Logging
{
    public interface ILogMethod
    {
        IReadOnlyList<LogEntry> Handle(IReadOnlyList<LogEntry> entries, bool flush);
    }
}
