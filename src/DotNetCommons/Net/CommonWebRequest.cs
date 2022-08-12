#nullable disable
using DotNetCommons.IO;
using DotNetCommons.Net.Cache;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Net;

/// <summary>
/// Class that encapsulates a specific HTTP request.
/// </summary>
public class CommonWebRequest
{
    private readonly CommonWebClient _client;

    public delegate void ProgressDelegate(object sender, ProgressArgs args);

    public string Accept { get; set; }
    public bool AllowRedirect { get; set; }
    public IWebCache Cache { get; set; }
    public byte[] ContentData { get; set; }
    public string ContentType { get; set; }
    public CookieContainer CookieContainer { get; set; } = new();
    public Encoding Encoding { get; set; } = Encoding.UTF8;
    public Dictionary<string, string> Headers { get; } = new();
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

    /// <summary>
    /// The request has JSON accept and content-type headers.
    /// </summary>
    public CommonWebRequest AsJson()
    {
        WithAccept(ContentTypes.Json);
        WithContentType(ContentTypes.Json);
        return this;
    }

    /// <summary>
    /// Set the HTTP verb to DELETE and execute the request.
    /// </summary>
    public CommonWebResult Delete()
    {
        WithMethod("DELETE");
        return _client.Request(this);
    }

    /// <summary>
    /// Set the HTTP verb to DELETE and execute the request asynchronously.
    /// </summary>
    public async Task<CommonWebResult> DeleteAsync()
    {
        WithMethod("DELETE");
        return await _client.RequestAsync(this);
    }

    public CommonWebResult Execute()
    {
        return _client.Request(this);
    }

    public async Task<CommonWebResult> ExecuteAsync()
    {
        return await _client.RequestAsync(this);
    }

    /// <summary>
    /// Set the HTTP verb to GET and execute the request.
    /// </summary>
    public CommonWebResult Get()
    {
        WithMethod("GET");
        return _client.Request(this);
    }

    /// <summary>
    /// Set the HTTP verb to GET and execute the request asynchronously.
    /// </summary>
    public async Task<CommonWebResult> GetAsync()
    {
        WithMethod("GET");
        return await _client.RequestAsync(this);
    }

    /// <summary>
    /// Set the HTTP verb to HEAD and execute the request.
    /// </summary>
    public CommonWebResult Head()
    {
        WithMethod("HEAD");
        return _client.Request(this);
    }

    /// <summary>
    /// Set the HTTP verb to HEAD and execute the request asynchronously.
    /// </summary>
    public async Task<CommonWebResult> HeadAsync()
    {
        WithMethod("HEAD");
        return await _client.RequestAsync(this);
    }

    /// <summary>
    /// Set the HTTP verb to OPTIONS and execute the request.
    /// </summary>
    public CommonWebResult Options()
    {
        WithMethod("OPTIONS");
        return _client.Request(this);
    }

    /// <summary>
    /// Set the HTTP verb to OPTIONS and execute the request asynchronously.
    /// </summary>
    public async Task<CommonWebResult> OptionsAsync()
    {
        WithMethod("OPTIONS");
        return await _client.RequestAsync(this);
    }

    /// <summary>
    /// Set the HTTP verb to POST and execute the request.
    /// </summary>
    public CommonWebResult Post()
    {
        WithMethod("POST");
        return _client.Request(this);
    }

    /// <summary>
    /// Set the HTTP verb to POST and execute the request asynchronously.
    /// </summary>
    public async Task<CommonWebResult> PostAsync()
    {
        WithMethod("POST");
        return await _client.RequestAsync(this);
    }

    /// <summary>
    /// Set the Accept: header to a given value.
    /// </summary>
    public CommonWebRequest WithAccept(string accept)
    {
        Accept = accept;
        return this;
    }

    /// <summary>
    /// Automatically follow redirects or not.
    /// </summary>
    public CommonWebRequest WithAllowRedirect(bool allowRedirect)
    {
        AllowRedirect = allowRedirect;
        return this;
    }

