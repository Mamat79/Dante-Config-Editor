using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Linq;
using DanteConfigEditor.Models;

namespace DanteConfigEditor.Services;

public sealed class MachineBankRepository
{
    private const long MaximumImageBytes = 10 * 1024 * 1024;
    private const int MaximumTemplateArchiveEntries = 16;
    private const long MaximumTemplateArchiveEntryBytes = 25L * 1024 * 1024;
    private const long MaximumTemplateArchiveBytes = 40L * 1024 * 1024;
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public MachineBankRepository(string rootPath)
    {
        if (string.IsNullOrWhiteSpace(rootPath))
        {
            throw new ArgumentException("Le répertoire de la banque doit être renseigné.", nameof(rootPath));
        }

        RootPath = Path.GetFullPath(rootPath);
    }

    public string RootPath { get; }

    public int GetFormatVersion() => LoadOrCreateManifest().FormatVersion;

    public MachineTemplateMetadata Save(MachineTemplatePackage package)
    {
        ArgumentNullException.ThrowIfNull(package);
        Directory.CreateDirectory(RootPath);
        Directory.CreateDirectory(MachinesPath);
        MachineBankManifest manifest = LoadOrCreateManifest();
        Guid templateId = package.Metadata.TemplateId;
        if (templateId == Guid.Empty)
        {
            throw new InvalidOperationException("Le modèle ne contient pas d'identifiant.");
        }

        if (manifest.TemplateIds.Contains(templateId)
            || Directory.Exists(GetTemplateDirectory(templateId)))
        {
            throw new InvalidOperationException(
                $"Le modèle {templateId:D} existe déjà. Aucun fichier n'a été remplacé.");
        }

        ValidateTemplateDocument(package.TemplateDocument, package.Metadata);
        string stagingDirectory = BuildStagingDirectory();
        Directory.CreateDirectory(stagingDirectory);
        try
        {
            MachineTemplateMetadata savedMetadata = WritePackageToDirectory(
                package,
                stagingDirectory,
                manifest.FormatVersion);
            DateTimeOffset now = DateTimeOffset.UtcNow;

            string finalDirectory = GetTemplateDirectory(templateId);
            Directory.Move(stagingDirectory, finalDirectory);
            try
            {
                manifest.TemplateIds.Add(templateId);
                manifest.UpdatedUtc = now;
                SaveManifest(manifest);
            }
            catch
            {
                if (Directory.Exists(finalDirectory))
                {
                    Directory.Delete(finalDirectory, recursive: true);
                }

                throw;
            }

            return savedMetadata;
        }
        finally
        {
            if (Directory.Exists(stagingDirectory))
            {
                Directory.Delete(stagingDirectory, recursive: true);
            }
        }
    }

    public MachineTemplateMetadata Update(MachineTemplatePackage package)
    {
        ArgumentNullException.ThrowIfNull(package);
        MachineBankManifest manifest = LoadOrCreateManifest();
        Guid templateId = package.Metadata.TemplateId;
        if (templateId == Guid.Empty || !manifest.TemplateIds.Contains(templateId))
        {
            throw new FileNotFoundException(
                $"Le modèle {templateId:D} n'existe pas dans la banque et ne peut pas être modifié.");
        }

        ValidateTemplateDocument(package.TemplateDocument, package.Metadata);
        string currentDirectory = GetTemplateDirectory(templateId);
        if (!Directory.Exists(currentDirectory))
        {
            throw new MachineBankCorruptionException(
                $"Le répertoire du modèle {templateId:D} est absent.");
        }

        string stagingDirectory = BuildStagingDirectory();
        string rollbackDirectory = Path.Combine(
            RootPath,
            $".rollback-{templateId:N}-{Guid.NewGuid():N}");
        Directory.CreateDirectory(stagingDirectory);
        try
        {
            MachineTemplateMetadata savedMetadata = WritePackageToDirectory(
                package,
                stagingDirectory,
                manifest.FormatVersion);
            Directory.Move(currentDirectory, rollbackDirectory);
            try
            {
                Directory.Move(stagingDirectory, currentDirectory);
                manifest.UpdatedUtc = DateTimeOffset.UtcNow;
                SaveManifest(manifest);
                PreserveDeletedOrReplacedDirectory(rollbackDirectory, "ReplacedModels", templateId);
                return savedMetadata;
            }
            catch
            {
                if (Directory.Exists(currentDirectory))
                {
                    Directory.Delete(currentDirectory, recursive: true);
                }

                if (Directory.Exists(rollbackDirectory))
                {
                    Directory.Move(rollbackDirectory, currentDirectory);
                }

                throw;
            }
        }
        finally
        {
            if (Directory.Exists(stagingDirectory))
            {
                Directory.Delete(stagingDirectory, recursive: true);
            }
        }
    }

