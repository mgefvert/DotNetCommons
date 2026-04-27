namespace DotNetCommons.Web.Elements;

public class HtmlRaw : HtmlNode
{
    public string? Content { get; set; }

    public HtmlRaw()
    {
    }

    public HtmlRaw(string htmlContent)
    {
        Content = htmlContent;
    }

    public override string Render()
    {
        return Content ?? "";
    }
}