using DotNetCommons.Web.Elements;
using FluentAssertions;

namespace DotNetCommonTests.Web.Elements;

[TestClass]
public class HAttributeTests
{
    [TestMethod]
    public void Constructor_NameOnly_SetsNameAndLeavesValueNull()
    {
        var attribute = new HAttribute("disabled");

        attribute.Name.Should().Be("disabled");
        attribute.Value.Should().BeNull();
    }

    [TestMethod]
    public void Constructor_NameAndValue_SetsNameAndValue()
    {
        var attribute = new HAttribute("href", "/products");

        attribute.Name.Should().Be("href");
        attribute.Value.Should().Be("/products");
    }

    [TestMethod]
    public void Clone_DeepCopiesValueAndChildren()
    {
        var original = new HAttribute("data-value", "original");
        original.Children.Add(HText.Raw("original child"));

        var clone = (HAttribute)original.Clone();
        clone.Value = "clone";
        ((HText)clone.Children[0]).Content = "clone child";
        clone.Children.Add(HText.Raw("new child"));

        clone.Should().NotBeSameAs(original);
        clone.Children[0].Should().NotBeSameAs(original.Children[0]);
        original.Value.Should().Be("original");
        ((HText)original.Children[0]).Content.Should().Be("original child");
        original.Children.Should().ContainSingle();
    }

    [TestMethod]
    public void Render_WithNullValue_RendersNameOnly()
    {
        var attribute = new HAttribute("required");

        attribute.Render().Should().Be("required");
    }

    [TestMethod]
    public void Render_WithValue_EncodesAndFormatsHtmlAttribute()
    {
        var attribute = new HAttribute("title", "Bob \"<Admin>\" & Co");

        attribute.Render().Should().Be("title=\"Bob &quot;&lt;Admin>&quot; &amp; Co\"");
    }
}
