using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Net;

[Serializable]
public class CommonWebResult
{
    public bool Success { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public string StatusDescription { get; set; }
    public string ContentEncoding { get; set; }
    public string CharacterSet { get; set; }
    public string ContentType { get; set; }
    public Dictionary<string, string> Headers { get; } = new(StringComparer.InvariantCultureIgnoreCase);
    public byte[] Data { get; set; }

    public string Text => Encoding.GetEncoding(CharacterSet ?? "utf-8").GetString(Data);
}