    public MachineTemplatePackage Load(Guid templateId)
    {
        MachineBankManifest manifest = LoadOrCreateManifest();
        if (!manifest.TemplateIds.Contains(templateId))
        {
            throw new FileNotFoundException($"Le modèle {templateId:D} n'est pas référencé dans la banque.");
        }

        return LoadPackageFromDirectory(
            GetTemplateDirectory(templateId),
            expectedTemplateId: templateId,
            expectedFormatVersion: manifest.FormatVersion);
    }

    public IReadOnlyList<MachineTemplateMetadata> List()
    {
        MachineBankManifest manifest = LoadOrCreateManifest();
        return manifest.TemplateIds
            .Select(templateId => LoadMetadataFromDirectory(
                GetTemplateDirectory(templateId),
                templateId,
                manifest.FormatVersion))
            .OrderBy(metadata => metadata.Manufacturer, StringComparer.OrdinalIgnoreCase)
            .ThenBy(metadata => metadata.Model, StringComparer.OrdinalIgnoreCase)
            .ThenBy(metadata => metadata.TemplateName, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public void Delete(Guid templateId)
    {
        MachineBankManifest manifest = LoadOrCreateManifest();
        if (!manifest.TemplateIds.Contains(templateId))
        {
            throw new FileNotFoundException($"Le modèle {templateId:D} n'existe pas dans la banque.");
        }

        string templateDirectory = GetTemplateDirectory(templateId);
        string recycleDirectory = Path.Combine(RootPath, $".deleted-{templateId:N}-{Guid.NewGuid():N}");
        Directory.Move(templateDirectory, recycleDirectory);
        try
        {
            manifest.TemplateIds.Remove(templateId);
            manifest.UpdatedUtc = DateTimeOffset.UtcNow;
            SaveManifest(manifest);
            PreserveDeletedOrReplacedDirectory(recycleDirectory, "DeletedModels", templateId);
        }
        catch
        {
            if (Directory.Exists(recycleDirectory) && !Directory.Exists(templateDirectory))
            {
                Directory.Move(recycleDirectory, templateDirectory);
            }

            if (!manifest.TemplateIds.Contains(templateId))
            {
                manifest.TemplateIds.Add(templateId);
                manifest.UpdatedUtc = DateTimeOffset.UtcNow;
                SaveManifest(manifest);
            }

            throw;
        }
    }

    public void Export(Guid templateId, string destinationArchivePath)
    {
        if (string.IsNullOrWhiteSpace(destinationArchivePath))
        {
            throw new ArgumentException("Le chemin d'export doit être renseigné.", nameof(destinationArchivePath));
        }

        MachineTemplatePackage package = Load(templateId);
        string destination = Path.GetFullPath(destinationArchivePath);
        if (File.Exists(destination))
        {
            throw new IOException($"Le fichier existe déjà et ne sera pas remplacé : {destination}");
        }

        string? directory = Path.GetDirectoryName(destination);
        if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException($"Le dossier d'export n'existe pas : {directory}");
        }

        string temporaryArchive = Path.Combine(directory, $".{Path.GetFileName(destination)}.{Guid.NewGuid():N}.tmp");
        try
        {
            ZipFile.CreateFromDirectory(
                GetTemplateDirectory(package.Metadata.TemplateId),
                temporaryArchive,
                CompressionLevel.Optimal,
                includeBaseDirectory: false);
            File.Move(temporaryArchive, destination);
        }
        finally
        {
            if (File.Exists(temporaryArchive))
            {
                File.Delete(temporaryArchive);
            }
        }
    }

    public MachineTemplateMetadata Import(string archivePath)
    {
        if (string.IsNullOrWhiteSpace(archivePath) || !File.Exists(archivePath))
        {
            throw new FileNotFoundException("L'archive de modèle est introuvable.", archivePath);
        }

        Directory.CreateDirectory(RootPath);
        Directory.CreateDirectory(MachinesPath);
        string extractionDirectory = BuildStagingDirectory();
        string preparedDirectory = BuildStagingDirectory();
        Directory.CreateDirectory(extractionDirectory);
        try
        {
            ExtractArchiveSafely(archivePath, extractionDirectory);
            MachineTemplatePackage package = LoadPackageFromDirectory(extractionDirectory);
            MachineBankManifest manifest = LoadOrCreateManifest();
            Guid templateId = package.Metadata.TemplateId;
            string finalDirectory = GetTemplateDirectory(templateId);
            if (manifest.TemplateIds.Contains(templateId) || Directory.Exists(finalDirectory))
            {
                throw new InvalidOperationException(
                    $"Le modèle {templateId:D} existe déjà. L'import n'a remplacé aucun fichier.");
            }

            Directory.CreateDirectory(preparedDirectory);
            MachineTemplateMetadata importedMetadata = WritePackageToDirectory(
                package,
                preparedDirectory,
                manifest.FormatVersion);
            Directory.Move(preparedDirectory, finalDirectory);
            try
            {
                manifest.TemplateIds.Add(templateId);
                manifest.UpdatedUtc = DateTimeOffset.UtcNow;
                SaveManifest(manifest);
            }
            catch
            {
                if (Directory.Exists(finalDirectory))
                {
                    Directory.Delete(finalDirectory, recursive: true);
                }

                throw;
            }

            return importedMetadata;
        }
        finally
        {
            if (Directory.Exists(extractionDirectory))
            {
                Directory.Delete(extractionDirectory, recursive: true);
            }
            if (Directory.Exists(preparedDirectory))
            {
                Directory.Delete(preparedDirectory, recursive: true);
            }
        }
    }

    private string ManifestPath => Path.Combine(RootPath, "bank.json");

    private string MachinesPath => Path.Combine(RootPath, "machines");

    private MachineBankManifest LoadOrCreateManifest()
    {
        Directory.CreateDirectory(RootPath);
        Directory.CreateDirectory(MachinesPath);
        if (!File.Exists(ManifestPath))
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            MachineBankManifest created = new()
            {
                BankId = Guid.NewGuid(),
                CreatedUtc = now,
                UpdatedUtc = now
            };
            SaveManifest(created);
            return created;
        }

        try
        {
            MachineBankManifest? manifest = JsonSerializer.Deserialize<MachineBankManifest>(
                File.ReadAllText(ManifestPath, Encoding.UTF8),
                JsonOptions);
            if (manifest is null)
            {
                throw new MachineBankCorruptionException("Le fichier bank.json est vide.");
            }

            MachineBankMigrationService.EnsureSupportedBankFormat(manifest.FormatVersion);

            if (manifest.BankId == Guid.Empty)
            {
                throw new MachineBankCorruptionException("Le fichier bank.json ne contient pas de bankId valide.");
            }

            manifest.TemplateIds ??= [];
            if (manifest.TemplateIds.Count != manifest.TemplateIds.Distinct().Count())
            {
                throw new MachineBankCorruptionException("Le fichier bank.json contient des identifiants de modèles dupliqués.");
            }

            return manifest;
        }
        catch (MachineBankCorruptionException)
        {
            throw;
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            throw new MachineBankCorruptionException(
                $"Impossible de lire la banque de machines : {ex.Message}",
                ex);
        }
    }

