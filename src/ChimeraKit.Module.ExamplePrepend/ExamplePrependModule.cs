using ChimeraKit.Core;
using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ChimeraKit.Core.Abstractions;
using ChimeraKit.Core.Extensions;
using ChimeraKit.Core.Exceptions;
using ChimeraKit.Module.ExamplePrepend.Configuration;
using ChimeraKit.Module.ExamplePrepend.Services;
using ChimeraKit.Module.ExamplePrepend.Cli;

namespace ChimeraKit.Module.ExamplePrepend;

public class ExamplePrependModule : IModule
{
    public string Name => "ExamplePrepend";
    public string Description => "A module that prepends a string and capitalizes it.";

    public void ConfigureModuleServices(IServiceCollection services, IConfiguration configuration)
    {
        services.ConfigureAndRegister<ExamplePrependConfiguration>(configuration,
            ExamplePrependConfiguration.SectionName);

        services.AddTransient<IExamplePrependService, ExamplePrependService>();
    }

    public async Task<ExitCode> ExecuteAsync(IModuleContext context, string[] args)
    {
        ILogger logger = context.Logger;
        IExamplePrependService moduleService = context.GetService<IExamplePrependService>();
        ExamplePrependCliArguments cliArgs = ParseCliArguments(args);

        logger.LogInformation("Starting {ModuleName} execution", Name);

        try
        {
            string result = await moduleService.ProcessAsync(cliArgs, context.CancellationToken);
            
            Console.WriteLine($"ExamplePrepend Result: {result}");

            logger.LogInformation("Module {ModuleName} completed successfully", Name);
            return ExitCode.Ok;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Module {ModuleName} execution failed", Name);
            return ExitCode.Error;
        }
    }
    
    private static ExamplePrependCliArguments ParseCliArguments(string[] args)
    {
        ParserResult<ExamplePrependCliArguments> parseResult = Parser.Default
            .ParseArguments<ExamplePrependCliArguments>(args);
            
        if (parseResult.Errors.Any())
        {
            throw new CliParseException(
                $"Error parsing cli args: {string.Join(Environment.NewLine, parseResult.Errors)}");
        }
        
        ExamplePrependCliArguments parsedArgs = parseResult.Value;
        
        // Do more validation
        
        return parsedArgs;
    }
}
