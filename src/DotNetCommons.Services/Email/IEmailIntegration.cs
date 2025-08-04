using System.Net.Mail;

namespace DotNetCommons.Services.Email;

public interface IEmailIntegration
{
    Task<List<MailMessageResult>> SendAsync(List<MailMessage> messages, CancellationToken cancellationToken = default);
}