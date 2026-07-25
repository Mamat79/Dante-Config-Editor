param(
    [string]$InstallerPath = "",
    [string]$ExpectedVersion = "3.6",
    [switch]$AllowCustomInstallLocation
)

$ErrorActionPreference = "Stop"

$root = Split-Path $PSScriptRoot -Parent
if ([string]::IsNullOrWhiteSpace($InstallerPath)) {
    $InstallerPath = Join-Path $root "dist\DanteConfigEditorV3_6_Installer.exe"
}

$installer = (Resolve-Path -LiteralPath $InstallerPath -ErrorAction Stop).Path
$identity = [Security.Principal.WindowsIdentity]::GetCurrent()
$principal = [Security.Principal.WindowsPrincipal]::new($identity)
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    throw "Ce test d'installation doit être lancé dans PowerShell en tant qu'administrateur."
}

$targetRegistryPaths = @(
    "HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\{A11FA3C8-3461-46CA-AC61-6A14316E8DBB}_is1",
    "HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\{A11FA3C8-3461-46CA-AC61-6A14316E8DBB}_is1"
)

# V3.4.2 reste la version stable installée en parallèle. Le test vérifie que
# l'installation et la mise à niveau V3.6 ne modifient jamais cette entrée.
$stableRegistryPaths = @(
    "HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\{76E68F80-5C89-4415-A090-370CA60EB3AD}_is1",
    "HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\{76E68F80-5C89-4415-A090-370CA60EB3AD}_is1"
)

function Get-InstallRecords {
    param([Parameter(Mandatory = $true)][string[]]$Paths)

    return @(
        foreach ($registryPath in $Paths) {
            if (Test-Path -LiteralPath $registryPath) {
                Get-ItemProperty -LiteralPath $registryPath
            }
        }
    )
}

function Get-TargetInstallRecords {
    return @(Get-InstallRecords -Paths $targetRegistryPaths)
}

function Get-StableInstallRecords {
    return @(Get-InstallRecords -Paths $stableRegistryPaths)
}

function Get-StableSnapshot {
    return @(
        Get-StableInstallRecords | ForEach-Object {
            $installLocation = [string]$_.InstallLocation
            $exePath = if ($installLocation) { Join-Path $installLocation "DanteConfigEditorV3.exe" } else { "" }
            $hash = if ($exePath -and (Test-Path -LiteralPath $exePath)) {
                (Get-FileHash -LiteralPath $exePath -Algorithm SHA256).Hash
            }
            else {
                ""
            }

            "$($_.DisplayVersion)|$installLocation|$hash"
        } | Sort-Object
    )
}

function Invoke-InstallerPass {
    param([Parameter(Mandatory = $true)][string]$Name)

    Write-Host "$Name : $installer"
    $process = Start-Process -FilePath $installer `
        -ArgumentList @("/VERYSILENT", "/SUPPRESSMSGBOXES", "/NORESTART", "/SP-", "/LANG=french") `
        -WindowStyle Hidden `
        -Wait `
        -PassThru

    if ($process.ExitCode -ne 0) {
        throw "$Name a échoué avec le code retour $($process.ExitCode)."
    }
}

function Assert-StableInstallUnchanged {
    param(
        [Parameter(Mandatory = $true)][string]$Step,
        [AllowEmptyCollection()]
        [Parameter(Mandatory = $true)][string[]]$ExpectedSnapshot
    )

    $actualSnapshot = @(Get-StableSnapshot)
    if (($actualSnapshot -join "`n") -ne ($ExpectedSnapshot -join "`n")) {
        throw "$Step : l'installation stable V3.4.2 a été modifiée."
    }
}

