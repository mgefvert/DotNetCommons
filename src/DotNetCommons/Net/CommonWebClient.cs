using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Net;

public class CommonWebClient
{
    public CommonWebRequest Default { get; }

    public CommonWebClient()
    {
        Default = new CommonWebRequest(this, null);
    }

    // Static methods

    public static Dictionary<string, string> ParametersFromObject(object data)
    {
        return data?
            .GetType()
            .GetProperties()
            .ToDictionary(p => p.Name, p => (p.GetValue(data) ?? "").ToString());
    }

    public static string EncodeQuery(IDictionary parameters)
    {
        if (parameters == null || parameters.Count == 0)
            return null;

        var query = new List<string>();
        foreach (DictionaryEntry pair in parameters)
            query.Add(pair.Key + "=" + WebUtility.UrlEncode(pair.Value?.ToString()));

        return string.Join("&", query);
    }

    public static string EncodeQuery(string url, IDictionary parameters)
    {
        var query = EncodeQuery(parameters);
        if (url == null)
            return query;

        return url + (url.Contains("?") ? "&" : "?") + query;
    }

    // Internal methods

    internal CommonWebResult Request(CommonWebRequest request)
    {
        var cache = request.Cache != null && request.Method == "GET";
        if (cache && request.Cache.TryFetch(request.Uri.ToString(), out var result))
            return result;

        result = DoRequest(request);

        if (cache)
            request.Cache.Store(request.Uri.ToString(), result);

        return result;
    }

    internal async Task<CommonWebResult> RequestAsync(CommonWebRequest request)
    {
        var cache = request.Cache != null && request.Method == "GET";
        if (cache && request.Cache.TryFetch(request.Uri.ToString(), out var result))
            return result;

        result = await DoRequestAsync(request);

        if (cache)
            request.Cache.Store(request.Uri.ToString(), result);

        return result;
    }

    protected CommonWebResult DoRequest(CommonWebRequest request)
    {
        var http = CreateRequest(request);

        request.FireTransferStarted(new ProgressArgs(request.Uri, -1, -1));
        if (request.ContentData != null)
            WriteContent(request, http);

        try
        {
            using var response = (HttpWebResponse)http.GetResponse();
            return BuildResponse(request, response, true);
        }
        catch (WebException ex)
        {
            CommonWebResult result = null;
            if (ex.Response != null)
            {
                using var response = (HttpWebResponse)ex.Response;
                result = BuildResponse(request, response, false);
            }

            if (!request.ThrowExceptions)
                return result;

            if (result != null)
                throw new CommonWebException(result, ex);

            throw;
        }
    }

    protected async Task<CommonWebResult> DoRequestAsync(CommonWebRequest request)
    {
        var http = CreateRequest(request);

        request.FireTransferStarted(new ProgressArgs(request.Uri, -1, -1));
        if (request.ContentData != null)
            WriteContent(request, http);

        try
        {
            using var response = (HttpWebResponse)(await http.GetResponseAsync());
            return BuildResponse(request, response, true);
        }
        catch (WebException ex)
        {
            CommonWebResult result = null;
            if (ex.Response != null)
            {
                using var response = (HttpWebResponse)ex.Response;
                result = BuildResponse(request, response, false);
            }

            if (!request.ThrowExceptions)
                return result;

            if (result != null)
                throw new CommonWebException(result, ex);

            throw;
        }
    }

    private static void WriteContent(CommonWebRequest request, HttpWebRequest http)
    {
        http.ContentLength = request.ContentData.Length;
        using var requestStream = http.GetRequestStream();
        requestStream.Write(request.ContentData, 0, request.ContentData.Length);
    }

    private static HttpWebRequest CreateRequest(CommonWebRequest request)
    {
        var http = WebRequest.CreateHttp(request.Uri);
        http.AllowAutoRedirect = request.AllowRedirect;
        http.CookieContainer = request.CookieContainer;
        http.Headers.Add("Origin", request.Uri.GetLeftPart(UriPartial.Authority));
        http.MaximumAutomaticRedirections = 5;
        http.Method = request.Method;
        http.Timeout = (int)request.Timeout.TotalMilliseconds;

        if (!string.IsNullOrEmpty(request.Accept))
            http.Accept = request.Accept;
        if (!string.IsNullOrEmpty(request.ContentType))
            http.ContentType = request.ContentType;
        if (request.Referer != null)
            http.Headers.Add("Referer", request.Referer.ToString());
        if (!string.IsNullOrEmpty(request.UserAgent))
            http.UserAgent = request.UserAgent;

        foreach (var header in request.Headers)
            http.Headers[header.Key] = header.Value;
        return http;
    }

