using System.Net.Mail;

namespace DotNetCommons.Services.Email;

/// Defines the contract for implementing email integration services. Provides methods to send a collection
/// of email messages asynchronously. Implementations may provide various mechanisms for handling the email
/// dispatch process, such as using an SMTP client or in-memory storage for debugging purposes.
public interface IEmailIntegration
{
    /// <summary>
    /// Sends a collection of email messages asynchronously.
    /// </summary>
    /// <param name="messages">A list of <see cref="MailMessage"/> objects to be sent.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a list of
    /// <see cref="MailMessageResult"/> objects, each representing the result of sending a corresponding email.
    /// </returns>
    Task<List<MailMessageResult>> SendAsync(List<MailMessage> messages, CancellationToken cancellationToken = default);
}