using System;
using System.Linq;
using System.Threading;
using DotNetCommons.Logger.Test.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Logger.Test
{
    [TestClass]
    public class LogEntryDurationTest
    {
        [TestMethod]
        public void Test()
        {
            var mock = new MockLogger();
            var logger = LogSystem.CreateLogger(null, false);
            var chain = new LogChain("mock");
            chain.Push(mock);
            logger.LogChains.Add(chain);

            using (new LogEntryDuration(logger))
            {
                Thread.Sleep(100);
            }

            Assert.AreEqual(1, mock.Entries.Count);
            Assert.IsNotNull(mock.Entries.Single().Parameters);

            var ts = (TimeSpan) mock.Entries.Single().Parameters["duration"];
            Assert.IsTrue(ts.TotalMilliseconds >= 100);
        }
    }
}
