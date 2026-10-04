[CmdletBinding()]
param([string]$Dotnet = 'dotnet')

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$repository = Split-Path $PSScriptRoot -Parent
& $Dotnet run --project (Join-Path $repository 'src/Cauce.Desktop/Cauce.Desktop.csproj')
if ($LASTEXITCODE -ne 0) { throw 'Cauce development run failed.' }
