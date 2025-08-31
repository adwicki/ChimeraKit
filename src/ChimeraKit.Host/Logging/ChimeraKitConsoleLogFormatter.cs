using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace ChimeraKit.Host.Logging;

public class ChimeraKitConsoleLogFormatter : ConsoleFormatter
{
    private readonly ChimeraKitConsoleLogFormatterOptions _options;
    
    private const string DefaultForegroundColor = "\x1B[39m\x1B[22m";
    
    public ChimeraKitConsoleLogFormatter(IOptionsMonitor<ChimeraKitConsoleLogFormatterOptions> options)
        : base("chimera-kit-formatter")
    {
        _options = options.CurrentValue;
    }
    
    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
    {
        string timestamp = _options.TimestampFormat != null ? DateTime.Now.ToString(_options.TimestampFormat) : "";

        string logLevel = GetLogLevelString(logEntry.LogLevel);
        string className = GetClassName(logEntry.Category);
        string message = logEntry.Formatter.Invoke(logEntry.State, logEntry.Exception);

        if (_options.SingleLine)
        {
            textWriter.WriteLine($"{timestamp}{GetForegroundColorForLoglevel(logEntry.LogLevel)}{logLevel}{DefaultForegroundColor}: {className}[{logEntry.EventId}] {message}");
        }
        else
        {
            textWriter.WriteLine($"{timestamp}{logLevel}: {className}[{logEntry.EventId}]");
            textWriter.WriteLine($"      {message}");
        }

        if (logEntry.Exception != null)
        {
            textWriter.WriteLine(logEntry.Exception.ToString());
        }
    }
    
    private static string GetClassName(string category)
    {
        if (string.IsNullOrEmpty(category))
        {
            return category;
        }

        int lastDotIndex = category.LastIndexOf('.');
        return lastDotIndex >= 0 ? category[(lastDotIndex + 1)..] : category;
    }

    private static string GetLogLevelString(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => "TRACE",
            LogLevel.Debug => "DEBUG",
            LogLevel.Information => "INFO",
            LogLevel.Warning => "WARN",
            LogLevel.Error => "FAIL",
            LogLevel.Critical => "CRIT",
            _ => "NONE"
        };
    }

    private static string GetForegroundColorForLoglevel(LogLevel logLevel) =>
        logLevel switch
        {
            LogLevel.Trace => GetForegroundColorEscapeCode(ConsoleColor.DarkMagenta),
            LogLevel.Debug => DefaultForegroundColor,
            LogLevel.Information => GetForegroundColorEscapeCode(ConsoleColor.DarkGreen),
            LogLevel.Warning => GetForegroundColorEscapeCode(ConsoleColor.DarkYellow),
            LogLevel.Error => GetForegroundColorEscapeCode(ConsoleColor.DarkRed),
            LogLevel.Critical => GetForegroundColorEscapeCode(ConsoleColor.Red),
            LogLevel.None => DefaultForegroundColor,
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null)
        };

    private static string GetForegroundColorEscapeCode(ConsoleColor color) =>
        color switch
        {
            ConsoleColor.DarkRed => "\x1B[31m",
            ConsoleColor.DarkGreen => "\x1B[32m",
            ConsoleColor.DarkYellow => "\x1B[33m",
            ConsoleColor.DarkMagenta => "\x1B[35m",
            ConsoleColor.Red => "\x1B[1m\x1B[31m",
            _ => DefaultForegroundColor
        };
}