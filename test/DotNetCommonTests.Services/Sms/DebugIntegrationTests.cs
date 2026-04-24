using DotNetCommons.Services;
using DotNetCommons.Services.Sms;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace DotNetCommonTests.Services.Sms;

[TestClass]
public class DebugIntegrationTests
{
    private DebugSmsIntegration _integration = null!;
    private IntegrationConfiguration _config = null!;

    [TestInitialize]
    public void Setup()
    {
        _config = new IntegrationConfiguration
        {
            SmsConfiguration = new SmsConfiguration
            {
                AllowedNumbers = ["+45", "+46", "+47"],
                DefaultCountryCode = "+46",
                SenderNumber = "+4612345678",
                SenderType = SpiriusIntegration.FromTypeNational
            }
        };

        _integration = new DebugSmsIntegration(Options.Create(_config));
    }

    
    [TestMethod]
    public async Task SendAsync_WithAllowedNumber_Works()
    {
        var message = new SmsMessage
        {
            Recipient = "+451234567",
            Content   = "Hello World!",
        };

        var result = await _integration.SendAsync([message]);
        result.Count.Should().Be(1);
        result[0].Result.Should().Be(Result.Success);
        result[0].SmsMessage.Should().Be(message);
        result[0].Completed.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        result[0].Exception.Should().BeNull();

        _integration.Messages.Should().BeEquivalentTo(result);
    }

    [TestMethod]
    public async Task SendAsync_WithDefaultCountryCode_Works()
    {
        var message = new SmsMessage
        {
            Recipient = "031-1234567",
            Content   = "Hello World!",
        };

        var result = await _integration.SendAsync([message]);
        result.Single().SmsMessage.Recipient.Should().Be("+46311234567");
    }

    [TestMethod]
    public async Task SendAsync_WithRecipientOverride_Works()
    {
        _config.SmsConfiguration.RecipientOverride = "+18005551234";

        var message = new SmsMessage
        {
            Recipient = "+451234567",
            Content   = "Hello World!",
        };

        var result = await _integration.SendAsync([message]);
        result.Single().SmsMessage.Recipient.Should().Be("+18005551234");
    }

    [TestMethod]
    public async Task SendAsync_WithDisallowedNumber_ReturnsFalse()
    {
        var message = new SmsMessage
        {
            Recipient = "+3581234567",
            Content   = "Hello World!",
        };

        var result = await _integration.SendAsync([message]);
        result[0].SmsMessage.Should().Be(message);
        result[0].Result.Should().Be(Result.RecipientNumberNotAllowed);
        result[0].Completed.Should().BeNull();
        result[0].Exception.Should().BeNull();
    }

    [TestMethod]
    public async Task SendAsync_MissingProperties()
    {
        var message = new SmsMessage
        {
            Content   = "Hello World!",
        };

        var result = await _integration.SendAsync([message]);
        result[0].SmsMessage.Should().Be(message);
        result[0].Completed.Should().BeNull();
        result[0].Exception.Should().BeNull();
        result[0].Result.Should().Be(Result.MissingProperties);
    }
}