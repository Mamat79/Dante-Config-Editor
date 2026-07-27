using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using System.Xml.Linq;
using DanteConfigEditor.Domain.History;
using DanteConfigEditor.Domain.Projects;
using DanteConfigEditor.Domain.Validation;
using DanteConfigEditor.Domain.Workspace;
using DanteConfigEditor.Infrastructure.Projects;
using DanteConfigEditor.Models;

namespace DanteConfigEditorV3.Tests;

public sealed class DceProjectPackageTests
{
    [Fact]
    public void PackageRoundTripPreservesDanteXmlWorkspaceHistoryAndValidation()
    {
        using TestDirectory directory = new();
        DanteProject project = OpenFixture();
        DceProjectWriteRequest request = CreateRequest(project);
        string destination = directory.File("roundtrip.dceproj");
        DceProjectPackageService service = new();

        DceProjectSaveResult saved = service.Save(request, destination);
        DceProjectPackage reopened = service.Open(destination);

        Assert.Equal(destination, saved.DestinationPath);
        Assert.Equal(64, saved.PackageSha256.Length);
        Assert.True(XNode.DeepEquals(project.Document, reopened.OpenedXml.Project.Document));
        Assert.Equal(
            project.Devices.Select(device => device.Name),
            reopened.OpenedXml.Project.Devices.Select(device => device.Name));
        Assert.Equivalent(request.Workspace, reopened.Workspace, strict: true);
        Assert.Equivalent(request.History, reopened.History, strict: true);
        Assert.Equivalent(request.Validation, reopened.Validation, strict: true);
        Assert.Equal(request.Assets!["images/overview.png"], reopened.Assets["images/overview.png"]);
        Assert.Equal("recognized-complete", reopened.OpenedXml.Profile.Id);
    }

