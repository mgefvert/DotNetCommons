using System.Web;

namespace DotNetCommons.Web.Elements;

public enum TextType
{
    Escape,
    RawHtml,
    Style,
    Script
}

public class HText : HNode
{
    public string? Content { get; set; }
    public TextType Type { get; set; }

    protected HText()
    {
    }

    public static HText Escape(string? text) => new() { Content = text, Type = TextType.Escape };
    public static HText Raw(string? text)    => new() { Content = text, Type = TextType.RawHtml };
    public static HText Style(string? text)  => new() { Content = text, Type = TextType.Style };
    public static HText Script(string? text) => new() { Content = text, Type = TextType.Script };

    public override string Render()
    {
        return Type == TextType.RawHtml
            ? Content ?? ""
            : HttpUtility.HtmlEncode(Content ?? "");
    }
}