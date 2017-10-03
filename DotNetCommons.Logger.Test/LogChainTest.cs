using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Logger.Test
{
    [TestClass]
    public class LogChainTest
    {
        [TestMethod]
        public void Test()
        {
            var log = new LogSystem();
            var logger = log.CreateLogger("test", true);

            Assert.AreEqual("test", logger.Channel);
            Assert.IsTrue(logger.LogChains.Count >= 1);
            Assert.IsTrue(logger.LogChains.First().Count >= 1);

            logger.Normal("Hello, world!");
        }
    }
}
