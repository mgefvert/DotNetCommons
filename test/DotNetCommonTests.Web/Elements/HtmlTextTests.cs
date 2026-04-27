using DotNetCommons.Web.Elements;
using FluentAssertions;

namespace DotNetCommonTests.Web.Elements;

[TestClass]
public class HtmlTextTests
{
    [TestMethod]
    public void Constructor_WithoutContent_LeavesContentNullAndRendersEmptyString()
    {
        var text = new HtmlText();

        text.Content.Should().BeNull();
        text.Render().Should().Be("");
    }

    [TestMethod]
    public void Constructor_WithContent_SetsContent()
    {
        var text = new HtmlText("hello");

        text.Content.Should().Be("hello");
    }

    [TestMethod]
    public void Render_EncodesHtml()
    {
        var text = new HtmlText("<b>5 > 3 & 2</b>");

        text.Render().Should().Be("&lt;b&gt;5 &gt; 3 &amp; 2&lt;/b&gt;");
    }
}
