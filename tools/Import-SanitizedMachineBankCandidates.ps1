[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string[]]$SourceDirectory,

    [string]$CatalogPath,

    [switch]$SkipImageDownload,

    [switch]$SkipArchiveBuild
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$root = [System.IO.Path]::GetFullPath((Split-Path $PSScriptRoot -Parent))
if ([string]::IsNullOrWhiteSpace($CatalogPath)) {
    $CatalogPath = Join-Path $root "machine-banks\community-device-sources.json"
}
$CatalogPath = [System.IO.Path]::GetFullPath($CatalogPath)
$bundledRoot = Join-Path $root "Resources\MachineBanks\Bundled"
$targetRoot = Join-Path $bundledRoot "DCE Community Devices 2026.1"
$legacyCommunityRoot = Join-Path $bundledRoot "DCE Community Devices 3.6"
$stagingRoot = Join-Path $bundledRoot (".community-staging-" + [Guid]::NewGuid().ToString("N"))
$cacheRoot = Join-Path $root "tmp\machine-bank-image-cache"
$backupRoot = Join-Path $root "tmp\machine-bank-backups"
$utf8 = [System.Text.UTF8Encoding]::new($false)
$fixedDate = [DateTimeOffset]::Parse(
    "2026-07-27T00:00:00+00:00",
    [System.Globalization.CultureInfo]::InvariantCulture)
$forbiddenNames = @(
    "instance_id",
    "device_id",
    "default_name",
    "interface",
    "txflow",
    "rxflow",
    "subscribed_device",
    "subscription_device",
    "tx_device",
    "source_device",
    "subscribed_channel",
    "subscribed_channel_name",
    "subscribed_channel_label",
    "subscribed_tx_channel",
    "subscribed_tx_channel_name",
    "subscribed_label",
    "source_channel",
    "source_channel_name",
    "ip_address",
    "ipv4_address",
    "gateway",
    "dns",
    "mac_address"
)

Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

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

    Write-Utf8File -Path $Path -Content (($Value | ConvertTo-Json -Depth 20) + "`n")
}

function Get-NormalizedKey {
    param(
        [AllowNull()]
        [string]$Value
    )

    if ([string]::IsNullOrWhiteSpace($Value)) {
        return ""
    }

    # Le signe + fait partie du nom commercial de certains modèles (D8+).
    # Il reste distinct du modèle sans suffixe lors du dédoublonnage.
    $valueWithMeaningfulSymbols = $Value.Replace(
        "+",
        " plus ",
        [StringComparison]::Ordinal)
    $normalized = $valueWithMeaningfulSymbols.Normalize(
        [Text.NormalizationForm]::FormD)
    $builder = [Text.StringBuilder]::new()
    foreach ($character in $normalized.ToCharArray()) {
        $category = [Globalization.CharUnicodeInfo]::GetUnicodeCategory($character)
        if ($category -ne [Globalization.UnicodeCategory]::NonSpacingMark -and
            [char]::IsLetterOrDigit($character)) {
            [void]$builder.Append([char]::ToLowerInvariant($character))
        }
    }
    return $builder.ToString()
}

function Get-OptionalPropertyValue {
    param(
        [Parameter(Mandatory = $true)]
        [object]$InputObject,
        [Parameter(Mandatory = $true)]
        [string]$Name
    )

    $property = $InputObject.PSObject.Properties[$Name]
    if ($null -eq $property) {
        return $null
    }
    return $property.Value
}

function Get-StableGuid {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Key
    )

    $bytes = [Security.Cryptography.SHA256]::HashData(
        $utf8.GetBytes("dce-community-device-v1:" + $Key))
    $guidBytes = [byte[]]::new(16)
    [Array]::Copy($bytes, $guidBytes, 16)
    $guidBytes[7] = ($guidBytes[7] -band 0x0F) -bor 0x50
    $guidBytes[8] = ($guidBytes[8] -band 0x3F) -bor 0x80
    return [Guid]::new($guidBytes)
}

