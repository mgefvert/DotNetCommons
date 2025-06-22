#nullable disable
using System.Net;
using System.Text;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Net;

[Serializable]
public class CommonWebResult
{
    /// <summary>
    /// Successful or not (HTTP status = 2xx)
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// HTTP status code.
    /// </summary>
    public HttpStatusCode StatusCode { get; set; }

    /// <summary>
    /// HTTP status description.
    /// </summary>
    public string StatusDescription { get; set; }

    /// <summary>
    /// Content-Encoding header.
    /// </summary>
    public string ContentEncoding { get; set; }

    /// <summary>
    /// Character-Set header.
    /// </summary>
    public string CharacterSet { get; set; }

    /// <summary>
    /// Content-Type header.
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// All headers of the request.
    /// </summary>
    public Dictionary<string, string> Headers { get; } = new(StringComparer.InvariantCultureIgnoreCase);

    /// <summary>
    /// The data bytes returned in the response.
    /// </summary>
    public byte[] Data { get; set; }

    /// <summary>
    /// The data string returned in the response.
    /// </summary>
    public string Text => Encoding.GetEncoding(CharacterSet ?? "utf-8").GetString(Data);
}