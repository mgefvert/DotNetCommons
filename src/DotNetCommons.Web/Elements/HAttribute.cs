using System.Web;

namespace DotNetCommons.Web.Elements;

public class HAttribute : HNode
{
    public string Name { get; }
    public string? Value { get; set; }

    public HAttribute(string name)
    {
        Name = name;
    }

    public HAttribute(string name, string value) : this(name)
    {
        Value = value;
    }

    public override string Render()
    {
        return Value == null ? Name : $"{Name}=\"{HttpUtility.HtmlAttributeEncode(Value)}\"";
    }
}