function Get-DirectChild {
    param(
        [Parameter(Mandatory = $true)]
        [System.Xml.Linq.XElement]$Parent,
        [Parameter(Mandatory = $true)]
        [string]$LocalName
    )

    return $Parent.Elements() |
        Where-Object { $_.Name.LocalName -eq $LocalName } |
        Select-Object -First 1
}

function Get-DirectChildren {
    param(
        [Parameter(Mandatory = $true)]
        [System.Xml.Linq.XElement]$Parent,
        [Parameter(Mandatory = $true)]
        [string]$LocalName
    )

    return @($Parent.Elements() | Where-Object { $_.Name.LocalName -eq $LocalName })
}

function Get-DirectChildValue {
    param(
        [Parameter(Mandatory = $true)]
        [System.Xml.Linq.XElement]$Parent,
        [Parameter(Mandatory = $true)]
        [string]$LocalName
    )

    $child = Get-DirectChild -Parent $Parent -LocalName $LocalName
    if ($null -eq $child) {
        return ""
    }
    return $child.Value.Trim()
}

function Get-ElementHash {
    param(
        [Parameter(Mandatory = $true)]
        [System.Xml.Linq.XElement]$Element
    )

    $bytes = $utf8.GetBytes($Element.ToString([System.Xml.Linq.SaveOptions]::DisableFormatting))
    return [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes))
}

function New-EmptyCaptureInfo {
    param(
        [Parameter(Mandatory = $true)]
        [System.Xml.Linq.XNamespace]$Namespace
    )

    $capture = [System.Xml.Linq.XElement]::new($Namespace + "captureInfo")
    foreach ($name in @(
        "device_name",
        "interface_ipv4_addresses",
        "device_samplerate",
        "device_encoding",
        "device_unicast_latency",
        "rtp",
        "clock",
        "clock_priority",
        "txchannel_names",
        "txflows",
        "rxchannel_names",
        "rxchannel_subscriptions",
        "rxflows"
    )) {
        $capture.Add([System.Xml.Linq.XElement]::new($Namespace + $name, ""))
    }
    return $capture
}

function New-SafeChannel {
    param(
        [Parameter(Mandatory = $true)]
        [System.Xml.Linq.XElement]$Source,
        [Parameter(Mandatory = $true)]
        [ValidateSet("TX", "RX")]
        [string]$Kind,
        [Parameter(Mandatory = $true)]
        [int]$Index,
        [Parameter(Mandatory = $true)]
        [int]$Width
    )

    $channel = [System.Xml.Linq.XElement]::new($Source.Name)
    foreach ($attribute in $Source.Attributes()) {
        if ($forbiddenNames -notcontains $attribute.Name.LocalName) {
            $channel.Add([System.Xml.Linq.XAttribute]::new($attribute))
        }
    }
    $label = "{0} {1}" -f $Kind, $Index.ToString(
        ("D" + $Width),
        [Globalization.CultureInfo]::InvariantCulture)
    $labelElementName = if ($Kind -eq "TX") { "label" } else { "name" }
    $channel.Add([System.Xml.Linq.XElement]::new(
        $Source.Name.Namespace + $labelElementName,
        $label))
    return $channel
}

