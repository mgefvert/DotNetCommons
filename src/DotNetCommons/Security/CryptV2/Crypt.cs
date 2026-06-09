using System.Security.Cryptography;
using DotNetCommons.IO;

namespace DotNetCommons.Security.CryptV2;

public static class Crypt
{
    private const int TagLength = 16;
    private const int NonceLength = 12;

    /// <summary>
    /// Encrypts the specified plaintext data using the provided cryptographic key and optional associated data.
    /// </summary>
    /// <param name="key">The cryptographic key used for encryption.</param>
    /// <param name="plaintextData">The plaintext data to be encrypted.</param>
    /// <param name="associatedData">Optional associated data to be included in the encryption process
    /// for additional authentication. This data is not encrypted but is authenticated with the ciphertext.</param>
    /// <returns>A byte array containing the encrypted data, including the nonce, authentication tag, and ciphertext.</returns>
    public static byte[] EncryptAesGcm(CryptKey key, byte[] plaintextData, byte[]? associatedData = null)
    {
        var result = new byte[TagLength + NonceLength + plaintextData.Length];

        var nonce  = result.AsSpan(0, NonceLength);
        var tag    = result.AsSpan(NonceLength, TagLength);
        var buffer = result.AsSpan(NonceLength + TagLength);

        RandomNumberGenerator.Fill(nonce);

        using var aes = new AesGcm(key.Key, 16);
        aes.Encrypt(nonce, plaintextData, buffer, tag, associatedData);

        return result;
    }

    /// <summary>
    /// Encrypts the specified plaintext data using AES-GCM, optionally compressing it before encryption.
    /// </summary>
    /// <param name="key">The cryptographic key used for encryption.</param>
    /// <param name="plaintextData">The plaintext data to be encrypted.</param>
    /// <param name="compress">If true, the plaintext data is compressed before encryption.</param>
    /// <param name="associatedData">Optional associated data authenticated with the ciphertext.</param>
    /// <returns>A byte array containing the encrypted data, including the nonce, authentication tag, and ciphertext.</returns>
    public static byte[] EncryptAesGcm(CryptKey key, byte[] plaintextData, bool compress, byte[]? associatedData = null)
    {
        if (!compress)
            return EncryptAesGcm(key, plaintextData, associatedData);

        var compressedData = Compression.Compress(plaintextData, CompressionMethod.Brotli);
        try
        {
            return EncryptAesGcm(key, compressedData, associatedData);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(compressedData);
        }
    }

    /// <summary>
    /// Decrypts the specified encrypted data using the provided cryptographic key and optional associated data.
    /// </summary>
    /// <param name="key">The cryptographic key used for decryption.</param>
    /// <param name="encryptedData">The encrypted data to be decrypted, including the nonce, authentication tag, and ciphertext.</param>
    /// <param name="associatedData">Optional associated data that was included during the encryption process
    /// for additional authentication. This data must match the one used during encryption to successfully decrypt.</param>
    /// <returns>A byte array containing the original plaintext data.</returns>
    public static byte[] DecryptAesGcm(CryptKey key, byte[] encryptedData, byte[]? associatedData = null)
    {
        var nonce  = encryptedData.AsSpan(0, NonceLength);
        var tag    = encryptedData.AsSpan(NonceLength, TagLength);
        var cipher = encryptedData.AsSpan(NonceLength + TagLength);

        var result = new byte[cipher.Length];

        using var aes = new AesGcm(key.Key, 16);
        aes.Decrypt(nonce, cipher, tag, result, associatedData);

        return result;
    }

    /// <summary>
    /// Decrypts the specified AES-GCM encrypted data, optionally decompressing the decrypted payload.
    /// </summary>
    /// <param name="key">The cryptographic key used for decryption.</param>
    /// <param name="encryptedData">The encrypted data to be decrypted, including the nonce, authentication tag, and ciphertext.</param>
    /// <param name="decompress">If true, the decrypted payload is decompressed before being returned.</param>
    /// <param name="associatedData">Optional associated data authenticated with the ciphertext.</param>
    /// <returns>The original plaintext data.</returns>
    public static byte[] DecryptAesGcm(CryptKey key, byte[] encryptedData, bool decompress, byte[]? associatedData = null)
    {
        var decryptedData = DecryptAesGcm(key, encryptedData, associatedData);
        if (!decompress)
            return decryptedData;

        try
        {
            return Compression.Decompress(decryptedData, CompressionMethod.Brotli);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(decryptedData);
        }
    }
}
