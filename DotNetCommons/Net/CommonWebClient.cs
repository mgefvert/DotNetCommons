using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace DotNetCommons.Net
{
    public class CommonWebClient
    {
        public class ProgressArgs : EventArgs
        {
            public Uri Location { get; }
            public long Size { get; }
            public long Progress { get; }

            public ProgressArgs(Uri location, long size, long progress)
            {
                Location = location;
                Size = size;
                Progress = progress;
            }
        }

        public delegate void ProgressDelegate(object sender, ProgressArgs args);

        public string Accept { get; set; }
        public bool AllowRedirect { get; set; }
        public IDictionary<string, CommonWebResult> Cache { get; set; }
        public CookieContainer CookieContainer { get; set; } = new CookieContainer();
        public Encoding Encoding { get; set; } = Encoding.UTF8;
        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(60);
        public bool ThrowExceptions { get; set; } = true;
        public string UserAgent { get; set; }

        public event ProgressDelegate TransferStarted;
        public event ProgressDelegate TransferProgress;
        public event ProgressDelegate TransferCompleted;

        public CommonWebResult Request(Uri uri, string method, string contentType, byte[] requestData, Uri referer)
        {
            var cache = Cache != null && method == "GET";
            if (cache && Cache.TryGetValue(uri.ToString(), out var cacheResult))
                return cacheResult;

            var result = DoRequest(uri, method, contentType, requestData, referer);

            if (cache)
                Cache[uri.ToString()] = result;

            return result;
        }

        protected CommonWebResult DoRequest(Uri uri, string method, string contentType, byte[] requestData, Uri referer)
        {
            var request = WebRequest.CreateHttp(uri);
            request.AllowAutoRedirect = AllowRedirect;
            request.CookieContainer = CookieContainer;
            request.Headers.Add("Origin", uri.GetLeftPart(UriPartial.Authority));
            request.MaximumAutomaticRedirections = 5;
            request.Method = method;
            request.Timeout = (int)Timeout.TotalMilliseconds;

            if (!string.IsNullOrEmpty(Accept))
                request.Accept = Accept;
            if (!string.IsNullOrEmpty(contentType))
                request.ContentType = contentType;
            if (referer != null)
                request.Headers.Add("Referer", referer.ToString());
            if (!string.IsNullOrEmpty(UserAgent))
                request.UserAgent = UserAgent;

            foreach (var header in Headers)
                request.Headers[header.Key] = header.Value;

            TransferStarted?.Invoke(this, new ProgressArgs(uri, -1, -1));
            if (requestData != null)
            {
                request.ContentLength = requestData.Length;
                using (var requestStream = request.GetRequestStream())
                    requestStream.Write(requestData, 0, requestData.Length);
            }

            try
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    return BuildResponse(uri, response, true);
                }
            }
            catch (WebException ex)
            {
                CommonWebResult result = null;
                if (ex.Response != null)
                    using (var response = (HttpWebResponse)ex.Response)
                    {
                        result = BuildResponse(uri, response, false);
                    }

                if (!ThrowExceptions)
                    return result;

                if (result != null)
                    throw new CommonWebException(result, ex);

                throw;
            }
        }

        protected CommonWebResult BuildResponse(Uri uri, HttpWebResponse response, bool success)
        {
            if (response == null)
                return null;

            using (var stream = response.GetResponseStream())
            {
                return new CommonWebResult
                {
                    Success = success,
                    CharacterSet = NullIfEmpty(response.CharacterSet),
                    ContentEncoding = NullIfEmpty(response.ContentEncoding),
                    ContentType = NullIfEmpty(response.ContentType),
                    Headers = response.Headers,
                    StatusCode = response.StatusCode,
                    StatusDescription = response.StatusDescription,
                    Data = ReadToEnd(stream, uri, response.ContentLength)
                };
            }
        }

        private string NullIfEmpty(string s)
        {
            return string.IsNullOrWhiteSpace(s) ? null : s;
        }

        public byte[] ReadToEnd(Stream stream, Uri uri, long length)
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
                    TransferProgress?.Invoke(this, new ProgressArgs(uri, length, result.Position));
                }
            }
            finally
            {
                TransferCompleted?.Invoke(this, new ProgressArgs(uri, length, result.Position));
            }
        }

        protected string AddParametersToQueryString(Uri source, Dictionary<string, string> parameters)
        {
            var query = BuildQuery(parameters);
            if (source == null)
                return query;

            var s = source.ToString();
            return s + (s.Contains("?") ? "&" : "?") + query;
        }

        public static string BuildQuery(object parameters)
        {
            Dictionary<string, string> result;

            switch (parameters)
            {
                case null:
                    return null;

                case IDictionary pdict:
                    result = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
                    foreach (DictionaryEntry item in pdict)
                        result[item.Key.ToString()] = item.Value?.ToString();
                    break;

                default:
                    result = parameters.GetType().GetProperties()
                        .ToDictionary(p => p.Name, p => (p.GetValue(parameters) ?? "").ToString());
                    break;
            }

            return result.Any()
                ? string.Join("&", result.Select(x => x.Key + "=" + WebUtility.UrlEncode(x.Value?.ToString())))
                : null;
        }

        // DELETE

        public CommonWebResult Delete(Uri uri, Uri referer = null)
        {
            return Request(uri, "DELETE", null, null, referer);
        }

        // GET

        public byte[] Download(string uri)
        {
            return Request(new Uri(uri), "GET", null, null, null).Data;
        }

        public string Get(string uri)
        {
            return Request(new Uri(uri), "GET", null, null, null).Text;
        }

        public CommonWebResult Get(Uri uri, Uri referer = null)
        {
            return Request(uri, "GET", null, null, referer);
        }

        public CommonWebResult Get(Uri uri, Dictionary<string, string> parameters, Uri referer = null)
        {
            return Request(new Uri(AddParametersToQueryString(uri, parameters)), "GET", null, null, referer);
        }

        // HEAD

        public CommonWebResult Head(Uri uri, Uri referer = null)
        {
            return Request(uri, "HEAD", null, null, referer);
        }

        // OPTIONS

        public CommonWebResult Options(Uri uri)
        {
            return Request(uri, "GET", null, null, null);
        }

        // POST

        public CommonWebResult Post(Uri uri, Uri referer = null)
        {
            return Post(uri, null, (byte[])null, referer);
        }

        public CommonWebResult Post(Uri uri, string contentType, string data, Uri referer = null)
        {
            return Request(uri, "POST", contentType, Encoding.GetBytes(data), referer);
        }

        public CommonWebResult Post(Uri uri, string contentType, byte[] data, Uri referer = null)
        {
            return Request(uri, "POST", contentType, data, referer);
        }

        public CommonWebResult Post(Uri uri, Dictionary<string, string> form, Uri referer = null)
        {
            return Post(uri, "application/x-www-form-urlencoded", AddParametersToQueryString(null, form), referer);
        }

        // PUT

        public CommonWebResult Put(Uri uri, string contentType, string data, Uri referer = null)
        {
            return Request(uri, "PUT", contentType, Encoding.GetBytes(data), referer);
        }

        public CommonWebResult Put(Uri uri, string contentType, byte[] data, Uri referer = null)
        {
            return Request(uri, "PUT", contentType, data, referer);
        }
    }
}
