param(
    [Parameter(Mandatory=$true)]
    [string]$ModuleName,
    
    [Parameter(Mandatory=$false)]
    [string]$Description = "A new module for ChimeraKit",
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipBuild
)

if ($ModuleName -notmatch '^[A-Za-z][A-Za-z0-9]*$') {
    Write-Error "Module name must be alphanumeric and start with a letter"
    exit 1
}

$ProjectName = "ChimeraKit.Module.$ModuleName"
$ProjectDir = "src\$ProjectName"
$NetVersion = "net10.0"

Write-Host "Creating new module: $ProjectName" -ForegroundColor DarkCyan
Write-Host "Description: $Description" -ForegroundColor DarkCyan
Write-Host ""

if (Test-Path $ProjectDir) {
    Write-Error "Module $ProjectName already exists at $ProjectDir"
    exit 1
}

try {
    Write-Host "Creating project..." -ForegroundColor DarkCyan
    dotnet new classlib -n $ProjectName -o $ProjectDir
    if ($LASTEXITCODE -ne 0) { throw "Failed to create project" }

    Write-Host "Adding to solution..." -ForegroundColor DarkCyan
    dotnet sln add "$ProjectDir\$ProjectName.csproj" --in-root
    if ($LASTEXITCODE -ne 0) { throw "Failed to add to solution" }

    Write-Host "Updating project file..." -ForegroundColor DarkCyan
    $projectContent = @"
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>$NetVersion</TargetFramework>
	<ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <EnableDynamicLoading>true</EnableDynamicLoading>
  </PropertyGroup>
  
  <ItemGroup>
    <ProjectReference Include="..\ChimeraKit.Core\ChimeraKit.Core.csproj">
      <Private>false</Private>
      <ExcludeAssets>runtime</ExcludeAssets>
    </ProjectReference>
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="CommandLineParser" Version="2.9.1" />
  </ItemGroup>

</Project>
"@
    $projectContent | Out-File -FilePath "$ProjectDir\$ProjectName.csproj" -Encoding UTF8

    New-Item -ItemType Directory -Path "$ProjectDir\Services" -Force | Out-Null
    New-Item -ItemType Directory -Path "$ProjectDir\Configuration" -Force | Out-Null
    New-Item -ItemType Directory -Path "$ProjectDir\Cli" -Force | Out-Null

    # Config Class
    Write-Host "Creating configuration class..." -ForegroundColor DarkCyan
    $configContent = @"
namespace $ProjectName.Configuration;

public class $($ModuleName)Configuration
{
    public const string SectionName = "$ModuleName";
    
    public string ModuleSpecificSetting { get; set; } = "ModuleSpecificSetting";
}
"@
    $configContent | Out-File -FilePath "$ProjectDir\Configuration\$($ModuleName)Configuration.cs" -Encoding UTF8

    # Cli arguments class
    Write-Host "Creating cli argument class..." -ForegroundColor DarkCyan
    $configContent = @"
using CommandLine;

namespace $ProjectName.Cli;

public class $($ModuleName)CliArguments
{
    [Option('i', "Input",
        Required = true,
        HelpText = "An input")]
    public required string Input { get; set; }
}
"@
    $configContent | Out-File -FilePath "$ProjectDir\Cli\$($ModuleName)CliArguments.cs" -Encoding UTF8

    # Service interface and implementation
    Write-Host "Creating service classes..." -ForegroundColor DarkCyan
	$serviceInterfaceContent = @"
using $ProjectName.Cli;
	
namespace $ProjectName.Services;

public interface I$($ModuleName)Service
{
    Task ProcessAsync($($ModuleName)CliArguments args);
}
"@
	$serviceInterfaceContent | Out-File -FilePath "$ProjectDir\Services\I$($ModuleName)Service.cs" -Encoding UTF8
	
    $serviceContent = @"
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using $ProjectName.Configuration;
using $ProjectName.Cli;

namespace $ProjectName.Services;

public class $($ModuleName)Service : I$($ModuleName)Service
{
    private readonly ILogger<I$($ModuleName)Service> _logger;
    private readonly $($ModuleName)Configuration _config;

    public $($ModuleName)Service(
        ILogger<I$($ModuleName)Service> logger,
        $($ModuleName)Configuration config)
    {
        _logger = logger;
        _config = config;
    }

    public async Task ProcessAsync($($ModuleName)CliArguments args)
    {
        _logger.LogInformation("$($ModuleName)Service processing: {Input}", args.Input);

        // TODO: Implement your module logic here
        await Task.Delay(100); // Simulate processing
    }
}
"@
    $serviceContent | Out-File -FilePath "$ProjectDir\Services\$($ModuleName)Service.cs" -Encoding UTF8

    # Create main module class
    Write-Host "Creating main module class..." -ForegroundColor DarkCyan
    $moduleContent = @"
using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ChimeraKit.Core;
using ChimeraKit.Core.Abstractions;
using ChimeraKit.Core.Extensions;
using ChimeraKit.Core.Exceptions;
using ChimeraKit.Core.SharedServices;
using $ProjectName.Configuration;
using $ProjectName.Services;
using $ProjectName.Cli;

namespace $ProjectName;

public class $($ModuleName)Module : IModule
{
    public string Name => "$ModuleName";
    public string Description => "$Description";

    public void ConfigureModuleServices(IServiceCollection services, IConfiguration configuration)
    {
        services.ConfigureAndRegister<$($ModuleName)Configuration>(configuration,
            $($ModuleName)Configuration.SectionName);

        services.AddTransient<I$($ModuleName)Service, $($ModuleName)Service>();
    }

    public async Task<ExitCode> ExecuteAsync(IModuleContext context, string[] args)
    {
        ILogger logger = context.Logger;
        logger.LogInformation("Starting {ModuleName} execution", Name);
        
        I$($ModuleName)Service moduleService = context.GetService<I$($ModuleName)Service>();
        $($ModuleName)CliArguments cliArgs = ParseCliArguments(args);

        try
        {
            await moduleService.ProcessAsync(cliArgs);
            logger.LogInformation("Module {ModuleName} completed successfully", Name);
            return ExitCode.Ok;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Module {ModuleName} execution failed", Name);
            return ExitCode.Error;
        }
    }
    
    private static $($ModuleName)CliArguments ParseCliArguments(string[] args)
    {
        ParserResult<$($ModuleName)CliArguments> parseResult = Parser.Default
            .ParseArguments<$($ModuleName)CliArguments>(args);
            
        if (parseResult.Errors.Any())
        {
            throw new CliParseException(
                $"Error parsing cli args: {string.Join(Environment.NewLine, parseResult.Errors)}");
        }
        
        $($ModuleName)CliArguments parsedArgs = parseResult.Value;
        
        // Do more validation
        
        return parsedArgs;
    }
}
"@
    $moduleContent | Out-File -FilePath "$ProjectDir\$($ModuleName)Module.cs" -Encoding UTF8

    # Remove default Class1.cs
    $defaultFile = "$ProjectDir\Class1.cs"
    if (Test-Path $defaultFile) {
        Remove-Item $defaultFile
    }

    Write-Host "Updating appsettings.json..." -ForegroundColor DarkCyan
    $appsettingsPath = "src\ChimeraKit.Host\appsettings.json"
    
    if (Test-Path $appsettingsPath) {
        $appsettings = Get-Content $appsettingsPath -Raw | ConvertFrom-Json
        
        # Add module configuration section
        $moduleConfig = @{
            "ModuleSpecificSetting" = "ModSpecSetting"
        }
        
        $appsettings | Add-Member -Type NoteProperty -Name "$ModuleName" -Value $moduleConfig
        
        # Add to available modules
        $newAvailableModule = @{
            "ModuleName" = $ProjectName
            "ModulePath" = $ProjectName
        }
        
        $appsettings.Core.AvailableModules += $newAvailableModule
        
        $appsettings | ConvertTo-Json -Depth 10 | Out-File -FilePath $appsettingsPath -Encoding UTF8
        Write-Host "Added $ModuleName configuration to appsettings.json" -ForegroundColor DarkGreen
    }
    
    Write-Host "Updating appsettings.Development.json..." -ForegroundColor DarkCyan
    $appsettingsDevPath = "src\ChimeraKit.Host\appsettings.Development.json"

    if (Test-Path $appsettingsDevPath) {
        $appsettingsDev = Get-Content $appsettingsDevPath -Raw | ConvertFrom-Json

        $newAvailableModule = @{
            "ModuleName" = $ProjectName
            "ModulePath" = "$ProjectName\bin\Debug\$NetVersion"
        }

        $appsettingsDev.Core.AvailableModules += $newAvailableModule

        $appsettingsDev | ConvertTo-Json -Depth 10 | Out-File -FilePath $appsettingsDevPath -Encoding UTF8
        Write-Host "Added $ModuleName configuration to appsettings.Development.json" -ForegroundColor DarkGreen
    }

    if (-not $SkipBuild) {
        Write-Host "Building solution..." -ForegroundColor DarkCyan
        dotnet build
        if ($LASTEXITCODE -ne 0) { 
            Write-Warning "Build failed, but module was created successfully"
        } else {
            Write-Host "Build completed successfully" -ForegroundColor DarkGreen
        }
    }

    Write-Host ""
    Write-Host "Module $ProjectName created successfully!" -ForegroundColor DarkGreen
    Write-Host "Configuration section '$ModuleName' has been added to appsettings.json" -ForegroundColor DarkGreen
} catch {
    Write-Error "Failed to create module: $_"
    
    if (Test-Path $ProjectDir) {
        Write-Host "Cleaning up..." -ForegroundColor DarkCyan
        Remove-Item $ProjectDir -Recurse -Force
    }
    
    exit 1
}