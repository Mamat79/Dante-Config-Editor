using System.IO.Compression;
using System.Security.Cryptography;
using DanteConfigEditor.Infrastructure.Migration;

namespace DanteConfigEditorV3.Tests;

public sealed class V36SettingsMigrationTests
{
    [Fact]
    public void MigrationCopiesKnownDataBacksUpSourceAndPreservesExistingDestination()
    {
        using MigrationDirectory directory = new();
        string source = directory.Subdirectory("v36");
        string destination = directory.Subdirectory("2026.1");
        Write(source, "language.txt", "en");
        Write(source, "configuration-editors.txt", "expanded");
        Write(source, "machine-bank-location.txt", @"D:\Shared\DCE Bank");
        Write(source, Path.Combine("Recovery", "recovery.xml"), "<preset />");
        Write(source, Path.Combine("Recovery", "recovery.json"), "{}");
        Write(
            source,
            Path.Combine("Synoptics", "layout.synoptic.json"),
            "{\"schemaVersion\":2}");
        Write(source, "ignored-private-file.txt", "must not migrate");
        Write(destination, "language.txt", "fr");
        Dictionary<string, string> sourceHashes = HashTree(source);

        V36MigrationResult result =
            new V36SettingsMigrationService(source, destination).Migrate();

        Assert.False(result.WasAlreadyComplete);
        Assert.True(result.Report.Completed);
        Assert.Empty(result.Report.Errors);
        Assert.True(File.Exists(result.Report.BackupArchivePath));
        Assert.Equal("fr", File.ReadAllText(Path.Combine(destination, "language.txt")));
        Assert.Equal(
            @"D:\Shared\DCE Bank",
            File.ReadAllText(Path.Combine(destination, "machine-bank-location.txt")));
        Assert.True(File.Exists(Path.Combine(destination, "Recovery", "recovery.xml")));
        Assert.True(File.Exists(
            Path.Combine(destination, "Synoptics", "layout.synoptic.json")));
        Assert.False(File.Exists(
            Path.Combine(destination, "ignored-private-file.txt")));
        Assert.Equal(sourceHashes, HashTree(source));
        Assert.Contains(
            result.Report.Files,
            file => file.RelativePath == "language.txt"
                    && file.Status == V36MigrationFileStatus.ExistingDestinationPreserved);
        Assert.Contains(
            result.Report.Files,
            file => file.RelativePath == "machine-bank-location.txt"
                    && file.Status == V36MigrationFileStatus.Copied);

        using ZipArchive backup = ZipFile.OpenRead(result.Report.BackupArchivePath);
        Assert.NotNull(backup.GetEntry("language.txt"));
        Assert.NotNull(backup.GetEntry("Recovery/recovery.xml"));
        Assert.NotNull(backup.GetEntry("Synoptics/layout.synoptic.json"));
        Assert.Null(backup.GetEntry("ignored-private-file.txt"));
    }

    [Fact]
    public void CompletedMigrationIsIdempotent()
    {
        using MigrationDirectory directory = new();
        string source = directory.Subdirectory("v36");
        string destination = directory.Subdirectory("2026.1");
        Write(source, "language.txt", "en");
        V36SettingsMigrationService service = new(source, destination);

        V36MigrationResult first = service.Migrate();
        V36MigrationResult second = service.Migrate();

        Assert.False(first.WasAlreadyComplete);
        Assert.True(second.WasAlreadyComplete);
        Assert.Equivalent(first.Report, second.Report, strict: true);
        Assert.Single(Directory.EnumerateFiles(
            Path.Combine(destination, "MigrationBackups"),
            "*.zip"));
    }

    [Fact]
    public void MissingLegacyProfileProducesACompletedEmptyReport()
    {
        using MigrationDirectory directory = new();
        string source = Path.Combine(directory.Path, "missing");
        string destination = Path.Combine(directory.Path, "2026.1");

        V36MigrationResult result =
            new V36SettingsMigrationService(source, destination).Migrate();

        Assert.True(result.Report.Completed);
        Assert.Empty(result.Report.Files);
        Assert.Empty(result.Report.Errors);
        Assert.Equal(string.Empty, result.Report.BackupArchivePath);
        Assert.True(File.Exists(
            Path.Combine(destination, "migration-v3.6-to-2026.1.json")));
    }

    private static void Write(string root, string relativePath, string content)
    {
        string path = Path.Combine(root, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
    }

    private static Dictionary<string, string> HashTree(string root) =>
        Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .ToDictionary(
                path => Path.GetRelativePath(root, path),
                HashFile,
                StringComparer.OrdinalIgnoreCase);

    private static string HashFile(string path)
    {
        using FileStream stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private sealed class MigrationDirectory : IDisposable
    {
        public MigrationDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"V36SettingsMigrationTests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public string Subdirectory(string name)
        {
            string path = System.IO.Path.Combine(Path, name);
            Directory.CreateDirectory(path);
            return path;
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
