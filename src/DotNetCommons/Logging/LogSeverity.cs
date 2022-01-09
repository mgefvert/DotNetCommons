using System;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Logging;

public enum LogSeverity
{
    Trace,         // Low-level diagnostic debugging
    Debug,         // Debug information for examining what a program does
    Normal,        // Default message
    Info,          // Informational message, same as Normal but triggers possible extra forwarding
    Api,           // API entry/exit
    Notice,        // Normal but significant conditions (program start, job start etc)
    Warning,       // Warning conditions
    Error,         // Error conditions
    Critical,      // Action must be taken immediately
    Fatal          // System is unusable
}