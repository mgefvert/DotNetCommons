#nullable disable

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Net.Cache;

public class CacheItem
{
    public string Uri { get; set; }
    public DateTime Timestamp { get; set; }
    public CommonWebResult Result { get; set; }
}