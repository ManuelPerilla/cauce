[CmdletBinding()]
param([string]$Version, [string]$Iscc = "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe")
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'common.ps1')
$Version = Resolve-CauceVersion -Version $Version
$root = Split-Path $PSScriptRoot -Parent
$out = Join-Path $root 'artifacts/release'
if (-not (Test-Path -LiteralPath $Iscc)) { throw 'Inno Setup 6 compiler was not found.' }
$numeric = ($Version -split '[-+]')[0]
$parts = $numeric.Split('.')
if ([int]$parts[0] -gt 255 -or [int]$parts[1] -gt 255 -or [int]$parts[2] -gt 65535) { throw 'Version exceeds Windows Installer limits.' }
function Escape-Xml([string]$Value) { [Security.SecurityElement]::Escape($Value) }
foreach ($arch in @('x64', 'arm64')) {
    $base = "Cauce-$Version-win-$arch-unsigned"
    $stage = Join-Path $out ("stage-win-$arch-" + [Guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Path $stage | Out-Null
    try {
        $payload = Join-Path $stage 'payload'
        Expand-Archive -LiteralPath (Join-Path $out "$base.zip") -DestinationPath $payload
        if (-not (Test-Path (Join-Path $payload 'Cauce.exe'))) { throw 'Portable package has no Cauce.exe.' }
        # Portable instructions and hashes describe the ZIP, not an installed application.
        Remove-Item -LiteralPath (Join-Path $payload 'README-PORTABLE.txt'), (Join-Path $payload 'INSTALLATION.md'), (Join-Path $payload 'SHA256SUMS.txt')
        $upgrade = if ($arch -eq 'x64') { '3A42CCB0-E39F-4B09-93E0-BB60F128A38A' } else { '675AA967-A702-4868-B2A8-64E4F353C19A' }
        $script:componentIds = [Collections.Generic.List[string]]::new()
        $script:index = 0
        function New-PayloadXml([string]$Directory) {
            $builder = [Text.StringBuilder]::new()
            foreach ($file in (Get-ChildItem -LiteralPath $Directory -File | Sort-Object Name)) {
                $script:index++
                $id = "C$script:index"
                $script:componentIds.Add($id)
                $shortcut = if ($file.Name -eq 'Cauce.exe') { '<Shortcut Id="StartMenuShortcut" Directory="ProgramMenuFolder" Name="Cauce (MSI)" Advertise="yes" WorkingDirectory="INSTALLFOLDER" />' } else { '' }
                [void]$builder.Append("<Component Id=`"$id`" Guid=`"*`"><File Id=`"F$script:index`" Source=`"$(Escape-Xml $file.FullName)`" KeyPath=`"yes`">$shortcut</File></Component>")
            }
            foreach ($childDirectory in (Get-ChildItem -LiteralPath $Directory -Directory | Sort-Object Name)) {
                $script:index++
                $directoryId = "D$script:index"
                $children = New-PayloadXml $childDirectory.FullName
                [void]$builder.Append("<Directory Id=`"$directoryId`" Name=`"$(Escape-Xml $childDirectory.Name)`">$children</Directory>")
            }
            $builder.ToString()
        }
        $files = New-PayloadXml $payload
        $refs = ($script:componentIds | ForEach-Object { "<ComponentRef Id=`"$_`" />" }) -join ''
        $wxs = @"
<Wix xmlns="http://wixtoolset.org/schemas/v4/wxs">
  <Package Name="Cauce ($arch)" Manufacturer="Cauce" Version="$numeric" UpgradeCode="$upgrade" Scope="perMachine">
    <MajorUpgrade DowngradeErrorMessage="A newer version of Cauce is installed." />
    <MediaTemplate EmbedCab="yes" />
    <StandardDirectory Id="ProgramFiles64Folder"><Directory Id="INSTALLFOLDER" Name="Cauce-MSI-$arch">$files</Directory></StandardDirectory>
    <StandardDirectory Id="ProgramMenuFolder" />
    <Feature Id="Main" Title="Cauce" Level="1">$refs</Feature>
  </Package>
</Wix>
"@
        $wxsPath = Join-Path $stage 'Package.wxs'
        $wxs | Set-Content -LiteralPath $wxsPath -Encoding utf8
        & wix build $wxsPath -arch $arch -o (Join-Path $out "$base.msi") -pdbtype none
        if ($LASTEXITCODE -ne 0) { throw "WiX build failed for $arch." }
        $allowed = if ($arch -eq 'arm64') { 'arm64' } else { 'x64compatible' }
        $iss = @"
[Setup]
AppId=Cauce-EXE-$arch
AppName=Cauce
AppVersion=$Version
AppPublisher=Cauce
AppPublisherURL=https://github.com/ManuelPerilla/cauce
DefaultDirName={autopf}\Cauce-EXE-$arch
DefaultGroupName=Cauce
UninstallDisplayIcon={app}\Cauce.exe
ArchitecturesAllowed=$allowed
ArchitecturesInstallIn64BitMode=$allowed
PrivilegesRequired=admin
OutputDir=$out
OutputBaseFilename=$base-setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
CloseApplications=yes
[Files]
Source: "$payload\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
[Icons]
Name: "{commonprograms}\Cauce (EXE)"; Filename: "{app}\Cauce.exe"
"@
        $issPath = Join-Path $stage 'Setup.iss'
        $iss | Set-Content -LiteralPath $issPath -Encoding utf8
        & $Iscc $issPath
        if ($LASTEXITCODE -ne 0) { throw "Inno Setup build failed for $arch." }
        foreach ($path in @((Join-Path $out "$base.msi"), (Join-Path $out "$base-setup.exe"))) {
            if ((Get-Item -LiteralPath $path).Length -eq 0) { throw "Empty installer: $path" }
        }
    } finally { Remove-CauceStage -Path $stage -OutputRoot $out }
}
$packages = @(Get-ChildItem -LiteralPath $out -File | Where-Object { $_.Extension -in @('.zip', '.msi', '.exe') })
if ($packages.Count -ne 6) { throw 'Expected EXE, MSI and portable ZIP for x64 and ARM64.' }
$packages | Sort-Object Name | ForEach-Object {
    '{0}  {1}' -f (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant(), $_.Name
} | Set-Content -LiteralPath (Join-Path $out 'SHA256SUMS.txt') -Encoding ascii
