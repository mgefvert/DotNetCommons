using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Cryptography;
using System.Text;

namespace DotNetCommons.Test;

[TestClass]
public class CommonHashExtensionsTest
{
    [TestMethod]
    public void TestComputeString()
    {
        Assert.AreEqual("5f4dcc3b5aa765d61d8327deb882cf99", MD5.Create().ComputeString(Encoding.ASCII.GetBytes("password")));
    }
}