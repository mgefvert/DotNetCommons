using DotNetCommons.Text.Tokenizer;
using System.Collections.Generic;
using System.Linq;

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
        new Characters<CsvToken>(TokenMode.Any, CsvToken.Data, false),
        new Characters<CsvToken>(TokenMode.Whitespace, CsvToken.Whitespace, false),
        new Strings<CsvToken>("\r\n", CsvToken.Newline, false),
        new Strings<CsvToken>("\n", CsvToken.Linefeed, false),
        new Strings<CsvToken>(",", CsvToken.Comma, false),
        new Section<CsvToken>("\"", "\"", false, CsvToken.Quotation, false),
        new Escape<CsvToken>('\\')
    );

    public static CsvToken GuessLineEnding(TokenList<CsvToken> tokens)
    {
        var crlf = 0;
        var lf = 0;

        foreach (var token in tokens)
        {
            if (token.ID == CsvToken.Newline)
                crlf++;
            else if (token.ID == CsvToken.Linefeed)
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
            tokenField.Trim((int)CsvToken.Whitespace);
            result.Add(tokenField.ToString(true));
        }

        return result;
    }
}