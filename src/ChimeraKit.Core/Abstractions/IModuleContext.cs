using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ChimeraKit.Core.Abstractions;

public interface IModuleContext
{
    IServiceProvider ServiceProvider { get; }
    ILogger Logger { get; }
    CancellationToken CancellationToken { get; }

    T GetService<T>() where T : notnull;
    T? GetOptionalService<T>();
}