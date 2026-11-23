using System.Xml;
using System.Xml.Linq;
using Microsoft.AspNetCore.Components;

namespace DotNetCommons.Blazor.Components;

public sealed class AppIconProvider
{
    // ReSharper disable once CollectionNeverQueried.Global MemberCanBePrivate.Global
    public Dictionary<string, string> Icons { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public IEnumerable<string?> Names => Icons.Keys;

    public AppIconProvider(string iconDirectory)
    {
        if (!Directory.Exists(iconDirectory))
            return;

        foreach (var file in Directory.EnumerateFiles(iconDirectory, "*.svg"))
        {
            using var reader = XmlReader.Create(file,
                new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null });

            var document = XDocument.Load(reader);
            var svg      = document.Root;
            if (svg?.Name.LocalName != "svg" || svg.Name.NamespaceName != "http://www.w3.org/2000/svg")
                throw new InvalidDataException($"Icon '{file}' must contain an SVG root element.");

            svg.SetAttributeValue("class", "h-full w-full");
            svg.SetAttributeValue("aria-hidden", "true");
            svg.SetAttributeValue("focusable", "false");

            Icons.Add(Path.GetFileNameWithoutExtension(file), svg.ToString(SaveOptions.DisableFormatting));
        }
    }

    public MarkupString? Get(string name)
    {
        return Icons.TryGetValue(name, out var svg) ? new MarkupString(svg) : null;
    }
}
