using ChimeraKit.Core.SharedServices;
using ChimeraKit.Module.ExamplePrepend.Cli;
using ChimeraKit.Module.ExamplePrepend.Configuration;
using ChimeraKit.Module.ExamplePrepend.Services;
using ChimeraKit.Tests.Abstractions;
using NSubstitute;
using NSubstitute.Extensions;

namespace ChimeraKit.Tests.Module.ExamplePrepend;

public class ExamplePrependTests
{
    private readonly ITestOutputHelper _outputHelper;

    public ExamplePrependTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }
    
    [Theory]
    [InlineData("lower", "prefix", "*", "PREFIX*LOWER")]
    [InlineData("ALREADYUPPER", "anotherPrefix", "_", "ANOTHERPREFIX_ALREADYUPPER")]
    public async Task Extraction_Should_Extract_Files(string input, string prefix, string separationChar, string expected)
    {
        // Arrange
        IExampleCapitalizationService exampleCapitalizationService = Substitute.For<IExampleCapitalizationService>();
        exampleCapitalizationService.Configure()
            .CapitalizeAsync(Arg.Any<string>())
            .Returns(info => Task.FromResult(info.ArgAt<string>(0).ToUpper()));
        
        IExamplePrependService prependService = new ExamplePrependService(
            XUnitLogger.CreateLogger<IExamplePrependService>(_outputHelper), 
            new ExamplePrependConfiguration
            {
                SeparationCharacter = separationChar
            }, exampleCapitalizationService);
        
        // Act
        string result = await prependService.ProcessAsync(new ExamplePrependCliArguments
        {
            Input = input,
            Prefix = prefix
        }, CancellationToken.None);
        
        // Assert
        Assert.Equal(expected, result);
    }
}