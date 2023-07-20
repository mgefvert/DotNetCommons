namespace DotNetCommons.PlaywrightTesting;

public class TestResult
{
    public string ClassName { get; }
    public string MethodName { get; }
    public bool Success { get; }
    public string? Message { get; }

    public TestResult(string className, string methodName, bool success, string? message)
    {
        ClassName = className;
        MethodName = methodName;
        Success = success;
        Message = message;
    }
}