using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using DanteConfigEditor.Models;
using DanteConfigEditor.Services;

namespace DanteConfigEditorV3.Tests;

public sealed class MachineBankV36Tests
{
    [Fact]
    public void TemplateRoundTripUsesVersionedHumanReadableFilesAndNoHardwareIdentity()
    {
        using TestWorkspace workspace = new();
        DanteProject project = DanteProject.Load(workspace.SourcePath);
        DanteDevice source = Assert.IsType<DanteDevice>(project.FindDevice("DEVICE-A"));
        MachineTemplatePackage package = MachineTemplateService.CreateFromDevice(
            source,
            project.PresetVersion,
            new MachineTemplateCreateRequest
            {
                TemplateName = "Test TX 2x1",
                Manufacturer = "Test Manufacturer",
                Model = "Test TX",
                Description = "Modèle anonymisé pour les tests.",
                Category = "Stagebox",
                Tags = ["audio", "test"]
            });
        MachineBankRepository repository = new(workspace.BankPath);

        MachineTemplateMetadata saved = repository.Save(package);
        MachineTemplatePackage loaded = repository.Load(saved.TemplateId);

        Assert.Equal(MachineTemplateMetadata.CurrentFormatVersion, loaded.Metadata.FormatVersion);
        Assert.Equal("Test TX 2x1", loaded.Metadata.TemplateName);
        Assert.Equal(2, loaded.Metadata.TxCount);
        Assert.Equal(1, loaded.Metadata.RxCount);
        Assert.True(File.Exists(Path.Combine(workspace.BankPath, "bank.json")));
        Assert.True(File.Exists(Path.Combine(
            workspace.BankPath,
            "machines",
            saved.TemplateId.ToString("D"),
            "machine.json")));
        Assert.True(File.Exists(Path.Combine(
            workspace.BankPath,
            "machines",
            saved.TemplateId.ToString("D"),
            "template.xml")));

        XElement templateDevice = Assert.IsType<XElement>(loaded.TemplateDocument.Root);
        Assert.Equal("device", templateDevice.Name.LocalName);
        Assert.Null(Child(templateDevice, "instance_id"));
        Assert.Null(Child(templateDevice, "default_name"));
        Assert.Empty(Children(templateDevice, "interface"));
        Assert.All(
            Children(templateDevice, "rxchannel"),
            channel => Assert.Null(Child(channel, "subscribed_device")));
    }

