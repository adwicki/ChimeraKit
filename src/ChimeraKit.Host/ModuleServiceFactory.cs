using ChimeraKit.Core.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChimeraKit.Host;

public class ModuleServiceFactory : IModuleServiceFactory
{
    private readonly IServiceCollection _coreServices;
    private readonly IConfiguration _configuration;

    public ModuleServiceFactory(IServiceCollection coreServices, IConfiguration configuration)
    {
        _coreServices = coreServices;
        _configuration = configuration;
    }
    
    /// <summary>
    /// Ensures that the services loaded by the host (such as logging or shared services) are
    /// made available to each module. Eventually this calls `ConfigureModuleServices` of the module
    /// to load in the module's own dependencies.
    /// </summary>
    /// <param name="module"></param>
    /// <returns></returns>
    public IServiceProvider CreateModuleServiceProvider(IModule module)
    {
        ServiceCollection moduleServices = new();
        foreach (ServiceDescriptor serviceDescriptor in _coreServices)
        {
            moduleServices.Add(serviceDescriptor);
        }
        
        module.ConfigureModuleServices(moduleServices, _configuration);
        return moduleServices.BuildServiceProvider();
    }
}