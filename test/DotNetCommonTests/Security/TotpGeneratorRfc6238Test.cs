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
}