    [Fact]
    public void CorruptedTemplateHashIsRejectedWithoutModifyingTheBank()
    {
        using TestWorkspace workspace = new();
        DanteProject project = DanteProject.Load(workspace.SourcePath);
        MachineTemplatePackage package = MachineTemplateService.CreateFromDevice(
            project.FindDevice("DEVICE-A")!,
            project.PresetVersion,
            new MachineTemplateCreateRequest
            {
                TemplateName = "Corruption test"
            });
        MachineBankRepository repository = new(workspace.BankPath);
        MachineTemplateMetadata saved = repository.Save(package);
        string templatePath = Path.Combine(
            workspace.BankPath,
            "machines",
            saved.TemplateId.ToString("D"),
            "template.xml");
        string original = File.ReadAllText(templatePath, Encoding.UTF8);
        File.WriteAllText(templatePath, original.Replace("PROGRAM L", "ALTERED", StringComparison.Ordinal), new UTF8Encoding(false));

        MachineBankCorruptionException error = Assert.Throws<MachineBankCorruptionException>(() =>
            repository.Load(saved.TemplateId));

        Assert.Contains("SHA-256", error.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Single(repository.List());
    }

    [Fact]
    public void AddingTemplateToProjectCreatesIndependentRoleAndLeavesTemplateUntouched()
    {
        using TestWorkspace workspace = new();
        DanteProject project = DanteProject.Load(workspace.SourcePath);
        MachineTemplatePackage package = MachineTemplateService.CreateFromDevice(
            project.FindDevice("DEVICE-A")!,
            project.PresetVersion,
            new MachineTemplateCreateRequest
            {
                TemplateName = "Reusable TX"
            });
        string templateBefore = package.TemplateDocument.ToString(SaveOptions.DisableFormatting);

        MachineCloneResult result = project.AddDeviceFromTemplate(
            package,
            new MachineInstanceOptions
            {
                NewName = "BANK-TX-01",
                TxLabelPrefix = "Bank TX",
                RxLabelPrefix = "Bank RX"
            });

        DanteDevice inserted = Assert.IsType<DanteDevice>(project.FindDevice("BANK-TX-01"));
        Assert.True(inserted.IsGenericRole);
        Assert.Equal(["Bank TX 1", "Bank TX 2"], inserted.TxChannels.Select(channel => channel.DisplayName));
        Assert.Equal(["Bank RX 1"], inserted.RxChannels.Select(channel => channel.DisplayName));
        Assert.Equal(templateBefore, package.TemplateDocument.ToString(SaveOptions.DisableFormatting));
        Assert.Equal("BANK-TX-01", result.NewName);
        Assert.False(project.Validate().HasErrors);
        Assert.False(project.ValidateXmlChangeGuard().HasErrors);
    }

    [Fact]
    public void RepositoryCopiesOptionalImageIntoTemplateDirectory()
    {
        using TestWorkspace workspace = new();
        string imagePath = Path.Combine(workspace.DirectoryPath, "front.png");
        File.WriteAllBytes(imagePath, [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]);
        DanteProject project = DanteProject.Load(workspace.SourcePath);
        MachineTemplatePackage package = MachineTemplateService.CreateFromDevice(
            project.FindDevice("DEVICE-A")!,
            project.PresetVersion,
            new MachineTemplateCreateRequest
            {
                TemplateName = "Template with image",
                ImageSourcePath = imagePath
            });
        MachineBankRepository repository = new(workspace.BankPath);

        MachineTemplateMetadata saved = repository.Save(package);
        MachineTemplatePackage loaded = repository.Load(saved.TemplateId);

        Assert.Equal("image.png", loaded.Metadata.ImageFileName);
        Assert.True(File.Exists(Path.Combine(
            workspace.BankPath,
            "machines",
            saved.TemplateId.ToString("D"),
            "image.png")));
    }

    [Fact]
    public void RepositoryExportImportKeepsTemplatePortable()
    {
        using TestWorkspace sourceWorkspace = new();
        DanteProject sourceProject = DanteProject.Load(sourceWorkspace.SourcePath);
        MachineBankRepository sourceRepository = new(sourceWorkspace.BankPath);
        MachineTemplateMetadata saved = sourceRepository.Save(MachineTemplateService.CreateFromDevice(
            sourceProject.FindDevice("DEVICE-A")!,
            sourceProject.PresetVersion,
            new MachineTemplateCreateRequest
            {
                TemplateName = "Portable TX"
            }));
        string archivePath = Path.Combine(sourceWorkspace.DirectoryPath, "portable.dce-machine.zip");
        sourceRepository.Export(saved.TemplateId, archivePath);

        using TestWorkspace targetWorkspace = new();
        MachineBankRepository targetRepository = new(targetWorkspace.BankPath);
        MachineTemplateMetadata imported = targetRepository.Import(archivePath);

        Assert.Equal(saved.TemplateId, imported.TemplateId);
        Assert.Equal("Portable TX", targetRepository.Load(imported.TemplateId).Metadata.TemplateName);
    }

    [Fact]
    public void TemplateFromAnotherPresetVersionIsRejectedBeforeProjectMutation()
    {
        using TestWorkspace workspace = new();
        DanteProject project = DanteProject.Load(workspace.SourcePath);
        MachineTemplatePackage sourcePackage = MachineTemplateService.CreateFromDevice(
            project.FindDevice("DEVICE-A")!,
            "2.1.0",
            new MachineTemplateCreateRequest
            {
                TemplateName = "Old format"
            });

        InvalidOperationException error = Assert.Throws<InvalidOperationException>(() =>
            project.AddDeviceFromTemplate(
                sourcePackage,
                new MachineInstanceOptions
                {
                    NewName = "OLD-FORMAT"
                }));

        Assert.Contains("migration", error.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(3, project.Devices.Count);
    }

    [Fact]
    public void MetadataNamespaceMismatchIsRejectedAsCorruption()
    {
        using TestWorkspace workspace = new();
        DanteProject project = DanteProject.Load(workspace.SourcePath);
        MachineBankRepository repository = new(workspace.BankPath);
        MachineTemplateMetadata saved = repository.Save(MachineTemplateService.CreateFromDevice(
            project.FindDevice("DEVICE-A")!,
            project.PresetVersion,
            new MachineTemplateCreateRequest
            {
                TemplateName = "Namespace check"
            }));
        string metadataPath = Path.Combine(
            workspace.BankPath,
            "machines",
            saved.TemplateId.ToString("D"),
            "machine.json");
        string metadata = File.ReadAllText(metadataPath, Encoding.UTF8);
        File.WriteAllText(
            metadataPath,
            metadata.Replace(
                "\"sourceXmlNamespace\": \"\"",
                "\"sourceXmlNamespace\": \"urn:unexpected\"",
                StringComparison.Ordinal),
            new UTF8Encoding(false));

        MachineBankCorruptionException error = Assert.Throws<MachineBankCorruptionException>(() =>
            repository.Load(saved.TemplateId));

        Assert.Contains("namespace", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void UnsupportedTemplateFormatUsesTheMigrationGateAndLeavesFilesUntouched()
    {
        using TestWorkspace workspace = new();
        DanteProject project = DanteProject.Load(workspace.SourcePath);
        MachineBankRepository repository = new(workspace.BankPath);
        MachineTemplateMetadata saved = repository.Save(MachineTemplateService.CreateFromDevice(
            project.FindDevice("DEVICE-A")!,
            project.PresetVersion,
            new MachineTemplateCreateRequest
            {
                TemplateName = "Migration gate"
            }));
        string metadataPath = Path.Combine(
            workspace.BankPath,
            "machines",
            saved.TemplateId.ToString("D"),
            "machine.json");
        string metadata = File.ReadAllText(metadataPath, Encoding.UTF8);
        string unsupported = metadata.Replace(
            "\"formatVersion\": 1",
            "\"formatVersion\": 0",
            StringComparison.Ordinal);
        File.WriteAllText(metadataPath, unsupported, new UTF8Encoding(false));

        MachineBankCorruptionException error = Assert.Throws<MachineBankCorruptionException>(() =>
            repository.Load(saved.TemplateId));

        Assert.Contains("migration", error.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(unsupported, File.ReadAllText(metadataPath, Encoding.UTF8));
    }

    [Fact]
    public void TemplateUpdateIsTransactionalAndPreservesItsIdentityAndImage()
    {
        using TestWorkspace workspace = new();
        string imagePath = Path.Combine(workspace.DirectoryPath, "front.png");
        File.WriteAllBytes(imagePath, [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]);
        DanteProject project = DanteProject.Load(workspace.SourcePath);
        MachineBankRepository repository = new(workspace.BankPath);
        MachineTemplateMetadata saved = repository.Save(MachineTemplateService.CreateFromDevice(
            project.FindDevice("DEVICE-A")!,
            project.PresetVersion,
            new MachineTemplateCreateRequest
            {
                TemplateName = "Before update",
                Manufacturer = "Maker",
                Model = "One",
                ImageSourcePath = imagePath
            }));
        MachineTemplatePackage loaded = repository.Load(saved.TemplateId);
        MachineTemplatePackage edited = MachineTemplateService.Update(
            loaded,
            new MachineTemplateEditRequest
            {
                TemplateName = "After update",
                Manufacturer = "Maker",
                Model = "Two",
                TxLabels = ["TX 01", "TX 02"],
                RxLabels = ["RX 01"]
            });

        MachineTemplateMetadata updated = repository.Update(edited);
        MachineTemplatePackage reloaded = repository.Load(saved.TemplateId);

        Assert.Equal(saved.TemplateId, updated.TemplateId);
        Assert.Equal("After update", reloaded.Metadata.TemplateName);
        Assert.Equal("Two", reloaded.Metadata.Model);
        Assert.Equal(["TX 01", "TX 02"], ReadLabels(reloaded.TemplateDocument, "txchannel"));
        Assert.Equal(["RX 01"], ReadLabels(reloaded.TemplateDocument, "rxchannel"));
        Assert.Equal("image.png", reloaded.Metadata.ImageFileName);
        Assert.True(File.Exists(reloaded.ImagePath));
        Assert.NotEmpty(Directory.GetDirectories(
            Path.Combine(workspace.BankPath, "Backups", "ReplacedModels")));
    }

    [Fact]
    public void TemplateDuplicationCreatesAnIndependentTemplateId()
    {
        using TestWorkspace workspace = new();
        DanteProject project = DanteProject.Load(workspace.SourcePath);
        MachineBankRepository repository = new(workspace.BankPath);
        MachineTemplateMetadata saved = repository.Save(MachineTemplateService.CreateFromDevice(
            project.FindDevice("DEVICE-A")!,
            project.PresetVersion,
            new MachineTemplateCreateRequest
            {
                TemplateName = "Original template"
            }));
        MachineTemplatePackage duplicate = MachineTemplateService.Duplicate(
            repository.Load(saved.TemplateId),
            new MachineTemplateEditRequest
            {
                TemplateName = "Copied template"
            });

        MachineTemplateMetadata copied = repository.Save(duplicate);

        Assert.NotEqual(saved.TemplateId, copied.TemplateId);
        Assert.Equal(2, repository.List().Count);
        Assert.Equal("Original template", repository.Load(saved.TemplateId).Metadata.TemplateName);
        Assert.Equal("Copied template", repository.Load(copied.TemplateId).Metadata.TemplateName);
    }

    [Fact]
    public void DeleteRemovesTheManifestEntryButKeepsARecoverableCopy()
    {
        using TestWorkspace workspace = new();
        DanteProject project = DanteProject.Load(workspace.SourcePath);
        MachineBankRepository repository = new(workspace.BankPath);
        MachineTemplateMetadata saved = repository.Save(MachineTemplateService.CreateFromDevice(
            project.FindDevice("DEVICE-A")!,
            project.PresetVersion,
            new MachineTemplateCreateRequest
            {
                TemplateName = "Delete safely"
            }));

        repository.Delete(saved.TemplateId);

        Assert.Empty(repository.List());
        string[] backups = Directory.GetDirectories(
            Path.Combine(workspace.BankPath, "Backups", "DeletedModels"));
        string backup = Assert.Single(backups);
        Assert.True(File.Exists(Path.Combine(backup, "machine.json")));
        Assert.True(File.Exists(Path.Combine(backup, "template.xml")));
    }

    [Fact]
    public void WholeBankBackupCanBeRestoredIntoANewDirectory()
    {
        using TestWorkspace workspace = new();
        DanteProject project = DanteProject.Load(workspace.SourcePath);
        MachineBankRepository repository = new(workspace.BankPath);
        MachineTemplateMetadata saved = repository.Save(MachineTemplateService.CreateFromDevice(
            project.FindDevice("DEVICE-A")!,
            project.PresetVersion,
            new MachineTemplateCreateRequest
            {
                TemplateName = "Whole bank"
            }));
        string archivePath = Path.Combine(workspace.DirectoryPath, "bank.dce-bank.zip");
        string restoredPath = Path.Combine(workspace.DirectoryPath, "RestoredBank");

        MachineBankArchiveService.ExportBank(workspace.BankPath, archivePath);
        MachineBankArchiveService.RestoreBank(archivePath, restoredPath);

        MachineBankRepository restored = new(restoredPath);
        Assert.Equal("Whole bank", restored.Load(saved.TemplateId).Metadata.TemplateName);
    }

    [Fact]
    public void WholeBankRestoreNeverOverwritesANonEmptyDirectory()
    {
        using TestWorkspace workspace = new();
        DanteProject project = DanteProject.Load(workspace.SourcePath);
        MachineBankRepository repository = new(workspace.BankPath);
        repository.Save(MachineTemplateService.CreateFromDevice(
            project.FindDevice("DEVICE-A")!,
            project.PresetVersion,
            new MachineTemplateCreateRequest
            {
                TemplateName = "Do not overwrite"
            }));
        string archivePath = Path.Combine(workspace.DirectoryPath, "bank.dce-bank.zip");
        MachineBankArchiveService.ExportBank(workspace.BankPath, archivePath);
        string occupiedPath = Path.Combine(workspace.DirectoryPath, "Occupied");
        Directory.CreateDirectory(occupiedPath);
        string sentinel = Path.Combine(occupiedPath, "keep.txt");
        File.WriteAllText(sentinel, "keep");

        Assert.Throws<IOException>(() =>
            MachineBankArchiveService.RestoreBank(archivePath, occupiedPath));

        Assert.Equal("keep", File.ReadAllText(sentinel));
    }

    [Fact]
    public void BundledGenericBankAndGithubArchiveAreValidAndContainNoProjectIdentity()
    {
        string bundledPath = RepositoryFile(
            "Resources",
            "MachineBanks",
            "Bundled",
            MachineBankDistributionService.BundledBankFolderName);
        MachineBankRepository bundled = new(bundledPath);

        MachineTemplateMetadata[] templates = bundled.List().ToArray();

        Assert.Equal(2, templates.Length);
        Assert.Equal([8, 32], templates.Select(item => item.TxCount).Order().ToArray());
        Assert.Equal([8, 32], templates.Select(item => item.RxCount).Order().ToArray());
        Assert.All(templates, metadata =>
        {
            Assert.Equal("3.0.0", metadata.SourcePresetVersion);
            MachineTemplatePackage package = bundled.Load(metadata.TemplateId);
            XElement root = Assert.IsType<XElement>(package.TemplateDocument.Root);
            Assert.Null(Child(root, "instance_id"));
            Assert.Null(Child(root, "device_id"));
            Assert.Null(Child(root, "default_name"));
            Assert.Empty(Children(root, "interface"));
            Assert.Empty(Children(root, "txflow"));
            Assert.All(Children(root, "rxchannel"), channel =>
            {
                Assert.Null(Child(channel, "subscribed_device"));
                Assert.Null(Child(channel, "subscribed_channel"));
            });
        });

        string archivePath = RepositoryFile(
            "machine-banks",
            "DCE_Generic_Roles_3_6.dce-bank.zip");
        using JsonDocument catalog = JsonDocument.Parse(
            File.ReadAllText(RepositoryFile("machine-banks", "catalog.json")));
        string expectedHash = catalog.RootElement
            .GetProperty("banks")[0]
            .GetProperty("sha256")
            .GetString()!;
        string actualHash = Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(archivePath)))
            .ToLowerInvariant();
        Assert.Equal(expectedHash, actualHash);

        using TestWorkspace workspace = new();
        string restoredPath = Path.Combine(workspace.DirectoryPath, "DownloadedBank");
        MachineBankArchiveService.RestoreBank(archivePath, restoredPath);
        Assert.Equal(2, new MachineBankRepository(restoredPath).List().Count);

        JsonElement communityEntry = catalog.RootElement
            .GetProperty("banks")
            .EnumerateArray()
            .Single(item => item.GetProperty("id").GetString()
                == "yamaha-ql1-fohhn-di4-1000");
        string communityArchivePath = RepositoryFile(
            "machine-banks",
            communityEntry.GetProperty("file").GetString()!);
        string communityHash = Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(
                File.ReadAllBytes(communityArchivePath)))
            .ToLowerInvariant();
        Assert.Equal(communityEntry.GetProperty("sha256").GetString(), communityHash);

        using TestWorkspace communityWorkspace = new();
        string communityPath = Path.Combine(
            communityWorkspace.DirectoryPath,
            "CommunityBank");
        MachineBankArchiveService.RestoreBank(communityArchivePath, communityPath);
        MachineBankRepository communityRepository = new(communityPath);
        MachineTemplateMetadata[] communityTemplates = communityRepository
            .List()
            .OrderBy(item => item.TemplateName, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(["DI4.1000", "QL1"], communityTemplates
            .Select(item => item.TemplateName)
            .ToArray());
        Assert.Equal([(0, 4), (32, 32)], communityTemplates
            .Select(item => (item.TxCount, item.RxCount))
            .ToArray());
        Assert.All(communityTemplates, metadata =>
        {
            Assert.False(string.IsNullOrWhiteSpace(metadata.ImageFileName));
            Assert.True(File.Exists(Path.Combine(
                communityPath,
                "machines",
                metadata.TemplateId.ToString(),
                metadata.ImageFileName)));

            XElement root = Assert.IsType<XElement>(
                communityRepository.Load(metadata.TemplateId).TemplateDocument.Root);
            Assert.Null(Child(root, "instance_id"));
            Assert.Null(Child(root, "device_id"));
            Assert.Null(Child(root, "default_name"));
            Assert.Empty(Children(root, "interface"));
            Assert.Empty(Children(root, "txflow"));
            Assert.All(Children(root, "rxchannel"), channel =>
            {
                Assert.Null(Child(channel, "subscribed_device"));
                Assert.Null(Child(channel, "subscribed_channel"));
            });
        });
    }

    [Fact]
    public void GithubBankCatalogUsesTheVersionedPublicRepositoryFolder()
    {
        Assert.Equal(
            "https://github.com/Mamat79/DanteConfigEditorV3/tree/v3.6/machine-banks",
            MachineBankDistributionService.GitHubBanksUrl);
    }

    [Fact]
    public void BundledBankFilesKeepStableLineEndingsAcrossGitCheckouts()
    {
        string attributes = File.ReadAllText(RepositoryFile(".gitattributes"));

        Assert.Contains(
            "Resources/MachineBanks/**/*.json text eol=lf",
            attributes,
            StringComparison.Ordinal);
        Assert.Contains(
            "Resources/MachineBanks/**/*.xml text eol=lf",
            attributes,
            StringComparison.Ordinal);
        Assert.Contains("*.zip binary", attributes, StringComparison.Ordinal);
    }

    private static string[] ReadLabels(XDocument document, string channelElementName)
    {
        return document.Root!.Elements()
            .Where(element => element.Name.LocalName == channelElementName)
            .Select(channel => Child(channel, "label")?.Value
                ?? Child(channel, "name")?.Value
                ?? string.Empty)
            .ToArray();
    }

    private static XElement? Child(XElement? parent, string localName)
    {
        return parent?.Elements().FirstOrDefault(element => element.Name.LocalName == localName);
    }

    private static IEnumerable<XElement> Children(XElement? parent, string localName)
    {
        return parent?.Elements().Where(element => element.Name.LocalName == localName) ?? [];
    }

    private static string RepositoryFile(params string[] relativeParts)
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null
               && !File.Exists(Path.Combine(directory.FullName, "DanteConfigEditorV3.csproj")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);
        return Path.Combine([directory!.FullName, .. relativeParts]);
    }

    private sealed class TestWorkspace : IDisposable
    {
        public TestWorkspace()
        {
            DirectoryPath = Path.Combine(Path.GetTempPath(), "DanteConfigEditorV3.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(DirectoryPath);
            SourcePath = Path.Combine(DirectoryPath, "representative-preset.xml");
            File.Copy(
                Path.Combine(AppContext.BaseDirectory, "Fixtures", "representative-preset.xml"),
                SourcePath);
            BankPath = Path.Combine(DirectoryPath, "MachineBank");
        }

        public string DirectoryPath { get; }

        public string SourcePath { get; }

        public string BankPath { get; }

        public void Dispose()
        {
            if (Directory.Exists(DirectoryPath))
            {
                Directory.Delete(DirectoryPath, true);
            }
        }
    }
}
