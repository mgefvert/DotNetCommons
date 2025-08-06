using DotNetCommons.Security;
using Microsoft.Extensions.Options;

namespace DotNetCommons.Services.Sms;

public abstract class AbstractSmsIntegration
{
    protected readonly IntegrationConfiguration Configuration;

    protected AbstractSmsIntegration(IOptions<IntegrationConfiguration> configuration)
    {
        Configuration = configuration.Value;
    }

    protected SmsMessageResult PreprocessMessage(SmsMessage message)
    {
        string? ProcessNumber(string? number, string? defaultNumber) =>
            WhiteWash.PhoneNumberToItuNumber(number ?? defaultNumber, Configuration.SmsConfiguration.DefaultCountryCode);

        var result = new SmsMessageResult(message);

        message.From      = ProcessNumber(message.From, Configuration.SmsConfiguration.SenderNumber);
        message.FromType  ??= Configuration.SmsConfiguration.SenderType;
        message.Recipient = Configuration.SmsConfiguration.RecipientOverride ?? ProcessNumber(message.Recipient, null);
        if (message.From.IsEmpty() || message.Recipient.IsEmpty() || message.Content.IsEmpty())
        {
            result.Result = Result.MissingProperties;
            return result;
        }

        if (!Configuration.SmsConfiguration.IsAllowedNumber(message.Recipient))
            result.Result = Result.RecipientNumberNotAllowed;

        return result;
    }
}