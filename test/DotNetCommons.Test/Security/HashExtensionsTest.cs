using System;
using System.Security.Cryptography;
using System.Text;
using DotNetCommons.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Test.Security
{
    [TestClass]
    public class HashExtensionsTest
    {
        [TestMethod]
        public void TestComputeString()
        {
            Assert.AreEqual("5f4dcc3b5aa765d61d8327deb882cf99", MD5.Create().ComputeString(Encoding.ASCII.GetBytes("password")));
        }
    }
}
