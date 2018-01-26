using System;
using System.Collections.Generic;
using System.Text;
using DotNetCommons.Text.Tokenizer;

namespace DotNetCommons.Text.Parsers
{
    public class ConfigStringParser
    {
        private const int WhitespaceToken = 1;
        private const int TextToken = 2;
        private const int SeparatorToken = 3;
        private const int QuoteToken = 4;
        private const int EqualsToken = 5;

        private readonly List<TokenDefinition> _definitions = new List<TokenDefinition>();

        public ConfigStringParser(string separator = ";")
        {
            _definitions.Add(new TokenCharacterModeDefinition(TokenMode.Any, TextToken, false));
            _definitions.Add(new TokenCharacterModeDefinition(TokenMode.Whitespace, WhitespaceToken, false));
            _definitions.Add(new TokenStringDefinition(separator, SeparatorToken, false));
            _definitions.Add(new TokenStringDefinition("=", EqualsToken, false));
            _definitions.Add(new TokenSectionDefinition("\"", "\"", false, QuoteToken, false));
            _definitions.Add(new TokenEscapeDefinition('\\'));
        }

        public Dictionary<string, string> Parse(string text)
        {
            var tokenizer = new StringTokenizer(_definitions);
            var tokens = tokenizer.Tokenize(text);

            var result = new Dictionary<string, string>();
            var items = tokens.Split(SeparatorToken);
            foreach (var item in items)
            {
                item.Trim(WhitespaceToken);
                var key = item.Consume(TextToken);

                item.Trim(WhitespaceToken);
                item.Consume(EqualsToken);
                item.Trim(WhitespaceToken);

                var sb = new StringBuilder();
                foreach (var token in item.ConsumeAll(TextToken, QuoteToken))
                    sb.Append(token.Text);

                result[key.Text] = sb.ToString();
            }

            return result;
        }
    }
}
