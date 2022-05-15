namespace DotNetCommons.Text.Tokenizer;

public class Token
{
    private static Token? _empty;

    public string? Text { get; private set; }
    public string? InsideText { get; private set; }
    public int Line { get; }
    public int Column { get; }
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

public class Token<T> : Token where T : struct
{
    public Definition<T>? Definition { get; }
    public TokenList<T> Section { get; } = new();
    public T ID { get; }

    public Token(Definition<T> definition, int line, int column, string? text = null)
        : base(line, column, text)
    {
        Definition = definition;
        ID = definition.ID;
    }

    public override string ToString()
    {
        return $"[{ID}:{Text}]";
    }
}