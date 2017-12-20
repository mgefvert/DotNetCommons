using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DotNetCommons.Logging
{
    public static class Logger
    {
        public static LogConfiguration Configuration => LogSystem.Configuration;
        public static LogChannel LogChannel => LogSystem.DefaultLogger;
        public static List<LogChain> LogChains => LogChannel.LogChains;
            
        [Obsolete]
        public static void Warn(string text) => LogChannel.Write(LogSeverity.Warning, text);
        [Obsolete]
        public static void Err(string text) => LogChannel.Write(LogSeverity.Error, text);
        [Obsolete]
        public static void Err(Exception ex) => Error(ex);

        public static void Trace(string text, Dictionary<string, string> extraValues = null) => LogChannel.Write(LogSeverity.Trace, text, extraValues);
        public static void Debug(string text, Dictionary<string, string> extraValues = null) => LogChannel.Write(LogSeverity.Debug, text, extraValues);
        public static void Log(string text, Dictionary<string, string> extraValues = null) => LogChannel.Write(LogSeverity.Normal, text, extraValues);
        public static void Normal(string text, Dictionary<string, string> extraValues = null) => LogChannel.Write(LogSeverity.Normal, text, extraValues);
        public static void Api(string text, Dictionary<string, string> extraValues = null) => LogChannel.Write(LogSeverity.Api, text, extraValues);
        public static void Notice(string text, Dictionary<string, string> extraValues = null) => LogChannel.Write(LogSeverity.Notice, text, extraValues);
        public static void Warning(string text, Dictionary<string, string> extraValues = null) => LogChannel.Write(LogSeverity.Warning, text, extraValues);
        public static void Error(string text, Dictionary<string, string> extraValues = null) => LogChannel.Write(LogSeverity.Error, text, extraValues);
        public static void Critical(string text, Dictionary<string, string> extraValues = null) => LogChannel.Write(LogSeverity.Critical, text, extraValues);
        public static void Fatal(string text, Dictionary<string, string> extraValues = null) => LogChannel.Write(LogSeverity.Fatal, text, extraValues);

        public static void Enter(string message, Dictionary<string, string> extraValues = null) => LogChannel.Enter(message, extraValues);
        public static void Enter(LogSeverity severity, string message, Dictionary<string, string> extraValues = null) => LogChannel.Enter(severity, message, extraValues);
        public static void Leave() => LogChannel.Leave();
        public static void Leave(string message, Dictionary<string, string> extraValues = null) => LogChannel.Leave(message, extraValues);
        public static void Leave(LogSeverity severity, string message, Dictionary<string, string> extraValues = null) => LogChannel.Leave(severity, message, extraValues);

        public static string Channel
        {
            get => LogChannel.Channel;
            set => LogChannel.Channel = value;
        }

        public static void Write(LogSeverity severity, string text, Dictionary<string, string> extraValues = null)
        {
            LogChannel.Write(severity, text, extraValues);
        }

        public static bool IsLoggingFor(LogSeverity severity)
        {
            return severity >= LogChannel.ActualSeverity;
        }

        public static void Error(Exception ex)
        {
            Error(ex.GetDetailedInformation());
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
                Error(ex);
                return false;
            }
        }

        public static bool Catch(string enterMessage, Action action, string leaveMessage, LogSeverity severity = LogSeverity.Normal)
        {
            Enter(severity, enterMessage);
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
            finally
            {
                Leave(severity, leaveMessage);
            }
        }

        public static void Flush()
        {
            LogChannel.Flush();
        }
    }
}
