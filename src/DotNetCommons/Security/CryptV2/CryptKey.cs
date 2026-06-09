using System.Security.Cryptography;
using System.Text;

namespace DotNetCommons.Security.CryptV2;

public class CryptKey : IDisposable
{
    private static Encoding Utf8NoBom { get; } = new UTF8Encoding(false);

    private byte[]? _keyBuffer;
    private byte[]? _saltBuffer;

    /// Encoding used for string operations. Defaults to UTF8 without BOM.
    public Encoding Encoding { get; set; } = Utf8NoBom;

    /// Desired length of the key in bits.
    public int KeyBits => KeyLength * 8;

    /// Desired length of the key in bytes.
    public int KeyLength { get; }

    public ReadOnlySpan<byte> Key => _keyBuffer ?? throw new CryptographicException("Key not set");
    public ReadOnlySpan<byte> Salt => _saltBuffer ?? throw new CryptographicException("Salt not set");

    /// Represents a cryptographic key that provides key management and operations for various cryptographic methods, including
    /// random generation, derivation from PBKDF2 or SHA hashes, and management of associated salt values.
    public CryptKey(int keyLength)
    {
        KeyLength = keyLength;
    }

    /// <summary>
    /// Represents a cryptographic key that facilitates secure key management and supports a variety of cryptographic operations.
    /// This includes generating random keys, deriving keys using PBKDF2 or SHA hash algorithms, and managing associated salt values.
    /// </summary>
    /// <param name="cryptMethod">Specifies the cryptographic method that ends up consuming the key; used for determining the
    /// key size.</param>
    public CryptKey(CryptMethod cryptMethod)
        : this(GetKeySize(cryptMethod))
    {
    }

    /// Creates a new instance of the CryptKey class with a randomly generated cryptographic key and salt,
    /// suitable for the specified cryptographic method.
    /// <param name="cryptMethod">The cryptographic method used to determine the key size (e.g., Aes128, Aes192, Aes256).</param>
    /// <returns>A new instance of the CryptKey class with a randomly generated key.</returns>
    public static CryptKey CreateRandom(CryptMethod cryptMethod = CryptMethod.Aes256)
    {
        var result = new CryptKey(cryptMethod);
        result.GenerateRandom();
        return result;
    }

    /// Releases all resources used by the CryptKey instance and securely clears cryptographic buffers.
    public void Dispose()
    {
        Clear();
    }

    /// Securely clears all cryptographic buffers and key material.
    public void Clear()
    {
        ClearSalt();
        ClearKey();
    }

    /// Securely clears the key.
    public void ClearKey()
    {
        if (_keyBuffer == null)
            return;

        CryptographicOperations.ZeroMemory(_keyBuffer);
        _keyBuffer = null;
    }

    /// Securely clears the salt.
    public void ClearSalt()
    {
        if (_saltBuffer == null)
            return;

        CryptographicOperations.ZeroMemory(_saltBuffer);
        _saltBuffer = null;
    }

    /// Sets the cryptographic key to the specified value. Ensures the provided key meets the required length for the cryptographic
    /// operations; if the key is longer than needed, it will be truncated to the required length.
    public void SetKey(ReadOnlySpan<byte> key)
    {
        if (KeyLength > key.Length)
            throw WrongKeySize(KeyLength, key.Length);

        ClearKey();
        _keyBuffer = key[..KeyLength].ToArray();
    }

    /// Sets the cryptographic salt value.
    public void SetSalt(ReadOnlySpan<byte> salt)
    {
        ClearSalt();
        _saltBuffer = salt.ToArray();
    }


    // --- Key generation ---

    /// Generates a cryptographically secure random key of the specified length.
    public void GenerateRandom()
    {
        Clear();
        _keyBuffer = RandomNumberGenerator.GetBytes(KeyLength);
    }

    /// Derives a cryptographic key using the PBKDF2 (Password-Based Key Derivation Function 2) algorithm with
    /// configurable parameters for input data, iterations, salt size, and hashing algorithm. The derived key and
    /// salt are stored securely in memory, replacing any previous key material.
    /// <remarks>
    /// The generated salt is stored along with the new key, but is not specifically used otherwise.
    /// </remarks>
    /// <param name="data">The input data used as the base for key derivation.</param>
    /// <param name="iterations">The number of iterations for the PBKDF2 algorithm.</param>
    /// <param name="saltSize">The size, in bytes, of the randomly generated cryptographic salt.</param>
    /// <param name="hashMethod">The hashing algorithm to use with PBKDF2.</param>
    public void GeneratePbkdf2(byte[] data, int iterations = 100_000, int saltSize = 16, HashMethod hashMethod = HashMethod.Sha256)
    {
        Clear();

        var hashAlgorithm = GetAlgorithmName(hashMethod);
        _saltBuffer       = RandomNumberGenerator.GetBytes(saltSize);
        _keyBuffer        = Rfc2898DeriveBytes.Pbkdf2(data, _saltBuffer, iterations, hashAlgorithm, KeyLength);
    }

