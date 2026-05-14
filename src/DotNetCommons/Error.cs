using System.Net;

namespace DotNetCommons;

public class Error(
    string type,
    string description,
    HttpStatusCode? code = null)
{
    public string Type { get; } = type;
    public string Description { get; } = description;
    public HttpStatusCode? Code { get; } = code;

    public AppException ToAppException()
    {
        return new AppException(Code ?? HttpStatusCode.InternalServerError, $"{Type}: {Description}");
    }
}
