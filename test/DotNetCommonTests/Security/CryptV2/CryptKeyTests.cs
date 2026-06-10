using DotNetCommons.Security.CryptV2;
using FluentAssertions;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace DotNetCommonTests.Security.CryptV2;

[TestClass]
public class CryptKeyTests
{
    [TestMethod]
    [DataRow(CryptMethod.Aes128, 16)]
    [DataRow(CryptMethod.Aes192, 24)]
    [DataRow(CryptMethod.Aes256, 32)]
    public void Constructor_WithCryptMethod_ShouldSetExpectedKeyLength(CryptMethod method, int expectedLength)
    {
        using var key = new CryptKey(method);

        key.KeyLength.Should().Be(expectedLength);
        key.KeyBits.Should().Be(expectedLength * 8);
    }

    [TestMethod]
    public void Constructor_WithInvalidCryptMethod_ShouldThrow()
    {
        var act = () => new CryptKey((CryptMethod)999);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("algorithm");
    }

    [TestMethod]
    public void Constructor_WithExplicitLength_ShouldSetLengthAndBits()
    {
        using var key = new CryptKey(7);

        key.KeyLength.Should().Be(7);
        key.KeyBits.Should().Be(56);
    }

    [TestMethod]
    public void EmptyKey_ShouldThrowWhenReadingKeyOrSalt()
    {
        using var key = new CryptKey(16);

        FluentActions.Invoking(() => key.Key.ToArray())
            .Should().Throw<CryptographicException>()
            .WithMessage("Key not set");

        FluentActions.Invoking(() => key.Salt.ToArray())
            .Should().Throw<CryptographicException>()
            .WithMessage("Salt not set");
    }

    [TestMethod]
    public void CreateRandom_ShouldUseAes256ByDefault()
    {
        using var key = CryptKey.CreateRandom();

        key.KeyLength.Should().Be(32);
        key.Key.Should().HaveCount(32);
        key.Key.ToArray().Should().Contain(value => value != 0);
    }

    [TestMethod]
    [DataRow(CryptMethod.Aes128, 16)]
    [DataRow(CryptMethod.Aes192, 24)]
    [DataRow(CryptMethod.Aes256, 32)]
    public void CreateRandom_WithCryptMethod_ShouldGenerateKeyOfExpectedLength(CryptMethod method, int expectedLength)
    {
        using var key = CryptKey.CreateRandom(method);

        key.KeyLength.Should().Be(expectedLength);
        key.Key.Should().HaveCount(expectedLength);
        key.Key.ToArray().Should().Contain(value => value != 0);
    }

    [TestMethod]
    public void GenerateRandom_ShouldReplaceKeyAndClearExistingSalt()
    {
        using var key = new CryptKey(16);
        key.SetKey(Enumerable.Range(1, 16).Select(x => (byte)x).ToArray());
        key.SetSalt([1, 2, 3, 4]);
        var oldKeyBuffer = GetKeyBuffer(key);
        var oldSaltBuffer = GetSaltBuffer(key);

        key.GenerateRandom();

        key.Key.Should().HaveCount(16);
        key.Key.ToArray().Should().Contain(value => value != 0);
        oldKeyBuffer.Should().AllBeEquivalentTo((byte)0);
        oldSaltBuffer.Should().AllBeEquivalentTo((byte)0);
        FluentActions.Invoking(() => key.Salt.ToArray())
            .Should().Throw<CryptographicException>();
    }

    [TestMethod]
    public void GenerateRandom_CalledTwice_ShouldProduceDifferentKeys()
    {
        using var key1 = CryptKey.CreateRandom();
        using var key2 = CryptKey.CreateRandom();

        key1.Key.ToArray().Should().NotEqual(key2.Key.ToArray());
    }

    [TestMethod]
    public void SetKey_WithExactLength_ShouldCopyInput()
    {
        using var key = new CryptKey(4);
        var input = new byte[] { 1, 2, 3, 4 };

        key.SetKey(input);
        input[0] = 99;

        key.Key.ToArray().Should().Equal(1, 2, 3, 4);
    }

    [TestMethod]
    public void SetKey_WithLongerInput_ShouldTruncateAndCopyInput()
    {
        using var key = new CryptKey(4);
        var input = new byte[] { 1, 2, 3, 4, 5, 6 };

        key.SetKey(input);
        input[0] = 99;

        key.Key.ToArray().Should().Equal(1, 2, 3, 4);
    }

