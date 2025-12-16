using System.Security.Cryptography;

namespace DotNetCommons.Security.CryptV2;

public static class Crypt
{
    private const int TagLength = 16;
    private const int NonceLength = 12;

    public static byte[] Encrypt(CryptKey key, byte[] plaintextData, byte[]? associatedData = null)
    {
        var result = new byte[TagLength + NonceLength + plaintextData.Length];

        var nonce  = result.AsSpan(0, NonceLength);
        var tag    = result.AsSpan(NonceLength, TagLength);
        var buffer = result.AsSpan(NonceLength + TagLength);

        RandomNumberGenerator.Fill(nonce);

        using var aes = new AesGcm(key.KeyBuffer, 16);
        aes.Encrypt(nonce, plaintextData, buffer, tag, associatedData);

        return result;
    }

    public static byte[] Decrypt(CryptKey key, byte[] encryptedData, byte[]? associatedData = null)
    {
        var nonce  = encryptedData.AsSpan(0, NonceLength);
        var tag    = encryptedData.AsSpan(NonceLength, TagLength);
        var cipher = encryptedData.AsSpan(NonceLength + TagLength);

        var result = new byte[cipher.Length];

        using var aes = new AesGcm(key.KeyBuffer, 16);
        aes.Decrypt(nonce, cipher, tag, result, associatedData);

        return result;
    }
}