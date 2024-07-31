using DotNetCommons.IO;
using System.Security.Cryptography;
using System.Text;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Security;

/// <summary>
/// Class that provides easy encryption and decryption of data using AES-256 encryption.
/// </summary>
public static class Crypt
{
    public const int KeyLength = 32;

    /// <summary>
    /// Create a new key using HMAC-SHA256 based on a master key and a specific message key.
    /// </summary>
    public static byte[] CreateKey(byte[] masterKey, byte[] messageKey)
    {
        using var hmac = new HMACSHA256(masterKey);
        return hmac.ComputeHash(messageKey);
    }

    /// <summary>
    /// Create a new key using HMAC-SHA256 based on a master key and a specific message key.
    /// </summary>
    public static byte[] CreateKey(string masterKey, string messageKey)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(masterKey));
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(messageKey));
    }

    /// <summary>
    /// Decrypt a byte message using a given key. Keys are padded if they're not the exact length required.
    /// </summary>
    public static byte[] Decrypt(byte[] key, byte[] data)
    {
        using var aes = Aes.Create();
        aes.Key = PadKey(key, KeyLength);
        using var mem = new MemoryStream(data);
        byte[] buffer;

        using (var reader = new BinaryReader(mem, Encoding.UTF8, true))
        {
            var ivlen = reader.ReadInt32();
            aes.IV = reader.ReadBytes(ivlen);
            buffer = new byte[reader.ReadInt32()];
        }

        using var crypto = new CryptoStream(mem, aes.CreateDecryptor(), CryptoStreamMode.Read);

        var read = StreamTools.ReadIntoBuffer(crypto, buffer);
        if (read != buffer.Length)
            throw new Exception($"Expected {buffer.Length} of decrypted data, got {read}.");

        return buffer;
    }

    /// <summary>
    /// Decrypt a string message using a given key. Keys are padded if they're not the exact length required.
    /// </summary>
    public static string Decrypt(byte[] key, string data)
    {
        var bytes = Convert.FromBase64String(data);
        return Encoding.UTF8.GetString(Decrypt(key, bytes));
    }

    /// <summary>
    /// Encrypt a byte message using a given key. Keys are padded if they're not the exact length required.
    /// </summary>
    public static byte[] Encrypt(byte[] key, byte[] data)
    {
        using var aes = Aes.Create();
        aes.GenerateIV();
        aes.Key = PadKey(key, KeyLength);

        using var mem = new MemoryStream();
        mem.Write(BitConverter.GetBytes(aes.IV.Length), 0, 4);
        mem.Write(aes.IV, 0, aes.IV.Length);
        mem.Write(BitConverter.GetBytes(data.Length), 0, 4);

        using (var crypto = new CryptoStream(mem, aes.CreateEncryptor(), CryptoStreamMode.Write))
        {
            crypto.Write(data, 0, data.Length);
        }

        return mem.ToArray();
    }

    /// <summary>
    /// Encrypt a string message using a given key. Keys are padded if they're not the exact length required.
    /// </summary>
    public static string Encrypt(byte[] key, string data)
    {
        var bytes = Encoding.UTF8.GetBytes(data);
        return Convert.ToBase64String(Encrypt(key, bytes));
    }

    private static byte[] PadKey(byte[] key, int length)
    {
        var result = new byte[length];
        var maxlen = Math.Max(length, key.Length);
        for (var i = 0; i < maxlen; i++)
            result[i % length] ^= key[i % key.Length];

        return result;
    }

    /// <summary>
    /// Burn a key after use to prevent information leaks.
    /// </summary>
    public static void Zero(byte[] key)
    {
        for (var i = 0; i < key.Length; i++)
            key[i] = 0;
    }
}