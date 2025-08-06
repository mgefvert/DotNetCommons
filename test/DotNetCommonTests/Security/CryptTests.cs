using DotNetCommons.Security;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommonTests.Security;

[TestClass]
public class CryptTests
{
    [TestMethod]
    public void CompressedEncryptionTest()
    {
        var key = new CryptKey("abc123");
        
        var plainText = new string('A', 128);
        var encrypted = Crypt.Encrypt(key, plainText, true);

        encrypted.Should().NotBe(plainText);
        encrypted.Length.Should().BeLessThan(plainText.Length);

        var decrypted = Crypt.Decrypt(key, encrypted, true);
        decrypted.Should().Be(plainText);
    }
    
    [TestMethod]
    public void UncompressedEncryptionTest()
    {
        var key = new CryptKey("abc123");
        
        var plainText = new string('A', 128);
        Console.WriteLine(plainText);
        var encrypted = Crypt.Encrypt(key, plainText, false);
        Console.WriteLine(encrypted);

        encrypted.Should().NotBe(plainText);
        encrypted.Length.Should().BeGreaterThan(plainText.Length);
        
        var decrypted = Crypt.Decrypt(key, encrypted, false);
        decrypted.Should().Be(plainText);
    }
}