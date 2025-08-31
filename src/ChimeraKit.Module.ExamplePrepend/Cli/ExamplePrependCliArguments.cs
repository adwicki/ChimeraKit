using CommandLine;

namespace ChimeraKit.Module.ExamplePrepend.Cli;

public class ExamplePrependCliArguments
{
    [Option('i', "Input",
        Required = true,
        HelpText = "The input string")]
    public required string Input { get; set; }
    
    [Option('p', "Prefix", 
        Required = true, 
        HelpText = "The prefix")]
    public required string Prefix { get; set; }
}
