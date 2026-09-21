using DotNetCommons.Web.Elements;
using FluentAssertions;

namespace DotNetCommonTests.Web.Elements;

[TestClass]
public class HCommentTests
{
    [TestMethod]
    public void Constructor_WithoutText_LeavesTextNull()
    {
        var comment = new HComment();

        comment.Text.Should().BeNull();
    }

    [TestMethod]
    public void Constructor_WithText_SetsText()
    {
        var comment = new HComment("hello");

        comment.Text.Should().Be("hello");
    }

    [TestMethod]
    public void Clone_DeepCopiesTextAndChildren()
    {
        var original = new HComment("original");
        original.Children.Add(HText.Raw("original child"));

        var clone = (HComment)original.Clone();
        clone.Text = "clone";
        ((HText)clone.Children[0]).Content = "clone child";

        clone.Should().NotBeSameAs(original);
        clone.Children[0].Should().NotBeSameAs(original.Children[0]);
        original.Text.Should().Be("original");
        ((HText)original.Children[0]).Content.Should().Be("original child");
    }

    [TestMethod]
    public void Render_EncodesTextInsideHtmlComment()
    {
        var comment = new HComment("<script>alert('x')</script>");

        comment.Render().Should().Be("<!-- &lt;script&gt;alert(&#39;x&#39;)&lt;/script&gt; -->");
    }
}
