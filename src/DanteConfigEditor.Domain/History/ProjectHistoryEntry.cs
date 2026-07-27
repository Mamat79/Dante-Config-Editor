using DanteConfigEditor.Domain.Projects;

namespace DanteConfigEditor.Domain.History;

public sealed record ProjectHistoryEntry(
    Guid Id,
    DateTimeOffset Timestamp,
    string CommandId,
    string DescriptionKey,
    string Summary,
    int AffectedElementCount,
    IReadOnlyList<ProjectEntityReference> AffectedElements,
    IReadOnlyList<string> Warnings,
    bool WasUndone = false);
