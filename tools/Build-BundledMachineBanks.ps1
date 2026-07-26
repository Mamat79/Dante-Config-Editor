$ErrorActionPreference = "Stop"

$root = [System.IO.Path]::GetFullPath((Split-Path $PSScriptRoot -Parent))
$bundledRoot = Join-Path $root "Resources\MachineBanks\Bundled"
$genericBankRoot = Join-Path $bundledRoot "DCE Generic Roles 3.6"
$communityBankRoot = Join-Path $bundledRoot "DCE Community Devices 3.6"
$githubRoot = Join-Path $root "machine-banks"
$genericArchivePath = Join-Path $githubRoot "DCE_Generic_Roles_3_6.dce-bank.zip"
$communityArchivePath = Join-Path $githubRoot "DCE_Community_Devices_3_6.dce-bank.zip"
$catalogPath = Join-Path $githubRoot "catalog.json"
$utf8 = [System.Text.UTF8Encoding]::new($false)
$fixedDate = [DateTimeOffset]::Parse(
    "2026-07-26T00:00:00+00:00",
    [System.Globalization.CultureInfo]::InvariantCulture)

function Assert-RepositoryPath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    $absolutePath = [System.IO.Path]::GetFullPath($Path)
    $prefix = $root.TrimEnd(
        [System.IO.Path]::DirectorySeparatorChar,
        [System.IO.Path]::AltDirectorySeparatorChar
    ) + [System.IO.Path]::DirectorySeparatorChar
    if (-not $absolutePath.StartsWith($prefix, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Chemin généré hors du dépôt refusé : $absolutePath"
    }

    return $absolutePath
}

function Write-Utf8File {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [string]$Content
    )

    [System.IO.File]::WriteAllText($Path, $Content, $utf8)
}

function Write-JsonFile {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [object]$Value
    )

    Write-Utf8File -Path $Path -Content (($Value | ConvertTo-Json -Depth 10) + "`n")
}

function New-GenericTemplateXml {
    param(
        [Parameter(Mandatory = $true)]
        [int]$TxCount,
        [Parameter(Mandatory = $true)]
        [int]$RxCount
    )

    $settings = [System.Xml.XmlWriterSettings]::new()
    $settings.Encoding = $utf8
    $settings.Indent = $true
    $settings.NewLineChars = "`n"
    $settings.OmitXmlDeclaration = $true

    $builder = [System.Text.StringBuilder]::new()
    $writer = [System.Xml.XmlWriter]::Create($builder, $settings)
    try {
        $writer.WriteStartElement("device")
        $writer.WriteStartElement("captureInfo")
        foreach ($name in @(
            "device_name",
            "device_samplerate",
            "device_encoding",
            "device_unicast_latency",
            "txchannel_names",
            "txflows",
            "rxchannel_names",
            "rxchannel_subscriptions",
            "rxflows"
        )) {
            $writer.WriteElementString($name, "")
        }
        $writer.WriteEndElement()
        $writer.WriteElementString("friendly_name", "MACHINE-TEMPLATE")
        $writer.WriteElementString("samplerate", "48000")
        $writer.WriteElementString("encoding", "24")
        $writer.WriteElementString("unicast_latency", "1000")
        $writer.WriteStartElement("preferred_master")
        $writer.WriteAttributeString("value", "false")
        $writer.WriteEndElement()

        for ($index = 1; $index -le $TxCount; $index++) {
            $writer.WriteStartElement("txchannel")
            $writer.WriteAttributeString("danteId", $index.ToString([System.Globalization.CultureInfo]::InvariantCulture))
            $writer.WriteAttributeString("mediaType", "audio")
            $writer.WriteElementString("label", ("TX {0:D2}" -f $index))
            $writer.WriteEndElement()
        }

        for ($index = 1; $index -le $RxCount; $index++) {
            $writer.WriteStartElement("rxchannel")
            $writer.WriteAttributeString("danteId", $index.ToString([System.Globalization.CultureInfo]::InvariantCulture))
            $writer.WriteAttributeString("mediaType", "audio")
            $writer.WriteElementString("name", ("RX {0:D2}" -f $index))
            $writer.WriteEndElement()
        }

        $writer.WriteEndElement()
        $writer.Flush()
    }
    finally {
        $writer.Dispose()
    }

    return "<?xml version=`"1.0`" encoding=`"UTF-8`" standalone=`"yes`"?>`n" +
        $builder.ToString() + "`n"
}

