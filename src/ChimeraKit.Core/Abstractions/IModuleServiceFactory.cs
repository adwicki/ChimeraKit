namespace ChimeraKit.Core.Abstractions;

public interface IModuleServiceFactory
{
    IServiceProvider CreateModuleServiceProvider(IModule module);
}