using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetCommons.Logger
{
    public class LogChain : Stack<ILogMethod>
    {
        public LogChain()
        {
        }

        public LogChain(params ILogMethod[] methods)
        {
            foreach (var link in methods)
                Push(link);
        }

        public LogChain(LogChain chain)
        {
            foreach(var link in chain.Reverse())
                Push(link);
        }

        public void Process(List<LogEntry> entries, bool flush)
        {
            foreach (var link in this)
                entries = link.Handle(entries, flush);
        }
    }
}
