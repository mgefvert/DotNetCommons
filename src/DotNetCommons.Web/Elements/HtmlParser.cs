namespace DotNetCommons.Web.Elements;

/// <summary>
/// The HtmlParser class is responsible for parsing an HTML string and converting it into a hierarchical structure of nodes.
/// It processes the HTML text, identifying elements, attributes, and content, constructing a tree representation of the document.
/// </summary>
/// <remarks>
/// The parser supports textual content, simple and nested elements, and ensures proper tag matching.
/// An exception will be thrown if the HTML contains unmatched or improperly nested tags.
/// </remarks>
public class HtmlParser
{
    public static HNode Parse(string html)
    {
        var parser = new HtmlParser(html);
        return parser.Parse();
    }

    private readonly string _html;
    private int _position;

    private HtmlParser(string html)
    {
        _html = html ?? "";
    }

    private HNode Parse()
    {
        var root = new HNode();
        var stack = new Stack<HElement>();

        while (_position < _html.Length)
        {
            var parent = stack.Count == 0 ? root : stack.Peek();

                if (Peek("<!--"))
                {
                    parent.Children.Add(ReadComment());
                    continue;
                }

            if (Current != '<')
            {
                AddText(parent, ReadText());
                continue;
            }

            if (Peek("</"))
            {
                var name = ReadEndTag();
                if (stack.Count == 0 || !string.Equals(stack.Peek().Name, name, StringComparison.OrdinalIgnoreCase))
                    throw new FormatException($"Unexpected closing tag </{name}>.");

                stack.Pop();
                continue;
            }

            var element = ReadStartTag(out var selfClosing);
            parent.Children.Add(element);

            if (!selfClosing && !element.IsVoidElement)
                stack.Push(element);
        }

        if (stack.Count > 0)
            throw new FormatException($"Missing closing tag </{stack.Peek().Name}>.");

        return root;
    }

    private char Current => _html[_position];

    private void AddText(HNode parent, string text)
    {
        if (text.Length > 0)
            parent.Children.Add(HText.Raw(text));
    }

    private string ReadText()
    {
        var start = _position;
        while (_position < _html.Length && Current != '<')
            _position++;

        return _html[start.._position];
    }

        private HComment ReadComment()
        {
            _position += 4;
            return new HComment(ReadUntil("-->").Trim());
        }

        private string ReadUntil(string marker)
        {
            var start = _position;
            var end = _html.IndexOf(marker, _position, StringComparison.Ordinal);
            if (end == -1)
            {
                _position = _html.Length;
                return _html[start..];
            }

            _position = end + marker.Length;
            return _html[start..end];
        }

    private string ReadEndTag()
    {
        _position += 2;
        SkipWhitespace();

        var name = ReadName();
        SkipWhitespace();
        Expect('>');

        return name;
    }

    private HElement ReadStartTag(out bool selfClosing)
    {
        Expect('<');
        SkipWhitespace();

        var element = new HElement(ReadName());
        selfClosing = false;

        while (_position < _html.Length)
        {
            SkipWhitespace();

            if (Peek("/>"))
            {
                _position += 2;
                selfClosing = true;
                return element;
            }

            if (Current == '>')
            {
                _position++;
                return element;
            }

            ReadAttribute(element);
        }

        throw new FormatException($"Unterminated start tag <{element.Name}>.");
    }

    private void ReadAttribute(HElement element)
    {
        var name = ReadName();
        SkipWhitespace();

        if (_position >= _html.Length || Current != '=')
        {
            element.Children.Add(new HAttribute(name));
            return;
        }

        _position++;
        SkipWhitespace();

        element.Children.Add(new HAttribute(name, ReadAttributeValue()));
    }

    private string ReadAttributeValue()
    {
        if (_position >= _html.Length)
            return "";

        if (Current is '"' or '\'')
        {
            var quote = Current;
            _position++;

            var start = _position;
            while (_position < _html.Length && Current != quote)
                _position++;

            var value = _html[start.._position];
            if (_position < _html.Length)
                _position++;

            return value;
        }

        var unquotedStart = _position;
        while (_position < _html.Length && !char.IsWhiteSpace(Current) && Current != '>' && !Peek("/>"))
            _position++;

        return _html[unquotedStart.._position];
    }

    private string ReadName()
    {
        var start = _position;
        while (_position < _html.Length && IsNameChar(Current))
            _position++;

        if (start == _position)
            throw new FormatException($"Expected a name at position {_position}.");

        return _html[start.._position];
    }

    private bool Peek(string value)
    {
        return _position + value.Length <= _html.Length &&
               string.CompareOrdinal(_html, _position, value, 0, value.Length) == 0;
    }

    private void Expect(char value)
    {
        if (_position >= _html.Length || Current != value)
            throw new FormatException($"Expected '{value}' at position {_position}.");

        _position++;
    }

    private void SkipWhitespace()
    {
        while (_position < _html.Length && char.IsWhiteSpace(Current))
            _position++;
    }

    private static bool IsNameChar(char c)
    {
        return char.IsLetterOrDigit(c) || c is '-' or '_' or ':' or '.';
    }
}
