namespace DotNetCommons.Services.Sms;

public interface ISmsIntegration
{
    Task<List<SmsMessageResult>> SendAsync(List<SmsMessage> messages, CancellationToken cancellationToken = default);
}