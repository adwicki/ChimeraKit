using ChimeraKit.Core.Cli;
using ChimeraKit.Core.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChimeraKit.Core.Abstractions;

/// <summary>
/// Base class that owns the whole module lifecycle - argument parsing, validation, execution,
/// exception handling and exit-code mapping - so a concrete module only has to supply its domain
/// logic.
/// </summary>
/// <typeparam name="TArgs">The module's typed CLI options class (CommandLineParser attributes).</typeparam>
public abstract class ModuleBase<TArgs> : IModule where TArgs : class
{
    public abstract string Name { get; }
    public abstract string Description { get; }

    public abstract void ConfigureModuleServices(IServiceCollection services, IConfiguration configuration);

    /// <summary>Optional extra validation once arguments have parsed. Throw a
    /// <see cref="ChimeraKitException"/> to report a domain-level problem.</summary>
    protected virtual void ValidateArguments(TArgs args, IModuleContext context) { }

    /// <summary>The module's actual work. Everything around it is handled by the base class.</summary>
    protected abstract Task<ExitCode> RunAsync(TArgs args, IModuleContext context);

    public async Task<ExitCode> ExecuteAsync(IModuleContext context, string[] args)
    {
        ILogger logger = context.Logger;

        try
        {
            TArgs parsedArgs = CliArgumentParser.Parse<TArgs>(args);
            ValidateArguments(parsedArgs, context);

            logger.LogDebug("Starting {ModuleName} execution", Name);
            ExitCode result = await RunAsync(parsedArgs, context);
            logger.LogDebug("Module {ModuleName} completed with {Result}", Name, result);

            return result;
        }
        catch (CliParseException ex)
        {
            logger.LogError("{Message}", ex.Message);
            return ExitCode.InvalidArguments;
        }
        catch (ChimeraKitException ex)
        {
            logger.LogError("{Message}", ex.Message);
            return ExitCode.Error;
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Module {ModuleName} was cancelled", Name);
            return ExitCode.Cancelled;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Module {ModuleName} execution failed", Name);
            return ExitCode.Error;
        }
    }
}
