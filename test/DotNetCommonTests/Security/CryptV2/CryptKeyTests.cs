using DotNetCommons.Security.CryptV2;
using FluentAssertions;
using System.Security.Cryptography;

namespace DotNetCommonTests.Security.CryptV2;

[TestClass]
public class CryptKeyTests
{
    [TestMethod]
    public void CreateRandom_ShouldGenerateValidKey_ForAes128()
    {
        using var key = CryptKey.CreateRandom(CryptAlgorithm.Aes128);

        key.Should().NotBeNull();
        key.KeyBuffer.Should().HaveCount(16);
        key.Algorithm.Should().Be(CryptAlgorithm.Aes128);
    }

    [TestMethod]
    public void CreateRandom_ShouldGenerateValidKey_ForAes256()
    {
        using var key = CryptKey.CreateRandom(CryptAlgorithm.Aes256);

        key.Should().NotBeNull();
        key.KeyBuffer.Should().HaveCount(32);
        key.Algorithm.Should().Be(CryptAlgorithm.Aes256);
    }

    [TestMethod]
    public void CreateRandom_ShouldGenerateValidKey_ForAes512()
    {
        using var key = CryptKey.CreateRandom(CryptAlgorithm.Aes512);

        key.Should().NotBeNull();
        key.KeyBuffer.Should().HaveCount(64);
        key.Algorithm.Should().Be(CryptAlgorithm.Aes512);
    }

    [TestMethod]
    public void CreateRandom_ShouldGenerateDifferentKeys()
    {
        using var key1 = CryptKey.CreateRandom(CryptAlgorithm.Aes256);
        using var key2 = CryptKey.CreateRandom(CryptAlgorithm.Aes256);

        key1.KeyBuffer.Should().NotBeEquivalentTo(key2.KeyBuffer);
    }

    [TestMethod]
    public void FromPbkdf2_ShouldGenerateValidKey()
    {
        using var key = CryptKey.FromPbkdf2("testpassword", 10000, 16, CryptAlgorithm.Aes256);

        key.Should().NotBeNull();
        key.KeyBuffer.Should().HaveCount(32);
        key.Algorithm.Should().Be(CryptAlgorithm.Aes256);
    }

    [TestMethod]
    public void FromXorPad_WithByteArray_ShouldGenerateValidKey()
    {
        using var key = CryptKey.FromXorPad([1, 2, 3, 4, 5], CryptAlgorithm.Aes256);

        key.Should().NotBeNull();
        key.KeyBuffer.Should().HaveCount(32);
        key.Algorithm.Should().Be(CryptAlgorithm.Aes256);
    }

    [TestMethod]
    public void FromXorPad_WithString_ShouldGenerateValidKey()
    {
        using var key = CryptKey.FromXorPad("testpassword", CryptAlgorithm.Aes256);

        key.Should().NotBeNull();
        key.KeyBuffer.Should().HaveCount(32);
        key.Algorithm.Should().Be(CryptAlgorithm.Aes256);
    }

    [TestMethod]
    public void FromXorPad_ShouldProduceDeterministicResult()
    {
        using var key1 = CryptKey.FromXorPad("testpassword", CryptAlgorithm.Aes256);
        using var key2 = CryptKey.FromXorPad("testpassword", CryptAlgorithm.Aes256);

        key1.KeyBuffer.Should().BeEquivalentTo(key2.KeyBuffer);
    }

    [TestMethod]
    public void FromSha_WithByteArray_ShouldGenerateValidKey_ForAes128()
    {
        using var key = CryptKey.FromSha([1, 2, 3, 4, 5], CryptAlgorithm.Aes128);

        key.Should().NotBeNull();
        key.KeyBuffer.Should().HaveCount(16);
        key.Algorithm.Should().Be(CryptAlgorithm.Aes128);
    }

    [TestMethod]
    public void FromSha_WithByteArray_ShouldGenerateValidKey_ForAes256()
    {
        using var key = CryptKey.FromSha([1, 2, 3, 4, 5], CryptAlgorithm.Aes256);

        key.Should().NotBeNull();
        key.KeyBuffer.Should().HaveCount(32);
        key.Algorithm.Should().Be(CryptAlgorithm.Aes256);
    }

