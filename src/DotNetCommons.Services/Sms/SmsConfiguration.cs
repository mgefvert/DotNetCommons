using DotNetCommons.Security;

namespace DotNetCommons.Services.Sms;

public class SmsConfiguration
{
    public List<string> AllowedNumbers { get; set; } = [];

    public string? DefaultCountryCode { get; set; }
    public string? RecipientOverride { get; set; }
    public string? SenderType { get; set; }
    public string? SenderNumber { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }

    public bool IsAllowedNumber(string? phoneNumber)
    {
        var ituNumber = WhiteWash.PhoneNumberToItuNumber(phoneNumber, DefaultCountryCode);
        if (ituNumber.IsEmpty())
            return false;

        return AllowedNumbers.Count == 0 || AllowedNumbers.Any(ituNumber.StartsWith);
    }
}