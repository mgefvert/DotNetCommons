using System;
using DotNetCommons.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Test.Logging
{
    [TestClass]
    public class LogEntryTest
    {
        [TestMethod]
        public void Test()
        {
            var entry = new LogEntry
            {
                Message = "Hello, world!",
                ThreadId = 56,
                Severity = LogSeverity.Critical,
                Time = new DateTime(2017, 12, 31, 23, 59, 01, 456)
            };

            Assert.AreEqual("2017-12-31 23:59:01.456 CRIT  [56] Hello, world!", entry.ToString());
            Assert.AreEqual("2017-12-31 23:59:01.456 CRIT  [56] Hello, world!", entry.ToString(LogFormat.Long));
            Assert.AreEqual("23:59:01.456 CRIT  [56] Hello, world!", entry.ToString(LogFormat.Short));
        }
    }
}
