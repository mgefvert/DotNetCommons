using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotNetCommons.Services.Email;

/// <summary>
/// Provides an email integration implementation using an SMTP client, enabling the dispatching
/// of email messages via SMTP servers. This class supports configuration of SMTP server details,
/// credentials, and optional TLS encryption.
/// </summary>
public class SmtpClientIntegration : AbstractEmailIntegration, IEmailIntegration, IDisposable
{
    private readonly SmtpClient _smtpClient;

    public SmtpClientIntegration(IOptions<IntegrationConfiguration> configuration, ILogger<SmtpClientIntegration> logger)
        : base(configuration, logger)
    {
        Configuration.Require(c => c.EmailConfiguration.Smtp?.Host, "EmailConfiguration.Smtp.Host");

        var smtpConfig = configuration.Value.EmailConfiguration.Smtp!;

        _smtpClient = new SmtpClient(
            smtpConfig.Host,
            smtpConfig.Port ?? (smtpConfig.UseTls ? 587 : 25)
        );
        _smtpClient.EnableSsl = smtpConfig.UseTls;

        if (smtpConfig.Username.IsSet() || smtpConfig.Password.IsSet())
            _smtpClient.Credentials = new NetworkCredential(smtpConfig.Username, smtpConfig.Password);
    }

    public void Dispose()
    {
        _smtpClient.Dispose();
    }

    public override async Task<List<MailMessageResult>> SendAsync(List<MailMessage> messages,
        string? fromEmailOrKey = null, CancellationToken cancellationToken = default)
    {
        var fromEmail = fromEmailOrKey.IsSet() ? GetEmailFromKey(fromEmailOrKey) : null;
        var results = messages.Select(message => PreprocessMessage(message, fromEmail)).ToList();
        foreach (var item in results.Where(m => m.Result == Result.None))
        {
            if (cancellationToken.IsCancellationRequested)
                item.Result = Result.Cancelled;
            else
                await SendMessage(item, cancellationToken);
        }

        return results;
    }

    private async Task SendMessage(MailMessageResult item, CancellationToken cancellationToken)
    {
        var message = item.MailMessage;

        try
        {
            await _smtpClient.SendMailAsync(message, cancellationToken);
            item.Result    = Result.Success;
            item.Completed = DateTime.UtcNow;
            Logger.LogInformation("Message sent to {Recipient}: {Subject}", message.To, message.Subject);
        }
        catch (OperationCanceledException ex)
        {
            item.Result    = Result.Cancelled;
            item.Exception = ex;
            Logger.LogInformation("Message sending cancelled for {Recipient}: {ExceptionMessage}", message.To, ex.Message);
        }
        catch (SmtpFailedRecipientsException ex)
        {
            item.Result    = Result.RecipientNotFound;
            item.Exception = ex;
            Logger.LogInformation("Message sending failed for {Recipient}: {ExceptionMessage}", message.To, ex.Message);
        }
        catch (SmtpException ex)
            when (ex.StatusCode is SmtpStatusCode.MailboxBusy or SmtpStatusCode.MailboxUnavailable or SmtpStatusCode.TransactionFailed)
        {
            item.Result    = Result.RetriableFailure;
            item.Exception = ex;
            Logger.LogInformation("Message sending failed for {Recipient}, retriable failure: {ExceptionMessage}", message.To, ex.Message);
        }
        catch (Exception ex)
        {
            item.Result    = Result.HardFailure;
            item.Exception = ex;
            Logger.LogInformation("Message sending failed for {Recipient}, hard failure: {ExceptionMessage}", message.To, ex.Message);
        }
    }
}