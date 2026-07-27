using System.Xml.Linq;
using DanteConfigEditor.Models;
using DanteConfigEditor.Services;
using DanteConfigEditorV3.TestSupport;

namespace DanteConfigEditorV3.Tests;

public sealed class SynopticExportTests
{
    [Fact]
    public void DevicesAreSortedByLocationThenExplicitOrder()
    {
        using TestDirectory directory = new();
        string source = directory.PathFor("ordered.xml");
        SyntheticPresetFactory.Create(source, deviceCount: 4, txPerDevice: 2, rxPerDevice: 2);
        DanteProject project = DanteProject.Load(source);
        SynopticLayoutDocument layout = SynopticExportService.LoadOrCreate(project, directory.PathFor("layout"));
        SetLocation(layout, "DEVICE-001", "Scène");
        SetLocation(layout, "DEVICE-002", "Régie");
        SetLocation(layout, "DEVICE-003", "Scène");
        SetLocation(layout, "DEVICE-004", "Régie");
        layout.Devices.Single(item => item.DeviceName == "DEVICE-001").Order = 20;
        layout.Devices.Single(item => item.DeviceName == "DEVICE-003").Order = 10;
        layout.Devices.Single(item => item.DeviceName == "DEVICE-002").Order = 20;
        layout.Devices.Single(item => item.DeviceName == "DEVICE-004").Order = 10;

        SynopticDiagram diagram = SynopticExportService.BuildDiagram(project, layout);

        Assert.Equal(["Régie", "Scène"], diagram.Locations.Select(location => location.Name));
        Assert.Equal(
            ["DEVICE-004", "DEVICE-002", "DEVICE-003", "DEVICE-001"],
            diagram.Devices.OrderBy(device => device.X).ThenBy(device => device.Y).Select(device => device.Name));
    }

    [Fact]
    public void ManualDeviceCoordinatesArePersistedAndUsedByTheDiagram()
    {
        using TestDirectory directory = new();
        string source = directory.PathFor("manual-layout.xml");
        string layoutDirectory = directory.PathFor("layout");
        SyntheticPresetFactory.Create(source, deviceCount: 2, txPerDevice: 2, rxPerDevice: 2);
        DanteProject project = DanteProject.Load(source);
        SynopticLayoutDocument layout = SynopticExportService.LoadOrCreate(project, layoutDirectory);
        SynopticDevicePlacement moved = layout.Devices.Single(item => item.DeviceName == "DEVICE-002");
        moved.ManualX = 777;
        moved.ManualY = 333;

        SynopticExportService.SaveLayout(project, layout, layoutDirectory);
        SynopticLayoutDocument reloaded = SynopticExportService.LoadOrCreate(project, layoutDirectory);
        SynopticDiagram diagram = SynopticExportService.BuildDiagram(project, reloaded);
        SynopticDeviceNode node = diagram.Devices.Single(item => item.Name == "DEVICE-002");

        Assert.Equal(777, node.X);
        Assert.Equal(333, node.Y);
    }

    [Fact]
    public void ConsecutiveSubscriptionsAreCollapsedIntoOneCableRange()
    {
        using TestDirectory directory = new();
        string source = directory.PathFor("synoptic.xml");
        SyntheticPresetFactory.Create(source, deviceCount: 2, txPerDevice: 32, rxPerDevice: 32);
        DanteProject project = DanteProject.Load(source);
        SynopticLayoutDocument layout = SynopticExportService.LoadOrCreate(project, directory.PathFor("layout"));
        SetLocation(layout, "DEVICE-001", "Régie");
        SetLocation(layout, "DEVICE-002", "Scène");

        SynopticDiagram diagram = SynopticExportService.BuildDiagram(project, layout);

        SynopticCable cable = Assert.Single(diagram.Cables, item =>
            item.SourceDevice == "DEVICE-001" && item.TargetDevice == "DEVICE-002");
        Assert.Single(cable.Labels);
        Assert.Equal("TX 1-32 → RX 1-32  (32 can.)", cable.Labels[0]);
        Assert.Equal(32, cable.Subscriptions.Count);
        Assert.StartsWith("synoptic-link:", cable.StableId, StringComparison.Ordinal);
        Assert.Equal(2, diagram.Locations.Count);
        AssertOrthogonal(cable.RoutePoints);
        Assert.True(diagram.LegendX > diagram.Locations.Max(location => location.X + location.Width));
    }

