param(
    [ValidateSet('win-x64', 'win-arm64')][string]$Runtime = 'win-x64',
    [string]$Dotnet = 'dotnet',
    [string]$SigningThumbprint,
    [string]$SignTool = 'signtool.exe'
)
$ErrorActionPreference = 'Stop'
$repository = Split-Path $PSScriptRoot -Parent
$outputRoot = Join-Path $repository 'artifacts/cauce'
$publishDirectory = Join-Path $outputRoot ('stage-' + $Runtime + '-' + [Guid]::NewGuid().ToString('N'))
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
$checksums = Get-ChildItem -LiteralPath $publishDirectory -File -Recurse | Sort-Object FullName | ForEach-Object {
    '{0}  {1}' -f (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant(), [IO.Path]::GetRelativePath($publishDirectory, $_.FullName).Replace('\', '/')
}
$checksums | Set-Content -LiteralPath (Join-Path $publishDirectory 'SHA256SUMS.txt') -Encoding utf8
$zip = Join-Path $outputRoot "Cauce-0.6.0-alpha.1-$Runtime-$signatureLabel.zip"
Compress-Archive -Path (Join-Path $publishDirectory '*') -DestinationPath $zip -Force
$resolvedStage = (Resolve-Path -LiteralPath $publishDirectory).Path
$resolvedOutput = (Resolve-Path -LiteralPath $outputRoot).Path.TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
if (-not $resolvedStage.StartsWith($resolvedOutput, [StringComparison]::OrdinalIgnoreCase) -or (Split-Path $resolvedStage -Leaf) -notmatch '^stage-win-(x64|arm64)-[a-f0-9]{32}$') { throw 'Refusing to clean an unexpected staging directory.' }
Remove-Item -LiteralPath $resolvedStage -Recurse -Force
Write-Output $zip

