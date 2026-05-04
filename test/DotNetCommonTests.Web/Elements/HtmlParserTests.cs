using DotNetCommons.Web.Elements;
using FluentAssertions;

namespace DotNetCommonTests.Web.Elements;

[TestClass]
public class HtmlParserTests
{
    [TestMethod]
    public void Parse_EmptyString_ReturnsEmptyRootNode()
    {
        var root = HtmlParser.Parse("");

        root.Children.Should().BeEmpty();
        root.Render().Should().Be("");
    }

    [TestMethod]
    public void Parse_TextOnly_ReturnsRawTextNode()
    {
        var root = HtmlParser.Parse("Hello &lt; world");

        var text = root.Children.Should().ContainSingle().Subject.Should().BeOfType<HText>().Subject;
        text.Content.Should().Be("Hello &lt; world");
        text.Type.Should().Be(TextType.RawHtml);
    }

    [TestMethod]
    public void Parse_SimpleElement_ReturnsElementWithTextChild()
    {
        var root = HtmlParser.Parse("<p>Hello</p>");

        var paragraph = root.Children.Should().ContainSingle().Subject.Should().BeOfType<HElement>().Subject;
        paragraph.Name.Should().Be("p");
        paragraph.Children.Should().ContainSingle().Which.Should().BeOfType<HText>()
            .Which.Content.Should().Be("Hello");
    }

    [TestMethod]
    public void Parse_NestedElements_KeepsChildOrder()
    {
        var root = HtmlParser.Parse("<div>Hi <span>there</span><br>now</div>");

        var div = root.Children.Should().ContainSingle().Subject.Should().BeOfType<HElement>().Subject;
        div.Name.Should().Be("div");
        div.Children.Should().HaveCount(4);
        div.Children[0].Should().BeOfType<HText>().Which.Content.Should().Be("Hi ");
        div.Children[1].Should().BeOfType<HElement>().Which.Name.Should().Be("span");
        div.Children[2].Should().BeOfType<HElement>().Which.Name.Should().Be("br");
        div.Children[3].Should().BeOfType<HText>().Which.Content.Should().Be("now");
    }

    [TestMethod]
    public void Parse_Fragment_ReturnsRootWithMultipleChildren()
    {
        var root = HtmlParser.Parse("<h1>Title</h1><p>Body</p>");

        root.Children.Should().HaveCount(2);
        root.Children[0].Should().BeOfType<HElement>().Which.Name.Should().Be("h1");
        root.Children[1].Should().BeOfType<HElement>().Which.Name.Should().Be("p");
    }

    [TestMethod]
    public void Parse_Attributes_HandlesQuotedUnquotedAndBooleanAttributes()
    {
        var root = HtmlParser.Parse("<input type=\"text\" value='hello world' disabled data-id=42>");

        var input = root.Children.Should().ContainSingle().Subject.Should().BeOfType<HElement>().Subject;
        input.Attr("type").Should().Be("text");
        input.Attr("value").Should().Be("hello world");
        input.Attr("data-id").Should().Be("42");
        input.FindAttr("disabled").Should().NotBeNull();
        input.Attr("disabled").Should().BeNull();
    }

    [TestMethod]
    public void Parse_SelfClosingElement_DoesNotConsumeFollowingNodes()
    {
        var root = HtmlParser.Parse("<div><span />after</div>");

        var div = root.Children.Should().ContainSingle().Subject.Should().BeOfType<HElement>().Subject;
        div.Children.Should().HaveCount(2);
        div.Children[0].Should().BeOfType<HElement>().Which.Name.Should().Be("span");
        div.Children[1].Should().BeOfType<HText>().Which.Content.Should().Be("after");
    }

    [TestMethod]
    public void Parse_Comments_ReturnsCommentNode()
    {
        var root = HtmlParser.Parse("<div><!-- note --><span>x</span></div>");

        var div = root.Children.Should().ContainSingle().Subject.Should().BeOfType<HElement>().Subject;
        div.Children[0].Should().BeOfType<HComment>().Which.Text.Should().Be("note");
    }

    [TestMethod]
    public void Parse_Render_RoundTripsSimpleFragment()
    {
        const string html = "<div class=\"a b\">Hi <span data-id=\"42\">there</span><br></div>";

        HtmlParser.Parse(html).Render().Should().Be(html);
    }

    [TestMethod]
    public void Parse_MismatchedClosingTag_ThrowsFormatException()
    {
        var action = () => HtmlParser.Parse("<div><span>x</div>");

        action.Should().Throw<FormatException>();
    }

    [TestMethod]
    public void Parse_MissingClosingTag_ThrowsFormatException()
    {
        var action = () => HtmlParser.Parse("<div><span>x</span>");

        action.Should().Throw<FormatException>();
    }
}
