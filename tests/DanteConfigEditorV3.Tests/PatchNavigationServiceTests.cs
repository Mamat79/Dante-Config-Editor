using System.Xml.Linq;
using DanteConfigEditor.Models;
using DanteConfigEditor.Services;

namespace DanteConfigEditorV3.Tests;

public sealed class PatchNavigationServiceTests
{
    [Fact]
    public void RxNavigationResolvesExternalAndLocalSources()
    {
        DanteProject project = LoadRepresentativePreset();
        PatchWorkspaceSession session = new(project.PatchMatrix.Subscriptions);
        PatchNavigationService navigation = new(project, session);

        PatchSourceNavigationResult external = navigation.FindSource(
            Target(project, "DEVICE-B", 1));
        PatchSourceNavigationResult local = navigation.FindSource(
            Target(project, "DEVICE-A", 1));

        Assert.Equal(PatchNavigationStatus.Found, external.Status);
        Assert.Equal("DEVICE-A", external.Source?.DeviceName);
        Assert.Equal(1, external.Source?.DanteId);
        Assert.Equal("PROGRAM L", external.Source?.ChannelName);
        Assert.Equal(PatchNavigationStatus.Found, local.Status);
        Assert.Equal("DEVICE-A", local.Source?.DeviceName);
        Assert.Equal(1, local.Source?.DanteId);
    }

    [Fact]
    public void RxNavigationReportsFreeAndMissingSourcesWithoutGuessing()
    {
        DanteProject project = LoadRepresentativePreset();
        PatchWorkspaceSession session = new(project.PatchMatrix.Subscriptions);
        PatchNavigationService navigation = new(project, session);

        PatchSourceNavigationResult free = navigation.FindSource(
            Target(project, "DEVICE-C", 1));

        Assert.Equal(PatchNavigationStatus.Free, free.Status);
        Assert.Null(free.Source);

        using TemporaryPreset temporary = MutateRepresentative(document =>
        {
            XElement receiver = Device(document, "DEVICE-B")
                .Elements()
                .Where(element => element.Name.LocalName == "rxchannel")
                .Single(element =>
                    string.Equals(
                        element.Attribute("danteId")?.Value,
                        "1",
                        StringComparison.Ordinal));
            receiver.Elements()
                .Single(element => element.Name.LocalName == "subscribed_device")
                .Value = "DEVICE-ABSENT";
        });
        DanteProject missingProject = DanteProject.Load(temporary.Path);
        PatchNavigationService missingNavigation = new(
            missingProject,
            new PatchWorkspaceSession(missingProject.PatchMatrix.Subscriptions));

        PatchSourceNavigationResult missing = missingNavigation.FindSource(
            Target(missingProject, "DEVICE-B", 1));

        Assert.Equal(PatchNavigationStatus.MissingDevice, missing.Status);
        Assert.Equal("DEVICE-ABSENT", missing.RequestedDeviceName);
        Assert.Null(missing.Source);
    }

    [Fact]
    public void TxNavigationReturnsEveryDestinationInStableDisplayOrder()
    {
        DanteProject project = LoadRepresentativePreset();
        PatchWorkspaceSession session = new(project.PatchMatrix.Subscriptions);
        DanteDevice txDevice = Assert.IsType<DanteDevice>(project.FindDevice("DEVICE-A"));
        PatchSourceDescriptor source = Source(txDevice, 1);
        PatchTargetDescriptor newDestination = Target(project, "DEVICE-C", 1);
        session.Assign(new PlannedPatchAssignment(source, newDestination));
        PatchNavigationService navigation = new(project, session);

        PatchDestinationNavigationResult result = navigation.FindDestinations(source);

        Assert.Equal(PatchNavigationStatus.Found, result.Status);
        Assert.Equal(
            [("DEVICE-A", 1), ("DEVICE-B", 1), ("DEVICE-C", 1)],
            result.Destinations
                .Select(destination => (destination.DeviceName, destination.DanteId))
                .ToArray());
        Assert.Equal(0, result.AmbiguousReferenceCount);
    }

    [Fact]
    public void TxNavigationReportsNoDestination()
    {
        DanteProject project = LoadRepresentativePreset();
        PatchWorkspaceSession session = new(project.PatchMatrix.Subscriptions);
        DanteDevice txDevice = Assert.IsType<DanteDevice>(project.FindDevice("DEVICE-A"));
        PatchNavigationService navigation = new(project, session);

        PatchDestinationNavigationResult result = navigation.FindDestinations(
            Source(txDevice, 2));

        Assert.Equal(PatchNavigationStatus.Found, result.Status);
        PatchTargetDescriptor destination = Assert.Single(result.Destinations);
        Assert.Equal("DEVICE-B", destination.DeviceName);
        Assert.Equal(2, destination.DanteId);

        DanteDevice unusedTxDevice = Assert.IsType<DanteDevice>(project.FindDevice("DEVICE-C"));
        PatchDestinationNavigationResult unused = navigation.FindDestinations(
            Source(unusedTxDevice, 1));
        Assert.Equal(PatchNavigationStatus.NoDestinations, unused.Status);
        Assert.Empty(unused.Destinations);
    }

