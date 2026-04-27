using DotNetCommons.Web.Elements;
using FluentAssertions;

namespace DotNetCommonTests.Web.Elements;

[TestClass]
public class HNodeTests
{
    [TestMethod]
    public void Render_DefaultNode_ReturnsEmptyString()
    {
        var node = new HNode();
        node.Render().Should().Be("");
        node.RenderChildren().Should().Be("");
    }

    [TestMethod]
    public void Render_ConcatenatesInOrder()
    {
        var parent = new HNode();
        parent.Children.Add(HText.Escape("first"));
        parent.Children.Add(HText.Escape("second"));
        parent.Children.Add(HText.Escape("third"));

        parent.Render().Should().Be("firstsecondthird");
    }
}
