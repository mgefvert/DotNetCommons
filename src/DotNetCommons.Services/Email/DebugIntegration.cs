using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace DotNetCommons.Services.Email;

/// <summary>
/// The DebugIntegration class provides a mechanism to simulate email sending
/// for development and debugging purposes. It implements the IEmailIntegration
/// interface and captures sent emails in memory without actually dispatching them.
/// </summary>
public class DebugIntegration : IEmailIntegration
{
    private readonly IntegrationConfiguration _configuration;

    public List<MailMessageResult> Messages { get; } = new();

    public DebugIntegration(IOptions<IntegrationConfiguration> configuration)
    {
        _configuration = configuration.Value;
    }

    public Task<List<MailMessageResult>> SendAsync(List<MailMessage> messages, CancellationToken cancellationToken = default)
    {
        var result = messages.Select(SendMessage).ToList();

        Messages.AddRange(result);
        return Task.FromResult(result);
    }

    private MailMessageResult SendMessage(MailMessage message)
    {
        var result = new MailMessageResult(message);

        if (message.From == null || message.To.IsEmpty())
        {
            result.Result = Result.MissingProperties;
            return result;
        }

        if (message.To.Any(to => !_configuration.EmailConfiguration.IsAllowedDomain(to)) ||
            message.CC.Any(to => !_configuration.EmailConfiguration.IsAllowedDomain(to)) ||
            message.Bcc.Any(to => !_configuration.EmailConfiguration.IsAllowedDomain(to)))
        {
            result.Result = Result.RecipientDomainNotAllowed;
            return result;
        }

        result.Completed = DateTime.UtcNow;
        result.Result    = Result.Success;

        return result;
    }
}