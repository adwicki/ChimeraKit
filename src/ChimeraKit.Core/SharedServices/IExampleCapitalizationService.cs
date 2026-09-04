namespace ChimeraKit.Core.SharedServices;

public interface IExampleCapitalizationService : ISharedService
{
    Task<string> CapitalizeAsync(string input);
}