using Microsoft.Extensions.Logging;

namespace ChimeraKit.Core.SharedServices;

public class ExampleExampleCapitalizationService : IExampleCapitalizationService
{
    private readonly ILogger<IExampleCapitalizationService> _logger;

    public ExampleExampleCapitalizationService(ILogger<IExampleCapitalizationService> logger)
    {
        _logger = logger;
    }
    
    public async Task<string> CapitalizeAsync(string input)
    {
        _logger.LogInformation("Starting capitalization");
        
        string capitalized = input.ToUpper();
        await Task.Delay(100);
        return capitalized;
    }
}