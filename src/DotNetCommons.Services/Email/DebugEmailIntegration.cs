using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace DotNetCommons.Services.Email;

/// <summary>
/// The DebugIntegration class provides a mechanism to simulate email sending
/// for development and debugging purposes. It implements the IEmailIntegration
/// interface and captures sent emails in memory without actually dispatching them.
/// </summary>
public class DebugEmailIntegration : AbstractEmailIntegration, IEmailIntegration
{
    public List<MailMessageResult> Messages { get; } = new();

    public DebugEmailIntegration(IOptions<IntegrationConfiguration> configuration) : base(configuration)
    {
    }

    public override Task<List<MailMessageResult>> SendAsync(List<MailMessage> messages,
        string? fromEmailOrKey = null, CancellationToken cancellationToken = default)
    {
        var fromEmail = fromEmailOrKey.IsSet() ? GetEmailFromKey(fromEmailOrKey) : null;
        var result    = messages.Select(m => SendMessage(m, fromEmail)).ToList();

        Messages.AddRange(result);
        return Task.FromResult(result);
    }

    private MailMessageResult SendMessage(MailMessage message, string? fromEmail)
    {
        var result = PreprocessMessage(message, fromEmail);
        if (result.Result != Result.None)
            return result;

        result.Completed = DateTime.UtcNow;
        result.Result    = Result.Success;

        return result;
    }
}