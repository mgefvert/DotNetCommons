using System;
using System.Net;

namespace DotNetCommons.Web;

public class AppException : Exception
{
    public HttpStatusCode StatusCode { get; }

    public AppException(HttpStatusCode statusCode, string message, Exception? innerException = null) : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}