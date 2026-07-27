using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using DanteConfigEditor.DanteXml;
using DanteConfigEditor.Domain.History;
using DanteConfigEditor.Domain.Validation;
using DanteConfigEditor.Domain.Workspace;
using DanteConfigEditor.Services;

namespace DanteConfigEditor.Infrastructure.Projects;

public sealed class DceProjectPackageService
{
    private static readonly IReadOnlySet<string> AllowedAssetExtensions =
        new HashSet<string>(
            [".png", ".jpg", ".jpeg", ".webp"],
            StringComparer.OrdinalIgnoreCase);

    private readonly IDanteXmlDocumentSerializer _xmlSerializer;
    private readonly DceProjectPackageLimits _limits;
    private readonly JsonSerializerOptions _jsonOptions;

    public DceProjectPackageService(
        IDanteXmlDocumentSerializer? xmlSerializer = null,
        DceProjectPackageLimits? limits = null)
    {
        _xmlSerializer = xmlSerializer ?? new DanteXmlDocumentSerializer();
        _limits = limits ?? new DceProjectPackageLimits();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };
        _jsonOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    }

    public DceProjectSaveResult Save(
        DceProjectWriteRequest request,
        string destinationPath,
        Action<DceProjectSaveStage>? saveStageObserver = null)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(destinationPath))
        {
            throw new ArgumentException("A destination path is required.", nameof(destinationPath));
        }

        string fullDestinationPath = Path.GetFullPath(destinationPath);
        string destinationDirectory = Path.GetDirectoryName(fullDestinationPath)
            ?? throw new InvalidOperationException("The destination directory cannot be resolved.");
        if (!Directory.Exists(destinationDirectory))
        {
            throw new DirectoryNotFoundException(destinationDirectory);
        }

        if (request.Project.Validate().HasErrors)
        {
            throw new InvalidOperationException("DceProject.Validation.XmlHasBlockingErrors");
        }

        string temporaryPath = Path.Combine(
            destinationDirectory,
            $".{Path.GetFileName(fullDestinationPath)}.{Guid.NewGuid():N}.tmp");
        string backupPath = string.Empty;

        try
        {
            WritePackage(request, temporaryPath);
            saveStageObserver?.Invoke(DceProjectSaveStage.AfterTemporaryPackageCreated);
            _ = Open(temporaryPath);
            saveStageObserver?.Invoke(DceProjectSaveStage.BeforeDestinationCommit);

            if (File.Exists(fullDestinationPath))
            {
                backupPath = SafeFileService.BuildDestinationBackupPath(fullDestinationPath);
                File.Replace(
                    temporaryPath,
                    fullDestinationPath,
                    backupPath,
                    ignoreMetadataErrors: true);
            }
            else
            {
                File.Move(temporaryPath, fullDestinationPath);
            }

            return new DceProjectSaveResult(
                fullDestinationPath,
                backupPath,
                HashFile(fullDestinationPath));
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    public DceProjectPackage Open(string packagePath)
    {
        if (string.IsNullOrWhiteSpace(packagePath))
        {
            throw new ArgumentException("A package path is required.", nameof(packagePath));
        }

        string fullPath = Path.GetFullPath(packagePath);
        using FileStream stream = new(
            fullPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read);
        using ZipArchive archive = new(stream, ZipArchiveMode.Read, leaveOpen: false);

        Dictionary<string, ZipArchiveEntry> entries = ValidateArchiveStructure(archive);
        DceProjectManifest manifest = ReadJson<DceProjectManifest>(
            entries[DceProjectPackageEntries.Manifest],
            DceProjectPackageEntries.Manifest);
        ValidateManifest(manifest);
        ValidateHashes(entries, manifest);

        using Stream xmlStream = entries[manifest.DanteXmlEntry].Open();
        DanteXmlOpenResult openedXml = _xmlSerializer.Load(
            xmlStream,
            $"{fullPath}|{manifest.DanteXmlEntry}");

        DceWorkspaceMetadataPayload metadataPayload = ReadJson<DceWorkspaceMetadataPayload>(
            entries[DceProjectPackageEntries.Metadata],
            DceProjectPackageEntries.Metadata);
        IReadOnlyList<SynopticNodeLayout> layout = ReadJson<SynopticNodeLayout[]>(
            entries[DceProjectPackageEntries.Layout],
            DceProjectPackageEntries.Layout);
        DceAnnotationsPayload annotations = ReadJson<DceAnnotationsPayload>(
            entries[DceProjectPackageEntries.Annotations],
            DceProjectPackageEntries.Annotations);
        IReadOnlyList<ProjectHistoryEntry> history = ReadJson<ProjectHistoryEntry[]>(
            entries[DceProjectPackageEntries.History],
            DceProjectPackageEntries.History);
        ProjectViewSettings settings = ReadJson<ProjectViewSettings>(
            entries[DceProjectPackageEntries.Settings],
            DceProjectPackageEntries.Settings);
        ProjectValidationState validation = entries.TryGetValue(
            DceProjectPackageEntries.Validation,
            out ZipArchiveEntry? validationEntry)
            ? ReadJson<ProjectValidationState>(
                validationEntry,
                DceProjectPackageEntries.Validation)
            : ProjectValidationState.Empty;
        Dictionary<string, byte[]> assets = entries
            .Where(item => item.Key.StartsWith("assets/", StringComparison.Ordinal))
            .ToDictionary(
                item => item.Key["assets/".Length..],
                item => ReadBytes(item.Value),
                StringComparer.Ordinal);

        ProjectMetadata metadata = metadataPayload.ToMetadata();
        ProjectWorkspaceData workspace = new(
            metadata,
            settings,
            layout,
            annotations.Annotations,
            annotations.HiddenDeviceIds,
            metadataPayload.WorkspaceExtensions);

        return new DceProjectPackage(
            fullPath,
            manifest,
            openedXml,
            workspace,
            history,
            validation,
            assets);
    }

    private void WritePackage(DceProjectWriteRequest request, string path)
    {
        byte[] xml = _xmlSerializer.Serialize(request.Project);
        byte[] metadata = SerializeJson(
            DceWorkspaceMetadataPayload.FromWorkspace(request.Workspace));
        byte[] layout = SerializeJson(request.Workspace.SynopticLayout);
        byte[] annotations = SerializeJson(new DceAnnotationsPayload(
            request.Workspace.Annotations,
            request.Workspace.HiddenDeviceIds));
        byte[] history = SerializeJson(request.History);
        byte[] settings = SerializeJson(request.Workspace.ViewSettings);
        byte[] validation = SerializeJson(request.Validation);

        Dictionary<string, byte[]> content = new(StringComparer.Ordinal)
        {
            [DceProjectPackageEntries.DanteXml] = xml,
            [DceProjectPackageEntries.Metadata] = metadata,
            [DceProjectPackageEntries.Layout] = layout,
            [DceProjectPackageEntries.Annotations] = annotations,
            [DceProjectPackageEntries.History] = history,
            [DceProjectPackageEntries.Settings] = settings,
            [DceProjectPackageEntries.Validation] = validation
        };
        foreach ((string relativePath, byte[] bytes) in request.Assets
                     ?? new Dictionary<string, byte[]>(StringComparer.Ordinal))
        {
            string assetEntry = BuildAssetEntryName(relativePath);
            if (!content.TryAdd(assetEntry, bytes))
            {
                throw new InvalidDataException(
                    $"DceProject.Archive.DuplicateEntry:{assetEntry}");
            }
        }
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DceProjectManifest manifest = new()
        {
            SchemaVersion = DceProjectManifest.CurrentSchemaVersion,
            CreatedWithVersion = request.ApplicationVersion,
            CreatedAt = request.ExistingManifest?.CreatedAt ?? now,
            ModifiedAt = now,
            ProjectName = request.Workspace.Metadata.Name,
            Description = request.Workspace.Metadata.Description,
            DanteXmlEntry = DceProjectPackageEntries.DanteXml,
            ContentSha256 = content.ToDictionary(
                item => item.Key,
                item => HashBytes(item.Value),
                StringComparer.Ordinal),
            AdditionalData = CloneAdditionalData(request.ExistingManifest?.AdditionalData)
        };
        byte[] manifestBytes = SerializeJson(manifest);
        Dictionary<string, byte[]> packageEntries = new(content, StringComparer.Ordinal)
        {
            [DceProjectPackageEntries.Manifest] = manifestBytes
        };
        ValidateContentSizes(packageEntries);

        using FileStream stream = new(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
        using ZipArchive archive = new(stream, ZipArchiveMode.Create, leaveOpen: false);
        WriteEntry(archive, DceProjectPackageEntries.Manifest, manifestBytes);
        foreach ((string entryName, byte[] bytes) in content.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            WriteEntry(archive, entryName, bytes);
        }
    }

    private Dictionary<string, ZipArchiveEntry> ValidateArchiveStructure(ZipArchive archive)
    {
        if (archive.Entries.Count == 0 || archive.Entries.Count > _limits.MaximumEntryCount)
        {
            throw new InvalidDataException("DceProject.Archive.EntryCountInvalid");
        }

        Dictionary<string, ZipArchiveEntry> entries = new(StringComparer.Ordinal);
        long total = 0;
        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            ValidateEntryName(entry.FullName);
            if (!entries.TryAdd(entry.FullName, entry))
            {
                throw new InvalidDataException($"DceProject.Archive.DuplicateEntry:{entry.FullName}");
            }

            long limit = EntryLimit(entry.FullName);
            if (entry.Length < 0 || entry.Length > limit)
            {
                throw new InvalidDataException($"DceProject.Archive.EntryTooLarge:{entry.FullName}");
            }

            checked
            {
                total += entry.Length;
            }
            if (total > _limits.MaximumTotalUncompressedBytes)
            {
                throw new InvalidDataException("DceProject.Archive.TotalSizeExceeded");
            }
        }

        string[] missing = DceProjectPackageEntries.Required
            .Where(required => !entries.ContainsKey(required))
            .ToArray();
        if (missing.Length > 0)
        {
            throw new InvalidDataException(
                $"DceProject.Archive.RequiredEntriesMissing:{string.Join(",", missing)}");
        }

        return entries;
    }

    private static void ValidateEntryName(string entryName)
    {
        if (string.IsNullOrWhiteSpace(entryName)
            || entryName.StartsWith("/", StringComparison.Ordinal)
            || entryName.Contains('\\', StringComparison.Ordinal)
            || entryName.Split('/').Any(segment => segment is "." or "..")
            || entryName.Contains('\0'))
        {
            throw new InvalidDataException($"DceProject.Archive.UnsafeEntry:{entryName}");
        }
    }

    private static string BuildAssetEntryName(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)
            || relativePath.StartsWith("assets/", StringComparison.Ordinal))
        {
            throw new InvalidDataException(
                $"DceProject.Asset.InvalidRelativePath:{relativePath}");
        }

        string entryName = $"assets/{relativePath}";
        ValidateEntryName(entryName);
        ValidateAssetExtension(entryName);
        return entryName;
    }

    private static void ValidateAssetExtension(string entryName)
    {
        if (!AllowedAssetExtensions.Contains(Path.GetExtension(entryName)))
        {
            throw new InvalidDataException(
                $"DceProject.Asset.UnsupportedFormat:{entryName}");
        }
    }

    private void ValidateManifest(DceProjectManifest manifest)
    {
        if (!Version.TryParse(manifest.SchemaVersion, out Version? version)
            || version.Major != 1)
        {
            throw new InvalidDataException(
                $"DceProject.Manifest.UnsupportedSchema:{manifest.SchemaVersion}");
        }

        ValidateEntryName(manifest.DanteXmlEntry);
        if (!string.Equals(
                manifest.DanteXmlEntry,
                DceProjectPackageEntries.DanteXml,
                StringComparison.Ordinal))
        {
            throw new InvalidDataException("DceProject.Manifest.UnexpectedXmlEntry");
        }
        if (string.IsNullOrWhiteSpace(manifest.ProjectName))
        {
            throw new InvalidDataException("DceProject.Manifest.ProjectNameRequired");
        }
        if (manifest.ContentSha256 is null)
        {
            throw new InvalidDataException("DceProject.Manifest.HashesRequired");
        }

        string[] missingHashes = DceProjectPackageEntries.Required
            .Where(entry => !string.Equals(
                entry,
                DceProjectPackageEntries.Manifest,
                StringComparison.Ordinal))
            .Where(entry => !manifest.ContentSha256.ContainsKey(entry))
            .ToArray();
        if (missingHashes.Length > 0)
        {
            throw new InvalidDataException(
                $"DceProject.Manifest.HashesMissing:{string.Join(",", missingHashes)}");
        }
    }

    private void ValidateHashes(
        IReadOnlyDictionary<string, ZipArchiveEntry> entries,
        DceProjectManifest manifest)
    {
        foreach (string entryName in entries.Keys.Where(entry => !string.Equals(
                     entry,
                     DceProjectPackageEntries.Manifest,
                     StringComparison.Ordinal)))
        {
            if (entryName.StartsWith("assets/", StringComparison.Ordinal))
            {
                ValidateAssetExtension(entryName);
            }
            if (!manifest.ContentSha256.ContainsKey(entryName))
            {
                throw new InvalidDataException($"DceProject.Hash.Missing:{entryName}");
            }
        }

        foreach ((string entryName, string expectedHash) in manifest.ContentSha256)
        {
            if (!entries.TryGetValue(entryName, out ZipArchiveEntry? entry))
            {
                throw new InvalidDataException($"DceProject.Hash.EntryMissing:{entryName}");
            }

            using Stream stream = entry.Open();
            string actualHash = HashStream(stream);
            if (!string.Equals(expectedHash, actualHash, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException($"DceProject.Hash.Mismatch:{entryName}");
            }
        }
    }

    private T ReadJson<T>(ZipArchiveEntry entry, string entryName)
    {
        using Stream stream = entry.Open();
        T? value;
        try
        {
            value = JsonSerializer.Deserialize<T>(stream, _jsonOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException($"DceProject.Json.Invalid:{entryName}", ex);
        }

        return value ?? throw new InvalidDataException($"DceProject.Json.Empty:{entryName}");
    }

    private static byte[] ReadBytes(ZipArchiveEntry entry)
    {
        using Stream source = entry.Open();
        using MemoryStream destination = new((int)entry.Length);
        source.CopyTo(destination);
        return destination.ToArray();
    }

    private void ValidateContentSizes(IReadOnlyDictionary<string, byte[]> content)
    {
        long total = 0;
        foreach ((string entryName, byte[] bytes) in content)
        {
            if (bytes.LongLength > EntryLimit(entryName))
            {
                throw new InvalidDataException($"DceProject.Archive.EntryTooLarge:{entryName}");
            }

            checked
            {
                total += bytes.LongLength;
            }
        }

        if (total > _limits.MaximumTotalUncompressedBytes)
        {
            throw new InvalidDataException("DceProject.Archive.TotalSizeExceeded");
        }
    }

    private long EntryLimit(string entryName)
    {
        if (string.Equals(entryName, DceProjectPackageEntries.DanteXml, StringComparison.Ordinal))
        {
            return _limits.MaximumXmlBytes;
        }
        if (entryName.StartsWith("assets/", StringComparison.Ordinal))
        {
            return _limits.MaximumAssetBytes;
        }

        return _limits.MaximumJsonBytes;
    }

    private byte[] SerializeJson<T>(T value) =>
        JsonSerializer.SerializeToUtf8Bytes(value, _jsonOptions);

    private static void WriteEntry(ZipArchive archive, string name, byte[] bytes)
    {
        ValidateEntryName(name);
        ZipArchiveEntry entry = archive.CreateEntry(name, CompressionLevel.Optimal);
        using Stream stream = entry.Open();
        stream.Write(bytes);
    }

    private static Dictionary<string, JsonElement>? CloneAdditionalData(
        IReadOnlyDictionary<string, JsonElement>? source)
    {
        return source?.ToDictionary(
            item => item.Key,
            item => item.Value.Clone(),
            StringComparer.Ordinal);
    }

    private static string HashBytes(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static string HashStream(Stream stream) =>
        Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();

    private static string HashFile(string path)
    {
        using FileStream stream = File.OpenRead(path);
        return HashStream(stream);
    }
}
