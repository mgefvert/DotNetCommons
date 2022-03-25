using System;
using System.Net;

namespace DotNetCommons.Web;

public class ApiException : Exception
{
    public HttpStatusCode HttpCode { get; }

    public ApiException(HttpStatusCode httpCode, string message, Exception? innerException = null) : base(message, innerException)
    {
        HttpCode = httpCode;
    }
}