    [Fact]
    public void FailureAfterTemporaryCreationLeavesExistingDestinationUntouched()
    {
        using TestDirectory directory = new();
        DceProjectPackageService service = new();
        DanteProject project = OpenFixture();
        string destination = directory.File("atomic.dceproj");
        service.Save(CreateRequest(project), destination);
        string originalHash = HashFile(destination);

        project.RenameDevice("DEVICE-A", "DEVICE-A-NEW");
        InvalidOperationException error = Assert.Throws<InvalidOperationException>(() =>
            service.Save(
                CreateRequest(project, projectName: "Changed"),
                destination,
                stage =>
                {
                    if (stage == DceProjectSaveStage.AfterTemporaryPackageCreated)
                    {
                        throw new InvalidOperationException("Simulated failure");
                    }
                }));

        Assert.Equal("Simulated failure", error.Message);
        Assert.Equal(originalHash, HashFile(destination));
        Assert.NotNull(service.Open(destination).OpenedXml.Project.FindDevice("DEVICE-A"));
        Assert.DoesNotContain(
            Directory.EnumerateFiles(directory.Path),
            path => path.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void SuccessfulReplacementCreatesBackupOfPreviousDestination()
    {
        using TestDirectory directory = new();
        DceProjectPackageService service = new();
        DanteProject project = OpenFixture();
        string destination = directory.File("replace.dceproj");
        service.Save(CreateRequest(project), destination);
        string originalHash = HashFile(destination);

        project.RenameDevice("DEVICE-A", "DEVICE-A-NEW");
        DceProjectSaveResult result = service.Save(
            CreateRequest(project, projectName: "Replacement"),
            destination);

        Assert.True(File.Exists(result.BackupPath));
        Assert.Equal(originalHash, HashFile(result.BackupPath));
        Assert.NotEqual(originalHash, result.PackageSha256);
        Assert.NotNull(service.Open(destination).OpenedXml.Project.FindDevice("DEVICE-A-NEW"));
    }

    [Fact]
    public void UnknownManifestFieldsSurviveOpenAndResave()
    {
        using TestDirectory directory = new();
        DceProjectPackageService service = new();
        string source = directory.File("future-source.dceproj");
        string destination = directory.File("future-copy.dceproj");
        DceProjectWriteRequest request = CreateRequest(OpenFixture());
        service.Save(request, source);
        ReplaceManifest(source, root =>
        {
            Dictionary<string, object?> values = root
                .EnumerateObject()
                .ToDictionary(
                    property => property.Name,
                    property => (object?)property.Value.Clone(),
                    StringComparer.Ordinal);
            values["futureOption"] = new { enabled = true, mode = "future" };
            return JsonSerializer.SerializeToUtf8Bytes(values, JsonOptions);
        });

        DceProjectPackage opened = service.Open(source);
        service.Save(
            request with { ExistingManifest = opened.Manifest },
            destination);

        using ZipArchive archive = ZipFile.OpenRead(destination);
        using JsonDocument manifest = ReadJsonDocument(
            archive.GetEntry(DceProjectPackageEntries.Manifest)!);
        JsonElement future = manifest.RootElement.GetProperty("futureOption");
        Assert.True(future.GetProperty("enabled").GetBoolean());
        Assert.Equal("future", future.GetProperty("mode").GetString());
    }

    [Fact]
    public void CorruptedContentIsRejectedByChecksum()
    {
        using TestDirectory directory = new();
        DceProjectPackageService service = new();
        string path = directory.File("corrupt.dceproj");
        service.Save(CreateRequest(OpenFixture()), path);

        ReplaceEntry(path, DceProjectPackageEntries.Settings, "{}"u8.ToArray());

        InvalidDataException error = Assert.Throws<InvalidDataException>(() => service.Open(path));
        Assert.Contains("DceProject.Hash.Mismatch", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void MissingRequiredEntryIsRejected()
    {
        using TestDirectory directory = new();
        DceProjectPackageService service = new();
        string path = directory.File("missing.dceproj");
        service.Save(CreateRequest(OpenFixture()), path);
        DeleteEntry(path, DceProjectPackageEntries.History);

        InvalidDataException error = Assert.Throws<InvalidDataException>(() => service.Open(path));
        Assert.Contains("RequiredEntriesMissing", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void UnsafeArchiveEntryIsRejectedWithoutExtraction()
    {
        using TestDirectory directory = new();
        DceProjectPackageService service = new();
        string path = directory.File("traversal.dceproj");
        service.Save(CreateRequest(OpenFixture()), path);
        using (ZipArchive archive = ZipFile.Open(path, ZipArchiveMode.Update))
        {
            using StreamWriter writer = new(archive.CreateEntry("../outside.txt").Open());
            writer.Write("must never be extracted");
        }

        InvalidDataException error = Assert.Throws<InvalidDataException>(() => service.Open(path));
        Assert.Contains("UnsafeEntry", error.Message, StringComparison.Ordinal);
        Assert.False(File.Exists(directory.File("outside.txt")));
    }

    [Fact]
    public void UnsupportedSchemaIsRejected()
    {
        using TestDirectory directory = new();
        DceProjectPackageService service = new();
        string path = directory.File("schema.dceproj");
        service.Save(CreateRequest(OpenFixture()), path);
        ReplaceManifest(path, root =>
        {
            Dictionary<string, object?> values = root
                .EnumerateObject()
                .ToDictionary(
                    property => property.Name,
                    property => (object?)property.Value.Clone(),
                    StringComparer.Ordinal);
            values["schemaVersion"] = "2.0";
            return JsonSerializer.SerializeToUtf8Bytes(values, JsonOptions);
        });

        InvalidDataException error = Assert.Throws<InvalidDataException>(() => service.Open(path));
        Assert.Contains("UnsupportedSchema", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void MalformedArchiveIsRejected()
    {
        using TestDirectory directory = new();
        string path = directory.File("not-a-zip.dceproj");
        File.WriteAllText(path, "not a zip");

        Assert.Throws<InvalidDataException>(() => new DceProjectPackageService().Open(path));
    }

    [Fact]
    public void ConfigurableLimitsRejectOversizedXmlBeforeDestinationIsCreated()
    {
        using TestDirectory directory = new();
        DceProjectPackageService service = new(
            limits: new DceProjectPackageLimits(
                MaximumXmlBytes: 32,
                MaximumJsonBytes: 1024 * 1024));
        string destination = directory.File("oversized.dceproj");

        InvalidDataException error = Assert.Throws<InvalidDataException>(() =>
            service.Save(CreateRequest(OpenFixture()), destination));

        Assert.Contains("EntryTooLarge", error.Message, StringComparison.Ordinal);
        Assert.False(File.Exists(destination));
    }

    [Fact]
    public void ConfigurableLimitsRejectOversizedAsset()
    {
        using TestDirectory directory = new();
        DceProjectPackageService service = new(
            limits: new DceProjectPackageLimits(MaximumAssetBytes: 3));
        DceProjectWriteRequest request = CreateRequest(OpenFixture()) with
        {
            Assets = new Dictionary<string, byte[]>
            {
                ["device.png"] = [1, 2, 3, 4]
            }
        };

        InvalidDataException error = Assert.Throws<InvalidDataException>(() =>
            service.Save(request, directory.File("asset-too-large.dceproj")));

        Assert.Contains("EntryTooLarge:assets/device.png", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ExecutableAssetIsRejected()
    {
        using TestDirectory directory = new();
        DceProjectWriteRequest request = CreateRequest(OpenFixture()) with
        {
            Assets = new Dictionary<string, byte[]>
            {
                ["payload.exe"] = [1, 2, 3]
            }
        };

        InvalidDataException error = Assert.Throws<InvalidDataException>(() =>
            new DceProjectPackageService().Save(
                request,
                directory.File("unsafe-asset.dceproj")));

        Assert.Contains("UnsupportedFormat", error.Message, StringComparison.Ordinal);
    }

    private static DceProjectWriteRequest CreateRequest(
        DanteProject project,
        string projectName = "DCE package test")
    {
        DateTimeOffset created = new(2026, 7, 27, 9, 0, 0, TimeSpan.Zero);
        DanteDevice device = project.Devices[0];
        ProjectEntityReference target = new(
            ProjectEntityKind.Device,
            device.StableIdentity,
            device.Name);
        ProjectWorkspaceData workspace = new(
            new ProjectMetadata(
                projectName,
                "Workspace description",
                created,
                created.AddMinutes(5),
                "2026.1-test",
                new Dictionary<string, string> { ["metadata.future"] = "preserved" }),
            new ProjectViewSettings(
                "patch",
                false,
                true,
                320,
                new Dictionary<string, string> { ["device.search"] = "DEVICE" },
                new Dictionary<string, double> { ["device.name"] = 180 }),
            [
                new SynopticNodeLayout(
                    device.StableIdentity,
                    120,
                    80,
                    false,
                    1,
                    "Stage")
            ],
            ["A project note"],
            [project.Devices[^1].StableIdentity],
            new Dictionary<string, string> { ["workspace.future"] = "preserved" });
        ProjectHistoryEntry history = new(
            Guid.Parse("b2d9fd90-0962-45b4-81f9-08a37387a8c2"),
            created.AddMinutes(2),
            "device.rename",
            "History.DeviceRename",
            "DEVICE-A -> DEVICE-A-NEW",
            1,
            [target],
            []);
        ProjectValidationState validation = new(
            created.AddMinutes(4),
            [
                new ProjectValidationIssue(
                    "TEST-WARNING",
                    ProjectValidationSeverity.Warning,
                    "test",
                    "Validation.TestWarning",
                    "Synthetic warning",
                    target,
                    "/preset/device[1]")
            ]);

        return new DceProjectWriteRequest(
            project,
            workspace,
            [history],
            validation,
            "2026.1-test",
            Assets: new Dictionary<string, byte[]>
            {
                ["images/overview.png"] = [137, 80, 78, 71, 13, 10, 26, 10]
            });
    }

    private static DanteProject OpenFixture() =>
        DanteProject.Load(Path.Combine(
            AppContext.BaseDirectory,
            "Fixtures",
            "representative-preset.xml"));

    private static void ReplaceManifest(
        string path,
        Func<JsonElement, byte[]> transform)
    {
        using ZipArchive archive = ZipFile.Open(path, ZipArchiveMode.Update);
        ZipArchiveEntry entry = archive.GetEntry(DceProjectPackageEntries.Manifest)!;
        byte[] replacement;
        using (JsonDocument manifest = ReadJsonDocument(entry))
        {
            replacement = transform(manifest.RootElement);
        }
        entry.Delete();
        WriteEntry(archive, DceProjectPackageEntries.Manifest, replacement);
    }

    private static void ReplaceEntry(string path, string entryName, byte[] replacement)
    {
        using ZipArchive archive = ZipFile.Open(path, ZipArchiveMode.Update);
        archive.GetEntry(entryName)!.Delete();
        WriteEntry(archive, entryName, replacement);
    }

    private static void DeleteEntry(string path, string entryName)
    {
        using ZipArchive archive = ZipFile.Open(path, ZipArchiveMode.Update);
        archive.GetEntry(entryName)!.Delete();
    }

    private static void WriteEntry(ZipArchive archive, string entryName, byte[] content)
    {
        ZipArchiveEntry entry = archive.CreateEntry(entryName);
        using Stream stream = entry.Open();
        stream.Write(content);
    }

    private static JsonDocument ReadJsonDocument(ZipArchiveEntry entry)
    {
        using Stream stream = entry.Open();
        return JsonDocument.Parse(stream);
    }

    private static string HashFile(string path)
    {
        using FileStream stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static JsonSerializerOptions JsonOptions { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private sealed class TestDirectory : IDisposable
    {
        public TestDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"DceProjectPackageTests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

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
