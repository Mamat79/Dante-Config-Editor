using DanteConfigEditor.Domain.Validation;
using DanteConfigEditor.Domain.Workspace;
using DanteConfigEditor.Infrastructure.Projects;
using DanteConfigEditor.Infrastructure.Recovery;
using DanteConfigEditor.Models;

namespace DanteConfigEditorV3.Tests;

public sealed class DceProjectRecoveryTests
{
    [Fact]
    public async Task RecoveryRoundTripValidatesPackageAndSourceFingerprint()
    {
        using RecoveryDirectory directory = new();
        string source = directory.File("project.dceproj");
        File.WriteAllText(source, "source-v1");
        DceProjectRecoveryService service = new(
            recoveryDirectory: directory.RecoveryPath);

        DceProjectRecoveryCandidate saved = await service.SaveAsync(
            CreateRequest(OpenFixture(), "Recovery test"),
            source);
        DceProjectRecoveryCandidate found = service.Find(source)!;

        Assert.True(saved.SourceMatches);
        Assert.True(found.SourceMatches);
        Assert.Equal(saved.RecoveryPackagePath, found.RecoveryPackagePath);
        Assert.NotNull(found.Package.OpenedXml.Project.FindDevice("DEVICE-A"));
        Assert.Single(Directory.EnumerateFiles(
            directory.RecoveryPath,
            "*.recovery.dceproj"));

        File.WriteAllText(source, "source-v2-different");
        Assert.False(service.Find(source)!.SourceMatches);
    }

    [Fact]
    public async Task FailedReplacementKeepsPreviousRecoveryIntact()
    {
        using RecoveryDirectory directory = new();
        string source = directory.File("project.dceproj");
        File.WriteAllText(source, "source");
        DceProjectRecoveryService service = new(
            recoveryDirectory: directory.RecoveryPath);
        DanteProject project = OpenFixture();
        DceProjectRecoveryCandidate first = await service.SaveAsync(
            CreateRequest(project, "First"),
            source);
        string firstHash = HashFile(first.RecoveryPackagePath);
        project.RenameDevice("DEVICE-A", "DEVICE-A-NEW");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SaveAsync(
                CreateRequest(project, "Second"),
                source,
                stageObserver: stage =>
                {
                    if (stage == DceProjectRecoveryStage.BeforeRecoveryMetadataCommit)
                    {
                        throw new InvalidOperationException("Simulated metadata failure");
                    }
                }));

        DceProjectRecoveryCandidate restored = service.Find(source)!;
        Assert.Equal(first.RecoveryPackagePath, restored.RecoveryPackagePath);
        Assert.Equal(firstHash, HashFile(restored.RecoveryPackagePath));
        Assert.NotNull(restored.Package.OpenedXml.Project.FindDevice("DEVICE-A"));
        Assert.Null(restored.Package.OpenedXml.Project.FindDevice("DEVICE-A-NEW"));
        Assert.Single(Directory.EnumerateFiles(
            directory.RecoveryPath,
            "*.recovery.dceproj"));
    }

    [Fact]
    public async Task DeleteRemovesMetadataAndRecoveryPackage()
    {
        using RecoveryDirectory directory = new();
        string source = directory.File("project.dceproj");
        File.WriteAllText(source, "source");
        DceProjectRecoveryService service = new(
            recoveryDirectory: directory.RecoveryPath);
        await service.SaveAsync(CreateRequest(OpenFixture(), "Delete"), source);

        await service.DeleteAsync(source);

        Assert.Null(service.Find(source));
        Assert.Empty(Directory.EnumerateFiles(directory.RecoveryPath));
    }

    [Fact]
    public async Task CorruptedRecoveryPackageIsReported()
    {
        using RecoveryDirectory directory = new();
        string source = directory.File("project.dceproj");
        File.WriteAllText(source, "source");
        DceProjectRecoveryService service = new(
            recoveryDirectory: directory.RecoveryPath);
        DceProjectRecoveryCandidate candidate = await service.SaveAsync(
            CreateRequest(OpenFixture(), "Corrupt"),
            source);
        await File.AppendAllTextAsync(candidate.RecoveryPackagePath, "corruption");

        InvalidDataException error = Assert.Throws<InvalidDataException>(() =>
            service.Find(source));

        Assert.Equal("DceProject.Recovery.PackageHashMismatch", error.Message);
    }

    [Fact]
    public void CleanupIgnoresRecentMalformedMetadataAndRemovesOldArtifacts()
    {
        using RecoveryDirectory directory = new();
        Directory.CreateDirectory(directory.RecoveryPath);
        string malformed = Path.Combine(directory.RecoveryPath, "x.recovery.json");
        string orphan = Path.Combine(
            directory.RecoveryPath,
            "0123456789abcdef01234567.old.recovery.dceproj");
        File.WriteAllText(malformed, "{");
        File.WriteAllText(orphan, "orphan");
        File.SetLastWriteTimeUtc(malformed, DateTime.UtcNow.AddDays(-10));
        File.SetLastWriteTimeUtc(orphan, DateTime.UtcNow.AddDays(-10));

        new DceProjectRecoveryService(recoveryDirectory: directory.RecoveryPath)
            .CleanupOld(TimeSpan.FromDays(1));

        Assert.False(File.Exists(malformed));
        Assert.False(File.Exists(orphan));
    }

    private static DceProjectWriteRequest CreateRequest(
        DanteProject project,
        string name)
    {
        DateTimeOffset now = new(2026, 7, 27, 12, 0, 0, TimeSpan.Zero);
        ProjectWorkspaceData workspace = new(
            new ProjectMetadata(name, string.Empty, now, now, "2026.1-test"),
            new ProjectViewSettings(
                "overview",
                false,
                false,
                320,
                new Dictionary<string, string>(),
                new Dictionary<string, double>()),
            [],
            [],
            []);
        return new DceProjectWriteRequest(
            project,
            workspace,
            [],
            ProjectValidationState.Empty,
            "2026.1-test");
    }

    private static DanteProject OpenFixture() =>
        DanteProject.Load(Path.Combine(
            AppContext.BaseDirectory,
            "Fixtures",
            "representative-preset.xml"));

    private static string HashFile(string path)
    {
        using FileStream stream = File.OpenRead(path);
        return Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(stream))
            .ToLowerInvariant();
    }

    private sealed class RecoveryDirectory : IDisposable
    {
        public RecoveryDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"DceProjectRecoveryTests-{Guid.NewGuid():N}");
            RecoveryPath = System.IO.Path.Combine(Path, "Recovery");
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public string RecoveryPath { get; }

        public string File(string name) => System.IO.Path.Combine(Path, name);

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
