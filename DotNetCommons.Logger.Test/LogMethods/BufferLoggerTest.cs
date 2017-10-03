using System;
using System.Collections.Generic;
using DotNetCommons.Logger.LogMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Logger.Test.LogMethods
{
    [TestClass]
    public class BufferLoggerTest
    {
        [TestMethod]
        public void Test()
        {
            var logger = new BufferLogger(3, null);
            var list = new List<LogEntry> { new LogEntry() };

            Assert.AreEqual(0, logger.Handle(list, false).Count);
            Assert.AreEqual(0, logger.Handle(list, false).Count);
            Assert.AreEqual(3, logger.Handle(list, false).Count);
            Assert.AreEqual(0, logger.Handle(list, false).Count);
            Assert.AreEqual(0, logger.Handle(list, false).Count);
            Assert.AreEqual(3, logger.Handle(list, false).Count);
            Assert.AreEqual(1, logger.Handle(list, true).Count);
        }
    }
}
