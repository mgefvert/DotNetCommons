using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using DotNetCommons.Logger.LogMethods;

namespace DotNetCommons.Logger
{
    public class LogSystem
    {
        public LogConfiguration Configuration { get; } = new LogConfiguration();
        public List<LogChain> LogChains { get; } = new List<LogChain>();
        public ConsoleColor DefaultColor { get; } = Console.ForegroundColor;
        public int MainThreadId { get; } = Thread.CurrentThread.ManagedThreadId;

        internal readonly string MachineName = Environment.MachineName;
        internal readonly string ProcessName = Process.GetCurrentProcess().ProcessName;
        private LogChannel _defaultLogger;
        private bool _initialized;

        public LogChannel DefaultLogger()
        {
            return _defaultLogger ?? (_defaultLogger = CreateLogger(null, true));
        }

        public LogChannel CreateLogger(string channel, bool defaultChains)
        {
            if (!_initialized)
            {
                Configuration.LoadFromAppSettings();

                var chain = new LogChain();
                if (Configuration.UseErrorLog)
                {
                    chain.Push(new FileLogger(Configuration.Rotation, Configuration.Directory,
                        Configuration.Name, ".err", Configuration.MaxRotations, Configuration.CompressOnRotate));
                    chain.Push(new LimitSeverityLogger(LogSeverity.Warning, LogSeverity.Error, LogSeverity.Critical, LogSeverity.Fatal));
                }

                chain.Push(new FileLogger(Configuration.Rotation, Configuration.Directory,
                    Configuration.Name, ".log", Configuration.MaxRotations, Configuration.CompressOnRotate));

                LogChains.Add(chain);

                _initialized = true;
            }

            return new LogChannel(this, channel, defaultChains);
        }
    }
}
