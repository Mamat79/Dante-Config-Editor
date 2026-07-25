using System.Xml.Linq;
using DanteConfigEditor.Models;
using DanteConfigEditor.Services;

namespace DanteConfigEditorV3.Tests;

public sealed class ProjectCreationV36Tests
{
    [Fact]
    public void NewProjectUsesOfficialCreatorCompatibleGenericRoleStructure()
    {
        using TemporaryDirectory workspace = new();
        string intendedPath = Path.Combine(workspace.Path, "new-project.xml");
        DanteProject project = DanteProject.CreateNew(
            intendedPath,
            new NewProjectOptions
            {
                ProjectName = "Training preset",
                Description = "Preset expérimental de test",
                Machines =
                [
                    new NewCustomMachineDefinition
                    {
                        Name = "STAGEBOX-01",
                        TxCount = 32,
                        RxCount = 16
                    }
                ]
            });

        DanteDevice device = Assert.Single(project.Devices);
        Assert.Equal("STAGEBOX-01", device.Name);
        Assert.True(device.IsGenericRole);
        Assert.Equal(32, device.TxCount);
        Assert.Equal(16, device.RxCount);
        XElement xmlDevice = Assert.Single(
            project.Document.Root!.Elements(),
            element => element.Name.LocalName == "device");
        Assert.Null(Child(xmlDevice, "name"));
        Assert.Null(Child(xmlDevice, "instance_id"));
        Assert.Null(Child(xmlDevice, "default_name"));
        Assert.Equal("STAGEBOX-01", Child(xmlDevice, "friendly_name")!.Value);
        Assert.Equal(
            [
                "device_name",
                "device_samplerate",
                "device_encoding",
                "device_unicast_latency",
                "txchannel_names",
                "txflows",
                "rxchannel_names",
                "rxchannel_subscriptions",
                "rxflows"
            ],
            Child(xmlDevice, "captureInfo")!.Elements().Select(element => element.Name.LocalName));
        Assert.False(project.Validate().HasErrors);
    }

    [Fact]
    public void NewProjectCanSaveAndReloadWithoutAnExistingSourceFile()
    {
        using TemporaryDirectory workspace = new();
        string destinationPath = Path.Combine(workspace.Path, "created.xml");
        DanteProject project = DanteProject.CreateNew(
            destinationPath,
            new NewProjectOptions
            {
                ProjectName = "Created",
                Machines =
                [
                    new NewCustomMachineDefinition
                    {
                        Name = "CUSTOM-ONE",
                        TxCount = 2,
                        RxCount = 2
                    }
                ]
            });

        string backup = project.SaveAs(destinationPath);
        DanteProject reloaded = DanteProject.Load(destinationPath);

        Assert.Equal(string.Empty, backup);
        Assert.False(project.IsModified);
        Assert.Single(reloaded.Devices);
        Assert.True(reloaded.Devices[0].IsGenericRole);
        Assert.False(reloaded.Validate().HasErrors);
    }

    [Fact]
    public void NewProjectFromBankTemplateKeepsTheTemplateIndependent()
    {
        using TemporaryDirectory workspace = new();
        string sourcePath = Path.Combine(workspace.Path, "source.xml");
        File.Copy(
            Path.Combine(AppContext.BaseDirectory, "Fixtures", "representative-preset.xml"),
            sourcePath);
        DanteProject source = DanteProject.Load(sourcePath);
        MachineTemplatePackage template = MachineTemplateService.CreateFromDevice(
            source.FindDevice("DEVICE-A")!,
            source.PresetVersion,
            new MachineTemplateCreateRequest
            {
                TemplateName = "Bank source"
            });
        string templateBefore = template.TemplateDocument.ToString(SaveOptions.DisableFormatting);

        DanteProject created = DanteProject.CreateNewFromTemplate(
            Path.Combine(workspace.Path, "from-bank.xml"),
            "From bank",
            null,
            template,
            new MachineInstanceOptions
            {
                NewName = "BANK-DEVICE"
            });

        Assert.Equal("BANK-DEVICE", Assert.Single(created.Devices).Name);
        Assert.Equal(templateBefore, template.TemplateDocument.ToString(SaveOptions.DisableFormatting));
        Assert.False(created.Validate().HasErrors);
    }

    [Theory]
    [InlineData(-1, 1)]
    [InlineData(513, 1)]
    [InlineData(1, -1)]
    [InlineData(1, 513)]
    [InlineData(0, 0)]
    public void NewProjectRejectsInvalidChannelCounts(int txCount, int rxCount)
    {
        using TemporaryDirectory workspace = new();

        Assert.Throws<InvalidOperationException>(() =>
            DanteProject.CreateNew(
                Path.Combine(workspace.Path, "invalid.xml"),
                new NewProjectOptions
                {
                    ProjectName = "Invalid",
                    Machines =
                    [
                        new NewCustomMachineDefinition
                        {
                            Name = "INVALID-DEVICE",
                            TxCount = txCount,
                            RxCount = rxCount
                        }
                    ]
                }));
    }

    [Fact]
    public void NewProjectNeverOverwritesAnExistingFile()
    {
        using TemporaryDirectory workspace = new();
        string existingPath = Path.Combine(workspace.Path, "existing.xml");
        File.WriteAllText(existingPath, "do-not-replace");

        Assert.Throws<IOException>(() =>
            DanteProject.CreateNew(
                existingPath,
                new NewProjectOptions
                {
                    ProjectName = "Blocked",
                    Machines =
                    [
                        new NewCustomMachineDefinition
                        {
                            Name = "CUSTOM",
                            TxCount = 1
                        }
                    ]
                }));
        Assert.Equal("do-not-replace", File.ReadAllText(existingPath));
    }

    private static XElement? Child(XElement? parent, string localName)
    {
        return parent?.Elements().FirstOrDefault(element => element.Name.LocalName == localName);
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "DanteConfigEditorV3.Tests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
