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

        var encrypted = Crypt.EncryptAesGcm(key, plaintext);
        encrypted.Length.Should().Be(12 + 16 + plaintext.Length); // Nonce + Tag + Ciphertext

        var decrypted = Crypt.DecryptAesGcm(key, encrypted);
        decrypted.Should().BeEquivalentTo(plaintext);
    }

    [TestMethod]
    public void Encrypt_Decrypt_RoundTrip_WithAssociatedData_ShouldReturnOriginalData()
    {
        using var key = CryptKey.CreateRandom();
        var plaintext = "Hello, World!"u8.ToArray();
        var associatedData = "metadata"u8.ToArray();

        var encrypted = Crypt.EncryptAesGcm(key, plaintext, associatedData);
        encrypted.Length.Should().Be(12 + 16 + plaintext.Length); // Nonce + Tag + Ciphertext

        var decrypted = Crypt.DecryptAesGcm(key, encrypted, associatedData);
        decrypted.Should().BeEquivalentTo(plaintext);
    }

    [TestMethod]
    public void Decrypt_WithWrongKey_ShouldThrowCryptographicException()
    {
        using var key1 = CryptKey.CreateRandom();
        using var key2 = CryptKey.CreateRandom();
        var plaintext = "Hello, World!"u8.ToArray();
        var encrypted = Crypt.EncryptAesGcm(key1, plaintext);

        var act = () => Crypt.DecryptAesGcm(key2, encrypted);
        act.Should().Throw<System.Security.Cryptography.CryptographicException>();
    }

    [TestMethod]
    public void Decrypt_WithTamperedCiphertext_ShouldThrowCryptographicException()
    {
        using var key = CryptKey.CreateRandom();
        var plaintext = "Hello, World!"u8.ToArray();
        var encrypted = Crypt.EncryptAesGcm(key, plaintext);
        encrypted[28] ^= 0xFF;

        var act = () => Crypt.DecryptAesGcm(key, encrypted);
        act.Should().Throw<System.Security.Cryptography.CryptographicException>();
    }

    [TestMethod]
    public void Decrypt_WithTamperedTag_ShouldThrowCryptographicException()
    {
        using var key = CryptKey.CreateRandom();
        var plaintext = "Hello, World!"u8.ToArray();
        var encrypted = Crypt.EncryptAesGcm(key, plaintext);
        encrypted[12] ^= 0xFF;

        var act = () => Crypt.DecryptAesGcm(key, encrypted);
        act.Should().Throw<System.Security.Cryptography.CryptographicException>();
    }

    [TestMethod]
    public void Decrypt_WithWrongAssociatedData_ShouldThrowCryptographicException()
    {
        using var key = CryptKey.CreateRandom();
        var plaintext = "Hello, World!"u8.ToArray();
        var associatedData = "metadata"u8.ToArray();
        var wrongAssociatedData = "wrong"u8.ToArray();
        var encrypted = Crypt.EncryptAesGcm(key, plaintext, associatedData);

        var act = () => Crypt.DecryptAesGcm(key, encrypted, wrongAssociatedData);
        act.Should().Throw<System.Security.Cryptography.CryptographicException>();
    }

    [TestMethod]
    public void Encrypt_Decrypt_EmptyData_ShouldReturnEmptyData()
    {
        using var key = CryptKey.CreateRandom();
        var plaintext = Array.Empty<byte>();

        var encrypted = Crypt.EncryptAesGcm(key, plaintext);
        var decrypted = Crypt.DecryptAesGcm(key, encrypted);

        decrypted.Should().BeEmpty();
        encrypted.Length.Should().Be(28); // Nonce + Tag only
    }

    [TestMethod]
    public void Encrypt_Decrypt_LargeData_ShouldReturnOriginalData()
    {
        using var key = CryptKey.CreateRandom();
        var plaintext = new byte[1024 * 1024]; // 1 MB
        System.Security.Cryptography.RandomNumberGenerator.Fill(plaintext);

        var encrypted = Crypt.EncryptAesGcm(key, plaintext);
        var decrypted = Crypt.DecryptAesGcm(key, encrypted);
        decrypted.Should().BeEquivalentTo(plaintext);
    }

    [TestMethod]
    public void Encrypt_MultipleTimes_ShouldProduceDifferentOutputs()
    {
        using var key = CryptKey.CreateRandom();
        var plaintext = "Hello, World!"u8.ToArray();

        var encrypted1 = Crypt.EncryptAesGcm(key, plaintext);
        var encrypted2 = Crypt.EncryptAesGcm(key, plaintext);
        encrypted1.Should().NotBeEquivalentTo(encrypted2);
    }

    [TestMethod]
    public void Encrypt_Decrypt_WithCompression_ShouldReturnOriginalData()
    {
        using var key = CryptKey.CreateRandom();
        var plaintext = string.Join('\n', Enumerable.Repeat("The same line compresses well.", 5000))
            .Select(c => (byte)c)
            .ToArray();

        var encrypted = Crypt.EncryptAesGcm(key, plaintext, compress: true);
        var decrypted = Crypt.DecryptAesGcm(key, encrypted, decompress: true);

        decrypted.Should().Equal(plaintext);
        encrypted.Length.Should().BeLessThan(12 + 16 + plaintext.Length);
    }

    [TestMethod]
    public void Encrypt_Decrypt_WithCompressionAndAssociatedData_ShouldReturnOriginalData()
    {
        using var key = CryptKey.CreateRandom();
        var plaintext = string.Join('|', Enumerable.Repeat("metadata-bound payload", 1000))
            .Select(c => (byte)c)
            .ToArray();
        var associatedData = "metadata"u8.ToArray();

        var encrypted = Crypt.EncryptAesGcm(key, plaintext, compress: true, associatedData: associatedData);
        var decrypted = Crypt.DecryptAesGcm(key, encrypted, decompress: true, associatedData: associatedData);

        decrypted.Should().Equal(plaintext);
    }

    [TestMethod]
    public void Decrypt_WithCompressionAndWrongAssociatedData_ShouldThrowCryptographicException()
    {
        using var key = CryptKey.CreateRandom();
        var plaintext = string.Join('|', Enumerable.Repeat("metadata-bound payload", 1000))
            .Select(c => (byte)c)
            .ToArray();
        var encrypted = Crypt.EncryptAesGcm(key, plaintext, compress: true, associatedData: "metadata"u8.ToArray());

        var act = () => Crypt.DecryptAesGcm(key, encrypted, decompress: true, associatedData: "wrong"u8.ToArray());

        act.Should().Throw<System.Security.Cryptography.CryptographicException>();
    }

    [TestMethod]
    public void Encrypt_WithCompressFalse_ShouldUseNormalAesGcmPayloadLength()
    {
        using var key = CryptKey.CreateRandom();
        var plaintext = "Hello, World!"u8.ToArray();

        var encrypted = Crypt.EncryptAesGcm(key, plaintext, compress: false);
        var decrypted = Crypt.DecryptAesGcm(key, encrypted, decompress: false);

        encrypted.Length.Should().Be(12 + 16 + plaintext.Length);
        decrypted.Should().Equal(plaintext);
    }

    [TestMethod]
    public void Decrypt_WithDecompressTrue_OnUncompressedCiphertext_ShouldThrowInvalidDataException()
    {
        using var key = CryptKey.CreateRandom();
        var plaintext = "Hello, World!"u8.ToArray();
        var encrypted = Crypt.EncryptAesGcm(key, plaintext);

        var act = () => Crypt.DecryptAesGcm(key, encrypted, decompress: true);

        act.Should().Throw<InvalidOperationException>();
    }
}