    [Fact]
    public void AmbiguousTxLabelsNeverProduceAFalseNavigationTarget()
    {
        using TemporaryPreset temporary = MutateRepresentative(document =>
        {
            XElement txDevice = Device(document, "DEVICE-A");
            XElement secondTx = txDevice.Elements()
                .Where(element => element.Name.LocalName == "txchannel")
                .ElementAt(1);
            secondTx.Elements()
                .Single(element => element.Name.LocalName is "label" or "name")
                .Value = "PROGRAM L";
        });
        DanteProject project = DanteProject.Load(temporary.Path);
        PatchWorkspaceSession session = new(project.PatchMatrix.Subscriptions);
        PatchNavigationService navigation = new(project, session);
        DanteDevice txDevice = Assert.IsType<DanteDevice>(project.FindDevice("DEVICE-A"));

        PatchSourceNavigationResult source = navigation.FindSource(
            Target(project, "DEVICE-B", 1));
        PatchDestinationNavigationResult destinations = navigation.FindDestinations(
            Source(txDevice, 1));

        Assert.Equal(PatchNavigationStatus.AmbiguousChannel, source.Status);
        Assert.Null(source.Source);
        Assert.Equal(PatchNavigationStatus.AmbiguousChannel, destinations.Status);
        Assert.Empty(destinations.Destinations);
        Assert.True(destinations.AmbiguousReferenceCount >= 1);
    }

    [Fact]
    public void NavigationQueriesDoNotModifyProjectSessionOrSourceFile()
    {
        string sourcePath = FixturePath();
        byte[] before = File.ReadAllBytes(sourcePath);
        DanteProject project = DanteProject.Load(sourcePath);
        PatchWorkspaceSession session = new(project.PatchMatrix.Subscriptions);
        PatchNavigationService navigation = new(project, session);
        DanteDevice txDevice = Assert.IsType<DanteDevice>(project.FindDevice("DEVICE-A"));

        _ = navigation.FindSource(Target(project, "DEVICE-B", 1));
        _ = navigation.FindDestinations(Source(txDevice, 1));

        Assert.False(project.IsModified);
        Assert.False(session.HasChanges);
        Assert.Equal(before, File.ReadAllBytes(sourcePath));
        Assert.False(project.ValidateXmlChangeGuard().HasErrors);
    }

    private static DanteProject LoadRepresentativePreset() =>
        DanteProject.Load(FixturePath());

    private static string FixturePath() =>
        Path.Combine(AppContext.BaseDirectory, "Fixtures", "representative-preset.xml");

    private static PatchSourceDescriptor Source(DanteDevice device, int danteId)
    {
        DanteChannel channel = Assert.Single(
            device.TxChannels,
            candidate => candidate.DanteId == danteId);
        return new PatchSourceDescriptor(
            device.Name,
            channel.DanteId,
            channel.PositionIndex,
            channel.DisplayName);
    }

    private static PatchTargetDescriptor Target(
        DanteProject project,
        string deviceName,
        int danteId)
    {
        DanteDevice device = Assert.IsType<DanteDevice>(project.FindDevice(deviceName));
        DanteChannel channel = Assert.Single(
            device.RxChannels,
            candidate => candidate.DanteId == danteId);
        return new PatchTargetDescriptor(
            device.Name,
            channel.DanteId,
            channel.PositionIndex,
            channel.DisplayName);
    }

    private static TemporaryPreset MutateRepresentative(
        Action<XDocument> mutation)
    {
        XDocument document = XDocument.Load(
            FixturePath(),
            LoadOptions.PreserveWhitespace);
        mutation(document);
        return TemporaryPreset.Create(document);
    }

    private static XElement Device(XDocument document, string name) =>
        document
            .Descendants()
            .Where(element => element.Name.LocalName == "device")
            .Single(device => device.Elements().Any(element =>
                element.Name.LocalName == "name"
                && string.Equals(
                    element.Value.Trim(),
                    name,
                    StringComparison.OrdinalIgnoreCase)));

    private sealed class TemporaryPreset : IDisposable
    {
        private TemporaryPreset(string root, string path)
        {
            Root = root;
            Path = path;
        }

        public string Root { get; }

        public string Path { get; }

        public static TemporaryPreset Create(XDocument document)
        {
            string root = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "DcePatchNavigationTests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            string path = System.IO.Path.Combine(root, "preset.xml");
            document.Save(path, SaveOptions.DisableFormatting);
            return new TemporaryPreset(root, path);
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(Root, recursive: true);
            }
            catch
            {
                // Le nettoyage du dossier temporaire ne doit pas masquer le test.
            }
        }
    }
}
