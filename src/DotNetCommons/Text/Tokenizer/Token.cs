namespace DotNetCommons.Text.Tokenizer;

/// <summary>
/// Encapsulates a specific token found in a stream.
/// </summary>
public class Token
{
    private static Token? _empty;

    /// <summary>
    /// Text captured by this token. Includes quotes and surrounding text.
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// Text captured by this token. Excludes surrounding quotes and similar.
    /// </summary>
    public string? InsideText { get; set; }

    /// <summary>
    /// Line number where this text was captured.
    /// </summary>
    public int Line { get; }

    /// <summary>
    /// Column number where this text was captured.
    /// </summary>
    public int Column { get; }

    /// <summary>
    /// Generic tag that can hold any object.
    /// </summary>
    public object Tag { get; set; }

    /// <summary>
    /// Default Empty token.
    /// </summary>
    public static Token Empty => _empty ??= new(0, 0);

    public Token(int line, int column, string? text = null)
    {
        Line = line;
        Column = column;
        Text = InsideText = text;
    }

    public void SetText(string text)
    {
        Text = InsideText = text;
    }

    public void SetText(string text, string insideText)
    {
        Text = text;
        InsideText = insideText;
    }
}

public class Token<T> : Token where T : struct, Enum
{
    public Definition<T>? Definition { get; }
    public TokenList<T> Section { get; } = [];
    public T ID { get; set; }

    public Token(Definition<T> definition, int line, int column, string? text = null)
        : base(line, column, text)
    {
        Definition = definition;
        ID = definition.ID;
    }

    public override string ToString() => ToString(false);

    public string ToString(bool insideText)
    {
        return $"[{ID}:{(insideText ? InsideText : Text)}{(Section.Any() ? " " + Section : "")}]";
    }
}