[CmdletBinding()]
param(
    [string]$Version,
    [ValidateSet('x64', 'arm64')][string[]]$Architectures = @('x64', 'arm64'),
    [string]$Dotnet = 'dotnet',
    [string]$SigningThumbprint,
    [string]$SignTool = 'signtool.exe'
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'common.ps1')
$Version = Resolve-CauceVersion -Version $Version
if ($Architectures.Count -eq 0) { throw 'Choose at least one supported architecture.' }
$repository = Split-Path $PSScriptRoot -Parent
$releaseDirectory = Join-Path $repository 'artifacts/release'
New-Item -ItemType Directory -Path $releaseDirectory -Force | Out-Null
$signatureLabel = if ($SigningThumbprint) { 'signed' } else { 'unsigned' }
$packages = foreach ($architecture in ($Architectures | Select-Object -Unique)) {
    $runtime = "win-$architecture"
    & (Join-Path $PSScriptRoot 'cauce.ps1') -Runtime $runtime -Version $Version -Dotnet $Dotnet -SigningThumbprint $SigningThumbprint -SignTool $SignTool | Out-Host
    $name = "Cauce-$Version-$runtime-$signatureLabel.zip"
    $target = Join-Path $releaseDirectory $name
    Copy-Item -LiteralPath (Join-Path $repository "artifacts/cauce/$name") -Destination $target -Force
    Get-Item -LiteralPath $target
}
$packages | Sort-Object Name | ForEach-Object {
    '{0}  {1}' -f (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant(), $_.Name
} | Set-Content -LiteralPath (Join-Path $releaseDirectory 'SHA256SUMS.txt') -Encoding ascii
Write-Output $releaseDirectory
