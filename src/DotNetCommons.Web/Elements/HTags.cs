namespace DotNetCommons.Web.Elements;

public static class HTags
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

    // Void elements

    public static HElement Base(string href) => New("base").Attr("href", href);
    public static HElement Br() => New("br");
    public static HElement Col() => New("col");
    public static HElement Hr() => New("hr");
    public static HElement Img() => New("img");
    public static HElement Img(string src, string? alt = null) => New("img").Attr("src", src).Attr("alt", alt);
    public static HElement Link() => New("link");
    public static HElement Link(string href, string rel) => New("link").Attr("href", href).Attr("rel", rel);
    public static HElement Meta() => New("meta");
    public static HElement Wbr() => New("wbr");

    // Content elements

    public static HElement A(string? href, params HNode?[] content) => New("a", content).Attr("href", href);
    public static HElement Div(params HNode?[] content) => New("div", content);
    public static HElement H1(string title) => New("h1", [HText.Escape(title)]);
    public static HElement H1(params HNode?[] content) => New("h1", content);
    public static HElement H2(string title) => New("h2", [HText.Escape(title)]);
    public static HElement H2(params HNode?[] content) => New("h2", content);
    public static HElement H3(string title) => New("h3", [HText.Escape(title)]);
    public static HElement H3(params HNode?[] content) => New("h3", content);
    public static HElement Label(string label) => New("label", [HText.Escape(label)]);
    public static HElement Label(params HNode?[] content) => New("label", content);
    public static HElement Li(params HNode?[] content) => New("li", content);
    public static HElement Ol(params HNode?[] content) => New("ol", content);
    public static HElement Script(string script) => New("script", [HText.Script(script)]);
    public static HElement Span(params HNode?[] content) => New("span", content);
    public static HElement Style(string style) => New("style", [HText.Style(style)]);
    public static HElement Ul(params HNode?[] content) => New("ul", content);

    private static HElement New(string tag, HNode?[]? content = null)
    {
        var result = new HElement(tag);
        result.AddNodes(content);

        return result;
    }
}