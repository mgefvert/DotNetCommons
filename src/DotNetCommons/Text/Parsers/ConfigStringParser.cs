using DotNetCommons.Text.Tokenizer;
using System.Text;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Text.Parsers;

/// <summary>
/// Class that captures configuration strings on the format of "key=value; key=value", handles quoted strings as part of the value.
/// </summary>
public class ConfigStringParser
{
    private readonly bool _allowSingleKeywords;

    public enum Token
    {
        Whitespace,
        Text,
        Separator,
        Quote,
        Equal
    }

    private readonly List<Definition<Token>> _definitions = [];

    public ConfigStringParser(string separator = ";", bool allowSingleKeywords = false, bool allowSpacesInKeywords = false)
    {
        _allowSingleKeywords = allowSingleKeywords;

        var chars = new Characters<Token>(Token.Text, false)
            .Add(TokenMode.Letter, TokenMode.Digit, TokenMode.Symbols)
            .Except(separator + "=");

        if (allowSpacesInKeywords)
            chars.Add(TokenMode.Whitespace);
        
        _definitions.Add(chars);

        _definitions.Add(new Strings<Token>(Token.Separator, separator, false));
        _definitions.Add(new Strings<Token>(Token.Equal, "=", false));
        _definitions.Add(new Section<Token>(Token.Quote, "\"", "\"", false, false));
        
        if (!allowSpacesInKeywords)
            _definitions.Add(new Strings<Token>(Token.Whitespace, " ", true));
    }

    public Dictionary<string, string> Parse(string text)
    {
        var tokenizer = new StringTokenizer<Token>(_definitions.ToArray());
        var tokens = tokenizer.Tokenize(text);

        var result = new Dictionary<string, string>();
        var items = tokens.Split(Token.Separator);
        foreach (var item in items)
        {
            if (!item.Any())
                break;

            var key = item.Consume(false, Token.Text);
            if (key == null)
                continue;

            if (!item.Any())
            {
                if (_allowSingleKeywords && key.Text.IsSet())
                    result[key.Text] = "true";
                else if (!_allowSingleKeywords)
                    throw new StringTokenizerException($"Invalid keyword in text at {key.Line}:{key.Column}");
                continue;
            }
            
            item.Consume(true, Token.Equal);

            var sb = new StringBuilder();
            foreach (var token in item.ConsumeAll(Token.Text, Token.Quote))
                sb.Append(token.InsideText);

            if (!string.IsNullOrEmpty(key.Text))
                result[key.Text] = sb.ToString();
        }

        return result;
    }
}