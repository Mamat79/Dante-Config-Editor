using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using DanteConfigEditor.Infrastructure.Migration;
using DanteConfigEditor.Models;
using DanteConfigEditor.Services;

namespace DanteConfigEditorV3.Tests;

public sealed class MachineBank2026Tests
{
    [Fact]
    public void IncludedBankDiscoveryReturnsOnlyValidBankFolders()
    {
        using BankWorkspace workspace = new();
        string alpha = workspace.Subdirectory("Alpha");
        string ignored = workspace.Subdirectory("Ignored");
        string zulu = workspace.Subdirectory("Zulu");
        File.WriteAllText(Path.Combine(alpha, "bank.json"), "{}");
        File.WriteAllText(Path.Combine(zulu, "bank.json"), "{}");
        File.WriteAllText(Path.Combine(ignored, "readme.txt"), "not a bank");

        IReadOnlyList<string> discovered =
            MachineBankDistributionService.DiscoverIncludedBankPaths(workspace.Path);

        Assert.Equal([alpha, zulu], discovered);
    }

    [Fact]
    public void CommunityBank2026KeepsAllGeneratedDeviceTemplates()
    {
        string bankPath = RepositoryFile(
            "Resources",
            "MachineBanks",
            "Bundled",
            "DCE Community Devices 2026.1");

        MachineBankRepository repository = new(bankPath);

        Assert.Equal(41, repository.List().Count);
    }

    [Fact]
    public void NewBankUsesFormat2AndDetectsImageCorruption()
    {
        using BankWorkspace workspace = new();
        string imagePath = workspace.File("front.png");
        File.WriteAllBytes(
            imagePath,
            [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]);
        DanteProject project = OpenFixture();
        MachineBankRepository repository = new(workspace.BankPath);

        MachineTemplateMetadata saved = repository.Save(
            MachineTemplateService.CreateFromDevice(
                project.FindDevice("DEVICE-A")!,
                project.PresetVersion,
                new MachineTemplateCreateRequest
                {
                    TemplateName = "Format 2 image",
                    ImageSourcePath = imagePath
                }));

        Assert.Equal(2, repository.GetFormatVersion());
        Assert.Equal(2, saved.FormatVersion);
        Assert.False(string.IsNullOrWhiteSpace(saved.ImageSha256));
        string storedImage = Path.Combine(
            workspace.BankPath,
            "machines",
            saved.TemplateId.ToString("D"),
            saved.ImageFileName!);
        File.AppendAllText(storedImage, "corruption");

        MachineBankCorruptionException error =
            Assert.Throws<MachineBankCorruptionException>(() =>
                repository.Load(saved.TemplateId));
        Assert.Contains("SHA-256", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void LegacyBundledBankRemainsReadableWithoutModification()
    {
        string bankPath = RepositoryFile(
            "Resources",
            "MachineBanks",
            "Bundled",
            "DCE Generic Roles 3.6");
        Dictionary<string, string> before = HashTree(bankPath);
        MachineBankRepository repository = new(bankPath);

        MachineTemplatePackage[] templates = repository.List()
            .Select(metadata => repository.Load(metadata.TemplateId))
            .ToArray();

        Assert.Equal(1, repository.GetFormatVersion());
        Assert.Equal(2, templates.Length);
        Assert.All(templates, template => Assert.Equal(1, template.Metadata.FormatVersion));
        Assert.Equal(before, HashTree(bankPath));
    }

    [Fact]
    public void MigrationCreatesIndependentFormat2CopyAndPreservesUnknownJson()
    {
        using BankWorkspace workspace = new();
        string source = workspace.Subdirectory("LegacyBank");
        CopyDirectory(
            RepositoryFile(
                "Resources",
                "MachineBanks",
                "Bundled",
                "DCE Community Devices 3.6"),
            source);
        AddFutureField(Path.Combine(source, "bank.json"), "futureBankField", "keep-bank");
        string machineJson = Directory.EnumerateFiles(
            Path.Combine(source, "machines"),
            "machine.json",
            SearchOption.AllDirectories).First();
        AddFutureField(machineJson, "futureTemplateField", "keep-template");
        Dictionary<string, string> before = HashTree(source);
        string destination = Path.Combine(workspace.Path, "MigratedBank");

        MachineBankV2MigrationResult result =
            new MachineBankV2MigrationService().Migrate(source, destination);

        Assert.Equal(before, HashTree(source));
        Assert.True(File.Exists(result.BackupArchivePath));
        Assert.Equal(9, result.Report.TemplateCount);
        MachineBankRepository migrated = new(destination);
        Assert.Equal(2, migrated.GetFormatVersion());
        MachineTemplateMetadata[] metadata = migrated.List().ToArray();
        Assert.Equal(9, metadata.Length);
        Assert.All(metadata, item =>
        {
            Assert.Equal(2, item.FormatVersion);
            MachineTemplatePackage package = migrated.Load(item.TemplateId);
            if (package.ImagePath is not null)
            {
                Assert.False(string.IsNullOrWhiteSpace(item.ImageSha256));
            }
        });

        using JsonDocument bankManifest = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(destination, "bank.json")));
        Assert.Equal(
            "keep-bank",
            bankManifest.RootElement.GetProperty("futureBankField").GetString());
        string migratedMachineJson = Path.Combine(
            destination,
            "machines",
            Path.GetFileName(Path.GetDirectoryName(machineJson))!,
            "machine.json");
        using JsonDocument machineMetadata = JsonDocument.Parse(
            File.ReadAllText(migratedMachineJson));
        Assert.Equal(
            "keep-template",
            machineMetadata.RootElement
                .GetProperty("futureTemplateField")
                .GetString());
    }

