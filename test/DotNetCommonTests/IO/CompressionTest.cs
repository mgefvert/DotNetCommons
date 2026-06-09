using System.Text;
using DotNetCommons;
using DotNetCommons.IO;

namespace DotNetCommonTests.IO;

[TestClass]
public class CompressionTest
{
    [TestMethod]
    public void Test_Deflate()
    {
        var data = "The quick brown fox jumped over the lazy dog. ".Repeat(100);

        var bytes = Compression.CompressString(data, method: CompressionMethod.Deflate);
        CollectionAssert.AreNotEqual(bytes, Encoding.UTF8.GetBytes(data));
        Assert.IsLessThan(data.Length, bytes.Length);
        Assert.IsGreaterThan(0, bytes.Length);

        var newData = Compression.DecompressString(bytes, CompressionMethod.Deflate);
        Assert.AreEqual(data, newData);
    }

    [TestMethod]
    public void Test_GZip()
    {
        var data = "The quick brown fox jumped over the lazy dog. ".Repeat(100);

        var deflateBytes = Compression.CompressString(data, method: CompressionMethod.Deflate);

        var bytes = Compression.CompressString(data, method: CompressionMethod.GZip);
        CollectionAssert.AreNotEqual(bytes, Encoding.UTF8.GetBytes(data));
        CollectionAssert.AreNotEqual(bytes, deflateBytes);
        Assert.IsLessThan(data.Length, bytes.Length);
        Assert.IsGreaterThan(0, bytes.Length);

        var newData = Compression.DecompressString(bytes, CompressionMethod.GZip);
        Assert.AreEqual(data, newData);
    }

    [TestMethod]
    public void Test_Brotli()
    {
        var data = "The quick brown fox jumped over the lazy dog. ".Repeat(100);

        var deflateBytes = Compression.CompressString(data, method: CompressionMethod.Deflate);

        var bytes = Compression.CompressString(data, method: CompressionMethod.Brotli);
        CollectionAssert.AreNotEqual(bytes, Encoding.UTF8.GetBytes(data));
        CollectionAssert.AreNotEqual(bytes, deflateBytes);
        Assert.IsLessThan(data.Length, bytes.Length);
        Assert.IsGreaterThan(0, bytes.Length);

        var newData = Compression.DecompressString(bytes, CompressionMethod.Brotli);
        Assert.AreEqual(data, newData);
    }
}