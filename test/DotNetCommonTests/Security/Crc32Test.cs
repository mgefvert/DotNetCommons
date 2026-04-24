using DotNetCommons.Security;

namespace DotNetCommonTests.Security;

[TestClass]
public class Crc32Test
{
    [TestMethod]
    public void Test()
    {
        Assert.AreEqual(0x414F_A339u, Crc32.ComputeChecksum("The quick brown fox jumps over the lazy dog"));
    }
}