[CmdletBinding()]
param(
    [string]$Version = "0.2.0",
    [ValidateSet("x64", "arm64", "x86")]
    [string[]]$Architectures = @("x64", "arm64", "x86"),
    [switch]$SkipInstaller
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$Root = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$Project = Join-Path $Root "src\NgMusic\NgMusic.csproj"
$InstallerProject = Join-Path $Root "packaging\windows\NgMusic.Setup.wixproj"
$Artifacts = Join-Path $Root "artifacts"
$PublishRoot = Join-Path $Artifacts "publish"
$ReleaseRoot = Join-Path $Artifacts "release"

function Assert-Command([string]$Name) {
    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "Required command '$Name' was not found. Install the .NET 10 SDK to build releases."
    }
}

function New-CleanDirectory([string]$Path) {
    if (Test-Path $Path) { Remove-Item -Recurse -Force $Path }
    New-Item -ItemType Directory -Path $Path | Out-Null
}

Assert-Command "dotnet"
New-CleanDirectory $Artifacts
New-Item -ItemType Directory -Path $PublishRoot, $ReleaseRoot -Force | Out-Null

Write-Host "NgMusic release $Version" -ForegroundColor Cyan
Write-Host "Architectures: $($Architectures -join ', ')"

foreach ($Arch in $Architectures) {
    $Rid = "win-$Arch"
    $PublishDir = Join-Path $PublishRoot $Rid
    New-Item -ItemType Directory -Path $PublishDir -Force | Out-Null

    Write-Host "`n[$Rid] Publishing self-contained single-file app..." -ForegroundColor Cyan
    & dotnet publish $Project `
        --configuration Release `
        --runtime $Rid `
        --self-contained true `
        -p:Version=$Version `
        -p:PublishDir="$PublishDir\" `
        -p:PublishSingleFile=true `
        -p:PublishReadyToRun=true `
        -p:PublishReadyToRunShowWarnings=true `
        -p:PublishTrimmed=false `
        -p:DebugType=None `
        -p:DebugSymbols=false
    if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed for $Rid." }

    $PortableStage = Join-Path $Artifacts "portable\$Rid"
    New-CleanDirectory $PortableStage
    Copy-Item -Path (Join-Path $PublishDir "*") -Destination $PortableStage -Recurse -Force

    $PortableReadme = @"
NgMusic $Version portable ($Rid)
================================

1. Put this folder anywhere you want.
2. Read INSTALLATION.md once to configure your Google OAuth client.
3. Run ngmusic.exe.

No .NET runtime installation is required. This build is self-contained.
Nothing is added to Program Files, the Start menu, or PATH.
OAuth tokens are still stored in Windows Credential Manager for the current Windows user.
"@
    Set-Content -Path (Join-Path $PortableStage "README-PORTABLE.txt") -Value $PortableReadme -Encoding UTF8
    Copy-Item (Join-Path $Root "docs\installation.md") (Join-Path $PortableStage "INSTALLATION.md") -Force

    $PortableZip = Join-Path $ReleaseRoot "NgMusic-$Version-$Rid-portable.zip"
    Write-Host "[$Rid] Creating portable ZIP..."
    Compress-Archive -Path (Join-Path $PortableStage "*") -DestinationPath $PortableZip -CompressionLevel Optimal

    if (-not $SkipInstaller) {
        Write-Host "[$Rid] Building MSI..."
        $MsiOut = Join-Path $Artifacts "msi\$Rid"
        New-Item -ItemType Directory -Path $MsiOut -Force | Out-Null

        $WixObj = Join-Path $Artifacts "wix-obj\$Rid"
        New-CleanDirectory $WixObj

        & dotnet build $InstallerProject `
            --configuration Release `
            --no-incremental `
            -p:Version=$Version `
            -p:InstallerPlatform=$Arch `
            -p:NgMusicPayloadDir=$PublishDir `
            -p:IntermediateOutputPath="$WixObj\" `
            -p:OutputPath="$MsiOut\"
        if ($LASTEXITCODE -ne 0) { throw "MSI build failed for $Rid." }

        $Msi = Get-ChildItem -Path $MsiOut -Filter "*.msi" -Recurse | Select-Object -First 1
        if (-not $Msi) { throw "MSI build completed but no MSI was found for $Rid." }
        Copy-Item $Msi.FullName (Join-Path $ReleaseRoot "NgMusic-$Version-$Rid.msi") -Force
    }
}

Write-Host "`nRelease artifacts:" -ForegroundColor Green
Get-ChildItem $ReleaseRoot | ForEach-Object { Write-Host " - $($_.Name)" }
