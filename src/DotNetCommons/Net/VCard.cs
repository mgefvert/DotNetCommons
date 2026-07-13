using System.Text;
using System.Text.Json;

namespace DotNetCommons.Net;

public class VCard
{
    public string? Version { get; set; }
    public string? FormattedName { get; set; }
    public string? Kind { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Org { get; set; }
    public string? Title { get; set; }
    public string? Role { get; set; }
    public string? Url { get; set; }

    public string? Country
    {
        get
        {
            if (Address.IsEmpty())
                return null;

            var n = Address.LastIndexOfAny('\r', '\n');
            if (n < 0)
                return null;

            return Address[(n + 1)..].Trim();
        }
    }

    public Dictionary<string, string> Others { get; } = [];

    public void Set(string type, string value)
    {
        switch (type)
        {
            case "version": Version       = value; break;
            case "fn":      FormattedName = value; break;
            case "kind":    Kind          = value; break;
            case "email":   Email         = value; break;
            case "tel":     Phone         = value; break;
            case "adr":     Address       = value; break;
            case "org":     Org           = value; break;
            case "title":   Title         = value; break;
            case "role":    Role          = value; break;
            case "url":     Url           = value; break;
            default:        Others[type]  = value; break;
        }
    }

    public override string ToString()
    {
        var sb = new StringBuilder(40);
        sb.Append("ver");
        sb.Append(Version);

        Append(", kind=", Kind);
        Append(", fn=", FormattedName);
        Append(", org=", Org);
        Append(", adr=", Address);
        Append(", email=", Email);
        Append(", phone=", Phone);
        Append(", title=", Title);
        Append(", role=", Role);
        Append(", url=", Url);

        return sb.ToString();

        void Append(string name, string? value)
        {
            if (value.IsSet())
                sb.Append(name).Append(value);
        }
    }

    public static List<VCard> ParseVCardArray(List<JsonElement> vcardArray)
    {
        if (vcardArray[0].ValueKind != JsonValueKind.String)
            throw new Exception("Expected vCardArray to start with a single 'vcard' element.");
        if (vcardArray[0].GetString() != "vcard")
            throw new Exception("Expected vCardArray to start with a single 'vcard' element.");

        var results = new List<VCard>();
        foreach (var vcardData in vcardArray.Skip(1))
            results.AddIfNotNull(ParseVCard(vcardData));

        return results;
    }

    public static VCard ParseVCard(JsonElement vcardData)
    {
        var result = new VCard();

        foreach (var value in vcardData.EnumerateArray())
        {
            var type = value[0].GetString()!;
            if (value[2].GetString() != "text")
                continue;

            string? data;

            if (value[3].ValueKind == JsonValueKind.String)
            {
                var subvalues = value.EnumerateArray()
                    .Skip(3)
                    .Where(x => x.ValueKind == JsonValueKind.String)
                    .Select(x => x.GetString()!)
                    .Where(x => x.IsSet());
                data = string.Join(", ", subvalues);
                if (data.IsSet())
                {
                    result.Set(type, data);
                    continue;
                }
            }

            if (value[3].ValueKind == JsonValueKind.Array)
            {
                var subvalues = value[3].EnumerateArray()
                    .Where(x => x.ValueKind == JsonValueKind.String)
                    .Select(x => x.GetString()!)
                    .Where(x => x.IsSet());
                data = string.Join(", ", subvalues);
                if (data.IsSet())
                {
                    result.Set(type, data);
                    continue;
                }
            }

            foreach (var x in value[1].EnumerateObject())
            {
                if (x.Name == "label")
                    result.Set(type, x.Value.GetString()!);
            }
        }

        return result;
    }
}