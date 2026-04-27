using DotNetCommons.Web.Elements;
using FluentAssertions;

namespace DotNetCommonTests.Web.Elements;

[TestClass]
public class HtmlCommentTests
{
    [TestMethod]
    public void Constructor_WithoutText_LeavesTextNull()
    {
        var comment = new HtmlComment();

        comment.Text.Should().BeNull();
    }

    [TestMethod]
    public void Constructor_WithText_SetsText()
    {
        var comment = new HtmlComment("hello");

        comment.Text.Should().Be("hello");
    }

    [TestMethod]
    public void Render_EncodesTextInsideHtmlComment()
    {
        var comment = new HtmlComment("<script>alert('x')</script>");

        comment.Render().Should().Be("<!-- &lt;script&gt;alert(&#39;x&#39;)&lt;/script&gt; -->");
    }
}
