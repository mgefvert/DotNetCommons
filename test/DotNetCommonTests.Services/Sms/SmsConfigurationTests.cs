using DotNetCommons.Services.Sms;
using FluentAssertions;

namespace DotNetCommonTests.Services.Sms;

[TestClass]
public class SmsConfigurationTests
{
    private SmsConfiguration _config = null!;

    [TestInitialize]
    public void Setup()
    {
        _config = new SmsConfiguration
        {
            AllowedNumbers     = ["+45", "+46", "+47"],
            DefaultCountryCode = "+46"
        };
    }

    [TestMethod]
    public void IsAllowedNumber_WithAllowedPrefix_ReturnsTrue()
    {
        _config.IsAllowedNumber("+451234567").Should().BeTrue();
        _config.IsAllowedNumber("+462345678").Should().BeTrue();
        _config.IsAllowedNumber("+4798765432").Should().BeTrue();

        _config.IsAllowedNumber("00451234567").Should().BeTrue();
        _config.IsAllowedNumber("00462345678").Should().BeTrue();
        _config.IsAllowedNumber("004798765432").Should().BeTrue();
    }

    [TestMethod]
    public void IsAllowedNumber_WithDefaultCountryCode_ReturnsTrue()
    {
        _config.IsAllowedNumber("0123456789").Should().BeTrue();
        _config.IsAllowedNumber("031-123456").Should().BeTrue();
    }

    [TestMethod]
    public void IsAllowedNumber_WithDisallowedPrefix_ReturnsFalse()
    {
        _config.IsAllowedNumber("+12125551234").Should().BeFalse();
        _config.IsAllowedNumber("+441234567890").Should().BeFalse();

        _config.IsAllowedNumber("0012125551234").Should().BeFalse();
        _config.IsAllowedNumber("00441234567890").Should().BeFalse();
    }

    [TestMethod]
    public void IsAllowedNumber_WithInvalidFormat_ReturnsFalse()
    {
        _config.IsAllowedNumber("invalid").Should().BeFalse();
        _config.IsAllowedNumber("+456").Should().BeFalse();
        _config.IsAllowedNumber("456").Should().BeFalse();
        _config.IsAllowedNumber("00").Should().BeFalse();
        _config.IsAllowedNumber("0").Should().BeFalse();
        _config.IsAllowedNumber("+").Should().BeFalse();
        _config.IsAllowedNumber("").Should().BeFalse();
        _config.IsAllowedNumber(null).Should().BeFalse();
    }
}