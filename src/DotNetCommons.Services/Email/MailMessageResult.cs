using System.Net.Mail;

namespace DotNetCommons.Services.Email;

public enum Result
{
    None,
    Success,
    Cancelled,
    MissingProperties,
    RecipientDomainNotAllowed,
    HardFailure,
    RetriableFailure,
    RecipientNotFound
}

/// Represents the result of an email message being sent.
public class MailMessageResult
{
    /// This property contains the original MailMessage that was processed.
    public MailMessage MailMessage { get; }

    /// Represents the status or outcome of an email message transmission.
    public Result Result { get; set; }

    /// This property specifies the date and time when the email message processing was completed. It may be null
    /// if the processing has not yet finished or was unsuccessful.
    public DateTime? Completed { get; set; }

    /// This property contains the exception that occurred during the processing of the email message, if any.
    public Exception? Exception { get; set; }

    public bool Success => Result == Result.Success;
    public IEnumerable<MailAddress> AllRecipients =>
        MailMessage.To
            .Concat(MailMessage.CC)
            .Concat(MailMessage.Bcc);

    public MailMessageResult(MailMessage mailMessage)
    {
        MailMessage = mailMessage;
    }
}