    [TestMethod]
    public void SetKey_WithShortInput_ShouldThrowAndPreserveExistingKey()
    {
        using var key = new CryptKey(4);
        key.SetKey([1, 2, 3, 4]);

        var act = () => key.SetKey([1, 2, 3]);

        act.Should().Throw<CryptographicException>()
            .WithMessage("Wrong key length 24 bits, expected 32 bits");
        key.Key.ToArray().Should().Equal(1, 2, 3, 4);
    }

    [TestMethod]
    public void SetKey_ReplacingExistingKey_ShouldZeroPreviousKeyBuffer()
    {
        using var key = new CryptKey(4);
        key.SetKey([1, 2, 3, 4]);
        var oldBuffer = GetKeyBuffer(key);

        key.SetKey([5, 6, 7, 8]);

        oldBuffer.Should().AllBeEquivalentTo((byte)0);
        key.Key.ToArray().Should().Equal(5, 6, 7, 8);
    }

    [TestMethod]
    public void SetSalt_ShouldCopyInput()
    {
        using var key = new CryptKey(4);
        var salt = new byte[] { 1, 2, 3 };

        key.SetSalt(salt);
        salt[0] = 99;

        key.Salt.ToArray().Should().Equal(1, 2, 3);
    }

    [TestMethod]
    public void SetSalt_ReplacingExistingSalt_ShouldZeroPreviousSaltBuffer()
    {
        using var key = new CryptKey(4);
        key.SetSalt([1, 2, 3]);
        var oldBuffer = GetSaltBuffer(key);

        key.SetSalt([4, 5]);

        oldBuffer.Should().AllBeEquivalentTo((byte)0);
        key.Salt.ToArray().Should().Equal(4, 5);
    }

    [TestMethod]
    public void ClearKey_ShouldZeroKeyBufferAndLeaveSalt()
    {
        using var key = new CryptKey(4);
        key.SetKey([1, 2, 3, 4]);
        key.SetSalt([5, 6]);
        var oldKeyBuffer = GetKeyBuffer(key);

        key.ClearKey();

        oldKeyBuffer.Should().AllBeEquivalentTo((byte)0);
        key.Salt.ToArray().Should().Equal(5, 6);
        FluentActions.Invoking(() => key.Key.ToArray())
            .Should().Throw<CryptographicException>()
            .WithMessage("Key not set");
    }

    [TestMethod]
    public void ClearSalt_ShouldZeroSaltBufferAndLeaveKey()
    {
        using var key = new CryptKey(4);
        key.SetKey([1, 2, 3, 4]);
        key.SetSalt([5, 6]);
        var oldSaltBuffer = GetSaltBuffer(key);

        key.ClearSalt();

        oldSaltBuffer.Should().AllBeEquivalentTo((byte)0);
        key.Key.ToArray().Should().Equal(1, 2, 3, 4);
        FluentActions.Invoking(() => key.Salt.ToArray())
            .Should().Throw<CryptographicException>()
            .WithMessage("Salt not set");
    }

    [TestMethod]
    public void Clear_ShouldZeroKeyAndSaltBuffers()
    {
        using var key = new CryptKey(4);
        key.SetKey([1, 2, 3, 4]);
        key.SetSalt([5, 6]);
        var oldKeyBuffer = GetKeyBuffer(key);
        var oldSaltBuffer = GetSaltBuffer(key);

        key.Clear();

        oldKeyBuffer.Should().AllBeEquivalentTo((byte)0);
        oldSaltBuffer.Should().AllBeEquivalentTo((byte)0);
        FluentActions.Invoking(() => key.Key.ToArray()).Should().Throw<CryptographicException>();
        FluentActions.Invoking(() => key.Salt.ToArray()).Should().Throw<CryptographicException>();
    }