    /// <summary>
    /// Use a cache to store already known requests.
    /// </summary>
    public CommonWebRequest WithCache(IWebCache cache)
    {
        Cache = cache;
        return this;
    }

    /// <summary>
    /// Set a specific cookie container.
    /// </summary>
    public CommonWebRequest WithCookieContainer(CookieContainer container)
    {
        CookieContainer = container;
        return this;
    }

    /// <summary>
    /// Add specific POST content to the request.
    /// </summary>
    public CommonWebRequest WithContent(byte[] data)
    {
        ContentData = data;
        return this;
    }

    /// <summary>
    /// Add specific form data to the request, encoded properly.
    /// </summary>
    public CommonWebRequest WithContent(IDictionary<string, string> formData)
    {
        WithContent(CommonWebClient.EncodeQuery(formData as IDictionary));
        return this;
    }

    /// <summary>
    /// Add specific POST content to the request.
    /// </summary>
    public CommonWebRequest WithContent(string data)
    {
        ContentData = Encoding.GetBytes(data);
        return this;
    }

    /// <summary>
    /// Add specific POST content to the request.
    /// </summary>
    public CommonWebRequest WithContent(Stream stream)
    {
        ContentData = StreamTools.ReadToEnd(stream);
        return this;
    }

    /// <summary>
    /// Set the Content-Type: header of the requset.
    /// </summary>
    public CommonWebRequest WithContentType(string contentType)
    {
        ContentType = contentType;
        return this;
    }

    /// <summary>
    /// Specify a specific encoding for parsing the result data.
    /// </summary>
    public CommonWebRequest WithEncoding(Encoding encoding)
    {
        Encoding = encoding;
        return this;
    }

    /// <summary>
    /// Add a given header to the request.
    /// </summary>
    public CommonWebRequest WithHeader(string name, string value)
    {
        Headers[name] = value;
        return this;
    }

    /// <summary>
    /// Add a sequence of headers to the request.
    /// </summary>
    public CommonWebRequest WithHeaders(IEnumerable<KeyValuePair<string, string>> headers)
    {
        foreach (var header in headers)
            Headers[header.Key] = header.Value;
        return this;
    }

    /// <summary>
    /// Set a given HTTP verb.
    /// </summary>
    public CommonWebRequest WithMethod(string method)
    {
        Method = method;
        return this;
    }

    /// <summary>
    /// Add a referer header to the request.
    /// </summary>
    public CommonWebRequest WithReferer(string referer)
    {
        Referer = new Uri(referer);
        return this;
    }

    /// <summary>
    /// Add a referer header to the request.
    /// </summary>
    public CommonWebRequest WithReferer(Uri referer)
    {
        Referer = referer;
        return this;
    }

    /// <summary>
    /// Set the request timeout.
    /// </summary>
    public CommonWebRequest WithTimeout(TimeSpan timeout)
    {
        Timeout = timeout;
        return this;
    }

    /// <summary>
    /// Specify if exceptions should be thrown on errors.
    /// </summary>
    public CommonWebRequest WithThrowExceptions(bool throwExceptions)
    {
        ThrowExceptions = throwExceptions;
        return this;
    }

    /// <summary>
    /// Set the URI of the request, optionally encoding parameters in the URI as well.
    /// </summary>
    public CommonWebRequest WithUri(string uri, IDictionary<string, string> query = null)
    {
        return WithUri(new Uri(uri), query);
    }

    /// <summary>
    /// Set the URI of the request, optionally encoding parameters in the URI as well.
    /// </summary>
    public CommonWebRequest WithUri(Uri uri, IDictionary<string, string> query = null)
    {
        Uri = query == null
            ? uri
            : new Uri(CommonWebClient.EncodeQuery(uri.ToString(), query as IDictionary));

        return this;
    }

    /// <summary>
    /// Add a User-Agent: header to the request.
    /// </summary>
    /// <param name="userAgent"></param>
    /// <returns></returns>
    public CommonWebRequest WithUserAgent(string userAgent)
    {
        UserAgent = userAgent;
        return this;
    }
}