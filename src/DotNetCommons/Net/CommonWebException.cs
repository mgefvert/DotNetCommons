#nullable disable
using System.Net;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Net;

public class CommonWebException : Exception
{
    public CommonWebResult Result { get; }

    /// <summary>
    /// Raw response data buffer.
    /// </summary>
    public byte[] ResponseData => Result.Data;

    /// <summary>
    /// Response data as a string.
    /// </summary>
    public string ResponseText => Result.Text;

    /// <summary>
    /// HTTP status.
    /// </summary>
    public int Status => (int)Result.StatusCode;

    /// <summary>
    /// HTTP status category (i.e. 4 = 4xx response codes, 3 = 3xx response codes, etc).
    /// </summary>
    public int StatusCategory => (int)Result.StatusCode / 100;

    /// <summary>
    /// HTTP status code.
    /// </summary>
    public HttpStatusCode StatusCode => Result.StatusCode;

    /// <summary>
    /// HTTP status message.
    /// </summary>
    public string StatusMessage => Result.StatusDescription;

    public CommonWebException()
    {
    }

    public CommonWebException(CommonWebResult result, Exception innerException)
        : base((int)result.StatusCode + " " + result.StatusDescription, innerException)
    {
        Result = result;
    }
}