using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace DotNetCommons.Net
{
    public class HttpClient
    {
        private readonly CookieContainer _cookies = new CookieContainer();

        public bool AllowRedirect { get; set; }
        public bool EchoToConsole { get; set; }
        public bool ThrowExceptions { get; set; }
        public string UserAgent { get; set; }

        public HttpClient()
        {
            AllowRedirect = false;
            ThrowExceptions = false;
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/48.0.2564.116 Safari/537.36";
        }

        protected HttpWebRequest CreateRequest(string method, Uri uri, Uri referer = null)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.AllowAutoRedirect = AllowRedirect;
            request.CookieContainer = _cookies;
            request.MaximumAutomaticRedirections = 5;
            request.Method = method;
            request.UserAgent = UserAgent;

            request.Headers.Add("Origin", uri.GetLeftPart(UriPartial.Authority));

            if (referer != null)
                request.Headers.Add("Referer", referer.ToString());

            return request;
        }

        public HttpResult Delete(Uri uri)
        {
            var request = CreateRequest("DELETE", uri);
            return ReadResponse(request);
        }

        public HttpResult Get(Uri uri)
        {
            var request = CreateRequest("GET", uri);
            return ReadResponse(request);
        }

        public string Query(Dictionary<string, string> parameters)
        {
            return string.Join("&", parameters.Select(x => x.Key + "=" + Uri.EscapeDataString(x.Value)));
        }

        public HttpResult Post(Uri uri, Dictionary<string, string> parameters)
        {
            return Post(uri, Query(parameters));
        }

        public HttpResult Post(Uri uri, string data)
        {
            var request = CreateRequest("POST", uri);

            var bytes = Encoding.UTF8.GetBytes(data);
            request.ContentLength = data.Length;
            request.ContentType = "application/x-www-form-urlencoded";

            using (var postStream = request.GetRequestStream())
                postStream.Write(bytes, 0, bytes.Length);

            return ReadResponse(request);
        }

        protected byte[] ReadResponseStream(Stream stream)
        {
            using (var memory = new MemoryStream())
            {
                var buffer = new byte[1024];

                int size;
                do
                {
                    size = stream.Read(buffer, 0, buffer.Length);
                    if (size > 0)
                        memory.Write(buffer, 0, size);
                } while (size > 0);

                return memory.ToArray();
            }
        }

        protected HttpResult ReadResponse(HttpWebRequest http)
        {
            Console.WriteLine(http.RequestUri);

            try
            {
                using (var response = (HttpWebResponse)http.GetResponse())
                using (var stream = response.GetResponseStream())
                {
                    return new HttpResult
                    {
                        Success = true,
                        CharacterSet = response.CharacterSet,
                        ContentEncoding = response.ContentEncoding,
                        ContentType = response.ContentType,
                        Headers = response.Headers,
                        StatusCode = response.StatusCode,
                        StatusDescription = response.StatusDescription,
                        Data = stream != null ? ReadResponseStream(stream) : new byte[0]
                    };
                }
            }
            catch (WebException ex)
            {
                using (var response = (HttpWebResponse)ex.Response)
                using (var stream = response?.GetResponseStream())
                {
                    var result = new HttpResult
                    {
                        Success = false,
                        CharacterSet = response?.CharacterSet,
                        ContentEncoding = response?.ContentEncoding,
                        ContentType = response?.ContentType,
                        Headers = response?.Headers,
                        StatusCode = response?.StatusCode,
                        StatusDescription = response?.StatusDescription,
                        Data = stream != null ? ReadResponseStream(stream) : new byte[0]
                    };

                    if (ThrowExceptions)
                        throw new HttpClientException(result);

                    return (result);
                }
            }
        }
    }
}
