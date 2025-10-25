using System.Collections.Immutable;
using System.Net.Mail;
using System.Text;

namespace DotNetCommons.Security;

/// <summary>
/// Class that washes input, like "keep only letters and digits" or "clean up this file name".
/// </summary>
public static class WhiteWash
{
    private static readonly ImmutableHashSet<char> InvalidFileNameChars = Path.GetInvalidFileNameChars().ToImmutableHashSet();
    private static readonly ImmutableHashSet<string> ReservedFileNames = new[]
    {
        "CON", "PRN", "AUX", "NUL",
        "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
        "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
    }.ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);

    /// Validates and normalizes an email address. If the input is invalid or empty, null is returned.
    public static string? EmailAddress(string emailAddress)
    {
        if (string.IsNullOrWhiteSpace(emailAddress))
            return null;

        try
        {
            var addr = new MailAddress(emailAddress);
            return addr.Address;
        }
        catch
        {
            return null;
        }
    }

    /// Determines whether the given email address is valid by comparing it with its normalized form.
    public static bool IsValidEmailAddress(string emailAddress)
    {
        return EmailAddress(emailAddress) == emailAddress;
    }

    /// <summary>
    /// Sanitizes a file name by removing invalid characters and trimming the result to a maximum length of 200 characters.
    /// If the cleaned file name is reserved or invalid, an exception is thrown.
    /// </summary>
    /// <param name="fileName">The original file name to be sanitized.</param>
    /// <returns>A cleaned and valid file name.</returns>
    public static string FileName(string fileName)
    {
        fileName = fileName.Trim();

        var buffer = new StringBuilder(fileName.Length);
        foreach (var c in fileName)
            if (!InvalidFileNameChars.Contains(c))
                buffer.Append(c);

        var result = buffer.ToString().Left(200);

        // Validate *after* cleaning — user may have given us junk like "///"
        ArgumentException.ThrowIfNullOrWhiteSpace(result, nameof(fileName));
        if (ReservedFileNames.Contains(Path.GetFileNameWithoutExtension(result)))
            throw new ArgumentException($"File name is reserved ({result}).", nameof(fileName));

        return result;
    }

    /// <summary>
    /// Converts a phone number to ITU standard by retaining only digits and valid symbols.
    /// Adds the default country code if a local number is detected.
    /// </summary>
    /// <param name="number">The phone number to be converted.</param>
    /// <param name="defaultCountryCode">The default country code to prepend when the number starts with a local prefix.</param>
    /// <returns>The ITU-standardized phone number, or null if the input is invalid or empty.</returns>
    public static string? PhoneNumberToItuNumber(string? number, string? defaultCountryCode)
    {
        if (string.IsNullOrWhiteSpace(number))
            return null;

        number = new string(number.Where(c => c == '+' || char.IsDigit(c)).ToArray());

        if (number.StartsWith("00"))
            number = "+" + number.Mid(2);
        else if (number.StartsWith("0") && defaultCountryCode.IsSet())
            number = defaultCountryCode + number.Mid(1);
        else if (!number.StartsWith("+"))
            number = "+" + number;

        return number.Length > 1 ? number : null;
    }
}