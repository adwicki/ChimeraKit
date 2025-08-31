using ChimeraKit.Module.ExamplePrepend.Cli;

namespace ChimeraKit.Module.ExamplePrepend.Services;

public interface IExamplePrependService
{
    Task<string> ProcessAsync(ExamplePrependCliArguments args, CancellationToken token);
}
