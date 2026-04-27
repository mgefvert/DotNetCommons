namespace DotNetCommons.Web.Elements;

public class HNode
{
    public List<HNode> Children { get; } = [];
    public IEnumerable<HElement> Elements => Children.OfType<HElement>();
    public IEnumerable<HNode> Nodes => Children.Where(x => x is not HAttribute);

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