using DotNetCommons.Security;

namespace DotNetCommons.Services.Sms;

/// <summary>
/// Provides configuration settings for SMS functionality within an application.
/// This class includes settings for allowed numbers, default country code, and sender details.
/// </summary>
public class SmsConfiguration
{
    /// Gets or sets a list of phone number prefixes that are allowed when sending SMS messages. This is used to prevent sending
    /// SMS messages to unintended or expensive destinations. For instance, the list may contain `+45`, `+46`, `+47` which will
    /// only allow messages to be sent within Scandinavia.
    /// If the list is empty, all phone numbers are considered allowed.
    public List<string> AllowedNumbers { get; set; } = [];

    /// Default country code to be applied when a phone number starts with 0 (e.g. 031-1234567). Setting this to `+46` will replace
    /// the 0 and form +46311234567 instead, which is required for SMS delivery.
    public string? DefaultCountryCode { get; set; }

    /// Gets or sets a recipient phone number override. If this property is set, all SMS messages will be sent to this specific
    /// recipient, regardless of their original intended destination. This can be useful for testing or debugging scenarios
    /// where actual message delivery must be limited to a controlled recipient. If null or empty, the original recipient is used.
    public string? RecipientOverride { get; set; }

    /// Gets or sets the type of the sender when sending SMS messages. This property typically defines how the sender is represented
    /// in the message, such as a phone number, short code, or name. The value of this property may vary depending on the SMS provider's
    /// requirements or restrictions.
    public string? SenderType { get; set; }

    /// Gets or sets the default sender phone number used when sending SMS messages, if none provided in the SmsMessage object.
    /// This number represents the origin of the SMS and is often displayed to the recipient as the sender of the message.
    /// This must be in the correct ITU format (start with a plus, then country code, then number without spaces or symbols).
    public string? SenderNumber { get; set; }

    /// Gets or sets the username used for authentication with the SMS integration provider.
    public string? Username { get; set; }

    /// Gets or sets the password used for authentication with the SMS integration provider.
    public string? Password { get; set; }

    /// <summary>
    /// Determines whether the specified phone number is allowed based on configured allowed numbers.
    /// </summary>
    /// <param name="phoneNumber">The phone number to be checked, which can be in any format.</param>
    /// <returns>True if the number is allowed or if no allowed numbers are configured; otherwise, false.</returns>
    public bool IsAllowedNumber(string? phoneNumber)
    {
        var ituNumber = WhiteWash.PhoneNumberToItuNumber(phoneNumber, DefaultCountryCode);

        // The minimum length for ITU E.164 is 8 digits plus the +
        if (ituNumber.IsEmpty() || ituNumber.Length < 9)
            return false;

        return AllowedNumbers.Count == 0 || AllowedNumbers.Any(ituNumber.StartsWith);
    }
}