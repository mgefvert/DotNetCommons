using System;
using System.Collections.Generic;
using DotNetCommons.Logger.LogMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Logger.Test.LogMethods
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
