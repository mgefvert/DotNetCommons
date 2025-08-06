using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace DotNetCommons.Services.Email;

/// <summary>
/// The DebugIntegration class provides a mechanism to simulate email sending
/// for development and debugging purposes. It implements the IEmailIntegration
/// interface and captures sent emails in memory without actually dispatching them.
/// </summary>
public class DebugIntegration : AbstractEmailIntegration, IEmailIntegration
{
    public List<MailMessageResult> Messages { get; } = new();

    public DebugIntegration(IOptions<IntegrationConfiguration> configuration) : base(configuration)
    {
    }

    public Task<List<MailMessageResult>> SendAsync(List<MailMessage> messages, CancellationToken cancellationToken = default)
    {
        var result = messages.Select(SendMessage).ToList();

        Messages.AddRange(result);
        return Task.FromResult(result);
    }

    private MailMessageResult SendMessage(MailMessage message)
    {
        var result = PreprocessMessage(message);
        if (result.Result != Result.None)
            return result;

        result.Completed = DateTime.UtcNow;
        result.Result    = Result.Success;

        return result;
    }
}