function New-DeterministicZip {
    param(
        [Parameter(Mandatory = $true)]
        [string]$SourceDirectory,
        [Parameter(Mandatory = $true)]
        [string]$DestinationArchive
    )

    Add-Type -AssemblyName System.IO.Compression
    if (Test-Path -LiteralPath $DestinationArchive) {
        Remove-Item -LiteralPath (Assert-RepositoryPath $DestinationArchive) -Force
    }

    $stream = [System.IO.File]::Create($DestinationArchive)
    $archive = [System.IO.Compression.ZipArchive]::new(
        $stream,
        [System.IO.Compression.ZipArchiveMode]::Create,
        $false)
    try {
        $files = Get-ChildItem -LiteralPath $SourceDirectory -File -Recurse |
            Sort-Object FullName
        foreach ($file in $files) {
            $relative = [System.IO.Path]::GetRelativePath(
                $SourceDirectory,
                $file.FullName)
            $relative = $relative.Replace(
                [System.IO.Path]::DirectorySeparatorChar,
                "/")
            $entry = $archive.CreateEntry(
                $relative,
                [System.IO.Compression.CompressionLevel]::NoCompression)
            $entry.LastWriteTime = $fixedDate
            $entryStream = $entry.Open()
            try {
                if ($file.Extension -in @(".json", ".xml")) {
                    $content = [System.IO.File]::ReadAllText($file.FullName)
                    $normalized = $content.Replace("`r`n", "`n").Replace("`r", "`n")
                    $bytes = $utf8.GetBytes($normalized)
                    $entryStream.Write($bytes, 0, $bytes.Length)
                }
                else {
                    $sourceStream = $file.OpenRead()
                    try {
                        $sourceStream.CopyTo($entryStream)
                    }
                    finally {
                        $sourceStream.Dispose()
                    }
                }
            }
            finally {
                $entryStream.Dispose()
            }
        }
    }
    finally {
        $archive.Dispose()
        $stream.Dispose()
    }
}

