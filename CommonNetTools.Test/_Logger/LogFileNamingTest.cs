using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonNetTools.Test
{
    [TestClass]
    public class LogFileNamingTest
    {
        private LogFileDaily _daily;
        private LogFileMonthly _monthly;
        private readonly DateTime _dt = new DateTime(2016, 9, 26);

        [TestInitialize]
        public void Setup()
        {
            _daily = new LogFileDaily();
            _monthly = new LogFileMonthly();
        }

        [TestMethod]
        public void TestGetAllowedFiles()
        {
            var dailies = new[]
            {
                "logfile-2016-09-26.log",
                "logfile-2016-09-26.log.gz",
                "logfile-2016-09-25.log",
                "logfile-2016-09-25.log.gz",
                "logfile-2016-09-24.log",
                "logfile-2016-09-24.log.gz",
            };
            var testDailies = _daily.GetAllowedFiles("logfile", ".log", 2, _dt).ToArray();
            CollectionAssert.AreEqual(dailies, testDailies);

            var monthlies = new[]
            {
                "logfile-2016-09.log",
                "logfile-2016-09.log.gz",
                "logfile-2016-08.log",
                "logfile-2016-08.log.gz",
                "logfile-2016-07.log",
                "logfile-2016-07.log.gz",
            };
            var testMonthlies = _monthly.GetAllowedFiles("logfile", ".log", 2, _dt).ToArray();
            CollectionAssert.AreEqual(monthlies, testMonthlies);
        }

        [TestMethod]
        public void TestGetCurrentFileName()
        {
            Assert.AreEqual("logfile-2016-09-26.log", _daily.GetCurrentFileName("logfile", ".log", _dt));
            Assert.AreEqual("logfile-2016-09.log", _monthly.GetCurrentFileName("logfile", ".log", _dt));
        }

        [TestMethod]
        public void GetFileSpec()
        {
            Assert.AreEqual("logfile-????-??-??.log", _daily.GetFileSpec("logfile", ".log"));
            Assert.AreEqual("logfile-????-??.log", _monthly.GetFileSpec("logfile", ".log"));
        }
    }
}
