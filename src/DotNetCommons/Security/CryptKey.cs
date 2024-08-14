using System.Security.Cryptography;
using System.Text;

namespace DotNetCommons.Security;

public class CryptKey : IDisposable
{
    public const int KeyLength = 32;
    private const string AlreadyDisposed = "Encryption key has already been disposed";
    
    private static readonly Encoding Utf8 = new UTF8Encoding(false);
    private bool _disposed;

    public byte[] KeyBuffer { get; }
    
    public CryptKey(byte[] key)
    {
        KeyBuffer = key;
        if (KeyBuffer.Length != KeyLength)
            throw new CryptographicException($"Invalid key length {KeyBuffer.Length} bytes, expected {KeyLength} bytes");
    }

    public CryptKey(string key)
    {
        KeyBuffer = StringToKey(key);
    }

    ~CryptKey()
    {
        Dispose();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Array.Clear(KeyBuffer);
            _disposed = true;
        }
    }

    /// <summary>
    /// Create a new key from the current key and a message key using HMAC-SHA256, suitable for encrypting a single message.
    /// </summary>
    public CryptKey GenerateMessageKey(byte[] messageKey)
    {
        if (_disposed)
            throw new ObjectDisposedException(AlreadyDisposed);
        
        using var hmac = new HMACSHA256(KeyBuffer);
        return new CryptKey(hmac.ComputeHash(messageKey));
    }

    /// <summary>
    /// Create a new key from the current key and a message key using HMAC-SHA256, suitable for encrypting a single message.
    /// </summary>
    public CryptKey GenerateMessageKey(string messageKey)
    {
        return GenerateMessageKey(StringToKey(messageKey));
    }

    private static byte[] PadKey(byte[] key, int length)
    {
        var result = new byte[length];
        var maxlen = Math.Max(length, key.Length);
        for (var i = 0; i < maxlen; i++)
            result[i % length] ^= key[i % key.Length];

        return result;
    }

    private static byte[] StringToKey(string key)
    {
        return PadKey(Utf8.GetBytes(key), KeyLength);
    }
}