namespace DotNetCommons;

public enum ErrorCategory
{
    // 100 errors are mostly benign errors that may be ignored.
    NotFound           = 101,
    AlreadyCompleted   = 102,
    NoOp               = 103,

    // 200 errors are authentication/authorization errors.
    Authentication     = 200,
    Authorization      = 201,

    // 300 represents problems with the data provided.
    InvalidParameters  = 300,
    InvalidData        = 301,
    Conflict           = 302,

    // 400 errors are rate-limiting errors or other transient errors that can be retried.
    RateExceeded       = 400,
    Unavailable        = 401,
    Timeout            = 402,

    // 500 errors are hard errors that require investigation.
    InternalError      = 500,
}

public class Error(ErrorCategory category, string description)
{
    public ErrorCategory Category { get; } = category;
    public string Description { get; } = description;

    public Exception ToException() => new(ToString());

    public override string ToString() => $"{Category:G}: {Description}";
}
