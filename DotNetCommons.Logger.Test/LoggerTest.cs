using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Logger.Test
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
