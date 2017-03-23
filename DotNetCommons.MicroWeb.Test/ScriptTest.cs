using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.MicroWeb.Test
{
    [TestClass]
    public class ScriptTest
    {
        [TestMethod]
        public void TestScript()
        {
            Assert.AreEqual(7, new Script().Run("4 + 3"));
        }

        [TestMethod]
        public void TestHasNamespaces()
        {
            new Script().Run("Thread.Sleep(1);");
        }

        [TestMethod]
        public void TestDynamic()
        {
            Assert.AreEqual(2, new Script().Run("var x = new { a=1, b=2 }; x.a * x.b"));
        }
    }
}
