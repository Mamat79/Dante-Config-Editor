using System.Xml.Linq;
using DanteConfigEditor.Models;
using DanteConfigEditor.Services;

namespace DanteConfigEditorV3.Tests;

public sealed class TechnicalXmlCapabilityTests
{
    [Fact]
    public void UnsupportedRedundancyCannotBeInventedAndDaisychainIsANoOp()
    {
        using TestWorkspace workspace = new();
        string source = workspace.CreatePreset(
            Device(
                "ULTIMO",
                captureCapabilities: ["device_name", "device_samplerate"],
                technicalElements:
                [
                    new XElement("samplerate", "48000")
                ]));
        DanteProject project = DanteProject.Load(source);
        XDocument before = new(project.Document);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => project.SetNetworkMode("ULTIMO", redundant: true));

        Assert.Contains("redondance", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.True(XNode.DeepEquals(before, project.Document));

        project.SetNetworkMode("ULTIMO", redundant: false);

        Assert.True(XNode.DeepEquals(before, project.Document));
        Assert.False(project.IsModified);
        Assert.Null(DeviceElement(project, "ULTIMO").Elements().SingleOrDefault(
            element => element.Name.LocalName == "redundancy"));
    }

    [Fact]
    public void SupportedRedundancyCanRoundTripWithoutChangingTheXmlShape()
    {
        using TestWorkspace workspace = new();
        string source = workspace.CreatePreset(
            Device(
                "BROOKLYN",
                captureCapabilities: ["device_name", "device_redundancy"],
                technicalElements:
                [
                    new XElement("redundancy", new XAttribute("value", "false"))
                ]));
        string destination = workspace.PathFor("supported-redundancy.xml");
        DanteProject project = DanteProject.Load(source);

        project.SetNetworkMode("BROOKLYN", redundant: true);
        Assert.True(project.FindDevice("BROOKLYN")!.IsRedundant);
        project.SetNetworkMode("BROOKLYN", redundant: false);

        XElement redundancy = Child(DeviceElement(project, "BROOKLYN"), "redundancy");
        Assert.Equal("false", redundancy.Attribute("value")?.Value);
        Assert.False(project.ValidateXmlChangeGuard().HasErrors);
        project.SaveAs(destination);

        DanteProject reloaded = DanteProject.Load(destination);
        Assert.False(reloaded.FindDevice("BROOKLYN")!.IsRedundant);
        Assert.Single(
            DeviceElement(reloaded, "BROOKLYN").Elements(),
            element => element.Name.LocalName == "redundancy");
    }

    [Fact]
    public void DirectTechnicalChangesRejectMissingPresetElements()
    {
        using TestWorkspace workspace = new();
        string source = workspace.CreatePreset(
            Device(
                "PARTIAL",
                captureCapabilities: ["device_name"],
                technicalElements: []));
        DanteProject project = DanteProject.Load(source);
        XDocument before = new(project.Document);

        Assert.Throws<InvalidOperationException>(() => project.SetLatency("PARTIAL", "1000"));
        Assert.Throws<InvalidOperationException>(() => project.SetSamplerate("PARTIAL", "48000"));
        Assert.Throws<InvalidOperationException>(() => project.SetEncoding("PARTIAL", "24"));
        Assert.Throws<InvalidOperationException>(() => project.SetPreferredMaster("PARTIAL", preferredMaster: true));
        Assert.Throws<InvalidOperationException>(
            () => project.SetIpAddressStatic("PARTIAL", "192.168.1.20", "255.255.255.0", "192.168.1.1"));

        project.SetPreferredMaster("PARTIAL", preferredMaster: false);
        Assert.True(XNode.DeepEquals(before, project.Document));
        Assert.False(project.IsModified);
    }

