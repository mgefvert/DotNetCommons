using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using DotNetCommons.Core.Logging.LogMethods;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Core.Logging
{
    public static class LogSystem
    {
        public static LogConfiguration Configuration { get; } = new LogConfiguration();
        public static ConcurrentBag<LogChain> LogChains { get; } = new ConcurrentBag<LogChain>();
        public static ConsoleColor DefaultColor { get; } = Console.ForegroundColor;
        public static int MainThreadId { get; } = Thread.CurrentThread.ManagedThreadId;

        internal static readonly string MachineName = Environment.MachineName;
        internal static readonly string ProcessName = Process.GetCurrentProcess().ProcessName;
        private static bool _initialized;
        private static LogChannel _defaultLogger;

        public static LogChannel DefaultLogger => _defaultLogger ?? (_defaultLogger = CreateLogger("", LogChannelChainMode.UseDefault));

        public static LogChannel CreateLogger(string channel, LogChannelChainMode mode)
        {
            if (!_initialized)
                InitializeLogSystem();

            return new LogChannel(channel, mode);
        }

        private static void InitializeLogSystem()
        {
            Configuration.LoadFromAppSettings();

            var chain = new LogChain("default");
            if (Configuration.UseErrorLog)
            {
                chain.Push(new FileLogger(Configuration.Rotation, Configuration.Directory,
                    Configuration.Name, ".err", Configuration.MaxRotations, Configuration.CompressOnRotate));
                chain.Push(new LimitSeverityLogger(LogSeverity.Warning));
            }

            chain.Push(new FileLogger(Configuration.Rotation, Configuration.Directory,
                Configuration.Name, ".log", Configuration.MaxRotations, Configuration.CompressOnRotate));

            LogChains.Add(chain);

            _initialized = true;
        }
    }
}
