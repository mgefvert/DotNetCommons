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

    [TestMethod]
    public void Clone_DeepCopiesChildren()
    {
        var original = new HNode();
        original.Children.Add(HText.Escape("original"));

        var clone = original.Clone();
        var clonedText = clone.Children.Should().ContainSingle().Subject.Should().BeOfType<HText>().Subject;
        clonedText.Content = "clone";
        clone.Children.Add(HText.Raw("new"));

        clone.Should().NotBeSameAs(original);
        clonedText.Should().NotBeSameAs(original.Children[0]);
        original.Children.Should().ContainSingle();
        ((HText)original.Children[0]).Content.Should().Be("original");
    }
}
