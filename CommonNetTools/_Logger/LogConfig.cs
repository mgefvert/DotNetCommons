using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace CommonNetTools
{
    public enum LogFileNaming
    {
        Daily,
        Monthly
    }

    public class LogConfig
    {
        public bool Colorize { get; set; }
        public bool CompressOnRotate { get; set; }
        public string Directory { get; set; }
        public bool EchoToConsole { get; set; }
        public LogFileNaming FileNaming { get; set; }
        public int MaxRotations { get; set; }
        public string Name { get; set; }
        public LogSeverity Severity { get; set; }
        public bool UseErrorLog { get; set; }

        public LogConfig()
        {
            Colorize = true;
            CompressOnRotate = true;
            Directory = ".";
            EchoToConsole = true;
            FileNaming = LogFileNaming.Monthly;
            MaxRotations = 7;
            Severity = LogSeverity.Normal;

            Name = Assembly.GetEntryAssembly() != null
                ? Assembly.GetEntryAssembly().GetName().Name
                : Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().ProcessName);
        }

        public void LoadFromAppSettings()
        {
            Directory = Environment.ExpandEnvironmentVariables(ConfigurationManager.AppSettings["LogPath"]);
            if (string.IsNullOrWhiteSpace(Directory))
                Directory = System.IO.Directory.GetCurrentDirectory();

            if (Bool(ConfigurationManager.AppSettings["LogDebug"]))
                Severity = LogSeverity.Debug;

            UseErrorLog = Bool(ConfigurationManager.AppSettings["LogErrors"]);
        }

        private static bool Bool(string value)
        {
            bool result;
            return bool.TryParse(value, out result) && result;
        }
    }
}
