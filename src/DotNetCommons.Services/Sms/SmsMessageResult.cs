namespace DotNetCommons.Services.Sms;

public enum Result
{
    None,
    Success,
    Cancelled,
    MissingProperties,
    RecipientNumberNotAllowed,
    HardFailure,
    RetriableFailure
}

/// <summary>
/// Represents the result of an attempted SMS message operation, encapsulating the message,
/// the operation result, completion time, and any associated exception.
/// </summary>
public class SmsMessageResult
{
    /// Represents the original SMS message that was processed by an SMS integration.
    public SmsMessage SmsMessage { get; }

    /// Represents the status or outcome of an SMS message transmission.
    public Result Result { get; set; }

    /// This property specifies the date and time when the email message processing was completed. It may be null
    /// if the processing has not yet finished or was unsuccessful.
    public DateTime? Completed { get; set; }

    /// This property contains the exception that occurred during the processing of the email message, if any.
    public Exception? Exception { get; set; }

    public bool Success => Result == Result.Success;

    public SmsMessageResult(SmsMessage smsMessage)
    {
        SmsMessage = smsMessage;
    }
}