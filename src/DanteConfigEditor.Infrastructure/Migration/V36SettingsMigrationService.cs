using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DanteConfigEditor.Services;

namespace DanteConfigEditor.Infrastructure.Migration;

public sealed class V36SettingsMigrationService
{
    private static readonly string[] PreferenceFiles =
    [
        "language.txt",
        "configuration-editors.txt",
        "recent-files.txt",
        "machine-bank-location.txt",
        "support-reminder.json"
    ];

    private static readonly string[] DataDirectories =
    [
        "Recovery",
        "Synoptics"
    ];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private readonly string _sourceRoot;
    private readonly string _destinationRoot;

    public V36SettingsMigrationService(string sourceRoot, string destinationRoot)
    {
        _sourceRoot = Path.GetFullPath(sourceRoot);
        _destinationRoot = Path.GetFullPath(destinationRoot);
        if (string.Equals(_sourceRoot, _destinationRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "La source V3.6 et la destination 2026.1 doivent être différentes.");
        }
    }

    public static V36SettingsMigrationService CreateDefault() =>
        new(ApplicationStoragePaths.LegacyV36RootPath, ApplicationStoragePaths.RootPath);

    public V36MigrationResult Migrate()
    {
        string reportPath = Path.Combine(
            _destinationRoot,
            "migration-v3.6-to-2026.1.json");
        V36MigrationReport? existing = TryReadReport(reportPath);
        if (existing?.Completed == true)
        {
            return new V36MigrationResult(true, existing);
        }

        Directory.CreateDirectory(_destinationRoot);
        DateTimeOffset startedAt = DateTimeOffset.UtcNow;
        List<string> errors = [];
        List<V36MigrationFile> files = [];
        string backupPath = string.Empty;
        IReadOnlyList<SourceFile> sourceFiles = EnumerateSourceFiles();

        if (sourceFiles.Count > 0)
        {
            backupPath = CreateBackup(sourceFiles);
        }

        foreach (SourceFile source in sourceFiles)
        {
            try
            {
                string destinationPath = ResolveDestination(source.RelativePath);
                if (File.Exists(destinationPath))
                {
                    files.Add(new V36MigrationFile(
                        source.RelativePath,
                        source.Length,
                        source.Sha256,
                        V36MigrationFileStatus.ExistingDestinationPreserved));
                    continue;
                }

                CopyAndVerify(source, destinationPath);
                files.Add(new V36MigrationFile(
                    source.RelativePath,
                    source.Length,
                    source.Sha256,
                    V36MigrationFileStatus.Copied));
            }
            catch (Exception ex) when (ex is IOException
                                       or UnauthorizedAccessException
                                       or InvalidDataException)
            {
                errors.Add($"{source.RelativePath}: {ex.Message}");
                files.Add(new V36MigrationFile(
                    source.RelativePath,
                    source.Length,
                    source.Sha256,
                    V36MigrationFileStatus.Failed));
            }
        }

        V36MigrationReport report = new(
            SchemaVersion: 1,
            SourceRoot: _sourceRoot,
            DestinationRoot: _destinationRoot,
            StartedAtUtc: startedAt,
            CompletedAtUtc: DateTimeOffset.UtcNow,
            Completed: errors.Count == 0,
            BackupArchivePath: backupPath,
            Files: files,
            Errors: errors);
        WriteReportAtomically(reportPath, report);
        return new V36MigrationResult(false, report);
    }

