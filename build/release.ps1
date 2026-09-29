[CmdletBinding()]
param(
    [string]$Version = "0.5.0",
    [ValidateSet("x64", "arm64", "x86")]
    [string[]]$Architectures = @("x64", "arm64", "x86"),
    [switch]$SkipInstaller
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$Root = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$Project = Join-Path $Root "src\NgMusic\NgMusic.csproj"
$InstallerProject = Join-Path $Root "packaging\windows\NgMusic.Setup.wixproj"
$GuiSetupProject = Join-Path $Root "src\NgMusic.Setup\NgMusic.Setup.csproj"
$ConfiguratorProject = Join-Path $Root "src\NgMusic.Configurator\NgMusic.Configurator.csproj"
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

    Write-Host "`n[$Rid] Publishing app..." -ForegroundColor Cyan
    & dotnet publish $Project `
        --configuration Release `
        --runtime $Rid `
        --self-contained true `
        -p:Version=$Version `
        -p:PublishDir="$PublishDir\" `
        -p:PublishSingleFile=true `
        -p:PublishReadyToRun=true `
        -p:PublishTrimmed=false `
        -p:DebugType=None `
        -p:DebugSymbols=false
    if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed for $Rid." }

    $PortableStage = Join-Path $Artifacts "portable\$Rid"
    New-CleanDirectory $PortableStage
    Copy-Item -Path (Join-Path $PublishDir "*") -Destination $PortableStage -Recurse -Force
    Copy-Item (Join-Path $Root "docs\installation.md") (Join-Path $PortableStage "INSTALLATION.md") -Force

    @"
NgMusic $Version portable ($Rid)
================================
Run ngmusic.exe. On first login, NgMusic can configure the Google OAuth Client ID interactively.
No .NET runtime is required. Portable mode does not modify Program Files or PATH.
"@ | Set-Content -Path (Join-Path $PortableStage "README-PORTABLE.txt") -Encoding UTF8

    $PortableZip = Join-Path $ReleaseRoot "NgMusic-$Version-$Rid-portable.zip"
    Compress-Archive -Path (Join-Path $PortableStage "*") -DestinationPath $PortableZip -CompressionLevel Optimal

    if (-not $SkipInstaller) {
        Write-Host "[$Rid] Publishing post-install configurator..."
        $ConfiguratorOut = Join-Path $Artifacts "configurator\$Rid"
        New-CleanDirectory $ConfiguratorOut

        & dotnet publish $ConfiguratorProject `
            --configuration Release `
            --runtime $Rid `
            --self-contained true `
            -p:Version=$Version `
            "-p:PublishDir=$ConfiguratorOut\" `
            -p:PublishSingleFile=true `
            -p:PublishTrimmed=false
        if ($LASTEXITCODE -ne 0) { throw "Configurator build failed for $Rid." }

        $ConfiguratorExe = Get-ChildItem $ConfiguratorOut -Filter "NgMusicConfigurator.exe" -File | Select-Object -First 1
        if (-not $ConfiguratorExe) { throw "NgMusicConfigurator.exe was not found for $Rid." }

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
            "-p:NgMusicConfiguratorPath=$($ConfiguratorExe.FullName)" `
            -p:IntermediateOutputPath="$WixObj\" `
            -p:OutputPath="$MsiOut\"
        if ($LASTEXITCODE -ne 0) { throw "MSI build failed for $Rid." }

        $Msi = Get-ChildItem $MsiOut -Filter "*.msi" -Recurse | Select-Object -First 1
        if (-not $Msi) { throw "MSI build completed but no MSI was found for $Rid." }
        Copy-Item $Msi.FullName (Join-Path $ReleaseRoot "NgMusic-$Version-$Rid.msi") -Force

        Write-Host "[$Rid] Building graphical Setup.exe..."
        $GuiSetupOut = Join-Path $Artifacts "gui-setup\$Rid"
        New-CleanDirectory $GuiSetupOut

        & dotnet publish $GuiSetupProject `
            --configuration Release `
            --runtime $Rid `
            --self-contained true `
            -p:Version=$Version `
            "-p:MsiPath=$($Msi.FullName)" `
            "-p:PublishDir=$GuiSetupOut\" `
            -p:PublishSingleFile=true `
            -p:PublishTrimmed=false
        if ($LASTEXITCODE -ne 0) { throw "Graphical setup build failed for $Rid." }

        $SetupExe = Get-ChildItem $GuiSetupOut -Filter "NgMusicSetup.exe" -File | Select-Object -First 1
        if (-not $SetupExe) { throw "NgMusicSetup.exe was not found for $Rid." }
        Copy-Item $SetupExe.FullName (Join-Path $ReleaseRoot "NgMusic-$Version-$Rid-setup.exe") -Force
    }
}

Write-Host "`nRelease artifacts:" -ForegroundColor Green
Get-ChildItem $ReleaseRoot | ForEach-Object { Write-Host " - $($_.Name)" }
