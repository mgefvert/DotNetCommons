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
}
