using System.Text;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace DotNetCommons.Web.Elements;

public class HtmlElement : HtmlNode
{
    public static readonly HashSet<string> VoidElements = new(StringComparer.OrdinalIgnoreCase)
    {
        "area",
        "base",
        "br",
        "col",
        "embed",
        "hr",
        "img",
        "input",
        "link",
        "meta",
        "param",
        "source",
        "track",
        "wbr",
    }; 
    
    private readonly StringBuilder _sb = new();

    public string? Name { get; set; }

    public IEnumerable<HtmlAttribute> Attributes => Children.OfType<HtmlAttribute>();
    public bool IsVoidElement => VoidElements.Contains(Name ?? "");

    public HtmlElement(string name)
    {
        Name = name;
    }

    public override string Render()
    {
        try
        {
            _sb.Append('<');
            _sb.Append(Name);
            foreach (var attr in Attributes)
                _sb.Append(' ').Append(attr.Render());

            _sb.Append('>');

            if (!IsVoidElement)
            {
                foreach (var node in Nodes)
                    _sb.Append(node.Render());

                _sb.Append("</");
                _sb.Append(Name);
                _sb.Append('>');
            }

            return _sb.ToString();
        }
        finally
        {
            _sb.Clear();
        }
    }

    public HtmlElement AddClass(string className)
    {
        if (HasClass(className))
            return this;

        var classAttribute = FindAttribute(className);
        if (classAttribute == null)
            Children.Add(classAttribute = new HtmlAttribute(className));

        classAttribute.Value += ' ' + className;
        return this;
    }

    public HtmlElement AddNode(HtmlNode? node)
    {
        Children.AddIfNotNull(node);
        return this;
    }

    public HtmlElement AddNodes(IEnumerable<HtmlNode?>? node)
    {
        Children.AddRangeIfNotNull(node);
        return this;
    }

    private void ClearContent()
    {
        var i = 0;
        while (i < Children.Count)
        {
            if (Children[i] is not HtmlAttribute)
                Children.RemoveAt(i);
            else
                i++;
        }
    }

    public HtmlAttribute? FindAttribute(string name)
    {
        return Attributes.FirstOrDefault(a => a.Name == name);
    }

    public string? GetAttribute(string name)
    {
        return FindAttribute(name)?.Value;
    }

    public bool HasClass(string className)
    {
        var classAttribute = FindAttribute(className);
        if (classAttribute == null)
            return false;

        var value = classAttribute.Value?.Trim();
        if (value.IsEmpty())
            return false;

        for (var i = 0; i < value.GetSubItemCount(' '); i++)
            if (value.GetSubItem(' ', i) == className)
                return true;

        return false;
    }

    public HtmlElement RemoveClass(string className)
    {
        var classAttribute = FindAttribute(className);
        if (classAttribute == null)
            return this;

        var value = classAttribute.Value?.Trim();
        if (value.IsEmpty())
            return this;

        var i = 0;
        while (i < value.GetSubItemCount(' '))
        {
            if (value.GetSubItem(' ', i) == className)
                value.RemoveSubItem(' ', i);
            else
                i++;
        }

        return this;
    }

    public HtmlElement RemoveNode(HtmlNode htmlNode)
    {
        Children.Remove(htmlNode);
        return this;
    }

    public HtmlElement SetAttribute(string name, string? value)
    {
        var attribute = FindAttribute(name);
        switch (attribute)
        {
            case null when value == null:
                return this;
            case null:
                Children.Add(new HtmlAttribute(name, value));
                return this;
            default:
                attribute.Value = value;
                return this;
        }
    }

    public HtmlElement SetHtml(string html)
    {
        ClearContent();
        Children.Add(new HtmlText(html));
        return this;
    }

    public HtmlElement SetText(string text)
    {
        ClearContent();
        Children.Add(new HtmlText(text));
        return this;
    }

    public HtmlString ToHtmlString()
    {
        return new HtmlString(Render());
    }

    public override string ToString()
    {
        return Render();
    }

    public void WriteTo(TagHelperOutput output)
    {
        output.TagName = Name;
        output.TagMode = TagMode.StartTagAndEndTag;

        foreach (var attr in Attributes)
            output.Attributes.SetAttribute(attr.Name, attr.Value);

        output.Content.SetHtmlContent(RenderChildren());
    }

    public static HtmlElement A(string? href = null, IEnumerable<HtmlNode?>? content = null) =>
        new HtmlElement("a")
            .SetAttribute("href", href)
            .AddNodes(content);

    public static HtmlElement Br() =>
        new HtmlElement("br");

    public static HtmlElement Div(IEnumerable<HtmlNode?>? content = null) =>
        new HtmlElement("div")
            .AddNodes(content);

    public static HtmlElement Img(string? src = null, string? alt = null, IEnumerable<HtmlNode?>? content = null) =>
        new HtmlElement("img")
            .SetAttribute("src", src)
            .SetAttribute("alt", alt)
            .AddNodes(content);

    public static HtmlElement H1(string? title = null, IEnumerable<HtmlNode?>? content = null) =>
        new HtmlElement("h1")
            .AddNode(title != null ? new HtmlText(title) : null)
            .AddNodes(content);

    public static HtmlElement H2(string? title = null, IEnumerable<HtmlNode?>? content = null) =>
        new HtmlElement("h2")
            .AddNode(title != null ? new HtmlText(title) : null)
            .AddNodes(content);

    public static HtmlElement H3(string? title = null, IEnumerable<HtmlNode?>? content = null) =>
        new HtmlElement("h3")
            .AddNode(title != null ? new HtmlText(title) : null)
            .AddNodes(content);

    public static HtmlElement H4(string? title = null, IEnumerable<HtmlNode?>? content = null) =>
        new HtmlElement("h4")
            .AddNode(title != null ? new HtmlText(title) : null)
            .AddNodes(content);

    public static HtmlElement H5(string? title = null, IEnumerable<HtmlNode?>? content = null) =>
        new HtmlElement("h5")
            .AddNode(title != null ? new HtmlText(title) : null)
            .AddNodes(content);

    public static HtmlElement H6(string? title = null, IEnumerable<HtmlNode?>? content = null) =>
        new HtmlElement("h6")
            .AddNode(title != null ? new HtmlText(title) : null)
            .AddNodes(content);
}