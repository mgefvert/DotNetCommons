using System;
using System.Collections.Generic;
using System.Linq;
using DotNetCommons.Collections;
using DotNetCommons.Text;

namespace DotNetCommons.Logging
{
    public enum LogFormat
    {
        Short,
        Long
    }

    [Serializable]
    public class LogEntry
    {
        private string _renderDate;
        private string _render;

        /// <summary>Millisecond timestamp</summary>
        public DateTime Time { get; set; } = DateTime.Now;

        /// <summary>Channel identifying job, service or similar</summary>
        public string Channel { get; set; }

        /// <summary>Actual message</summary>
        public string Message { get; set; }

        /// <summary>Log severity</summary>
        public LogSeverity Severity { get; set; } = LogSeverity.Normal;

        /// <summary>Machine name on which the event occurred</summary>
        public string MachineName { get; set; }

        /// <summary>Process name for the event</summary>
        public string ProcessName { get; set; }

        /// <summary>Managed thread ID</summary>
        public int? ThreadId { get; set; }

        /// <summary>Optional other parameters</summary>
        public Dictionary<string, string> ExtraValues { get; } = new Dictionary<string, string>();

        /// <summary>Indentation level</summary>
        public int Level { get; set; }

        public LogEntry()
        {
        }

        public LogEntry(LogSeverity severity, string message)
        {
            Severity = severity;
            Message = message;
        }

        public void Add(string key, string value)
        {
            ExtraValues[key] = value;
        }

        public string Get(string parameter)
        {
            return ExtraValues.GetOrDefault(parameter);
        }

        public string GetParametersAsText(string separator, params string[] excludeKeys)
        {
            var data = ExtraValues.Keys
                .Where(x => !excludeKeys.Contains(x))
                .Select(x => x + "=" + ExtraValues[x])
                .ToList();

            return data.Any() ? string.Join(separator, data).Left(255) : null;
        }

        public override string ToString()
        {
            return ToString(LogFormat.Long);
        }

        public string ToString(LogFormat format)
        {
            if (_render == null)
            {
                _renderDate = Time.ToString("yyyy-MM-dd") + " ";
                _render =
                    (Time.ToString("HH:mm:ss.fff") + " " +
                    LogChannel.SeverityToText(Severity) + " " +
                    new string(' ', Level * 4) +
                    (Severity <= LogSeverity.Debug ? "- " : null) +
                    (ThreadId != null ? $"[{ThreadId}] " : null) +
                    (!string.IsNullOrEmpty(Channel) ? "@" + Channel + " " : "") +
                    Message + "  " + GetParametersAsText(", ")).Trim();
            }

            return (format == LogFormat.Long ? _renderDate : null) + _render;
        }
    }
}
