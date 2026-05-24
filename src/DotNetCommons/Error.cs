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

    public AppException ToAppException() => new(Code ?? HttpStatusCode.InternalServerError, ToString());

    public override string ToString() => $"{Type}: {Description}";
}
