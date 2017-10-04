using System;
using System.Collections.Specialized;
using System.Linq;

namespace DotNetCommons.Logger
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
        public ListDictionary Parameters { get; } = new ListDictionary();

        public void Add(string key, object value)
        {
            Parameters[key] = value;
        }

        public T Get<T>(string parameter)
        {
            return Parameters.Contains(parameter) 
                ? (T) Parameters[parameter] 
                : default(T);
        }

        public string GetParametersAsText(string separator, params string[] excludeKeys)
        {
            var data = Parameters.Keys
                .Cast<string>()
                .Where(x => !excludeKeys.Contains(x))
                .Select(x => x + "=" + Parameters[x])
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
                    (Severity <= LogSeverity.Debug ? "- " : null) +
                    (ThreadId != null ? $"[{ThreadId}] " : null) +
                    Message + "  " + GetParametersAsText(", ")).Trim();
            }

            return (format == LogFormat.Long ? _renderDate : null) + _render;
        }
    }
}
