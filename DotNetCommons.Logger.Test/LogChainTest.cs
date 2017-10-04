using System;
using System.Linq;
using DotNetCommons.Logger.LogMethods;
using DotNetCommons.Logger.Test.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Logger.Test
{
    [TestClass]
    public class LogChainTest
    {
        [TestMethod]
        public void TestDefaultChains()
        {
            var logger = LogSystem.CreateLogger("test", true);

            Assert.AreEqual("test", logger.Channel);
            Assert.IsTrue(logger.LogChains.Count >= 1);
            Assert.IsTrue(logger.LogChains.First().Count >= 1);

            logger.Normal("Hello, world!");
        }

        [TestMethod]
        public void TestCustomChains()
        {
            var logger = LogSystem.CreateLogger("test", false);
            var chain = new LogChain("test");
            var mock = new MockLogger();
            chain.Push(mock);
            chain.Push(new LimitSeverityLogger(LogSeverity.Error));
            logger.LogChains.Add(chain);

            logger.Normal("This is normal");
            logger.Error("This is an error");

            Assert.AreEqual(1, mock.Entries.Count);
            Assert.AreEqual("This is an error", mock.Entries.First().Message);
        }
    }
}
