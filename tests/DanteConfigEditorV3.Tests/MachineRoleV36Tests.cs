using System.Xml.Linq;
using DanteConfigEditor.Models;
using DanteConfigEditor.Services;

namespace DanteConfigEditorV3.Tests;

public sealed class MachineRoleV36Tests
{
    [Fact]
    public void OfficialPresetCreatorRoleWithoutNameOrInstanceIdUsesFriendlyName()
    {
        using TestWorkspace workspace = new("official-preset-creator-custom.xml");

        DanteProject project = DanteProject.Load(workspace.SourcePath);

        DanteDevice device = Assert.Single(project.Devices);
        Assert.Equal("CUSTOM-IO", device.Name);
        Assert.Equal("CUSTOM-IO", device.FriendlyName);
        Assert.True(device.IsGenericRole);
        Assert.Equal(string.Empty, device.TechnicalDeviceId);
        Assert.False(project.Validate().HasErrors);
    }

    [Fact]
    public void DuplicateDeviceCreatesIndependentGenericRoleAndCanRoundTrip()
    {
        using TestWorkspace workspace = new("representative-preset.xml");
        DanteProject project = DanteProject.Load(workspace.SourcePath);
        string sourceTechnicalId = project.FindDevice("DEVICE-A")!.TechnicalDeviceId;
        project.PushUndoSnapshot("duplicate");

        MachineCloneResult result = project.DuplicateDevice(
            "DEVICE-A",
            new MachineCloneOptions
            {
                NewName = "DEVICE-A-CLONE"
            });

        DanteDevice source = Assert.IsType<DanteDevice>(project.FindDevice("DEVICE-A"));
        DanteDevice clone = Assert.IsType<DanteDevice>(project.FindDevice("DEVICE-A-CLONE"));
        Assert.Equal(sourceTechnicalId, source.TechnicalDeviceId);
        Assert.True(clone.IsGenericRole);
        Assert.Equal(string.Empty, clone.TechnicalDeviceId);
        Assert.Equal(0, result.CopiedSubscriptionCount);
        Assert.DoesNotContain(
            project.PatchMatrix.Subscriptions,
            subscription => subscription.RxDevice == clone.Name && subscription.IsActive);
        Assert.False(project.Validate().HasErrors);
        Assert.False(project.ValidateXmlChangeGuard().HasErrors);

        string outputPath = Path.Combine(workspace.DirectoryPath, "duplicated.xml");
        project.SaveAs(outputPath);
        DanteProject reloaded = DanteProject.Load(outputPath);
        DanteDevice reloadedClone = Assert.IsType<DanteDevice>(reloaded.FindDevice("DEVICE-A-CLONE"));
        Assert.True(reloadedClone.IsGenericRole);
        Assert.Equal(string.Empty, reloadedClone.TechnicalDeviceId);
        Assert.False(reloaded.Validate().HasErrors);
    }

    [Fact]
    public void DuplicateDeviceDefaultsToNeutralNetworkAndPreservesUnknownIntrinsicXml()
    {
        using TestWorkspace workspace = new("representative-preset.xml");
        XDocument sourceDocument = XDocument.Load(workspace.SourcePath, LoadOptions.PreserveWhitespace);
        XElement firstDevice = sourceDocument.Root!.Elements().First(element => element.Name.LocalName == "device");
        firstDevice.Add(new XElement(firstDevice.Name.Namespace + "vendor_extension",
            new XAttribute("mode", "preserve"),
            new XElement(firstDevice.Name.Namespace + "opaque", "42")));
        sourceDocument.Save(workspace.SourcePath, SaveOptions.DisableFormatting);
        DanteProject project = DanteProject.Load(workspace.SourcePath);

        project.DuplicateDevice(
            "DEVICE-A",
            new MachineCloneOptions
            {
                NewName = "DEVICE-A-GENERIC"
            });

        XElement clone = FindDeviceElement(project.Document, "DEVICE-A-GENERIC");
        Assert.Null(Child(clone, "instance_id"));
        Assert.Null(Child(clone, "default_name"));
        Assert.Empty(Children(clone, "interface"));
        Assert.NotNull(Child(clone, "vendor_extension"));
        Assert.All(
            Children(clone, "rxchannel"),
            channel =>
            {
                Assert.Null(Child(channel, "subscribed_device"));
                Assert.Null(Child(channel, "subscribed_channel"));
            });
    }

