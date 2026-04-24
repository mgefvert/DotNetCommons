using System.Net.Mail;

namespace DotNetCommons.Services.Email;

/// Defines the contract for implementing email integration services. Provides methods to send a collection
/// of email messages asynchronously. Implementations may provide various mechanisms for handling the email
/// dispatch process, such as using an SMTP client or in-memory storage for debugging purposes.
public interface IEmailIntegration
{
    string GetEmailFromKey(string key);

    /// <summary>
    /// Sends a collection of email messages asynchronously.
    /// </summary>
    /// <param name="messages">A list of <see cref="MailMessage"/> objects to be sent.</param>
    /// <param name="fromEmailOrKey">Default from email address if none specified, or the key to look up the From: email address in the
    /// EmailConfiguration.FromAddresses dictionary.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    Task<List<MailMessageResult>> SendAsync(List<MailMessage> messages, string? fromEmailOrKey = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an email message to the administrator(s) asynchronously.
    /// </summary>
    /// <param name="message">The <see cref="MailMessage"/> object representing the email to be sent to the administrator.</param>
    /// <param name="fromEmailOrKey">Default from email address if none specified, or the key to look up the From: email address in the
    /// EmailConfiguration.FromAddresses dictionary.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    Task<MailMessageResult> SendToAdminAsync(MailMessage message, string? fromEmailOrKey = null,
        CancellationToken cancellationToken = default);
}