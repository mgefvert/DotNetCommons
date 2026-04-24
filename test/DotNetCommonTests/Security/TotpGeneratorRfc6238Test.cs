using DotNetCommons.Security;
using FluentAssertions;

namespace DotNetCommonTests.Security;

[TestClass]
public class TotpGeneratorRfc6238Test
{
    [TestMethod]
    public void Generate_HMAC_SHA256()
    {
        var totp = new TotpGeneratorRfc6238("12345678901234567890123456789012", 8);

        totp.GeneratePassword(new DateTime(1970, 1, 1, 0, 0, 59, DateTimeKind.Utc)).Should().Be("46119246");
        totp.GeneratePassword(new DateTime(2005, 3, 18, 01, 58, 29, DateTimeKind.Utc)).Should().Be("68084774");
        totp.GeneratePassword(new DateTime(2603, 10, 11, 11, 33, 20, DateTimeKind.Utc)).Should().Be("77737706");
    }

    [TestMethod]
    public void Generate_HMAC_SHA1()
    {
        var totp = new TotpGeneratorRfc6238("12345678901234567890", 8, TotpGeneratorRfc6238.Algorithm.HmacSha1);

        totp.GeneratePassword(new DateTime(1970, 1, 1, 0, 0, 59, DateTimeKind.Utc)).Should().Be("94287082");
        totp.GeneratePassword(new DateTime(2005, 3, 18, 01, 58, 29, DateTimeKind.Utc)).Should().Be("07081804");
        totp.GeneratePassword(new DateTime(2603, 10, 11, 11, 33, 20, DateTimeKind.Utc)).Should().Be("65353130");
    }

    [TestMethod]
    public void Validate()
    {
        var totp = new TotpGeneratorRfc6238("12345678901234567890", 8);

        var currentPassword  = totp.GeneratePassword();
        var previousPassword = totp.GeneratePassword(DateTime.UtcNow.AddSeconds(-30));
        var nextPassword     = totp.GeneratePassword(DateTime.UtcNow.AddSeconds(30));
        var next2Password    = totp.GeneratePassword(DateTime.UtcNow.AddSeconds(60));

        // Test with drift count 2 (validates current time ±2 intervals)
        totp.ValidatePassword(previousPassword, 2).Should().BeTrue();
        totp.ValidatePassword(currentPassword, 2).Should().BeTrue();
        totp.ValidatePassword(nextPassword, 2).Should().BeTrue();
        totp.ValidatePassword(next2Password, 2).Should().BeTrue();

        // Test with drift count 1 (validates current time ±1 interval)
        totp.ValidatePassword(previousPassword, 1).Should().BeTrue();
        totp.ValidatePassword(currentPassword, 1).Should().BeTrue();
        totp.ValidatePassword(nextPassword, 1).Should().BeTrue();
        totp.ValidatePassword(next2Password, 1).Should().BeFalse();

        // Test with drift count 0 (validates only current time)
        totp.ValidatePassword(previousPassword, 0).Should().BeFalse();
        totp.ValidatePassword(currentPassword, 0).Should().BeTrue();
        totp.ValidatePassword(nextPassword, 0).Should().BeFalse();
        totp.ValidatePassword(next2Password, 0).Should().BeFalse();
    }
}