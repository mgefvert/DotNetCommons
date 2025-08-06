using DotNetCommons.Security;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommonTests.Security;

[TestClass]
public class CryptKeyTests
{
    private static readonly byte[] EmptyKey = new byte[32];
    
    [TestMethod]
    public void Test()
    {
        var key = new CryptKey("Hello world");
        key.KeyBuffer.Length.Should().Be(32);
        key.KeyBuffer.Should().NotBeEquivalentTo(EmptyKey);
        
        var newKey = key.GenerateMessageKey("123456");
        newKey.KeyBuffer.Should().NotBeEquivalentTo(key.KeyBuffer);
        
        key.Dispose();
        key.KeyBuffer.Should().BeEquivalentTo(EmptyKey);
    }
}