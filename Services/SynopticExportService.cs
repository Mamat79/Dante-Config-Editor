using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using DanteConfigEditor.Models;

namespace DanteConfigEditor.Services;

public static class SynopticExportService
{
    private static readonly XNamespace IllustratorNamespace = "http://ns.adobe.com/AdobeIllustrator/10.0/";

    private const double LocationWidth = 300;
    private const double LocationGap = 280;
    private const double NodeWidth = 240;
    private const double MinimumNodeHeight = 82;
    private const double NodeGap = 34;
    private const double ConnectionPortSpacing = 12;
    private const double BaseTopMargin = 88;
    private const double OverheadLaneSpacing = 10;
    private const double LegendSingleWidth = 420;
    private const double LegendDoubleWidth = 760;

    private static readonly string[] Palette =
    [
        "#1677D2", "#0F8B6D", "#C2415A", "#7A5AF8",
        "#B76E00", "#25748A", "#9A4D8C", "#4E7A31"
    ];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static SynopticLayoutDocument LoadOrCreate(DanteProject project, string? storageDirectory = null)
    {
        ArgumentNullException.ThrowIfNull(project);
        string path = ResolveLayoutPath(project, storageDirectory);
        SynopticLayoutDocument layout = new();

        if (File.Exists(path))
        {
            try
            {
                layout = JsonSerializer.Deserialize<SynopticLayoutDocument>(File.ReadAllText(path, Encoding.UTF8), JsonOptions)
                    ?? new SynopticLayoutDocument();
            }
            catch (Exception exception) when (exception is JsonException or IOException or UnauthorizedAccessException)
            {
                layout = new SynopticLayoutDocument();
            }
        }

        return Synchronize(project, layout);
    }

