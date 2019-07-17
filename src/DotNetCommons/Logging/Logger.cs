using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Logging
{
    public static class Logger
    {
        public static LogConfiguration Configuration => LogSystem.Configuration;
        public static LogChannel LogChannel => LogSystem.DefaultLogger;
            
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
        public static void Info(string text, Dictionary<string, string> extraValues = null) => LogChannel.Write(LogSeverity.Info, text, extraValues);
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

        public static string Expose(object obj)
        {
            var seen = new HashSet<object>();
            var result = new StringBuilder();
            Expose(null, obj, result, 0, seen);
            return result.ToString();
        }

        public static string ExposeWeb(object obj)
        {
            var result = Expose(obj);
            return 
                "<pre>" + 
                result
                    .Replace("<", "&lt;")
                    .Replace(">", "&gt;")
                    .Replace("\r\n", "<br>") + 
                "</pre>";
        }

        private static void Expose(string name, object obj, StringBuilder sb, int indent, HashSet<object> seen)
        {
            var indentstr = new string(' ', indent * 4);
            if (!string.IsNullOrEmpty(name))
                sb.Append(indentstr + name + ": ");

            try
            {
                if (obj == null)
                {
                    sb.AppendLine("(null)");
                    return;
                }

                var type = obj.GetType();
                var objstr = obj.ToString();
                if (objstr == type.FullName)
                    objstr = "hash:" + obj.GetHashCode();

                if (type.IsValueType)
                {
                    sb.AppendLine($"{objstr} <{type.Name}>");
                    return;
                }

                if (obj is string s)
                {
                    sb.AppendLine($"\"{s}\"");
                    return;
                }

                if (seen.Contains(obj))
                {
                    sb.AppendLine($"*SEEN* {objstr} <{type.Name}>");
                    return;
                }

                seen.Add(obj);

                if (obj is byte[] buffer)
                {
                    sb.AppendLine($"<{type.Name}> [");
                    for (var i = 0; i < buffer.Length; i++)
                    {
                        if (i % 32 == 0)
                        {
                            if (i > 0)
                                sb.AppendLine();
                            sb.Append(indentstr);
                            sb.Append("    ");
                        }

                        sb.Append(buffer[i].ToString("X2") + ' ');
                    }
                    sb.AppendLine();
                    sb.AppendLine(indentstr + $"    ({buffer.Length} bytes)");
                    sb.AppendLine(indentstr + "]");
                    return;
                }

                if (obj is IDictionary dictionary)
                {
                    sb.AppendLine($"<{type.Name}> {{");
                    var c = 0;
                    foreach (DictionaryEntry item in dictionary)
                    {
                        Expose(item.Key.ToString(), item.Value, sb, indent + 1, seen);
                        c++;
                    }
                    sb.AppendLine(indentstr + (c == 0 ? "    (empty)" : $"    ({c} items)"));
                    sb.AppendLine(indentstr + "}");
                    return;
                }

                if (obj is IEnumerable enumerable)
                {
                    sb.AppendLine($"<{type.Name}> [");
                    var i = 0;
                    foreach (var item in enumerable)
                        Expose((i++).ToString(), item, sb, indent + 1, seen);
                    if (i == 0)
                        sb.AppendLine(indentstr + "    (empty)");
                    sb.AppendLine(indentstr + "]");
                    return;
                }

                if (type.FullName?.StartsWith("System.") ?? false)
                {
                    sb.AppendLine($"{objstr} <{type.Name}>");
                    return;
                }

                sb.AppendLine($"<{type.Name}> {{");
                foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).OrderBy(x => x.Name))
                {
                    try
                    {
                        Expose(prop.Name, prop.GetValue(obj), sb, indent + 1, seen);
                    }
                    catch (Exception e)
                    {
                        sb.AppendLine($"{indentstr}    {prop.Name}: <{e.GetType().Name}: {e.Message}>");
                    }
                }

                sb.AppendLine(indentstr + "}");
            }
            catch (Exception e)
            {
                sb.AppendLine($"{indentstr} <{e.GetType().Name}: {e.Message}>");
            }
        }
    }
}
