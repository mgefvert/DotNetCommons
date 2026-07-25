using Microsoft.Extensions.Logging;

namespace DotNetCommons.Logging;

public static class CommonLogging
{
    public static ILoggingBuilder AddCommonConsole(this ILoggingBuilder builder)
    {
        builder
            .AddConsole(x => x.FormatterName = nameof(CommonLoggingFormatter))
            .AddConsoleFormatter<CommonLoggingFormatter, CommonLoggingOptions>();
        
        return builder;
    }
}