namespace ChimeraKit.Core.SharedServices;

public interface IExampleCapitalizationService
{
    Task<string> CapitalizeAsync(string input);
}