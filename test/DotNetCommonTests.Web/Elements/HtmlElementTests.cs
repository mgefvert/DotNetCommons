using DotNetCommons.Web.Elements;
using FluentAssertions;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace DotNetCommonTests.Web.Elements;

[TestClass]
public class HtmlElementTests
{
    [TestMethod]
    public void Constructor_SetsElementName()
    {
        var element = new HtmlElement("section");

        element.Name.Should().Be("section");
    }

    [TestMethod]
    public void Render_WithAttributes_RendersOpeningTagWithEncodedAttributes()
    {
        var element = new HtmlElement("a")
            .SetAttribute("href", "/home")
            .SetAttribute("title", "Bob \"<Admin>\"");

        element.Render().Should().Be("<a href=\"/home\" title=\"Bob &quot;&lt;Admin>&quot;\"></a>");
    }

    [TestMethod]
    public void RenderChildren_ComposesChildNodeOutputInOrder()
    {
        var element = new HtmlElement("div")
            .AddNode(new HtmlText("Start"))
            .AddNode(new HtmlRaw("<span>raw</span>"))
            .AddNode(new HtmlComment("Done"));

        element.RenderChildren().Should().Be("Start<span>raw</span><!-- Done -->");
    }

    [TestMethod]
    public void Attributes_And_Elements_FilterChildrenByType()
    {
        var child = new HtmlElement("span");
        var element = new HtmlElement("div")
            .SetAttribute("id", "root")
            .AddNode(child)
            .AddNode(new HtmlText("x"));

        element.Attributes.Should().ContainSingle(a => a.Name == "id" && a.Value == "root");
        element.Elements.Should().ContainSingle(e => ReferenceEquals(e, child));
    }

    [TestMethod]
    public void AddNode_WithNull_DoesNotMutateChildren()
    {
        var element = new HtmlElement("div");

        element.AddNode(null);

        element.Children.Should().BeEmpty();
    }

    [TestMethod]
    public void AddNodes_WithNulls_AddsOnlyNonNullNodes()
    {
        var t1 = new HtmlText("A");
        var t2 = new HtmlText("B");
        HtmlNode?[] nodes = [t1, null, t2, null];

        var element = new HtmlElement("div").AddNodes(nodes);

        element.Children.Should().HaveCount(2);
        element.Children.Should().ContainInOrder(t1, t2);
    }

    [TestMethod]
    public void RemoveNode_RemovesExistingNode()
    {
        var child = new HtmlText("A");
        var element = new HtmlElement("div").AddNode(child);

        element.RemoveNode(child);

        element.Children.Should().BeEmpty();
    }

    [TestMethod]
    public void SetAttribute_AddsUpdatesAndKeepsExistingWhenSettingNull()
    {
        var element = new HtmlElement("input")
            .SetAttribute("type", "text")
            .SetAttribute("type", "password")
            .SetAttribute("type", null);

        element.FindAttribute("type").Should().NotBeNull();
        element.GetAttribute("type").Should().BeNull();
    }

    [TestMethod]
    public void FindAttribute_And_GetAttribute_ReturnNullWhenMissing()
    {
        var element = new HtmlElement("div");

        element.FindAttribute("id").Should().BeNull();
        element.GetAttribute("id").Should().BeNull();
    }

    [TestMethod]
    public void SetText_ClearsNonAttributeChildrenAndReplacesWithEncodedTextNode()
    {
        var element = new HtmlElement("div")
            .SetAttribute("id", "one")
            .AddNode(new HtmlRaw("<b>remove me</b>"))
            .SetText("<b>keep as text</b>");

        element.Children.Should().HaveCount(2);
        element.Attributes.Should().ContainSingle(a => a.Name == "id");
        element.RenderChildren().Should().Contain("&lt;b&gt;keep as text&lt;/b&gt;");
        element.RenderChildren().Should().NotContain("<b>remove me</b>");
    }

    [TestMethod]
    public void SetHtml_ClearsNonAttributeChildrenAndStoresHtmlInTextNode()
    {
        var element = new HtmlElement("div")
            .SetAttribute("id", "one")
            .AddNode(new HtmlRaw("<b>remove me</b>"))
            .SetHtml("<i>html</i>");

        element.Children.Should().HaveCount(2);
        element.Attributes.Should().ContainSingle(a => a.Name == "id");
        element.Children.OfType<HtmlText>().Should().ContainSingle(t => t.Content == "<i>html</i>");
    }

    [TestMethod]
    public void ToHtmlString_And_ToString_ReturnRenderOutput()
    {
        var element = new HtmlElement("hr");

        element.ToString().Should().Be("<hr>");
        element.ToHtmlString().ToString().Should().Be("<hr>");
    }

    [TestMethod]
    public void WriteTo_SetsTagNameTagModeAndAttributes()
    {
        var output = CreateTagHelperOutput();
        var element = new HtmlElement("section")
            .SetAttribute("id", "hero")
            .SetAttribute("data-kind", "banner");

        element.WriteTo(output);

        output.TagName.Should().Be("section");
        output.TagMode.Should().Be(TagMode.StartTagAndEndTag);
        output.Attributes["id"].Value.Should().Be("hero");
        output.Attributes["data-kind"].Value.Should().Be("banner");
    }

    [TestMethod]
    public void WriteTo_WritesRenderedChildrenIntoContent()
    {
        var output = CreateTagHelperOutput();
        var element = new HtmlElement("section")
            .AddNode(new HtmlText("safe <b>text</b>"))
            .AddNode(new HtmlRaw("<em>raw</em>"));

        element.WriteTo(output);

        output.Content.GetContent().Should().Be("safe &lt;b&gt;text&lt;/b&gt;<em>raw</em>");
    }

    [TestMethod]
    public void FactoryMethods_CreateExpectedShallowElements()
    {
        HtmlElement.A("/home", [new HtmlText("Home")]).Render()
            .Should().Be("<a href=\"/home\">Home</a>");

        HtmlElement.Br().Render()
            .Should().Be("<br>");

        HtmlElement.Div().Render()
            .Should().Be("<div></div>");
        HtmlElement.Div([new HtmlText("content")]).Render()
            .Should().Be("<div>content</div>");

        HtmlElement.Img("/img/pic.png", "Picture").Render()
            .Should().Be("<img src=\"/img/pic.png\" alt=\"Picture\">");

        HtmlElement.H1("Title").Render().Should().Be("<h1>Title</h1>");
        HtmlElement.H2("Sub").Render().Should().Be("<h2>Sub</h2>");
        HtmlElement.H3("Sub").Render().Should().Be("<h3>Sub</h3>");
        HtmlElement.H4("Sub").Render().Should().Be("<h4>Sub</h4>");
        HtmlElement.H5("Sub").Render().Should().Be("<h5>Sub</h5>");
        HtmlElement.H6("Sub").Render().Should().Be("<h6>Sub</h6>");
    }

    private static TagHelperOutput CreateTagHelperOutput()
    {
        return new TagHelperOutput(
            "ignored",
            new TagHelperAttributeList(),
            (_, _) =>
            {
                TagHelperContent content = new DefaultTagHelperContent();
                return Task.FromResult(content);
            });
    }
}
