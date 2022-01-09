using System;
using System.Collections.Generic;
using System.Linq;
using DotNetCommons.Text.Tokenizer;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Text.Parsers;

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

    private static readonly TokenDefinition[] Definitions = {
        new TokenCharacterModeDefinition(TokenMode.Any, (int)CsvToken.Data, false),
        new TokenCharacterModeDefinition(TokenMode.Whitespace, (int)CsvToken.Whitespace, false),
        new TokenStringDefinition("\r\n", (int)CsvToken.Newline, false),
        new TokenStringDefinition("\n", (int)CsvToken.Linefeed, false),
        new TokenStringDefinition(",", (int)CsvToken.Comma, false),
        new TokenSectionDefinition("\"", "\"", false, (int)CsvToken.Quotation, false),
        new TokenEscapeDefinition('\\')
    };

    private static readonly StringTokenizer Tokenizer = new(Definitions);

    public static CsvToken GuessLineEnding(TokenList tokens)
    {
        var crlf = 0;
        var lf = 0;

        foreach (var token in tokens)
        {
            if (token.Value == (int)CsvToken.Newline)
                crlf++;
            else if (token.Value == (int)CsvToken.Linefeed)
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
        var newline = (int)GuessLineEnding(tokens);

        return tokens
            .Split(newline)
            .Select(TokensToLine)
            .ToList();
    }

    internal List<string> TokensToLine(TokenList tokens)
    {
        var tokenFields = tokens.Split((int)CsvToken.Comma);
        var result = new List<string>();

        foreach (var tokenField in tokenFields)
        {
            tokenField.Trim((int)CsvToken.Whitespace);
            result.Add(tokenField.ToString());
        }

        return result;
    }
}