using System.Net;

namespace DotNetCommons.Web;

public class HttpStatusException : Exception
{
    public HttpStatusCode StatusCode { get; }

    public HttpStatusException(HttpStatusCode statusCode, string message, Exception? innerException = null) : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}