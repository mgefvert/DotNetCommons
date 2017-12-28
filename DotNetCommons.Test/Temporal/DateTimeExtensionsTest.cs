using System;
using DotNetCommons.Temporal;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Test.Temporal
{
    [TestClass]
    public class DateTimeExtensionsTest
    {
        [TestMethod]
        public void TestDateTime()
        {
            var dt = DateTimeExtensions.FromUnixSeconds(1469284816);
            Assert.AreEqual("2016-07-23 14:40:16", dt.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"));
        }

        [TestMethod]
        public void TestDateTimeMillis()
        {
            var dt = DateTimeExtensions.FromUnixMilliseconds(1469284816000L);
            Assert.AreEqual("2016-07-23 14:40:16", dt.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"));
        }

        [TestMethod]
        public void TestDateTimeOffset()
        {
            var dt = DateTimeExtensions.FromUnixSecondsOffset(1469284816);
            Assert.AreEqual("2016-07-23 14:40:16", dt.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"));
        }

        [TestMethod]
        public void TestDateTimeOffsetMillis()
        {
            var dt = DateTimeExtensions.FromUnixMillisecondsOffset(1469284816000);
            Assert.AreEqual("2016-07-23 14:40:16", dt.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"));
        }

        [TestMethod]
        public void TestTimestamp()
        {
            var dt = new DateTime(2016, 7, 23, 14, 40, 16, DateTimeKind.Utc);
            Assert.AreEqual(1469284816, dt.ToUnixSeconds());
        }

        [TestMethod]
        public void TestTimestampMillis()
        {
            var dt = new DateTime(2016, 7, 23, 14, 40, 16, DateTimeKind.Utc);
            Assert.AreEqual(1469284816000L, dt.ToUnixMilliseconds());
        }

        [TestMethod]
        public void TestTimestampOffset()
        {
            var dt = new DateTimeOffset(2016, 7, 23, 15, 40, 16, TimeSpan.FromHours(1));
            Assert.AreEqual(1469284816, dt.ToUnixSeconds());
        }

        [TestMethod]
        public void TestTimestampOffsetMillis()
        {
            var dt = new DateTimeOffset(2016, 7, 23, 15, 40, 16, TimeSpan.FromHours(1));
            Assert.AreEqual(1469284816000L, dt.ToUnixMilliseconds());
        }
    }
}
