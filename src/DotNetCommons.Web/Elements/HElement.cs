using System.Text;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace DotNetCommons.Web.Elements;

public class HElement : HNode
{
    private readonly StringBuilder _sb = new();

    public string? Name { get; set; }

    public IEnumerable<HAttribute> Attributes => Children.OfType<HAttribute>();
    public bool IsVoidElement => HTags.VoidElements.Contains(Name ?? "");

    public HElement(string name)
    {
        Name = name;
    }

    // --- Class ---

    private HAttribute? GetClassAttribute(bool create)
    {
        var classAttribute = FindAttr("class");
        if (classAttribute != null)
            return classAttribute;

        if (!create)
            return null;

        Children.Add(classAttribute = new HAttribute("class"));
        return classAttribute;
    }

    public HElement AddClass(string? className)
    {
        if (string.IsNullOrWhiteSpace(className))
            return this;

        var classAttribute = GetClassAttribute(true)!;

        var newClasses = new SortedSet<string>(className.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        var classList = new SortedSet<string>(classAttribute.Value?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? []);

        foreach (var c in newClasses)
            classList.Add(c);

        classAttribute.Value = string.Join(" ", classList);
        return this;
    }

    public bool HasClass(string className)
    {
        if (string.IsNullOrWhiteSpace(className))
            return false;

        var classAttribute = GetClassAttribute(false);
        if (classAttribute == null)
            return false;

        var newClasses = new SortedSet<string>(className.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        var classList = new SortedSet<string>(classAttribute.Value?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? []);

        return newClasses.All(classList.Contains);
    }

    public HElement RemoveClass(string className)
    {
        if (string.IsNullOrWhiteSpace(className))
            return this;

        var classAttribute = GetClassAttribute(false);
        if (classAttribute == null)
            return this;

        var removeClasses = new SortedSet<string>(className.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        var classList     = new SortedSet<string>(classAttribute.Value?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? []);

        foreach (var c in removeClasses)
            classList.Remove(c);

        classAttribute.Value = string.Join(" ", classList);
        return this;
    }

    // --- Attributes ---

    public HAttribute? FindAttr(string name)
    {
        return Attributes.FirstOrDefault(a => a.Name == name);
    }

    public string? Attr(string name)
    {
        return FindAttr(name)?.Value;
    }

    public HElement Attr(string name, string? value)
    {
        var attribute = FindAttr(name);
        switch (attribute)
        {
            case null when value == null:
                return this;
            case null:
                Children.Add(new HAttribute(name, value));
                return this;
            default:
                attribute.Value = value;
                return this;
        }
    }

    // --- Nodes ---

    public HElement AddNode(HNode? node)
    {
        Children.AddIfNotNull(node);
        return this;
    }

    public HElement AddNodes(IEnumerable<HNode?>? node)
    {
        Children.AddRangeIfNotNull(node);
        return this;
    }

    public HElement RemoveNode(HNode hNode)
    {
        Children.Remove(hNode);
        return this;
    }

    // --- Set content ---

    public void ClearContent()
    {
        var i = 0;
        while (i < Children.Count)
        {
            if (Children[i] is not HAttribute)
                Children.RemoveAt(i);
            else
                i++;
        }
    }

    // --- Render ---

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
        output.TagMode = IsVoidElement ? TagMode.StartTagOnly : TagMode.StartTagAndEndTag;

        foreach (var attr in Attributes)
            output.Attributes.SetAttribute(attr.Name, attr.Value);

        output.Content.SetHtmlContent(RenderChildren());
    }
}