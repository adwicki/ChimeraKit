using Microsoft.Extensions.Logging;

namespace ChimeraKit.Host.Logging;

public static class ChimeraConsoleLogUtils
{
    public const string DefaultForegroundColor = "\x1B[39m\x1B[22m";
    
    public static string GetLogLevelString(LogLevel logLevel)
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
    
    public static string GetClassName(string category)
    {
        if (string.IsNullOrEmpty(category))
        {
            return category;
        }

        int lastDotIndex = category.LastIndexOf('.');
        return lastDotIndex >= 0 ? category[(lastDotIndex + 1)..] : category;
    }
    
    public static string GetForegroundColorForLoglevel(LogLevel logLevel) =>
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