namespace DanteConfigEditor.Models;

public sealed class SynopticLayoutDocument
{
    public const int CurrentSchemaVersion = 2;

    public int SchemaVersion { get; set; } = CurrentSchemaVersion;

    public List<SynopticDevicePlacement> Devices { get; set; } = [];
}

public sealed class SynopticDevicePlacement
{
    public string DeviceIdentity { get; set; } = string.Empty;

    public string DeviceName { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public bool IsVisible { get; set; } = true;

    public int Order { get; set; }

    public double? ManualX { get; set; }

    public double? ManualY { get; set; }
}

public sealed record SynopticDiagram(
    string Title,
    double Width,
    double Height,
    double LegendX,
    double LegendWidth,
    int LegendColumns,
    IReadOnlyList<SynopticLocationArea> Locations,
    IReadOnlyList<SynopticDeviceNode> Devices,
    IReadOnlyList<SynopticCable> Cables,
    int HiddenDeviceCount,
    int SkippedPatchCount);

public sealed record SynopticLocationArea(
    string Name,
    string Color,
    double X,
    double Y,
    double Width,
    double Height);

public sealed record SynopticDeviceNode(
    string Identity,
    string Name,
    string FriendlyName,
    string Location,
    string Color,
    int TxCount,
    int RxCount,
    double X,
    double Y,
    double Width,
    double Height);

public sealed record SynopticCable(
    string StableId,
    string SourceDevice,
    string TargetDevice,
    string Color,
    IReadOnlyList<string> Labels,
    IReadOnlyList<SynopticCableSubscription> Subscriptions,
    double StartX,
    double StartY,
    double EndX,
    double EndY,
    double LabelX,
    double LabelY,
    IReadOnlyList<SynopticRoutePoint> RoutePoints,
    bool IsLoopback,
    bool IsBidirectional)
{
    public bool HasError => Subscriptions.Any(subscription =>
        subscription.Kind == DanteSubscriptionKind.Conflict);

    public bool HasWarning => Subscriptions.Any(subscription =>
        subscription.Kind is DanteSubscriptionKind.ExternalMissingDevice
            or DanteSubscriptionKind.MissingChannel);
}

public sealed record SynopticCableSubscription(
    string SourceDevice,
    int? TxDanteId,
    string TxChannelName,
    string TargetDevice,
    int RxDanteId,
    string RxChannelName,
    DanteSubscriptionKind Kind,
    string Status);

public sealed record SynopticRoutePoint(double X, double Y);
