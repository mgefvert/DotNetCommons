using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonNetTools.Test
{
    [TestClass]
    public class LogFilesTest
    {
        private LogConfig _config;
        private LogFiles _logFiles;

        [TestInitialize]
        public void Setup()
        {
            _config = new LogConfig();
            _logFiles = new LogFiles(_config);
        }

        [TestMethod]
        public void TestCompressFile()
        {
        }

        [TestMethod]
        public void TestOpenCurrent()
        {
            var file = _logFiles.GetCurrentLogfile(".log");
            try
            {
                using (var stream = _logFiles.OpenCurrent(".log"))
                {
                    stream.Write(Encoding.Default.GetBytes("Hello"), 0, 5);
                }

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
        }
    }
}
