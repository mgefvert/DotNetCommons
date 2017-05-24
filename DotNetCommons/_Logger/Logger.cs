using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace DotNetCommons
{
    /// <summary>
    /// Log severity.
    /// </summary>
    public enum LogSeverity
    {
        Debug,
        Normal,
        Notice,
        Warning,
        Error
    }

    public delegate void LogEvent(LogSeverity severity, string text);

    /// <summary>
    /// Static class that handles all the logging for a process.
    /// </summary>
    public static class Logger
    {
        public static LogConfig Configuration { get; } = new LogConfig();
        public static event LogEvent LogEvent;

        private static readonly LogFiles LogFiles;
        private static readonly ConsoleColor DefaultColor;
        private static readonly object LockFile = new object();
        private static readonly object LockConsole = new object();
        private static Stream _errorFile;
        private static Stream _logFile;
        private static DateTime _logDate;
        private static readonly int MainThreadId;
        private static readonly Dictionary<int, int> ThreadLevels = new Dictionary<int, int>();

        static Logger()
        {
            DefaultColor = Console.ForegroundColor;
            MainThreadId = Thread.CurrentThread.ManagedThreadId;

            Configuration.LoadFromAppSettings();
            LogFiles = new LogFiles(Configuration);
        }

        private static string SeverityToText(LogSeverity severity)
        {
            switch (severity)
            {
                case LogSeverity.Notice:
                    return "*** ";
                case LogSeverity.Warning:
                    return "WARN";
                case LogSeverity.Error:
                    return "ERR ";
                default:
                    return "    ";
            }
        }

        /// <summary>
        /// Enter a function, causing messages after this (on the same thread) to be written indented.
        /// The function can be called as many times as needed, each time indenting one further step.
        /// </summary>
        /// <param name="message">Message to write.</param>
        /// <param name="severity">Severity to use for the message.</param>
        public static void Enter(string message, LogSeverity severity = LogSeverity.Normal)
        {
            if (severity < Configuration.Severity)
                return;

            Write(message, severity);
            ThreadLevels.Increase(Thread.CurrentThread.ManagedThreadId);
        }

        /// <summary>
        /// Leave a function, causing messages after this (on the same thread) to be unindented.
        /// The function can be called as many times as needed, each time indenting one further step.
        /// </summary>
        /// <param name="message">Message to write.</param>
        /// <param name="severity">Severity to use for the message.</param>
        public static void Leave(string message = null, LogSeverity severity = LogSeverity.Normal)
        {
            if (severity < Configuration.Severity)
                return;

            var threadId = Thread.CurrentThread.ManagedThreadId;
            ThreadLevels.Increase(threadId, -1);
            if (ThreadLevels[threadId] < 0)
                ThreadLevels[threadId] = 0;

            if (message != null)
                Write(message, severity);
        }

        /// <summary>
        /// Write new log data.
        /// </summary>
        /// <param name="severity">Severity for the log line.</param>
        /// <param name="text">Log text.</param>
        public static void Write(string text, LogSeverity severity)
        {
            if (severity < Configuration.Severity)
                return;

            if (MainThreadId != Thread.CurrentThread.ManagedThreadId)
                text = "[" + Thread.CurrentThread.ManagedThreadId + "] " + text;

            if (severity == LogSeverity.Debug)
                text = "- " + text;

            if (ThreadLevels.TryGetValue(Thread.CurrentThread.ManagedThreadId, out var level))
            {
                while (level-- > 0)
                    text = "|   " + text;
            }

            LogEvent?.Invoke(severity, text);

            text = SeverityToText(severity) + " " + text;

            lock (LockFile)
                if (_logFile == null || _logDate != DateTime.Today)
                {
                    _logFile?.Dispose();
                    _logFile = null;
                    _errorFile?.Dispose();
                    _errorFile = null;

                    LogFiles.Rotate(".log");
                    LogFiles.Rotate(".err");

                    _logFile = LogFiles.OpenCurrent(".log");
                    _logDate = DateTime.Today;

                    if (Configuration.UseErrorLog)
                        _errorFile = LogFiles.OpenCurrent(".err");
                }

            var now = DateTime.Now;
            var buffer = Encoding.Default.GetBytes(now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + text + "\r\n");

            lock (LockFile)
            {
                _logFile.Write(buffer, 0, buffer.Length);
                _logFile.Flush();

                if (severity >= LogSeverity.Warning && _errorFile != null)
                {
                    _errorFile.Write(buffer, 0, buffer.Length);
                    _errorFile.Flush();
                }
            }

            lock (LockConsole)
            {
                if (severity == LogSeverity.Error)
                    Console.ForegroundColor = ConsoleColor.Red;
                else if (severity == LogSeverity.Warning)
                    Console.ForegroundColor = ConsoleColor.Yellow;
                else if (severity == LogSeverity.Notice)
                    Console.ForegroundColor = ConsoleColor.Green;

                Console.WriteLine(now.ToString("HH:mm:ss.fff") + " " + text);
                Console.ForegroundColor = DefaultColor;
            }
        }

        /// <summary>
        /// Log an exception as an error log line.
        /// </summary>
        public static void Err(Exception ex)
        {
            Write(ex.GetType().Name + ": " + ex.Message, LogSeverity.Error);
        }

        /// <summary>
        /// Write a new error log line. Errors are red.
        /// </summary>
        public static void Err(string text)
        {
            Write(text, LogSeverity.Error);
        }

        /// <summary>
        /// Write a new warning log line. Warnings are yellow.
        /// </summary>
        public static void Warn(string text)
        {
            Write(text, LogSeverity.Warning);
        }

        /// <summary>
        /// Write a new notice log line. Notices are green.
        /// </summary>
        public static void Notice(string text)
        {
            Write(text, LogSeverity.Notice);
        }

        /// <summary>
        /// Write a new log line.
        /// </summary>
        public static void Log(string text)
        {
            Write(text, LogSeverity.Normal);
        }

        /// <summary>
        /// Write a new debug log line. Debug output is not visible unless enabled with DebugLogs = true.
        /// </summary>
        public static void Debug(string text)
        {
            if (Configuration.Severity <= LogSeverity.Debug)
                Write(text, LogSeverity.Debug);
        }

        /// <summary>
        /// Wrap a particular action in a try/catch block, automatically logging the exception
        /// as an Error condition to the log output and returning true/false for success.
        /// </summary>
        /// <param name="enterMessage">Log message upon entering the function.</param>
        /// <param name="action">Action to execute.</param>
        /// <param name="leaveMessage">Log message upon leaving the function</param>
        /// <param name="severity">Severity for the log messages.</param>
        /// <returns>True if the action succeeded, false if an error was caught.</returns>
        [DebuggerStepThrough]
        public static bool Catch(string enterMessage, Action action, string leaveMessage = null, LogSeverity severity = LogSeverity.Normal)
        {
            Enter(enterMessage, severity);
            try
            {
                action();
                return true;
            }
            catch (Exception ex)
            {
                Err(ex);
                return false;
            }
            finally
            {
                Leave(leaveMessage, severity);
            }
        }

        /// <summary>
        /// Wrap a particular action in a try/catch block, automatically logging the exception
        /// as an Error condition to the log output and returning true/false for success.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        /// <returns>True if the action succeeded, false if an error was caught.</returns>
        [DebuggerStepThrough]
        public static bool Catch(Action action)
        {
            try
            {
                action();
                return true;
            }
            catch (Exception ex)
            {
                Err(ex);
                return false;
            }
        }
    }
}
