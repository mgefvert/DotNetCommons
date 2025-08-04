using System.Net.Mail;

namespace DotNetCommons.Services.Email;

public class EmailConfiguration
{
    public const string AdminEmail = "admin";
    public const string InfoEmail = "info";
    public const string NoReplyEmail = "noreply";
    public const string SupportEmail = "support";

    /// Gets or sets the list of domains that are allowed for processing or validation in the email service.
    /// This property is used to restrict allowed domains when sending or receiving emails, ensuring that
    /// only specified domains are permitted. If the list is empty, all domains are considered allowed.
    public List<string> AllowedDomains { get; set; } = [];

    /// Gets or sets a dictionary that maps identifiers to email addresses used as the sender in outgoing emails.
    /// This property standardizes various "From" email addresses across the system, allowing easier configuration
    /// and consistent usage. The keys should preferably be <see cref="AdminEmail"/>, <see cref="InfoEmail"/>,
    /// etc; and the values are the email addresses to use.
    public Dictionary<string, string> FromAddresses { get; set; } = new();

    /// Gets or sets an email address used to override all recipients when sending emails through the service.
    /// When this property is set to a valid email address, all emails will be delivered to this address instead of
    /// the original recipients.
    public string? RecipientOverride { get; set; }

    /// Gets or sets the configuration for the SMTP settings used for sending emails.
    /// This property contains details such as the SMTP host, port, credentials, and whether to use TLS.
    public SmtpConfiguration? Smtp { get; set; }

    /// Checks if the given domain part of an email address is allowed based on the configured allowed domains.
    public bool IsAllowedDomain(MailAddress address)
    {
        return IsAllowedDomain(address.Host);
    }

    /// Determines if a given domain is within the list of allowed domains.
    public bool IsAllowedDomain(string domain)
    {
        if (string.IsNullOrWhiteSpace(domain))
            return false;

        if (AllowedDomains.Count == 0)
            return true;

        var hostParts = domain.Split('.');
        var currentDomain = "";
        for (var i = hostParts.Length - 1; i >= 0; i--)
        {
            currentDomain = string.IsNullOrEmpty(currentDomain)
                ? hostParts[i] 
                : hostParts[i] + "." + currentDomain;

            if (AllowedDomains.Contains(currentDomain, StringComparer.CurrentCultureIgnoreCase))
                return true;
        }

        return false;
    }
}