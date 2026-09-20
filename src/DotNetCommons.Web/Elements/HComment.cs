using System.Web;

namespace DotNetCommons.Web.Elements;

public class HComment : HNode
{
    public string? Text { get; set; }

    public HComment()
    {
    }

    public HComment(string text)
    {
        Text = text;
    }

    public override HNode Clone()
    {
        return new HComment
        {
            Text = Text,
            Children = Children.Select(x => x.Clone()).ToList()
        };
    }

    public override string Render()
    {
        return $"<!-- {HttpUtility.HtmlEncode(Text)} -->";
    }
}