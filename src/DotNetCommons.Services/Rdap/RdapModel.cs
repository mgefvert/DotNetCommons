using System.Text.Json;
using System.Text.Json.Serialization;
using DotNetCommons.Net;

namespace DotNetCommons.Services.Rdap;

public class RdapResult
{
    public string? Name { get; set; }
    public string? Network { get; set; }
    public string? StartAddress { get; set; }
    public string? EndAddress { get; set; }
    public string? IpVerson { get; set; }

    public List<VCard> VCards { get; } = [];

    public IEnumerable<VCard> OrgVCards   => VCards.Where(v => v.Kind == "org");
    public IEnumerable<VCard> GroupVCards => VCards.Where(v => v.Kind == "group");
    public IEnumerable<VCard> OtherVCards => VCards.Where(v => v.Kind != "org" && v.Kind != "group");

    public VCard? PrimaryVCard => OrgVCards.FirstOrDefault(v => v.Kind == "org" || v.Kind == "group");
}

public class RdapResponse
{
    [JsonPropertyName("notices")]         public List<RdapNotice> Notices { get; set; } = [];
    [JsonPropertyName("handle")]          public string? Handle { get; set; }
    [JsonPropertyName("startAddress")]    public string? StartAddress { get; set; }
    [JsonPropertyName("endAddress")]      public string? EndAddress { get; set; }
    [JsonPropertyName("ipVersion")]       public string? IpVersion { get; set; }
    [JsonPropertyName("name")]            public string? Name { get; set; }
    [JsonPropertyName("type")]            public string? Type { get; set; }
    [JsonPropertyName("parentHandle")]    public string? ParentHandle { get; set; }
    [JsonPropertyName("objectClassName")] public string? ObjectClassName { get; set; }
    [JsonPropertyName("port43")]          public string? Port43 { get; set; }
    [JsonPropertyName("status")]          public List<string> Status { get; set; } = [];
    [JsonPropertyName("events")]          public List<RdapEvent> Events { get; set; } = [];
    [JsonPropertyName("links")]           public List<RdapLink> Links { get; set; } = [];
    [JsonPropertyName("entities")]        public List<RdapEntity> Entities { get; set; } = [];
    [JsonPropertyName("cidr0_cidrs")]     public List<RdapCidr> Cidrs { get; set; } = [];
}

public class RdapEntity
{
    [JsonPropertyName("handle")]          public string? Handle { get; set; }
    [JsonPropertyName("objectClassName")] public string? ObjectClassName { get; set; }
    [JsonPropertyName("port43")]          public string? Port43 { get; set; }
    [JsonPropertyName("roles")]           public List<string> Roles { get; set; } = [];
    [JsonPropertyName("status")]          public List<string> Status { get; set; } = [];
    [JsonPropertyName("events")]          public List<RdapEvent> Events { get; set; } = [];
    [JsonPropertyName("links")]           public List<RdapLink> Links { get; set; } = [];
    [JsonPropertyName("remarks")]         public List<RdapRemark> Remarks { get; set; } = [];
    [JsonPropertyName("entities")]        public List<RdapEntity> Entities { get; set; } = [];
    [JsonPropertyName("vcardArray")]      public List<JsonElement> VcardArray { get; set; } = [];
    [JsonExtensionData]                   public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

public class RdapCidr
{
    [JsonPropertyName("v4prefix")] public string? V4Prefix { get; set; }
    [JsonPropertyName("v6prefix")] public string? V6Prefix { get; set; }
    [JsonPropertyName("length")]   public int? Length { get; set; }
}

public class RdapNotice
{
    [JsonPropertyName("title")]       public string? Title { get; set; }
    [JsonPropertyName("description")] public List<string> Description { get; set; } = [];
    [JsonPropertyName("links")]       public List<RdapLink> Links { get; set; } = [];
    [JsonExtensionData]               public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

public class RdapRemark
{
    [JsonPropertyName("title")]       public string? Title { get; set; }
    [JsonPropertyName("description")] public List<string> Description { get; set; } = [];
    [JsonPropertyName("links")]       public List<RdapLink> Links { get; set; } = [];
    [JsonExtensionData]               public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

public class RdapEvent
{
    [JsonPropertyName("eventAction")] public string? EventAction { get; set; }
    [JsonPropertyName("eventDate")]   public DateTimeOffset? EventDate { get; set; }
    [JsonExtensionData]               public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}

public class RdapLink
{
    [JsonPropertyName("href")]     public string? Href { get; set; }
    [JsonPropertyName("rel")]      public string? Rel { get; set; }
    [JsonPropertyName("type")]     public string? Type { get; set; }
    [JsonPropertyName("value")]    public string? Value { get; set; }
    [JsonPropertyName("hreflang")] public List<string> HrefLang { get; set; } = [];
    [JsonPropertyName("title")]    public string? Title { get; set; }
    [JsonPropertyName("media")]    public string? Media { get; set; }
    [JsonExtensionData]            public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}
