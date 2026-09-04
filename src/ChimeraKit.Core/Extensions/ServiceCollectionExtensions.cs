using System.Reflection;
using ChimeraKit.Core.SharedServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ChimeraKit.Core.Extensions;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void ConfigureAndRegister<T>(IConfiguration configuration, string sectionName) where T : class, new()
        {
            services.Configure<T>(configuration.GetSection(sectionName));
            services.AddSingleton<T>(provider => provider.GetRequiredService<IOptions<T>>().Value);
        }

        /// <summary>
        /// Registers every shared service by convention: any concrete class in <paramref name="assembly"/>
        /// (the Core assembly by default) that implements an interface deriving from
        /// <see cref="ISharedService"/> is registered against that interface. New shared services are
        /// picked up automatically - they only need their interface to derive from
        /// <see cref="ISharedService"/>, with no wiring in the host. The lifetime is singleton unless the
        /// implementation carries a <see cref="SharedServiceAttribute"/> declaring a different one.
        /// </summary>
        public IServiceCollection AddSharedServices(Assembly? assembly = null)
        {
            Assembly sharedAssembly = assembly ?? typeof(ISharedService).Assembly;

            IEnumerable<Type> implementations = sharedAssembly.GetTypes()
                .Where(type => type is { IsClass: true, IsAbstract: false });

            foreach (Type implementation in implementations)
            {
                Type[] sharedInterfaces = implementation.GetInterfaces()
                    .Where(serviceInterface => serviceInterface != typeof(ISharedService)
                                               && typeof(ISharedService).IsAssignableFrom(serviceInterface))
                    .ToArray();

                if (sharedInterfaces.Length == 0)
                {
                    continue;
                }

                ServiceLifetime lifetime = implementation.GetCustomAttribute<SharedServiceAttribute>()?.Lifetime
                                           ?? ServiceLifetime.Singleton;

                foreach (Type serviceInterface in sharedInterfaces)
                {
                    services.Add(new ServiceDescriptor(serviceInterface, implementation, lifetime));
                }
            }

            return services;
        }
    }
}
