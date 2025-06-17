using System.Text;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Text.Tokenizer;

/// <summary>
/// Class that divides a source string up into tokens.
/// </summary>
public class StringTokenizer<T> where T : struct
{
    private record MatchResult(string MatchText, Definition<T> Definition);

    private record TokenizeResult(TokenList<T> Tokens, string Text, string InsideText);

    private List<Definition<T>> Definitions { get; } = [];

    private readonly List<Characters<T>>? _modes;
    private readonly List<char>? _escapeChars;
    private readonly ILookup<char, MatchResult> _strings;
    private readonly StringDefinitions _endOfLine;

    private string _source = null!;
    private int _position;
    private int _line;
    private int _column;

    /// <summary>
    /// Instantiate a new StringTokenizer class from a given number of definitions.
    /// </summary>
    public StringTokenizer(params Definition<T>[] definitions)
    {
        Definitions.AddRange(definitions);
        
        // remove incorrect warning for incompatible types
        #pragma warning disable CA2021   
        
        // Build a list of end of line characters
        _endOfLine = new StringDefinitions();
        foreach (var definition in Definitions.OfType<EndOfLine<T>>())
            _endOfLine.Add(definition.Texts.Strings.ToArray());

        // Build a list of escape characters
        _escapeChars = Definitions
            .OfType<Escape<T>>()
            .Select(x => x.EscapeChar)
            .ToList();

        // Build a list of character modes
        _modes = Definitions
            .OfType<Characters<T>>()
            .ToList();

        // Build a list of all matching strings and their corresponding tokens, ordered by descending length and
        // grouped by their starting character
        _strings = Definitions
            .OfType<Strings<T>>()
            .SelectMany(def => def.Texts.Strings.Select(s => new MatchResult(s, def)))
            .OrderByDescending(x => x.MatchText.Length)
            .ToLookup(x => x.MatchText[0]);
        
        #pragma warning restore CA2021
    }

    /// <summary>
    /// Parse a string into a series of tokens.
    /// </summary>
    public TokenList<T> Tokenize(string text)
    {
        _source = text;
        _position = 0;
        _line = 1;
        _column = 1;
        return DoTokenize(null, 0).Tokens;
    }

    private TokenizeResult DoTokenize(StringDefinitions? endTexts, int start)
    {
        var originalPosition = _position;
        var end = 0;
        var result = new TokenList<T>();
        if (string.IsNullOrEmpty(_source))
            return new TokenizeResult(result, "", "");

        void AddToResult(Definition<T> definition, Token<T> token)
        {
            if (!definition.Discard)
                result.Add(token);
        }

        var sb = new StringBuilder();
        Token<T>? current = null;

        while (_position < _source.Length)
        {
            // Did we hit any end text?
            if (MatchEndText(endTexts, false) != null)
                break;

            // Find matching definition
            var stringMatch = MatchStrings();
            var modeMatch = MatchModes();

            // If we have a string match, and we're *not* in a modeMatch that just happpened to match
            // the previous mode we were in, match the string token. This is to make sure that once we've
            // started parsing a text mode stream, we don't want to be interrupted until it actually ends.
            // Otherwise, a string "map" would match in the middle of the text "testmap".
            if (stringMatch != null && (modeMatch == null || modeMatch != current?.Definition))
            {
                // --- Matched a string/section token

                // Flush previous text and generate a new token
                UpdateTokenText(current, sb);
                AddToResult(stringMatch.Definition, current = new Token<T>(stringMatch.Definition, _line, _column,
                    _source.Substring(_position, stringMatch.MatchText.Length)));

                var sectionStart = _position;
                _position += stringMatch.MatchText.Length;
                if (stringMatch.Definition is EndOfLine<T>)
                {
                    _line++;
                    _column = 1;
                }
                else
                    _column += stringMatch.MatchText.Length;

                if (stringMatch.Definition is Section<T> sectionMatch)
                {
                    if (sectionMatch.SectionTakesTokens)
                    {
                        // Subsection - recurse
                        var section = DoTokenize(sectionMatch.EndTexts, sectionStart);
                        current.Section.AddRange(section.Tokens);
                        current.SetText(section.Text, section.InsideText);
                    }
                    else
                        // Just capture text to this token
                        CaptureSection(sectionMatch.EndTexts, current);
                }
            }
            else if (modeMatch != null)
            {
                // --- Matched a character mode token

                if (current == null || modeMatch != current.Definition)
                {
                    // It's different. Flush the text and generate a new one.
                    UpdateTokenText(current, sb);

                    AddToResult(modeMatch, current = new Token<T>(modeMatch, _line, _column));
                }

                // Add text
                sb.Append(_source[_position++]);
                _column++;
            }
            else
                throw new StringTokenizerException(
                    $"Unexpected token '{_source[_position]}' near '{_source.Mid(_position, 30)}' at line {_line}, column {_column}");

            end = _position;
        }

        // Flush any existing text to the last token
        UpdateTokenText(current, sb);

        return new TokenizeResult(
            result,
            _source.Substring(start, _position - start),
            _source.Substring(originalPosition, end - originalPosition)
        );
    }

