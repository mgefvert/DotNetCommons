using System.Security.Cryptography;
using System.Text;

namespace DotNetCommons.Security;

public class TotpGeneratorRfc6238
{
    public enum Algorithm
    {
        HmacSha1,
        HmacSha256,
        HmacSha384,
        HmacSha512
    }
    
    private readonly int _digits;
    private readonly Algorithm _algorithm;
    private readonly int _timeStep;
    private readonly byte[] _secret;

    /// <summary>
    /// Provides functionality to generate a TOTP (Time-based One-Time Password) based on RFC 6238 specification.
    /// </summary>
    /// <remarks>
    /// This implementation follows RFC 6238 (https://tools.ietf.org/html/rfc6238) for generating
    /// Time-based One-Time Passwords, commonly used in two-factor authentication (2FA) systems.
    /// The default parameters are chosen to be compatible with most authenticator apps.
    /// </remarks>
    /// <param name="secret">The shared secret key used to generate the TOTP. This is a plaintext ASCII string. </param>
    /// <param name="digits">The number of digits in the generated TOTP code. Common values are 6-8 digits: </param>
    /// <param name="algorithm"> The HMAC algorithm to use for TOTP generation. HmacSha1 is the original algorithm; HmacSha256 (default)
    ///     is recommended for new implementations. </param>
    /// <param name="timeStep"> The time step in seconds for TOTP generation. Default is 30 seconds.</param>
    public TotpGeneratorRfc6238(string secret, int digits = 6, Algorithm algorithm = Algorithm.HmacSha256, int timeStep = 30)
        : this(Encoding.ASCII.GetBytes(secret), digits, algorithm, timeStep)
    {
    }

    /// <summary>
    /// Provides functionality to generate a TOTP (Time-based One-Time Password) based on RFC 6238 specification.
    /// </summary>
    /// <remarks>
    /// This implementation follows RFC 6238 (https://tools.ietf.org/html/rfc6238) for generating
    /// Time-based One-Time Passwords, commonly used in two-factor authentication (2FA) systems.
    /// The default parameters are chosen to be compatible with most authenticator apps.
    /// </remarks>
    /// <param name="secret">The shared secret key used to generate the TOTP as a binary array.</param>
    /// <param name="digits">The number of digits in the generated TOTP code. Common values are 6-8 digits: </param>
    /// <param name="algorithm"> The HMAC algorithm to use for TOTP generation. HmacSha1 is the original algorithm; HmacSha256 (default)
    ///     is recommended for new implementations. </param>
    /// <param name="timeStep"> The time step in seconds for TOTP generation. Default is 30 seconds.</param>
    public TotpGeneratorRfc6238(byte[] secret, int digits = 6, Algorithm algorithm = Algorithm.HmacSha256, int timeStep = 30)
    {
        if (secret.IsEmpty())
            throw new Exception("Rfc6238Totp: Shared secret is empty");

        _digits    = digits;
        _algorithm = algorithm;
        _timeStep  = timeStep;
        _secret    = secret;
    }

    /// <summary>
    /// Generates a TOTP (Time-based One-Time Password) for the current time using the configured parameters.
    /// </summary>
    /// <returns>A time-based one-time password as a string with the specified number of digits, based on the secret, algorithm,
    /// and time step configuration.
    /// </returns>
    public string GeneratePassword()
    {
        return GeneratePassword(DateTime.UtcNow);
    }

    /// <summary>
    /// Generates a TOTP (Time-based One-Time Password) for the specified time using the configured parameters.
    /// </summary>
    /// <param name="time">The time for which the TOTP is generated, usually in UTC.</param>
    /// <returns>A time-based one-time password as a string with the specified number of digits, based on the secret, algorithm,
    /// and time step configuration.</returns>
    public string GeneratePassword(DateTime time)
    {
        long unixTime     = time.ToUnixSeconds() / _timeStep;
        var  counterBytes = BitConverter.GetBytes(unixTime);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(counterBytes);

        var hmac = CreateHmac(_algorithm);
        hmac.Key = _secret;
        var hash = hmac.ComputeHash(counterBytes);
        
        var offset = hash[^1] & 0x0F;
        var binaryCode = ((hash[offset] & 0x7F) << 24) |
                         ((hash[offset + 1] & 0xFF) << 16) |
                         ((hash[offset + 2] & 0xFF) << 8) |
                         (hash[offset + 3] & 0xFF);
        var totp = binaryCode % (int)Math.Pow(10, _digits);

        return totp.ToString().PadLeft(_digits, '0');
    }

    private static HMAC CreateHmac(Algorithm algorithm)
    {
        return algorithm switch
        {
            Algorithm.HmacSha1   => new HMACSHA1(),
            Algorithm.HmacSha256 => new HMACSHA256(),
            Algorithm.HmacSha384 => new HMACSHA384(),
            Algorithm.HmacSha512 => new HMACSHA512(),
            _                    => throw new ArgumentException("Invalid TOTP algorithm", nameof(algorithm))
        };
    }

    public bool ValidatePassword(string numericPassword, int driftCount = 1)
    {
		if (string.IsNullOrEmpty(numericPassword))
			return false;
		
        var now = DateTime.UtcNow;
        for (var i = -driftCount; i <= driftCount; i++)
        {
            var testTime = now.AddSeconds(_timeStep * i);
            if (GeneratePassword(testTime) == numericPassword)
                return true;
        }

        return false;
    }
}