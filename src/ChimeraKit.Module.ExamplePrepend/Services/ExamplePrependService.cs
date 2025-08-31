using ChimeraKit.Core.SharedServices;
using ChimeraKit.Module.ExamplePrepend.Cli;
using Microsoft.Extensions.Logging;
using ChimeraKit.Module.ExamplePrepend.Configuration;

namespace ChimeraKit.Module.ExamplePrepend.Services;

public class ExamplePrependService : IExamplePrependService
{
    private readonly ILogger<IExamplePrependService> _logger;
    private readonly ExamplePrependConfiguration _config;
    private readonly IExampleCapitalizationService _exampleCapitalizationService;

    public ExamplePrependService(
        ILogger<IExamplePrependService> logger,
        ExamplePrependConfiguration config,
        IExampleCapitalizationService exampleCapitalizationService)
    {
        _logger = logger;
        _config = config;
        _exampleCapitalizationService = exampleCapitalizationService;
    }

    public async Task<string> ProcessAsync(ExamplePrependCliArguments args, CancellationToken token)
    {
        _logger.LogInformation("ExamplePrependService starts processing");

        string prepended = $"{args.Prefix}{_config.SeparationCharacter}{args.Input}";
        await Task.Delay(100, token);

        // Use a service shared between all modules
        string capitalized = await _exampleCapitalizationService.CapitalizeAsync(prepended);
        
        return capitalized;
    }
}
