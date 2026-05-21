using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotNetCommons.Services.Sms;

/// <summary>
/// DebugIntegration is an implementation of the ISmsIntegration interface, intended for
/// development and testing purposes, allowing for debugging and verification of SMS messages.
/// This integration does not send actual SMS messages but simulates the behavior in a controlled environment.
/// </summary>
public class DebugSmsIntegration : AbstractSmsIntegration, ISmsIntegration
{
    public List<SmsMessageResult> Messages { get; } = new();

    public DebugSmsIntegration(IOptions<IntegrationConfiguration> configuration, ILogger? logger)
        : base(configuration, logger)
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
        {
            Logger?.LogInformation("SMS preprocessing failed: {Result}", result.Result);
            return result;
        }

        result.Completed = DateTime.UtcNow;
        result.Result    = Result.Success;

        Logger?.LogInformation("SMS sent successfully to {Recipient}", message.Recipient);
        return result;
    }
}