using Microsoft.AspNetCore.Http;

namespace DotNetCommons.Web.Logging;

public class ApiLogRequest
{
    private List<string>? _request;
    private List<string>? _data;
    
    public DateTime Start { get; } = DateTime.UtcNow;
    public bool HasRequest => _request is { Count: > 0 };
    public bool HasData => _data is { Count: > 0 };

    public int Elapsed => (int)(DateTime.UtcNow - Start).TotalMilliseconds;

    public void Request(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return;
        
        _request ??= [];
        _request.Add(text);
    }

    public void Request(QueryString queryString)
    {
        var query = queryString.ToString().TrimStart('?');
        if (!string.IsNullOrEmpty(query))
            Request(query);
    }

    public void Data(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;
        
        _data ??= [];
        _data.Add(text);
    }

    public void Data(IEnumerable<string> text)
    {
        foreach (var s in text)
            Data(s);
    }

    public string RequestToString() => HasRequest ? string.Join(",", _request!.Select(x => $"[{x}]")) : "";
    public string? DataToString() => HasData ? string.Join(",", _data!.Select(x => $"[{x}]")) : null;
}