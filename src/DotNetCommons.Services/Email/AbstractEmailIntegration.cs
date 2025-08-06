using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace DotNetCommons.Services.Email;

public abstract class AbstractEmailIntegration
{
    protected readonly IntegrationConfiguration Configuration;

    protected AbstractEmailIntegration(IOptions<IntegrationConfiguration> configuration)
    {
        Configuration = configuration.Value;
    }

    protected MailMessageResult PreprocessMessage(MailMessage message)
    {
        var result = new MailMessageResult(message);

        if (message.From == null || message.To.IsEmpty())
        {
            result.Result = Result.MissingProperties;
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
}