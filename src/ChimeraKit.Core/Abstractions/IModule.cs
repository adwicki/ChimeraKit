using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChimeraKit.Core.Abstractions;

public interface IModule
{
    string Name { get; }
    string Description { get; }

    void ConfigureModuleServices(IServiceCollection services, IConfiguration configuration);
    
    Task<ExitCode> ExecuteAsync(IModuleContext context, string[] args);
}