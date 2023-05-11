using System.Text;
using DotNetCommons.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Test.IO;

[TestClass]
public class CompressionTest
{
    [TestMethod]
    public void Test()
    {
        var data = "";
        for (int i = 0; i < 100; i++)
            data += "The quick brown fox jumped over the lazy dog. ";

        var bytes = Compression.PackString(data);
        Assert.AreNotEqual(bytes, Encoding.UTF8.GetBytes(data));
        Assert.IsTrue(bytes.Length < data.Length);

        var newData = Compression.UnpackString(bytes);
        Assert.AreEqual(data, newData);
    }
}