using DotNetCommons.Web.Elements;
using FluentAssertions;

namespace DotNetCommonTests.Web.Elements;

[TestClass]
public class HTextTests
{
    [TestMethod]
    public void Constructor_Null_RendersEmptyString()
    {
        var text = HText.Escape(null);

        text.Content.Should().BeNull();
        text.Render().Should().Be("");
    }

    [TestMethod]
    public void Escape_Works()
    {
        var text = HText.Escape("<b>5 > 3 & 2</b>");
        text.Content.Should().Be("<b>5 > 3 & 2</b>");
        text.Render().Should().Be("&lt;b&gt;5 &gt; 3 &amp; 2&lt;/b&gt;");
    }

    [TestMethod]
    public void Render_EncodesHtml()
    {
        var text = HText.Raw("<strong>hi & bye</strong>");
        text.Content.Should().Be("<strong>hi & bye</strong>");
        text.Render().Should().Be("<strong>hi & bye</strong>");
    }

    [TestMethod]
    public void Clone_DeepCopiesContentTypeAndChildren()
    {
        var original = HText.Escape("original");
        original.Children.Add(new HComment("original child"));

        var clone = (HText)original.Clone();
        clone.Content = "clone";
        clone.Type = TextType.RawHtml;
        ((HComment)clone.Children[0]).Text = "clone child";

        clone.Should().NotBeSameAs(original);
        clone.Children[0].Should().NotBeSameAs(original.Children[0]);
        original.Content.Should().Be("original");
        original.Type.Should().Be(TextType.Escape);
        ((HComment)original.Children[0]).Text.Should().Be("original child");
    }
}