    protected CommonWebResult BuildResponse(CommonWebRequest request, HttpWebResponse response, bool success)
    {
        if (response == null)
            return null;

        using var stream = response.GetResponseStream();

        var result = new CommonWebResult
        {
            Success = success,
            CharacterSet = NullIfEmpty(response.CharacterSet),
            ContentEncoding = NullIfEmpty(response.ContentEncoding),
            ContentType = NullIfEmpty(response.ContentType),
            StatusCode = response.StatusCode,
            StatusDescription = response.StatusDescription,
            Data = ReadToEnd(request, stream, response.ContentLength)
        };

        foreach (var key in response.Headers.AllKeys)
            result.Headers[key] = response.Headers[key];

        return result;
    }

    private string NullIfEmpty(string s)
    {
        return string.IsNullOrWhiteSpace(s) ? null : s;
    }

    protected byte[] ReadToEnd(CommonWebRequest request, Stream stream, long length)
    {
        var result = new MemoryStream();
        try
        {
            if (stream == null)
                return result.ToArray();

            var buffer = new byte[65536];
            for (; ; )
            {
                var len = stream.Read(buffer, 0, 16384);
                if (len == 0)
                    return result.ToArray();

                result.Write(buffer, 0, len);
                request.FireTransferProgress(new ProgressArgs(request.Uri, length, result.Position));
            }
        }
        finally
        {
            request.FireTransferCompleted(new ProgressArgs(request.Uri, length, result.Position));
        }
    }

    // Public methods

    public CommonWebRequest NewJsonRequest()
    {
        return NewRequest()
            .WithAccept("application/json")
            .WithContentType("application/json");
    }

    public CommonWebRequest NewRequest()
    {
        return new CommonWebRequest(this, Default);
    }

    public CommonWebResult Delete(Uri uri)
    {
        return NewRequest()
            .WithUri(uri)
            .Delete();
    }

    public async Task<CommonWebResult> DeleteAsync(Uri uri)
    {
        return await NewRequest()
            .WithUri(uri)
            .DeleteAsync();
    }

    public CommonWebResult Get(string uri, IDictionary<string, string> query = null)
    {
        return Get(new Uri(uri), query);
    }

    public async Task<CommonWebResult> GetAsync(string uri, IDictionary<string, string> query = null)
    {
        return await GetAsync(new Uri(uri), query);
    }

    public CommonWebResult Get(Uri uri, IDictionary<string, string> query = null)
    {
        return NewRequest()
            .WithUri(uri, query)
            .Get();
    }

    public async Task<CommonWebResult> GetAsync(Uri uri, IDictionary<string, string> query = null)
    {
        return await NewRequest()
            .WithUri(uri, query)
            .GetAsync();
    }

    public CommonWebResult PostData(Uri uri, byte[] data, string contentType)
    {
        return NewRequest()
            .WithUri(uri)
            .WithContent(data)
            .WithContentType(contentType)
            .Post();
    }

    public async Task<CommonWebResult> PostDataAsync(Uri uri, byte[] data, string contentType)
    {
        return await NewRequest()
            .WithUri(uri)
            .WithContent(data)
            .WithContentType(contentType)
            .PostAsync();
    }

    public CommonWebResult PostData(Uri uri, string data, string contentType)
    {
        return NewRequest()
            .WithUri(uri)
            .WithContent(data)
            .WithContentType(contentType)
            .Post();
    }

    public async Task<CommonWebResult> PostDataAsync(Uri uri, string data, string contentType)
    {
        return await NewRequest()
            .WithUri(uri)
            .WithContent(data)
            .WithContentType(contentType)
            .PostAsync();
    }

    public CommonWebResult PostForm(Uri uri, IDictionary<string, string> formdata)
    {
        return NewRequest()
            .WithUri(uri)
            .WithContent(formdata)
            .WithContentType(ContentTypes.UrlEncoded)
            .Post();
    }

    public async Task<CommonWebResult> PostFormAsync(Uri uri, IDictionary<string, string> formdata)
    {
        return await NewRequest()
            .WithUri(uri)
            .WithContent(formdata)
            .WithContentType(ContentTypes.UrlEncoded)
            .PostAsync();
    }

    public CommonWebResult PostJson(Uri uri, string data)
    {
        return NewRequest()
            .WithUri(uri)
            .WithContent(data)
            .WithContentType(ContentTypes.Json)
            .Post();
    }
    public async Task<CommonWebResult> PostJsonAsync(Uri uri, string data)
    {
        return await NewRequest()
            .WithUri(uri)
            .WithContent(data)
            .WithContentType(ContentTypes.Json)
            .PostAsync();
    }
}