    [Fact]
    public void ImportingLegacyTemplateIntoFormat2BankUpgradesItsMetadata()
    {
        using BankWorkspace workspace = new();
        MachineBankRepository legacy = new(RepositoryFile(
            "Resources",
            "MachineBanks",
            "Bundled",
            "DCE Community Devices 3.6"));
        MachineTemplateMetadata source = legacy.List().First();
        string archive = workspace.File("legacy-template.zip");
        legacy.Export(source.TemplateId, archive);
        MachineBankRepository target = new(workspace.BankPath);

        MachineTemplateMetadata imported = target.Import(archive);

        Assert.Equal(2, target.GetFormatVersion());
        Assert.Equal(2, imported.FormatVersion);
        Assert.False(string.IsNullOrWhiteSpace(imported.ImageSha256));
        Assert.NotNull(target.Load(imported.TemplateId).ImagePath);
    }

    [Fact]
    public void TemplateImportRejectsExcessiveEntryCount()
    {
        using BankWorkspace workspace = new();
        string archivePath = workspace.File("too-many.zip");
        using (ZipArchive archive = ZipFile.Open(archivePath, ZipArchiveMode.Create))
        {
            for (int index = 0; index < 17; index++)
            {
                archive.CreateEntry($"entry-{index}.txt");
            }
        }

        MachineBankCorruptionException error =
            Assert.Throws<MachineBankCorruptionException>(() =>
                new MachineBankRepository(workspace.BankPath).Import(archivePath));

        Assert.Contains("trop d'entrées", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static DanteProject OpenFixture() =>
        DanteProject.Load(Path.Combine(
            AppContext.BaseDirectory,
            "Fixtures",
            "representative-preset.xml"));

    private static void AddFutureField(
        string path,
        string propertyName,
        string value)
    {
        JsonObject root = JsonNode.Parse(File.ReadAllText(path, Encoding.UTF8))!
            .AsObject();
        root[propertyName] = value;
        File.WriteAllText(
            path,
            root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }),
            new UTF8Encoding(false));
    }

    private static void CopyDirectory(string source, string destination)
    {
        foreach (string directory in Directory.EnumerateDirectories(
                     source,
                     "*",
                     SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(Path.Combine(
                destination,
                Path.GetRelativePath(source, directory)));
        }
        Directory.CreateDirectory(destination);
        foreach (string file in Directory.EnumerateFiles(
                     source,
                     "*",
                     SearchOption.AllDirectories))
        {
            string target = Path.Combine(
                destination,
                Path.GetRelativePath(source, file));
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(file, target);
        }
    }

    private static Dictionary<string, string> HashTree(string root) =>
        Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                path => Path.GetRelativePath(root, path),
                HashFile,
                StringComparer.OrdinalIgnoreCase);

    private static string HashFile(string path)
    {
        using FileStream stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static string RepositoryFile(params string[] relativeParts) =>
        Path.Combine([RepositoryDirectory(), .. relativeParts]);

    private static string RepositoryDirectory()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null
               && !File.Exists(Path.Combine(
                   directory.FullName,
                   "DanteConfigEditorV3.csproj")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);
        return directory!.FullName;
    }

    private sealed class BankWorkspace : IDisposable
    {
        public BankWorkspace()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"MachineBank2026Tests-{Guid.NewGuid():N}");
            BankPath = System.IO.Path.Combine(Path, "Bank");
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public string BankPath { get; }

        public string File(string name) => System.IO.Path.Combine(Path, name);

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