    /// <summary>
    /// Capture text until a given list of end strings.
    /// </summary>
    private void CaptureSection(StringDefinitions endTexts, Token<T> token)
    {
        var sb = new StringBuilder();

        while (_position < _source.Length)
        {
            (string? Text, int End)? endText;
            var c = _source[_position];
            if (_escapeChars!.Contains(c))
            {
                _position++;
                _column++;
                if (_position >= _source.Length)
                    throw new StringTokenizerException("Unexpected end of string", _position, _source);

                c = _source[_position];
            }
            else if ((endText = MatchEndText(endTexts, false)) != null)
            {
                var s = sb.ToString();
                token.SetText(token.Text + s + endText.Value.Text, s);
                return;
            }
            else if ((endText = MatchEndText(_endOfLine, true)) != null)
            {
                sb.Append(endText.Value.Text);
                continue;
            }

            sb.Append(c);
            _position++;
            _column++;
        }

        throw new StringTokenizerException("Unexpected end of string", _position, _source);
    }

    private MatchResult? MatchStrings()
    {
        var c = _source[_position];

        // Escape character?
        var escape = _escapeChars!.Contains(c);
        if (escape)
        {
            _position++;
            if (_position >= _source.Length)
                throw new StringTokenizerException("Unexpected end of string", _position, _source);

            c = c switch
            {
                '0' => '\0',
                'r' => '\r',
                'n' => '\n',
                't' => '\t',
                _ => _source[_position]
            };
        }

        // Try to match against text definitions first
        if (!escape)
        {
            foreach (var item in _strings[c])
            {
                if (string.CompareOrdinal(_source, _position, item.MatchText, 0, item.MatchText.Length) == 0)
                    return item;
            }
        }

        return null;
    }

    private Definition<T>? MatchModes()
    {
        // Try to match against character classes
        return _modes!.FirstOrDefault(mode => mode.IsMode(_source[_position]));
    }

    private (string? Text, int End)? MatchEndText(StringDefinitions? endTexts, bool forceEOL)
    {
        (string Match, bool EOL)? endText;
        if (endTexts == null)
            return null;

        if ((endText = endTexts.Match(_source, _position, _endOfLine)) != null)
        {
            var end = _position;
            _position += endText.Value.Match.Length;

            if (endText.Value.EOL || forceEOL)
            {
                _line++;
                _column = 1;
            }
            else
                _column += endText.Value.Match.Length;

            return (endText.Value.Match, end);
        }

        return null;
    }

    private void UpdateTokenText(Token<T>? token, StringBuilder sb)
    {
        if (sb.Length > 0 && token != null && token.Text == null)
            token.SetText(sb.ToString());

        sb.Clear();
    }
}