    private IReadOnlyList<SourceFile> EnumerateSourceFiles()
    {
        if (!Directory.Exists(_sourceRoot))
        {
            return [];
        }

        Dictionary<string, SourceFile> files = new(StringComparer.OrdinalIgnoreCase);
        foreach (string relativePath in PreferenceFiles)
        {
            AddFileIfSafe(files, Path.Combine(_sourceRoot, relativePath));
        }

        foreach (string relativeDirectory in DataDirectories)
        {
            string root = Path.Combine(_sourceRoot, relativeDirectory);
            if (!Directory.Exists(root))
            {
                continue;
            }

            Stack<DirectoryInfo> pending = new();
            pending.Push(new DirectoryInfo(root));
            while (pending.Count > 0)
            {
                DirectoryInfo directory = pending.Pop();
                foreach (FileInfo file in directory.EnumerateFiles())
                {
                    if (!file.Attributes.HasFlag(FileAttributes.ReparsePoint))
                    {
                        AddFileIfSafe(files, file.FullName);
                    }
                }

                foreach (DirectoryInfo child in directory.EnumerateDirectories())
                {
                    if (!child.Attributes.HasFlag(FileAttributes.ReparsePoint))
                    {
                        pending.Push(child);
                    }
                }
            }
        }

        return files.Values
            .OrderBy(file => file.RelativePath, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private void AddFileIfSafe(IDictionary<string, SourceFile> files, string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        string fullPath = Path.GetFullPath(path);
        string relativePath = Path.GetRelativePath(_sourceRoot, fullPath);
        if (IsOutsideRoot(relativePath))
        {
            throw new InvalidDataException($"Chemin V3.6 hors profil : {fullPath}");
        }

        FileInfo info = new(fullPath);
        files[relativePath] = new SourceFile(
            fullPath,
            relativePath,
            info.Length,
            HashFile(fullPath));
    }

    private string CreateBackup(IReadOnlyList<SourceFile> files)
    {
        string backupDirectory = Path.Combine(_destinationRoot, "MigrationBackups");
        Directory.CreateDirectory(backupDirectory);
        string backupPath = Path.Combine(
            backupDirectory,
            $"V36_Settings_{DateTime.UtcNow:yyyyMMdd_HHmmss_fff}_{Guid.NewGuid():N}.zip");
        string temporaryPath = backupPath + ".tmp";
        try
        {
            using (FileStream stream = new(
                       temporaryPath,
                       FileMode.CreateNew,
                       FileAccess.ReadWrite,
                       FileShare.None))
            using (ZipArchive archive = new(stream, ZipArchiveMode.Create))
            {
                foreach (SourceFile file in files)
                {
                    string entryName = file.RelativePath.Replace('\\', '/');
                    ZipArchiveEntry entry = archive.CreateEntry(
                        entryName,
                        CompressionLevel.Optimal);
                    using Stream input = File.OpenRead(file.SourcePath);
                    using Stream output = entry.Open();
                    input.CopyTo(output);
                }
            }

            VerifyBackup(temporaryPath, files);
            File.Move(temporaryPath, backupPath);
            return backupPath;
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    private static void VerifyBackup(string archivePath, IReadOnlyList<SourceFile> files)
    {
        using ZipArchive archive = ZipFile.OpenRead(archivePath);
        foreach (SourceFile file in files)
        {
            string entryName = file.RelativePath.Replace('\\', '/');
            ZipArchiveEntry entry = archive.GetEntry(entryName)
                ?? throw new InvalidDataException(
                    $"Sauvegarde V3.6 incomplète : {entryName}");
            using Stream stream = entry.Open();
            if (!string.Equals(
                    HashStream(stream),
                    file.Sha256,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    $"Empreinte invalide dans la sauvegarde V3.6 : {entryName}");
            }
        }
    }

    private static void CopyAndVerify(SourceFile source, string destinationPath)
    {
        string directory = Path.GetDirectoryName(destinationPath)
            ?? throw new InvalidDataException("Dossier de migration introuvable.");
        Directory.CreateDirectory(directory);
        string temporaryPath = Path.Combine(
            directory,
            $".{Path.GetFileName(destinationPath)}.{Guid.NewGuid():N}.tmp");
        try
        {
            File.Copy(source.SourcePath, temporaryPath, overwrite: false);
            if (!string.Equals(
                    HashFile(temporaryPath),
                    source.Sha256,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    $"La copie de {source.RelativePath} ne correspond pas à la source.");
            }

            File.Move(temporaryPath, destinationPath);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    private string ResolveDestination(string relativePath)
    {
        if (IsOutsideRoot(relativePath))
        {
            throw new InvalidDataException(
                $"Chemin de migration non sûr : {relativePath}");
        }

        return Path.Combine(_destinationRoot, relativePath);
    }

    private static bool IsOutsideRoot(string relativePath) =>
        Path.IsPathRooted(relativePath)
        || string.Equals(relativePath, "..", StringComparison.Ordinal)
        || relativePath.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
        || relativePath.StartsWith($"..{Path.AltDirectorySeparatorChar}", StringComparison.Ordinal);

    private static V36MigrationReport? TryReadReport(string path)
    {
        try
        {
            return File.Exists(path)
                ? JsonSerializer.Deserialize<V36MigrationReport>(
                    File.ReadAllText(path, Encoding.UTF8),
                    JsonOptions)
                : null;
        }
        catch (Exception ex) when (ex is IOException or JsonException)
        {
            return null;
        }
    }

    private static void WriteReportAtomically(string path, V36MigrationReport report)
    {
        string directory = Path.GetDirectoryName(path)
            ?? throw new InvalidDataException("Dossier du rapport de migration introuvable.");
        Directory.CreateDirectory(directory);
        string temporaryPath = path + $".{Guid.NewGuid():N}.tmp";
        string backupPath = path + ".bak";
        try
        {
            File.WriteAllText(
                temporaryPath,
                JsonSerializer.Serialize(report, JsonOptions) + Environment.NewLine,
                new UTF8Encoding(false));
            _ = JsonSerializer.Deserialize<V36MigrationReport>(
                File.ReadAllText(temporaryPath, Encoding.UTF8),
                JsonOptions)
                ?? throw new InvalidDataException(
                    "Le rapport de migration temporaire est vide.");

            if (File.Exists(path))
            {
                File.Replace(temporaryPath, path, backupPath, ignoreMetadataErrors: true);
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

    private static string HashFile(string path)
    {
        using FileStream stream = File.OpenRead(path);
        return HashStream(stream);
    }

    private static string HashStream(Stream stream) =>
        Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();

    private sealed record SourceFile(
        string SourcePath,
        string RelativePath,
        long Length,
        string Sha256);
}

public enum V36MigrationFileStatus
{
    Copied,
    ExistingDestinationPreserved,
    Failed
}

public sealed record V36MigrationFile(
    string RelativePath,
    long Length,
    string Sha256,
    V36MigrationFileStatus Status);

public sealed record V36MigrationReport(
    int SchemaVersion,
    string SourceRoot,
    string DestinationRoot,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset CompletedAtUtc,
    bool Completed,
    string BackupArchivePath,
    IReadOnlyList<V36MigrationFile> Files,
    IReadOnlyList<string> Errors);

public sealed record V36MigrationResult(
    bool WasAlreadyComplete,
    V36MigrationReport Report);
