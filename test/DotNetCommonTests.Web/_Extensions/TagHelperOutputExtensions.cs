using DotNetCommons.Web.Elements;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace DotNetCommonTests.Web;

public static class TagHelperExtensions
{
    public static async Task<HText> ToHText(this TagHelperOutput output)
    {
        var contents = await output.GetChildContentAsync();
        var contents2 = contents?.GetContent();
        return HText.Raw(contents2 ?? "");
    }
}