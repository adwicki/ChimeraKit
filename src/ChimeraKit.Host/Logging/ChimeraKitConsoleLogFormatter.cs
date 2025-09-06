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

        string logLevel = ChimeraConsoleLogUtils.GetLogLevelString(logEntry.LogLevel);
        string className = ChimeraConsoleLogUtils.GetClassName(logEntry.Category);
        string message = logEntry.Formatter.Invoke(logEntry.State, logEntry.Exception);

        if (_options.SingleLine)
        {
            textWriter.WriteLine($"{timestamp}{ChimeraConsoleLogUtils.GetForegroundColorForLoglevel(logEntry.LogLevel)}" +
                                 $"{logLevel}{ChimeraConsoleLogUtils.DefaultForegroundColor}: " +
                                 $"{className}[{logEntry.EventId}] {message}");
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
}