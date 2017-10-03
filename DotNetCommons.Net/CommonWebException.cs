using System;
using System.Net;

namespace DotNetCommons.Net
{
    public class CommonWebException : Exception
    {
        public CommonHttpResponse Result { get; }

        public byte[] ResponseData => Result.Data;
        public string ResponseText => Result.Text;
        public int Status => (int)Result.StatusCode;
        public int StatusCategory => (int)Result.StatusCode / 100;
        public HttpStatusCode StatusCode => Result.StatusCode;
        public string StatusMessage => Result.StatusDescription;

        public CommonWebException()
        {
        }

        public CommonWebException(CommonHttpResponse result, Exception innerException) 
            : base((int)result.StatusCode + " " + result.StatusDescription, innerException)
        {
            Result = result;
        }
    }
}
