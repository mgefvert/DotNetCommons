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
            new Characters<Token>(TokenMode.Any, Token.Text, false),
            new Characters<Token>(TokenMode.Whitespace, Token.None, true),
            new Strings<Token>("include", Token.Keyword, false),
            new Strings<Token>("location", Token.Keyword, false),
            new Strings<Token>("job", Token.Keyword, false),
            new Strings<Token>("map", Token.Keyword, false),
            new Strings<Token>("memory", Token.Keyword, false),
            new Strings<Token>("=", Token.Equals, false),
            new Strings<Token>("{", Token.OpenBrace, false),
            new Strings<Token>("}", Token.CloseBrace, false),
            new Strings<Token>(";", Token.EndStatement, false),
            new Section<Token>("\"", new[] { "\"", "\r\n", "\r", "\n" }, false, Token.Text, false),
            new Section<Token>("\"\"\"", "\"\"\"", false, Token.LongText, false),
            new Strings<Token>("\r\n", Token.EndOfLine, false),
            new Strings<Token>("\r", Token.EndOfLine, false),
            new Strings<Token>("\n", Token.EndOfLine, false),
            new Section<Token>("//", new[] { "\r\n", "\r", "\n" }, false, Token.Comment, true).WithAppend(Token.EndOfLine),
            new Section<Token>("/*", "*/", false, Token.Comment, true)
        );
    }

    private void Verify(Token<Token> token, Token id, string? text, string? insideText = null)
    {
        token.ID.Should().Be(id);
        token.Text.Should().Be(text);
        if (insideText != null)
            token.InsideText.Should().Be(insideText);
    }

    [TestMethod]
    public void HelloWorld_Works()
    {
        var stream = _tokenizer.Tokenize("Hello world!");

        stream.Count.Should().Be(2);
        Verify(stream[0], Token.Text, "Hello");
        Verify(stream[1], Token.Text, "world!");
    }

    [TestMethod]
    public void VarAssignment_Works()
    {
        var stream = _tokenizer.Tokenize("user=\"Hello world!\"");

        stream.Count.Should().Be(3);
        Verify(stream[0], Token.Text, "user");
        Verify(stream[1], Token.Equals, "=");
        Verify(stream[2], Token.Text, "\"Hello world!\"", "Hello world!");
    }

    [TestMethod]
    public void MultiLineComments_Work()
    {
        var stream = _tokenizer.Tokenize(@"hey /* Comment goes here
and ends here */ bonk");

        stream.Count.Should().Be(2);
        Verify(stream[0], Token.Text, "hey");
        Verify(stream[1], Token.Text, "bonk");
    }

    [TestMethod]
    public void SingleLineComments_Work()
    {
        var stream = _tokenizer.Tokenize(@"hey // Comment goes here
bonk");

        stream.Count.Should().Be(3);
        Verify(stream[0], Token.Text, "hey");
        Verify(stream[1], Token.EndOfLine, null);
        Verify(stream[2], Token.Text, "bonk");
    }
}