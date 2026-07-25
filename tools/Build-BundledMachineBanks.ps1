$ErrorActionPreference = "Stop"

$root = [System.IO.Path]::GetFullPath((Split-Path $PSScriptRoot -Parent))
$bundledRoot = Join-Path $root "Resources\MachineBanks\Bundled"
$bankRoot = Join-Path $bundledRoot "DCE Generic Roles 3.6"
$githubRoot = Join-Path $root "machine-banks"
$archivePath = Join-Path $githubRoot "DCE_Generic_Roles_3_6.dce-bank.zip"
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
                [System.IO.Compression.CompressionLevel]::Optimal)
            $entry.LastWriteTime = $fixedDate
            $entryStream = $entry.Open()
            $sourceStream = $file.OpenRead()
            try {
                $sourceStream.CopyTo($entryStream)
            }
            finally {
                $sourceStream.Dispose()
                $entryStream.Dispose()
            }
        }
    }
    finally {
        $archive.Dispose()
        $stream.Dispose()
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

if (Test-Path -LiteralPath $bankRoot) {
    Remove-Item -LiteralPath (Assert-RepositoryPath $bankRoot) -Recurse -Force
}

New-Item -ItemType Directory -Force -Path $bankRoot, $githubRoot | Out-Null
$templateIds = @()
foreach ($template in $templates) {
    $templateId = [Guid]::Parse($template.id)
    $templateIds += $templateId
    $templateDirectory = Join-Path $bankRoot ("machines\" + $templateId.ToString("D"))
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
Write-JsonFile -Path (Join-Path $bankRoot "bank.json") -Value $manifest

New-DeterministicZip -SourceDirectory $bankRoot -DestinationArchive $archivePath
$archiveHash = (Get-FileHash -LiteralPath $archivePath -Algorithm SHA256).Hash.ToLowerInvariant()
$catalog = [ordered]@{
    formatVersion = 1
    updatedUtc = $fixedDate
    banks = @(
        [ordered]@{
            id = "dce-generic-roles-3.6"
            name = "DCE Generic Roles 3.6"
            file = [System.IO.Path]::GetFileName($archivePath)
            sha256 = $archiveHash
            minimumDceVersion = "3.6"
            language = "fr-en"
            descriptionFr = "Deux rôles génériques 8x8 et 32x32 sans identité matérielle, réseau ni abonnement."
            descriptionEn = "Two generic 8x8 and 32x32 roles without hardware identity, network settings or subscriptions."
        }
    )
}
Write-JsonFile -Path $catalogPath -Value $catalog

Write-Host "Banque fournie générée : $bankRoot"
Write-Host "Archive GitHub générée : $archivePath"
Write-Host "SHA-256 : $archiveHash"
