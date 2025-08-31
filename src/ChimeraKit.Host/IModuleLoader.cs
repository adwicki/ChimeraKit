using ChimeraKit.Core.Abstractions;

namespace ChimeraKit.Host;

public interface IModuleLoader
{
    List<IModule> LoadModules();
}