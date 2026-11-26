using System.Collections.Immutable;
using System.Globalization;
using System.Net.Mail;
using System.Text;

namespace DotNetCommons.Security;

/// <summary>
/// Class that washes input, like "keep only letters and digits" or "clean up this file name".
/// </summary>
public static class Sanitizer
{
    private static readonly ImmutableHashSet<char> InvalidFileNameChars = Path.GetInvalidFileNameChars().ToImmutableHashSet();
    private static readonly ImmutableHashSet<string> ReservedFileNames = new[]
    {
        "CON", "PRN", "AUX", "NUL",
        "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
        "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
    }.ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);
    private static readonly ImmutableHashSet<string> AdditionalDosDeviceNames = new[]
    {
        "CONIN$", "CONOUT$", "COM0", "LPT0",
        "COM\u00B9", "COM\u00B2", "COM\u00B3", "LPT\u00B9", "LPT\u00B2", "LPT\u00B3"
    }.ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    /// Validates and normalizes an email address. If the input is invalid or empty, null is returned.
    public static string? EmailAddress(string emailAddress)
    {
        if (string.IsNullOrWhiteSpace(emailAddress))
            return null;

        try
        {
            var addr = new MailAddress(emailAddress);

            if (!IsValidHostName(addr.Host))
                return null;
            if (!IsValidEmailUser(addr.User))
                return null;

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
        return !string.IsNullOrEmpty(emailAddress) && EmailAddress(emailAddress) == emailAddress;
    }

    /// Determines whether the given email user name (local part) of an email address is valid based on character and format constraints.
    public static bool IsValidEmailUser(string emailUserName)
    {
        // Validate local part according to RFC 5321/5322 standards
        if (string.IsNullOrEmpty(emailUserName) || emailUserName.Length > 64)
            return false;

        // Cannot start or end with dot
        if (emailUserName.StartsWith('.') || emailUserName.EndsWith('.'))
            return false;

        // Check for consecutive dots
        if (emailUserName.Contains(".."))
            return false;

        // Validate characters: letters, digits, and special characters allowed in email local part
        // Common allowed special characters: . ! # $ % & ' * + - / = ? ^ _ ` { | } ~
        foreach (var c in emailUserName)
        {
            if (!char.IsLetterOrDigit(c) &&
                c != '.' && c != '!' && c != '#' && c != '$' && c != '%' &&
                c != '&' && c != '\'' && c != '*' && c != '+' && c != '-' &&
                c != '/' && c != '=' && c != '?' && c != '^' && c != '_' &&
                c != '`' && c != '{' && c != '|' && c != '}' && c != '~')
                return false;
        }

        return true;
    }

    /// Validates and checks if the given host name adheres to DNS standards.
    public static bool IsValidHostName(string hostName)
    {
        // Validate host part according to DNS standards
        if (string.IsNullOrEmpty(hostName) || hostName.Length > 253)
            return false;

        // Cannot start or end with dot or hyphen
        if (hostName.StartsWith('.') || hostName.EndsWith('.') ||
            hostName.StartsWith('-') || hostName.EndsWith('-'))
            return false;

        // Check for consecutive dots
        if (hostName.Contains(".."))
            return false;

        // Validate characters: only letters, digits, dots, and hyphens
        foreach (var c in hostName)
        {
            if (!char.IsLetterOrDigit(c) && c != '.' && c != '-')
                return false;
        }

        // Validate individual labels (parts between dots)
        var labels = hostName.Split('.');
        foreach (var label in labels)
        {
            // Each label must be 1-63 characters
            if (string.IsNullOrEmpty(label) || label.Length > 63)
                return false;

            // Labels cannot start or end with hyphen
            if (label.StartsWith('-') || label.EndsWith('-'))
                return false;
        }

        return true;
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
    /// Escapes a single name for use as a filename on Windows and Unix.
    /// </summary>
    /// <remarks>
    /// Uses percent escapes for invalid characters, literal percent signs, trailing spaces and periods,
    /// and the first letter of a reserved DOS device name. The result preserves case, Unicode, and
    /// the identity of distinct inputs under ordinal comparison. Callers must account for filesystem
    /// length limits and any case folding or Unicode normalization performed by the filesystem.
    /// </remarks>
    /// <exception cref="ArgumentException">The input is empty or contains malformed UTF-16.</exception>
    /// <exception cref="ArgumentNullException">The input is null.</exception>
    public static string EscapeFileName(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        _ = StrictUtf8.GetByteCount(name);

        var extensionIndex = name.IndexOf('.');
        var stem = (extensionIndex < 0 ? name : name[..extensionIndex]).TrimEnd(' ');
        var isDevice = ReservedFileNames.Contains(stem) || AdditionalDosDeviceNames.Contains(stem);
        var trailingStart = name.AsSpan().TrimEnd(" .").Length;

        var buffer = new StringBuilder(name.Length);
        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (c < ' ' || c is '"' or '<' or '>' or ':' or '/' or '\\' or '|' or '?' or '*' or '%' or '\u007f' ||
                i >= trailingStart || (i == 0 && isDevice))
                buffer.Append('%').Append(((int)c).ToString("X2", CultureInfo.InvariantCulture));
            else
                buffer.Append(c);
        }

        return buffer.ToString();
    }

    /// <summary>
    /// Converts a phone number to ITU standard by retaining only digits and valid symbols.
    /// Adds the default country code if a local number is detected.
    /// </summary>
    /// <param name="number">The phone number to be converted.</param>
    /// <param name="defaultCountryCode">The default country code to prepend when the number starts with a local prefix.</param>
    /// <returns>The ITU-standardized phone number, or null if the input is invalid or empty.</returns>
    public static string? PhoneNumberToItuNumber(string? number, string? defaultCountryCode = null)
    {
        if (string.IsNullOrWhiteSpace(number))
            return null;

        number = new string(number.Where(c => c == '+' || char.IsDigit(c)).ToArray());

        if (number.StartsWith("00"))
            number = "+" + number.Mid(2);
        else if (number.StartsWith("0") && defaultCountryCode.IsSet())
            number = defaultCountryCode + number.Mid(1);
        else if (!number.StartsWith("+") && defaultCountryCode.IsSet())
            number = defaultCountryCode + number;

        return number.Length > 1 ? number : null;
    }

    /// <summary>
    /// Removes HTML tags from the input string while preserving the remaining text content.
    /// Handles escape characters and attributes within HTML tags appropriately.
    /// </summary>
    /// <remarks>
    /// While functional, it's not to be treated as a hardened security function - it simply removes tags. It does not remove
    /// script blocks, just the tags; it may treat &lt; and &gt; as tags when not intended to; and it can possibly be tricked
    /// by crafty adversaries.
    /// </remarks>
    public static string? StripHtmlTags(string? source)
    {
        if (string.IsNullOrEmpty(source))
            return source;

        var inTag       = false;
        var inAttribute = '\0';
        var escape      = false;

        var result = new StringBuilder(source.Length);
        foreach (var c in source)
        {
            if (escape)
            {
                // Last character was a \ in an attribute string, just ignore this character.
                escape = false;
            }
            else if (inAttribute != 0)
            {
                if (c == '\\')
                    escape = true;
                else if (c == inAttribute)
                    inAttribute = '\0';
            }
            else if (inTag)
            {
                if (c is '\'' or '\"' or '`')
                    inAttribute = c;
                else if (c == '>')
                    inTag = false;
            }
            else if (c == '<')
                inTag = true;
            else
                result.Append(c);
        }

        return result.ToString();
    }
}
