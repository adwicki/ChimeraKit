using ChimeraKit.Core;
using ChimeraKit.Core.Abstractions;
using ChimeraKit.Core.Configuration;
using ChimeraKit.Core.Extensions;
using ChimeraKit.Core.SharedServices;
using ChimeraKit.Host.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace ChimeraKit.Host;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        try
        {
            IConfiguration configuration = BuildConfiguration();
            ServiceCollection services = [];
            ConfigureServices(services, configuration);

            await using ServiceProvider serviceProvider = services.BuildServiceProvider();
            return (int)await RunApplicationAsync(serviceProvider, args);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal error: {ex.Message}");
            return (int)ExitCode.Error;
        }
    }

    private static IConfiguration BuildConfiguration()
    {
        string environment = Environment.GetEnvironmentVariable("CHIMERAKIT_ENV") ?? "Development";
        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{environment}.json", true)
            .Build();
    }
    
    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.ConfigureAndRegister<CoreConfiguration>(configuration, CoreConfiguration.SectionName);
        services.AddSingleton(configuration);

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddConfiguration(configuration.GetSection("Logging"));
            builder.AddConsole(options =>
            {
                options.FormatterName = "chimera-kit-formatter";
            });
            builder.AddConsoleFormatter<ChimeraKitConsoleLogFormatter, ChimeraKitConsoleLogFormatterOptions>();
            builder.AddSerilog(new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger());
        });

        services.AddSingleton<IModuleLoader, ModuleLoader>();
        
        // Add shared services
        services.AddSingleton<IExampleCapitalizationService, ExampleExampleCapitalizationService>();
        
        // Add module factory
        services.AddSingleton<IModuleServiceFactory>(_ => new ModuleServiceFactory(services, configuration));
    }

    private static async Task<ExitCode> RunApplicationAsync(IServiceProvider serviceProvider, string[] args)
    {
        ILogger logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        IModuleLoader moduleLoader = serviceProvider.GetRequiredService<IModuleLoader>();
        
        logger.LogInformation("Starting application execution");

        List<IModule> modules = moduleLoader.LoadModules();

        if (modules.Count == 0)
        {
            logger.LogWarning("No modules found");
            return ExitCode.Error;
        }

        if (args.Length == 0 || args[0] == "list")
        {
            Console.WriteLine("Available modules:");
            foreach (IModule module in modules)
            {
                Console.WriteLine($"   {module.Name} - {module.Description}");
            }

            return ExitCode.Ok;
        }

        string targetModuleName = args[0];
        IModule? targetModule = modules.FirstOrDefault(m =>
            string.Equals(m.Name, targetModuleName, StringComparison.OrdinalIgnoreCase));

        if (targetModule == null)
        {
            logger.LogError("Module {ModuleName} not found", targetModuleName);
            return ExitCode.Error;
        }
        
        IModuleServiceFactory moduleServiceFactory = serviceProvider.GetRequiredService<IModuleServiceFactory>();
        IServiceProvider moduleServiceProvider = moduleServiceFactory.CreateModuleServiceProvider(targetModule);
        
        ILogger moduleLogger = moduleServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger(targetModule.GetType());

        IModuleContext moduleContext = new ModuleContext(moduleServiceProvider, moduleLogger, CancellationToken.None);

        string[] moduleArgs = args.Skip(1).ToArray();
        logger.LogInformation("Executing module {ModuleName}", targetModule.Name);

        ExitCode result = await targetModule.ExecuteAsync(moduleContext, moduleArgs);
        
        logger.LogInformation("Module execution completed with result: {Result}", result);
        return result;
    }
}