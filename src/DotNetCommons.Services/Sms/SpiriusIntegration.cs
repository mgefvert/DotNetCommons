using DotNetCommons.Security;
using Microsoft.Extensions.Options;

namespace DotNetCommons.Services.Sms;

/// <summary>
/// Represents the Spirius integration for sending SMS messages. Implements the <see cref="ISmsIntegration"/> interface to provide
/// functionality for sending messages via the Spirius SMS gateway.
/// </summary>
public class SpiriusIntegration : ISmsIntegration
{
    private readonly HttpClient _httpClient;
    private readonly IntegrationConfiguration _configuration;
    private readonly SmsConfiguration _smsConfig;

    /// Gets or sets the base URL of the Spirius SMS gateway API used for sending messages.
    public Uri ApiUrl { get; set; } = new Uri("https://get.spiricom.spirius.com:55001/cgi-bin/sendsms");

    /// Represents a constant test phone number used for configuring or testing SMS functionalities within the Spirius SMS integration.
    /// This will never be processed by the gateway and will never result in billing charges.
    public const string TestNumber = "+46123456789";

    /// National numbers, max length = 15 characters
    public const string FromTypeNational = "N";

    /// International numbers, max length = 15 characters
    public const string FromTypeInternational = "I";

    /// Short codes, max length = 6 characters
    public const string FromTypeShort = "S";

    /// Alphanumeric data, max length = 11 characters
    public const string FromTypeAlphanumeric = "A";

    public SpiriusIntegration(IOptions<IntegrationConfiguration> configuration, HttpClient httpClient)
    {
        _configuration = configuration.Value;
        _configuration.Require(c => c.SmsConfiguration.DefaultCountryCode, "SmsConfiguration.DefaultCountryCode");
        _configuration.Require(c => c.SmsConfiguration.Username, "SmsConfiguration.Username");
        _configuration.Require(c => c.SmsConfiguration.Password, "SmsConfiguration.Password");
        _configuration.Require(c => c.SmsConfiguration.SenderNumber, "SmsConfiguration.SenderNumber");
        _configuration.Require(c => c.SmsConfiguration.SenderType, "SmsConfiguration.SenderType");

        _httpClient = httpClient;
        _smsConfig  = _configuration.SmsConfiguration;
    }

    public async Task<List<SmsMessageResult>> SendAsync(List<SmsMessage> messages, CancellationToken cancellationToken = default)
    {
        var results = messages.Select(m => new SmsMessageResult(m)).ToList();
        foreach (var item in results)
        {
            if (cancellationToken.IsCancellationRequested)
                item.Result = Result.Cancelled;
            else
                await SendMessage(item, cancellationToken);
        }

        return results;
    }

    private async Task SendMessage(SmsMessageResult item, CancellationToken cancellationToken)
    {
        var sms = item.SmsMessage;

        sms.Recipient = WhiteWash.PhoneNumberToItuNumber(sms.Recipient, _smsConfig.DefaultCountryCode);
        sms.From      = WhiteWash.PhoneNumberToItuNumber(sms.From ?? _smsConfig.SenderNumber, _smsConfig.DefaultCountryCode);
        sms.FromType  ??= _smsConfig.SenderType;

        if (_smsConfig.RecipientOverride.IsSet())
        {
            sms.Recipient = _smsConfig.RecipientOverride;
        }
        else
        {
            if (!_smsConfig.IsAllowedNumber(sms.Recipient))
            {
                item.Result = Result.RecipientNumberNotAllowed;
                return;
            }
        }

        try
        {
            var parameters = new Dictionary<string, string?>
            {
                ["User"]     = _smsConfig.Username!,
                ["Pass"]     = _smsConfig.Password!,
                ["To"]       = sms.Recipient,
                ["From"]     = sms.From,
                ["FromType"] = sms.FromType,
                ["Msg"]      = sms.Content,
                ["Concat"]   = "1",
                ["CharSet"]  = "UTF8"
            };

            if (sms.Validity != null)
                parameters["VP"] = ((int)Math.Round(sms.Validity.Value.TotalMinutes, MidpointRounding.AwayFromZero)).ToString();

            var uri = ApiUrl.WithQuery(parameters);

            for (var i = 0; i < 10; i++)
            {
                var result = await _httpClient.GetAsync(uri, cancellationToken);

                int? status      = (int)result.StatusCode;
                var  statusgroup = (int)(status / 100);

                if (status == 409)
                {
                    // Rate limit exceeded
                    await Task.Delay(1000, cancellationToken);
                }
                else if (statusgroup == 2)
                {
                    item.Result = Result.Success;
                    item.Completed = DateTime.UtcNow;
                    return;
                }
                else
                {
                    item.Result = Result.HardFailure;
                    return;
                }
            }
        }
        catch (OperationCanceledException ex)
        {
            item.Result    = Result.Cancelled;
            item.Exception = ex;
        }
        catch (Exception ex)
        {
            item.Result    = Result.HardFailure;
            item.Exception = ex;
        }
    }
}