    [Fact]
    public void ReciprocalSubscriptionsUseOneCableWithArrowsAtBothEnds()
    {
        using TestDirectory directory = new();
        string source = directory.PathFor("bidirectional.xml");
        SyntheticPresetFactory.Create(source, deviceCount: 2, txPerDevice: 4, rxPerDevice: 4);
        DanteProject project = DanteProject.Load(source);
        project.ApplyPatch("DEVICE-001", 1, "DEVICE-002", "TX-01");
        SynopticLayoutDocument layout = SynopticExportService.LoadOrCreate(project, directory.PathFor("layout"));
        SetLocation(layout, "DEVICE-001", "Régie");
        SetLocation(layout, "DEVICE-002", "Scène");

        SynopticDiagram diagram = SynopticExportService.BuildDiagram(project, layout);
        SynopticCable cable = Assert.Single(diagram.Cables, item =>
            !item.IsLoopback
            && new[] { item.SourceDevice, item.TargetDevice }.ToHashSet(StringComparer.OrdinalIgnoreCase)
                .SetEquals(["DEVICE-001", "DEVICE-002"]));
        string svg = SynopticExportService.BuildSvg(diagram);

        Assert.True(cable.IsBidirectional);
        Assert.Contains(cable.Labels, label => label.StartsWith("DEVICE-001 → DEVICE-002", StringComparison.Ordinal));
        Assert.Contains(cable.Labels, label => label.StartsWith("DEVICE-002 → DEVICE-001", StringComparison.Ordinal));
        Assert.Contains("marker-start=", svg, StringComparison.Ordinal);
        Assert.Contains("marker-end=", svg, StringComparison.Ordinal);
        Assert.Contains("data-arrow=\"start\"", svg, StringComparison.Ordinal);
        Assert.Contains("data-arrow=\"end\"", svg, StringComparison.Ordinal);
        Assert.Contains(cable.Subscriptions, item =>
            item.SourceDevice == "DEVICE-001"
            && item.TargetDevice == "DEVICE-002");
        Assert.Contains(cable.Subscriptions, item =>
            item.SourceDevice == "DEVICE-002"
            && item.TargetDevice == "DEVICE-001");
    }

    [Fact]
    public void PatchSubscriptionAndSynopticCableShareOneStableSelection()
    {
        using TestDirectory directory = new();
        string source = directory.PathFor("selection.xml");
        SyntheticPresetFactory.Create(source, deviceCount: 3, txPerDevice: 4, rxPerDevice: 4);
        DanteProject project = DanteProject.Load(source);
        SynopticLayoutDocument layout = SynopticExportService.LoadOrCreate(
            project,
            directory.PathFor("layout"));
        SynopticDiagram diagram = SynopticExportService.BuildDiagram(project, layout);
        DanteSubscription subscription = project.PatchMatrix.Subscriptions.First(item =>
            item.IsActive
            && !item.IsLocalSubscription
            && !item.IsWarning);

        SynopticCable? cable = SynopticSelectionService.FindCable(
            diagram,
            subscription);

        Assert.NotNull(cable);
        Assert.Contains(cable!.Subscriptions, item =>
            item.RxDanteId == subscription.RxDanteId
            && item.TargetDevice == subscription.RxDevice);
        Assert.Same(
            cable,
            SynopticSelectionService.FindCable(
                diagram,
                subscription.RxDevice,
                subscription.RxDanteId));
    }

