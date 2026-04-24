namespace DotNetCommons.Services.Sms;

/// <summary>
/// Represents an SMS message that can be sent using an SMS integration.
/// This class provides properties to define the sender, recipient, content,
/// and additional metadata related to the SMS message.
/// </summary>
public class SmsMessage
{
    /// Gets or sets the sender of the SMS message. If not set the default configuration sender will be used.
    /// This property represents the originator or sender identifier for the SMS. It can be a phone number,
    /// alphanumeric string, or other valid identifier supported by the SMS integration being used. The value
    /// is typically formatted as an ITU-T E.164 number.
    public string? From { get; set; }

    /// Gets or sets the type of the sender. If not set the default configuration sender will be used.
    /// This property indicates the classification or category of the sender, which may represent specific sender types
    /// supported by the SMS integration.
    public string? FromType { get; set; }

    /// Gets or sets the recipient of the SMS message. This property specifies the destination or recipient identifier for the SMS.
    /// It is typically formatted as an ITU-T E.164 number to ensure proper routing and delivery of the message.
    public string? Recipient { get; set; }

    /// Gets or sets the textual content of the SMS message. This property represents the body of the message to be sent to
    /// the recipient. The value should provide the intended message content while adhering to character limits and encoding
    /// supported by the SMS integration being used.
    public string? Content { get; set; }

    /// Gets or sets the validity period of the SMS message. This property specifies the time duration that the SMS remains
    /// valid for delivery attempts. If the message is not delivered within the specified period, it will expire and no further attempts
    /// will be made to send it. The value is typically represented as a TimeSpan duration and converted
    /// into a suitable format (e.g., minutes) by the SMS integration being used.
    public TimeSpan? Validity { get; set; }

    /// Gets or sets the project associated with the SMS message.
    /// This property can be used to group or categorize SMS messages under a specific project or context,
    /// enabling easier identification and organization within the SMS integration system.
    public string? Project { get; set; }
}