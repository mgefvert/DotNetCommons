using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.IO.Test
{
    [TestClass]
    public class ProtectedStorageTest
    {
        [TestMethod]
        public void TestProtect()
        {
            var storage = new ProtectedStorage("this is my private key");

            var plain = "Hello, world!";
            var data = storage.Protect(plain);
            Assert.AreNotEqual(data, plain);
            data = storage.Unprotect(data);
            Assert.AreEqual(plain, data);
        }
    }
}
