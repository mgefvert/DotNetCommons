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

    public string? FormatPhoneNumber(string? phoneNumber, string? defaultNumber = null)
    {
        return WhiteWash.PhoneNumberToItuNumber(phoneNumber ?? defaultNumber,
            Configuration.SmsConfiguration.DefaultCountryCode);
    }

    protected SmsMessageResult PreprocessMessage(SmsMessage message)
    {
        var result = new SmsMessageResult(message);

        message.From      = FormatPhoneNumber(message.From, Configuration.SmsConfiguration.SenderNumber);
        message.FromType  ??= Configuration.SmsConfiguration.SenderType;
        message.Recipient = Configuration.SmsConfiguration.RecipientOverride ?? FormatPhoneNumber(message.Recipient);
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