    [Fact]
    public void GlobalTechnicalActionsOnlyModifyElementsAlreadyPresent()
    {
        using TestWorkspace workspace = new();
        string source = workspace.CreatePreset(
            Device(
                "FULL",
                captureCapabilities:
                [
                    "device_name",
                    "device_redundancy",
                    "device_samplerate",
                    "device_encoding",
                    "device_unicast_latency",
                    "clock_priority"
                ],
                technicalElements:
                [
                    new XElement("redundancy", new XAttribute("value", "false")),
                    new XElement("preferred_master", new XAttribute("value", "false")),
                    new XElement("samplerate", "48000"),
                    new XElement("encoding", "24"),
                    new XElement("unicast_latency", "1000")
                ]),
            Device(
                "PARTIAL",
                captureCapabilities: ["device_name"],
                technicalElements: []));
        DanteProject project = DanteProject.Load(source);

        project.SetAllNetworkModes(redundant: true);
        project.SetAllLatencies("2000");
        project.SetAllSamplerates("96000");
        project.SetAllEncodings("32");
        project.SetExclusivePreferredMaster("FULL");

        XElement full = DeviceElement(project, "FULL");
        Assert.Equal("true", Child(full, "redundancy").Attribute("value")?.Value);
        Assert.Equal("true", Child(full, "preferred_master").Attribute("value")?.Value);
        Assert.Equal("2000", Child(full, "unicast_latency").Value);
        Assert.Equal("96000", Child(full, "samplerate").Value);
        Assert.Equal("32", Child(full, "encoding").Value);

        XElement partial = DeviceElement(project, "PARTIAL");
        Assert.DoesNotContain(
            partial.Elements(),
            element => element.Name.LocalName is
                "redundancy" or "preferred_master" or "unicast_latency" or "samplerate" or "encoding");
        Assert.False(project.ValidateXmlChangeGuard().HasErrors);
    }

