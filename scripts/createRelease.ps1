<#
.SYNOPSIS
    Publishes ChimeraKit (host + core + all registered modules) into a self-consistent release folder
    and, optionally, a timestamped archive.

.DESCRIPTION
    The set of modules to ship is read from the host's appsettings.json (Core.AvailableModules).

.PARAMETER SkipTests
    Skip the tests.

.PARAMETER NoArchive
    Produce the release folder not the .zip archive.
#>
[CmdletBinding()]
param(
    [switch]$SkipTests,
    [switch]$NoArchive
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $false

$solutionName  = 'ChimeraKit'
$dotnetVersion = 'net10.0'
$publishConfig = 'Release'

$repoRoot     = Split-Path -Parent $PSScriptRoot
$solutionPath = Join-Path $repoRoot "$solutionName.sln"
$srcRoot      = Join-Path $repoRoot 'src'
$releasePath  = Join-Path $repoRoot 'release'
$hostSettings = Join-Path $srcRoot "$solutionName.Host\appsettings.json"

function Invoke-Dotnet {
    param(
        [Parameter(Mandatory)][string[]]$Arguments,
        [switch]$AllowFailure
    )
    
    & dotnet @Arguments 2>&1 | ForEach-Object { Write-Host $_ }
    if ($LASTEXITCODE -ne 0 -and -not $AllowFailure) {
        throw "dotnet $($Arguments -join ' ') failed with exit code $LASTEXITCODE"
    }
    return $LASTEXITCODE
}

if (-not (Test-Path $hostSettings)) {
    throw "Host settings not found at '$hostSettings'"
}

$config  = Get-Content $hostSettings -Raw | ConvertFrom-Json
$modules = @($config.Core.AvailableModules)

foreach ($module in $modules) {
    $projectFile = Join-Path $srcRoot "$($module.ModuleName)\$($module.ModuleName).csproj"
    if (-not (Test-Path $projectFile)) {
        throw "Module '$($module.ModuleName)' is registered in appsettings.json but no project exists at $projectFile"
    }
}
Write-Host "Manifest lists $($modules.Count) module(s): $(($modules.ModuleName) -join ', ')" -ForegroundColor DarkCyan

if ($SkipTests) {
    Write-Warning 'Skipping tests (-SkipTests). The release has not been verified.'
} else {
    Write-Host 'Running tests' -ForegroundColor DarkCyan
    Invoke-Dotnet @('test', $solutionPath, '--configuration', $publishConfig) | Out-Null
}

if (Test-Path $releasePath) {
    Write-Host "Removing existing release folder '$releasePath'" -ForegroundColor DarkCyan
    Remove-Item -Recurse -Force $releasePath
}
New-Item -Path $releasePath -ItemType Directory | Out-Null

Write-Host 'Cleaning solution' -ForegroundColor DarkCyan
if ((Invoke-Dotnet @('clean', $solutionPath, '--configuration', $publishConfig) -AllowFailure) -ne 0) {
    Write-Warning 'dotnet clean returned non-zero (usually nothing to clean); continuing.'
}

Write-Host "Publishing solution ($publishConfig)" -ForegroundColor DarkCyan
Invoke-Dotnet @('publish', $solutionPath, '--configuration', $publishConfig) | Out-Null

$hostPublish = Join-Path $srcRoot "$solutionName.Host\bin\$publishConfig\$dotnetVersion\publish"
Write-Host 'Copying host + core' -ForegroundColor DarkCyan
Copy-Item -Path (Join-Path $hostPublish '*') -Destination $releasePath -Recurse

$modulesRoot = Join-Path $releasePath 'modules'
New-Item -Path $modulesRoot -ItemType Directory | Out-Null

foreach ($module in $modules) {
    $modulePublish = Join-Path $srcRoot "$($module.ModuleName)\bin\$publishConfig\$dotnetVersion\publish"
    $destination = Join-Path $modulesRoot $module.ModulePath
    New-Item -Path $destination -ItemType Directory -Force | Out-Null
    Copy-Item -Path (Join-Path $modulePublish '*') -Destination $destination -Recurse
    Write-Host "Copied module '$($module.ModuleName)'" -ForegroundColor DarkCyan
}

if ($NoArchive) {
    Write-Host 'Skipping archive (-NoArchive)' -ForegroundColor DarkCyan
} else {
    $timestamp = Get-Date -Format 'yyyyMMdd_HHmmss'
    $zipPath = Join-Path $repoRoot "release_$timestamp.zip"
    Write-Host "Packaging release into '$zipPath'" -ForegroundColor DarkCyan
    Compress-Archive -Path $releasePath -DestinationPath $zipPath
}

Write-Host 'All done' -ForegroundColor Green
