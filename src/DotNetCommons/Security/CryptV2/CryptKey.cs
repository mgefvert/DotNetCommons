using System.Security.Cryptography;
using System.Text;

namespace DotNetCommons.Security.CryptV2;

public class CryptKey : IDisposable
{
    public static Encoding Utf8 { get; } = new UTF8Encoding(false);

    public CryptAlgorithm Algorithm { get; }
    public byte[] KeyBuffer { get; }

    public static CryptKey CreateRandom(CryptAlgorithm algorithm = CryptAlgorithm.Aes256)
    {
        var len = GetKeySize(algorithm);
        var key = RandomNumberGenerator.GetBytes(len);
        return new CryptKey(key, algorithm);
    }

    public static CryptKey FromPbkdf2(string key,
        int iterations = 100_000, int saltSize = 16, CryptAlgorithm algorithm = CryptAlgorithm.Aes256)
    {
        var hashAlgorithm = GetHashAlgorithmName(algorithm);
        var keySize       = GetKeySize(algorithm);
        var keyBytes = Rfc2898DeriveBytes.Pbkdf2(Utf8.GetBytes(key), RandomNumberGenerator.GetBytes(saltSize),
            iterations, hashAlgorithm, keySize);

        return new CryptKey(keyBytes, algorithm);
    }

    public static CryptKey FromXorPad(byte[] key, CryptAlgorithm algorithm = CryptAlgorithm.Aes256)
    {
        var len    = GetKeySize(algorithm);
        var result = new byte[len];
        var maxlen = Math.Max(len, key.Length);
        for (var i = 0; i < maxlen; i++)
            result[i % len] ^= key[i % key.Length];

        return new CryptKey(result, algorithm);
    }

    public static CryptKey FromXorPad(string key, CryptAlgorithm algorithm = CryptAlgorithm.Aes256, Encoding? encoding = null)
    {
        return FromXorPad((encoding ?? Utf8).GetBytes(key), algorithm);
    }

    public static CryptKey FromSha(byte[] key, CryptAlgorithm algorithm = CryptAlgorithm.Aes256)
    {
        var sha  = GetAlgorithm(algorithm);
        var hash = sha.ComputeHash(key);
        var keySize = GetKeySize(algorithm);

        // Truncate hash to match required key size if needed
        var keyBytes = hash.Length > keySize 
            ? hash.AsSpan(0, keySize).ToArray() 
            : hash;

        if (keyBytes.Length != keySize)
            throw WrongKeySize(keySize, keyBytes.Length);

        return new CryptKey(keyBytes, algorithm);
    }

    public static CryptKey FromSha(string key, CryptAlgorithm algorithm = CryptAlgorithm.Aes256, Encoding? encoding = null)
    {
        var keyBytes = (encoding ?? Utf8).GetBytes(key);
        return FromSha(keyBytes, algorithm);
    }

    public CryptKey(byte[] key, CryptAlgorithm algorithm = CryptAlgorithm.Aes256)
    {
        var keySize = GetKeySize(algorithm);
        if (keySize != key.Length)
            throw WrongKeySize(keySize, key.Length);

        Algorithm = algorithm;
        KeyBuffer = key;
    }

    public void Dispose()
    {
        CryptographicOperations.ZeroMemory(KeyBuffer);
    }

    public CryptKey GenerateMessageKey(byte[] messageKey)
    {
        using var hmac = GetHmacAlgorithm(Algorithm, KeyBuffer);
        return new CryptKey(hmac.ComputeHash(messageKey));
    }

    public CryptKey GenerateMessageKey(string messageKey, Encoding? encoding = null)
    {
        return GenerateMessageKey((encoding ?? Utf8).GetBytes(messageKey));
    }

    private static HashAlgorithm GetAlgorithm(CryptAlgorithm algorithm) =>
        algorithm switch
        {
            CryptAlgorithm.Aes128 => SHA1.Create(),
            CryptAlgorithm.Aes256 => SHA256.Create(),
            CryptAlgorithm.Aes512 => SHA512.Create(),
            _                     => throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null)
        };

    private static HashAlgorithmName GetHashAlgorithmName(CryptAlgorithm algorithm) =>
        algorithm switch
        {
            CryptAlgorithm.Aes128 => HashAlgorithmName.SHA1,
            CryptAlgorithm.Aes256 => HashAlgorithmName.SHA256,
            CryptAlgorithm.Aes512 => HashAlgorithmName.SHA512,
            _                     => throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null)
        };

    private static HashAlgorithm GetHmacAlgorithm(CryptAlgorithm algorithm, byte[] key) =>
        algorithm switch
        {
            CryptAlgorithm.Aes128 => new HMACSHA1(key),
            CryptAlgorithm.Aes256 => new HMACSHA256(key),
            CryptAlgorithm.Aes512 => new HMACSHA512(key),
            _                     => throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null)
        };

    private static int GetKeySize(CryptAlgorithm algorithm) =>
        algorithm switch
        {
            CryptAlgorithm.Aes128 => 16,
            CryptAlgorithm.Aes256 => 32,
            CryptAlgorithm.Aes512 => 64,
            _                     => throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null)
        };

    private static Exception WrongKeySize(int needed, int given)
    {
        throw new CryptographicException($"Wrong key length {given * 8} bits, expected {needed * 8} bits");
    }
}