using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Logger.Test
{
    [TestClass]
    public class LogPackerTest
    {
        [TestMethod]
        public void Test()
        {
            var entries = new List<LogEntry>
            {
                new LogEntry { Channel = "test", Message = "Hello, world!" },
                new LogEntry { Channel = "test", Message = "Hello, world!", Options = new LogEntryOptions { User = "local\\user" }},
                new LogEntryDuration { Channel = "test", Message = "Hello, world!", Options = new LogEntryOptions { Duration = TimeSpan.FromMinutes(1.5) }}
            };

            var buffer = LogPacker.Pack(entries);
            Assert.IsTrue(buffer.Length > 0);

            var result = LogPacker.Unpack(buffer);
            CollectionAssert.AreEquivalent(entries.Select(x => x.ToString()).ToList(), result.Select(x => x.ToString()).ToList());
        }
    }
}
