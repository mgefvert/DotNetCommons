using System;
using DotNetCommons.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Test.Logging
{
    [TestClass]
    public class LoggerTest
    {
        [TestMethod]
        public void Test()
        {
            Logger.Normal("Hello, world!");
        }
    }
}
