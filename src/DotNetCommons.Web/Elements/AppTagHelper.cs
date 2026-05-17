using Microsoft.AspNetCore.Razor.TagHelpers;

namespace DotNetCommons.Web.Elements;

public abstract class AppTagHelper : TagHelper
{
    private TagHelperOutput? _output;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        _output = output;

        var result = await Render();
        if (result == null)
            return;

        output.TagName = result.Name;
        output.TagMode = result.IsVoidElement ? TagMode.StartTagOnly : TagMode.StartTagAndEndTag;

        foreach (var attr in result.Attributes)
            output.Attributes.SetAttribute(attr.Name, attr.Value);

        output.Content.SetHtmlContent(result.RenderChildren());
    }

    protected async Task<HText?> ReadTagContents()
    {
        if (_output == null)
            return null;

        var contents  = await _output.GetChildContentAsync();
        var contents2 = contents?.GetContent();
        return HText.Raw(contents2 ?? "");
    }

    public abstract Task<HElement?> Render();
}