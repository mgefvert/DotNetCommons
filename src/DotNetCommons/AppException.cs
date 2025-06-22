using System.Net;

namespace DotNetCommons;

public class AppException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public string UserMessage { get; }

    public AppException(string userMessage, string diagnosticMessage) : 
        this(HttpStatusCode.InternalServerError, userMessage, diagnosticMessage)
    {
    }

    public AppException(string userMessage) : 
        this(HttpStatusCode.InternalServerError, userMessage)
    {
    }

    public AppException(HttpStatusCode statusCode, string userMessage)
    {
        StatusCode  = statusCode;
        UserMessage = userMessage;
    }

    public AppException(HttpStatusCode statusCode, string userMessage, string internalMessage) : base(internalMessage)
    {
        StatusCode  = statusCode;
        UserMessage = userMessage;
    }
}