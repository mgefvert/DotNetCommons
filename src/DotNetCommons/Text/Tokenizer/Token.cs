namespace DotNetCommons.Text.Tokenizer;

public class Token<T>
{
    public Definition<T> Definition { get; }
    public TokenList<T> Section { get; } = new();
    public string? Text { get; set; }
    public T ID => Definition.ID;

    public Token(Definition<T> definition, string? text = null)
    {
        Definition = definition;
        Text = text;
    }

    public override string ToString()
    {
        return $"[{ID}:{Text}]";
    }
}