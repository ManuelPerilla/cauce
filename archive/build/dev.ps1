[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
$Root = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$Project = Join-Path $Root "src\Cauce.Terminal\Cauce.Terminal.csproj"

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "The .NET 10 SDK is required for development. Releases themselves are self-contained."
}

& dotnet run --project $Project
exit $LASTEXITCODE
