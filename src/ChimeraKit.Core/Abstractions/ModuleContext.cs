using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChimeraKit.Core.Abstractions;

public class ModuleContext : IModuleContext
{
    public IServiceProvider ServiceProvider { get; }
    public ILogger Logger { get; }
    public CancellationToken CancellationToken { get; }

    public ModuleContext(
        IServiceProvider serviceProvider,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        ServiceProvider = serviceProvider;
        Logger = logger;
        CancellationToken = cancellationToken;
    }

    public T GetService<T>() where T : notnull => ServiceProvider.GetRequiredService<T>();

    public T? GetOptionalService<T>() => ServiceProvider.GetService<T>();
}