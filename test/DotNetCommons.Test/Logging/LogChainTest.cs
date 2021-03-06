﻿using System;
using System.Linq;
using DotNetCommons.Logging;
using DotNetCommons.Logging.LogMethods;
using DotNetCommons.Test.Logging.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Test.Logging
{
    [TestClass]
    public class LogChainTest
    {
        [TestMethod]
        public void TestDefaultChains()
        {
            var logger = LogSystem.CreateLogger("test", LogChannelChainMode.CopyDefault);

            Assert.AreEqual("test", logger.Channel);
            Assert.IsTrue(logger.Chains.Count >= 1);
            Assert.IsTrue(logger.Chains.First().Count >= 1);

            logger.Normal("Hello, world!");
        }

        [TestMethod]
        public void TestCustomChains()
        {
            var logger = LogSystem.CreateLogger("test", LogChannelChainMode.Clear);
            var chain = new LogChain("test");
            var mock = new MockLogger();
            chain.Push(mock);
            chain.Push(new LimitSeverityLogger(LogSeverity.Error));
            logger.Chains.Add(chain);

            logger.Normal("This is normal");
            logger.Error("This is an error");

            Assert.AreEqual(1, mock.Entries.Count);
            Assert.AreEqual("This is an error", mock.Entries.First().Message);
        }
    }
}
