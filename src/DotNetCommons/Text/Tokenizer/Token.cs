namespace DotNetCommons.Text.Tokenizer;

public class Token<T>
{
    public TokenDefinition<T> Definition { get; }
    public TokenList<T> Section { get; } = new();
    public string? Text { get; set; }
    public T ID => Definition.ID;

    public Token(TokenDefinition<T> definition, string? text = null)
    {
        Definition = definition;
        Text = text;
    }

    public override string ToString()
    {
        return $"[{ID}:{Text}]";
    }
}