function Assert-CommunityBank {
    param(
        [Parameter(Mandatory = $true)]
        [string]$BankDirectory
    )

    $manifestPath = Join-Path $BankDirectory "bank.json"
    $machinesPath = Join-Path $BankDirectory "machines"
    if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
        throw "Manifeste de banque communautaire absent : $manifestPath"
    }
    if (-not (Test-Path -LiteralPath $machinesPath -PathType Container)) {
        throw "Dossier de modèles communautaires absent : $machinesPath"
    }

    $manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
    if ($manifest.formatVersion -ne 1) {
        throw "Version de banque communautaire non prise en charge : $($manifest.formatVersion)"
    }

    $expectedTemplates = @{
        "QL1" = @{
            manufacturer = "Yamaha Corporation"
            model = "QL1"
            tx = 32
            rx = 32
            image = "image.jpg"
        }
        "DI4.1000" = @{
            manufacturer = "Fohhn"
            model = "DI-AMP DAN"
            tx = 0
            rx = 4
            image = "image.png"
        }
        "LM 44 - 4 4RX" = @{
            manufacturer = "Lake"
            model = "LM 44"
            tx = 0
            rx = 4
            image = "image.jpg"
        }
        "Rio1608-D2" = @{
            manufacturer = "Yamaha Corporation"
            model = "Rio1608-D2"
            tx = 16
            rx = 8
            image = "image.jpg"
        }
        "Digiface Dante" = @{
            manufacturer = "RME GmbH"
            model = "Digiface Dante"
            tx = 64
            rx = 64
            image = "image.jpg"
        }
    }
    $forbiddenXmlElements = @(
        "instance_id",
        "device_id",
        "default_name",
        "interface",
        "txflow",
        "rxflow",
        "subscribed_device",
        "subscribed_channel",
        "ip_address",
        "gateway",
        "dns"
    )

    $templateDirectories = @(Get-ChildItem -LiteralPath $machinesPath -Directory)
    if ($templateDirectories.Count -ne $expectedTemplates.Count) {
        throw "Nombre inattendu de modèles dans la banque communautaire."
    }
    $manifestTemplateIds = @($manifest.templateIds | ForEach-Object {
        [Guid]::Parse([string]$_).ToString("D")
    })
    $hasExpectedManifestIds =
        ($manifestTemplateIds.Count -eq $expectedTemplates.Count) -and
        (@($manifestTemplateIds | Sort-Object -Unique).Count -eq $expectedTemplates.Count)
    if (-not $hasExpectedManifestIds) {
        throw "Le manifeste de la banque communautaire contient des identifiants incohérents."
    }

    $seenNames = @()
    foreach ($templateDirectory in $templateDirectories) {
        $metadataPath = Join-Path $templateDirectory.FullName "machine.json"
        $templatePath = Join-Path $templateDirectory.FullName "template.xml"
        $hasRequiredFiles =
            (Test-Path -LiteralPath $metadataPath -PathType Leaf) -and
            (Test-Path -LiteralPath $templatePath -PathType Leaf)
        if (-not $hasRequiredFiles) {
            throw "Modèle communautaire incomplet : $($templateDirectory.FullName)"
        }

        $metadata = Get-Content -LiteralPath $metadataPath -Raw | ConvertFrom-Json
        $expected = $expectedTemplates[$metadata.templateName]
        if ($null -eq $expected) {
            throw "Modèle communautaire inattendu : $($metadata.templateName)"
        }
        if ($seenNames -contains $metadata.templateName) {
            throw "Modèle communautaire dupliqué : $($metadata.templateName)"
        }
        $seenNames += $metadata.templateName

        $templateId = [Guid]::Parse([string]$metadata.templateId).ToString("D")
        $hasMatchingIdentity =
            ($templateDirectory.Name -eq $templateId) -and
            ($manifestTemplateIds -contains $templateId)
        if (-not $hasMatchingIdentity) {
            throw "Identifiant incohérent pour le modèle $($metadata.templateName)."
        }
        $hasExpectedMetadata =
            ($metadata.manufacturer -eq $expected.manufacturer) -and
            ($metadata.model -eq $expected.model) -and
            ($metadata.txCount -eq $expected.tx) -and
            ($metadata.rxCount -eq $expected.rx) -and
            ($metadata.imageFileName -eq $expected.image)
        if (-not $hasExpectedMetadata) {
            throw "Métadonnées inattendues pour le modèle $($metadata.templateName)."
        }

        $actualTemplateHash = (
            Get-FileHash -LiteralPath $templatePath -Algorithm SHA256
        ).Hash
        if (-not $actualTemplateHash.Equals(
            [string]$metadata.templateSha256,
            [System.StringComparison]::OrdinalIgnoreCase)) {
            throw "Empreinte XML invalide pour le modèle $($metadata.templateName)."
        }

        $imagePath = Join-Path $templateDirectory.FullName $metadata.imageFileName
        if (-not (Test-Path -LiteralPath $imagePath -PathType Leaf)) {
            throw "Image absente pour le modèle $($metadata.templateName)."
        }

        [xml]$templateXml = Get-Content -LiteralPath $templatePath -Raw
        $forbiddenNodes = @($templateXml.SelectNodes("//*") | Where-Object {
            $forbiddenXmlElements -contains $_.LocalName
        })
        if ($forbiddenNodes.Count -gt 0) {
            $names = ($forbiddenNodes | ForEach-Object LocalName | Sort-Object -Unique) -join ", "
            throw "Données de projet interdites dans $($metadata.templateName) : $names"
        }
    }
}

$templates = @(
    [ordered]@{
        id = "4f58fe46-7259-4af7-b355-20277376a408"
        name = "DCE Generic 8x8"
        model = "Generic 8x8"
        tx = 8
        rx = 8
    },
    [ordered]@{
        id = "b61c5608-fe92-40fa-a179-9c6e8f51dd44"
        name = "DCE Generic 32x32"
        model = "Generic 32x32"
        tx = 32
        rx = 32
    }
)

if (Test-Path -LiteralPath $genericBankRoot) {
    Remove-Item -LiteralPath (Assert-RepositoryPath $genericBankRoot) -Recurse -Force
}

