using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotNetCommons.Services.Email;

public abstract class AbstractEmailIntegration
{
    protected readonly ILogger Logger;
    protected readonly IntegrationConfiguration Configuration;

    protected AbstractEmailIntegration(IOptions<IntegrationConfiguration> configuration, ILogger logger)
    {
        Logger        = logger;
        Configuration = configuration.Value;
    }

    public string GetEmailFromKey(string key)
    {
        return Configuration.EmailConfiguration.FromAddresses.GetValueOrDefault(key)
               ?? throw new InvalidOperationException($"No email address defined for key '{key}'");
    }

    protected virtual MailMessageResult PreprocessMessage(MailMessage message, string? fromEmail)
    {
        var result = new MailMessageResult(message);

        if (message.From == null && fromEmail.IsSet())
            message.From = new MailAddress(fromEmail);

        if (message.From == null)
        {
            result.Result = Result.MissingFromAddress;
            Logger.LogInformation("Message preprocessing failed: Missing From address; subject={Subject}", message.Subject);
            return result;
        }

        if (message.To.IsEmpty())
        {
            result.Result = Result.MissingToAddress;
            Logger.LogInformation("Message preprocessing failed: Missing To address; subject={Subject}", message.Subject);
            return result;
        }

        if (Configuration.EmailConfiguration.RecipientOverride.IsSet())
        {
            var to = message.To.FirstOrDefault()
                     ?? message.CC.FirstOrDefault()
                     ?? message.Bcc.FirstOrDefault();

            message.To.Clear();
            message.CC.Clear();
            message.Bcc.Clear();
            message.To.Add(new MailAddress(Configuration.EmailConfiguration.RecipientOverride));
            message.Subject =  $"{to}: {message.Subject}";
        }
        else if (message.To.Any(to => !Configuration.EmailConfiguration.IsAllowedDomain(to)) ||
                 message.CC.Any(to => !Configuration.EmailConfiguration.IsAllowedDomain(to)) ||
                 message.Bcc.Any(to => !Configuration.EmailConfiguration.IsAllowedDomain(to)))
        {
            result.Result = Result.RecipientDomainNotAllowed;
        }

        return result;
    }

    public abstract Task<List<MailMessageResult>> SendAsync(List<MailMessage> messages,
        string? fromEmailOrKey = null, CancellationToken cancellationToken = default);

    public virtual async Task<MailMessageResult> SendToAdminAsync(MailMessage message, string? fromEmailOrKey = null,
        CancellationToken cancellationToken = default)
    {
        var adminEmails = Configuration.EmailConfiguration.AdminEmails;
        foreach (var email in adminEmails)
            message.To.Add(new MailAddress(email));

        return (await SendAsync([message], fromEmailOrKey, cancellationToken)).First();
    }
}