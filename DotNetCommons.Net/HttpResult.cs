using System;
using System.Net;
using System.Text;

namespace DotNetCommons.Net
{
    public class HttpResult
    {
        public bool Success { get; set; }
        public HttpStatusCode? StatusCode { get; set; }
        public string StatusDescription { get; set; }
        public string ContentEncoding { get; set; }
        public string CharacterSet { get; set; }
        public string ContentType { get; set; }
        public WebHeaderCollection Headers { get; set; }
        public byte[] Data { get; set; }

        public string Text => Encoding.GetEncoding(CharacterSet ?? "utf-8").GetString(Data);
    }
}
