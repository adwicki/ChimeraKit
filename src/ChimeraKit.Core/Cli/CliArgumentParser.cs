using ChimeraKit.Core.Exceptions;
using CommandLine;

namespace ChimeraKit.Core.Cli;

/// <summary>
/// Single entry point for turning a raw <c>string[]</c> into a typed options object. Modules declare
/// an options class decorated with CommandLineParser <c>[Option]</c> attributes.
/// </summary>
public static class CliArgumentParser
{
    public static TArgs Parse<TArgs>(string[] args) where TArgs : class
    {
        ParserResult<TArgs> result = Parser.Default.ParseArguments<TArgs>(args);

        if (result.Errors.Any())
        {
            throw new CliParseException(
                $"Error parsing CLI arguments:{Environment.NewLine}" +
                string.Join(Environment.NewLine, result.Errors));
        }

        return result.Value;
    }
}