function New-SanitizedTemplate {
    param(
        [Parameter(Mandatory = $true)]
        [System.Xml.Linq.XElement]$Source
    )

    $namespace = $Source.Name.Namespace
    $device = [System.Xml.Linq.XElement]::new($Source.Name)
    $device.Add((New-EmptyCaptureInfo -Namespace $namespace))

    foreach ($name in @(
        "manufacturer_id",
        "manufacturer_name",
        "model_id",
        "model_name",
        "model_version",
        "device_type",
        "device_type_string"
    )) {
        $sourceElement = Get-DirectChild -Parent $Source -LocalName $name
        if ($null -ne $sourceElement) {
            $device.Add([System.Xml.Linq.XElement]::new($sourceElement))
        }
    }

    $device.Add([System.Xml.Linq.XElement]::new($namespace + "friendly_name", "MACHINE-TEMPLATE"))
    $redundancy = [System.Xml.Linq.XElement]::new($namespace + "redundancy")
    $redundancy.SetAttributeValue("value", "false")
    $device.Add($redundancy)
    $preferredMaster = [System.Xml.Linq.XElement]::new($namespace + "preferred_master")
    $preferredMaster.SetAttributeValue("value", "false")
    $device.Add($preferredMaster)
    $externalClock = [System.Xml.Linq.XElement]::new($namespace + "external_word_clock")
    $externalClock.SetAttributeValue("value", "false")
    $device.Add($externalClock)
    $device.Add([System.Xml.Linq.XElement]::new($namespace + "samplerate", "48000"))
    $device.Add([System.Xml.Linq.XElement]::new($namespace + "encoding", "24"))
    $device.Add([System.Xml.Linq.XElement]::new($namespace + "unicast_latency", "1000"))

    $txChannels = @(Get-DirectChildren -Parent $Source -LocalName "txchannel")
    $rxChannels = @(Get-DirectChildren -Parent $Source -LocalName "rxchannel")
    $txWidth = [Math]::Max(2, $txChannels.Count.ToString(
        [Globalization.CultureInfo]::InvariantCulture).Length)
    $rxWidth = [Math]::Max(2, $rxChannels.Count.ToString(
        [Globalization.CultureInfo]::InvariantCulture).Length)
    for ($index = 0; $index -lt $txChannels.Count; $index++) {
        $device.Add((New-SafeChannel `
            -Source $txChannels[$index] `
            -Kind "TX" `
            -Index ($index + 1) `
            -Width $txWidth))
    }
    for ($index = 0; $index -lt $rxChannels.Count; $index++) {
        $device.Add((New-SafeChannel `
            -Source $rxChannels[$index] `
            -Kind "RX" `
            -Index ($index + 1) `
            -Width $rxWidth))
    }

    $document = [System.Xml.Linq.XDocument]::new(
        [System.Xml.Linq.XDeclaration]::new("1.0", "UTF-8", "yes"),
        $device)
    return $document
}

function Write-TemplateXml {
    param(
        [Parameter(Mandatory = $true)]
        [System.Xml.Linq.XDocument]$Document,
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    $settings = [System.Xml.XmlWriterSettings]::new()
    $settings.Encoding = $utf8
    $settings.Indent = $true
    $settings.NewLineChars = "`n"
    $settings.NewLineHandling = [System.Xml.NewLineHandling]::Replace
    $settings.OmitXmlDeclaration = $false
    $writer = [System.Xml.XmlWriter]::Create($Path, $settings)
    try {
        $Document.Save($writer)
    }
    finally {
        $writer.Dispose()
    }
}

function Get-ImageExtension {
    param(
        [Parameter(Mandatory = $true)]
        [object]$Image
    )

    $extension = [string]$Image.extension
    if ([string]::IsNullOrWhiteSpace($extension)) {
        $path = ([Uri][string]$Image.downloadUrl).AbsolutePath
        $extension = [IO.Path]::GetExtension($path).TrimStart(".")
    }
    $extension = $extension.ToLowerInvariant()
    if ($extension -eq "jpeg") {
        $extension = "jpg"
    }
    if ($extension -notin @("png", "jpg")) {
        throw "Format d'image automatisé non pris en charge : $extension"
    }
    return $extension
}

function Invoke-SafeDownload {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Uri,
        [Parameter(Mandatory = $true)]
        [string]$Destination
    )

    Invoke-WebRequest `
        -Uri $Uri `
        -OutFile $Destination `
        -UseBasicParsing `
        -Headers @{ "User-Agent" = "DanteConfigEditor-BankBuilder/2026.1" }
    if (-not (Test-Path -LiteralPath $Destination -PathType Leaf) -or
        (Get-Item -LiteralPath $Destination).Length -eq 0) {
        throw "Téléchargement vide ou absent."
    }
}

