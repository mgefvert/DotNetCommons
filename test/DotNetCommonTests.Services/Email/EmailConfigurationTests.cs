using System.Net.Mail;
using DotNetCommons.Services.Email;
using FluentAssertions;

namespace DotNetCommonTests.Services.Email;

[TestClass]
public class EmailConfigurationTests
{
    private EmailConfiguration _config = null!;

    [TestInitialize]
    public void Setup()
    {
        _config = new EmailConfiguration
        {
            AllowedDomains = ["example.com", "test.com", "company.net"]
        };
    }

    [TestMethod]
    public void IsAllowedDomain_WithAllowedDomain_ReturnsTrue()
    {
        _config.IsAllowedDomain("user@example.com").Should().BeTrue();
        _config.IsAllowedDomain("user@subnet.example.com").Should().BeTrue();
        _config.IsAllowedDomain("user@x4.subnet.example.com").Should().BeTrue();
        _config.IsAllowedDomain("admin@test.com").Should().BeTrue();
        _config.IsAllowedDomain("info@company.net").Should().BeTrue();
    }

    [TestMethod]
    public void IsAllowedDomain_MailAddress_WithAllowedDomain_ReturnsTrue()
    {
        _config.IsAllowedDomain(new MailAddress("user@example.com")).Should().BeTrue();
        _config.IsAllowedDomain(new MailAddress("user@subnet.example.com")).Should().BeTrue();
        _config.IsAllowedDomain(new MailAddress("user@x4.subnet.example.com")).Should().BeTrue();
        _config.IsAllowedDomain(new MailAddress("admin@test.com")).Should().BeTrue();
        _config.IsAllowedDomain(new MailAddress("info@company.net")).Should().BeTrue();
    }

    [TestMethod]
    public void IsAllowedDomain_WithDisallowedDomain_ReturnsFalse()
    {
        _config.IsAllowedDomain("user@unknown.com").Should().BeFalse();
        _config.IsAllowedDomain("admin@other.net").Should().BeFalse();
        _config.IsAllowedDomain("admin@net").Should().BeFalse();
    }

    [TestMethod]
    public void IsAllowedDomain_MailAddress_WithDisallowedDomain_ReturnsFalse()
    {
        _config.IsAllowedDomain(new MailAddress("user@unknown.com")).Should().BeFalse();
        _config.IsAllowedDomain(new MailAddress("admin@other.net")).Should().BeFalse();
        _config.IsAllowedDomain(new MailAddress("admin@net")).Should().BeFalse();
    }

    [TestMethod]
    public void IsAllowedDomain_WithInvalidFormat_ReturnsFalse()
    {
        _config.IsAllowedDomain("invalid").Should().BeFalse();
        _config.IsAllowedDomain("@domain.com").Should().BeFalse();
        _config.IsAllowedDomain("user@").Should().BeFalse();
        _config.IsAllowedDomain("").Should().BeFalse();
        _config.IsAllowedDomain((string?)null!).Should().BeFalse();
    }
}