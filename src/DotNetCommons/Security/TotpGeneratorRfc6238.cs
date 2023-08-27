using System;
using System.Security.Cryptography;
using System.Text;

namespace DotNetCommons.Security;

public class TotpGeneratorRfc6238
{
    private readonly int _digits;
    private readonly string _algorithm;
    private readonly int _timeStep;
    private readonly byte[] _secret;
    
    public TotpGeneratorRfc6238(string secret, int digits, string algorithm = "hmacsha256", int timeStep = 30)
    {
        if (secret.IsEmpty())
            throw new Exception("Rfc6238Totp: Shared secret is empty");
        
        _digits = digits;
        _algorithm = algorithm;
        _timeStep = timeStep;
        _secret = Encoding.ASCII.GetBytes(secret);
    }

    public string GeneratePassword()
    {
        return GeneratePassword(DateTime.UtcNow);
    }
    
    public string GeneratePassword(DateTime time)
    {
        long unixTime = time.ToUnixSeconds() / _timeStep;
        var counterBytes = BitConverter.GetBytes(unixTime);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(counterBytes);

        var hmac = HMAC.Create(_algorithm) ?? throw new Exception("Unable to create HMAC hash");
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
}