$ErrorActionPreference = "Stop"

$root = [System.IO.Path]::GetFullPath((Split-Path $PSScriptRoot -Parent))
$bundledRoot = Join-Path $root "Resources\MachineBanks\Bundled"
$genericBankRoot = Join-Path $bundledRoot "DCE Generic Roles 2026.1"
$communityBankRoot = Join-Path $bundledRoot "DCE Community Devices 2026.1"
$githubRoot = Join-Path $root "machine-banks"
$genericArchivePath = Join-Path $githubRoot "DCE_Generic_Roles_2026_1.dce-bank.zip"
$communityArchivePath = Join-Path $githubRoot "DCE_Community_Devices_2026_1.dce-bank.zip"
$communitySourceCatalogPath = Join-Path $githubRoot "community-device-sources.json"
$catalogPath = Join-Path $githubRoot "catalog.json"
$utf8 = [System.Text.UTF8Encoding]::new($false)
$fixedDate = [DateTimeOffset]::Parse(
    "2026-07-26T00:00:00+00:00",
    [System.Globalization.CultureInfo]::InvariantCulture)
$catalogDate = [DateTimeOffset]::Parse(
    "2026-07-27T00:00:00+00:00",
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
        [string]$BankDirectory,
        [Parameter(Mandatory = $true)]
        [string]$SourceCatalogPath
    )

    $manifestPath = Join-Path $BankDirectory "bank.json"
    $machinesPath = Join-Path $BankDirectory "machines"
    if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
        throw "Manifeste de banque communautaire absent : $manifestPath"
    }
    if (-not (Test-Path -LiteralPath $machinesPath -PathType Container)) {
        throw "Dossier de modèles communautaires absent : $machinesPath"
    }
    if (-not (Test-Path -LiteralPath $SourceCatalogPath -PathType Leaf)) {
        throw "Catalogue source communautaire absent : $SourceCatalogPath"
    }

    $manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
    if ($manifest.formatVersion -ne 2) {
        throw "Version de banque communautaire non prise en charge : $($manifest.formatVersion)"
    }

    $sourceCatalog = Get-Content -LiteralPath $SourceCatalogPath -Raw |
        ConvertFrom-Json
    if ([int]$sourceCatalog.formatVersion -ne 1) {
        throw "Version de catalogue source communautaire non prise en charge."
    }
    $expectedTemplates = @{}
    foreach ($profile in @($sourceCatalog.profiles)) {
        $key = [string]$profile.key
        if ([string]::IsNullOrWhiteSpace($key) -or
            $expectedTemplates.ContainsKey($key)) {
            throw "Clé de catalogue communautaire absente ou dupliquée : '$key'."
        }
        $matcher = @($profile.sourceMatchers)[0]
        $expectedTemplates[$key] = [ordered]@{
            name = [string]$profile.templateName
            manufacturer = [string]$profile.manufacturer
            model = [string]$profile.model
            tx = [int]$matcher.txCount
            rx = [int]$matcher.rxCount
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
        "dns",
        "mac_address"
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
        $catalogKey = [string]$metadata.catalogKey
        $expected = $expectedTemplates[$catalogKey]
        if ($null -eq $expected) {
            throw "Modèle communautaire inattendu : $($metadata.templateName)."
        }
        if ($seenNames -contains [string]$metadata.templateName) {
            throw "Modèle communautaire dupliqué : $($metadata.templateName)"
        }
        $seenNames += [string]$metadata.templateName

        $templateId = [Guid]::Parse([string]$metadata.templateId).ToString("D")
        $hasMatchingIdentity =
            ($templateDirectory.Name -eq $templateId) -and
            ($manifestTemplateIds -contains $templateId)
        if (-not $hasMatchingIdentity) {
            throw "Identifiant incohérent pour le modèle $($metadata.templateName)."
        }
        $hasExpectedMetadata =
            ([int]$metadata.formatVersion -eq 2) -and
            ($metadata.templateName -eq $expected.name) -and
            ($metadata.manufacturer -eq $expected.manufacturer) -and
            ($metadata.model -eq $expected.model) -and
            ($metadata.txCount -eq $expected.tx) -and
            ($metadata.rxCount -eq $expected.rx)
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
        $actualImageHash = (
            Get-FileHash -LiteralPath $imagePath -Algorithm SHA256
        ).Hash
        if (-not $actualImageHash.Equals(
            [string]$metadata.imageSha256,
            [System.StringComparison]::OrdinalIgnoreCase)) {
            throw "Empreinte d'image invalide pour le modèle $($metadata.templateName)."
        }

        [xml]$templateXml = Get-Content -LiteralPath $templatePath -Raw
        $forbiddenNodes = @($templateXml.SelectNodes("//*") | Where-Object {
            $forbiddenXmlElements -contains $_.LocalName
        })
        if ($forbiddenNodes.Count -gt 0) {
            $names = ($forbiddenNodes | ForEach-Object LocalName | Sort-Object -Unique) -join ", "
            throw "Données de projet interdites dans $($metadata.templateName) : $names"
        }
        $forbiddenAttributes = @($templateXml.SelectNodes("//@*") | Where-Object {
            $forbiddenXmlElements -contains $_.LocalName
        })
        if ($forbiddenAttributes.Count -gt 0) {
            throw "Attribut de projet interdit dans $($metadata.templateName)."
        }
        $friendlyName = @($templateXml.SelectNodes(
            "/*[local-name()='device']/*[local-name()='friendly_name']"))
        if ($friendlyName.Count -ne 1 -or
            $friendlyName[0].InnerText -ne "MACHINE-TEMPLATE") {
            throw "Nom générique absent du modèle $($metadata.templateName)."
        }
        $txLabels = @($templateXml.SelectNodes(
            "/*[local-name()='device']/*[local-name()='txchannel']/*[local-name()='label']"))
        $rxLabels = @($templateXml.SelectNodes(
            "/*[local-name()='device']/*[local-name()='rxchannel']/*[local-name()='name']"))
        if ($txLabels.Count -ne [int]$expected.tx -or
            $rxLabels.Count -ne [int]$expected.rx -or
            @($txLabels | Where-Object { $_.InnerText -notmatch '^TX \d+$' }).Count -gt 0 -or
            @($rxLabels | Where-Object { $_.InnerText -notmatch '^RX \d+$' }).Count -gt 0) {
            throw "Canaux ou labels non génériques dans $($metadata.templateName)."
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
        createdByDceVersion = "2026.1"
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

Assert-CommunityBank `
    -BankDirectory $communityBankRoot `
    -SourceCatalogPath $communitySourceCatalogPath
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
    updatedUtc = $catalogDate
    banks = @(
        [ordered]@{
            id = "dce-generic-roles-2026.1"
            name = "DCE Generic Roles 2026.1"
            file = [System.IO.Path]::GetFileName($genericArchivePath)
            sha256 = $genericArchiveHash
            minimumDceVersion = "2026.1"
            language = "fr-en"
            descriptionFr = "Deux rôles génériques 8x8 et 32x32 sans identité matérielle, réseau ni abonnement."
            descriptionEn = "Two generic 8x8 and 32x32 roles without hardware identity, network settings or subscriptions."
        },
        [ordered]@{
            id = "dce-community-devices-2026.1"
            name = "DCE Community Devices 2026.1"
            file = [System.IO.Path]::GetFileName($communityArchivePath)
            sha256 = $communityArchiveHash
            minimumDceVersion = "2026.1"
            language = "fr-en"
            descriptionFr = "Quarante-trois modèles illustrés et assainis, sans identité matérielle, réseau, flow ni abonnement."
            descriptionEn = "Forty-three illustrated sanitized templates without hardware identity, network settings, flows, or subscriptions."
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
