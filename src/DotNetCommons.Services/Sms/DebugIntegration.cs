using Microsoft.Extensions.Options;

namespace DotNetCommons.Services.Sms;

/// <summary>
/// DebugIntegration is an implementation of the ISmsIntegration interface, intended for
/// development and testing purposes, allowing for debugging and verification of SMS messages.
/// This integration does not send actual SMS messages but simulates the behavior in a controlled environment.
/// </summary>
public class DebugIntegration : AbstractSmsIntegration, ISmsIntegration
{
    public List<SmsMessageResult> Messages { get; } = new();

    public DebugIntegration(IOptions<IntegrationConfiguration> configuration) : base(configuration)
    {
    }

    public Task<List<SmsMessageResult>> SendAsync(List<SmsMessage> messages, CancellationToken cancellationToken = default)
    {
        var result = messages.Select(SendMessage).ToList();

        Messages.AddRange(result);
        return Task.FromResult(result);
    }

    private SmsMessageResult SendMessage(SmsMessage message)
    {
        var result = PreprocessMessage(message);
        if (result.Result != Result.None)
            return result;

        result.Completed = DateTime.UtcNow;
        result.Result    = Result.Success;

        return result;
    }
}