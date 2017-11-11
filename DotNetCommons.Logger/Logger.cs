using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DotNetCommons.Logger
{
    public static class Logger
    {
        public static LogSeverity? Severity = null;
        public static LogConfiguration Configuration => LogSystem.Configuration;
        public static LogChannel LogChannel => LogSystem.DefaultLogger;
        public static List<LogChain> LogChains => LogChannel.LogChains;
            
        [Obsolete]
        public static void Warn(string text, params object[] parameters) => LogChannel.Write(LogSeverity.Warning, text, parameters);
        [Obsolete]
        public static void Err(string text, params object[] parameters) => LogChannel.Write(LogSeverity.Error, text, parameters);
        [Obsolete]
        public static void Err(Exception ex) => Error(ex);

        public static void Trace(string text, params object[] parameters) => LogChannel.Write(LogSeverity.Trace, text, parameters);
        public static void Debug(string text, params object[] parameters) => LogChannel.Write(LogSeverity.Debug, text, parameters);
        public static void Log(string text, params object[] parameters) => LogChannel.Write(LogSeverity.Normal, text, parameters);
        public static void Normal(string text, params object[] parameters) => LogChannel.Write(LogSeverity.Normal, text, parameters);
        public static void Api(string text, params object[] parameters) => LogChannel.Write(LogSeverity.Api, text, parameters);
        public static void Notice(string text, params object[] parameters) => LogChannel.Write(LogSeverity.Notice, text, parameters);
        public static void Warning(string text, params object[] parameters) => LogChannel.Write(LogSeverity.Warning, text, parameters);
        public static void Error(string text, params object[] parameters) => LogChannel.Write(LogSeverity.Error, text, parameters);
        public static void Critical(string text, params object[] parameters) => LogChannel.Write(LogSeverity.Critical, text, parameters);
        public static void Fatal(string text, params object[] parameters) => LogChannel.Write(LogSeverity.Fatal, text, parameters);

        public static void Enter(string message, object[] parameters = null, object options = null) => LogChannel.Enter(message, parameters, options);
        public static void Enter(LogSeverity severity, string message, object[] parameters = null, object options = null) => LogChannel.Enter(severity, message, parameters, options);
        public static void Leave() => LogChannel.Leave();
        public static void Leave(string message, object[] parameters = null, object options = null) => LogChannel.Leave(message, parameters, options);
        public static void Leave(LogSeverity severity, string message, object[] parameters = null, object options = null) => LogChannel.Leave(severity, message, parameters, options);

        public static string Channel
        {
            get => LogChannel.Channel;
            set => LogChannel.Channel = value;
        }

        public static void Write(LogSeverity severity, string text, object[] parameters = null, object options = null)
            => LogChannel.Write(severity, text, parameters, options);

        public static void Error(Exception ex)
        {
            Error(ex.GetType().Name + ": " + ex.Message);
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