function Assert-InstalledState {
    param([Parameter(Mandatory = $true)][string]$Step)

    $targetRecords = @(Get-TargetInstallRecords)
    if ($targetRecords.Count -ne 1) {
        throw "$Step : une seule entrée V3.6 était attendue, trouvé $($targetRecords.Count)."
    }

    $record = $targetRecords[0]
    if ($record.DisplayVersion -ne $ExpectedVersion) {
        throw "$Step : version installée '$($record.DisplayVersion)' au lieu de '$ExpectedVersion'."
    }

    $installLocation = [System.IO.Path]::GetFullPath([string]$record.InstallLocation)
    if (-not $AllowCustomInstallLocation) {
        $programFilesRoot = [System.IO.Path]::GetFullPath($env:ProgramFiles).TrimEnd('\') + '\'
        if (-not $installLocation.StartsWith($programFilesRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
            throw "$Step : installation hors de Program Files : $installLocation"
        }
    }

    $requiredFiles = @(
        "DanteConfigEditorV3.exe",
        "QuickStart_DanteConfigEditorV3_FR.pdf",
        "QuickStart_DanteConfigEditorV3_EN.pdf",
        "Notice_DanteConfigEditorV3_FR.pdf",
        "Notice_DanteConfigEditorV3_EN.pdf",
        "README.md",
        "README_EN.md",
        "RELEASE_NOTES.md",
        "RELEASE_NOTES_EN.md",
        "unins000.exe"
    )

    foreach ($requiredFile in $requiredFiles) {
        $path = Join-Path $installLocation $requiredFile
        if (-not (Test-Path -LiteralPath $path)) {
            throw "$Step : fichier installé manquant : $path"
        }
    }

    $shortcut = Join-Path ([Environment]::GetFolderPath([Environment+SpecialFolder]::CommonPrograms)) "Dante Config Editor V3.6\DCE V3.6.lnk"
    if (-not (Test-Path -LiteralPath $shortcut)) {
        throw "$Step : raccourci Menu Démarrer manquant : $shortcut"
    }

    $desktopShortcutCandidates = @(
        (Join-Path ([Environment]::GetFolderPath([Environment+SpecialFolder]::DesktopDirectory)) "DCE V3.6.lnk"),
        (Join-Path ([Environment]::GetFolderPath([Environment+SpecialFolder]::CommonDesktopDirectory)) "DCE V3.6.lnk")
    ) | Select-Object -Unique
    if (-not ($desktopShortcutCandidates | Where-Object { Test-Path -LiteralPath $_ })) {
        throw "$Step : raccourci Bureau manquant. Chemins vérifiés : $($desktopShortcutCandidates -join ', ')"
    }

    return $record
}

$stableSnapshotBefore = @(Get-StableSnapshot)
$targetInstallRecordsBefore = @(Get-TargetInstallRecords).Count

Invoke-InstallerPass -Name "Installation V3.6"
$firstRecord = Assert-InstalledState -Step "Après le premier passage"
Assert-StableInstallUnchanged -Step "Après le premier passage" -ExpectedSnapshot $stableSnapshotBefore

Invoke-InstallerPass -Name "Mise à niveau de contrôle"
$secondRecord = Assert-InstalledState -Step "Après le second passage"
Assert-StableInstallUnchanged -Step "Après le second passage" -ExpectedSnapshot $stableSnapshotBefore

if (-not [string]::Equals([string]$firstRecord.InstallLocation, [string]$secondRecord.InstallLocation, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "Le chemin d'installation a changé pendant la mise à niveau."
}

$exePath = Join-Path ([string]$secondRecord.InstallLocation) "DanteConfigEditorV3.exe"
$fileVersion = (Get-Item -LiteralPath $exePath).VersionInfo.FileVersion

[pscustomobject]@{
    Result = "Success"
    Version = [string]$secondRecord.DisplayVersion
    FileVersion = $fileVersion
    InstallLocation = [string]$secondRecord.InstallLocation
    Installer = $installer
    InstallerSha256 = (Get-FileHash -LiteralPath $installer -Algorithm SHA256).Hash
    UpgradePasses = 2
    TargetInstallRecordsBefore = $targetInstallRecordsBefore
    TargetInstallRecords = @(Get-TargetInstallRecords).Count
    StableInstallRecords = @(Get-StableInstallRecords).Count
} | ConvertTo-Json -Depth 3
