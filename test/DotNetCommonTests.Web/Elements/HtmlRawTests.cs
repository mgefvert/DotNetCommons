using DotNetCommons.Web.Elements;
using FluentAssertions;

namespace DotNetCommonTests.Web.Elements;

[TestClass]
public class HtmlRawTests
{
    [TestMethod]
    public void Constructor_WithoutContent_LeavesContentNullAndRendersEmptyString()
    {
        var html = new HtmlRaw();

        html.Content.Should().BeNull();
        html.Render().Should().Be("");
    }

    [TestMethod]
    public void Constructor_WithContent_SetsContent()
    {
        var html = new HtmlRaw("<strong>hi</strong>");

        html.Content.Should().Be("<strong>hi</strong>");
    }

    [TestMethod]
    public void Render_DoesNotEncodeHtml()
    {
        var html = new HtmlRaw("<strong>hi & bye</strong>");

        html.Render().Should().Be("<strong>hi & bye</strong>");
    }
}
