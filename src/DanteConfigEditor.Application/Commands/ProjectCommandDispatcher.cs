using DanteConfigEditor.Domain.History;
using DanteConfigEditor.Models;

namespace DanteConfigEditor.Application.Commands;

public sealed class ProjectCommandDispatcher
{
    private readonly ProjectSession _session;
    private readonly int _undoLimit;
    private readonly List<ExecutedTransaction> _undo = [];
    private readonly List<ExecutedTransaction> _redo = [];

    public ProjectCommandDispatcher(ProjectSession session, int undoLimit)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _undoLimit = Math.Clamp(undoLimit, 1, 200);
    }

    public bool CanUndo => _undo.Count > 0;

    public bool CanRedo => _redo.Count > 0;

    public string? NextUndoCommandId => _undo.LastOrDefault()?.CommandId;

    public string? NextRedoCommandId => _redo.LastOrDefault()?.CommandId;

    public CommandPreparation Preview(IProjectCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        EnsureProjectOpen();
        return command.Prepare(_session);
    }

    public CommandExecutionResult Execute(IProjectCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        EnsureProjectOpen();

        CommandPreparation preparation = command.Prepare(_session);
        if (!preparation.CanExecute)
        {
            string details = string.Join(
                Environment.NewLine,
                preparation.Errors.Select(error => $"{error.Code}: {error.TechnicalDetail}"));
            throw new InvalidOperationException(details);
        }

        DanteProject.DanteProjectState before = _session.Project.CaptureState();
        try
        {
            command.Execute(_session);
        }
        catch
        {
            _session.Project.RestoreState(before);
            _session.RefreshValidation();
            throw;
        }

        DanteProject.DanteProjectState after = _session.Project.CaptureState();
        Guid historyId = Guid.NewGuid();
        ProjectHistoryEntry historyEntry = new(
            historyId,
            DateTimeOffset.Now,
            command.Id,
            command.DescriptionKey,
            command.Id,
            preparation.AffectedElements.Count,
            preparation.AffectedElements,
            preparation.Warnings.Select(warning => warning.MessageKey).ToArray());

        _undo.Add(new ExecutedTransaction(
            historyId,
            command.Id,
            before,
            after));
        TrimUndoHistory();
        _redo.Clear();
        _session.AddHistory(historyEntry);
        _session.NotifyModelChanged(command.Id);

        return new CommandExecutionResult(
            historyId,
            command.Id,
            preparation.AffectedElements.Count,
            preparation.Warnings);
    }

    public string Undo()
    {
        EnsureProjectOpen();
        if (_undo.Count == 0)
        {
            throw new InvalidOperationException("Command.Undo.Empty");
        }

        ExecutedTransaction transaction = PopLast(_undo);
        _session.Project.RestoreState(transaction.Before);
        _redo.Add(transaction);
        _session.SetHistoryUndone(transaction.HistoryEntryId, true);
        _session.NotifyUndoRedo(transaction.CommandId);
        return transaction.CommandId;
    }

    public string Redo()
    {
        EnsureProjectOpen();
        if (_redo.Count == 0)
        {
            throw new InvalidOperationException("Command.Redo.Empty");
        }

        ExecutedTransaction transaction = PopLast(_redo);
        _session.Project.RestoreState(transaction.After);
        _undo.Add(transaction);
        TrimUndoHistory();
        _session.SetHistoryUndone(transaction.HistoryEntryId, false);
        _session.NotifyUndoRedo(transaction.CommandId);
        return transaction.CommandId;
    }

    public void Clear()
    {
        _undo.Clear();
        _redo.Clear();
    }

    private void EnsureProjectOpen()
    {
        if (!_session.HasProject)
        {
            throw new InvalidOperationException("Session.Project.Required");
        }
    }

    private void TrimUndoHistory()
    {
        int overflow = _undo.Count - _undoLimit;
        if (overflow > 0)
        {
            _undo.RemoveRange(0, overflow);
        }
    }

    private static ExecutedTransaction PopLast(List<ExecutedTransaction> source)
    {
        int index = source.Count - 1;
        ExecutedTransaction transaction = source[index];
        source.RemoveAt(index);
        return transaction;
    }

    private sealed record ExecutedTransaction(
        Guid HistoryEntryId,
        string CommandId,
        DanteProject.DanteProjectState Before,
        DanteProject.DanteProjectState After);
}
