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

public class MailMessageResult
{
    public MailMessage MailMessage { get; }
    public Result Result { get; set; }
    public DateTime? Completed { get; set; }
    public Exception? Exception { get; set; }

    public MailMessageResult(MailMessage mailMessage)
    {
        MailMessage = mailMessage;
    }
}