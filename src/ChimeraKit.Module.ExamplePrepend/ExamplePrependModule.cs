using ChimeraKit.Core;
using ChimeraKit.Core.Abstractions;
using ChimeraKit.Core.Exceptions;
using ChimeraKit.Core.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ChimeraKit.Module.ExamplePrepend.Configuration;
using ChimeraKit.Module.ExamplePrepend.Services;
using ChimeraKit.Module.ExamplePrepend.Cli;

namespace ChimeraKit.Module.ExamplePrepend;

public class ExamplePrependModule : ModuleBase<ExamplePrependCliArguments>
{
    public override string Name => "ExamplePrepend";
    public override string Description => "Prepend a prefix-string to an input-string and capitalize the whole thing.";

    public override void ConfigureModuleServices(IServiceCollection services, IConfiguration configuration)
    {
        services.ConfigureAndRegister<ExamplePrependConfiguration>(configuration,
            ExamplePrependConfiguration.SectionName);

        services.AddTransient<IExamplePrependService, ExamplePrependService>();
    }

    protected override async Task<ExitCode> RunAsync(ExamplePrependCliArguments args, IModuleContext context)
    {
        IExamplePrependService moduleService = context.GetService<IExamplePrependService>();

        string result = await moduleService.ProcessAsync(args, context.CancellationToken);
        context.Logger.LogInformation("ExamplePrepend Result: {Result}", result);

        return ExitCode.Ok;
    }

    protected override void ValidateArguments(ExamplePrependCliArguments args, IModuleContext context)
    {
        // Example argument validation logic
        if (args.Input.Length <= 1)
        {
            throw new CliParseException("Input string must be at least 2 characters long.");
        }
    }
}
