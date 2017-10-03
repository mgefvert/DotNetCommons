using System;

namespace DotNetCommons.Logger
{
    public enum LogSeverity
    {
        Trace,         // Low-level diagnostic debugging
        Debug,         // Debug information for examining what a program does
        Normal,        // Default message
        Api,           // API entry/exit
        Notice,        // Normal but significant conditions (program start, job start etc)
        Warning,       // Warning conditions
        Error,         // Error conditions
        Critical,      // Action must be taken immediately
        Fatal          // System is unusable
    }
}
