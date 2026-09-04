using Microsoft.Extensions.DependencyInjection;

namespace ChimeraKit.Core.SharedServices;

/// <summary>
/// Optionally declares the DI lifetime a shared service is registered with. Apply it to a shared
/// service implementation; when it is absent the service is registered as a singleton.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class SharedServiceAttribute : Attribute
{
    public ServiceLifetime Lifetime { get; }

    public SharedServiceAttribute(ServiceLifetime lifetime = ServiceLifetime.Singleton)
    {
        Lifetime = lifetime;
    }
}
