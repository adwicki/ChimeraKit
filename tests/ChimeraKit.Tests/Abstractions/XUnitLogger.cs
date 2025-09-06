using System.Text;
using ChimeraKit.Host.Logging;
using Microsoft.Extensions.Logging;

namespace ChimeraKit.Tests.Abstractions;

internal class XUnitLogger : ILogger
{
    private readonly ITestOutputHelper _outputHelper;
    private const string TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff ";
    private readonly string _categoryName;
    private readonly LoggerExternalScopeProvider _scopeProvider;
    
    public XUnitLogger(ITestOutputHelper outputHelper, LoggerExternalScopeProvider scopeProvider, string categoryName)
    {
        _outputHelper = outputHelper;
        _scopeProvider = scopeProvider;
        _categoryName = categoryName;
    }
    
    public static ILogger CreateLogger(ITestOutputHelper outputHelper) => 
        new XUnitLogger(outputHelper, new LoggerExternalScopeProvider(), "");

    public static ILogger<T> CreateLogger<T>(ITestOutputHelper outputHelper) =>
        new XUnitLogger<T>(outputHelper, new LoggerExternalScopeProvider());

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        string timestamp = DateTime.Now.ToString(TimestampFormat);
        string logLevelStr = ChimeraConsoleLogUtils.GetLogLevelString(logLevel);

        StringBuilder sb = new StringBuilder();
        sb.Append(timestamp);
        sb.Append($"{ChimeraConsoleLogUtils.GetForegroundColorForLoglevel(logLevel)}");
        sb.Append($"{logLevelStr}{ChimeraConsoleLogUtils.DefaultForegroundColor} ");
        sb.Append($"{_categoryName}[{eventId}] ");
        sb.Append(formatter(state, exception));

        if (exception != null)
        {
            sb.Append('\n').Append(exception);
        }

        _scopeProvider.ForEachScope((scope, scopeState) =>
        {
            scopeState.Append("\n => ").Append(scope);
        }, sb);

        _outputHelper.WriteLine(sb.ToString());
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel != LogLevel.None;
    }

    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        return _scopeProvider.Push(state);
    }
}

internal sealed class XUnitLogger<T> : XUnitLogger, ILogger<T>
{
    public XUnitLogger(ITestOutputHelper outputHelper, LoggerExternalScopeProvider scopeProvider)
        : base(outputHelper, scopeProvider, typeof(T).Name)
    { }
}