using DotNetCommons.Web.Elements;
using FluentAssertions;

namespace DotNetCommonTests.Web.Elements;

[TestClass]
public class HTagsTests
{
    [TestMethod]
    public void FactoryMethods_CreateExpectedShallowElements()
    {
        HTags.A("/home", HText.Escape("Home")).Render().Should().Be("<a href=\"/home\">Home</a>");
        HTags.Br().Render().Should().Be("<br>");
        HTags.Div().Render().Should().Be("<div></div>");
        HTags.Div(HText.Escape("content")).Render().Should().Be("<div>content</div>");
        HTags.Img("/img/pic.png", "Picture").Render().Should().Be("<img src=\"/img/pic.png\" alt=\"Picture\">");
        HTags.H1("Title").Render().Should().Be("<h1>Title</h1>");
        HTags.H2("Sub").Render().Should().Be("<h2>Sub</h2>");
        HTags.H3("Sub").Render().Should().Be("<h3>Sub</h3>");
    }
}
