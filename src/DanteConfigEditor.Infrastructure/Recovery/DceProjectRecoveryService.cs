using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DanteConfigEditor.Infrastructure.Projects;
using DanteConfigEditor.Services;

namespace DanteConfigEditor.Infrastructure.Recovery;

public sealed class DceProjectRecoveryService
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> WriteGates =
        new(StringComparer.OrdinalIgnoreCase);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private readonly DceProjectPackageService _packageService;
    private readonly string _recoveryDirectory;
    private readonly SemaphoreSlim _writeGate;

    public DceProjectRecoveryService(
        DceProjectPackageService? packageService = null,
        string? recoveryDirectory = null)
    {
        _packageService = packageService ?? new DceProjectPackageService();
        _recoveryDirectory = Path.GetFullPath(
            string.IsNullOrWhiteSpace(recoveryDirectory)
                ? ApplicationStoragePaths.Resolve("ProjectRecovery")
                : recoveryDirectory);
        _writeGate = WriteGates.GetOrAdd(
            _recoveryDirectory,
            _ => new SemaphoreSlim(1, 1));
    }

    public async Task<DceProjectRecoveryCandidate> SaveAsync(
        DceProjectWriteRequest request,
        string projectReference,
        CancellationToken cancellationToken = default,
        Action<DceProjectRecoveryStage>? stageObserver = null)
    {
        ArgumentNullException.ThrowIfNull(request);
        string normalizedReference = NormalizeReference(projectReference);
        await _writeGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await Task.Run(
                () => SaveCore(
                    request,
                    normalizedReference,
                    cancellationToken,
                    stageObserver),
                cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _writeGate.Release();
        }
    }

    public DceProjectRecoveryCandidate? Find(string projectReference)
    {
        string normalizedReference = NormalizeReference(projectReference);
        string identifier = BuildIdentifier(normalizedReference);
        string metadataPath = MetadataPath(identifier);
        if (!File.Exists(metadataPath))
        {
            return null;
        }

        DceProjectRecoveryMetadata metadata = ReadMetadata(metadataPath);
        if (!string.Equals(
                metadata.ProjectReference,
                normalizedReference,
                ReferenceComparison()))
        {
            throw new InvalidDataException(
                "DceProject.Recovery.ReferenceMismatch");
        }

        string packagePath = ResolvePackagePath(identifier, metadata.PackageFileName);
        if (!File.Exists(packagePath))
        {
            throw new InvalidDataException(
                "DceProject.Recovery.PackageMissing");
        }
        if (!string.Equals(
                HashFile(packagePath),
                metadata.PackageSha256,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException(
                "DceProject.Recovery.PackageHashMismatch");
        }

        DceProjectPackage package = _packageService.Open(packagePath);
        return new DceProjectRecoveryCandidate(
            normalizedReference,
            packagePath,
            metadata.SavedAtUtc,
            SourceMatches(metadata),
            package);
    }

    public async Task DeleteAsync(
        string projectReference,
        CancellationToken cancellationToken = default)
    {
        string normalizedReference = NormalizeReference(projectReference);
        await _writeGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            DeleteCore(BuildIdentifier(normalizedReference));
        }
        finally
        {
            _writeGate.Release();
        }
    }

    public void CleanupOld(TimeSpan maximumAge)
    {
        if (maximumAge < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumAge));
        }
        if (!Directory.Exists(_recoveryDirectory))
        {
            return;
        }

        DateTimeOffset limit = DateTimeOffset.UtcNow - maximumAge;
        HashSet<string> referencedPackages = new(StringComparer.OrdinalIgnoreCase);
        foreach (string metadataPath in Directory.EnumerateFiles(
                     _recoveryDirectory,
                     "*.recovery.json",
                     SearchOption.TopDirectoryOnly))
        {
            try
            {
                DceProjectRecoveryMetadata metadata = ReadMetadata(metadataPath);
                if (metadata.SavedAtUtc < limit)
                {
                    DeleteCore(IdentifierFromMetadataPath(metadataPath));
                    continue;
                }

                referencedPackages.Add(Path.Combine(
                    _recoveryDirectory,
                    metadata.PackageFileName));
            }
            catch (Exception ex) when (ex is IOException
                                       or UnauthorizedAccessException
                                       or JsonException
                                       or InvalidDataException)
            {
                if (File.GetLastWriteTimeUtc(metadataPath) < limit.UtcDateTime)
                {
                    File.Delete(metadataPath);
                }
            }
        }

        foreach (string packagePath in Directory.EnumerateFiles(
                     _recoveryDirectory,
                     "*.recovery.dceproj",
                     SearchOption.TopDirectoryOnly))
        {
            if (!referencedPackages.Contains(packagePath)
                && File.GetLastWriteTimeUtc(packagePath) < limit.UtcDateTime)
            {
                File.Delete(packagePath);
            }
        }

        foreach (string temporaryPath in Directory.EnumerateFiles(
                     _recoveryDirectory,
                     "*.tmp",
                     SearchOption.TopDirectoryOnly))
        {
            if (File.GetLastWriteTimeUtc(temporaryPath) < limit.UtcDateTime)
            {
                File.Delete(temporaryPath);
            }
        }
    }

    private DceProjectRecoveryCandidate SaveCore(
        DceProjectWriteRequest request,
        string normalizedReference,
        CancellationToken cancellationToken,
        Action<DceProjectRecoveryStage>? stageObserver)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Directory.CreateDirectory(_recoveryDirectory);
        string identifier = BuildIdentifier(normalizedReference);
        string metadataPath = MetadataPath(identifier);
        DceProjectRecoveryMetadata? previousMetadata = TryReadMetadata(metadataPath);
        string packageFileName =
            $"{identifier}.{DateTime.UtcNow:yyyyMMddHHmmssfff}.{Guid.NewGuid():N}.recovery.dceproj";
        string packagePath = Path.Combine(_recoveryDirectory, packageFileName);
        bool metadataCommitted = false;

        try
        {
            DceProjectSaveResult saved = _packageService.Save(
                request,
                packagePath);
            cancellationToken.ThrowIfCancellationRequested();
            stageObserver?.Invoke(DceProjectRecoveryStage.AfterRecoveryPackageSaved);

            SourceFingerprint source = CaptureSource(normalizedReference);
            DceProjectRecoveryMetadata metadata = new(
                SchemaVersion: 1,
                ProjectReference: normalizedReference,
                SourceExists: source.Exists,
                SourceLastWriteTimeUtc: source.LastWriteTimeUtc,
                SourceLength: source.Length,
                SourceSha256: source.Sha256,
                SavedAtUtc: DateTimeOffset.UtcNow,
                PackageFileName: packageFileName,
                PackageSha256: saved.PackageSha256);

            stageObserver?.Invoke(DceProjectRecoveryStage.BeforeRecoveryMetadataCommit);
            cancellationToken.ThrowIfCancellationRequested();
            WriteMetadataAtomically(metadataPath, metadata);
            metadataCommitted = true;

            if (previousMetadata is not null
                && !string.Equals(
                    previousMetadata.PackageFileName,
                    packageFileName,
                    StringComparison.OrdinalIgnoreCase))
            {
                DeleteIfExists(ResolvePackagePath(
                    identifier,
                    previousMetadata.PackageFileName));
            }

            DceProjectPackage package = _packageService.Open(packagePath);
            return new DceProjectRecoveryCandidate(
                normalizedReference,
                packagePath,
                metadata.SavedAtUtc,
                SourceMatches(metadata),
                package);
        }
        catch
        {
            if (!metadataCommitted)
            {
                DeleteIfExists(packagePath);
            }
            throw;
        }
    }

    private void DeleteCore(string identifier)
    {
        string metadataPath = MetadataPath(identifier);
        DceProjectRecoveryMetadata? metadata = TryReadMetadata(metadataPath);
        if (metadata is not null)
        {
            try
            {
                DeleteIfExists(ResolvePackagePath(identifier, metadata.PackageFileName));
            }
            catch (InvalidDataException)
            {
                // Le nettoyage par motif ci-dessous reste borné à ce dossier.
            }
        }

        DeleteIfExists(metadataPath);
        DeleteIfExists(metadataPath + ".bak");
        foreach (string packagePath in Directory.Exists(_recoveryDirectory)
                     ? Directory.EnumerateFiles(
                         _recoveryDirectory,
                         $"{identifier}.*.recovery.dceproj",
                         SearchOption.TopDirectoryOnly)
                     : [])
        {
            DeleteIfExists(packagePath);
        }
    }

    private string MetadataPath(string identifier) =>
        Path.Combine(_recoveryDirectory, $"{identifier}.recovery.json");

    private static string IdentifierFromMetadataPath(string metadataPath)
    {
        const string suffix = ".recovery.json";
        string fileName = Path.GetFileName(metadataPath);
        if (!fileName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException(
                "DceProject.Recovery.MetadataNameInvalid");
        }

        string identifier = fileName[..^suffix.Length];
        if (identifier.Length != 24
            || identifier.Any(character => !Uri.IsHexDigit(character)))
        {
            throw new InvalidDataException(
                "DceProject.Recovery.MetadataNameInvalid");
        }

        return identifier;
    }

    private string ResolvePackagePath(string identifier, string packageFileName)
    {
        if (string.IsNullOrWhiteSpace(packageFileName)
            || !string.Equals(
                Path.GetFileName(packageFileName),
                packageFileName,
                StringComparison.Ordinal)
            || !packageFileName.StartsWith($"{identifier}.", StringComparison.Ordinal)
            || !packageFileName.EndsWith(
                ".recovery.dceproj",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException(
                "DceProject.Recovery.UnsafePackageName");
        }

        return Path.Combine(_recoveryDirectory, packageFileName);
    }

    private static void WriteMetadataAtomically(
        string metadataPath,
        DceProjectRecoveryMetadata metadata)
    {
        string temporaryPath = metadataPath + $".{Guid.NewGuid():N}.tmp";
        string backupPath = metadataPath + ".bak";
        try
        {
            File.WriteAllText(
                temporaryPath,
                JsonSerializer.Serialize(metadata, JsonOptions) + Environment.NewLine,
                new UTF8Encoding(false));
            _ = ReadMetadata(temporaryPath);
            if (File.Exists(metadataPath))
            {
                File.Replace(
                    temporaryPath,
                    metadataPath,
                    backupPath,
                    ignoreMetadataErrors: true);
                DeleteIfExists(backupPath);
            }
            else
            {
                File.Move(temporaryPath, metadataPath);
            }
        }
        finally
        {
            DeleteIfExists(temporaryPath);
        }
    }

    private static DceProjectRecoveryMetadata ReadMetadata(string path)
    {
        try
        {
            DceProjectRecoveryMetadata? metadata =
                JsonSerializer.Deserialize<DceProjectRecoveryMetadata>(
                    File.ReadAllText(path, Encoding.UTF8),
                    JsonOptions);
            if (metadata is null
                || metadata.SchemaVersion != 1
                || string.IsNullOrWhiteSpace(metadata.ProjectReference)
                || string.IsNullOrWhiteSpace(metadata.PackageFileName)
                || string.IsNullOrWhiteSpace(metadata.PackageSha256))
            {
                throw new InvalidDataException(
                    "DceProject.Recovery.MetadataInvalid");
            }

            return metadata;
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException(
                "DceProject.Recovery.MetadataInvalid",
                ex);
        }
    }

    private static DceProjectRecoveryMetadata? TryReadMetadata(string path)
    {
        try
        {
            return File.Exists(path) ? ReadMetadata(path) : null;
        }
        catch (Exception ex) when (ex is IOException
                                   or UnauthorizedAccessException
                                   or InvalidDataException)
        {
            return null;
        }
    }

    private static SourceFingerprint CaptureSource(string normalizedReference)
    {
        if (!File.Exists(normalizedReference))
        {
            return SourceFingerprint.Missing;
        }

        FileInfo file = new(normalizedReference);
        return new SourceFingerprint(
            true,
            file.LastWriteTimeUtc,
            file.Length,
            HashFile(normalizedReference));
    }

    private static bool SourceMatches(DceProjectRecoveryMetadata metadata)
    {
        if (!metadata.SourceExists || !File.Exists(metadata.ProjectReference))
        {
            return false;
        }

        FileInfo source = new(metadata.ProjectReference);
        return source.LastWriteTimeUtc == metadata.SourceLastWriteTimeUtc.UtcDateTime
               && source.Length == metadata.SourceLength
               && string.Equals(
                   HashFile(metadata.ProjectReference),
                   metadata.SourceSha256,
                   StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeReference(string projectReference)
    {
        if (string.IsNullOrWhiteSpace(projectReference))
        {
            throw new ArgumentException(
                "La référence du projet est obligatoire.",
                nameof(projectReference));
        }

        return Path.GetFullPath(projectReference);
    }

    private static string BuildIdentifier(string normalizedReference)
    {
        string identity = OperatingSystem.IsWindows()
            ? normalizedReference.ToUpperInvariant()
            : normalizedReference;
        return Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(identity)))
            .ToLowerInvariant()[..24];
    }

    private static StringComparison ReferenceComparison() =>
        OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

    private static string HashFile(string path)
    {
        using FileStream stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static void DeleteIfExists(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private sealed record SourceFingerprint(
        bool Exists,
        DateTime LastWriteTimeUtc,
        long Length,
        string Sha256)
    {
        public static SourceFingerprint Missing { get; } =
            new(false, DateTime.MinValue, 0, string.Empty);
    }
}

public enum DceProjectRecoveryStage
{
    AfterRecoveryPackageSaved,
    BeforeRecoveryMetadataCommit
}

public sealed record DceProjectRecoveryMetadata(
    int SchemaVersion,
    string ProjectReference,
    bool SourceExists,
    DateTimeOffset SourceLastWriteTimeUtc,
    long SourceLength,
    string SourceSha256,
    DateTimeOffset SavedAtUtc,
    string PackageFileName,
    string PackageSha256);

public sealed record DceProjectRecoveryCandidate(
    string ProjectReference,
    string RecoveryPackagePath,
    DateTimeOffset SavedAtUtc,
    bool SourceMatches,
    DceProjectPackage Package);