    [Fact]
    public void RoutedCablesUseGuttersAndAvoidUnrelatedDeviceCards()
    {
        using TestDirectory directory = new();
        string source = directory.PathFor("routed.xml");
        SyntheticPresetFactory.Create(source, deviceCount: 4, txPerDevice: 4, rxPerDevice: 4);
        DanteProject project = DanteProject.Load(source);
        project.ApplyPatch("DEVICE-004", 1, "DEVICE-001", "TX-01");
        SynopticLayoutDocument layout = SynopticExportService.LoadOrCreate(project, directory.PathFor("layout"));
        SetLocation(layout, "DEVICE-001", "Régie");
        SetLocation(layout, "DEVICE-002", "Rack A");
        SetLocation(layout, "DEVICE-003", "Plateau");
        SetLocation(layout, "DEVICE-004", "Scène");

        SynopticDiagram diagram = SynopticExportService.BuildDiagram(project, layout);

        Assert.All(diagram.Cables, cable => AssertOrthogonal(cable.RoutePoints));
        Assert.Contains(diagram.Cables, cable => cable.RoutePoints.Count == 6);
        foreach (SynopticCable cable in diagram.Cables)
        {
            foreach (SynopticDeviceNode node in diagram.Devices.Where(node =>
                         !string.Equals(node.Name, cable.SourceDevice, StringComparison.OrdinalIgnoreCase)
                         && !string.Equals(node.Name, cable.TargetDevice, StringComparison.OrdinalIgnoreCase)))
            {
                Assert.False(RouteIntersectsInterior(cable.RoutePoints, node), $"{cable.SourceDevice} -> {cable.TargetDevice} traverse {node.Name}");
            }
        }
    }

    [Fact]
    public void SeveralTargetsFromOneSourceShareOneCableTrunk()
    {
        using TestDirectory directory = new();
        string source = directory.PathFor("bundled.xml");
        SyntheticPresetFactory.Create(source, deviceCount: 3, txPerDevice: 4, rxPerDevice: 4);
        DanteProject project = DanteProject.Load(source);
        project.ApplyPatch("DEVICE-003", 1, "DEVICE-001", "TX-01");
        SynopticLayoutDocument layout = SynopticExportService.LoadOrCreate(project, directory.PathFor("layout"));
        SetLocation(layout, "DEVICE-001", "Régie");
        SetLocation(layout, "DEVICE-002", "Plateau");
        SetLocation(layout, "DEVICE-003", "Plateau");

        SynopticDiagram diagram = SynopticExportService.BuildDiagram(project, layout);
        SynopticCable first = Assert.Single(diagram.Cables, cable => cable.SourceDevice == "DEVICE-001" && cable.TargetDevice == "DEVICE-002");
        SynopticCable second = Assert.Single(diagram.Cables, cable => cable.SourceDevice == "DEVICE-001" && cable.TargetDevice == "DEVICE-003");

        Assert.Equal(first.RoutePoints[1].X, second.RoutePoints[1].X, precision: 3);
        Assert.Equal(first.Color, second.Color);
        Assert.NotEqual(first.LabelY, second.LabelY);
    }

    [Fact]
    public void DenseLegendUsesTwoColumns()
    {
        using TestDirectory directory = new();
        string source = directory.PathFor("dense-legend.xml");
        SyntheticPresetFactory.Create(source, deviceCount: 20, txPerDevice: 2, rxPerDevice: 2);
        DanteProject project = DanteProject.Load(source);
        SynopticLayoutDocument layout = SynopticExportService.LoadOrCreate(project, directory.PathFor("layout"));
        for (int index = 0; index < layout.Devices.Count; index++)
        {
            layout.Devices[index].Location = $"Zone {index + 1}";
        }

        SynopticDiagram diagram = SynopticExportService.BuildDiagram(project, layout);

        Assert.True(diagram.Cables.Count > 18);
        Assert.Equal(2, diagram.LegendColumns);
        Assert.True(diagram.LegendWidth > 420);
    }

    [Fact]
    public void DenseIncomingCablesUseDistinctReadablePorts()
    {
        using TestDirectory directory = new();
        string source = directory.PathFor("dense-ports.xml");
        SyntheticPresetFactory.Create(source, deviceCount: 12, txPerDevice: 16, rxPerDevice: 16);
        DanteProject project = DanteProject.Load(source);
        for (int index = 1; index <= 10; index++)
        {
            project.ApplyPatch("DEVICE-012", index, $"DEVICE-{index:D3}", "TX-01");
        }

        SynopticLayoutDocument layout = SynopticExportService.LoadOrCreate(project, directory.PathFor("layout"));
        foreach (SynopticDevicePlacement placement in layout.Devices)
        {
            placement.Location = placement.DeviceName == "DEVICE-012" ? "Destination" : "Sources";
        }

        SynopticDiagram diagram = SynopticExportService.BuildDiagram(project, layout);
        SynopticCable[] incoming = diagram.Cables
            .Where(cable => cable.TargetDevice == "DEVICE-012")
            .OrderBy(cable => cable.EndY)
            .ToArray();
        SynopticDeviceNode target = diagram.Devices.Single(device => device.Name == "DEVICE-012");

        Assert.True(incoming.Length >= 10);
        Assert.True(target.Height > 82);
        Assert.Equal(incoming.Length, incoming.Select(cable => Math.Round(cable.EndY, 3)).Distinct().Count());
        Assert.All(incoming.Zip(incoming.Skip(1)), pair => Assert.True(pair.Second.EndY - pair.First.EndY >= 11.9));
    }

