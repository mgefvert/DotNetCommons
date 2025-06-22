using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Security;

/// <summary>
/// Class that provides easy encryption and decryption of data using AES-256 encryption and CBC mode with an automatically
/// generated IV, if need be. 
/// </summary>
public static class Crypt
{
    public static CryptIoReader GetDecryptionStream(CryptKey key, Stream encryptedStream, bool decompress)
    {
        var aes = Aes.Create();
        aes.Key = key.KeyBuffer;
        int messageSize;
        
        using (var header = new BinaryReader(encryptedStream, Encoding.UTF8, true))
        {
            var ivSize = header.ReadInt32();
            aes.IV = header.ReadBytes(ivSize); 
            messageSize = header.ReadInt32();
        }

        var cryptoStream = new CryptoStream(encryptedStream, aes.CreateDecryptor(), CryptoStreamMode.Read);

        GZipStream? gzipStream = null;
        if (decompress)
            gzipStream = new GZipStream(cryptoStream, CompressionMode.Decompress);

        return new CryptIoReader(aes, messageSize, encryptedStream, gzipStream, cryptoStream);
    }
    
    public static CryptIoWriter GetEncryptionStream(CryptKey key, int messageSize, Stream encryptedStream, bool compress)
    {
        using var aes = Aes.Create();
        aes.GenerateIV();
        aes.Key = key.KeyBuffer;

        encryptedStream.Write(BitConverter.GetBytes(aes.IV.Length), 0, 4);
        encryptedStream.Write(aes.IV, 0, aes.IV.Length);
        encryptedStream.Write(BitConverter.GetBytes(messageSize), 0, 4);

        var cryptoStream = new CryptoStream(encryptedStream, aes.CreateEncryptor(), CryptoStreamMode.Write);

        GZipStream? gzipStream = null;
        if (compress)
            gzipStream = new GZipStream(cryptoStream, CompressionLevel.Optimal);

        return new CryptIoWriter(aes, messageSize, encryptedStream, gzipStream, cryptoStream);
    }
    
    /// <summary>
    /// Decrypt a byte message using a given key.
    /// </summary>
    public static byte[] Decrypt(CryptKey key, byte[] data, bool decompress)
    {
        using var mem = new MemoryStream(data);
        using var io = GetDecryptionStream(key, mem, decompress);
        
        var result = new byte[io.MessageSize];
        io.IoStream.ReadExactly(result, 0, result.Length);
        
        return result;
    }

    /// <summary>
    /// Decrypt a Base64-encoded string message using a given key.
    /// </summary>
    public static string Decrypt(CryptKey key, string data, bool decompress)
    {
        var bytes = Convert.FromBase64String(data);
        return Encoding.UTF8.GetString(Decrypt(key, bytes, decompress));
    }

    /// <summary>
    /// Encrypt a byte message using a given key.
    /// </summary>
    public static byte[] Encrypt(CryptKey key, byte[] data, bool compress)
    {
        using var mem = new MemoryStream();
        using (var io = GetEncryptionStream(key, data.Length, mem, compress))
        {
            io.IoStream.Write(data, 0, data.Length);
        }

        return mem.ToArray();
    }

    /// <summary>
    /// Encrypt a Base64-encoded string message using a given key.
    /// </summary>
    public static string Encrypt(CryptKey key, string data, bool compress)
    {
        var bytes = Encoding.UTF8.GetBytes(data);
        var result = Encrypt(key, bytes, compress);
        return Convert.ToBase64String(result);
    }
}