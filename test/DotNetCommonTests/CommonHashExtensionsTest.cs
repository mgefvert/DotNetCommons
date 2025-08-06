using System.Security.Cryptography;
using System.Text;
using DotNetCommons;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommonTests;

[TestClass]
public class CommonHashExtensionsTest
{
    [TestMethod]
    public void TestComputeString()
    {
        Assert.AreEqual("5f4dcc3b5aa765d61d8327deb882cf99", MD5.Create().ComputeString(Encoding.ASCII.GetBytes("password")));
    }
}