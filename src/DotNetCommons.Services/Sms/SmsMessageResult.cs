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

public class SmsMessageResult
{
    public SmsMessage SmsMessage { get; }
    public Result Result { get; set; }
    public DateTime? Completed { get; set; }
    public Exception? Exception { get; set; }

    public SmsMessageResult(SmsMessage smsMessage)
    {
        SmsMessage = smsMessage;
    }
}