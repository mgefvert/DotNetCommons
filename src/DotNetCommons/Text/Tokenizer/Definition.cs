// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Text.Tokenizer;

public enum TokenMode
{
    None,
    Specific,
    Letter,
    Digit,
    Whitespace,
    EndOfLine,
    Symbols
}

/// <summary>
/// Abstract base class for all definitions. The ID is a value - usually an enum - that defines
/// the category or type, decided entirely by the caller and how the callers wants to group or
/// classify tokens.
/// </summary>
/// <typeparam name="T">Type of the ID value.</typeparam>
public abstract class Definition<T> where T : struct
{
    public bool Discard { get; }
    public T Id { get; }
    
    /// <summary>
    /// Generic value that can be used for user-defined things. 
    /// </summary>
    public object? Tag { get; set; }

    protected Definition(T id, bool discard)
    {
        Discard = discard;
        Id = id;
    }
}

/// <summary>
/// Capture a sequence of characters, e.g. digits, letters or digits, end of lines, whitespace etc.
/// </summary>
public class Characters<T> : Definition<T> where T : struct
{
    public HashSet<TokenMode> Modes { get; } = [];
    public HashSet<char> Include { get; } = [];
    public HashSet<char> Exclude { get; } = [];

    public Characters(T id, bool discard) : base(id, discard)
    {
    }

    public Characters<T> Add(params TokenMode[] modes)
    {
        foreach (var mode in modes)
            Modes.Add(mode);

        return this;
    }

    public Characters<T> AddSpecific(string characters)
    {
        Modes.Add(TokenMode.Specific);
        foreach (var c in characters)
            Include.Add(c);

        return this;
    }

    public Characters<T> Except(string characters)
    {
        foreach (var c in characters)
            Exclude.Add(c);

        return this;
    }

    public bool IsMode(char c)
    {
        if (Exclude.Contains(c))
            return false;

        foreach (var mode in Modes)
        {
            var result = mode switch
            {
                TokenMode.Letter => char.IsLetter(c),
                TokenMode.Digit => char.IsDigit(c),
                TokenMode.Whitespace => char.IsWhiteSpace(c),
                TokenMode.EndOfLine => c == 13 || c == 10,
                TokenMode.Specific => Include.Contains(c),
                TokenMode.Symbols => char.IsPunctuation(c) || char.IsSymbol(c),
                _ => false
            };
            if (result)
                return true;
        }

        return false;
    }
}

/// <summary>
/// Define an escape character. Not actually used as a token, but for internal state-keeping.
/// </summary>
public class Escape<T> : Definition<T> where T : struct
{
    public char EscapeChar { get; }

    public Escape(char escapeChar) : base(default!, false)
    {
        EscapeChar = escapeChar;
    }
}

/// <summary>
/// Match specific strings, like "=" or "and".
/// </summary>
public class Strings<T> : Definition<T> where T : struct
{
    public StringDefinitions Texts { get; }

    public Strings(T id, string text, bool discard) : base(id, discard)
    {
        Texts = new StringDefinitions().Add(text);
    }

    public Strings(T id, StringDefinitions texts, bool discard) : base(id, discard)
    {
        Texts = texts;
    }
}

/// <summary>
/// Define an End of Line character (usually \r\n, \r and \n). These have special meaning in that
/// they advance the line/column counters differently.
/// </summary>
public class EndOfLine<T> : Strings<T> where T : struct
{
    public EndOfLine(T id, string text, bool discard) : base(id, text, discard)
    {
    }

    public EndOfLine(T id, StringDefinitions texts, bool discard) : base(id, texts, discard)
    {
    }
}

/// <summary>
/// Capture a section. A section takes a starting text, ending text, and whether it takes
/// tokens or just strings. Useful for defining custom strings like "hello world!" and treating
/// the entire text as a single token defined by the quotes.
/// </summary>
public class Section<T> : Strings<T> where T : struct
{
    public StringDefinitions EndTexts { get; }
    public bool SectionTakesTokens { get; }

    public Section(T id, string text, string endText, bool takesTokens, bool discard)
        : base(id, text, discard)
    {
        if (string.IsNullOrEmpty(endText))
            throw new ArgumentException("End text must not be empty.", nameof(endText));

        EndTexts = new StringDefinitions().Add(endText);
        SectionTakesTokens = takesTokens;
    }

    public Section(T id, string text, StringDefinitions endTexts, bool takesTokens, bool discard)
        : base(id, text, discard)
    {
        EndTexts = endTexts;
        SectionTakesTokens = takesTokens;
    }
}

/// <summary>
/// Class that encapsulates a series of strings, and optionally a reference to the
/// EndOfLine definitions. Makes it easy to define a section with // as the starting part,
/// and EndOfLine as the ending part. Or, optionally, to terminate a " section at the
/// EndOfLine (and throwing an error).
///
/// Also, it automatically keeps the list of strings in descending order of length, which
/// makes it easy to match against correctly.
/// </summary>
public class StringDefinitions
{
    public List<string> Strings { get; private set; } = [];
    public bool EndOfLine;

    public StringDefinitions Add(params string[] strings)
    {
        if (strings.Any(string.IsNullOrEmpty))
            throw new ArgumentException("StringDefinition may not contain an empty string");

        Strings = Strings.Concat(strings).OrderByDescending(x => x.Length).ToList();
        return this;
    }

    public StringDefinitions IncludeEOL()
    {
        EndOfLine = true;
        return this;
    }

    public (string Match, bool EOL)? Match(string source, int position, StringDefinitions endOfLines)
    {
        if (EndOfLine)
            foreach (var endText in endOfLines.Strings)
                if (string.CompareOrdinal(source, position, endText, 0, endText.Length) == 0)
                    return (endText, true);

        foreach (var endText in Strings)
            if (string.CompareOrdinal(source, position, endText, 0, endText.Length) == 0)
                return (endText, false);

        return null;
    }
}