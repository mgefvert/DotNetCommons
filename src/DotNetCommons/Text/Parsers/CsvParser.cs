using DotNetCommons.Text.Tokenizer;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Text.Parsers;

/// <summary>
/// Parser that reads CSV data and returns a list of strings for each object, or a list of list
/// of strings for a sequence of objects.
/// </summary>
public class CsvParser
{
    public enum CsvToken
    {
        Whitespace,
        Data,
        Comma,
        Quotation,
        Newline,
        Linefeed
    }

    private static readonly StringTokenizer<CsvToken> Tokenizer = new(
        new Characters<CsvToken>(CsvToken.Data, false)
            .Add(TokenMode.Letter, TokenMode.Digit, TokenMode.Symbols)
            .AddSpecific("-.")
            .Except(","),
        new Characters<CsvToken>(CsvToken.Whitespace, false)
            .Add(TokenMode.Whitespace),
        new EndOfLine<CsvToken>(CsvToken.Newline, "\r\n", false),
        new Strings<CsvToken>(CsvToken.Linefeed, "\n", false),
        new Strings<CsvToken>(CsvToken.Comma, ",", false),
        new Section<CsvToken>(CsvToken.Quotation, "\"", "\"", false, false),
        new Escape<CsvToken>('\\')
    );

    public static CsvToken GuessLineEnding(TokenList<CsvToken> tokens)
    {
        var crlf = 0;
        var lf = 0;

        foreach (var token in tokens)
        {
            if (token.Id == CsvToken.Newline)
                crlf++;
            else if (token.Id == CsvToken.Linefeed)
                lf++;

            if (crlf + lf > 100)
                break;
        }

        return crlf > lf ? CsvToken.Newline : CsvToken.Linefeed;
    }

    public List<string> ParseRow(string text)
    {
        return TokensToLine(Tokenizer.Tokenize(text));
    }

    public List<List<string>> ParseRows(string text)
    {
        var tokens = Tokenizer.Tokenize(text);
        var newline = GuessLineEnding(tokens);

        return tokens
            .Split(newline)
            .Select(TokensToLine)
            .ToList();
    }

    internal List<string> TokensToLine(TokenList<CsvToken> tokens)
    {
        var tokenFields = tokens.Split(CsvToken.Comma);
        var result = new List<string>();

        foreach (var tokenField in tokenFields)
        {
            tokenField.Trim(CsvToken.Whitespace);
            result.Add(string.Join("", tokenField.Select(x => x.InsideText)).Trim());
        }

        return result;
    }
}