using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonNetTools.Test
{
    [TestClass]
    public class HashExtensionsTest
    {
        public void TestComputeString()
        {
            Assert.AreEqual("5f4dcc3b5aa765d61d8327deb882cf99", MD5.Create().ComputeString(Encoding.ASCII.GetBytes("password")));
        }
    }
}
