namespace DotNetCommons;

public enum ErrorCategory
{
    InvalidParameters  = 100,
    NotFound           = 101,
    AlreadyCompleted   = 102,
    AccessDenied       = 200,
    Conflict           = 300,
    RateExceeded       = 400,
    InternalError      = 500,
    Unavailable        = 501,
    Timeout            = 502,
}

public class Error(ErrorCategory category, string description)
{
    public ErrorCategory Category { get; } = category;
    public string Description { get; } = description;

    public Exception ToException() => new(ToString());

    public override string ToString() => $"{Category:G}: {Description}";
}
