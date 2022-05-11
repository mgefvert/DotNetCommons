using DotNetCommons.Text.Tokenizer;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Test.Text.Tokenizer;

[TestClass]
public class StringTokenizerTests
{
    private enum Token
    {
        None,
        Text,
        LongText,
        Equals,
        EndStatement,
        EndOfLine,
        OpenBrace,
        CloseBrace,
        Comment,
        Keyword
    }

    private StringTokenizer<Token> _tokenizer = null!;

    [TestInitialize]
    public void Setup()
    {
        _tokenizer = new StringTokenizer<Token>(
            new EndOfLine<Token>(Token.EndOfLine, new StringDefinitions().Add("\r\n", "\r", "\n"), true),
            new Characters<Token>(Token.Text, false).Add(TokenMode.Letter, TokenMode.Digit),
            new Characters<Token>(Token.None, true).Add(TokenMode.Whitespace),
            new Strings<Token>(Token.Keyword, "include", false),
            new Strings<Token>(Token.Keyword, "location", false),
            new Strings<Token>(Token.Keyword, "job", false),
            new Strings<Token>(Token.Keyword, "map", false),
            new Strings<Token>(Token.Keyword, "memory", false),
            new Strings<Token>(Token.Equals, "=", false),
            new Strings<Token>(Token.OpenBrace, "{", false),
            new Strings<Token>(Token.CloseBrace, "}", false),
            new Strings<Token>(Token.EndStatement, ";", false),
            new Section<Token>(Token.Text, "\"", new StringDefinitions().Add("\"").IncludeEOL(), false, false),
            new Section<Token>(Token.LongText, "\"\"\"", "\"\"\"", false, false),
            new Section<Token>(Token.Comment, "//", new StringDefinitions().IncludeEOL(), false, true),
            new Section<Token>(Token.Comment, "/*", "*/", false, true)
        );
    }

    private static void Verify(Token<Token> token, Token id, string? text, string? insideText, int line, int col)
    {
        token.ID.Should().Be(id);
        token.Text.Should().Be(text);
        if (insideText != null)
            token.InsideText.Should().Be(insideText);
        token.Line.Should().Be(line);
        token.Column.Should().Be(col);
    }

    [TestMethod]
    public void HelloWorld_Works()
    {
        var stream = _tokenizer.Tokenize("Hello world");

        stream.Count.Should().Be(2);
        Verify(stream[0], Token.Text, "Hello", "Hello", 1, 1);
        Verify(stream[1], Token.Text, "world", "world", 1, 7);
    }

    [TestMethod]
    public void KeywordInMiddle_Works()
    {
        var stream = _tokenizer.Tokenize("testmap");

        stream.Count.Should().Be(1);
        Verify(stream[0], Token.Text, "testmap", "testmap", 1, 1);
    }

    [TestMethod]
    public void MultiLineComments_Work()
    {
        var stream = _tokenizer.Tokenize(@"hey /* Comment goes here
and ends here */ bonk");

        stream.Count.Should().Be(2);
        Verify(stream[0], Token.Text, "hey", "hey", 1, 1);
        Verify(stream[1], Token.Text, "bonk", "bonk", 2, 18);
    }

    [TestMethod]
    public void SingleLineComments_Work()
    {
        var stream = _tokenizer.Tokenize(@"hey // Comment goes here
bonk");

        stream.Count.Should().Be(2);
        Verify(stream[0], Token.Text, "hey", "hey", 1, 1);
        Verify(stream[1], Token.Text, "bonk", "bonk", 2, 1);
    }

    [TestMethod]
    public void VarAssignment_Works()
    {
        var stream = _tokenizer.Tokenize("user=\"Hello world!\"");

        stream.Count.Should().Be(3);
        Verify(stream[0], Token.Text, "user", "user", 1, 1);
        Verify(stream[1], Token.Equals, "=", "=", 1, 5);
        Verify(stream[2], Token.Text, "\"Hello world!\"", "Hello world!", 1, 6);
    }
}