using System.Collections.Immutable;
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
}