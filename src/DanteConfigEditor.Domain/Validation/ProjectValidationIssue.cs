using DanteConfigEditor.Domain.Projects;

namespace DanteConfigEditor.Domain.Validation;

public enum ProjectValidationSeverity
{
    Information,
    Warning,
    Error
}

public sealed record ProjectValidationIssue(
    string Code,
    ProjectValidationSeverity Severity,
    string Category,
    string MessageKey,
    string TechnicalDetail,
    ProjectEntityReference? Target = null,
    string? XmlPath = null,
    string? SuggestedActionKey = null);

public sealed record ProjectValidationState(
    DateTimeOffset ValidatedAt,
    IReadOnlyList<ProjectValidationIssue> Issues)
{
    public static ProjectValidationState Empty { get; } =
        new(DateTimeOffset.MinValue, []);

    public int ErrorCount =>
        Issues.Count(issue => issue.Severity == ProjectValidationSeverity.Error);

    public int WarningCount =>
        Issues.Count(issue => issue.Severity == ProjectValidationSeverity.Warning);

    public bool CanSave => ErrorCount == 0;
}
