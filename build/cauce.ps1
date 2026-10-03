param(
    [ValidateSet('win-x64', 'win-arm64')][string]$Runtime = 'win-x64',
    [string]$Dotnet = 'dotnet',
    [string]$SigningThumbprint,
    [string]$SignTool = 'signtool.exe'
)
$ErrorActionPreference = 'Stop'
$repository = Split-Path $PSScriptRoot -Parent
$outputRoot = Join-Path $repository 'artifacts/cauce'
$publishDirectory = Join-Path $outputRoot $Runtime
# Artifacts are written only under the repository; publishing never changes the installed app.
New-Item -ItemType Directory -Path $publishDirectory -Force | Out-Null
& $Dotnet publish (Join-Path $repository 'src/Cauce.Desktop/Cauce.Desktop.csproj') -c Release -r $Runtime --self-contained true -o $publishDirectory -p:DebugType=None -p:DebugSymbols=false
if ($LASTEXITCODE -ne 0) { throw 'Cauce publish failed.' }

$signatureLabel = 'unsigned'
if ($SigningThumbprint) {
    if ($SigningThumbprint -notmatch '^[A-Fa-f0-9]{40}$') { throw 'Use the thumbprint of an installed code-signing certificate.' }
    foreach ($name in @('Cauce.exe', 'Cauce.dll', 'Cauce.Core.dll')) {
        $binary = Join-Path $publishDirectory $name
        & $SignTool sign /sha1 $SigningThumbprint /fd SHA256 /tr https://timestamp.digicert.com /td SHA256 $binary
        if ($LASTEXITCODE -ne 0) { throw "Signing failed: $name" }
        & $SignTool verify /pa $binary
        if ($LASTEXITCODE -ne 0) { throw "Signature verification failed: $name" }
    }
    $signatureLabel = 'signed'
}
$checksums = Get-ChildItem -LiteralPath $publishDirectory -File | Sort-Object Name | ForEach-Object {
    '{0}  {1}' -f (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant(), $_.Name
}
$checksums | Set-Content -LiteralPath (Join-Path $publishDirectory 'SHA256SUMS.txt') -Encoding utf8
$zip = Join-Path $outputRoot "Cauce-0.6.0-alpha.1-$Runtime-$signatureLabel.zip"
Compress-Archive -Path (Join-Path $publishDirectory '*') -DestinationPath $zip -Force
Write-Output $zip
