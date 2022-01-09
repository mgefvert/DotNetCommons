using System;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Text.Tokenizer;

public enum TokenMode
{
    None,
    Any,
    Letter,
    Digit,
    LetterOrDigit,
    Whitespace,
    EndOfLine
}

public abstract class TokenDefinition
{
    public bool Discard { get; }
    public int Value { get; }

    protected TokenDefinition(int value, bool discard)
    {
        Discard = discard;
        Value = value;
    }
}

public class TokenCharacterModeDefinition : TokenDefinition
{
    public TokenMode Mode { get; }

    public TokenCharacterModeDefinition(TokenMode mode, int value, bool discard) : base(value, discard)
    {
        Mode = mode;
    }

    public bool IsMode(char c)
    {
        switch (Mode)
        {
            case TokenMode.Any:
                return true;
            case TokenMode.Letter:
                return char.IsLetter(c);
            case TokenMode.Digit:
                return char.IsDigit(c);
            case TokenMode.LetterOrDigit:
                return char.IsLetterOrDigit(c);
            case TokenMode.Whitespace:
                return char.IsWhiteSpace(c);
            case TokenMode.EndOfLine:
                return c == 13 || c == 10;
            default:
                return false;
        }
    }
}

public class TokenSectionDefinition : TokenStringDefinition
{
    public string EndText { get; }
    public bool SectionTakesTokens { get; }

    public TokenSectionDefinition(string text, string endtext, bool takesTokens, int value, bool discard) : base(text, value, discard)
    {
        if (string.IsNullOrEmpty(endtext))
            throw new ArgumentException("End text must not be empty.", nameof(endtext));

        EndText = endtext;
        SectionTakesTokens = takesTokens;
    }
}

public class TokenStringDefinition : TokenDefinition
{
    public string Text { get; }

    public TokenStringDefinition(string text, int value, bool discard) : base(value, discard)
    {
        if (string.IsNullOrEmpty(text))
            throw new ArgumentException("Text must not be empty.", nameof(text));

        Text = text;
    }
}

public class TokenEscapeDefinition : TokenDefinition
{
    public char EscapeChar { get; }

    public TokenEscapeDefinition(char escapeChar) : base(0, false)
    {
        EscapeChar = escapeChar;
    }
}