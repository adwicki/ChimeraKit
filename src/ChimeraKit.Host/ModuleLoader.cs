using System.Reflection;
using ChimeraKit.Core.Abstractions;
using ChimeraKit.Core.Configuration;
using Microsoft.Extensions.Logging;

namespace ChimeraKit.Host;

public class ModuleLoader : IModuleLoader
{
    private readonly ILogger<IModuleLoader> _logger;
    private readonly CoreConfiguration _configuration;

    public ModuleLoader(ILogger<IModuleLoader> logger, CoreConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }
    
    public List<IModule> LoadModules()
    {
        List<IModule> modules = [];
        List<Assembly> moduleAssemblies = [];

        foreach (ModuleInformation moduleInformation in _configuration.AvailableModules)
        {
            Assembly? moduleAssembly = LoadPluginModule(moduleInformation, _configuration.ModuleRoot);
            if (moduleAssembly != null)
            {
                moduleAssemblies.Add(moduleAssembly);
            }
            else
            {
                _logger.LogWarning("Failed to load module {ModuleName}", moduleInformation.ModuleName);
            }
        }

        foreach (Assembly assembly in moduleAssemblies)
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (!typeof(IModule).IsAssignableFrom(type))
                {
                    continue;
                }

                if (Activator.CreateInstance(type) is IModule module)
                {
                    modules.Add(module);
                    _logger.LogDebug("Loaded module: {ModuleName}", module.Name);
                }
                else
                {
                    _logger.LogError("Failed to load module from type {ModuleType}", assembly.GetName().Name);
                }
            }
        }

        return modules;
    }

    private Assembly? LoadPluginModule(ModuleInformation moduleInfo, string moduleRoot)
    {
        string applicationRunningRoot = Path.Combine(
            Path.GetDirectoryName(typeof(Program).Assembly.Location)!, moduleRoot);
        string moduleLocation = Path.GetFullPath(
            Path.Combine(applicationRunningRoot, moduleInfo.ModulePath, $"{moduleInfo.ModuleName}.dll"));

        try
        {
            ModuleLoadContext loadContext = new(moduleLocation);
            Assembly assembly = loadContext.LoadFromAssemblyName(AssemblyName.GetAssemblyName(moduleLocation));
            _logger.LogDebug("Discovered assembly: {AssemblyName}", assembly.GetName().Name);
            return assembly;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not load assembly from {ModuleFile}", moduleLocation);
        }

        return null;
    }
}