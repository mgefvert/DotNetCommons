using System;
using System.IO;
using System.Linq;
using System.Text;
using DotNetCommons.Logging;
using DotNetCommons.Logging.LogMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Test.Logging.LogMethods
{
    [TestClass]
    public class FileLoggerTest
    {
        private readonly string _path = Path.GetTempPath();
        private readonly DateTime _dt = new DateTime(2016, 9, 26);

        [TestMethod]
        public void TestDailyNaming()
        {
            var provider = new FileLogger.LogFileDaily();

            var files = new[]
            {
                "logfile-2016-09-26.log",
                "logfile-2016-09-26.log.gz",
                "logfile-2016-09-25.log",
                "logfile-2016-09-25.log.gz",
                "logfile-2016-09-24.log",
                "logfile-2016-09-24.log.gz",
            };
            var testFiles = provider.GetAllowedFiles("logfile", ".log", 2, _dt).ToArray();
            CollectionAssert.AreEqual(files, testFiles);

            Assert.AreEqual("logfile-2016-09-26.log", provider.GetCurrentFileName("logfile", ".log", _dt));
            Assert.AreEqual("logfile-????-??-??.log", provider.GetFileSpec("logfile", ".log"));
        }

        [TestMethod]
        public void TestMonthlyNaming()
        {
            var provider = new FileLogger.LogFileMonthly();

            var files = new[]
            {
                "logfile-2016-09.log",
                "logfile-2016-09.log.gz",
                "logfile-2016-08.log",
                "logfile-2016-08.log.gz",
                "logfile-2016-07.log",
                "logfile-2016-07.log.gz",
            };
            var testFiles = provider.GetAllowedFiles("logfile", ".log", 2, _dt).ToArray();
            CollectionAssert.AreEqual(files, testFiles);

            Assert.AreEqual("logfile-2016-09.log", provider.GetCurrentFileName("logfile", ".log", _dt));
            Assert.AreEqual("logfile-????-??.log", provider.GetFileSpec("logfile", ".log"));
        }

        [TestMethod]
        public void TestCompressFile()
        {
            Assert.Inconclusive("Not implemented");
        }

        [TestMethod]
        public void TestOpenCurrent()
        {
            var log = new FileLogger(LogRotation.Daily, _path, "DotNetCommons.LogTest", "log", 3, true);
            var file = new FileInfo(Path.Combine(_path, $"DotNetCommons.LogTest-{DateTime.Today:yyyy-MM-dd}.log"));

            try
            {
                using (var stream = log.OpenCurrent())
                {
                    stream.Write(Encoding.Default.GetBytes("Hello"), 0, 5);
                }

                file.Refresh();
                Assert.IsTrue(file.Exists);
            }
            finally
            {
                file.Delete();
            }
        }

        [TestMethod]
        public void TestRotate()
        {
            Assert.Inconclusive("Not implemented");
        }
    }
}