    public static SynopticLayoutDocument Synchronize(DanteProject project, SynopticLayoutDocument? layout)
    {
        ArgumentNullException.ThrowIfNull(project);
        layout ??= new SynopticLayoutDocument();

        Dictionary<string, SynopticDevicePlacement> existing = layout.Devices
            .Where(item => !string.IsNullOrWhiteSpace(item.DeviceIdentity))
            .GroupBy(item => item.DeviceIdentity, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        List<SynopticDevicePlacement> synchronized = [];
        for (int index = 0; index < project.Devices.Count; index++)
        {
            DanteDevice device = project.Devices[index];
            if (existing.TryGetValue(device.StableIdentity, out SynopticDevicePlacement? placement))
            {
                placement.DeviceName = device.Name;
                placement.Order = placement.Order < 0 ? index : placement.Order;
                synchronized.Add(placement);
            }
            else
            {
                synchronized.Add(new SynopticDevicePlacement
                {
                    DeviceIdentity = device.StableIdentity,
                    DeviceName = device.Name,
                    IsVisible = true,
                    Order = index
                });
            }
        }

        layout.SchemaVersion = SynopticLayoutDocument.CurrentSchemaVersion;
        layout.Devices = synchronized;
        return layout;
    }

    public static string SaveLayout(DanteProject project, SynopticLayoutDocument layout, string? storageDirectory = null)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(layout);
        layout = Synchronize(project, layout);
        string path = ResolveLayoutPath(project, storageDirectory);
        string directory = Path.GetDirectoryName(path) ?? throw new InvalidOperationException("Dossier de synoptique introuvable.");
        Directory.CreateDirectory(directory);

        string temporaryPath = Path.Combine(directory, $".{Path.GetFileName(path)}.{Guid.NewGuid():N}.tmp");
        try
        {
            File.WriteAllText(temporaryPath, JsonSerializer.Serialize(layout, JsonOptions) + Environment.NewLine, new UTF8Encoding(false));
            File.Move(temporaryPath, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }

        return path;
    }

    public static SynopticDiagram BuildDiagram(DanteProject project, SynopticLayoutDocument layout, bool english = false)
    {
        ArgumentNullException.ThrowIfNull(project);
        layout = Synchronize(project, layout);

        Dictionary<string, DanteDevice> devicesByName = project.Devices
            .Where(device => !string.IsNullOrWhiteSpace(device.Name))
            .GroupBy(device => device.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        SynopticDevicePlacement[] visiblePlacements = layout.Devices
            .Where(placement => placement.IsVisible && devicesByName.ContainsKey(placement.DeviceName))
            .OrderBy(placement => placement.Location, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(placement => placement.Order)
            .ThenBy(placement => placement.DeviceName, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        string unspecified = english ? "Unspecified location" : "Emplacement non renseigné";
        IGrouping<string, SynopticDevicePlacement>[] locationGroups = visiblePlacements
            .GroupBy(placement => string.IsNullOrWhiteSpace(placement.Location) ? unspecified : placement.Location.Trim(), StringComparer.OrdinalIgnoreCase)
            .OrderBy(group => group.Key == unspecified ? 1 : 0)
            .ThenBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        Dictionary<string, int> locationIndexByDevice = [];
        for (int locationIndex = 0; locationIndex < locationGroups.Length; locationIndex++)
        {
            foreach (SynopticDevicePlacement placement in locationGroups[locationIndex])
            {
                locationIndexByDevice[placement.DeviceName] = locationIndex;
            }
        }

        DanteSubscription[] activeSubscriptions = project.PatchMatrix.Subscriptions
            .Where(subscription => subscription.IsActive)
            .ToArray();
        IGrouping<(string Source, string Target), DanteSubscription>[] subscriptionGroups = activeSubscriptions
            .Where(subscription => locationIndexByDevice.ContainsKey(subscription.ResolvedTxDeviceName)
                && locationIndexByDevice.ContainsKey(subscription.RxDevice))
            .GroupBy(subscription => (Source: subscription.ResolvedTxDeviceName, Target: subscription.RxDevice), DevicePairComparer.Instance)
            .OrderBy(group => group.Key.Source, StringComparer.OrdinalIgnoreCase)
            .ThenBy(group => group.Key.Target, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        Dictionary<string, int> sourceCableCounts = subscriptionGroups
            .GroupBy(group => group.Key.Source, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.OrdinalIgnoreCase);
        Dictionary<string, int> targetCableCounts = subscriptionGroups
            .GroupBy(group => group.Key.Target, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.OrdinalIgnoreCase);

        string[] overheadBundleKeys = subscriptionGroups
            .Where(group => Math.Abs(locationIndexByDevice[group.Key.Source] - locationIndexByDevice[group.Key.Target]) > 1)
            .Select(group => BundleKey(
                locationIndexByDevice[group.Key.Source],
                locationIndexByDevice[group.Key.Target],
                group.Key.Source))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(key => key, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        double topMargin = BaseTopMargin + (overheadBundleKeys.Length == 0 ? 0 : 28 + overheadBundleKeys.Length * OverheadLaneSpacing);

        List<SynopticLocationArea> locations = [];
        List<SynopticDeviceNode> nodes = [];
        for (int locationIndex = 0; locationIndex < locationGroups.Length; locationIndex++)
        {
            IGrouping<string, SynopticDevicePlacement> group = locationGroups[locationIndex];
            SynopticDevicePlacement[] placements = group.OrderBy(item => item.Order).ThenBy(item => item.DeviceName, StringComparer.OrdinalIgnoreCase).ToArray();
            double x = 34 + locationIndex * (LocationWidth + LocationGap);
            string color = Palette[locationIndex % Palette.Length];
            double nodeY = topMargin + 52;

            for (int deviceIndex = 0; deviceIndex < placements.Length; deviceIndex++)
            {
                SynopticDevicePlacement placement = placements[deviceIndex];
                DanteDevice device = devicesByName[placement.DeviceName];
                int connectionCount = Math.Max(
                    sourceCableCounts.GetValueOrDefault(device.Name),
                    targetCableCounts.GetValueOrDefault(device.Name));
                double nodeHeight = Math.Max(MinimumNodeHeight, 28 + connectionCount * ConnectionPortSpacing);
                nodes.Add(new SynopticDeviceNode(
                    placement.DeviceIdentity,
                    device.Name,
                    device.FriendlyName,
                    group.Key,
                    color,
                    device.TxCount,
                    device.RxCount,
                    placement.ManualX ?? x + (LocationWidth - NodeWidth) / 2,
                    placement.ManualY ?? nodeY,
                    NodeWidth,
                    nodeHeight));
                nodeY += nodeHeight + NodeGap;
            }

            double locationHeight = Math.Max(190, nodeY - topMargin - NodeGap + 18);
            locations.Add(new SynopticLocationArea(group.Key, color, x, topMargin, LocationWidth, locationHeight));
        }

        // Une position manuelle reste une donnée graphique du fichier annexe.
        // La zone est agrandie pour garder la machine déplacée dans le cadre sans
        // jamais écrire cette disposition dans le XML Dante.
        for (int index = 0; index < locations.Count; index++)
        {
            SynopticLocationArea location = locations[index];
            SynopticDeviceNode[] locationNodes = nodes
                .Where(node => string.Equals(node.Location, location.Name, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            if (locationNodes.Length == 0)
            {
                continue;
            }

            double left = Math.Min(location.X, locationNodes.Min(node => node.X) - 30);
            double top = Math.Min(location.Y, locationNodes.Min(node => node.Y) - 52);
            double right = Math.Max(location.X + location.Width, locationNodes.Max(node => node.X + node.Width) + 30);
            double bottom = Math.Max(location.Y + location.Height, locationNodes.Max(node => node.Y + node.Height) + 18);
            locations[index] = location with { X = left, Y = top, Width = right - left, Height = bottom - top };
        }

        Dictionary<string, SynopticDeviceNode> nodesByName = nodes
            .GroupBy(node => node.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        int skippedPatchCount = activeSubscriptions.Count(subscription =>
            !nodesByName.ContainsKey(subscription.ResolvedTxDeviceName)
            || !nodesByName.ContainsKey(subscription.RxDevice));

        CableDraft[] drafts = subscriptionGroups
            .Select(group => new CableDraft(
                nodesByName[group.Key.Source],
                nodesByName[group.Key.Target],
                locationIndexByDevice[group.Key.Source],
                locationIndexByDevice[group.Key.Target],
                BuildCableLabels(devicesByName[group.Key.Source], group, english),
                BuildCableSubscriptions(group)))
            .ToArray();

        Dictionary<int, string[]> corridorBundles = drafts
            .Where(draft => Math.Abs(draft.SourceLocationIndex - draft.TargetLocationIndex) <= 1)
            .GroupBy(CorridorIndex)
            .ToDictionary(
                group => group.Key,
                group => group.Select(BundleKey).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(key => key, StringComparer.OrdinalIgnoreCase).ToArray());
        Dictionary<string, int> bundleColorIndices = drafts
            .Select(BundleKey)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(key => key, StringComparer.OrdinalIgnoreCase)
            .Select((key, index) => (key, index))
            .ToDictionary(item => item.key, item => item.index, StringComparer.OrdinalIgnoreCase);
        Dictionary<string, int> overheadLaneIndices = overheadBundleKeys
            .Select((key, index) => (key, index))
            .ToDictionary(item => item.key, item => item.index, StringComparer.OrdinalIgnoreCase);

        List<SynopticCable> cables = [];
        HashSet<string> renderedBidirectionalPairs = new(StringComparer.OrdinalIgnoreCase);
        for (int cableIndex = 0; cableIndex < drafts.Length; cableIndex++)
        {
            CableDraft draft = drafts[cableIndex];
            string bundleKey = BundleKey(draft);
            bool loopback = string.Equals(draft.Source.Name, draft.Target.Name, StringComparison.OrdinalIgnoreCase);
            CableDraft? reverseDraft = loopback
                ? null
                : drafts.FirstOrDefault(candidate =>
                    string.Equals(candidate.Source.Name, draft.Target.Name, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(candidate.Target.Name, draft.Source.Name, StringComparison.OrdinalIgnoreCase));
            bool bidirectional = reverseDraft is not null;
            if (bidirectional && !renderedBidirectionalPairs.Add(UnorderedDevicePairKey(draft.Source.Name, draft.Target.Name)))
            {
                continue;
            }
            double sourceOffset = PortOffset(drafts, draft, draft.Source.Name, source: true);
            double targetOffset = PortOffset(drafts, draft, draft.Target.Name, source: false);
            IReadOnlyList<SynopticRoutePoint> route;
            bool manuallyPositioned = HasManualPosition(layout, draft.Source.Identity) || HasManualPosition(layout, draft.Target.Identity);
            if (manuallyPositioned)
            {
                route = BuildManualRoute(draft, sourceOffset, targetOffset);
            }
            else if (Math.Abs(draft.SourceLocationIndex - draft.TargetLocationIndex) <= 1)
            {
                int corridor = CorridorIndex(draft);
                string[] bundles = corridorBundles[corridor];
                int laneIndex = Array.FindIndex(bundles, key => string.Equals(key, bundleKey, StringComparison.OrdinalIgnoreCase));
                int laneCount = bundles.Length;
                double corridorLeft = locations[corridor].X + locations[corridor].Width;
                double laneX = corridorLeft + 24 + (laneIndex + 1) * (LocationGap - 48) / (laneCount + 1);
                route = BuildCorridorRoute(draft, laneX, sourceOffset, targetOffset, loopback);
            }
            else
            {
                int laneIndex = overheadLaneIndices[bundleKey];
                double laneY = BaseTopMargin + 18 + laneIndex * OverheadLaneSpacing;
                route = BuildOverheadRoute(draft, laneY, sourceOffset, targetOffset, laneIndex);
            }

            SynopticRoutePoint start = route[0];
            SynopticRoutePoint end = route[^1];
            SynopticRoutePoint label = PreferredLabelPoint(route);
            IReadOnlyList<string> labels = bidirectional
                ? draft.Labels.Select(item => $"{draft.Source.Name} → {draft.Target.Name}: {item}")
                    .Concat(reverseDraft!.Labels.Select(item => $"{reverseDraft.Source.Name} → {reverseDraft.Target.Name}: {item}"))
                    .ToArray()
                : draft.Labels;
            IReadOnlyList<SynopticCableSubscription> subscriptions = bidirectional
                ? draft.Subscriptions.Concat(reverseDraft!.Subscriptions).ToArray()
                : draft.Subscriptions;
            cables.Add(new SynopticCable(
                BuildCableStableId(draft, bidirectional),
                draft.Source.Name,
                draft.Target.Name,
                Palette[bundleColorIndices[bundleKey] % Palette.Length],
                labels,
                subscriptions,
                start.X,
                start.Y,
                end.X,
                end.Y,
                label.X,
                label.Y,
                route,
                loopback,
                bidirectional));
        }

        double locationsRight = locations.Count == 0 ? 34 : locations.Max(location => location.X + location.Width);
        double locationsBottom = locations.Count == 0 ? topMargin : locations.Max(location => location.Y + location.Height);
        if (nodes.Count > 0)
        {
            locationsRight = Math.Max(locationsRight, nodes.Max(node => node.X + node.Width));
            locationsBottom = Math.Max(locationsBottom, nodes.Max(node => node.Y + node.Height));
        }
        int legendColumns = cables.Count > 18 ? 2 : 1;
        double legendWidth = legendColumns == 1 ? LegendSingleWidth : LegendDoubleWidth;
        double legendHeight = 52 + LegendRowsHeight(cables, legendColumns);
        double legendX = locationsRight + LocationGap;
        double width = Math.Max(900, legendX + legendWidth + 34);
        double height = Math.Max(560, Math.Max(locationsBottom, topMargin + legendHeight) + 76);
        string title = string.IsNullOrWhiteSpace(project.PresetName)
            ? (english ? "Dante project synoptic" : "Synoptique du projet Dante")
            : project.PresetName;

        return new SynopticDiagram(
            title,
            width,
            height,
            legendX,
            legendWidth,
            legendColumns,
            locations,
            nodes,
            cables,
            layout.Devices.Count(placement => !placement.IsVisible),
            skippedPatchCount);
    }

    public static void ExportSvg(string path, SynopticDiagram diagram, bool english = false)
    {
        ArgumentNullException.ThrowIfNull(diagram);
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string svg = BuildSvg(diagram, english);
        _ = XDocument.Parse(svg, LoadOptions.PreserveWhitespace);
        string temporaryPath = Path.Combine(
            string.IsNullOrWhiteSpace(directory) ? Environment.CurrentDirectory : directory,
            $".{Path.GetFileName(path)}.{Guid.NewGuid():N}.tmp");
        try
        {
            File.WriteAllText(temporaryPath, svg, new UTF8Encoding(false));
            File.Move(temporaryPath, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    public static void ExportPdf(string path, SynopticDiagram diagram, bool english = false)
    {
        SynopticPdfExportService.Export(path, diagram, english);
    }

    public static string BuildSvg(SynopticDiagram diagram, bool english = false)
    {
        ArgumentNullException.ThrowIfNull(diagram);
        string width = Number(diagram.Width);
        string height = Number(diagram.Height);
        XNamespace svgNamespace = "http://www.w3.org/2000/svg";
        XElement root = new(svgNamespace + "svg",
            new XAttribute(XNamespace.Xmlns + "i", IllustratorNamespace),
            new XAttribute("width", width),
            new XAttribute("height", height),
            new XAttribute("viewBox", $"0 0 {width} {height}"),
            new XAttribute("version", "1.1"),
            new XAttribute("role", "img"),
            new XAttribute("shape-rendering", "geometricPrecision"),
            new XElement(svgNamespace + "title", diagram.Title));

        XElement definitions = new(svgNamespace + "defs");
        Dictionary<string, string> markerByColor = diagram.Cables
            .Select(cable => cable.Color)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select((color, index) => (color, id: $"arrow-{index + 1}"))
            .ToDictionary(item => item.color, item => item.id, StringComparer.OrdinalIgnoreCase);
        foreach (KeyValuePair<string, string> marker in markerByColor)
        {
            definitions.Add(new XElement(svgNamespace + "marker",
                new XAttribute("id", marker.Value),
                new XAttribute("markerWidth", "8"),
                new XAttribute("markerHeight", "6"),
                new XAttribute("refX", "7"),
                new XAttribute("refY", "3"),
                new XAttribute("orient", "auto"),
                new XAttribute("markerUnits", "strokeWidth"),
                new XElement(svgNamespace + "path",
                    new XAttribute("d", "M0,0 L8,3 L0,6 Z"),
                    new XAttribute("fill", marker.Key))));
        }
        root.Add(definitions);

        XElement backgroundLayer = Layer(svgNamespace, "layer-background", "Background");
        backgroundLayer.Add(
            SvgRect(svgNamespace, 0, 0, diagram.Width, diagram.Height, "#F7F9FC"),
            SvgText(svgNamespace, 34, 38, diagram.Title, 24, "#172033", bold: true),
            SvgText(svgNamespace, 34, 62,
                english ? "Grouped Dante subscriptions - offline export" : "Abonnements Dante regroupés - export hors ligne",
                12, "#526070"));

        XElement locationsLayer = Layer(svgNamespace, "layer-locations", "Locations");
        foreach (SynopticLocationArea location in diagram.Locations)
        {
            locationsLayer.Add(
                SvgRect(svgNamespace, location.X, location.Y, location.Width, location.Height, "#FFFFFF", location.Color, 2, 8),
                SvgRect(svgNamespace, location.X, location.Y, location.Width, 36, location.Color, radius: 7),
                SvgText(svgNamespace, location.X + 14, location.Y + 24, location.Name, 14, "#FFFFFF", bold: true));
        }

        XElement cablesLayer = Layer(svgNamespace, "layer-cables", "Cables");
        foreach (SynopticCable cable in diagram.Cables)
        {
            string path = BuildCablePath(cable);
            cablesLayer.Add(
                SvgPath(svgNamespace, path, "#FFFFFF", 8, opacity: 0.96),
                SvgPath(svgNamespace, path, cable.Color, 3.5, opacity: 0.92,
                    markerEnd: $"url(#{markerByColor[cable.Color]})",
                    markerStart: cable.IsBidirectional ? $"url(#{markerByColor[cable.Color]})" : null));
        }

        XElement devicesLayer = Layer(svgNamespace, "layer-devices", "Devices");
        foreach (SynopticDeviceNode node in diagram.Devices)
        {
            devicesLayer.Add(
                SvgRect(svgNamespace, node.X, node.Y, node.Width, node.Height, "#FFFFFF", node.Color, 2, 6),
                SvgRect(svgNamespace, node.X, node.Y, 8, node.Height, node.Color, radius: 4),
                SvgText(svgNamespace, node.X + 20, node.Y + 29, node.Name, 16, "#172033", bold: true));
            if (!string.IsNullOrWhiteSpace(node.FriendlyName)
                && !string.Equals(node.FriendlyName, node.Name, StringComparison.OrdinalIgnoreCase))
            {
                devicesLayer.Add(SvgText(svgNamespace, node.X + 20, node.Y + 49, Trim(node.FriendlyName, 32), 11, "#526070"));
            }
            devicesLayer.Add(SvgText(svgNamespace, node.X + 20, node.Y + 68, $"TX {node.TxCount}   RX {node.RxCount}", 11, "#526070"));
        }

        XElement labelsLayer = Layer(svgNamespace, "layer-labels", "Labels and legend");
        foreach (SynopticCable cable in diagram.Cables)
        {
            IReadOnlyList<SynopticRoutePoint> points = CablePoints(cable);
            if (points.Count < 2)
            {
                continue;
            }

            labelsLayer.Add(SvgArrowHead(svgNamespace, points[^1], points[^2], cable.Color, "end"));
            if (cable.IsBidirectional)
            {
                labelsLayer.Add(SvgArrowHead(svgNamespace, points[0], points[1], cable.Color, "start"));
            }
        }
        if (diagram.Cables.Count <= 18)
        {
            for (int index = 0; index < diagram.Cables.Count; index++)
            {
                SynopticCable cable = diagram.Cables[index];
                labelsLayer.Add(
                    new XElement(svgNamespace + "circle",
                        new XAttribute("cx", Number(cable.LabelX)),
                        new XAttribute("cy", Number(cable.LabelY)),
                        new XAttribute("r", "9"),
                        new XAttribute("fill", "#FFFFFF"),
                        new XAttribute("stroke", cable.Color),
                        new XAttribute("stroke-width", "2")),
                    SvgText(svgNamespace, cable.LabelX, cable.LabelY + 3, (index + 1).ToString(CultureInfo.InvariantCulture), 8, "#172033", bold: true, anchor: "middle"));
            }
        }

        AppendLegend(labelsLayer, svgNamespace, diagram, english);

        string summary = english
            ? $"{diagram.Devices.Count} devices shown - {diagram.Cables.Count} grouped cables - {diagram.HiddenDeviceCount} hidden - {diagram.SkippedPatchCount} subscriptions outside selection"
            : $"{diagram.Devices.Count} machines affichées - {diagram.Cables.Count} câbles regroupés - {diagram.HiddenDeviceCount} masquée(s) - {diagram.SkippedPatchCount} patch(s) hors sélection";
        labelsLayer.Add(
            SvgText(svgNamespace, 34, diagram.Height - 30, summary, 11, "#526070"),
            SvgText(svgNamespace, diagram.Width - 34, diagram.Height - 30,
                "Dante Config Editor V3.6 - By Mamat et ses agents  -------[]--", 10, "#718096", anchor: "end"));

        root.Add(backgroundLayer, locationsLayer, cablesLayer, devicesLayer, labelsLayer);
        XDocument document = new(new XDeclaration("1.0", "UTF-8", null), root);
        string svg = document.ToString(SaveOptions.DisableFormatting);
        _ = XDocument.Parse(svg);
        return svg + Environment.NewLine;
    }

    public static string ResolveLayoutPath(DanteProject project, string? storageDirectory = null)
    {
        string directory = string.IsNullOrWhiteSpace(storageDirectory)
            ? ApplicationStoragePaths.Resolve("Synoptics")
            : Path.GetFullPath(storageDirectory);
        string fingerprintSource = string.Join("\n", project.Devices.Select(device => device.StableIdentity).OrderBy(value => value, StringComparer.OrdinalIgnoreCase));
        if (string.IsNullOrWhiteSpace(fingerprintSource))
        {
            fingerprintSource = Path.GetFullPath(project.OriginalFilePath).ToUpperInvariant();
        }

        string hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(fingerprintSource))).ToLowerInvariant()[..20];
        return Path.Combine(directory, $"{hash}.synoptic.json");
    }

    private static IReadOnlyList<string> BuildCableLabels(DanteDevice sourceDevice, IEnumerable<DanteSubscription> subscriptions, bool english)
    {
        Dictionary<string, int> txIdsByName = sourceDevice.TxChannels
            .Where(channel => !string.IsNullOrWhiteSpace(channel.DisplayName))
            .GroupBy(channel => channel.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First().DanteId, StringComparer.OrdinalIgnoreCase);

        CablePoint[] points = subscriptions
            .Select(subscription => new CablePoint(
                txIdsByName.TryGetValue(subscription.TxChannelName, out int txId) ? txId : null,
                subscription.TxChannelName,
                subscription.RxDanteId))
            .OrderBy(point => point.RxId)
            .ToArray();

        List<CableRange> ranges = [];
        foreach (CablePoint point in points)
        {
            CableRange? current = ranges.LastOrDefault();
            bool consecutive = current is not null
                && point.RxId == current.RxEnd + 1
                && point.TxId.HasValue
                && current.TxEnd.HasValue
                && point.TxId.Value == current.TxEnd.Value + 1;
            if (consecutive)
            {
                current!.RxEnd = point.RxId;
                current.TxEnd = point.TxId;
                current.Count++;
            }
            else
            {
                ranges.Add(new CableRange(point.TxId, point.TxId, point.TxName, point.RxId, point.RxId, 1));
            }
        }

        return ranges.Select(range => FormatRange(range, english)).ToArray();
    }

    private static IReadOnlyList<SynopticCableSubscription> BuildCableSubscriptions(
        IEnumerable<DanteSubscription> subscriptions)
    {
        return subscriptions
            .OrderBy(subscription => subscription.RxDanteId)
            .Select(subscription => new SynopticCableSubscription(
                subscription.ResolvedTxDeviceName,
                subscription.TxDanteId,
                subscription.TxChannelName,
                subscription.RxDevice,
                subscription.RxDanteId,
                subscription.RxChannelName,
                subscription.Kind,
                subscription.Status))
            .ToArray();
    }

    private static string BuildCableStableId(CableDraft draft, bool bidirectional)
    {
        if (!bidirectional)
        {
            return $"synoptic-link:{draft.Source.Identity}>{draft.Target.Identity}";
        }

        return string.Compare(
                draft.Source.Identity,
                draft.Target.Identity,
                StringComparison.OrdinalIgnoreCase) <= 0
            ? $"synoptic-link:{draft.Source.Identity}<>{draft.Target.Identity}"
            : $"synoptic-link:{draft.Target.Identity}<>{draft.Source.Identity}";
    }

    private static string FormatRange(CableRange range, bool english)
    {
        string tx = range.TxStart.HasValue
            ? FormatNumberRange("TX", range.TxStart.Value, range.TxEnd ?? range.TxStart.Value)
            : $"TX {Trim(range.TxName, 18)}";
        string rx = FormatNumberRange("RX", range.RxStart, range.RxEnd);
        string count = english ? $"{range.Count} ch" : $"{range.Count} can.";
        return $"{tx} → {rx}  ({count})";
    }

    private static string FormatNumberRange(string prefix, int start, int end)
    {
        return start == end ? $"{prefix} {start}" : $"{prefix} {start}-{end}";
    }

    private static int CorridorIndex(CableDraft draft)
    {
        return draft.SourceLocationIndex == draft.TargetLocationIndex
            ? draft.SourceLocationIndex
            : Math.Min(draft.SourceLocationIndex, draft.TargetLocationIndex);
    }

    private static string BundleKey(CableDraft draft)
    {
        return BundleKey(draft.SourceLocationIndex, draft.TargetLocationIndex, draft.Source.Name);
    }

    private static string BundleKey(int sourceLocationIndex, int targetLocationIndex, string sourceDevice)
    {
        return $"{sourceLocationIndex}>{targetLocationIndex}:{sourceDevice}";
    }

    private static string UnorderedDevicePairKey(string first, string second)
    {
        return string.Compare(first, second, StringComparison.OrdinalIgnoreCase) <= 0
            ? $"{first}\u001f{second}"
            : $"{second}\u001f{first}";
    }

    private static double PortOffset(CableDraft[] drafts, CableDraft current, string deviceName, bool source)
    {
        CableDraft[] related = drafts
            .Where(draft => string.Equals(
                source ? draft.Source.Name : draft.Target.Name,
                deviceName,
                StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (related.Length <= 1)
        {
            return 0;
        }

        int index = Array.IndexOf(related, current);
        SynopticDeviceNode node = source ? current.Source : current.Target;
        double availableSpread = Math.Max(0, node.Height - 24);
        double spread = Math.Min(availableSpread, (related.Length - 1) * ConnectionPortSpacing);
        return -spread / 2 + index * spread / (related.Length - 1);
    }

    private static IReadOnlyList<SynopticRoutePoint> BuildCorridorRoute(
        CableDraft draft,
        double laneX,
        double sourceOffset,
        double targetOffset,
        bool loopback)
    {
        SynopticDeviceNode source = draft.Source;
        SynopticDeviceNode target = draft.Target;
        if (loopback)
        {
            double loopStartY = source.Y + source.Height / 2 - 14;
            double loopEndY = source.Y + source.Height / 2 + 14;
            return
            [
                new SynopticRoutePoint(source.X + source.Width, loopStartY),
                new SynopticRoutePoint(laneX, loopStartY),
                new SynopticRoutePoint(laneX, loopEndY),
                new SynopticRoutePoint(source.X + source.Width, loopEndY)
            ];
        }

        bool sameLocation = draft.SourceLocationIndex == draft.TargetLocationIndex;
        bool forward = draft.SourceLocationIndex < draft.TargetLocationIndex;
        double startX = sameLocation || forward ? source.X + source.Width : source.X;
        double endX = sameLocation || !forward ? target.X + target.Width : target.X;
        double startY = source.Y + source.Height / 2 + sourceOffset;
        double endY = target.Y + target.Height / 2 + targetOffset;
        return
        [
            new SynopticRoutePoint(startX, startY),
            new SynopticRoutePoint(laneX, startY),
            new SynopticRoutePoint(laneX, endY),
            new SynopticRoutePoint(endX, endY)
        ];
    }

    private static bool HasManualPosition(SynopticLayoutDocument layout, string identity)
    {
        SynopticDevicePlacement? placement = layout.Devices.FirstOrDefault(item =>
            string.Equals(item.DeviceIdentity, identity, StringComparison.OrdinalIgnoreCase));
        return placement is { ManualX: not null, ManualY: not null };
    }

    private static IReadOnlyList<SynopticRoutePoint> BuildManualRoute(
        CableDraft draft,
        double sourceOffset,
        double targetOffset)
    {
        SynopticDeviceNode source = draft.Source;
        SynopticDeviceNode target = draft.Target;
        if (string.Equals(source.Name, target.Name, StringComparison.OrdinalIgnoreCase))
        {
            double loopLaneX = source.X + source.Width + 42;
            return BuildCorridorRoute(draft, loopLaneX, sourceOffset, targetOffset, loopback: true);
        }

        bool forward = source.X + source.Width / 2 <= target.X + target.Width / 2;
        double startX = forward ? source.X + source.Width : source.X;
        double endX = forward ? target.X : target.X + target.Width;
        double startY = source.Y + source.Height / 2 + sourceOffset;
        double endY = target.Y + target.Height / 2 + targetOffset;
        double laneX = (startX + endX) / 2;
        return
        [
            new SynopticRoutePoint(startX, startY),
            new SynopticRoutePoint(laneX, startY),
            new SynopticRoutePoint(laneX, endY),
            new SynopticRoutePoint(endX, endY)
        ];
    }

    private static IReadOnlyList<SynopticRoutePoint> BuildOverheadRoute(
        CableDraft draft,
        double laneY,
        double sourceOffset,
        double targetOffset,
        int laneIndex)
    {
        bool forward = draft.SourceLocationIndex < draft.TargetLocationIndex;
        double direction = forward ? 1 : -1;
        double startX = forward ? draft.Source.X + draft.Source.Width : draft.Source.X;
        double endX = forward ? draft.Target.X : draft.Target.X + draft.Target.Width;
        double startY = draft.Source.Y + draft.Source.Height / 2 + sourceOffset;
        double endY = draft.Target.Y + draft.Target.Height / 2 + targetOffset;
        double sourceGutterX = startX + direction * (20 + (laneIndex % 4) * 4);
        double targetGutterX = endX - direction * (20 + ((laneIndex + 2) % 4) * 4);
        return
        [
            new SynopticRoutePoint(startX, startY),
            new SynopticRoutePoint(sourceGutterX, startY),
            new SynopticRoutePoint(sourceGutterX, laneY),
            new SynopticRoutePoint(targetGutterX, laneY),
            new SynopticRoutePoint(targetGutterX, endY),
            new SynopticRoutePoint(endX, endY)
        ];
    }

    private static SynopticRoutePoint PreferredLabelPoint(IReadOnlyList<SynopticRoutePoint> route)
    {
        if (route.Count == 4)
        {
            return Midpoint(route[2], route[3]);
        }
        if (route.Count >= 6)
        {
            return Midpoint(route[2], route[3]);
        }

        double longest = double.MinValue;
        SynopticRoutePoint midpoint = route[0];
        for (int index = 1; index < route.Count; index++)
        {
            SynopticRoutePoint first = route[index - 1];
            SynopticRoutePoint second = route[index];
            double length = Math.Abs(second.X - first.X) + Math.Abs(second.Y - first.Y);
            if (length > longest)
            {
                longest = length;
                midpoint = new SynopticRoutePoint((first.X + second.X) / 2, (first.Y + second.Y) / 2);
            }
        }
        return midpoint;
    }

    private static SynopticRoutePoint Midpoint(SynopticRoutePoint first, SynopticRoutePoint second)
    {
        return new SynopticRoutePoint((first.X + second.X) / 2, (first.Y + second.Y) / 2);
    }

    private static string BuildCablePath(SynopticCable cable)
    {
        if (cable.RoutePoints.Count == 0)
        {
            return $"M {Number(cable.StartX)} {Number(cable.StartY)} L {Number(cable.EndX)} {Number(cable.EndY)}";
        }

        return "M " + string.Join(" L ", cable.RoutePoints.Select(point => $"{Number(point.X)} {Number(point.Y)}"));
    }

    private static IReadOnlyList<SynopticRoutePoint> CablePoints(SynopticCable cable)
    {
        return cable.RoutePoints.Count == 0
            ? [new SynopticRoutePoint(cable.StartX, cable.StartY), new SynopticRoutePoint(cable.EndX, cable.EndY)]
            : cable.RoutePoints;
    }

    private static XElement SvgArrowHead(
        XNamespace svgNamespace,
        SynopticRoutePoint tip,
        SynopticRoutePoint previous,
        string fill,
        string direction)
    {
        double dx = tip.X - previous.X;
        double dy = tip.Y - previous.Y;
        double length = Math.Sqrt(dx * dx + dy * dy);
        if (length < 0.01)
        {
            return new XElement(svgNamespace + "g");
        }

        dx /= length;
        dy /= length;
        double backX = tip.X - dx * 12;
        double backY = tip.Y - dy * 12;
        double perpendicularX = -dy * 5;
        double perpendicularY = dx * 5;
        string points = string.Join(" ",
            $"{Number(tip.X)},{Number(tip.Y)}",
            $"{Number(backX + perpendicularX)},{Number(backY + perpendicularY)}",
            $"{Number(backX - perpendicularX)},{Number(backY - perpendicularY)}");
        return new XElement(svgNamespace + "polygon",
            new XAttribute("points", points),
            new XAttribute("fill", fill),
            new XAttribute("data-arrow", direction));
    }

    private static XElement Layer(XNamespace svgNamespace, string id, string name)
    {
        // Illustrator exige cet attribut propriétaire pour rouvrir un groupe comme un vrai calque.
        return new XElement(svgNamespace + "g",
            new XAttribute("id", id),
            new XAttribute("data-name", name),
            new XAttribute(IllustratorNamespace + "layer", "yes"));
    }

    private static XElement SvgRect(
        XNamespace svgNamespace,
        double x,
        double y,
        double width,
        double height,
        string fill,
        string? stroke = null,
        double strokeWidth = 0,
        double radius = 0)
    {
        XElement rectangle = new(svgNamespace + "rect",
            new XAttribute("x", Number(x)),
            new XAttribute("y", Number(y)),
            new XAttribute("width", Number(width)),
            new XAttribute("height", Number(height)),
            new XAttribute("fill", fill));
        if (radius > 0)
        {
            rectangle.Add(new XAttribute("rx", Number(radius)));
        }
        if (!string.IsNullOrWhiteSpace(stroke))
        {
            rectangle.Add(new XAttribute("stroke", stroke), new XAttribute("stroke-width", Number(strokeWidth)));
        }
        return rectangle;
    }

    private static XElement SvgText(
        XNamespace svgNamespace,
        double x,
        double y,
        string text,
        double fontSize,
        string fill,
        bool bold = false,
        string? anchor = null)
    {
        XElement element = new(svgNamespace + "text",
            new XAttribute("x", Number(x)),
            new XAttribute("y", Number(y)),
            new XAttribute("font-family", "Arial, sans-serif"),
            new XAttribute("font-size", Number(fontSize)),
            new XAttribute("fill", fill),
            text);
        if (bold)
        {
            element.Add(new XAttribute("font-weight", "700"));
        }
        if (!string.IsNullOrWhiteSpace(anchor))
        {
            element.Add(new XAttribute("text-anchor", anchor));
        }
        return element;
    }

    private static XElement SvgPath(
        XNamespace svgNamespace,
        string data,
        string stroke,
        double strokeWidth,
        double opacity,
        string? markerEnd = null,
        string? markerStart = null)
    {
        XElement path = new(svgNamespace + "path",
            new XAttribute("d", data),
            new XAttribute("fill", "none"),
            new XAttribute("stroke", stroke),
            new XAttribute("stroke-width", Number(strokeWidth)),
            new XAttribute("stroke-linecap", "round"),
            new XAttribute("stroke-linejoin", "round"),
            new XAttribute("opacity", Number(opacity)));
        if (!string.IsNullOrWhiteSpace(markerEnd))
        {
            path.Add(new XAttribute("marker-end", markerEnd));
        }
        if (!string.IsNullOrWhiteSpace(markerStart))
        {
            path.Add(new XAttribute("marker-start", markerStart));
        }
        return path;
    }

    private static void AppendLegend(XElement layer, XNamespace svgNamespace, SynopticDiagram diagram, bool english)
    {
        double topMargin = diagram.Locations.Count == 0 ? BaseTopMargin : diagram.Locations.Min(location => location.Y);
        double legendX = diagram.LegendX;
        double legendHeight = diagram.Height - topMargin - 76;
        int columns = Math.Max(1, diagram.LegendColumns);
        double gap = 10;
        double itemWidth = (diagram.LegendWidth - 24 - (columns - 1) * gap) / columns;
        layer.Add(
            SvgRect(svgNamespace, legendX, topMargin, diagram.LegendWidth, legendHeight, "#FFFFFF", "#CBD5E1", 1.5, 8),
            SvgText(svgNamespace, legendX + 18, topMargin + 29,
                english ? "Grouped subscriptions" : "Liaisons regroupées", 15, "#172033", bold: true));

        double y = topMargin + 48;
        for (int rowStart = 0; rowStart < diagram.Cables.Count; rowStart += columns)
        {
            SynopticCable[] row = diagram.Cables.Skip(rowStart).Take(columns).ToArray();
            double rowHeight = row.Max(LegendItemHeight);
            for (int column = 0; column < row.Length; column++)
            {
                int index = rowStart + column;
                SynopticCable cable = row[column];
                double itemX = legendX + 12 + column * (itemWidth + gap);
                layer.Add(
                    SvgRect(svgNamespace, itemX, y, itemWidth, rowHeight - 6, "#F8FAFC", "#E2E8F0", 1, 5),
                    new XElement(svgNamespace + "circle",
                        new XAttribute("cx", Number(itemX + 19)),
                        new XAttribute("cy", Number(y + 20)),
                        new XAttribute("r", "11"),
                        new XAttribute("fill", cable.Color)),
                    SvgText(svgNamespace, itemX + 19, y + 24, (index + 1).ToString(CultureInfo.InvariantCulture), 10, "#FFFFFF", bold: true, anchor: "middle"),
                    SvgText(svgNamespace, itemX + 38, y + 23,
                        Trim($"{cable.SourceDevice} {(cable.IsBidirectional ? "↔" : "→")} {cable.TargetDevice}", columns == 1 ? 52 : 38), 12, "#172033", bold: true));
                for (int labelIndex = 0; labelIndex < cable.Labels.Count; labelIndex++)
                {
                    layer.Add(SvgText(svgNamespace, itemX + 38, y + 42 + labelIndex * 16,
                        Trim(cable.Labels[labelIndex], columns == 1 ? 58 : 42), 10.5, "#526070"));
                }
            }
            y += rowHeight;
        }
    }

    private static double LegendRowsHeight(IReadOnlyList<SynopticCable> cables, int columns)
    {
        double height = 0;
        int safeColumns = Math.Max(1, columns);
        for (int rowStart = 0; rowStart < cables.Count; rowStart += safeColumns)
        {
            height += cables.Skip(rowStart).Take(safeColumns).Max(LegendItemHeight);
        }
        return height;
    }

    private static double LegendItemHeight(SynopticCable cable)
    {
        return Math.Max(62, 50 + cable.Labels.Count * 16);
    }

    private static string Number(double value) => value.ToString("0.##", CultureInfo.InvariantCulture);

    private static string Trim(string value, int maximumLength)
    {
        string clean = string.IsNullOrWhiteSpace(value) ? "?" : value.Trim();
        return clean.Length <= maximumLength ? clean : clean[..Math.Max(1, maximumLength - 1)] + "…";
    }

    private sealed record CablePoint(int? TxId, string TxName, int RxId);

    private sealed record CableDraft(
        SynopticDeviceNode Source,
        SynopticDeviceNode Target,
        int SourceLocationIndex,
        int TargetLocationIndex,
        IReadOnlyList<string> Labels,
        IReadOnlyList<SynopticCableSubscription> Subscriptions);

    private sealed class CableRange(int? txStart, int? txEnd, string txName, int rxStart, int rxEnd, int count)
    {
        public int? TxStart { get; } = txStart;
        public int? TxEnd { get; set; } = txEnd;
        public string TxName { get; } = txName;
        public int RxStart { get; } = rxStart;
        public int RxEnd { get; set; } = rxEnd;
        public int Count { get; set; } = count;
    }

    private sealed class DevicePairComparer : IEqualityComparer<(string Source, string Target)>
    {
        public static DevicePairComparer Instance { get; } = new();

        public bool Equals((string Source, string Target) left, (string Source, string Target) right)
        {
            return string.Equals(left.Source, right.Source, StringComparison.OrdinalIgnoreCase)
                && string.Equals(left.Target, right.Target, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode((string Source, string Target) value)
        {
            return HashCode.Combine(
                StringComparer.OrdinalIgnoreCase.GetHashCode(value.Source),
                StringComparer.OrdinalIgnoreCase.GetHashCode(value.Target));
        }
    }
}
