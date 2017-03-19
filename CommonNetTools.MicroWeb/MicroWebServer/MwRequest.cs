using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace CommonNetTools.Scripting.MicroWebServer
{
    public class MwRequest : EventArgs
    {
        public HttpListenerContext Context { get; }
        public HttpListenerRequest HttpRequest { get; }
        public string RequestBody { get; }

        public NameValueCollection QueryValues { get; }
        public NameValueCollection PostValues { get; }

        public MwRequest(HttpListenerContext context)
        {
            Context = context;
            HttpRequest = context.Request;
            RequestBody = GetRequestBody();

            QueryValues = context.Request.QueryString;
            if (context.Request.ContentType?.Contains("www-form-urlencoded") ?? false)
                PostValues = HttpUtility.ParseQueryString(RequestBody);
        }

        private string GetRequestBody()
        {
            using (var stream = HttpRequest.InputStream)
            {
                if (stream == null)
                    return null;

                using (var reader = new StreamReader(stream, Encoding.UTF8))
                    return reader.ReadToEnd();
            }
        }
    }
}