    [TestMethod]
    public void Dispose_ShouldZeroKeyAndSaltBuffersAndBeIdempotent()
    {
        var key = new CryptKey(4);
        key.SetKey([1, 2, 3, 4]);
        key.SetSalt([5, 6]);
        var oldKeyBuffer = GetKeyBuffer(key);
        var oldSaltBuffer = GetSaltBuffer(key);

        key.Dispose();
        key.Dispose();

        oldKeyBuffer.Should().AllBeEquivalentTo((byte)0);
        oldSaltBuffer.Should().AllBeEquivalentTo((byte)0);
        FluentActions.Invoking(() => key.Key.ToArray()).Should().Throw<CryptographicException>();
        FluentActions.Invoking(() => key.Salt.ToArray()).Should().Throw<CryptographicException>();
    }

    [TestMethod]
    public void GenerateFromSha_WithSha256_ShouldDeriveExpectedTruncatedKey()
    {
        using var key = new CryptKey(16);
        var data = "payload"u8.ToArray();
        var expected = SHA256.HashData(data).Take(16).ToArray();

        key.GenerateFromSha(data, HashMethod.Sha256);

        key.Key.ToArray().Should().Equal(expected);
        FluentActions.Invoking(() => key.Salt.ToArray())
            .Should().Throw<CryptographicException>();
    }

    [TestMethod]
    public void GenerateFromSha_WithSha512_ShouldSupportLongerKeys()
    {
        using var key = new CryptKey(64);
        var data = "payload"u8.ToArray();
        var expected = SHA512.HashData(data);

        key.GenerateFromSha(data, HashMethod.Sha512);

        key.Key.ToArray().Should().Equal(expected);
    }

    [TestMethod]
    public void GenerateFromSha_StringOverload_ShouldUseConfiguredEncoding()
    {
        using var key = new CryptKey(16)
        {
            Encoding = Encoding.Unicode
        };
        var expected = SHA256.HashData(Encoding.Unicode.GetBytes("åäö")).Take(16).ToArray();

        key.GenerateFromSha("åäö");

        key.Key.ToArray().Should().Equal(expected);
    }

    [TestMethod]
    public void GenerateFromSha_WhenHashIsShorterThanKey_ShouldThrow()
    {
        using var key = new CryptKey(33);

        var act = () => key.GenerateFromSha("payload", HashMethod.Sha256);

        act.Should().Throw<CryptographicException>()
            .WithMessage("Wrong key length 256 bits, expected 264 bits");
    }

    [TestMethod]
    public void GenerateFromSha_WithInvalidHashMethod_ShouldThrow()
    {
        using var key = new CryptKey(16);

        var act = () => key.GenerateFromSha("payload", (HashMethod)999);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("algorithm");
    }

    [TestMethod]
    public void GeneratePbkdf2_WithBytes_ShouldStoreSaltAndExpectedDerivedKey()
    {
        using var key = new CryptKey(24);
        var data = "password"u8.ToArray();

        key.GeneratePbkdf2(data, iterations: 7, saltSize: 12, hashMethod: HashMethod.Sha256);

        var expected = Rfc2898DeriveBytes.Pbkdf2(data, key.Salt.ToArray(), 7, HashAlgorithmName.SHA256, 24);
        key.Salt.Should().HaveCount(12);
        key.Key.ToArray().Should().Equal(expected);
    }

