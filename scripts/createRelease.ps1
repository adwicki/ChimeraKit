$releasePath = ".\release"
$publishConfiguration = "Release"
$createArchive = $true
$dotnetVersion = "net10.0"
$solutionName = "ChimeraKit"

if (Test-Path $releasePath) {
    Write-Host "Removing existing release folder '$releasePath'" -ForegroundColor DarkCyan
    try {
        Remove-Item -Recurse -Force $releasePath
    } catch {
        Write-Error "Error occured, stopping"
        Write-Error $_
        exit 1
    }
}

Write-Host "Recreating folder '$releasePath'" -ForegroundColor DarkCyan
try {
    New-Item -Path $releasePath -ItemType Directory -ErrorAction Stop
} catch {
    Write-Error "Could not create directory, exiting..."
    exit 1
}

Write-Host "Cleaning solution" -ForegroundColor DarkCyan
dotnet clean ..\${solutionName}.sln --configuration $publishConfiguration

Write-Host "Starting to publish solution with configuration '$publishConfiguration'" -ForegroundColor DarkCyan
dotnet publish ..\${solutionName}.sln --configuration $publishConfiguration

Write-Host "Copying files from release directories" -ForegroundColor DarkCyan
Copy-Item -Path "..\src\$solutionName.Core\bin\Release\$dotnetVersion\publish\*" -Destination "$releasePath" -Recurse -Exclude (Get-ChildItem "$releasePath")
Copy-Item -Path "..\src\$solutionName.Host\bin\Release\$dotnetVersion\publish\*" -Destination "$releasePath" -Recurse -Exclude (Get-ChildItem "$releasePath")

function Copy-Module {
    param(
        [string]$ModuleName
    )

    Write-Host "Copying module '$moduleName'..." -ForegroundColor DarkCyan
    $destModule = "$releasePath\modules\$solutionName.Module.$moduleName"
    try {
        New-Item -Path $destModule -ItemType Directory -ErrorAction Stop
    } catch {
        Write-Error "Could not create directory, exiting..."
        exit 1
    }
    Copy-Item -Path "..\src\$solutionName.Module.$moduleName\bin\Release\$dotnetVersion\publish\*" -Destination $destModule -Recurse -Exclude (Get-ChildItem $destModule)
}

Copy-Module -ModuleName "ExamplePrepend"

if ($createArchive -eq $true) {
    Write-Host "Packaging application into zip-file" -ForegroundColor DarkCyan
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $zipFileName = "$releasePath`_$timestamp.zip"
    Compress-Archive -Path $releasePath -DestinationPath $zipFileName
}

Write-Host "All done" -ForegroundColor Green