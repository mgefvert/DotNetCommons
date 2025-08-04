using DotNetCommons.Security;
using Microsoft.Extensions.Options;

namespace DotNetCommons.Services.Sms;

/// <summary>
/// DebugIntegration is an implementation of the ISmsIntegration interface, intended for
/// development and testing purposes, allowing for debugging and verification of SMS messages.
/// This integration does not send actual SMS messages but simulates the behavior in a controlled environment.
/// </summary>
public class DebugIntegration : ISmsIntegration
{
    private readonly IntegrationConfiguration _configuration;

    public List<SmsMessageResult> Messages { get; } = new();

    public DebugIntegration(IOptions<IntegrationConfiguration> configuration)
    {
        _configuration = configuration.Value;
    }

    public Task<List<SmsMessageResult>> SendAsync(List<SmsMessage> messages, CancellationToken cancellationToken = default)
    {
        var result = messages.Select(SendMessage).ToList();

        Messages.AddRange(result);
        return Task.FromResult(result);
    }

    private SmsMessageResult SendMessage(SmsMessage message)
    {
        string? ProcessNumber(string? number, string? defaultNumber) =>
            WhiteWash.PhoneNumberToItuNumber(number ?? defaultNumber, _configuration.SmsConfiguration.DefaultCountryCode);

        var result = new SmsMessageResult(message);

        message.From      = ProcessNumber(message.From, _configuration.SmsConfiguration.SenderNumber);
        message.Recipient = _configuration.SmsConfiguration.RecipientOverride ?? ProcessNumber(message.Recipient, null);
        if (message.From.IsEmpty() || message.Recipient.IsEmpty() || message.Content.IsEmpty())
        {
            result.Result = Result.MissingProperties;
            return result;
        }

        if (!_configuration.SmsConfiguration.IsAllowedNumber(message.Recipient))
        {
            result.Result = Result.RecipientNumberNotAllowed;
            return result;
        }

        result.Completed = DateTime.UtcNow;
        result.Result    = Result.Success;

        return result;
    }
}