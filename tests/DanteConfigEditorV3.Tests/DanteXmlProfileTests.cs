using DanteConfigEditor.DanteXml;
using DanteConfigEditor.DanteXml.Profiles;
using DanteConfigEditor.Domain.Projects;
using DanteConfigEditor.Models;

namespace DanteConfigEditorV3.Tests;

public sealed class DanteXmlProfileTests
{
    [Fact]
    public void RepresentativePresetExposesCompleteEditingCapabilities()
    {
        DanteProject project = DanteProject.Load(RepositoryFile(
            "tests",
            "DanteConfigEditorV3.Tests",
            "Fixtures",
            "representative-preset.xml"));

        DanteXmlProfileDescriptor profile = new DanteXmlProfileDetector().Detect(project);

        Assert.Equal("recognized-complete", profile.Id);
        Assert.Equal(DanteXmlRecognitionLevel.Complete, profile.RecognitionLevel);
        Assert.Equal(ProjectAccessMode.Full, profile.AccessMode);
        Assert.True(profile.Capabilities.CanReadMachines);
        Assert.True(profile.Capabilities.CanEditDeviceNames);
        Assert.True(profile.Capabilities.CanEditTxLabels);
        Assert.True(profile.Capabilities.CanEditRxLabels);
        Assert.True(profile.Capabilities.CanEditPatch);
        Assert.True(profile.Capabilities.CanSave);
    }

    [Fact]
    public void IncompleteChannelShapeProducesRestrictedProfileWithoutGuessing()
    {
        string path = WriteTemporaryPreset(
            """
            <?xml version="1.0" encoding="UTF-8"?>
            <preset version="3.0.0">
              <name>Partial</name>
              <device>
                <friendly_name>PARTIAL-DEVICE</friendly_name>
                <txchannel><label>TX 1</label></txchannel>
              </device>
            </preset>
            """);

        try
        {
            DanteXmlProfileDescriptor profile =
                new DanteXmlProfileDetector().Detect(DanteProject.Load(path));

            Assert.Equal("recognized-partial", profile.Id);
            Assert.Equal(DanteXmlRecognitionLevel.Partial, profile.RecognitionLevel);
            Assert.Equal(ProjectAccessMode.Restricted, profile.AccessMode);
            Assert.True(profile.Capabilities.CanSave);
            Assert.Contains("XmlProfile.SomeChannelsHaveNoDanteIdOrMediaType", profile.Reasons);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void UnknownRootIsReadOnlyAndAdapterRefusesSave()
    {
        string path = WriteTemporaryPreset(
            """
            <?xml version="1.0" encoding="UTF-8"?>
            <unknown version="3.0.0">
              <device>
                <friendly_name>UNKNOWN-DEVICE</friendly_name>
              </device>
            </unknown>
            """);
        string destination = Path.Combine(
            Path.GetDirectoryName(path)!,
            $"{Path.GetFileNameWithoutExtension(path)}-saved.xml");

        try
        {
            DanteXmlProjectAdapter adapter = new();
            DanteXmlOpenResult opened = adapter.Open(path);

            Assert.Equal("unknown-read-only", opened.Profile.Id);
            Assert.Equal(ProjectAccessMode.ReadOnly, opened.Profile.AccessMode);
            Assert.False(opened.Profile.Capabilities.CanSave);
            Assert.Throws<InvalidOperationException>(() => adapter.SaveAs(opened, destination));
            Assert.False(File.Exists(destination));
        }
        finally
        {
            File.Delete(path);
            if (File.Exists(destination))
            {
                File.Delete(destination);
            }
        }
    }

    private static string WriteTemporaryPreset(string xml)
    {
        string path = Path.Combine(Path.GetTempPath(), $"dce-profile-{Guid.NewGuid():N}.xml");
        File.WriteAllText(path, xml);
        return path;
    }

    private static string RepositoryFile(params string[] relativeParts) =>
        Path.Combine([RepositoryDirectory(), .. relativeParts]);

    private static string RepositoryDirectory()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null
               && !File.Exists(Path.Combine(directory.FullName, "DanteConfigEditorV3.csproj")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);
        return directory!.FullName;
    }
}
