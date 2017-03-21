using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonNetTools
{
    public class StringTokenizer
    {
        protected List<TokenDefinition> Definitions { get; } = new List<TokenDefinition>();
        protected bool IsPrepared;

        protected List<TokenCharacterModeDefinition> Modes;
        protected List<char> EscapeChars;
        protected ILookup<char, TokenStringDefinition> Strings;

        public StringTokenizer()
        {
        }

        public StringTokenizer(IEnumerable<TokenDefinition> definitions)
        {
            foreach (var definition in definitions)
                AddDefinition(definition);
        }

        public void AddDefinition(TokenDefinition definition)
        {
            Definitions.Add(definition);
            IsPrepared = false;
        }

        public void ClearDefinitions()
        {
            Definitions.Clear();
            Modes = null;
            Strings = null;
            EscapeChars = null;
            IsPrepared = false;
        }

        protected void Prepare()
        {
            EscapeChars = Definitions.OfType<TokenEscapeDefinition>().Select(x => x.EscapeChar).ToList();
            Modes = Definitions.OfType<TokenCharacterModeDefinition>().OrderByDescending(x => x.Mode).ToList();
            Strings = Definitions.OfType<TokenStringDefinition>().OrderByDescending(x => x.Text.Length).ToLookup(x => x.Text[0]);
            IsPrepared = true;
        }

        public TokenList Tokenize(string text)
        {
            if (!IsPrepared)
                Prepare();

            var position = 0;
            return DoTokenize(text, null, ref position);
        }

        private void CaptureTextToToken(string source, ref int position, string endtext, Token token)
        {
            var sb = new StringBuilder();
            var scan = endtext[0];

            while (position < source.Length)
            {
                var c = source[position++];
                if (EscapeChars.Contains(c))
                {
                    if (position >= source.Length)
                        throw new StringTokenizerException("Unexpected end of string", position, source);

                    c = source[position++];
                }
                else if (c == scan && string.CompareOrdinal(source, position, endtext, 1, endtext.Length - 1) == 0)
                {
                    token.Text = sb.ToString();
                    position += endtext.Length - 1;
                    return;
                }

                sb.Append(c);
            }

            throw new StringTokenizerException("Unexpected end of string", position, source);
        }

        private TokenList DoTokenize(string source, string endtext, ref int position)
        {
            var result = new TokenList();
            var sb = new StringBuilder();
            Token current = null;

            if (endtext == "")
                endtext = null;

            var ec = endtext?[0] ?? '\0';

            while (position < source.Length)
            {
                // Did we hit the end text?
                if (endtext != null && source[position] == ec && string.CompareOrdinal(source, position, endtext, 0, endtext.Length) == 0)
                {
                    position += endtext.Length;
                    UpdateTokenText(current, sb);
                    return result;
                }

                // Find matching definition
                var match = MatchText(source, ref position);
                if (match == null)
                    throw new StringTokenizerException("Unexpected token", position, source);

                var textMatch = match as TokenStringDefinition;
                if (textMatch != null)
                {
                    // --- Matched a text token

                    // Flush previous text and generate a new token
                    UpdateTokenText(current, sb);
                    current = new Token(match)
                    {
                        Text = source.Substring(position, textMatch.Text.Length)
                    };
                    if (!match.Discard)
                        result.Add(current);

                    position += textMatch.Text.Length;

                    var sectionMatch = match as TokenSectionDefinition;
                    if (sectionMatch != null)
                    {
                        if (sectionMatch.SectionTakesTokens)
                            // Subsection - recurse
                            current.Section.AddRange(DoTokenize(source, sectionMatch.EndText, ref position));
                        else
                            // Just capture text to this token
                            CaptureTextToToken(source, ref position, sectionMatch.EndText, current);
                    }
                }
                else if (match is TokenCharacterModeDefinition)
                {
                    // --- Matched a character mode token

                    if (current == null || match != current.Definition)
                    {
                        // It's different. Flush the text and generate a new one.
                        UpdateTokenText(current, sb);

                        current = new Token(match);
                        if (!match.Discard)
                            result.Add(current);
                    }

                    // Add text
                    sb.Append(source[position++]);
                }
            }

            // Flush any existing text to the last token
            UpdateTokenText(current, sb);

            return result;
        }

        private TokenDefinition MatchText(string source, ref int position)
        {
            var c = source[position];

            // Escape character?
            var escape = EscapeChars.Contains(c);
            if (escape)
            {
                position++;
                if (position >= source.Length)
                    throw new StringTokenizerException("Unexpected end of string", position, source);

                c = source[position];
                switch (c)
                {
                    case '0':
                        c = '\0';
                        break;
                    case 'r':
                        c = '\r';
                        break;
                    case 'n':
                        c = '\n';
                        break;
                    case 't':
                        c = '\t';
                        break;
                }
            }

            // Try to match against text definitions first
            if (!escape)
            {
                var definitions = Strings[c];
                foreach (var definition in definitions)
                    if (string.CompareOrdinal(source, position, definition.Text, 0, definition.Text.Length) == 0)
                        return definition;
            }

            // Try to match against character classes
            return Modes.FirstOrDefault(mode => mode.IsMode(c));
        }

        private void UpdateTokenText(Token token, StringBuilder sb)
        {
            if (sb.Length > 0 && token != null && token.Text == null)
                token.Text = sb.ToString();

            sb.Clear();
        }
    }
}
