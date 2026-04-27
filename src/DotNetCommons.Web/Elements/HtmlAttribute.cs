using System.Web;

namespace DotNetCommons.Web.Elements;

public class HtmlAttribute : HtmlNode
{
    public string Name { get; }
    public string? Value { get; set; }

    public HtmlAttribute(string name)
    {
        Name = name;
    }

    public HtmlAttribute(string name, string value) : this(name)
    {
        Value = value;
    }

    public override string Render()
    {
        return Value == null ? Name : $"{Name}=\"{HttpUtility.HtmlAttributeEncode(Value)}\"";
    }
}