    [Theory]
    [InlineData("")]
    [InlineData("DEVICE A")]
    [InlineData("-DEVICE")]
    [InlineData("DEVICE-")]
    [InlineData("DEVICE_2")]
    [InlineData("DEVICE-NAME-THAT-IS-DEFINITELY-LONGER-THAN-31")]
    public void DuplicateDeviceRejectsNamesOutsideOfficialCreatorRules(string newName)
    {
        using TestWorkspace workspace = new("representative-preset.xml");
        DanteProject project = DanteProject.Load(workspace.SourcePath);

        InvalidOperationException error = Assert.Throws<InvalidOperationException>(() =>
            project.DuplicateDevice(
                "DEVICE-A",
                new MachineCloneOptions
                {
                    NewName = newName
                }));

        Assert.Contains("nom", error.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(3, project.Devices.Count);
    }

    [Fact]
    public void DuplicateDeviceRejectsAnExistingVisibleName()
    {
        using TestWorkspace workspace = new("representative-preset.xml");
        DanteProject project = DanteProject.Load(workspace.SourcePath);

        Assert.Throws<InvalidOperationException>(() =>
            project.DuplicateDevice(
                "DEVICE-A",
                new MachineCloneOptions
                {
                    NewName = "DEVICE-B"
                }));
    }

    [Fact]
    public void ValidationBlocksDuplicateTechnicalDeviceIdentityPresentAtLoad()
    {
        using TestWorkspace workspace = new("representative-preset.xml");
        XDocument document = XDocument.Load(workspace.SourcePath, LoadOptions.PreserveWhitespace);
        XElement[] devices = document.Root!.Elements().Where(element => element.Name.LocalName == "device").ToArray();
        string firstId = Child(Child(devices[0], "instance_id")!, "device_id")!.Value;
        Child(Child(devices[1], "instance_id")!, "device_id")!.Value = firstId;
        document.Save(workspace.SourcePath, SaveOptions.DisableFormatting);

        DanteProject project = DanteProject.Load(workspace.SourcePath);
        DanteValidationResult validation = project.Validate();

        Assert.True(validation.HasErrors);
        Assert.Contains(
            validation.Issues,
            issue => issue.Category == DanteIssueCategory.Device
                && issue.Message.Contains("device_id", StringComparison.OrdinalIgnoreCase)
                && issue.Message.Contains("DEVICE-A", StringComparison.OrdinalIgnoreCase)
                && issue.Message.Contains("DEVICE-B", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ChangeGuardBlocksAnUnapprovedDeviceAddition()
    {
        using TestWorkspace workspace = new("representative-preset.xml");
        XDocument original = XDocument.Load(workspace.SourcePath, LoadOptions.PreserveWhitespace);
        XDocument current = new(original);
        XElement added = new(current.Root!.Elements().First(element => element.Name.LocalName == "device"));
        Child(added, "name")!.Value = "UNAPPROVED";
        Child(added, "friendly_name")!.Value = "UNAPPROVED";
        current.Root!.Add(added);

        DanteValidationResult guard = DanteXmlChangeGuardService.ValidateChanges(original, current);

        Assert.True(guard.HasErrors);
        Assert.Contains(
            guard.Errors,
            error => error.Contains("ajout", StringComparison.OrdinalIgnoreCase)
                && error.Contains("non autoris", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void DuplicateCanBeUndoneWithoutChangingTheSourceRole()
    {
        using TestWorkspace workspace = new("representative-preset.xml");
        DanteProject project = DanteProject.Load(workspace.SourcePath);
        project.PushUndoSnapshot("duplicate");

        project.DuplicateDevice(
            "DEVICE-A",
            new MachineCloneOptions
            {
                NewName = "DEVICE-A-COPY"
            });
        project.UndoLastChange();

        Assert.Equal(3, project.Devices.Count);
        Assert.Null(project.FindDevice("DEVICE-A-COPY"));
        Assert.Equal("001DC1FFFE000001", project.FindDevice("DEVICE-A")!.TechnicalDeviceId);
        Assert.False(project.IsModified);
    }

    [Fact]
    public void DuplicateInDefaultNamespaceKeepsEveryDanteElementInThatNamespace()
    {
        using TestWorkspace workspace = new("representative-preset.xml");
        XDocument document = XDocument.Load(workspace.SourcePath, LoadOptions.PreserveWhitespace);
        XNamespace ns = "urn:audinate:test:preset";
        foreach (XElement element in document.Root!.DescendantsAndSelf())
        {
            if (element.Name.Namespace == XNamespace.None)
            {
                element.Name = ns + element.Name.LocalName;
            }
        }
        document.Save(workspace.SourcePath, SaveOptions.DisableFormatting);
        DanteProject project = DanteProject.Load(workspace.SourcePath);

        project.DuplicateDevice(
            "DEVICE-A",
            new MachineCloneOptions
            {
                NewName = "NAMESPACE-COPY"
            });

        XElement clone = FindDeviceElement(project.Document, "NAMESPACE-COPY");
        Assert.All(clone.DescendantsAndSelf(), element => Assert.Equal(ns.NamespaceName, element.Name.NamespaceName));
        Assert.False(project.Validate().HasErrors);
        string outputPath = Path.Combine(workspace.DirectoryPath, "namespace-output.xml");
        project.SaveAs(outputPath);
        Assert.NotNull(DanteProject.Load(outputPath).FindDevice("NAMESPACE-COPY"));
    }

    [Fact]
    public void DuplicateWithPreservedSubscriptionsRewritesAnExplicitLocalReference()
    {
        using TestWorkspace workspace = new("representative-preset.xml");
        XDocument document = XDocument.Load(workspace.SourcePath, LoadOptions.PreserveWhitespace);
        XElement source = FindDeviceElement(document, "DEVICE-A");
        Child(Assert.Single(Children(source, "rxchannel")), "subscribed_device")!.Value = "DEVICE-A";
        document.Save(workspace.SourcePath, SaveOptions.DisableFormatting);
        DanteProject project = DanteProject.Load(workspace.SourcePath);

        project.DuplicateDevice(
            "DEVICE-A",
            new MachineCloneOptions
            {
                NewName = "DEVICE-A-COPY",
                PreserveSubscriptions = true
            });

        XElement clone = FindDeviceElement(project.Document, "DEVICE-A-COPY");
        XElement cloneRx = Assert.Single(Children(clone, "rxchannel"));
        Assert.Equal("DEVICE-A-COPY", Child(cloneRx, "subscribed_device")!.Value);
        Assert.Equal("DEVICE-A", Child(
            Assert.Single(Children(FindDeviceElement(project.Document, "DEVICE-A"), "rxchannel")),
            "subscribed_device")!.Value);
    }

    [Fact]
    public void AutomaticDuplicateRenameStaysWithinOfficialDeviceNameRules()
    {
        using TestWorkspace currentWorkspace = new("representative-preset.xml");
        using TestWorkspace importedWorkspace = new("representative-preset.xml");
        XDocument currentDocument = XDocument.Load(currentWorkspace.SourcePath, LoadOptions.PreserveWhitespace);
        XDocument importedDocument = XDocument.Load(importedWorkspace.SourcePath, LoadOptions.PreserveWhitespace);
        const string maximumLengthName = "DEVICE-NAME-1234567890123456789";
        RenameDeviceElement(currentDocument, "DEVICE-A", maximumLengthName);
        RenameDeviceElement(importedDocument, "DEVICE-A", maximumLengthName);
        currentDocument.Save(currentWorkspace.SourcePath, SaveOptions.DisableFormatting);
        importedDocument.Save(importedWorkspace.SourcePath, SaveOptions.DisableFormatting);
        DanteProject project = DanteProject.Load(currentWorkspace.SourcePath);

        IReadOnlyDictionary<string, string> renameMap = project.BuildAutomaticDuplicateRenameMap(
            importedWorkspace.SourcePath,
            "Import");

        string generatedName = renameMap[maximumLengthName];
        Assert.True(generatedName.Length <= DanteNameRules.MaximumNameLength);
        Assert.Null(DanteNameRules.ValidateDeviceName(generatedName));
        Assert.EndsWith("-Import", generatedName, StringComparison.Ordinal);
    }

    private static void RenameDeviceElement(XDocument document, string oldName, string newName)
    {
        XElement device = FindDeviceElement(document, oldName);
        Child(device, "name")!.Value = newName;
        Child(device, "friendly_name")!.Value = newName;
    }

    private static XElement FindDeviceElement(XDocument document, string name)
    {
        return document.Root!.Elements()
            .Where(element => element.Name.LocalName == "device")
            .Single(element => string.Equals(
                Child(element, "name")?.Value ?? Child(element, "friendly_name")?.Value,
                name,
                StringComparison.Ordinal));
    }

    private static XElement? Child(XElement? parent, string localName)
    {
        return parent?.Elements().FirstOrDefault(element => element.Name.LocalName == localName);
    }

    private static IEnumerable<XElement> Children(XElement? parent, string localName)
    {
        return parent?.Elements().Where(element => element.Name.LocalName == localName) ?? [];
    }

    private sealed class TestWorkspace : IDisposable
    {
        public TestWorkspace(string fixtureName)
        {
            DirectoryPath = Path.Combine(Path.GetTempPath(), "DanteConfigEditorV3.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(DirectoryPath);
            SourcePath = Path.Combine(DirectoryPath, fixtureName);
            File.Copy(Path.Combine(AppContext.BaseDirectory, "Fixtures", fixtureName), SourcePath);
        }

        public string DirectoryPath { get; }

        public string SourcePath { get; }

        public void Dispose()
        {
            if (Directory.Exists(DirectoryPath))
            {
                Directory.Delete(DirectoryPath, true);
            }
        }
    }
}