    [Theory]
    [InlineData("samplerate", "48000")]
    [InlineData("encoding", "24")]
    [InlineData("unicast_latency", "1000")]
    [InlineData("preferred_master", "false")]
    [InlineData("redundancy", "false")]
    public void GuardRejectsADeviceTechnicalElementMissingFromTheOriginal(
        string elementName,
        string value)
    {
        using TestWorkspace workspace = new();
        string source = workspace.CreatePreset(
            Device(
                "PARTIAL",
                captureCapabilities: ["device_name"],
                technicalElements: []));
        XDocument original = XDocument.Load(source, LoadOptions.PreserveWhitespace);
        XDocument current = new(original);
        XElement currentDevice = current.Root!.Elements()
            .Single(element => element.Name.LocalName == "device");
        XElement added = elementName is "preferred_master" or "redundancy"
            ? new XElement(elementName, new XAttribute("value", value))
            : new XElement(elementName, value);
        currentDevice.Add(added);

        DanteValidationResult result = DanteXmlChangeGuardService.ValidateChanges(original, current);

        Assert.True(result.HasErrors);
        Assert.Contains(
            result.Errors,
            error => error.Contains(elementName, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void StaticIpCannotCreateAnIpv4StructureThatWasNotInThePreset()
    {
        using TestWorkspace workspace = new();
        string source = workspace.CreatePreset(
            Device(
                "NO-IP",
                captureCapabilities: ["device_name"],
                technicalElements:
                [
                    new XElement("interface", new XAttribute("network", "0"))
                ]));
        DanteProject project = DanteProject.Load(source);
        XDocument before = new(project.Document);

        Assert.False(project.SupportsIpConfiguration("NO-IP"));
        Assert.Throws<InvalidOperationException>(
            () => project.SetIpAddressStatic("NO-IP", "192.168.1.20", "255.255.255.0", "192.168.1.1"));
        Assert.True(XNode.DeepEquals(before, project.Document));
    }

    [Fact]
    public void DuplicateWithoutSettingsDoesNotInventMissingTechnicalElements()
    {
        using TestWorkspace workspace = new();
        string source = workspace.CreatePreset(
            Device(
                "PARTIAL",
                captureCapabilities: ["device_name"],
                technicalElements: []));
        DanteProject project = DanteProject.Load(source);

        project.DuplicateDevice(
            "PARTIAL",
            new MachineCloneOptions
            {
                NewName = "PARTIAL-COPY",
                PreserveDeviceSettings = false,
                PreserveNetworkConfiguration = false,
                PreservePreferredMaster = false,
                PreserveTxLabels = true,
                PreserveRxLabels = true
            });

        XElement clone = DeviceElement(project, "PARTIAL-COPY");
        Assert.DoesNotContain(
            clone.Elements(),
            element => element.Name.LocalName is
                "redundancy" or "preferred_master" or "unicast_latency" or "samplerate" or "encoding");
        Assert.False(project.ValidateXmlChangeGuard().HasErrors);
    }

    [Fact]
    [Trait("Category", "LocalIntegration")]
    public void RealPresetCorpusDoesNotAllowInventedTechnicalElements()
    {
        string? corpusRoot = Environment.GetEnvironmentVariable("DANTE_REAL_XML_ROOT");
        if (string.IsNullOrWhiteSpace(corpusRoot) || !Directory.Exists(corpusRoot))
        {
            return;
        }

        string[] files = Directory.GetFiles(corpusRoot, "*.xml", SearchOption.AllDirectories);
        Assert.NotEmpty(files);

        foreach (string file in files)
        {
            DanteProject project = DanteProject.Load(file);
            foreach (DanteDevice device in project.Devices)
            {
                AssertUnsupportedChangeDoesNotMutate(
                    project,
                    device,
                    device.SupportsNetworkMode,
                    () => project.SetNetworkMode(device.Name, redundant: true));
                AssertUnsupportedChangeDoesNotMutate(
                    project,
                    device,
                    device.SupportsPreferredMaster,
                    () => project.SetPreferredMaster(device.Name, preferredMaster: true));
                AssertUnsupportedChangeDoesNotMutate(
                    project,
                    device,
                    device.SupportsLatency,
                    () => project.SetLatency(device.Name, "1000"));
                AssertUnsupportedChangeDoesNotMutate(
                    project,
                    device,
                    device.SupportsSampleRate,
                    () => project.SetSamplerate(device.Name, "48000"));
                AssertUnsupportedChangeDoesNotMutate(
                    project,
                    device,
                    device.SupportsEncoding,
                    () => project.SetEncoding(device.Name, "24"));
                AssertUnsupportedChangeDoesNotMutate(
                    project,
                    device,
                    device.SupportsIpConfiguration,
                    () => project.SetIpAddressStatic(
                        device.Name,
                        "192.168.254.20",
                        "255.255.255.0",
                        "0.0.0.0"));
            }
        }
    }

    private static void AssertUnsupportedChangeDoesNotMutate(
        DanteProject project,
        DanteDevice device,
        bool supported,
        Action action)
    {
        if (supported)
        {
            return;
        }

        XDocument before = new(project.Document);
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(action);
        Assert.Contains(device.Name, exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.True(
            XNode.DeepEquals(before, project.Document),
            $"The unsupported action changed {device.Name}.");
    }

    private static XElement Device(
        string name,
        IReadOnlyList<string> captureCapabilities,
        IReadOnlyList<XElement> technicalElements)
    {
        return new XElement(
            "device",
            new XElement(
                "captureInfo",
                captureCapabilities.Select(capability => new XElement(capability))),
            new XElement("name", name),
            new XElement("friendly_name", name),
            new XElement(
                "instance_id",
                new XElement("device_id", $"DEVICE-{name}"),
                new XElement("process_id", "0")),
            technicalElements.Select(element => new XElement(element)),
            new XElement(
                "txchannel",
                new XAttribute("danteId", "1"),
                new XAttribute("mediaType", "audio"),
                new XElement("label", "TX 1")),
            new XElement(
                "rxchannel",
                new XAttribute("danteId", "1"),
                new XAttribute("mediaType", "audio"),
                new XElement("name", "RX 1")));
    }

    private static XElement DeviceElement(DanteProject project, string name)
    {
        return project.Document.Root!.Elements()
            .Where(element => element.Name.LocalName == "device")
            .Single(element => string.Equals(
                element.Elements().FirstOrDefault(child => child.Name.LocalName == "name")?.Value
                    ?? element.Elements().FirstOrDefault(child => child.Name.LocalName == "friendly_name")?.Value,
                name,
                StringComparison.Ordinal));
    }

    private static XElement Child(XElement parent, string localName)
    {
        return parent.Elements().Single(element => element.Name.LocalName == localName);
    }

    private sealed class TestWorkspace : IDisposable
    {
        public TestWorkspace()
        {
            DirectoryPath = Path.Combine(
                Path.GetTempPath(),
                "DanteConfigEditorV3.TechnicalXmlCapabilityTests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(DirectoryPath);
        }

        public string DirectoryPath { get; }

        public string PathFor(string fileName) => Path.Combine(DirectoryPath, fileName);

        public string CreatePreset(params XElement[] devices)
        {
            string path = PathFor("technical-capabilities.xml");
            new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement(
                    "preset",
                    new XAttribute("version", "3.0.0"),
                    new XElement("name", "Technical capability test"),
                    devices))
                .Save(path, SaveOptions.DisableFormatting);
            return path;
        }

        public void Dispose()
        {
            if (Directory.Exists(DirectoryPath))
            {
                Directory.Delete(DirectoryPath, recursive: true);
            }
        }
    }
}
