using System.Web;

namespace DotNetCommons.Web.Elements;

public class HtmlComment : HtmlNode
{
    public string? Text { get; set; }

    public HtmlComment()
    {
    }

    public HtmlComment(string text)
    {
        Text = text;
    }

    public override string Render()
    {
        return $"<!-- {HttpUtility.HtmlEncode(Text)} -->";
    }
}