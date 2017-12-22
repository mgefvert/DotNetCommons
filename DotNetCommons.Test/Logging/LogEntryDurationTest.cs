using System;
using System.Linq;
using System.Threading;
using DotNetCommons.Logging;
using DotNetCommons.Test.Logging.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Test.Logging
{
    [TestClass]
    public class LogEntryDurationTest
    {
        [TestMethod]
        public void Test()
        {
            var mock = new MockLogger();
            var logger = LogSystem.CreateLogger(null, LogChannelChainMode.Clear);
            var chain = new LogChain("mock");
            chain.Push(mock);
            logger.Chains.Add(chain);

            using (new LogEntryDuration(logger))
            {
                Thread.Sleep(100);
            }

            Assert.AreEqual(1, mock.Entries.Count);
            Assert.IsNotNull(mock.Entries.Single().ExtraValues);

            var ts = TimeSpan.FromMilliseconds(long.Parse(mock.Entries.Single().ExtraValues["duration"]));
            Assert.IsTrue(ts.TotalMilliseconds >= 100);
        }
    }
}
