using Microsoft.Extensions.Logging.Console;

namespace ChimeraKit.Host.Logging;

public class ChimeraKitConsoleLogFormatterOptions : ConsoleFormatterOptions
{
    public bool SingleLine { get; set; } = true;
    public new string? TimestampFormat { get; set; } 
    
}