using DotNetCommons.Services.Email;
using DotNetCommons.Services.Sms;

namespace DotNetCommons.Services;

public class IntegrationConfiguration
{
    public EmailConfiguration EmailConfiguration { get; } = new();
    public SmsConfiguration SmsConfiguration { get; set; } = new();

    public void Require(Func<IntegrationConfiguration, object?> path, object integration)
    {
        var value = path(this);
        if (value == null || value is string s && string.IsNullOrWhiteSpace(s))
            throw new InvalidOperationException($"Integration {integration.GetType().Name} is missing required configuration");
    }
}