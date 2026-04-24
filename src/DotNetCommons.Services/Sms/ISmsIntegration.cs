namespace DotNetCommons.Services.Sms;

/// <summary>
/// Represents an interface for SMS integration, providing functionality to send SMS messages
/// to recipients through various implementation strategies.
/// </summary>
public interface ISmsIntegration
{
    string? FormatPhoneNumber(string? phoneNumber, string? defaultNumber = null);

    /// <summary>
    /// Sends a list of SMS messages asynchronously to the integration provider for delivery.
    /// </summary>
    /// <param name="messages">The list of SMS messages to be sent.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation that resolves to a list of <see cref="SmsMessageResult"/>
    /// containing the results of each SMS send operation.</returns>
    Task<List<SmsMessageResult>> SendAsync(List<SmsMessage> messages, CancellationToken cancellationToken = default);
}