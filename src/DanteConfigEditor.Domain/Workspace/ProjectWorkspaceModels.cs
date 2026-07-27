namespace DanteConfigEditor.Domain.Workspace;

public sealed record ProjectMetadata(
    string Name,
    string Description,
    DateTimeOffset CreatedAt,
    DateTimeOffset ModifiedAt,
    string CreatedWithVersion,
    IReadOnlyDictionary<string, string>? Extensions = null);

public sealed record SynopticNodeLayout(
    string StableDeviceId,
    double X,
    double Y,
    bool IsHidden,
    int Order,
    string Location);

public sealed record ProjectViewSettings(
    string ActivePage,
    bool IsNavigationCollapsed,
    bool IsInspectorCollapsed,
    double InspectorWidth,
    IReadOnlyDictionary<string, string> Filters,
    IReadOnlyDictionary<string, double> ColumnWidths);

public sealed record ProjectWorkspaceData(
    ProjectMetadata Metadata,
    ProjectViewSettings ViewSettings,
    IReadOnlyList<SynopticNodeLayout> SynopticLayout,
    IReadOnlyList<string> Annotations,
    IReadOnlyList<string> HiddenDeviceIds,
    IReadOnlyDictionary<string, string>? Extensions = null);
