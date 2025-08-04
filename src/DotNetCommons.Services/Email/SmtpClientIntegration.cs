using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace DotNetCommons.Services.Email;

public class SmtpClientIntegration : IEmailIntegration, IDisposable
{
    private readonly IntegrationConfiguration _configuration;
    private readonly SmtpClient _smtpClient;

    public SmtpClientIntegration(IOptions<IntegrationConfiguration> configuration)
    {
        _configuration = configuration.Value;
        _configuration.Require(c => c.EmailConfiguration.Smtp?.Host, this);

        var smtpConfig = configuration.Value.EmailConfiguration.Smtp!;

        _smtpClient = new SmtpClient(
            smtpConfig.Host,
            smtpConfig.Port ?? (smtpConfig.UseTls ? 587 : 25)
        );

        if (smtpConfig.Username.IsSet() || smtpConfig.Password.IsSet())
            _smtpClient.Credentials = new NetworkCredential(smtpConfig.Username, smtpConfig.Password);
    }

    public void Dispose()
    {
        _smtpClient.Dispose();
    }

    public async Task<List<MailMessageResult>> SendAsync(List<MailMessage> messages, CancellationToken cancellationToken = default)
    {
        var results = messages.Select(m => new MailMessageResult(m)).ToList();
        foreach (var item in results)
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

        if (_configuration.EmailConfiguration.RecipientOverride.IsSet())
        {
            var to = message.To.FirstOrDefault()
                        ?? message.CC.FirstOrDefault()
                        ?? message.Bcc.FirstOrDefault();

            message.To.Clear();
            message.CC.Clear();
            message.Bcc.Clear();
            message.To.Add(new MailAddress(_configuration.EmailConfiguration.RecipientOverride));
            message.Subject =  $"{to}: {message.Subject}";
        }
        else
        {
            var domains = message.To
                .Concat(message.CC)
                .Concat(message.Bcc)
                .Select(x => x.Host.ToLowerInvariant())
                .Distinct()
                .ToArray();

            if (domains.Any(d => !_configuration.EmailConfiguration.IsAllowedDomain(d)))
            {
                item.Result = Result.RecipientDomainNotAllowed;
                return;
            }
        }

        try
        {
            await _smtpClient.SendMailAsync(message, cancellationToken);
            item.Result    = Result.Success;
            item.Completed = DateTime.UtcNow;
        }
        catch (OperationCanceledException ex)
        {
            item.Result    = Result.Cancelled;
            item.Exception = ex;
        }
        catch (SmtpFailedRecipientsException ex)
        {
            item.Result    = Result.RecipientNotFound;
            item.Exception = ex;
        }
        catch (SmtpException ex)
            when (ex.StatusCode is SmtpStatusCode.MailboxBusy or SmtpStatusCode.MailboxUnavailable or SmtpStatusCode.TransactionFailed)
        {
            item.Result    = Result.RetriableFailure;
            item.Exception = ex;
        }
        catch (Exception ex)
        {
            item.Result    = Result.HardFailure;
            item.Exception = ex;
        }
    }
}