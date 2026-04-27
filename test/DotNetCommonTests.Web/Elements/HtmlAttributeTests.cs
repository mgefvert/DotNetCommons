using DotNetCommons.Web.Elements;
using FluentAssertions;

namespace DotNetCommonTests.Web.Elements;

[TestClass]
public class HtmlAttributeTests
{
    [TestMethod]
    public void Constructor_NameOnly_SetsNameAndLeavesValueNull()
    {
        var attribute = new HtmlAttribute("disabled");

        attribute.Name.Should().Be("disabled");
        attribute.Value.Should().BeNull();
    }

    [TestMethod]
    public void Constructor_NameAndValue_SetsNameAndValue()
    {
        var attribute = new HtmlAttribute("href", "/products");

        attribute.Name.Should().Be("href");
        attribute.Value.Should().Be("/products");
    }

    [TestMethod]
    public void Render_WithNullValue_RendersNameOnly()
    {
        var attribute = new HtmlAttribute("required");

        attribute.Render().Should().Be("required");
    }

    [TestMethod]
    public void Render_WithValue_EncodesAndFormatsHtmlAttribute()
    {
        var attribute = new HtmlAttribute("title", "Bob \"<Admin>\" & Co");

        attribute.Render().Should().Be("title=\"Bob &quot;&lt;Admin>&quot; &amp; Co\"");
    }
}
