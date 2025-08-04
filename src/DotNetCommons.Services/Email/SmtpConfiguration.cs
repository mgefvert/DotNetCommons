namespace DotNetCommons.Services.Email;

public class SmtpConfiguration
{
    /// Hostname or IP address of the SMTP server used for sending emails.
    public string? Host { get; set; }

    /// Port for sending emails. If not set, will use the UseTls property to figure out the correct port, 587 or 25.
    public int? Port { get; set; }

    /// Username to use for authentication.
    public string? Username { get; set; }

    /// Password to use for authentication.
    public string? Password { get; set; }

    /// Whether to use TLS for communication or not.
    public bool UseTls { get; set; }
}