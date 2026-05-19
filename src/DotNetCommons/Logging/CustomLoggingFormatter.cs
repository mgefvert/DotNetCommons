using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace DotNetCommons.Logging;

public class CustomLoggingFormatter : ConsoleFormatter, IDisposable
{
    private readonly IDisposable? _optionsReloadToken;
    private CustomLoggingOptions _formatterOptions;

    public CustomLoggingFormatter(IOptionsMonitor<CustomLoggingOptions> options)
        : base(nameof(CustomLoggingFormatter))
    {
        _formatterOptions = options.CurrentValue;
        _optionsReloadToken = options.OnChange(opt => _formatterOptions = opt);
    }

    private bool ConsoleColorFormattingEnabled => !Console.IsOutputRedirected;

    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
    {
        var now       = _formatterOptions.UseUtcTimestamp ? DateTime.UtcNow : DateTime.Now;
        var timestamp = now.ToString(_formatterOptions.TimestampFormat);
        var logLevel  = GetLogLevelText(logEntry.LogLevel);
        var category  = logEntry.Category.IsSet() ? $"{logEntry.Category.Split('.').LastOrDefault()}: " : "";
        var message   = logEntry.Formatter(logEntry.State, logEntry.Exception);
        var prefix    = logEntry.LogLevel is LogLevel.Debug or LogLevel.Trace ? "- " : "";

        string? scope = null;
        if (_formatterOptions.IncludeScopes && scopeProvider != null)
        {
            var scopeBuilder = new StringBuilder();
            scopeProvider.ForEachScope((scopeValue, state) =>
            {
                state.Append(" [");
                state.Append(scopeValue);
                state.Append(']');
            }, scopeBuilder);

            if (scopeBuilder.Length > 0)
                scope = scopeBuilder.ToString();
        }

        var color = ConsoleColorFormattingEnabled ? GetLogLevelColor(logEntry.LogLevel) : null;
        if (color != null)
            textWriter.Write(color);

        textWriter.Write($"{timestamp} {logLevel} {prefix}{category}{message}{scope}");
        if (logEntry.Exception != null)
            textWriter.Write($"\n{logEntry.Exception}");

        if (color != null)
            textWriter.Write("\x1b[0m");

        textWriter.WriteLine();
    }

    private static string? GetLogLevelColor(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace or
            LogLevel.Debug    => "\x1b[37m",   // dark gray
            LogLevel.Warning  => "\x1b[1m\x1b[33m",
            LogLevel.Error    => "\x1b[1m\x1b[31m",
            LogLevel.Critical => "\x1b[1m\x1b[31m\x1b[41m",
            _                 => null
        };
    }

    private static string GetLogLevelText(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Warning  => "WARN",
            LogLevel.Error    => "ERR ",
            LogLevel.Critical => "CRIT",
            _                 => "    "
        };
    }

    public void Dispose()
    {
        _optionsReloadToken?.Dispose();
    }
}