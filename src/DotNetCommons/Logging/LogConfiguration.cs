using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Logging;

public enum LogRotation
{
    Daily,
    Monthly
}

public class LogConfiguration
{
    public bool Colorize { get; set; }
    public bool CompressOnRotate { get; set; }
    public string Directory { get; set; }
    public bool EchoToConsole { get; set; }
    public int MaxRotations { get; set; }
    public string Name { get; set; }
    public LogRotation Rotation { get; set; }
    public LogSeverity Severity { get; set; }
    public bool StackTrace { get; set; }
    public bool UseErrorLog { get; set; }
    public bool Initialized { get; set; }

    public LogConfiguration()
    {
        Colorize = true;
        CompressOnRotate = true;
        Directory = ".";
        EchoToConsole = true;
        Rotation = LogRotation.Monthly;
        MaxRotations = 7;
        Severity = LogSeverity.Normal;
        StackTrace = false;

        Name = Assembly.GetEntryAssembly() != null
            ? Assembly.GetEntryAssembly()?.GetName().Name
            : Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().ProcessName);
    }

    public void LoadFromAppSettings()
    {
        Directory = Environment.ExpandEnvironmentVariables(ConfigurationManager.AppSettings["LogPath"] ?? "");
        if (string.IsNullOrWhiteSpace(Directory))
            Directory = System.IO.Directory.GetCurrentDirectory();

        if (Bool(ConfigurationManager.AppSettings["LogDebug"]))
            Severity = LogSeverity.Debug;

        UseErrorLog = Bool(ConfigurationManager.AppSettings["LogErrors"]);
    }

    private static bool Bool(string value)
    {
        return bool.TryParse(value, out var result) && result;
    }
}