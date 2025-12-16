using System.Security.Cryptography;
using System.Text;

namespace DotNetCommons.Security.CryptV1;

/// <summary>
/// Represents a cryptographic key used for encryption and decryption purposes. This class ensures the key provided is of the
/// correct length and supports generating message-specific keys using the HMAC-SHA256 algorithm.
/// </summary>
public class CryptKey : IDisposable
{
    public const int KeyLength = 32;
    private const string AlreadyDisposed = "Encryption key has already been disposed";
    
    private static readonly Encoding Utf8 = new UTF8Encoding(false);
    private bool _disposed;

    public byte[] KeyBuffer { get; }

    /// <summary>
    /// Represents a cryptographic key used for encryption and decryption purposes. This class ensures the key provided is of the
    /// correct length and supports generating message-specific keys using the HMAC-SHA256 algorithm.
    /// </summary>
    /// <param name="key">A string key that will be the converted to a 256-bit key using Utf8 encoding and XOR padding.</param>
    public CryptKey(byte[] key)
    {
        KeyBuffer = key;
        if (KeyBuffer.Length != KeyLength)
            throw new CryptographicException($"Invalid key length {KeyBuffer.Length} bytes, expected {KeyLength} bytes");
    }

    /// <summary>
    /// Represents a cryptographic key used for encryption and decryption purposes. This class ensures the key provided is of the
    /// correct length and supports generating message-specific keys using the HMAC-SHA256 algorithm.
    /// </summary>
    /// <param name="key">A string key that will be the converted to a 256-bit key using Utf8 encoding and XOR padding.</param>
    public CryptKey(string key)
    {
        KeyBuffer = XorPadKey(key);
    }

    ~CryptKey()
    {
        Dispose();
    }

    /// <summary>
    /// Releases the resources used by the current instance of <see cref="CryptKey"/>. This method clears the internal key buffer
    /// to ensure sensitive data is removed from memory and prevents further use of disposed objects.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            Array.Clear(KeyBuffer);
            _disposed = true;
        }
    }

    /// <summary>
    /// Creates a new encryption key by combining the current key with a message key using the HMAC-SHA256 algorithm.
    /// </summary>
    /// <param name="messageKey">The byte array representing the message key to be combined with the current key.</param>
    /// <returns>A new <see cref="CryptKey"/> instance created from the HMAC-SHA256 hash of the current key and the provided
    ///     message key.</returns>
    public CryptKey GenerateMessageKey(byte[] messageKey)
    {
        if (_disposed)
            throw new ObjectDisposedException(AlreadyDisposed);
        
        using var hmac = new HMACSHA256(KeyBuffer);
        return new CryptKey(hmac.ComputeHash(messageKey));
    }

    /// <summary>
    /// Creates a new encryption key by combining the current key with a message key represented as a Utf8 string,
    /// using the HMAC-SHA256 algorithm.
    /// </summary>
    /// <param name="messageKey">The string representation of the message key to be combined with the current key.</param>
    /// <returns>A new <see cref="CryptKey"/> instance created from the HMAC-SHA256 hash of the current key and the provided
    /// message key string.</returns>
    public CryptKey GenerateMessageKey(string messageKey)
    {
        return GenerateMessageKey(Utf8.GetBytes(messageKey));
    }

    /// <summary>
    /// Pads or trims the input key to ensure it matches the specified length. If the key is shorter than the
    /// desired length, the method repeatedly applies XOR to extend it. This creates a key of the exact required size.
    /// </summary>
    private static byte[] XorPadKey(byte[] key, int length)
    {
        var result = new byte[length];
        var maxlen = Math.Max(length, key.Length);
        for (var i = 0; i < maxlen; i++)
            result[i % length] ^= key[i % key.Length];

        return result;
    }

    /// <summary>
    /// Converts the provided string in Utf8 encoding into a byte array suitable for use as an encryption key.
    /// </summary>
    private static byte[] XorPadKey(string key)
    {
        return XorPadKey(Utf8.GetBytes(key), KeyLength);
    }
}