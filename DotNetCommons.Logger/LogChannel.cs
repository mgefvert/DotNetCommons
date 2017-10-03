﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace DotNetCommons.Logger
{
    public class LogChannel : IDisposable
    {
        private LogSeverity ActualSeverity => Severity ?? LogSystem.Configuration.Severity;

        public LogSeverity? Severity = null;
        public string Channel { get; }
        public LogSystem LogSystem { get; }

        public void Trace(string text, params object[] parameters) => Write(LogSeverity.Trace, text, parameters);
        public void Debug(string text, params object[] parameters) => Write(LogSeverity.Debug, text, parameters);
        public void Normal(string text, params object[] parameters) => Write(LogSeverity.Normal, text, parameters);
        public void Api(string text, params object[] parameters) => Write(LogSeverity.Api, text, parameters);
        public void Notice(string text, params object[] parameters) => Write(LogSeverity.Notice, text, parameters);
        public void Warning(string text, params object[] parameters) => Write(LogSeverity.Warning, text, parameters);
        public void Error(string text, params object[] parameters) => Write(LogSeverity.Error, text, parameters);
        public void Critical(string text, params object[] parameters) => Write(LogSeverity.Critical, text, parameters);
        public void Fatal(string text, params object[] parameters) => Write(LogSeverity.Fatal, text, parameters);

        public List<LogChain> LogChains { get; } = new List<LogChain>();

        internal LogChannel(LogSystem logSystem, string channel, bool copyChains)
        {
            LogSystem = logSystem;
            Channel = channel;

            if (copyChains)
                foreach (var chain in logSystem.LogChains)
                    LogChains.Add(new LogChain(chain));
        }

        public void Error(Exception ex)
        {
            Error(ex.GetType().Name + ": " + ex.Message, LogSeverity.Error);
        }

        public static string SeverityToText(LogSeverity severity)
        {
            switch (severity)
            {
                case LogSeverity.Notice:
                    return "***  ";
                case LogSeverity.Warning:
                    return "WARN ";
                case LogSeverity.Error:
                    return "ERR  ";
                case LogSeverity.Critical:
                    return "CRIT ";
                case LogSeverity.Fatal:
                    return "FATAL";
                default:
                    return "     ";
            }
        }

        public void Write(List<LogEntry> entries)
        {
            if (entries == null || entries.Count == 0)
                return;

            try
            {
                var severity = ActualSeverity;
                entries = entries.Where(x => x != null && x.Severity >= severity).ToList();
                if (!entries.Any())
                    return;

                foreach (var chain in LogChains)
                    chain.Process(entries.ToList(), false);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.GetType().Name + ": " + ex.Message);
            }
        }

        public void Write(LogEntry entry)
        {
            Write(new List<LogEntry> { entry });
        }

        public void Write(LogSeverity severity, string text, object[] parameters = null, LogEntryOptions options = null)
        {
            if (severity < ActualSeverity)
                return;

            try
            {
                Write(new LogEntry
                {
                    Time = DateTime.UtcNow,
                    Channel = Channel,
                    Message = parameters != null ? string.Format(text, parameters) : text,
                    MachineName = LogSystem.MachineName,
                    ProcessName = LogSystem.ProcessName,
                    Severity = severity,
                    ThreadId = Thread.CurrentThread.ManagedThreadId,
                    Options = options
                });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.GetType().Name + ": " + ex.Message);
            }
        }

        public LogEntryDuration Time(LogSeverity severity, string text, object[] parameters = null, LogEntryOptions options = null)
        {
            if (severity < ActualSeverity)
                return new LogEntryDuration(null);

            var thread = Thread.CurrentThread.ManagedThreadId;

            return new LogEntryDuration(this)
            {
                Time = DateTime.UtcNow,
                Channel = Channel,
                Message = parameters != null ? string.Format(text, parameters) : text,
                MachineName = LogSystem.MachineName,
                ProcessName = LogSystem.ProcessName,
                Severity = severity,
                ThreadId = thread != LogSystem.MainThreadId ? (int?)thread : null,
                Options = options
            };
        }

        /// <summary>
        /// Wrap a particular action in a try/catch block, automatically logging the exception
        /// as an Error condition to the log output and returning true/false for success.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        /// <returns>True if the action succeeded, false if an error was caught.</returns>
        [DebuggerStepThrough]
        public bool Catch(Action action)
        {
            try
            {
                action();
                return true;
            }
            catch (Exception ex)
            {
                Error(ex);
                return false;
            }
        }

        public void Dispose()
        {
            Flush();
            LogChains.Clear();
        }

        private void Flush()
        {
            var empty = new List<LogEntry>();
            foreach (var chain in LogChains)
                chain.Process(empty, true);
        }
    }
}