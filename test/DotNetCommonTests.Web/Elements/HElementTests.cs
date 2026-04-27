using DotNetCommons.Web.Elements;
using FluentAssertions;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace DotNetCommonTests.Web.Elements;

[TestClass]
public class HElementTests
{
    [TestMethod]
    public void Constructor_SetsElementName()
    {
        var element = new HElement("section");

        element.Name.Should().Be("section");
    }

    [TestMethod]
    public void AddClass_Works()
    {
        var element = new HElement("a");
        element.Attr("class").Should().BeNull();

        element.AddClass("test");
        element.Attr("class").Should().Be("test");

        element.AddClass("ng prime block");
        element.Attr("class").Should().Be("block ng prime test");

        element.AddClass("x block test");
        element.Attr("class").Should().Be("block ng prime test x");
    }

    [TestMethod]
    public void HasClass_Works()
    {
        var element = new HElement("a");
        element.HasClass("test").Should().BeFalse();

        element = new HElement("a").Attr("class", "block bg prime");
        element.HasClass("test").Should().BeFalse();
        element.HasClass("block").Should().BeTrue();
        element.HasClass("bg").Should().BeTrue();
        element.HasClass("block prime").Should().BeTrue();
    }

    [TestMethod]
    public void RemoveClass_Works()
    {
        var element = new HElement("a").RemoveClass("test");
        element.Attr("class").Should().BeNull();

        element = new HElement("a").Attr("class", "block bg prime x test");
        element.RemoveClass("block");
        element.Attr("class").Should().Be("bg prime test x");

        element.RemoveClass("bg x");
        element.Attr("class").Should().Be("prime test");
    }

    [TestMethod]
    public void Render_WithAttributes_RendersOpeningTagWithEncodedAttributes()
    {
        var element = new HElement("a")
            .Attr("href", "/home")
            .Attr("title", "Bob \"<Admin>\"");

        element.Render().Should().Be("<a href=\"/home\" title=\"Bob &quot;&lt;Admin>&quot;\"></a>");
    }

    [TestMethod]
    public void RenderChildren_ComposesChildNodeOutputInOrder()
    {
        var element = new HElement("div")
            .AddNode(HText.Escape("Start"))
            .AddNode(HText.Raw("<span>raw</span>"))
            .AddNode(new HComment("Done"));

        element.RenderChildren().Should().Be("Start<span>raw</span><!-- Done -->");
    }

    [TestMethod]
    public void Attributes_And_Elements_FilterChildrenByType()
    {
        var child = new HElement("span");
        var element = new HElement("div")
            .Attr("id", "root")
            .AddNode(child)
            .AddNode(HText.Escape("x"));

        element.Attributes.Should().ContainSingle(a => a.Name == "id" && a.Value == "root");
        element.Elements.Should().ContainSingle(e => ReferenceEquals(e, child));
    }

    [TestMethod]
    public void AddNode_WithNull_DoesNotMutateChildren()
    {
        var element = new HElement("div");

        element.AddNode(null);

        element.Children.Should().BeEmpty();
    }

    [TestMethod]
    public void AddNodes_WithNulls_AddsOnlyNonNullNodes()
    {
        var t1 = HText.Escape("A");
        var t2 = HText.Escape("B");
        HNode?[] nodes = [t1, null, t2, null];

        var element = new HElement("div").AddNodes(nodes);

        element.Children.Should().HaveCount(2);
        element.Children.Should().ContainInOrder(t1, t2);
    }

    [TestMethod]
    public void RemoveNode_RemovesExistingNode()
    {
        var child = HText.Escape("A");
        var element = new HElement("div").AddNode(child);

        element.RemoveNode(child);

        element.Children.Should().BeEmpty();
    }

    [TestMethod]
    public void SetAttribute_AddsUpdatesAndKeepsExistingWhenSettingNull()
    {
        var element = new HElement("input")
            .Attr("type", "text")
            .Attr("type", "password")
            .Attr("type", null);

        element.FindAttr("type").Should().NotBeNull();
        element.Attr("type").Should().BeNull();
    }

    [TestMethod]
    public void FindAttribute_And_GetAttribute_ReturnNullWhenMissing()
    {
        var element = new HElement("div");

        element.FindAttr("id").Should().BeNull();
        element.Attr("id").Should().BeNull();
    }

    [TestMethod]
    public void ToHtmlString_And_ToString_ReturnRenderOutput()
    {
        var element = new HElement("hr");

        element.ToString().Should().Be("<hr>");
        element.ToHtmlString().ToString().Should().Be("<hr>");
    }

    [TestMethod]
    public void WriteTo_SetsTagNameTagModeAndAttributes()
    {
        var output = CreateTagHelperOutput();
        var element = new HElement("section")
            .Attr("id", "hero")
            .Attr("data-kind", "banner");

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
        var element = new HElement("section")
            .AddNode(HText.Escape("safe <b>text</b>"))
            .AddNode(HText.Raw("<em>raw</em>"));

        element.WriteTo(output);

        output.Content.GetContent().Should().Be("safe &lt;b&gt;text&lt;/b&gt;<em>raw</em>");
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
