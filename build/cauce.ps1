[CmdletBinding()]
param(
    [ValidateSet('win-x64', 'win-arm64')][string]$Runtime = 'win-x64',
    [string]$Version,
    [string]$Dotnet = 'dotnet',
    [string]$SigningThumbprint,
    [string]$SignTool = 'signtool.exe'
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'common.ps1')
$Version = Resolve-CauceVersion -Version $Version
if ($SigningThumbprint -and $SigningThumbprint -notmatch '\A[A-Fa-f0-9]{40}\z') {
    throw 'Use the thumbprint of an installed code-signing certificate.'
}
$repository = Split-Path $PSScriptRoot -Parent
$outputRoot = Join-Path $repository 'artifacts/cauce'
$publishDirectory = Join-Path $outputRoot ('stage-' + $Runtime + '-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $publishDirectory -Force | Out-Null
try {
    & $Dotnet publish (Join-Path $repository 'src/Cauce.Desktop/Cauce.Desktop.csproj') -c Release -r $Runtime --self-contained true -o $publishDirectory "-p:Version=$Version" -p:DebugType=None -p:DebugSymbols=false
    if ($LASTEXITCODE -ne 0) { throw 'Cauce publish failed.' }

    $signatureLabel = 'unsigned'
    if ($SigningThumbprint) {
        foreach ($name in @('Cauce.exe', 'Cauce.dll', 'Cauce.Core.dll')) {
            $binary = Join-Path $publishDirectory $name
            & $SignTool sign /sha1 $SigningThumbprint /fd SHA256 /tr https://timestamp.digicert.com /td SHA256 $binary
            if ($LASTEXITCODE -ne 0) { throw "Signing failed: $name" }
            & $SignTool verify /pa $binary
            if ($LASTEXITCODE -ne 0) { throw "Signature verification failed: $name" }
        }
        $signatureLabel = 'signed'
    }
    @(
        "Cauce $Version ($Runtime)"
        ''
        'Extract this ZIP and run Cauce.exe. The .NET runtime is included.'
        'Local audio files are referenced, never copied into the library.'
        'External services are links; their catalogs and cloud sync are not integrated.'
        'Social sign-in remains unavailable until an identity service and providers are configured.'
        "Package signature: $signatureLabel. See docs/code-signing-policy.md in the repository."
        'Documentation and support: https://github.com/ManuelPerilla/cauce'
    ) | Set-Content -LiteralPath (Join-Path $publishDirectory 'README-PORTABLE.txt') -Encoding utf8
    # The repository guide links to sibling documents absent from a portable ZIP.
    # Generate a complete portable guide rather than shipping those broken links.
    @(
        "# Cauce $Version portable ($Runtime)"
        ''
        '## Install and listen'
        ''
        '1. Obtain this package from a maintained workflow artifact or reviewed repository release.'
        '2. Compare the ZIP SHA-256 hash with SHA256SUMS.txt from the same release. A hash is not a publisher signature.'
        '3. Extract the complete ZIP to a folder you can read. Keep the runtime and dependencies beside Cauce.exe.'
        '4. Open Cauce.exe, finish or dismiss the introduction, then use Biblioteca > Añadir archivos to choose local MP3, WAV or M4A files.'
        ''
        'The .NET runtime is included. Local playback needs no account, administrator installation or separate runtime.'
        'This portable package does not install into Program Files, change PATH, create shortcuts automatically or register an updater.'
        ''
        '## Files and accounts'
        ''
        'Music stays where you imported it. Moving or deleting a file makes its saved reference unavailable; import the new path if you move it.'
        'The library and preferences are stored in %LOCALAPPDATA%\Cauce\library.json, independently of the extracted application folder.'
        'External services remain links; streaming catalogs and cloud sync are not integrated.'
        'Social sign-in is unavailable until a distributor provides valid auth.json broker configuration beside Cauce.exe and registers the providers.'
        'Users do not need provider registrations or secrets for local playback.'
        ''
        '## Package trust'
        ''
        "Package signature label: $signatureLabel. SHA256SUMS.txt inside this ZIP records hashes of its files, excluding the checksum file itself."
        'An unsigned preview has no verified publisher signature. Windows may warn about the download.'
        'Verify the package source and checksum; preserve Windows security controls if execution is blocked.'
        ''
        '## Remove Cauce'
        ''
        'Exit the player, including its notification-area icon, then delete the extracted application folder.'
        'To remove metadata and preferences, first use Cuenta > Restablecer datos locales and confirm. Export beforehand if you want to keep your references.'
        'Reset removes Cauce data and recovery files without deleting audio. Deleting only the application folder preserves the local library.'
        ''
        '## Help and source'
        ''
        '[Repository and documentation](https://github.com/ManuelPerilla/cauce)'
        '[Report a problem](https://github.com/ManuelPerilla/cauce/issues)'
        'Review a report before sending it; do not include provider secrets, signing keys or personal library exports.'
    ) | Set-Content -LiteralPath (Join-Path $publishDirectory 'INSTALLATION.md') -Encoding utf8

    $checksums = Get-ChildItem -LiteralPath $publishDirectory -File -Recurse | Sort-Object FullName | ForEach-Object {
        '{0}  {1}' -f (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant(), [IO.Path]::GetRelativePath($publishDirectory, $_.FullName).Replace('\', '/')
    }
    $checksums | Set-Content -LiteralPath (Join-Path $publishDirectory 'SHA256SUMS.txt') -Encoding ascii
    $zip = Join-Path $outputRoot "Cauce-$Version-$Runtime-$signatureLabel.zip"
    Compress-Archive -Path (Join-Path $publishDirectory '*') -DestinationPath $zip -CompressionLevel Optimal -Force
    Write-Output $zip
} finally {
    Remove-CauceStage -Path $publishDirectory -OutputRoot $outputRoot
}