    [Fact]
    public void LayoutAndSvgExportNeverModifyTheDanteXml()
    {
        using TestDirectory directory = new();
        string source = directory.PathFor("source.xml");
        string svgPath = directory.PathFor("exports", "synoptic.svg");
        string pdfPath = directory.PathFor("exports", "synoptic.pdf");
        SyntheticPresetFactory.Create(source, deviceCount: 3, txPerDevice: 8, rxPerDevice: 8);
        DanteProject project = DanteProject.Load(source);
        string before = project.Document.ToString(SaveOptions.DisableFormatting);

        SynopticLayoutDocument layout = SynopticExportService.LoadOrCreate(project, directory.PathFor("layout"));
        SetLocation(layout, "DEVICE-001", "FOH");
        SetLocation(layout, "DEVICE-002", "Plateau");
        layout.Devices.Single(item => item.DeviceName == "DEVICE-003").IsVisible = false;
        string sidecar = SynopticExportService.SaveLayout(project, layout, directory.PathFor("layout"));
        SynopticDiagram diagram = SynopticExportService.BuildDiagram(project, layout);
        SynopticExportService.ExportSvg(svgPath, diagram);
        SynopticExportService.ExportPdf(pdfPath, diagram);

        Assert.Equal(before, project.Document.ToString(SaveOptions.DisableFormatting));
        Assert.False(project.IsModified);
        Assert.EndsWith(".synoptic.json", sidecar, StringComparison.OrdinalIgnoreCase);
        Assert.True(File.Exists(sidecar));
        string svg = File.ReadAllText(svgPath);
        Assert.Contains("<svg", svg, StringComparison.Ordinal);
        Assert.Contains("FOH", svg, StringComparison.Ordinal);
        Assert.Contains("Plateau", svg, StringComparison.Ordinal);
        Assert.Contains("By Mamat et ses agents", svg, StringComparison.Ordinal);
        Assert.DoesNotContain("DEVICE-003</text>", svg, StringComparison.Ordinal);
        byte[] pdf = File.ReadAllBytes(pdfPath);
        Assert.True(pdf.Length > 1_000);
        Assert.Equal("%PDF-1.4", System.Text.Encoding.ASCII.GetString(pdf, 0, 8));
        Assert.EndsWith("%%EOF", System.Text.Encoding.ASCII.GetString(pdf), StringComparison.Ordinal);
    }