    [TestMethod]
    public void FromSha_WithString_ShouldGenerateValidKey()
    {
        using var key = CryptKey.FromSha("testpassword", CryptAlgorithm.Aes256);

        key.Should().NotBeNull();
        key.KeyBuffer.Should().HaveCount(32);
        key.Algorithm.Should().Be(CryptAlgorithm.Aes256);
    }

    [TestMethod]
    public void FromSha_ShouldProduceDeterministicResult()
    {
        using var key1 = CryptKey.FromSha("testpassword", CryptAlgorithm.Aes256);
        using var key2 = CryptKey.FromSha("testpassword", CryptAlgorithm.Aes256);

        key1.KeyBuffer.Should().BeEquivalentTo(key2.KeyBuffer);
    }

    [TestMethod]
    public void Constructor_ShouldThrowException_ForInvalidKeyLength()
    {
        var act = () => new CryptKey(new byte[10], CryptAlgorithm.Aes256);

        act.Should().Throw<CryptographicException>()
            .WithMessage("*Wrong key length*");
    }

    [TestMethod]
    public void Constructor_ShouldAcceptValidKey_ForAes128()
    {
        using var key = new CryptKey(new byte[16], CryptAlgorithm.Aes128);

        key.Should().NotBeNull();
        key.KeyBuffer.Should().HaveCount(16);
        key.Algorithm.Should().Be(CryptAlgorithm.Aes128);
    }

    [TestMethod]
    public void Constructor_ShouldAcceptValidKey_ForAes256()
    {
        using var key = new CryptKey(new byte[32], CryptAlgorithm.Aes256);

        key.Should().NotBeNull();
        key.KeyBuffer.Should().HaveCount(32);
        key.Algorithm.Should().Be(CryptAlgorithm.Aes256);
    }

    [TestMethod]
    public void Constructor_ShouldAcceptValidKey_ForAes512()
    {
        using var key = new CryptKey(new byte[64], CryptAlgorithm.Aes512);

        key.Should().NotBeNull();
        key.KeyBuffer.Should().HaveCount(64);
        key.Algorithm.Should().Be(CryptAlgorithm.Aes512);
    }

    [TestMethod]
    public void Dispose_ShouldZeroOutKeyBuffer()
    {
        var keyBytes = new byte[32];
        Array.Fill(keyBytes, (byte)255);

        var key = new CryptKey(keyBytes, CryptAlgorithm.Aes256);
        key.Dispose();

        key.KeyBuffer.Should().AllBeEquivalentTo((byte)0);
    }

    [TestMethod]
    public void GenerateMessageKey_WithByteArray_ShouldProduceDifferentKeys()
    {
        using var masterKey = CryptKey.CreateRandom(CryptAlgorithm.Aes256);
        var message1 = new byte[] { 1, 2, 3 };
        var message2 = new byte[] { 4, 5, 6 };

        using var messageKey1 = masterKey.GenerateMessageKey(message1);
        using var messageKey2 = masterKey.GenerateMessageKey(message2);

        messageKey1.KeyBuffer.Should().NotBeEquivalentTo(messageKey2.KeyBuffer);
    }

    [TestMethod]
    public void GenerateMessageKey_WithString_ShouldProduceDifferentKeys()
    {
        using var masterKey = CryptKey.CreateRandom(CryptAlgorithm.Aes256);
        using var messageKey1 = masterKey.GenerateMessageKey("message1");
        using var messageKey2 = masterKey.GenerateMessageKey("message2");

        messageKey1.KeyBuffer.Should().NotBeEquivalentTo(messageKey2.KeyBuffer);
    }

    [TestMethod]
    public void GenerateMessageKey_ShouldProduceDeterministicResult()
    {
        using var masterKey = CryptKey.CreateRandom(CryptAlgorithm.Aes256);
        using var messageKey1 = masterKey.GenerateMessageKey("message");
        using var messageKey2 = masterKey.GenerateMessageKey("message");

        messageKey1.KeyBuffer.Should().BeEquivalentTo(messageKey2.KeyBuffer);
    }

    [TestMethod]
    public void AllAlgorithms_ShouldBeSupported()
    {
        var algorithms = new[] 
        { 
            CryptAlgorithm.Aes128, 
            CryptAlgorithm.Aes256, 
            CryptAlgorithm.Aes512 
        };

        foreach (var algorithm in algorithms)
        {
            var act = () => CryptKey.CreateRandom(algorithm);
            act.Should().NotThrow();

            using var key = act();
        }
    }
}