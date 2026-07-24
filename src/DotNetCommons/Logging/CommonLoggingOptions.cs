using Microsoft.Extensions.Logging.Console;

namespace DotNetCommons.Logging;

public class CommonLoggingOptions : ConsoleFormatterOptions
{
    public CommonLoggingOptions()
    {
        TimestampFormat = "yyyy-MM-dd HH:mm:ss";
        UseUtcTimestamp = false;
        IncludeScopes   = false;
    }
}