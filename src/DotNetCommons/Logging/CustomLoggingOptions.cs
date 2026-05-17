using Microsoft.Extensions.Logging.Console;

namespace DotNetCommons.Logging;

public class CustomLoggingOptions : ConsoleFormatterOptions
{
    public CustomLoggingOptions()
    {
        TimestampFormat = "yyyy-MM-dd HH:mm:ss";
        UseUtcTimestamp = false;
        IncludeScopes   = false;
    }
}