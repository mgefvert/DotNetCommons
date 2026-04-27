using System.Web;

namespace DotNetCommons.Web.Elements;

public class HtmlText : HtmlNode
{
    public string? Content { get; set; }

    public HtmlText()
    {
    }

    public HtmlText(string content)
    {
        Content = content;
    }

    public override string Render()
    {
        return HttpUtility.HtmlEncode(Content ?? "");
    }
}