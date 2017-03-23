using System;
using System.Linq;
using DotNetCommons.IO.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.IO.Test.Parsers
{
    [TestClass]
    public class ApacheLogParserTest
    {
        private readonly string[] _testData = 
        {
            "::1 - - [13/Mar/2017:06:53:46 +0100] \"OPTIONS * HTTP/1.0\" 200 125 \"-\" \"Apache/2.4.7 (Ubuntu) PHP/5.5.9-1ubuntu4.5 (internal dummy connection)\"",
            "54.246.180.28 - - [13/Mar/2017:06:54:28 +0100] \"GET / HTTP/1.1\" 301 464 \"-\" \"Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/38.0.2125.104 Safari/537.36\"",
            "54.246.180.28 - - [13/Mar/2017:06:54:28 +0100] \"GET /login.php?referer=kund.bebetteronline.com%2F HTTP/1.1\" 200 2753 \"-\" \"Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/38.0.2125.104 Safari/537.36\"",
            "80.252.216.250 - - [13/Mar/2017:10:01:29 +0100] \"POST /ajax.php?type=editReport HTTP/1.1\" 200 416 \"http://kund.bebetteronline.com/editReport.php?siteId=1306&customerId=3689\" \"Mozilla/5.0 (Macintosh; Intel Mac OS X 10_11_6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36\""
        };

        [TestMethod]
        public void Test()
        {
            var logs = _testData.Select(ApacheLogParser.Parse).ToList();

            Assert.AreEqual(4, logs.Count);
            Assert.AreEqual("::1", logs[0].IP.ToString());
            Assert.AreEqual("OPTIONS", logs[0].Method);
            Assert.AreEqual("HTTP/1.0", logs[0].Protocol);
            Assert.AreEqual("-", logs[0].Referer);
            Assert.AreEqual(200, logs[0].ResponseCode);
            Assert.AreEqual(125, logs[0].ResponseLength);
            Assert.AreEqual("2017-03-13 05:53:46", logs[0].Time.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"));
            Assert.AreEqual("*", logs[0].Url);
            Assert.AreEqual("Apache/2.4.7 (Ubuntu) PHP/5.5.9-1ubuntu4.5 (internal dummy connection)", logs[0].UserAgent);
            Assert.AreEqual("-", logs[0].UserName);
        }

        [TestMethod]
        public void SpeedTest()
        {
            var t0 = DateTime.Now;

            var c = 0;
            for (int i = 0; i < 5000; i++)
            {
                foreach (var line in _testData)
                {
                    ApacheLogParser.Parse(line);
                    c++;
                }
            }

            Console.WriteLine($"{c} lines analyzed in {(DateTime.Now - t0).TotalMilliseconds.ToString("N2")} ms");
        }
    }
}
