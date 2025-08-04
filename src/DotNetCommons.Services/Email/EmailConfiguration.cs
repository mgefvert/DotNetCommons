using System.Net.Mail;

namespace DotNetCommons.Services.Email;

public class EmailConfiguration
{
    public const string AdminEmail = "admin";
    public const string InfoEmail = "info";
    public const string NoReplyEmail = "noreply";
    public const string SupportEmail = "support";

    public List<string> AllowedDomains { get; set; } = [];
    public Dictionary<string, string> FromAddresses { get; set; } = new();
    public string? RecipientOverride { get; set; }

    public SmtpConfiguration? Smtp { get; set; }

    public bool IsAllowedDomain(MailAddress address)
    {
        return IsAllowedDomain(address.Host);
    }

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