    [TestMethod]
    public void GeneratePbkdf2_StringOverload_ShouldUseConfiguredEncoding()
    {
        using var key = new CryptKey(32)
        {
            Encoding = Encoding.Unicode
        };

        key.GeneratePbkdf2("åäö", iterations: 5, saltSize: 8, hashMethod: HashMethod.Sha512);

        var expected = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.Unicode.GetBytes("åäö"),
            key.Salt.ToArray(),
            5,
            HashAlgorithmName.SHA512,
            32);
        key.Salt.Should().HaveCount(8);
        key.Key.ToArray().Should().Equal(expected);
    }

    [TestMethod]
    public void GeneratePbkdf2_CalledTwice_ShouldUseNewSaltAndClearPreviousBuffers()
    {
        using var key = new CryptKey(16);
        key.GeneratePbkdf2("password", iterations: 3, saltSize: 8);
        var firstKey = key.Key.ToArray();
        var firstSalt = key.Salt.ToArray();
        var oldKeyBuffer = GetKeyBuffer(key);
        var oldSaltBuffer = GetSaltBuffer(key);

        key.GeneratePbkdf2("password", iterations: 3, saltSize: 8);

        oldKeyBuffer.Should().AllBeEquivalentTo((byte)0);
        oldSaltBuffer.Should().AllBeEquivalentTo((byte)0);
        key.Salt.ToArray().Should().NotEqual(firstSalt);
        key.Key.ToArray().Should().NotEqual(firstKey);
    }

    [TestMethod]
    public void GeneratePbkdf2_WithInvalidHashMethod_ShouldThrow()
    {
        using var key = new CryptKey(16);

        var act = () => key.GeneratePbkdf2("password", hashMethod: (HashMethod)999);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("algorithm");
    }

    [TestMethod]
    public void DeriveHmacKey_WithoutKey_ShouldThrow()
    {
        using var key = new CryptKey(16);

        var act = () => key.DeriveHmacKey("context");

        act.Should().Throw<CryptographicException>()
            .WithMessage("Unable to derive message key: Key not set");
    }

    [TestMethod]
    public void DeriveHmacKey_WithBytes_ShouldProduceExpectedTruncatedSha256Key()
    {
        using var key = new CryptKey(16);
        var masterKey = Enumerable.Range(1, 16).Select(x => (byte)x).ToArray();
        var context = "message-1"u8.ToArray();
        key.SetKey(masterKey);

        using var derived = key.DeriveHmacKey(context, HashMethod.Sha256);

        using var hmac = new HMACSHA256(masterKey);
        derived.KeyLength.Should().Be(16);
        derived.Key.ToArray().Should().Equal(hmac.ComputeHash(context).Take(16));
        key.Key.ToArray().Should().Equal(masterKey);
    }

    [TestMethod]
    public void DeriveHmacKey_WithSameContext_ShouldBeDeterministic()
    {
        using var key = new CryptKey(32);
        key.SetKey(Enumerable.Range(1, 32).Select(x => (byte)x).ToArray());

        using var derived1 = key.DeriveHmacKey("same-context", HashMethod.Sha512);
        using var derived2 = key.DeriveHmacKey("same-context", HashMethod.Sha512);

        derived1.Key.ToArray().Should().Equal(derived2.Key.ToArray());
    }

    [TestMethod]
    public void DeriveHmacKey_WithDifferentContexts_ShouldProduceDifferentKeys()
    {
        using var key = new CryptKey(32);
        key.SetKey(Enumerable.Range(1, 32).Select(x => (byte)x).ToArray());

        using var derived1 = key.DeriveHmacKey("context-1");
        using var derived2 = key.DeriveHmacKey("context-2");

        derived1.Key.ToArray().Should().NotEqual(derived2.Key.ToArray());
    }

    [TestMethod]
    public void DeriveHmacKey_StringOverload_ShouldUseConfiguredEncoding()
    {
        using var key = new CryptKey(16)
        {
            Encoding = Encoding.Unicode
        };
        var masterKey = Enumerable.Range(1, 16).Select(x => (byte)x).ToArray();
        key.SetKey(masterKey);

        using var derived = key.DeriveHmacKey("åäö", HashMethod.Sha256);

        using var hmac = new HMACSHA256(masterKey);
        var expected = hmac.ComputeHash(Encoding.Unicode.GetBytes("åäö")).Take(16).ToArray();
        derived.Key.ToArray().Should().Equal(expected);
    }

    [TestMethod]
    public void DeriveHmacKey_WithInvalidHashMethod_ShouldThrow()
    {
        using var key = new CryptKey(16);
        key.SetKey(Enumerable.Range(1, 16).Select(x => (byte)x).ToArray());

        var act = () => key.DeriveHmacKey("context", (HashMethod)999);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("algorithm");
    }

    private static byte[] GetKeyBuffer(CryptKey key)
    {
        var field = typeof(CryptKey).GetField("_keyBuffer", BindingFlags.Instance | BindingFlags.NonPublic);
        field.Should().NotBeNull();
        var buffer = (byte[]?)field!.GetValue(key);
        buffer.Should().NotBeNull();
        return buffer!;
    }

    private static byte[] GetSaltBuffer(CryptKey key)
    {
        var field = typeof(CryptKey).GetField("_saltBuffer", BindingFlags.Instance | BindingFlags.NonPublic);
        field.Should().NotBeNull();
        var buffer = (byte[]?)field!.GetValue(key);
        buffer.Should().NotBeNull();
        return buffer!;
    }
}
