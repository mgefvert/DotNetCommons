using System.Collections.Generic;
using System.Linq;
using System.Text;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Text.Tokenizer;

/// <summary>
/// Class that divides a source string up into tokens.
/// </summary>
public class StringTokenizer<T>
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
        return DoTokenize(text, null, ref position);
    }

    private void CaptureTextToToken(string source, ref int position, List<string> endTexts, Token<T> token)
    {
        var sb = new StringBuilder();

        while (position < source.Length)
        {
            string? endText = null;
            var c = source[position++];
            if (EscapeChars!.Contains(c))
            {
                if (position >= source.Length)
                    throw new StringTokenizerException("Unexpected end of string", position, source);

                c = source[position++];
            }
            else if ((endText = MatchesEndText(source, position, endTexts)) != null)
            {
                token.Text = sb.ToString();
                position += endText.Length - 1;
                return;
            }

            sb.Append(c);
        }

        throw new StringTokenizerException("Unexpected end of string", position, source);
    }

    private TokenList<T> DoTokenize(string source, List<string>? endTexts, ref int position)
    {
        var result = new TokenList<T>();
        if (string.IsNullOrEmpty(source))
            return result;

        var sb = new StringBuilder();
        Token<T>? current = null;

        while (position < source.Length)
        {
            // Did we hit the end text?
            string? endText;
            if (endTexts != null && endTexts.Any() && (endText = MatchesEndText(source, position, endTexts)) != null)
            {
                position += endText.Length;
                UpdateTokenText(current, sb);
                return result;
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
                current = new Token<T>(match)
                {
                    Text = source.Substring(position, textMatch.Text.Length)
                };
                if (!match.Discard)
                    result.Add(current);

                position += textMatch.Text.Length;

                if (match is Section<T> sectionMatch)
                {
                    if (sectionMatch.SectionTakesTokens)
                        // Subsection - recurse
                        current.Section.AddRange(DoTokenize(source, sectionMatch.EndTexts, ref position));
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
            token.Text = sb.ToString();

        sb.Clear();
    }
}