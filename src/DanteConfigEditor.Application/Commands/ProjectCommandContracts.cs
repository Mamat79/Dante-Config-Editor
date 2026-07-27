using DanteConfigEditor.Domain.Projects;

namespace DanteConfigEditor.Application.Commands;

public sealed record CommandProblem(
    string Code,
    string MessageKey,
    string TechnicalDetail);

public sealed record CommandPreparation(
    string CommandId,
    string DescriptionKey,
    IReadOnlyList<ProjectEntityReference> AffectedElements,
    IReadOnlyList<CommandProblem> Warnings,
    IReadOnlyList<CommandProblem> Errors)
{
    public bool CanExecute => Errors.Count == 0;
}

public sealed record CommandExecutionResult(
    Guid HistoryEntryId,
    string CommandId,
    int AffectedElementCount,
    IReadOnlyList<CommandProblem> Warnings);

public interface IProjectCommand
{
    string Id { get; }

    string DescriptionKey { get; }

    CommandPreparation Prepare(ProjectSession session);

    void Execute(ProjectSession session);
}
