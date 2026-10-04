[CmdletBinding()]
param([string]$Version)
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'common.ps1')
$Version = Resolve-CauceVersion -Version $Version
if (-not $IsWindows -or $env:CI -ne 'true' -or $env:RUNNER_ARCH -ne 'X64') {
    throw 'Run installer verification only on a disposable x64 Windows CI runner.'
}
$root = Split-Path $PSScriptRoot -Parent
$packages = Join-Path $root 'artifacts/release'
$logs = Join-Path $root 'artifacts/installer-verification'
New-Item -ItemType Directory -Path $logs -Force | Out-Null
$dataDirectory = Join-Path $env:LOCALAPPDATA 'Cauce'
if (Test-Path -LiteralPath $dataDirectory) { throw 'Refusing to use an existing Cauce profile.' }
New-Item -ItemType Directory -Path $dataDirectory | Out-Null
$data = Join-Path $dataDirectory 'library.json'
# Valid synthetic metadata; the installers must preserve it byte for byte.
'{"version":1,"tracks":[],"preferences":{"theme":"Sistema","genre":"Todos","artistSpacing":1,"onboardingCompleted":true}}' |
    Set-Content -LiteralPath $data -Encoding utf8
$dataHash = (Get-FileHash -LiteralPath $data).Hash
function Assert-DataPreserved {
    if (-not (Test-Path -LiteralPath $data) -or (Get-FileHash -LiteralPath $data).Hash -ne $dataHash) {
        throw 'Installer changed or removed the synthetic user library.'
    }
}
function Invoke-Installer([string]$Executable, [string[]]$Arguments) {
    $process = Start-Process -FilePath $Executable -ArgumentList $Arguments -PassThru
    if (-not $process.WaitForExit(180000)) {
        Stop-Process -Id $process.Id -Force
        throw "Installer timed out: $Executable"
    }
    if ($process.ExitCode -ne 0) { throw "Installer exited with $($process.ExitCode): $Executable" }
}
function Assert-Launch([string]$Directory) {
    $exe = Join-Path $Directory 'Cauce.exe'
    if (-not (Test-Path -LiteralPath $exe)) { throw "Installed executable missing: $exe" }
    $process = Start-Process -FilePath $exe -WorkingDirectory $Directory -PassThru
    try {
        $deadline = [DateTime]::UtcNow.AddSeconds(30)
        do {
            Start-Sleep -Milliseconds 500
            $process.Refresh()
            if ($process.HasExited) { throw "Installed Cauce exited during startup: $($process.ExitCode)" }
        } while ($process.MainWindowHandle -eq 0 -and [DateTime]::UtcNow -lt $deadline)
        if ($process.MainWindowHandle -eq 0) { throw 'Installed Cauce did not create a window.' }
        Write-Host "PASS installed application opens: $Directory"
    } finally {
        if (-not $process.HasExited) {
            Stop-Process -Id $process.Id -Force
            [void]$process.WaitForExit(10000)
        }
    }
}
$programs = [Environment]::GetFolderPath('CommonPrograms')
$base = Join-Path $packages "Cauce-$Version-win-x64-unsigned"
foreach ($format in @('EXE', 'MSI')) {
    $directory = Join-Path $env:ProgramFiles "Cauce-$format-x64"
    $shortcut = Join-Path $programs "Cauce ($format).lnk"
    if ((Test-Path -LiteralPath $directory) -or (Test-Path -LiteralPath $shortcut)) {
        throw "Refusing to change an existing $format installation."
    }
    $installer = if ($format -eq 'EXE') { "$base-setup.exe" } else { "$base.msi" }
    if ($format -eq 'EXE') {
        Invoke-Installer $installer @('/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART', '/SP-', "/LOG=`"$(Join-Path $logs 'exe-install.log')`"")
    } else {
        Invoke-Installer 'msiexec.exe' @('/i', "`"$installer`"", '/qn', '/norestart', '/L*v', "`"$(Join-Path $logs 'msi-install.log')`"")
    }
    if (-not (Test-Path -LiteralPath $shortcut)) { throw "Missing $format Start menu shortcut." }
    Assert-DataPreserved
    Assert-Launch $directory
    # Same-version reinstall is a repair check, not a cross-version upgrade test.
    if ($format -eq 'EXE') {
        Invoke-Installer $installer @('/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART', '/SP-', "/LOG=`"$(Join-Path $logs 'exe-reinstall.log')`"")
    } else {
        Invoke-Installer 'msiexec.exe' @('/fvomus', "`"$installer`"", '/qn', '/norestart', '/L*v', "`"$(Join-Path $logs 'msi-repair.log')`"")
    }
    Assert-DataPreserved
    Assert-Launch $directory
    if ($format -eq 'EXE') {
        Invoke-Installer (Join-Path $directory 'unins000.exe') @('/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART', "/LOG=`"$(Join-Path $logs 'exe-uninstall.log')`"")
    } else {
        Invoke-Installer 'msiexec.exe' @('/x', "`"$installer`"", '/qn', '/norestart', '/L*v', "`"$(Join-Path $logs 'msi-uninstall.log')`"")
    }
    if ((Test-Path -LiteralPath (Join-Path $directory 'Cauce.exe')) -or (Test-Path -LiteralPath $shortcut)) {
        throw "$format uninstall left its executable or shortcut."
    }
    Assert-DataPreserved
    Write-Host "PASS $format install, startup, repair, uninstall and metadata preservation."
}
@{
    version = $Version
    architecture = 'x64'
    passed = @('EXE install/startup/reinstall/uninstall', 'MSI install/startup/repair/uninstall', 'Start menu shortcuts', 'Synthetic library preserved')
    notTested = @('Native ARM64 installation', 'Cross-version upgrade', 'Real audio playback and codecs', 'Long-running performance', 'Publisher signature')
} | ConvertTo-Json -Depth 3 | Set-Content -LiteralPath (Join-Path $logs 'result.json') -Encoding utf8