    /// Derives a cryptographic key using the PBKDF2 (Password-Based Key Derivation Function 2) algorithm with
    /// configurable parameters for input data, iterations, salt size, and hashing algorithm. The derived key and
    /// salt are stored securely in memory, replacing any previous key material.
    /// <remarks>
    /// The generated salt is stored along with the new key, but is not specifically used otherwise.
    /// </remarks>
    /// <param name="data">The input data used as the base for key derivation.</param>
    /// <param name="iterations">The number of iterations for the PBKDF2 algorithm.</param>
    /// <param name="saltSize">The size, in bytes, of the randomly generated cryptographic salt.</param>
    /// <param name="hashMethod">The hashing algorithm to use with PBKDF2.</param>
    public void GeneratePbkdf2(string data, int iterations = 100_000, int saltSize = 16, HashMethod hashMethod = HashMethod.Sha256)
    {
        Clear();

        var buffer = Encoding.GetBytes(data);
        GeneratePbkdf2(buffer, iterations, saltSize, hashMethod);
        CryptographicOperations.ZeroMemory(buffer);
    }

    /// Derives a cryptographic key from the SHA hash of the input data, using the specified hashing method.
    /// The resulting hash is truncated as needed to fit the key length of the current instance.
    /// Ensures secure cleanup of any cryptographic buffers used during the operation.
    /// <param name="data">The input byte array to be hashed and used for key derivation.</param>
    /// <param name="hashMethod">The hashing algorithm to use for the SHA hash computation.</param>
    public void GenerateFromSha(byte[] data, HashMethod hashMethod = HashMethod.Sha256)
    {
        Clear();

        using var sha = GetAlgorithm(hashMethod);

        var hash = sha.ComputeHash(data);
        SetKey(hash);
        CryptographicOperations.ZeroMemory(hash);
    }

    /// Derives a cryptographic key from the SHA hash of the input data, using the specified hashing method.
    /// The resulting hash is truncated as needed to fit the key length of the current instance.
    /// Ensures secure cleanup of any cryptographic buffers used during the operation.
    /// <param name="data">The input data to be hashed and used for key derivation.</param>
    /// <param name="hashMethod">The hashing algorithm to use for the SHA hash computation.</param>
    public void GenerateFromSha(string data, HashMethod hashMethod = HashMethod.Sha256)
    {
        Clear();

        var buffer = Encoding.GetBytes(data);
        GenerateFromSha(buffer, hashMethod);
        CryptographicOperations.ZeroMemory(buffer);
    }


    // --- Derivation of message keys ---

    /// Derives a new cryptographic key using HMAC (Hash-based Message Authentication Code) on the current key and the provided context.
    /// This method is useful for generating message-specific keys for cryptographic operations based on the current key material.
    /// <param name="context">The context data used to derive the new key. This serves as input to the HMAC computation.</param>
    /// <param name="hashMethod">The hash method used for the HMAC operation.</param>
    /// <returns>A new instance of <see cref="CryptKey"/> initialized with the derived key material.</returns>
    public CryptKey DeriveHmacKey(byte[] context, HashMethod hashMethod = HashMethod.Sha256)
    {
        if (_keyBuffer == null)
            throw new CryptographicException("Unable to derive message key: Key not set");

        using var hmac = GetHmacAlgorithm(hashMethod, _keyBuffer);

        var newKey = hmac.ComputeHash(context);
        var result = new CryptKey(KeyLength);
        result.SetKey(newKey);

        CryptographicOperations.ZeroMemory(newKey);
        return result;
    }

    /// Derives a new cryptographic key using HMAC (Hash-based Message Authentication Code) on the current key and the provided context.
    /// This method is useful for generating message-specific keys for cryptographic operations based on the current key material.
    /// <param name="context">The context data used to derive the new key. This serves as input to the HMAC computation.</param>
    /// <param name="hashMethod">The hash method used for the HMAC operation.</param>
    /// <returns>A new instance of <see cref="CryptKey"/> initialized with the derived key material.</returns>
    public CryptKey DeriveHmacKey(string context, HashMethod hashMethod = HashMethod.Sha256)
    {
        var buffer = Encoding.GetBytes(context);
        var result = DeriveHmacKey(buffer, hashMethod);

        CryptographicOperations.ZeroMemory(buffer);
        return result;
    }


    // --- Private methods ---

    private static HashAlgorithm GetAlgorithm(HashMethod algorithm) =>
        algorithm switch
        {
            HashMethod.Sha256 => SHA256.Create(),
            HashMethod.Sha512 => SHA512.Create(),
            _                 => throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null)
        };

    private static HashAlgorithmName GetAlgorithmName(HashMethod algorithm) =>
        algorithm switch
        {
            HashMethod.Sha256 => HashAlgorithmName.SHA256,
            HashMethod.Sha512 => HashAlgorithmName.SHA512,
            _                 => throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null)
        };

    private static HashAlgorithm GetHmacAlgorithm(HashMethod algorithm, byte[] key) =>
        algorithm switch
        {
            HashMethod.Sha256 => new HMACSHA256(key),
            HashMethod.Sha512 => new HMACSHA512(key),
            _                 => throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null)
        };

    private static int GetKeySize(CryptMethod algorithm) =>
        algorithm switch
        {
            CryptMethod.Aes128 => 16,
            CryptMethod.Aes192 => 24,
            CryptMethod.Aes256 => 32,
            _                  => throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null)
        };

    private static Exception WrongKeySize(int needed, int given)
    {
        throw new CryptographicException($"Wrong key length {given * 8} bits, expected {needed * 8} bits");
    }
}
