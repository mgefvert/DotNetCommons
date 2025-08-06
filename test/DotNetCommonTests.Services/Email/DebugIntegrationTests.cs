using System.Net.Mail;
using DotNetCommons.Services;
using DotNetCommons.Services.Email;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace DotNetCommonTests.Services.Email;

[TestClass]
public class DebugIntegrationTests
{
    private DebugIntegration _integration = null!;
    private IntegrationConfiguration _config = null!;

    [TestInitialize]
    public void Setup()
    {
        _config = new IntegrationConfiguration
        {
            EmailConfiguration = new EmailConfiguration
            {
                AllowedDomains = ["example.com", "test.com"],
                FromAddresses = new Dictionary<string, string>
                {
                    [EmailConfiguration.NoReplyEmail] = "noreply@example.com"
                }
            }
        };

        _integration = new DebugIntegration(Options.Create(_config));
    }

    [TestMethod]
    public async Task SendAsync_WithAllowedDomain_Works()
    {
        var message = new MailMessage
        {
            From = new MailAddress("noreply@example.com"),
            To = { new MailAddress("user@example.com") },
            Subject = "Test Email",
            Body = "Hello World!"
        };

        var result = await _integration.SendAsync([message]);
        result.Count.Should().Be(1);
        result[0].Result.Should().Be(Result.Success);
        result[0].MailMessage.Should().Be(message);
        result[0].Completed.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        result[0].Exception.Should().BeNull();

        _integration.Messages.Should().BeEquivalentTo(result);
    }

    [TestMethod]
    public async Task SendAsync_WithRecipientOverride_Works()
    {
        _config.EmailConfiguration.RecipientOverride = "override@test.com";

        var message = new MailMessage
        {
            From = new MailAddress("noreply@example.com"),
            To = { new MailAddress("user@example.com") },
            Subject = "Test Email",
            Body = "Hello World!"
        };

        var result = await _integration.SendAsync([message]);
        result[0].MailMessage.To.Should().BeEquivalentTo([new MailAddress("override@test.com")]);
    }

    [TestMethod]
    public async Task SendAsync_WithDisallowedDomain_ReturnsFalse()
    {
        var message = new MailMessage
        {
            From = new MailAddress("noreply@example.com"),
            To = { new MailAddress("user@unknown.com") },
            Subject = "Test Email",
            Body = "Hello World!"
        };

        var result = await _integration.SendAsync([message]);
        result[0].Result.Should().Be(Result.RecipientDomainNotAllowed);
        result[0].MailMessage.Should().Be(message);
        result[0].Completed.Should().BeNull();
        result[0].Exception.Should().BeNull();
    }

    [TestMethod]
    public async Task SendAsync_MissingProperties()
    {
        var message = new MailMessage
        {
            Subject = "Test Email",
            Body = "Hello World!"
        };

        var result = await _integration.SendAsync([message]);
        result[0].Result.Should().Be(Result.MissingProperties);
        result[0].MailMessage.Should().Be(message);
        result[0].Completed.Should().BeNull();
        result[0].Exception.Should().BeNull();
    }
}