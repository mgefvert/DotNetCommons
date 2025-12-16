using DotNetCommons.Security.CryptV2;
using FluentAssertions;

namespace DotNetCommonTests.Security.CryptV2;

[TestClass]
public class CryptTests
{
    [TestMethod]
    public void Encrypt_Decrypt_RoundTrip_WithoutAssociatedData_ShouldReturnOriginalData()
    {
        using var key = CryptKey.CreateRandom();
        var plaintext = "Hello, World!"u8.ToArray();

        var encrypted = Crypt.Encrypt(key, plaintext);
        encrypted.Length.Should().Be(12 + 16 + plaintext.Length); // Nonce + Tag + Ciphertext

        var decrypted = Crypt.Decrypt(key, encrypted);
        decrypted.Should().BeEquivalentTo(plaintext);
    }

    [TestMethod]
    public void Encrypt_Decrypt_RoundTrip_WithAssociatedData_ShouldReturnOriginalData()
    {
        using var key = CryptKey.CreateRandom();
        var plaintext = "Hello, World!"u8.ToArray();
        var associatedData = "metadata"u8.ToArray();

        var encrypted = Crypt.Encrypt(key, plaintext, associatedData);
        encrypted.Length.Should().Be(12 + 16 + plaintext.Length); // Nonce + Tag + Ciphertext

        var decrypted = Crypt.Decrypt(key, encrypted, associatedData);
        decrypted.Should().BeEquivalentTo(plaintext);
    }

    [TestMethod]
    public void Decrypt_WithWrongKey_ShouldThrowCryptographicException()
    {
        using var key1 = CryptKey.CreateRandom();
        using var key2 = CryptKey.CreateRandom();
        var plaintext = "Hello, World!"u8.ToArray();
        var encrypted = Crypt.Encrypt(key1, plaintext);

        var act = () => Crypt.Decrypt(key2, encrypted);
        act.Should().Throw<System.Security.Cryptography.CryptographicException>();
    }

    [TestMethod]
    public void Decrypt_WithTamperedCiphertext_ShouldThrowCryptographicException()
    {
        using var key = CryptKey.CreateRandom();
        var plaintext = "Hello, World!"u8.ToArray();
        var encrypted = Crypt.Encrypt(key, plaintext);
        encrypted[28] ^= 0xFF;

        var act = () => Crypt.Decrypt(key, encrypted);
        act.Should().Throw<System.Security.Cryptography.CryptographicException>();
    }

    [TestMethod]
    public void Decrypt_WithTamperedTag_ShouldThrowCryptographicException()
    {
        using var key = CryptKey.CreateRandom();
        var plaintext = "Hello, World!"u8.ToArray();
        var encrypted = Crypt.Encrypt(key, plaintext);
        encrypted[12] ^= 0xFF;

        var act = () => Crypt.Decrypt(key, encrypted);
        act.Should().Throw<System.Security.Cryptography.CryptographicException>();
    }

    [TestMethod]
    public void Decrypt_WithWrongAssociatedData_ShouldThrowCryptographicException()
    {
        using var key = CryptKey.CreateRandom();
        var plaintext = "Hello, World!"u8.ToArray();
        var associatedData = "metadata"u8.ToArray();
        var wrongAssociatedData = "wrong"u8.ToArray();
        var encrypted = Crypt.Encrypt(key, plaintext, associatedData);

        var act = () => Crypt.Decrypt(key, encrypted, wrongAssociatedData);
        act.Should().Throw<System.Security.Cryptography.CryptographicException>();
    }

    [TestMethod]
    public void Encrypt_Decrypt_EmptyData_ShouldReturnEmptyData()
    {
        using var key = CryptKey.CreateRandom();
        var plaintext = Array.Empty<byte>();

        var encrypted = Crypt.Encrypt(key, plaintext);
        var decrypted = Crypt.Decrypt(key, encrypted);

        decrypted.Should().BeEmpty();
        encrypted.Length.Should().Be(28); // Nonce + Tag only
    }

    [TestMethod]
    public void Encrypt_Decrypt_LargeData_ShouldReturnOriginalData()
    {
        using var key = CryptKey.CreateRandom();
        var plaintext = new byte[1024 * 1024]; // 1 MB
        System.Security.Cryptography.RandomNumberGenerator.Fill(plaintext);

        var encrypted = Crypt.Encrypt(key, plaintext);
        var decrypted = Crypt.Decrypt(key, encrypted);
        decrypted.Should().BeEquivalentTo(plaintext);
    }

    [TestMethod]
    public void Encrypt_MultipleTimes_ShouldProduceDifferentOutputs()
    {
        using var key = CryptKey.CreateRandom();
        var plaintext = "Hello, World!"u8.ToArray();

        var encrypted1 = Crypt.Encrypt(key, plaintext);
        var encrypted2 = Crypt.Encrypt(key, plaintext);
        encrypted1.Should().NotBeEquivalentTo(encrypted2);
    }
}