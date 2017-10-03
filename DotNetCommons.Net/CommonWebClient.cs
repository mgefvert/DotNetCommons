using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace DotNetCommons.Net
{
    public class CommonWebClient
    {
        public string Accept { get; set; }
        public bool AllowRedirect { get; set; }
        public CookieContainer CookieContainer { get; set; } = new CookieContainer();
        public Encoding Encoding { get; set; } = Encoding.UTF8;
        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(60);
        public bool ThrowExceptions { get; set; }
        public string UserAgent { get; set; }

        public CommonHttpResponse Request(Uri url, string method, string contentType, byte[] requestData, Uri referer)
        {
            var request = WebRequest.CreateHttp(url);
            request.AllowAutoRedirect = AllowRedirect;
            request.CookieContainer = CookieContainer;
            request.Headers.Add("Origin", url.GetLeftPart(UriPartial.Authority));
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
                    return BuildResponse(response, true);
                }
            }
            catch (WebException ex)
            {
                CommonHttpResponse result = null;
                if (ex.Response != null)
                    using (var response = (HttpWebResponse)ex.Response)
                    {
                        result = BuildResponse(response, false);
                    }

                if (!ThrowExceptions)
                    return result;

                if (result != null)
                    throw new CommonWebException(result, ex);

                throw;
            }
        }

        protected CommonHttpResponse BuildResponse(HttpWebResponse response, bool success)
        {
            if (response == null)
                return null;

            using (var stream = response.GetResponseStream())
            {
                return new CommonHttpResponse
                {
                    Success = success,
                    CharacterSet = response.CharacterSet,
                    ContentEncoding = response.ContentEncoding,
                    ContentType = response.ContentType,
                    Headers = response.Headers,
                    StatusCode = response.StatusCode,
                    StatusDescription = response.StatusDescription,
                    Data = stream == null ? new byte[0] : StreamTools.ReadToEnd(stream)
                };
            }
        }

        protected string AddParametersToQueryString(Uri source, Dictionary<string, string> parameters)
        {
            var query = parameters != null && parameters.Any()
                ? string.Join("&", parameters.Select(x => x.Key + "=" + WebUtility.UrlEncode(x.Value)))
                : null;

            if (source == null)
                return query;

            var s = source.ToString();
            return s + (s.Contains("?") ? "&" : "?") + query;
        }

        // DELETE

        public CommonHttpResponse Delete(Uri url, Uri referer = null)
        {
            return Request(url, "DELETE", null, null, referer);
        }

        // GET

        public byte[] Download(string url)
        {
            return Request(new Uri(url), "GET", null, null, null).Data;
        }

        public string Get(string url)
        {
            return Request(new Uri(url), "GET", null, null, null).Text;
        }

        public CommonHttpResponse Get(Uri url, Uri referer = null)
        {
            return Request(url, "GET", null, null, referer);
        }

        public CommonHttpResponse Get(Uri url, Dictionary<string, string> parameters, Uri referer = null)
        {
            return Request(new Uri(AddParametersToQueryString(url, parameters)), "GET", null, null, referer);
        }

        // HEAD

        public CommonHttpResponse Head(Uri url, Uri referer = null)
        {
            return Request(url, "HEAD", null, null, referer);
        }

        // OPTIONS

        public CommonHttpResponse Options(Uri url)
        {
            return Request(url, "GET", null, null, null);
        }

        // POST

        public CommonHttpResponse Post(Uri url, Uri referer = null)
        {
            return Post(url, null, (byte[])null, referer);
        }

        public CommonHttpResponse Post(Uri url, string contentType, string data, Uri referer = null)
        {
            return Request(url, "POST", contentType, Encoding.GetBytes(data), referer);
        }

        public CommonHttpResponse Post(Uri url, string contentType, byte[] data, Uri referer = null)
        {
            return Request(url, "POST", contentType, data, referer);
        }

        public CommonHttpResponse Post(Uri url, Dictionary<string, string> form, Uri referer = null)
        {
            return Post(url, "application/x-www-form-urlencoded", AddParametersToQueryString(null, form), referer);
        }

        // PUT

        public CommonHttpResponse Put(Uri url, string contentType, string data, Uri referer = null)
        {
            return Request(url, "PUT", contentType, Encoding.GetBytes(data), referer);
        }

        public CommonHttpResponse Put(Uri url, string contentType, byte[] data, Uri referer = null)
        {
            return Request(url, "PUT", contentType, data, referer);
        }
    }
}
