using System;
using System.Collections.Generic;

namespace CommonNetTools.IO.Parsers
{
    public class CsvParser
    {
        private enum CsvToken
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

        private static readonly StringTokenizer Tokenizer = new StringTokenizer(Definitions);

        public List<List<string>> Parse(string text)
        {
            var tokens = Tokenizer.Tokenize(text);

            // Figure out line endings used
            var newline = GuessLineEnding(tokens);

            var lines = tokens.Split(newline);
            var result = new List<List<string>>();
            foreach (var line in lines)
            {
                var tokenFields = line.Split((int) CsvToken.Comma);
                var stringFields = new List<string>();
                foreach (var tokenField in tokenFields)
                {
                    tokenField.Trim((int)CsvToken.Whitespace);
                    stringFields.Add(tokenField.ToString());
                }

                result.Add(stringFields);
            }

            return result;
        }

        private static int GuessLineEnding(TokenList tokens)
        {
            var crlf = 0;
            var lf = 0;

            foreach (var token in tokens)
            {
                if (token.Value == (int) CsvToken.Newline)
                    crlf++;
                else if (token.Value == (int) CsvToken.Linefeed)
                    lf++;

                if (crlf + lf > 100)
                    break;
            }

            return crlf > lf ? (int) CsvToken.Newline : (int) CsvToken.Linefeed;
        }
    }
}
