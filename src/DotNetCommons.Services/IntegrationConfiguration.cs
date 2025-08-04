using DotNetCommons.Services.Email;
using DotNetCommons.Services.Sms;

namespace DotNetCommons.Services;

/// <summary>
/// Represents the configuration settings required for integrations within the application.
/// This class is the base class for the integration configuration.
/// </summary>
public class IntegrationConfiguration
{
    /// <summary>
    /// Gets the email configuration settings used within the application.
    /// This property provides access to manage email-related settings.
    /// </summary>
    public EmailConfiguration EmailConfiguration { get; set; } = new();

    /// <summary>
    /// Gets the SMS configuration settings used within the application.
    /// This property provides access to manage SMS-related settings, such as allowed recipient numbers,
    /// default country codes, sender information, and credentials required for sending SMS messages.
    /// </summary>
    public SmsConfiguration SmsConfiguration { get; set; } = new();

    /// <summary>
    /// Ensures that the specified path within the configuration is not null or empty.
    /// Throws an <see cref="InvalidOperationException"/> if the required configuration is missing or invalid.
    /// </summary>
    /// <param name="path">A function that specifies the path within the <see cref="IntegrationConfiguration"/> to validate.</param>
    /// <param name="pathName">The name of the configuration path in case of exceptions thrown, to let the user know what's wrong.</param>
    /// <exception cref="InvalidOperationException">Thrown when the specified path within the configuration is null, empty, or whitespace.</exception>
    public void Require(Func<IntegrationConfiguration, object?> path, string pathName)
    {
        var value = path(this);
        if (value == null || value is string s && string.IsNullOrWhiteSpace(s))
            throw new InvalidOperationException($"Configuration '{pathName}' is missing required configuration");
    }
}