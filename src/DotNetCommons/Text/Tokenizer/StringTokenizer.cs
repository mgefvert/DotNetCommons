using System.Collections.Generic;
using System.Linq;
using System.Text;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Text.Tokenizer;

/// <summary>
/// Class that divides a source string up into tokens.
/// </summary>
public class StringTokenizer<T> where T : struct
{
    protected List<Definition<T>> Definitions { get; } = new();
    protected bool IsPrepared;

    protected List<Characters<T>>? Modes;
    protected List<char>? EscapeChars;
    protected ILookup<char, Strings<T>>? Strings;

    /// <summary>
    /// Instantiate a new StringTokenizer class.
    /// </summary>
    public StringTokenizer()
    {
    }

    /// <summary>
    /// Instantiate a new StringTokenizer class from a given number of definitions.
    /// </summary>
    public StringTokenizer(params Definition<T>[] definitions)
    {
        Definitions.AddRange(definitions);

        EscapeChars = Definitions.OfType<Escape<T>>().Select(x => x.EscapeChar).ToList();
        Modes = Definitions.OfType<Characters<T>>().OrderByDescending(x => x.Mode).ToList();
        Strings = Definitions.OfType<Strings<T>>().OrderByDescending(x => x.Text.Length).ToLookup(x => x.Text[0]);
        IsPrepared = true;
    }

    /// <summary>
    /// Parse a string into a series of tokens.
    /// </summary>
    public TokenList<T> Tokenize(string text)
    {
        var position = 0;
        return DoTokenize(text, null, ref position, 0).Item1;
    }

    private void CaptureTextToToken(string source, ref int position, List<string> endTexts, Token<T> token)
    {
        var sb = new StringBuilder();

        while (position < source.Length)
        {
            string? endText;
            var c = source[position];
            if (EscapeChars!.Contains(c))
            {
                position++;
                if (position >= source.Length)
                    throw new StringTokenizerException("Unexpected end of string", position, source);

                c = source[position];
            }
            else if ((endText = MatchesEndText(source, position, endTexts)) != null)
            {
                var s = sb.ToString();
                token.SetText(token.Text + s + endText, s);
                position += endText.Length;
                return;
            }

            sb.Append(c);
            position++;
        }

        throw new StringTokenizerException("Unexpected end of string", position, source);
    }

    private (TokenList<T> Tokens, string Text, string InsideText) DoTokenize(string source, List<string>? endTexts, ref int position, int start)
    {
        var originalPosition = position;
        int end = 0;
        var result = new TokenList<T>();
        if (string.IsNullOrEmpty(source))
            return (result, "", "");

        void AddToResult(Definition<T> definition, Token<T> token)
        {
            if (!definition.Discard)
                result.Add(token);
            if (definition.Append != null)
                result.Add(new Token<T>(definition.Append.Value));
        }

        var sb = new StringBuilder();
        Token<T>? current = null;

        while (position < source.Length)
        {
            // Did we hit the end text?
            string? endText;
            if (endTexts != null && endTexts.Any() && (endText = MatchesEndText(source, position, endTexts)) != null)
            {
                end = position;
                position += endText.Length;
                break;
            }

            // Find matching definition
            var match = MatchText(source, ref position);
            if (match == null)
                throw new StringTokenizerException("Unexpected token", position, source);

            if (match is Strings<T> textMatch)
            {
                // --- Matched a text token

                // Flush previous text and generate a new token
                UpdateTokenText(current, sb);
                current = new Token<T>(match);
                current.SetText(source.Substring(position, textMatch.Text.Length));
                AddToResult(match, current);

                var sectionStart = position;
                position += textMatch.Text.Length;

                if (match is Section<T> sectionMatch)
                {
                    if (sectionMatch.SectionTakesTokens)
                    {
                        // Subsection - recurse
                        var section = DoTokenize(source, sectionMatch.EndTexts, ref position, sectionStart);
                        current.Section.AddRange(section.Tokens);
                        current.SetText(section.Text, section.InsideText);
                    }
                    else
                        // Just capture text to this token
                        CaptureTextToToken(source, ref position, sectionMatch.EndTexts, current);
                }
            }
            else if (match is Characters<T>)
            {
                // --- Matched a character mode token

                if (current == null || match != current.Definition)
                {
                    // It's different. Flush the text and generate a new one.
                    UpdateTokenText(current, sb);

                    current = new Token<T>(match);
                    AddToResult(match, current);
                }

                // Add text
                sb.Append(source[position++]);
            }

            end = position;
        }

        // Flush any existing text to the last token
        UpdateTokenText(current, sb);

        return (result, source.Substring(start, position - start), source.Substring(originalPosition, end - originalPosition));
    }

    private string? MatchesEndText(string source, int position, List<string> endTexts)
    {
        foreach (var endText in endTexts)
            if (string.CompareOrdinal(source, position, endText, 0, endText.Length) == 0)
                return endText;

        return null;
    }

    private Definition<T>? MatchText(string source, ref int position)
    {
        var c = source[position];

        // Escape character?
        var escape = EscapeChars!.Contains(c);
        if (escape)
        {
            position++;
            if (position >= source.Length)
                throw new StringTokenizerException("Unexpected end of string", position, source);

            c = c switch
            {
                '0' => '\0',
                'r' => '\r',
                'n' => '\n',
                't' => '\t',
                _ => source[position]
            };
        }

        // Try to match against text definitions first
        if (!escape)
        {
            var definitions = Strings![c];
            foreach (var definition in definitions)
                if (string.CompareOrdinal(source, position, definition.Text, 0, definition.Text.Length) == 0)
                    return definition;
        }

        // Try to match against character classes
        return Modes!.FirstOrDefault(mode => mode.IsMode(c));
    }

    private void UpdateTokenText(Token<T>? token, StringBuilder sb)
    {
        if (sb.Length > 0 && token != null && token.Text == null)
            token.SetText(sb.ToString());

        sb.Clear();
    }
}