function Get-ProfileImage {
    param(
        [Parameter(Mandatory = $true)]
        [object]$Profile,
        [Parameter(Mandatory = $true)]
        [string]$TemplateId,
        [Parameter(Mandatory = $true)]
        [string]$DestinationDirectory
    )

    $image = $Profile.image
    if ($null -eq $image) {
        throw "Image non déclarée pour $($Profile.templateName)."
    }

    $mode = [string]$image.mode
    if ($mode -eq "preserve") {
        $existingBankRoot = if (Test-Path -LiteralPath $targetRoot -PathType Container) {
            $targetRoot
        }
        else {
            $legacyCommunityRoot
        }
        $oldDirectory = Join-Path $existingBankRoot ("machines\" + $TemplateId)
        $metadataPath = Join-Path $oldDirectory "machine.json"
        if (-not (Test-Path -LiteralPath $metadataPath -PathType Leaf)) {
            throw "L'image existante de $($Profile.templateName) est introuvable."
        }
        $oldMetadata = Get-Content -LiteralPath $metadataPath -Raw | ConvertFrom-Json
        $oldImagePath = Join-Path $oldDirectory ([string]$oldMetadata.imageFileName)
        if (-not (Test-Path -LiteralPath $oldImagePath -PathType Leaf)) {
            throw "L'image existante de $($Profile.templateName) est absente."
        }
        $extension = [IO.Path]::GetExtension($oldImagePath).ToLowerInvariant()
        $destination = Join-Path $DestinationDirectory ("image" + $extension)
        Copy-Item -LiteralPath $oldImagePath -Destination $destination
        return $destination
    }

    if ($SkipImageDownload) {
        throw "Le téléchargement d'image est désactivé mais $($Profile.templateName) n'a pas d'image locale."
    }

    New-Item -ItemType Directory -Path $cacheRoot -Force | Out-Null
    $extension = Get-ImageExtension -Image $image
    $cacheKey = (Get-StableGuid -Key ([string]$Profile.key)).ToString("N")
    $rawPath = Join-Path $cacheRoot ($cacheKey + "-raw." + $extension)
    if ($mode -eq "url") {
        if (-not (Test-Path -LiteralPath $rawPath -PathType Leaf)) {
            Invoke-SafeDownload -Uri ([string]$image.downloadUrl) -Destination $rawPath
        }
    }
    elseif ($mode -eq "zip") {
        $zipPath = Join-Path $cacheRoot ($cacheKey + ".zip")
        if (-not (Test-Path -LiteralPath $zipPath -PathType Leaf)) {
            Invoke-SafeDownload -Uri ([string]$image.downloadUrl) -Destination $zipPath
        }
        $archive = [System.IO.Compression.ZipFile]::OpenRead($zipPath)
        try {
            $entryName = ([string]$image.entry).Replace("\", "/")
            $entry = $archive.Entries |
                Where-Object { $_.FullName.Replace("\", "/") -eq $entryName } |
                Select-Object -First 1
            if ($null -eq $entry) {
                throw "Image '$entryName' absente de l'archive média officielle."
            }
            $entryStream = $entry.Open()
            $fileStream = [IO.File]::Create($rawPath)
            try {
                $entryStream.CopyTo($fileStream)
            }
            finally {
                $fileStream.Dispose()
                $entryStream.Dispose()
            }
        }
        finally {
            $archive.Dispose()
        }
    }
    else {
        throw "Mode d'image inconnu pour $($Profile.templateName) : $mode"
    }

    $destination = Join-Path $DestinationDirectory ("image." + $extension)
    Copy-Item -LiteralPath $rawPath -Destination $destination
    return $destination
}

function Assert-ImageSignature {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    $bytes = [IO.File]::ReadAllBytes($Path)
    $extension = [IO.Path]::GetExtension($Path).ToLowerInvariant()
    $valid = if ($extension -eq ".png") {
        $bytes.Length -ge 8 -and
            $bytes[0] -eq 0x89 -and
            $bytes[1] -eq 0x50 -and
            $bytes[2] -eq 0x4E -and
            $bytes[3] -eq 0x47
    }
    else {
        $bytes.Length -ge 3 -and
            $bytes[0] -eq 0xFF -and
            $bytes[1] -eq 0xD8 -and
            $bytes[2] -eq 0xFF
    }
    if (-not $valid) {
        throw "Signature d'image invalide : $([IO.Path]::GetFileName($Path))"
    }
}

function Assert-SanitizedTemplate {
    param(
        [Parameter(Mandatory = $true)]
        [System.Xml.Linq.XDocument]$Document,
        [Parameter(Mandatory = $true)]
        [object]$Profile
    )

    $rootElement = $Document.Root
    if ($null -eq $rootElement -or $rootElement.Name.LocalName -ne "device") {
        throw "Le modèle $($Profile.templateName) ne possède pas de racine device."
    }
    $forbidden = @($rootElement.DescendantsAndSelf() | Where-Object {
        $forbiddenNames -contains $_.Name.LocalName
    })
    if ($forbidden.Count -gt 0) {
        $names = ($forbidden | ForEach-Object { $_.Name.LocalName } | Sort-Object -Unique) -join ", "
        throw "Données interdites dans $($Profile.templateName) : $names"
    }
    $forbiddenAttributes = @($rootElement.DescendantsAndSelf().Attributes() | Where-Object {
        $forbiddenNames -contains $_.Name.LocalName
    })
    if ($forbiddenAttributes.Count -gt 0) {
        throw "Attribut d'identité ou de réseau interdit dans $($Profile.templateName)."
    }
    if ((Get-DirectChildValue -Parent $rootElement -LocalName "friendly_name") -ne "MACHINE-TEMPLATE") {
        throw "Nom générique absent de $($Profile.templateName)."
    }

    $txChannels = @(Get-DirectChildren -Parent $rootElement -LocalName "txchannel")
    $rxChannels = @(Get-DirectChildren -Parent $rootElement -LocalName "rxchannel")
    $expected = @($Profile.sourceMatchers)[0]
    if ($txChannels.Count -ne [int]$expected.txCount -or
        $rxChannels.Count -ne [int]$expected.rxCount) {
        throw "Nombre de canaux inattendu dans $($Profile.templateName)."
    }
    for ($index = 0; $index -lt $txChannels.Count; $index++) {
        $label = Get-DirectChildValue -Parent $txChannels[$index] -LocalName "label"
        if ($label -notmatch '^TX \d+$') {
            throw "Label TX non générique dans $($Profile.templateName)."
        }
    }
    for ($index = 0; $index -lt $rxChannels.Count; $index++) {
        $label = Get-DirectChildValue -Parent $rxChannels[$index] -LocalName "name"
        if ($label -notmatch '^RX \d+$') {
            throw "Label RX non générique dans $($Profile.templateName)."
        }
    }
}

if (-not (Test-Path -LiteralPath $CatalogPath -PathType Leaf)) {
    throw "Catalogue source absent : $CatalogPath"
}
$catalog = Get-Content -LiteralPath $CatalogPath -Raw | ConvertFrom-Json
if ([int]$catalog.formatVersion -ne 1) {
    throw "Version de catalogue source non prise en charge : $($catalog.formatVersion)"
}

$resolvedSourceDirectories = foreach ($directory in $SourceDirectory) {
    $resolved = [IO.Path]::GetFullPath($directory)
    if (-not (Test-Path -LiteralPath $resolved -PathType Container)) {
        throw "Répertoire source absent : $resolved"
    }
    $resolved
}

$profileKeys = @{}
$modelKeys = @{}
foreach ($profile in @($catalog.profiles)) {
    $key = [string]$profile.key
    if ([string]::IsNullOrWhiteSpace($key) -or $profileKeys.ContainsKey($key)) {
        throw "Clé de profil absente ou dupliquée : '$key'."
    }
    $profileKeys[$key] = $true
    $modelKey = (Get-NormalizedKey -Value ([string]$profile.manufacturer)) + "|" +
        (Get-NormalizedKey -Value ([string]$profile.model))
    if ($modelKeys.ContainsKey($modelKey)) {
        throw "Doublon fabricant/modèle refusé : $($profile.manufacturer) $($profile.model)."
    }
    $modelKeys[$modelKey] = $true
}

$candidates = [Collections.Generic.List[object]]::new()
$invalidXmlCount = 0
foreach ($sourceRoot in $resolvedSourceDirectories) {
    $sourceFiles = @(Get-ChildItem -LiteralPath $sourceRoot -Recurse -File -Filter *.xml)
    $candidateCountBeforeRoot = $candidates.Count
    Write-Verbose ("Corpus privé : {0} XML à analyser." -f $sourceFiles.Count)
    foreach ($file in $sourceFiles) {
        try {
            $document = [System.Xml.Linq.XDocument]::Load(
                $file.FullName,
                [System.Xml.Linq.LoadOptions]::PreserveWhitespace)
            $presetVersion = [string]$document.Root.Attribute("version")
            foreach ($device in $document.Descendants() | Where-Object {
                $_.Name.LocalName -eq "device"
            }) {
                $manufacturer = Get-DirectChildValue -Parent $device -LocalName "manufacturer_name"
                $model = Get-DirectChildValue -Parent $device -LocalName "model_name"
                if ([string]::IsNullOrWhiteSpace($manufacturer) -and
                    [string]::IsNullOrWhiteSpace($model)) {
                    continue
                }
                $candidates.Add([pscustomobject]@{
                    Manufacturer = $manufacturer
                    Model = $model
                    TxCount = @(Get-DirectChildren -Parent $device -LocalName "txchannel").Count
                    RxCount = @(Get-DirectChildren -Parent $device -LocalName "rxchannel").Count
                    PresetVersion = $presetVersion
                    Hash = Get-ElementHash -Element $device
                    Device = [System.Xml.Linq.XElement]::new($device)
                })
            }
        }
        catch {
            $invalidXmlCount++
        }
    }
    Write-Verbose ("Représentations ajoutées depuis ce corpus : {0}." -f (
        $candidates.Count - $candidateCountBeforeRoot))
}
if ($candidates.Count -eq 0) {
    throw "Aucune machine exploitable trouvée dans le corpus privé."
}
Write-Verbose ("Représentations de machines indexées : {0}" -f $candidates.Count)

New-Item -ItemType Directory -Path $stagingRoot -Force | Out-Null
$templateIds = [Collections.Generic.List[Guid]]::new()
$generatedModels = [Collections.Generic.List[object]]::new()
try {
    foreach ($profile in @($catalog.profiles)) {
        $matches = foreach ($candidate in $candidates) {
            foreach ($matcher in @($profile.sourceMatchers)) {
                if ($candidate.Manufacturer -eq [string]$matcher.manufacturer -and
                    $candidate.Model -eq [string]$matcher.model -and
                    $candidate.TxCount -eq [int]$matcher.txCount -and
                    $candidate.RxCount -eq [int]$matcher.rxCount) {
                    $candidate
                    break
                }
            }
        }
        $source = @($matches | Sort-Object Hash | Select-Object -First 1)
        if ($source.Count -ne 1) {
            $expectedModels = @($profile.sourceMatchers | ForEach-Object {
                [string]$_.model
            })
            $nearby = @($candidates | Where-Object {
                $expectedModels -contains $_.Model
            } | Group-Object Manufacturer, Model, TxCount, RxCount | ForEach-Object {
                $_.Name
            })
            Write-Verbose ("Signatures proches : " + ($nearby -join " | "))
            throw "Aucune représentation exacte trouvée pour $($profile.templateName)."
        }
        $source = $source[0]

        $declaredTemplateId = [string](Get-OptionalPropertyValue `
            -InputObject $profile `
            -Name "templateId")
        $templateId = if (-not [string]::IsNullOrWhiteSpace($declaredTemplateId)) {
            [Guid]::Parse($declaredTemplateId)
        }
        else {
            Get-StableGuid -Key ([string]$profile.key)
        }
        $templateIds.Add($templateId)
        $templateDirectory = Join-Path $stagingRoot ("machines\" + $templateId.ToString("D"))
        New-Item -ItemType Directory -Path $templateDirectory -Force | Out-Null

        $templateDocument = New-SanitizedTemplate -Source $source.Device
        Assert-SanitizedTemplate -Document $templateDocument -Profile $profile
        $templatePath = Join-Path $templateDirectory "template.xml"
        Write-TemplateXml -Document $templateDocument -Path $templatePath
        $templateHash = (Get-FileHash -LiteralPath $templatePath -Algorithm SHA256).Hash

        $imagePath = Get-ProfileImage `
            -Profile $profile `
            -TemplateId $templateId.ToString("D") `
            -DestinationDirectory $templateDirectory
        Assert-ImageSignature -Path $imagePath
        $imageHash = (Get-FileHash -LiteralPath $imagePath -Algorithm SHA256).Hash
        $firstMatcher = @($profile.sourceMatchers)[0]
        $txCount = [int]$firstMatcher.txCount
        $rxCount = [int]$firstMatcher.rxCount
        $descriptionFr = "Rôle hors ligne assaini pour {0}, {1} TX / {2} RX. Les identifiants matériels, le réseau, les flows et les subscriptions ne sont jamais repris." -f `
            $profile.templateName, $txCount, $rxCount
        $descriptionEn = "Sanitized offline role for {0}, {1} Tx / {2} Rx. Hardware identifiers, network settings, flows and subscriptions are never reused." -f `
            $profile.templateName, $txCount, $rxCount
        $metadata = [ordered]@{
            formatVersion = 2
            templateId = $templateId
            templateName = [string]$profile.templateName
            manufacturer = [string]$profile.manufacturer
            model = [string]$profile.model
            description = $descriptionFr
            category = [string]$profile.category
            tags = @($profile.tags)
            txCount = $txCount
            rxCount = $rxCount
            sourcePresetVersion = "3.0.0"
            sourceXmlNamespace = $templateDocument.Root.Name.NamespaceName
            createdByDceVersion = [string]$catalog.generatedByDceVersion
            createdUtc = $fixedDate
            modifiedUtc = $fixedDate
            templateSha256 = $templateHash
            imageFileName = [IO.Path]::GetFileName($imagePath)
            imageSha256 = $imageHash
            catalogKey = [string]$profile.key
            descriptionEn = $descriptionEn
            imageSourcePage = [string](Get-OptionalPropertyValue `
                -InputObject $profile.image `
                -Name "sourcePage")
            imageSourceUrl = if ([string]$profile.image.mode -eq "preserve") {
                $null
            }
            else {
                [string]$profile.image.downloadUrl
            }
            imageAttribution = if ([string]$profile.image.mode -eq "preserve") {
                "Publication approved by contributor - " + [string]$profile.manufacturer
            }
            else {
                "Official product image - " + [string]$profile.manufacturer
            }
            sourceSignature = [ordered]@{
                manufacturer = $source.Manufacturer
                model = $source.Model
                txCount = $source.TxCount
                rxCount = $source.RxCount
            }
        }
        Write-JsonFile -Path (Join-Path $templateDirectory "machine.json") -Value $metadata
        $generatedModels.Add([pscustomobject]@{
            TemplateId = $templateId
            Key = [string]$profile.key
            Name = [string]$profile.templateName
            Manufacturer = [string]$profile.manufacturer
            Model = [string]$profile.model
            Tx = $txCount
            Rx = $rxCount
        })
    }

    $manifest = [ordered]@{
        formatVersion = 2
        bankId = [Guid]::Parse([string]$catalog.bankId)
        createdUtc = $fixedDate
        updatedUtc = $fixedDate
        templateIds = $templateIds
    }
    Write-JsonFile -Path (Join-Path $stagingRoot "bank.json") -Value $manifest

    $metadataFiles = @(Get-ChildItem -LiteralPath (Join-Path $stagingRoot "machines") -File -Recurse -Filter machine.json)
    if ($metadataFiles.Count -ne @($catalog.profiles).Count) {
        throw "La banque générée ne contient pas le nombre de modèles attendu."
    }
    foreach ($metadataFile in $metadataFiles) {
        $metadata = Get-Content -LiteralPath $metadataFile.FullName -Raw | ConvertFrom-Json
        $templatePath = Join-Path $metadataFile.Directory.FullName "template.xml"
        $actualTemplateHash = (Get-FileHash -LiteralPath $templatePath -Algorithm SHA256).Hash
        if (-not $actualTemplateHash.Equals(
            [string]$metadata.templateSha256,
            [StringComparison]::OrdinalIgnoreCase)) {
            throw "Empreinte XML incohérente pour $($metadata.templateName)."
        }
        $imagePath = Join-Path $metadataFile.Directory.FullName ([string]$metadata.imageFileName)
        $actualImageHash = (Get-FileHash -LiteralPath $imagePath -Algorithm SHA256).Hash
        if (-not $actualImageHash.Equals(
            [string]$metadata.imageSha256,
            [StringComparison]::OrdinalIgnoreCase)) {
            throw "Empreinte d'image incohérente pour $($metadata.templateName)."
        }
    }

    New-Item -ItemType Directory -Path $backupRoot -Force | Out-Null
    $backupPath = $null
    if (Test-Path -LiteralPath $targetRoot -PathType Container) {
        $backupPath = Join-Path $backupRoot (
            "DCE Community Devices 2026.1-" +
            [DateTime]::UtcNow.ToString("yyyyMMdd-HHmmss") +
            "-" +
            [Guid]::NewGuid().ToString("N"))
        Move-Item -LiteralPath (Assert-RepositoryPath $targetRoot) -Destination $backupPath
    }
    try {
        Move-Item -LiteralPath (Assert-RepositoryPath $stagingRoot) -Destination $targetRoot
    }
    catch {
        if ($null -ne $backupPath -and
            (Test-Path -LiteralPath $backupPath -PathType Container) -and
            -not (Test-Path -LiteralPath $targetRoot)) {
            Move-Item -LiteralPath $backupPath -Destination $targetRoot
        }
        throw
    }

    if (-not $SkipArchiveBuild) {
        & (Join-Path $PSScriptRoot "Build-BundledMachineBanks.ps1")
        if ($LASTEXITCODE -ne 0) {
            throw "La construction des archives de banque a échoué avec le code $LASTEXITCODE."
        }
    }

    Write-Host ("Corpus analysé : {0} représentations, {1} XML ignoré(s)." -f `
        $candidates.Count, $invalidXmlCount)
    Write-Host ("Banque générée : {0} modèles uniques." -f $generatedModels.Count)
    $generatedModels |
        Sort-Object Manufacturer, Model |
        Format-Table Manufacturer, Model, Tx, Rx -AutoSize
}
finally {
    if (Test-Path -LiteralPath $stagingRoot) {
        Remove-Item -LiteralPath (Assert-RepositoryPath $stagingRoot) -Recurse -Force
    }
}