    [Fact]
    public void SvgIsValidXmlWithStableNamedLayersAndOneDefinitionsBlock()
    {
        using TestDirectory directory = new();
        string source = directory.PathFor("layered-svg.xml");
        SyntheticPresetFactory.Create(source, deviceCount: 6, txPerDevice: 8, rxPerDevice: 8);
        DanteProject project = DanteProject.Load(source);
        SynopticLayoutDocument layout = SynopticExportService.LoadOrCreate(project, directory.PathFor("layout"));

        string svg = SynopticExportService.BuildSvg(SynopticExportService.BuildDiagram(project, layout));
        XDocument document = XDocument.Parse(svg, LoadOptions.PreserveWhitespace);
        XNamespace svgNamespace = "http://www.w3.org/2000/svg";
        XNamespace illustratorNamespace = "http://ns.adobe.com/AdobeIllustrator/10.0/";
        XElement[] layers = document
            .Descendants(svgNamespace + "g")
            .Where(element => ((string?)element.Attribute("id"))?.StartsWith("layer-", StringComparison.Ordinal) == true)
            .ToArray();

        Assert.Single(document.Descendants(svgNamespace + "defs"));
        Assert.Equal(
            ["layer-background", "layer-locations", "layer-cables", "layer-devices", "layer-labels"],
            layers.Select(element => (string)element.Attribute("id")!).ToArray());
        Assert.All(layers, layer => Assert.Equal("yes", (string?)layer.Attribute(illustratorNamespace + "layer")));
        Assert.Equal(
            document.Descendants().Attributes("id").Select(attribute => attribute.Value).Count(),
            document.Descendants().Attributes("id").Select(attribute => attribute.Value).Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void StableIdentityKeepsLocationAfterDeviceRename()
    {
        using TestDirectory directory = new();
        string source = directory.PathFor("rename.xml");
        SyntheticPresetFactory.Create(source, deviceCount: 1, txPerDevice: 2, rxPerDevice: 2);
        DanteProject project = DanteProject.Load(source);
        SynopticLayoutDocument layout = SynopticExportService.LoadOrCreate(project, directory.PathFor("layout"));
        SetLocation(layout, "DEVICE-001", "Studio A");
        string identity = layout.Devices[0].DeviceIdentity;

        project.RenameDevice("DEVICE-001", "STAGEBOX-A");
        SynopticLayoutDocument synchronized = SynopticExportService.Synchronize(project, layout);

        SynopticDevicePlacement placement = Assert.Single(synchronized.Devices);
        Assert.Equal(identity, placement.DeviceIdentity);
        Assert.Equal("STAGEBOX-A", placement.DeviceName);
        Assert.Equal("Studio A", placement.Location);
    }

    [Fact]
    public void CorruptedSidecarFallsBackWithoutTouchingTheProject()
    {
        using TestDirectory directory = new();
        string source = directory.PathFor("corrupted-layout.xml");
        string layoutDirectory = directory.PathFor("layout");
        SyntheticPresetFactory.Create(source, deviceCount: 2, txPerDevice: 2, rxPerDevice: 2);
        DanteProject project = DanteProject.Load(source);
        string before = project.Document.ToString(SaveOptions.DisableFormatting);
        string layoutPath = SynopticExportService.ResolveLayoutPath(project, layoutDirectory);
        Directory.CreateDirectory(layoutDirectory);
        File.WriteAllText(layoutPath, "{not valid json", System.Text.Encoding.UTF8);

        SynopticLayoutDocument layout = SynopticExportService.LoadOrCreate(project, layoutDirectory);

        Assert.Equal(2, layout.Devices.Count);
        Assert.All(layout.Devices, placement => Assert.True(placement.IsVisible));
        Assert.Equal(before, project.Document.ToString(SaveOptions.DisableFormatting));
        Assert.False(project.IsModified);
    }

    private static void SetLocation(SynopticLayoutDocument layout, string deviceName, string location)
    {
        layout.Devices.Single(item => item.DeviceName == deviceName).Location = location;
    }

    private static void AssertOrthogonal(IReadOnlyList<SynopticRoutePoint> points)
    {
        Assert.True(points.Count >= 2);
        for (int index = 1; index < points.Count; index++)
        {
            Assert.True(
                Math.Abs(points[index - 1].X - points[index].X) < 0.01
                || Math.Abs(points[index - 1].Y - points[index].Y) < 0.01,
                $"Segment diagonal entre {points[index - 1]} et {points[index]}");
        }
    }

    private static bool RouteIntersectsInterior(IReadOnlyList<SynopticRoutePoint> points, SynopticDeviceNode node)
    {
        double left = node.X + 1;
        double right = node.X + node.Width - 1;
        double top = node.Y + 1;
        double bottom = node.Y + node.Height - 1;
        for (int index = 1; index < points.Count; index++)
        {
            SynopticRoutePoint first = points[index - 1];
            SynopticRoutePoint second = points[index];
            if (Math.Abs(first.X - second.X) < 0.01)
            {
                double minY = Math.Min(first.Y, second.Y);
                double maxY = Math.Max(first.Y, second.Y);
                if (first.X > left && first.X < right && maxY > top && minY < bottom)
                {
                    return true;
                }
            }
            else
            {
                double minX = Math.Min(first.X, second.X);
                double maxX = Math.Max(first.X, second.X);
                if (first.Y > top && first.Y < bottom && maxX > left && minX < right)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private sealed class TestDirectory : IDisposable
    {
        public TestDirectory()
        {
            Root = Path.Combine(Path.GetTempPath(), "DanteConfigEditorV3Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Root);
        }

        public string Root { get; }

        public string PathFor(params string[] parts)
        {
            return Path.Combine([Root, .. parts]);
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(Root, recursive: true);
            }
            catch
            {
            }
        }
    }
}
