namespace DotNetCommons.Text.Tokenizer;

public class Token<T> where T : struct
{
    public Definition<T>? Definition { get; }
    public TokenList<T> Section { get; } = new();
    public string? Text { get; private set; }
    public string? InsideText { get; private set; }
    public T ID { get; }
    public int Line { get; }
    public int Column { get; }

    public Token(Definition<T> definition, int line, int column, string? text = null)
    {
        Definition = definition;
        Line = line;
        Column = column;
        Text = InsideText = text;
        ID = definition.ID;
    }

    public Token(T id, string? text = null)
    {
        Text = InsideText = text;
        ID = id;
    }

    public override string ToString()
    {
        return $"[{ID}:{Text}]";
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