namespace DotNetCommons.PlaywrightTesting;

public class PlaywrightTestingException : Exception
{
    public PlaywrightTestingException(string? message) : base(message)
    {
    }
}