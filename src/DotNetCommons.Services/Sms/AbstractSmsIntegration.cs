using DotNetCommons.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotNetCommons.Services.Sms;

public abstract class AbstractSmsIntegration
{
    protected readonly ILogger? Logger;
    protected readonly IntegrationConfiguration Configuration;

    protected AbstractSmsIntegration(IOptions<IntegrationConfiguration> configuration, ILogger? logger)
    {
        Configuration = configuration.Value;
    }

    public string? FormatPhoneNumber(string? phoneNumber, string? defaultNumber = null)
    {
        return WhiteWash.PhoneNumberToItuNumber(phoneNumber.NullIfEmpty() ?? defaultNumber,
            Configuration.SmsConfiguration.DefaultCountryCode);
    }

    protected SmsMessageResult PreprocessMessage(SmsMessage message)
    {
        var result = new SmsMessageResult(message);

        message.From      = FormatPhoneNumber(message.From, Configuration.SmsConfiguration.SenderNumber);
        message.FromType  ??= Configuration.SmsConfiguration.SenderType;
        message.Recipient = Configuration.SmsConfiguration.RecipientOverride.NullIfEmpty() ?? FormatPhoneNumber(message.Recipient);

        if (message.From.IsEmpty())
            return Fail(Result.MissingFromNumber);

        if (message.Recipient.IsEmpty())
            return Fail(Result.MissingRecipientNumber);

        if (message.Content.IsEmpty())
            return Fail(Result.MissingContent);

        if (!Configuration.SmsConfiguration.IsAllowedNumber(message.Recipient))
            return Fail(Result.RecipientNumberNotAllowed);

        return result;

        SmsMessageResult Fail(Result value)
        {
            result.Result = value;
            return result;
        }
    }
}