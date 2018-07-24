using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using DotNetCommons.IO;

namespace DotNetCommons.Net
{
    public class CommonWebRequest
    {
        private readonly CommonWebClient _client;

        public delegate void ProgressDelegate(object sender, ProgressArgs args);

        public string Accept { get; set; }
        public bool AllowRedirect { get; set; }
        public IWebCache Cache { get; set; }
        public byte[] ContentData { get; set; }
        public string ContentType { get; set; }
        public CookieContainer CookieContainer { get; set; } = new CookieContainer();
        public Encoding Encoding { get; set; } = Encoding.UTF8;
        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();
        public string Method { get; set; }
        public Uri Referer { get; set; }
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(60);
        public bool ThrowExceptions { get; set; } = true;
        public Uri Uri { get; set; }
        public string UserAgent { get; set; }

        public event ProgressDelegate TransferStarted;
        public event ProgressDelegate TransferProgress;
        public event ProgressDelegate TransferCompleted;

        internal void FireTransferStarted(ProgressArgs args) => TransferStarted?.Invoke(this, args);
        internal void FireTransferProgress(ProgressArgs args) => TransferProgress?.Invoke(this, args);
        internal void FireTransferCompleted(ProgressArgs args) => TransferCompleted?.Invoke(this, args);

        public CommonWebRequest(CommonWebClient client, CommonWebRequest settings)
        {
            _client = client;

            if (settings == null || settings == this)
                return;

            Accept = settings.Accept;
            AllowRedirect = settings.AllowRedirect;
            Cache = settings.Cache;
            CookieContainer = settings.CookieContainer;
            Encoding = settings.Encoding;
            Headers = settings.Headers.ToDictionary(x => x.Key, x => x.Value);
            Method = settings.Method;
            Timeout = settings.Timeout;
            ThrowExceptions = settings.ThrowExceptions;
            UserAgent = settings.UserAgent;
        }

        public CommonWebRequest AsJson()
        {
            WithAccept(ContentTypes.Json);
            WithContentType(ContentTypes.Json);
            return this;
        }

        public CommonWebResult Delete()
        {
            WithMethod("DELETE");
            return _client.Request(this);
        }

        public CommonWebResult Execute()
        {
            return _client.Request(this);
        }

        public CommonWebResult Get()
        {
            WithMethod("GET");
            return _client.Request(this);
        }

        public CommonWebResult Head()
        {
            WithMethod("HEAD");
            return _client.Request(this);
        }

        public CommonWebResult Options()
        {
            WithMethod("OPTIONS");
            return _client.Request(this);
        }

        public CommonWebResult Post()
        {
            WithMethod("POST");
            return _client.Request(this);
        }

        public CommonWebRequest WithAccept(string accept)
        {
            Accept = accept;
            return this;
        }

        public CommonWebRequest WithAllowRedirect(bool allowRedirect)
        {
            AllowRedirect = allowRedirect;
            return this;
        }

        public CommonWebRequest WithCache(IWebCache cache)
        {
            Cache = cache;
            return this;
        }

        public CommonWebRequest WithCookieContainer(CookieContainer container)
        {
            CookieContainer = container;
            return this;
        }

        public CommonWebRequest WithContent(byte[] data)
        {
            ContentData = data;
            return this;
        }

        public CommonWebRequest WithContent(IDictionary<string, string> formdata)
        {
            WithContent(EncodeQuery(formdata));
            return this;
        }

        public CommonWebRequest WithContent(string data)
        {
            ContentData = Encoding.GetBytes(data);
            return this;
        }

        public CommonWebRequest WithContent(Stream stream)
        {
            ContentData = StreamTools.ReadToEnd(stream);
            return this;
        }

        public CommonWebRequest WithContentType(string contentType)
        {
            ContentType = contentType;
            return this;
        }

        public CommonWebRequest WithEncoding(Encoding encoding)
        {
            Encoding = encoding;
            return this;
        }

        public CommonWebRequest WithHeader(string name, string value)
        {
            Headers[name] = value;
            return this;
        }

        public CommonWebRequest WithHeaders(IEnumerable<KeyValuePair<string, string>> headers)
        {
            foreach(var header in headers)
                Headers[header.Key] = header.Value;
            return this;
        }

        public CommonWebRequest WithMethod(string method)
        {
            Method = method;
            return this;
        }

        public CommonWebRequest WithReferer(string referer)
        {
            Referer = new Uri(referer);
            return this;
        }

        public CommonWebRequest WithReferer(Uri referer)
        {
            Referer = referer;
            return this;
        }

        public CommonWebRequest WithTimeout(TimeSpan timeout)
        {
            Timeout = timeout;
            return this;
        }

        public CommonWebRequest WithThrowExceptions(bool throwExceptions)
        {
            ThrowExceptions = throwExceptions;
            return this;
        }

        public CommonWebRequest WithUri(string uri, IDictionary<string, string> query = null)
        {
            return WithUri(new Uri(uri), query);
        }

        public CommonWebRequest WithUri(Uri uri, IDictionary<string, string> query = null)
        {
            Uri = query == null
                ? uri
                : new Uri(EncodeQuery(uri, query));

            return this;
        }

        public CommonWebRequest WithUserAgent(string userAgent)
        {
            UserAgent = userAgent;
            return this;
        }

        internal static string EncodeQuery(IDictionary<string, string> parameters)
        {
            return parameters != null && parameters.Any()
                ? string.Join("&", parameters.Select(x => x.Key + "=" + WebUtility.UrlEncode(x.Value?.ToString())))
                : null;
        }

        internal static string EncodeQuery(Uri source, IDictionary<string, string> parameters)
        {
            var query = EncodeQuery(parameters);
            if (source == null)
                return query;

            var s = source.ToString();
            return s + (s.Contains("?") ? "&" : "?") + query;
        }
    }
}
