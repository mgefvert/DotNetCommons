using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Html;

namespace DotNetCommons.Web.Html;

public static class DebugJson
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        WriteIndented = true
    };

    public static HtmlString Serialize(object obj)
    {
        return new HtmlString("<pre style=\"text-align: left\">" + JsonSerializer.Serialize(obj, SerializerOptions) + "</pre>");
    }
}