    private void SaveManifest(MachineBankManifest manifest)
    {
        string temporaryPath = Path.Combine(RootPath, $".bank.{Guid.NewGuid():N}.tmp");
        WriteJson(temporaryPath, manifest);
        try
        {
            if (File.Exists(ManifestPath))
            {
                string backupDirectory = Path.Combine(RootPath, "Backups");
                Directory.CreateDirectory(backupDirectory);
                string backupPath = Path.Combine(
                    backupDirectory,
                    $"bank_{DateTime.UtcNow:yyyyMMdd_HHmmss_fff}_{Guid.NewGuid():N}.json");
                File.Replace(temporaryPath, ManifestPath, backupPath, ignoreMetadataErrors: true);
            }
            else
            {
                File.Move(temporaryPath, ManifestPath);
            }
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    private MachineTemplatePackage LoadPackageFromDirectory(
        string directory,
        Guid? expectedTemplateId = null,
        int? expectedFormatVersion = null)
    {
        try
        {
            string metadataPath = Path.Combine(directory, "machine.json");
            string templatePath = Path.Combine(directory, "template.xml");
            if (!File.Exists(metadataPath) || !File.Exists(templatePath))
            {
                throw new MachineBankCorruptionException(
                    "Le modèle doit contenir machine.json et template.xml.");
            }

            MachineTemplateMetadata? metadata = JsonSerializer.Deserialize<MachineTemplateMetadata>(
                File.ReadAllText(metadataPath, Encoding.UTF8),
                JsonOptions);
            if (metadata is null)
            {
                throw new MachineBankCorruptionException("Le fichier machine.json est vide.");
            }

            ValidateMetadata(metadata, expectedTemplateId, expectedFormatVersion);
            byte[] templateBytes = File.ReadAllBytes(templatePath);
            string actualHash = ComputeSha256(templateBytes);
            if (!string.Equals(actualHash, metadata.TemplateSha256, StringComparison.OrdinalIgnoreCase))
            {
                throw new MachineBankCorruptionException(
                    $"Contrôle SHA-256 invalide pour {metadata.TemplateName}. "
                    + $"Attendu {metadata.TemplateSha256}, obtenu {actualHash}.");
            }

            XDocument templateDocument = XDocument.Load(
                templatePath,
                LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
            ValidateTemplateDocument(templateDocument, metadata);
            string? imagePath = null;
            if (!string.IsNullOrWhiteSpace(metadata.ImageFileName))
            {
                ValidateStoredImageFileName(metadata.ImageFileName);
                imagePath = Path.Combine(directory, metadata.ImageFileName);
                if (!File.Exists(imagePath))
                {
                    throw new MachineBankCorruptionException(
                        $"L'image déclarée '{metadata.ImageFileName}' est absente du modèle.");
                }
                ValidateStoredImage(imagePath, metadata);
            }
            else if (metadata.FormatVersion >= 2
                     && !string.IsNullOrWhiteSpace(metadata.ImageSha256))
            {
                throw new MachineBankCorruptionException(
                    "Le modèle déclare une empreinte d'image sans image associée.");
            }

            return new MachineTemplatePackage(metadata, templateDocument, storedImagePath: imagePath);
        }
        catch (MachineBankCorruptionException)
        {
            throw;
        }
        catch (Exception ex) when (ex is JsonException or XmlException or IOException or UnauthorizedAccessException)
        {
            throw new MachineBankCorruptionException(
                $"Le modèle de machine est illisible : {ex.Message}",
                ex);
        }
    }

    private static MachineTemplateMetadata LoadMetadataFromDirectory(
        string directory,
        Guid expectedTemplateId,
        int expectedFormatVersion)
    {
        try
        {
            string metadataPath = Path.Combine(directory, "machine.json");
            if (!File.Exists(metadataPath))
            {
                throw new MachineBankCorruptionException(
                    $"Le modèle {expectedTemplateId:D} ne contient pas de fichier machine.json.");
            }

            MachineTemplateMetadata? metadata = JsonSerializer.Deserialize<MachineTemplateMetadata>(
                File.ReadAllText(metadataPath, Encoding.UTF8),
                JsonOptions);
            if (metadata is null)
            {
                throw new MachineBankCorruptionException(
                    $"Le fichier machine.json du modèle {expectedTemplateId:D} est vide.");
            }

            ValidateMetadata(metadata, expectedTemplateId, expectedFormatVersion);
            return metadata;
        }
        catch (MachineBankCorruptionException)
        {
            throw;
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            throw new MachineBankCorruptionException(
                $"Impossible de lire les métadonnées du modèle {expectedTemplateId:D} : {ex.Message}",
                ex);
        }
    }

    private static void ValidateMetadata(
        MachineTemplateMetadata metadata,
        Guid? expectedTemplateId,
        int? expectedFormatVersion = null)
    {
        MachineBankMigrationService.EnsureSupportedTemplateFormat(metadata.FormatVersion);
        if (expectedFormatVersion.HasValue
            && metadata.FormatVersion != expectedFormatVersion.Value)
        {
            throw new MachineBankCorruptionException(
                $"Le modèle utilise le format {metadata.FormatVersion}, "
                + $"mais la banque déclare le format {expectedFormatVersion.Value}.");
        }

        if (metadata.TemplateId == Guid.Empty
            || (expectedTemplateId.HasValue && metadata.TemplateId != expectedTemplateId.Value))
        {
            throw new MachineBankCorruptionException("L'identifiant du modèle est absent ou incohérent.");
        }

        if (string.IsNullOrWhiteSpace(metadata.TemplateName))
        {
            throw new MachineBankCorruptionException("Le nom du modèle est absent.");
        }

        if (string.IsNullOrWhiteSpace(metadata.TemplateSha256))
        {
            throw new MachineBankCorruptionException("L'empreinte SHA-256 du modèle est absente.");
        }
    }

    private static void ValidateTemplateDocument(
        XDocument document,
        MachineTemplateMetadata metadata)
    {
        XElement root = document.Root
            ?? throw new MachineBankCorruptionException("Le fichier template.xml ne contient pas de racine.");
        if (!string.Equals(root.Name.LocalName, "device", StringComparison.Ordinal))
        {
            throw new MachineBankCorruptionException("La racine de template.xml doit être <device>.");
        }

        if (!string.Equals(
                root.Name.NamespaceName,
                metadata.SourceXmlNamespace ?? string.Empty,
                StringComparison.Ordinal))
        {
            throw new MachineBankCorruptionException(
                "Le namespace de template.xml ne correspond pas au namespace déclaré dans machine.json.");
        }

        if (root.Child("instance_id") is not null)
        {
            throw new MachineBankCorruptionException(
                "Le modèle contient un instance_id matériel. Enregistrez de nouveau la machine pour neutraliser son identité.");
        }

        if (root.Child("default_name") is not null)
        {
            throw new MachineBankCorruptionException(
                "Le modèle contient encore un default_name propre au matériel source.");
        }

        if (root.Children("interface").Any())
        {
            throw new MachineBankCorruptionException(
                "Le modèle contient une interface réseau propre au projet source.");
        }

        int txCount = root.Children("txchannel").Count();
        int rxCount = root.Children("rxchannel").Count();
        if (txCount != metadata.TxCount || rxCount != metadata.RxCount)
        {
            throw new MachineBankCorruptionException(
                $"Nombre de canaux incohérent : métadonnées {metadata.TxCount} TX/{metadata.RxCount} RX, "
                + $"XML {txCount} TX/{rxCount} RX.");
        }
    }

    private string GetTemplateDirectory(Guid templateId)
    {
        return Path.Combine(MachinesPath, templateId.ToString("D"));
    }

    private string BuildStagingDirectory()
    {
        return Path.Combine(RootPath, $".staging-{Guid.NewGuid():N}");
    }

    private static MachineTemplateMetadata WritePackageToDirectory(
        MachineTemplatePackage package,
        string directory,
        int targetFormatVersion)
    {
        MachineBankMigrationService.EnsureSupportedBankFormat(targetFormatVersion);
        string templatePath = Path.Combine(directory, "template.xml");
        WriteXml(package.TemplateDocument, templatePath);
        string templateHash = ComputeSha256(File.ReadAllBytes(templatePath));
        StoredImage? image = CopyImageIntoDirectory(package.ImagePath, directory);
        DateTimeOffset now = DateTimeOffset.UtcNow;
        MachineTemplateMetadata savedMetadata = package.Metadata with
        {
            FormatVersion = targetFormatVersion,
            ModifiedUtc = now,
            CreatedUtc = package.Metadata.CreatedUtc == default ? now : package.Metadata.CreatedUtc,
            TemplateSha256 = templateHash,
            ImageFileName = image?.FileName,
            ImageSha256 = targetFormatVersion >= 2 ? image?.Sha256 : null
        };
        WriteJson(Path.Combine(directory, "machine.json"), savedMetadata);
        return savedMetadata;
    }

    private void PreserveDeletedOrReplacedDirectory(
        string sourceDirectory,
        string category,
        Guid templateId)
    {
        if (!Directory.Exists(sourceDirectory))
        {
            return;
        }

        string backupDirectory = Path.Combine(RootPath, "Backups", category);
        Directory.CreateDirectory(backupDirectory);
        string destination = Path.Combine(
            backupDirectory,
            $"{templateId:D}_{DateTime.UtcNow:yyyyMMdd_HHmmss_fff}_{Guid.NewGuid():N}");
        Directory.Move(sourceDirectory, destination);
    }

    private static void WriteXml(XDocument document, string path)
    {
        XmlWriterSettings settings = new()
        {
            Encoding = Utf8WithoutBom,
            Indent = true,
            NewLineChars = Environment.NewLine,
            OmitXmlDeclaration = false
        };
        using XmlWriter writer = XmlWriter.Create(path, settings);
        document.Save(writer);
    }

    private static void WriteJson<T>(string path, T value)
    {
        string json = JsonSerializer.Serialize(value, JsonOptions);
        File.WriteAllText(path, json + Environment.NewLine, Utf8WithoutBom);
    }

    private static StoredImage? CopyImageIntoDirectory(
        string? sourcePath,
        string destinationDirectory)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
        {
            return null;
        }

        string fullSourcePath = Path.GetFullPath(sourcePath);
        if (!File.Exists(fullSourcePath))
        {
            throw new FileNotFoundException("L'image du modèle est introuvable.", fullSourcePath);
        }

        FileInfo info = new(fullSourcePath);
        if (info.Length <= 0 || info.Length > MaximumImageBytes)
        {
            throw new InvalidOperationException(
                $"L'image doit avoir une taille comprise entre 1 octet et {MaximumImageBytes / 1024 / 1024} Mio.");
        }

        string extension = Path.GetExtension(fullSourcePath).ToLowerInvariant();
        if (extension is not ".png" and not ".jpg" and not ".jpeg" and not ".webp")
        {
            throw new InvalidOperationException("Formats d'image acceptés : PNG, JPEG et WebP.");
        }

        ValidateImageSignature(fullSourcePath, extension);
        string normalizedExtension = extension == ".jpeg" ? ".jpg" : extension;
        string imageFileName = "image" + normalizedExtension;
        string destinationPath = Path.Combine(destinationDirectory, imageFileName);
        File.Copy(fullSourcePath, destinationPath, overwrite: false);
        return new StoredImage(
            imageFileName,
            ComputeSha256(File.ReadAllBytes(destinationPath)));
    }

    private static void ValidateStoredImageFileName(string imageFileName)
    {
        string extension = Path.GetExtension(imageFileName).ToLowerInvariant();
        if (!string.Equals(
                Path.GetFileName(imageFileName),
                imageFileName,
                StringComparison.Ordinal)
            || !imageFileName.StartsWith("image.", StringComparison.OrdinalIgnoreCase)
            || extension is not ".png" and not ".jpg" and not ".webp")
        {
            throw new MachineBankCorruptionException(
                $"Nom d'image non sûr ou format non pris en charge : {imageFileName}.");
        }
    }

    private static void ValidateStoredImage(
        string imagePath,
        MachineTemplateMetadata metadata)
    {
        string extension = Path.GetExtension(imagePath).ToLowerInvariant();
        try
        {
            ValidateImageSignature(imagePath, extension);
        }
        catch (InvalidOperationException ex)
        {
            throw new MachineBankCorruptionException(
                $"L'image du modèle '{metadata.TemplateName}' est invalide.",
                ex);
        }

        if (metadata.FormatVersion < 2)
        {
            return;
        }
        if (string.IsNullOrWhiteSpace(metadata.ImageSha256))
        {
            throw new MachineBankCorruptionException(
                $"L'empreinte de l'image du modèle '{metadata.TemplateName}' est absente.");
        }

        string actualHash = ComputeSha256(File.ReadAllBytes(imagePath));
        if (!string.Equals(
                actualHash,
                metadata.ImageSha256,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new MachineBankCorruptionException(
                $"Contrôle SHA-256 invalide pour l'image de {metadata.TemplateName}. "
                + $"Attendu {metadata.ImageSha256}, obtenu {actualHash}.");
        }
    }

    private static void ValidateImageSignature(string path, string extension)
    {
        byte[] header = new byte[12];
        using FileStream stream = File.OpenRead(path);
        int read = stream.Read(header, 0, header.Length);
        bool valid = extension switch
        {
            ".png" => read >= 8 && header.AsSpan(0, 8).SequenceEqual(
                new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
            ".jpg" or ".jpeg" => read >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            ".webp" => read >= 12
                && Encoding.ASCII.GetString(header, 0, 4) == "RIFF"
                && Encoding.ASCII.GetString(header, 8, 4) == "WEBP",
            _ => false
        };

        if (!valid)
        {
            throw new InvalidOperationException(
                $"Le contenu du fichier ne correspond pas au format d'image {extension}.");
        }
    }

    private static string ComputeSha256(byte[] bytes)
    {
        return Convert.ToHexString(SHA256.HashData(bytes));
    }

    private static void ExtractArchiveSafely(string archivePath, string destinationDirectory)
    {
        StringComparison pathComparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        string destinationRoot = Path.GetFullPath(destinationDirectory)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        using ZipArchive archive = ZipFile.OpenRead(archivePath);
        if (archive.Entries.Count > MaximumTemplateArchiveEntries)
        {
            throw new MachineBankCorruptionException(
                $"L'archive contient trop d'entrées ({archive.Entries.Count}).");
        }

        long totalBytes = 0;
        HashSet<string> extractedPaths = new(
            OperatingSystem.IsWindows()
                ? StringComparer.OrdinalIgnoreCase
                : StringComparer.Ordinal);
        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            if (entry.Length > MaximumTemplateArchiveEntryBytes)
            {
                throw new MachineBankCorruptionException(
                    $"L'entrée '{entry.FullName}' dépasse la taille autorisée.");
            }

            totalBytes = checked(totalBytes + entry.Length);
            if (totalBytes > MaximumTemplateArchiveBytes)
            {
                throw new MachineBankCorruptionException(
                    "La taille décompressée du modèle dépasse la limite autorisée.");
            }

            string destinationPath = Path.GetFullPath(Path.Combine(destinationDirectory, entry.FullName));
            if (!destinationPath.StartsWith(destinationRoot, pathComparison))
            {
                throw new MachineBankCorruptionException(
                    $"Archive refusée : chemin hors du modèle ({entry.FullName}).");
            }
            if (!extractedPaths.Add(destinationPath))
            {
                throw new MachineBankCorruptionException(
                    $"Archive refusée : chemin dupliqué ({entry.FullName}).");
            }

            if (string.IsNullOrEmpty(entry.Name))
            {
                Directory.CreateDirectory(destinationPath);
                continue;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
            entry.ExtractToFile(destinationPath, overwrite: false);
        }
    }

    private sealed class MachineBankManifest
    {
        public int FormatVersion { get; set; } = MachineBankMigrationService.CurrentBankFormatVersion;

        public Guid BankId { get; set; }

        public DateTimeOffset CreatedUtc { get; set; }

        public DateTimeOffset UpdatedUtc { get; set; }

        public List<Guid> TemplateIds { get; set; } = [];

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? AdditionalData { get; set; }
    }

    private sealed record StoredImage(string FileName, string Sha256);
}
