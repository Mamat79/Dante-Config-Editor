$ErrorActionPreference = "Stop"

$root = [System.IO.Path]::GetFullPath((Split-Path $PSScriptRoot -Parent))

function Assert-RepositoryPath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    # Toute suppression du script doit rester strictement sous la racine du dépôt.
    $absolutePath = [System.IO.Path]::GetFullPath($Path)
    $rootPrefix = $root.TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar
    if (-not $absolutePath.StartsWith($rootPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Chemin généré hors du dépôt refusé : $absolutePath"
    }

    return $absolutePath
}

function Remove-GeneratedPath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [switch]$Recurse
    )

    $safePath = Assert-RepositoryPath $Path
    if (-not (Test-Path -LiteralPath $safePath)) {
        return
    }

    if ($Recurse) {
        Remove-Item -LiteralPath $safePath -Recurse -Force
    }
    else {
        Remove-Item -LiteralPath $safePath -Force
    }
}

$project = Join-Path $root "DanteConfigEditorV3.csproj"
$dist = Join-Path $root "dist"
$payload = Join-Path $dist "installer_payload"
$installer = Join-Path $dist "DanteConfigEditorV3_6_Installer.exe"
$installerChecksum = "$installer.sha256"
$script = Join-Path $PSScriptRoot "DanteConfigEditorV3.iss"
$bankBuilder = Join-Path $root "tools\Build-BundledMachineBanks.ps1"
$isccCandidates = @(
    $env:ISCC_PATH,
    (Join-Path $env:LOCALAPPDATA "Programs\Inno Setup 6\ISCC.exe"),
    (Join-Path ${env:ProgramFiles(x86)} "Inno Setup 6\ISCC.exe"),
    (Join-Path $env:ProgramFiles "Inno Setup 6\ISCC.exe")
) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -Unique
$iscc = $isccCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1

$dotnetCommand = Get-Command dotnet -ErrorAction SilentlyContinue
$dotnetCandidates = @((Join-Path $env:USERPROFILE ".dotnet\dotnet.exe"))
if ($dotnetCommand) {
    $dotnetCandidates += $dotnetCommand.Source
}
$dotnetCandidates = $dotnetCandidates | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -Unique

$dotnet = $null
foreach ($candidate in $dotnetCandidates) {
    if ((Test-Path $candidate) -and ((& $candidate --list-sdks) -match "^8\."))
    {
        $dotnet = $candidate
        break
    }
}

if (-not $dotnet) {
    throw "SDK .NET 8 introuvable. Installez le SDK .NET 8 avant de construire l'installateur."
}

if (-not $iscc) {
    throw "Inno Setup est introuvable. Installez Inno Setup 6 ou définissez ISCC_PATH vers ISCC.exe avant de construire l'installateur."
}

New-Item -ItemType Directory -Force -Path $dist | Out-Null

& $bankBuilder
if ($LASTEXITCODE -ne 0) {
    throw "La génération des banques de machines fournies a échoué."
}

foreach ($temporaryPath in @($payload, (Join-Path $dist "self-contained-win-x64"), (Join-Path $dist "portable"), (Join-Path $dist "setup_payload"))) {
    Remove-GeneratedPath -Path $temporaryPath -Recurse
}

foreach ($obsoleteFile in @(
    (Join-Path $dist "DanteConfigEditorV3_Setup.exe"),
    (Join-Path $dist "DanteConfigEditorV3_SetupPayload.7z"),
    (Join-Path $dist "DanteConfigEditorV3_SetupPayload.zip"),
    (Join-Path $dist "DanteConfigEditorV3_SfxConfig.txt")
)) {
    Remove-GeneratedPath -Path $obsoleteFile
}

& $dotnet publish $project `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -o $payload

if ($LASTEXITCODE -ne 0) {
    throw "La publication .NET a échoué."
}

& $iscc $script

if ($LASTEXITCODE -ne 0) {
    throw "La création de l'installateur a échoué."
}

if (-not (Test-Path $installer)) {
    throw "L'installateur final est introuvable : $installer"
}

# La somme est recréée à chaque build pour ne jamais publier celle d'un ancien paquet.
$installerHash = (Get-FileHash -LiteralPath $installer -Algorithm SHA256).Hash.ToLowerInvariant()
$checksumLine = "$installerHash  $([System.IO.Path]::GetFileName($installer))`n"
[System.IO.File]::WriteAllText($installerChecksum, $checksumLine, [System.Text.UTF8Encoding]::new($false))

Remove-GeneratedPath -Path $payload -Recurse
Remove-GeneratedPath -Path (Join-Path $root "bin") -Recurse
Remove-GeneratedPath -Path (Join-Path $root "obj") -Recurse

Write-Host "Installateur professionnel créé : $installer"
Write-Host "Somme SHA-256 créée : $installerChecksum"
