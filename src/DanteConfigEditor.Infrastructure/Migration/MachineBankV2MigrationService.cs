using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using DanteConfigEditor.Models;
using DanteConfigEditor.Services;

namespace DanteConfigEditor.Infrastructure.Migration;

public sealed class MachineBankV2MigrationService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public MachineBankV2MigrationResult Migrate(
        string sourceBankPath,
        string destinationBankPath)
    {
        string source = Path.GetFullPath(sourceBankPath);
        string destination = Path.GetFullPath(destinationBankPath);
        ValidateLocations(source, destination);

        string sourceManifestPath = Path.Combine(source, "bank.json");
        if (!File.Exists(sourceManifestPath))
        {
            throw new FileNotFoundException(
                "La banque source ne contient pas bank.json.",
                sourceManifestPath);
        }

        MachineBankRepository sourceRepository = new(source);
        int sourceFormat = sourceRepository.GetFormatVersion();
        if (sourceFormat != 1)
        {
            throw new InvalidOperationException(
                $"La migration attend une banque V3.6 au format 1, pas le format {sourceFormat}.");
        }

        MachineTemplatePackage[] sourceTemplates = sourceRepository.List()
            .Select(metadata => sourceRepository.Load(metadata.TemplateId))
            .ToArray();
        Dictionary<string, string> sourceHashesBefore = HashBankContent(source);
        string parent = Path.GetDirectoryName(destination)
            ?? throw new InvalidOperationException(
                "Le dossier parent de la banque de destination est introuvable.");
        Directory.CreateDirectory(parent);
        string backupDirectory = Path.Combine(parent, "DCE_MachineBank_Migration_Backups");
        Directory.CreateDirectory(backupDirectory);
        string backupPath = Path.Combine(
            backupDirectory,
            $"MachineBank_V1_{DateTime.UtcNow:yyyyMMdd_HHmmss_fff}_{Guid.NewGuid():N}.dce-bank.zip");
        MachineBankArchiveService.ExportBank(source, backupPath);

        string staging = Path.Combine(
            parent,
            $".dce-bank-v2-migration-{Guid.NewGuid():N}");
        try
        {
            MachineBankArchiveService.RestoreBank(backupPath, staging);
            UpgradeStagingJson(staging, sourceTemplates);

            MachineBankRepository migratedRepository = new(staging);
            if (migratedRepository.GetFormatVersion()
                != MachineBankMigrationService.CurrentBankFormatVersion)
            {
                throw new InvalidDataException(
                    "La banque migrée ne déclare pas le format courant.");
            }

            MachineTemplatePackage[] migratedTemplates = migratedRepository.List()
                .Select(metadata => migratedRepository.Load(metadata.TemplateId))
                .ToArray();
            ValidateMigratedTemplates(sourceTemplates, migratedTemplates);

            Dictionary<string, string> sourceHashesAfter = HashBankContent(source);
            if (!DictionaryEqual(sourceHashesBefore, sourceHashesAfter))
            {
                throw new InvalidDataException(
                    "La banque source a changé pendant la migration. La destination n'est pas publiée.");
            }

            MachineBankV2MigrationReport report = new(
                SchemaVersion: 1,
                SourceBankPath: source,
                DestinationBankPath: destination,
                SourceFormatVersion: sourceFormat,
                DestinationFormatVersion: MachineBankMigrationService.CurrentBankFormatVersion,
                MigratedAtUtc: DateTimeOffset.UtcNow,
                BackupArchivePath: backupPath,
                TemplateCount: migratedTemplates.Length,
                TemplateIds: migratedTemplates
                    .Select(template => template.Metadata.TemplateId)
                    .Order()
                    .ToArray(),
                SourceContentSha256: sourceHashesBefore);
            WriteJsonAtomically(
                Path.Combine(staging, "migration-v1-to-v2.json"),
                report);

            if (Directory.Exists(destination))
            {
                Directory.Delete(destination, recursive: false);
            }
            Directory.Move(staging, destination);
            return new MachineBankV2MigrationResult(destination, backupPath, report);
        }
        finally
        {
            if (Directory.Exists(staging))
            {
                Directory.Delete(staging, recursive: true);
            }
        }
    }

    private static void UpgradeStagingJson(
        string staging,
        IReadOnlyList<MachineTemplatePackage> templates)
    {
        foreach (MachineTemplatePackage template in templates)
        {
            string templateDirectory = Path.Combine(
                staging,
                "machines",
                template.Metadata.TemplateId.ToString("D"));
            string metadataPath = Path.Combine(templateDirectory, "machine.json");
            JsonObject metadata = ReadJsonObject(metadataPath);
            metadata["formatVersion"] = MachineTemplateMetadata.CurrentFormatVersion;

            string? imageFileName = metadata["imageFileName"]?.GetValue<string?>();
            if (string.IsNullOrWhiteSpace(imageFileName))
            {
                metadata["imageSha256"] = null;
            }
            else
            {
                if (!string.Equals(
                        Path.GetFileName(imageFileName),
                        imageFileName,
                        StringComparison.Ordinal))
                {
                    throw new InvalidDataException(
                        $"Nom d'image non sûr pendant la migration : {imageFileName}.");
                }

                string imagePath = Path.Combine(templateDirectory, imageFileName);
                metadata["imageSha256"] = HashFile(imagePath);
            }

            WriteJsonAtomically(metadataPath, metadata);
        }

        string manifestPath = Path.Combine(staging, "bank.json");
        JsonObject manifest = ReadJsonObject(manifestPath);
        manifest["formatVersion"] = MachineBankMigrationService.CurrentBankFormatVersion;
        manifest["updatedUtc"] = DateTimeOffset.UtcNow;
        WriteJsonAtomically(manifestPath, manifest);
    }

    private static void ValidateMigratedTemplates(
        IReadOnlyList<MachineTemplatePackage> source,
        IReadOnlyList<MachineTemplatePackage> migrated)
    {
        Dictionary<Guid, MachineTemplatePackage> migratedById = migrated.ToDictionary(
            template => template.Metadata.TemplateId);
        if (source.Count != migratedById.Count)
        {
            throw new InvalidDataException(
                "Le nombre de modèles diffère après migration.");
        }

        foreach (MachineTemplatePackage sourceTemplate in source)
        {
            if (!migratedById.TryGetValue(
                    sourceTemplate.Metadata.TemplateId,
                    out MachineTemplatePackage? migratedTemplate))
            {
                throw new InvalidDataException(
                    $"Le modèle {sourceTemplate.Metadata.TemplateId:D} manque après migration.");
            }

            if (sourceTemplate.Metadata.TxCount != migratedTemplate.Metadata.TxCount
                || sourceTemplate.Metadata.RxCount != migratedTemplate.Metadata.RxCount
                || !string.Equals(
                    sourceTemplate.Metadata.TemplateName,
                    migratedTemplate.Metadata.TemplateName,
                    StringComparison.Ordinal)
                || !XmlSemanticComparisonService.Compare(
                    sourceTemplate.TemplateDocument,
                    migratedTemplate.TemplateDocument).AreEquivalent)
            {
                throw new InvalidDataException(
                    $"Le modèle {sourceTemplate.Metadata.TemplateId:D} a changé pendant la migration.");
            }
        }
    }

    private static void ValidateLocations(string source, string destination)
    {
        if (!Directory.Exists(source))
        {
            throw new DirectoryNotFoundException(source);
        }
        if (string.Equals(source, destination, PathComparison()))
        {
            throw new InvalidOperationException(
                "La migration doit écrire dans une nouvelle banque.");
        }

        string relative = Path.GetRelativePath(source, destination);
        if (!Path.IsPathRooted(relative)
            && !relative.StartsWith("..", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "La destination ne peut pas se trouver dans la banque source.");
        }
        if (File.Exists(destination)
            || (Directory.Exists(destination)
                && Directory.EnumerateFileSystemEntries(destination).Any()))
        {
            throw new IOException(
                "La destination de migration doit être nouvelle ou vide.");
        }
    }

    private static JsonObject ReadJsonObject(string path)
    {
        try
        {
            return JsonNode.Parse(File.ReadAllText(path, Encoding.UTF8)) as JsonObject
                ?? throw new InvalidDataException($"Objet JSON attendu : {path}");
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException($"JSON invalide : {path}", ex);
        }
    }

    private static void WriteJsonAtomically<T>(string path, T value)
    {
        string temporaryPath = path + $".{Guid.NewGuid():N}.tmp";
        string backupPath = path + ".bak";
        try
        {
            File.WriteAllText(
                temporaryPath,
                JsonSerializer.Serialize(value, JsonOptions) + Environment.NewLine,
                new UTF8Encoding(false));
            _ = JsonNode.Parse(File.ReadAllText(temporaryPath, Encoding.UTF8))
                ?? throw new InvalidDataException(
                    $"Le JSON temporaire est vide : {path}");
            if (File.Exists(path))
            {
                File.Replace(
                    temporaryPath,
                    path,
                    backupPath,
                    ignoreMetadataErrors: true);
                File.Delete(backupPath);
            }
            else
            {
                File.Move(temporaryPath, path);
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

    private static Dictionary<string, string> HashBankContent(string root)
    {
        string[] roots =
        [
            Path.Combine(root, "bank.json"),
            Path.Combine(root, "machines")
        ];
        return roots
            .SelectMany(path => File.Exists(path)
                ? [path]
                : Directory.Exists(path)
                    ? Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories)
                    : [])
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                path => Path.GetRelativePath(root, path),
                HashFile,
                OperatingSystem.IsWindows()
                    ? StringComparer.OrdinalIgnoreCase
                    : StringComparer.Ordinal);
    }

    private static bool DictionaryEqual(
        IReadOnlyDictionary<string, string> left,
        IReadOnlyDictionary<string, string> right) =>
        left.Count == right.Count
        && left.All(item => right.TryGetValue(item.Key, out string? value)
                            && string.Equals(
                                item.Value,
                                value,
                                StringComparison.OrdinalIgnoreCase));

    private static string HashFile(string path)
    {
        using FileStream stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static StringComparison PathComparison() =>
        OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
}

public sealed record MachineBankV2MigrationReport(
    int SchemaVersion,
    string SourceBankPath,
    string DestinationBankPath,
    int SourceFormatVersion,
    int DestinationFormatVersion,
    DateTimeOffset MigratedAtUtc,
    string BackupArchivePath,
    int TemplateCount,
    IReadOnlyList<Guid> TemplateIds,
    IReadOnlyDictionary<string, string> SourceContentSha256);

public sealed record MachineBankV2MigrationResult(
    string DestinationBankPath,
    string BackupArchivePath,
    MachineBankV2MigrationReport Report);
