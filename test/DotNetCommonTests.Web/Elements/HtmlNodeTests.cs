using DotNetCommons.Web.Elements;
using FluentAssertions;

namespace DotNetCommonTests.Web.Elements;

[TestClass]
public class HtmlNodeTests
{
    [TestMethod]
    public void Render_DefaultNode_ReturnsEmptyString()
    {
        var node = new HtmlNode();

        node.Render().Should().Be("");
    }

    [TestMethod]
    public void RenderChildren_EmptyChildren_ReturnsEmptyString()
    {
        var node = new HtmlNode();

        node.RenderChildren().Should().Be("");
    }

    [TestMethod]
    public void RenderChildren_WithMultipleChildren_ConcatenatesRenderedValuesInOrder()
    {
        var parent = new HtmlNode();
        parent.Children.Add(new HtmlText("first"));
        parent.Children.Add(new HtmlText("second"));
        parent.Children.Add(new HtmlText("third"));

        parent.RenderChildren().Should().Be("firstsecondthird");
    }
}