New-Item -ItemType Directory -Force -Path $genericBankRoot, $githubRoot | Out-Null
$templateIds = @()
foreach ($template in $templates) {
    $templateId = [Guid]::Parse($template.id)
    $templateIds += $templateId
    $templateDirectory = Join-Path $genericBankRoot ("machines\" + $templateId.ToString("D"))
    New-Item -ItemType Directory -Force -Path $templateDirectory | Out-Null
    $xmlPath = Join-Path $templateDirectory "template.xml"
    Write-Utf8File -Path $xmlPath -Content (
        New-GenericTemplateXml -TxCount $template.tx -RxCount $template.rx)
    $xmlHash = (Get-FileHash -LiteralPath $xmlPath -Algorithm SHA256).Hash
    $metadata = [ordered]@{
        formatVersion = 1
        templateId = $templateId
        templateName = $template.name
        manufacturer = "DCE"
        model = $template.model
        description = "Rôle générique hors ligne pour essais, formation et préparation de presets. Il ne représente pas un matériel Dante réel."
        category = "Générique / Formation"
        tags = @("generic", "offline", "training")
        txCount = $template.tx
        rxCount = $template.rx
        sourcePresetVersion = "3.0.0"
        sourceXmlNamespace = ""
        createdByDceVersion = "3.6"
        createdUtc = $fixedDate
        modifiedUtc = $fixedDate
        templateSha256 = $xmlHash
        imageFileName = $null
    }
    Write-JsonFile -Path (Join-Path $templateDirectory "machine.json") -Value $metadata
}

$manifest = [ordered]@{
    formatVersion = 1
    bankId = [Guid]::Parse("41b14852-a168-4e41-b94d-fc1b7cab85a6")
    createdUtc = $fixedDate
    updatedUtc = $fixedDate
    templateIds = $templateIds
}
Write-JsonFile -Path (Join-Path $genericBankRoot "bank.json") -Value $manifest

Assert-CommunityBank -BankDirectory $communityBankRoot
New-DeterministicZip -SourceDirectory $genericBankRoot -DestinationArchive $genericArchivePath
New-DeterministicZip -SourceDirectory $communityBankRoot -DestinationArchive $communityArchivePath
$genericArchiveHash = (
    Get-FileHash -LiteralPath $genericArchivePath -Algorithm SHA256
).Hash.ToLowerInvariant()
$communityArchiveHash = (
    Get-FileHash -LiteralPath $communityArchivePath -Algorithm SHA256
).Hash.ToLowerInvariant()
$catalog = [ordered]@{
    formatVersion = 1
    updatedUtc = $fixedDate
    banks = @(
        [ordered]@{
            id = "dce-generic-roles-3.6"
            name = "DCE Generic Roles 3.6"
            file = [System.IO.Path]::GetFileName($genericArchivePath)
            sha256 = $genericArchiveHash
            minimumDceVersion = "3.6"
            language = "fr-en"
            descriptionFr = "Deux rôles génériques 8x8 et 32x32 sans identité matérielle, réseau ni abonnement."
            descriptionEn = "Two generic 8x8 and 32x32 roles without hardware identity, network settings or subscriptions."
        },
        [ordered]@{
            id = "dce-community-devices-3.6"
            name = "DCE Community Devices 3.6"
            file = [System.IO.Path]::GetFileName($communityArchivePath)
            sha256 = $communityArchiveHash
            minimumDceVersion = "3.6"
            language = "fr-en"
            descriptionFr = "Cinq modèles illustrés : Yamaha QL1 et Rio1608-D2, Fohhn DI4.1000, Lake LM 44 et RME Digiface Dante, sans identité matérielle, réseau ni abonnement."
            descriptionEn = "Five illustrated templates: Yamaha QL1 and Rio1608-D2, Fohhn DI4.1000, Lake LM 44 and RME Digiface Dante, without hardware identity, network settings or subscriptions."
        }
    )
}
Write-JsonFile -Path $catalogPath -Value $catalog

Write-Host "Banque générique générée : $genericBankRoot"
Write-Host "Archive générique : $genericArchivePath"
Write-Host "SHA-256 générique : $genericArchiveHash"
Write-Host "Banque communautaire vérifiée : $communityBankRoot"
Write-Host "Archive communautaire : $communityArchivePath"
Write-Host "SHA-256 communautaire : $communityArchiveHash"
