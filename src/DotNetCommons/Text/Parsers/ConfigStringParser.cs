using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetCommons.Text.Tokenizer;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Text.Parsers;

public class ConfigStringParser
{
    public enum Token
    {
        Whitespace,
        Text,
        Separator,
        Quote,
        Equal
    }

    private readonly List<Definition<Token>> _definitions = new();

    public ConfigStringParser(string separator = ";")
    {
        _definitions.AddRange(new Definition<Token>[] {
            new Characters<Token>(Token.Text, false).Add(TokenMode.Letter, TokenMode.Digit).AddSpecific("-_"),
            new Characters<Token>(Token.Whitespace, false).Add(TokenMode.Whitespace),
            new Strings<Token>(Token.Separator, separator, false),
            new Strings<Token>(Token.Equal, "=", false),
            new Section<Token>(Token.Quote, "\"", "\"", false, false),
            new Escape<Token>('\\')
        });
    }

    public Dictionary<string, string> Parse(string text)
    {
        var tokenizer = new StringTokenizer<Token>(_definitions.ToArray());
        var tokens = tokenizer.Tokenize(text);

        var result = new Dictionary<string, string>();
        var items = tokens.Split(Token.Separator);
        foreach (var item in items)
        {
            item.Trim(Token.Whitespace);
            if (!item.Any())
                break;

            var key = item.Consume(false, Token.Text);
            if (key == null)
                continue;

            item.Trim(Token.Whitespace);
            item.Consume(true, Token.Equal);
            item.Trim(Token.Whitespace);

            var sb = new StringBuilder();
            foreach (var token in item.ConsumeAll(Token.Text, Token.Quote))
                sb.Append(token.Text);

            if (!string.IsNullOrEmpty(key.Text))
                result[key.Text] = sb.ToString();
        }

        return result;
    }
}