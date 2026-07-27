namespace DanteConfigEditor.Domain.Projects;

public enum ProjectEntityKind
{
    Project,
    Device,
    TxChannel,
    RxChannel,
    Subscription,
    SynopticLink,
    MachineTemplate,
    ValidationIssue
}

public sealed record ProjectEntityReference(
    ProjectEntityKind Kind,
    string StableId,
    string DisplayName,
    string? ParentStableId = null);

public sealed record ProjectSelection(IReadOnlyList<ProjectEntityReference> Items)
{
    public static ProjectSelection Empty { get; } = new([]);

    public ProjectEntityReference? Primary => Items.FirstOrDefault();

    public bool IsMultiple => Items.Count > 1;
}
