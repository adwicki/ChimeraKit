using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ChimeraKit.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static void ConfigureAndRegister<T>(this IServiceCollection services,
        IConfiguration configuration, string sectionName) where T : class, new()
    {
        services.Configure<T>(configuration.GetSection(sectionName));
        services.AddSingleton<T>(provider => provider.GetRequiredService<IOptions<T>>().Value);
    }
}