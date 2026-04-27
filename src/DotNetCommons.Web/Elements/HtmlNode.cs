namespace DotNetCommons.Web.Elements;

public class HtmlNode
{
    public List<HtmlNode> Children { get; } = [];
    public IEnumerable<HtmlElement> Elements => Children.OfType<HtmlElement>();
    public IEnumerable<HtmlNode> Nodes => Children.Where(x => x is not HtmlAttribute);

    public bool HasNodes => Nodes.Any();

    public virtual string Render()
    {
        return RenderChildren();
    }

    public virtual string RenderChildren()
    {
        return string.Join("", Nodes.Select(x => x.Render()));
    }
}