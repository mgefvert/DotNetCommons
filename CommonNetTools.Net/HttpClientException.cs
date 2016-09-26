using System;

namespace CommonNetTools.Net
{
    public class HttpClientException : Exception
    {
        public HttpResult Result { get; private set; }

        public HttpClientException(HttpResult result) : base((int)result.StatusCode + " " + result.StatusDescription)
        {
            Result = result;
        }
    }
}
