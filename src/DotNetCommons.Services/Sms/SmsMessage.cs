namespace DotNetCommons.Services.Sms;

public class SmsMessage
{
    public string? From { get; set; }
    public string? FromType { get; set; }
    public string? Recipient { get; set; }
    public string? Content { get; set; }
    public TimeSpan? Validity { get; set; }
}