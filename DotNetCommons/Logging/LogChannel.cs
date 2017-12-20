using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using DotNetCommons.Logging.LogMethods;

namespace DotNetCommons.Logging
{
    public delegate void LogEvent(object sender, LogEntry entry);

    public enum LogChannelChainMode
    {
        Clear,
        CopyDefault,
        UseDefault
    }

    public class LogChannel : IDisposable
    {
        internal LogSeverity ActualSeverity => Severity ?? LogSystem.Configuration.Severity;

        public LogSeverity? Severity = null;
        public string Channel { get; set; }

        public void Trace(string text, Dictionary<string, string> extraValues = null) => Write(LogSeverity.Trace, text, extraValues);
        public void Debug(string text, Dictionary<string, string> extraValues = null) => Write(LogSeverity.Debug, text, extraValues);
        public void Log(string text, Dictionary<string, string> extraValues = null) => Write(LogSeverity.Normal, text, extraValues);
        public void Normal(string text, Dictionary<string, string> extraValues = null) => Write(LogSeverity.Normal, text, extraValues);
        public void Api(string text, Dictionary<string, string> extraValues = null) => Write(LogSeverity.Api, text, extraValues);
        public void Notice(string text, Dictionary<string, string> extraValues = null) => Write(LogSeverity.Notice, text, extraValues);
        public void Warning(string text, Dictionary<string, string> extraValues = null) => Write(LogSeverity.Warning, text, extraValues);
        public void Error(string text, Dictionary<string, string> extraValues = null) => Write(LogSeverity.Error, text, extraValues);
        public void Critical(string text, Dictionary<string, string> extraValues = null) => Write(LogSeverity.Critical, text, extraValues);
        public void Fatal(string text, Dictionary<string, string> extraValues = null) => Write(LogSeverity.Fatal, text, extraValues);

        public event LogEvent LogEvent;

        public List<LogChain> LogChains { get; }
        protected ConsoleLogger ConsoleLogger = new ConsoleLogger();

        private int _level;

        internal LogChannel(string channel, LogChannelChainMode chainMode)
        {
            Channel = channel;

            switch (chainMode)
            {
                case LogChannelChainMode.Clear:
                    LogChains = new List<LogChain>();
                    break;

                case LogChannelChainMode.CopyDefault:
                    LogChains = new List<LogChain>();
                    foreach (var chain in LogSystem.LogChains)
                        LogChains.Add(new LogChain(chain));
                    break;

                case LogChannelChainMode.UseDefault:
                    LogChains = LogSystem.LogChains;
                    break;
            }
        }

        public void Error(Exception ex)
        {
            Error(ex.GetType().Name + ": " + ex.Message);
        }

        public bool IsLoggingFor(LogSeverity severity)
        {
            return severity >= ActualSeverity;
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

                if (LogEvent != null)
                    foreach (var entry in entries)
                        LogEvent?.Invoke(this, entry);

                foreach (var chain in LogChains)
                    chain.Process(entries.ToList(), false);

                if (LogSystem.Configuration.EchoToConsole)
                    ConsoleLogger.Handle(entries, false);
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

        public void Write(LogSeverity severity, string text, Dictionary<string, string> extraValues = null)
        {
            if (severity < ActualSeverity)
                return;

            try
            {
                var entry = new LogEntry();
                PopulateEntry(entry, _level, severity, text, extraValues);
                Write(entry);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.GetType().Name + ": " + ex.Message);
            }
        }

        public LogEntryDuration Time(LogSeverity severity, string text, Dictionary<string, string> extraValues = null)
        {
            if (severity < ActualSeverity)
                return new LogEntryDuration(null);

            var entry = new LogEntryDuration(this);
            PopulateEntry(entry, _level, severity, text, extraValues);
            return entry;
        }

        protected void PopulateEntry(LogEntry entry, int level, LogSeverity severity, string text, Dictionary<string, string> extraValues)
        {
            var threadId = Thread.CurrentThread.ManagedThreadId;
            entry.Level = level;
            entry.Time = DateTime.Now;
            entry.Channel = Channel;
            entry.Message = text;
            entry.MachineName = LogSystem.MachineName;
            entry.ProcessName = LogSystem.ProcessName;
            entry.Severity = severity;
            entry.ThreadId = threadId != LogSystem.MainThreadId ? (int?) threadId : null;

            if (extraValues != null)
                foreach (var item in extraValues)
                    entry.ExtraValues[item.Key] = item.Value;
        }

        public void Enter(string message, Dictionary<string, string> extraValues = null)
        {
            Enter(LogSeverity.Normal, message, extraValues);
        }

        public void Enter(LogSeverity severity, string message, Dictionary<string, string> extraValues = null)
        {
            Write(severity, message, extraValues);
            _level++;
        }

        public void Leave()
        {
            if (_level > 0)
                _level--;
        }

        public void Leave(string message, Dictionary<string, string> extraValues = null)
        {
            Leave(LogSeverity.Normal, message, extraValues);
        }

        public void Leave(LogSeverity severity, string message, Dictionary<string, string> extraValues = null)
        {
            if (_level > 0)
                _level--;
            Write(severity, message, extraValues);
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

        public void Flush()
        {
            var empty = new List<LogEntry>();
            foreach (var chain in LogChains)
                chain.Process(empty, true);
        }
    }
}
