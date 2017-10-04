using System;
using System.Collections.Generic;
using DotNetCommons.Logger.LogMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Logger.Test.LogMethods
{
    [TestClass]
    public class LimitSeverityLoggerTest
    {
        [TestMethod]
        public void Test()
        {
            var logger = new LimitSeverityLogger(new [] { LogSeverity.Critical, LogSeverity.Api });

            var list = new List<LogEntry>
            {
                new LogEntry { Severity = LogSeverity.Trace },
                new LogEntry { Severity = LogSeverity.Debug },
                new LogEntry { Severity = LogSeverity.Normal },
                new LogEntry { Severity = LogSeverity.Api },
                new LogEntry { Severity = LogSeverity.Notice },
                new LogEntry { Severity = LogSeverity.Warning },
                new LogEntry { Severity = LogSeverity.Error },
                new LogEntry { Severity = LogSeverity.Critical },
                new LogEntry { Severity = LogSeverity.Fatal }
            };

            Assert.AreEqual(2, logger.Handle(list, false).Count);
            Assert.AreEqual(2, logger.Handle(list, true).Count);
        }
    }
}
