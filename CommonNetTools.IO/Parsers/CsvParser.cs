using System;
using System.Collections.Generic;

namespace CommonNetTools.IO.Parsers
{
    public static class CsvParser
    {
        private enum CsvToken
        {
            Whitespace,
            Data,
            Comma,
            Quotation,
            NewLine
        }

        private static readonly TokenDefinition[] Definitions = {
            new TokenCharacterModeDefinition(TokenMode.Any, (int)CsvToken.Data, false), 
            new TokenCharacterModeDefinition(TokenMode.Whitespace, (int)CsvToken.Whitespace, false), 
            new TokenStringDefinition("\r\n", (int)CsvToken.NewLine, false), 
            new TokenStringDefinition(",", (int)CsvToken.Comma, false),
            new TokenSectionDefinition("\"", "\"", false, (int)CsvToken.Quotation, false),
            new TokenEscapeDefinition('\\')
        };

        private static readonly StringTokenizer Tokenizer = new StringTokenizer(Definitions);

        public static List<List<string>> Parse(string text)
        {
            var tokens = Tokenizer.Tokenize(text);

            var lines = tokens.Split((int)CsvToken.NewLine);
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
    }
}
