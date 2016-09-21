using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonNetTools.Test
{
    [TestClass]
    public class UnixTimeTest
    {
        [TestMethod]
        public void TestDateTime(long timestamp)
        {
            var dt = UnixTime.DateTime(1469284816);
            Assert.AreEqual("2016-07-23 14:40:16", dt.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"));
        }

        [TestMethod]
        public void TestDateTimeMillis(long millisTimestamp)
        {
            var dt = UnixTime.DateTimeMillis(1469284816000L);
            Assert.AreEqual("2016-07-23 14:40:16", dt.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"));
        }

        [TestMethod]
        public void TestDateTimeOffset(long timestamp)
        {
            var dt = UnixTime.DateTimeOffset(1469284816);
            Assert.AreEqual("2016-07-23 14:40:16", dt.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"));
        }

        [TestMethod]
        public void TestDateTimeOffsetMillis(long timestamp)
        {
            var dt = UnixTime.DateTimeOffsetMillis(1469284816);
            Assert.AreEqual("2016-07-23 14:40:16", dt.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"));
        }

        [TestMethod]
        public void TestTimestamp()
        {
            var dt = new DateTime(2016, 7, 23, 14, 40, 16, DateTimeKind.Utc);
            Assert.AreEqual(1469284816, dt.Timestamp());
        }

        [TestMethod]
        public void TestTimestampMillis()
        {
            var dt = new DateTime(2016, 7, 23, 14, 40, 16, DateTimeKind.Utc);
            Assert.AreEqual(1469284816000L, dt.TimestampMillis());
        }

        [TestMethod]
        public void TestTimestampOffset()
        {
            var dt = new DateTimeOffset(2016, 7, 23, 15, 40, 16, TimeSpan.FromHours(1));
            Assert.AreEqual(1469284816, dt.Timestamp());
        }

        [TestMethod]
        public void TestTimestampOffsetMillis()
        {
            var dt = new DateTimeOffset(2016, 7, 23, 15, 40, 16, TimeSpan.FromHours(1));
            Assert.AreEqual(1469284816000L, dt.TimestampMillis());
        }
    }
}
