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
            var logsystem = new LogSystem();
            var mock = new MockLogger();
            var logger = logsystem.CreateLogger(null, false);
            logger.LogChains.Add(new LogChain(mock));

            using (new LogEntryDuration(logger))
            {
                Thread.Sleep(100);
            }

            Assert.AreEqual(1, mock.Entries.Count);
            Assert.IsNotNull(mock.Entries.Single().Options);
            Assert.IsNotNull(mock.Entries.Single().Options.Duration);
            Assert.IsTrue((mock.Entries.Single().Options.Duration?.TotalMilliseconds ?? 0) >= 100);
        }
    }
}
