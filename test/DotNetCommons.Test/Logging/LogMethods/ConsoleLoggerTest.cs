using System;
using System.Collections.Generic;
using DotNetCommons.Logging;
using DotNetCommons.Logging.LogMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Test.Logging.LogMethods
{
    [TestClass]
    public class ConsoleLoggerTest
    {
        [TestMethod]
        public void Test()
        {
            var logger = new ConsoleLogger();
            var list = new List<LogEntry> { new LogEntry { Message = "Hello, world!" } };

            Assert.AreEqual(1, logger.Handle(list, false).Count);
            Assert.AreEqual(1, logger.Handle(list, true).Count);
        }
    }
}
