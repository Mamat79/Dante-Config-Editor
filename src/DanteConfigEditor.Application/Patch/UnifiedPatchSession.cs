using DanteConfigEditor.Application.Commands;
using DanteConfigEditor.Models;
using DanteConfigEditor.Services;

namespace DanteConfigEditor.Application.Patch;

public enum UnifiedPatchSessionChangeKind
{
    PendingChangesChanged,
    Rebased,
    Committed
}

public sealed record UnifiedPatchSessionChangedEventArgs(
    UnifiedPatchSessionChangeKind Kind,
    long Revision,
    int PendingCount);

public sealed record PatchRebaseResult(
    int PreservedCount,
    int DiscardedCount,
    IReadOnlyList<string> Warnings);

/// <summary>
/// Session Patch unique de 2026.1. Les vues continuent à manipuler les
/// descripteurs historiques, mais l'état conservé entre deux reconstructions
/// repose sur les identités stables des machines et les Dante Id des canaux.
/// </summary>
public sealed class UnifiedPatchSession : IPatchWorkspaceSession
{
    private DanteProject _project;
    private DantePatchMatrix _sourceMatrix;
    private PatchWorkspaceSession _workspace;
    private IReadOnlyList<StablePendingPatchEdit> _stablePending = [];

    public UnifiedPatchSession(DanteProject project)
    {
        _project = project ?? throw new ArgumentNullException(nameof(project));
        _sourceMatrix = project.PatchMatrix;
        _workspace = new PatchWorkspaceSession(project.PatchMatrix.Subscriptions);
    }

    public event EventHandler<UnifiedPatchSessionChangedEventArgs>? Changed;

    public long Revision { get; private set; }

    public bool HasChanges => _workspace.HasChanges;

    public int PendingCount => _workspace.PendingCount;

    public IReadOnlyList<PatchEditRequest> Edits => _workspace.Edits;

    public IReadOnlyList<PendingPatchChange> PendingChanges => _workspace.PendingChanges;

    public IReadOnlyList<string> RebaseWarnings { get; private set; } = [];

    public bool IsCurrent(DanteProject project)
    {
        return ReferenceEquals(_project, project)
            && ReferenceEquals(_sourceMatrix, project.PatchMatrix);
    }

    public EffectivePatchAssignment GetEffectiveAssignment(PatchTargetDescriptor target) =>
        _workspace.GetEffectiveAssignment(target);

    public EffectivePatchAssignment GetCommittedAssignment(PatchTargetDescriptor target) =>
        _workspace.GetCommittedAssignment(target);

    public PatchBatchPreview BuildPreview(IEnumerable<PlannedPatchAssignment> assignments) =>
        _workspace.BuildPreview(assignments);

    public PatchBatchPreview BuildCommittedPreview(IEnumerable<PlannedPatchAssignment> assignments) =>
        _workspace.BuildCommittedPreview(assignments);

    public PatchStageResult StagePreview(
        PatchBatchPreview preview,
        PatchConflictResolution conflictResolution)
    {
        PatchStageResult result = _workspace.StagePreview(preview, conflictResolution);
        if (!result.IsCancelled)
        {
            CaptureStablePending();
            RaiseChanged(UnifiedPatchSessionChangeKind.PendingChangesChanged);
        }

        return result;
    }

    public void StageEdits(IEnumerable<PatchEditRequest> edits)
    {
        ArgumentNullException.ThrowIfNull(edits);
        PatchEditRequest[] requested = edits.ToArray();
        if (requested.Length == 0)
        {
            return;
        }

        IReadOnlyList<StablePendingPatchEdit> previous = _stablePending;
        try
        {
            foreach (PatchEditRequest edit in requested)
            {
                PatchTargetDescriptor target = ResolveTarget(edit.RxDeviceName, edit.RxDanteId);
                if (edit.IsRemoval)
                {
                    _workspace.Remove(target);
                    continue;
                }

                PatchSourceDescriptor source = ResolveSource(
                    edit.TxDeviceName!,
                    edit.TxDanteId,
                    edit.TxChannelName);
                _workspace.Assign(new PlannedPatchAssignment(source, target));
            }

            CaptureStablePending();
            RaiseChanged(UnifiedPatchSessionChangeKind.PendingChangesChanged);
        }
        catch
        {
            RestoreStablePending(previous);
            throw;
        }
    }

    public void Remove(PatchTargetDescriptor target)
    {
        _workspace.Remove(target);
        CaptureStablePending();
        RaiseChanged(UnifiedPatchSessionChangeKind.PendingChangesChanged);
    }

    public int RemoveMany(IEnumerable<PatchTargetDescriptor> targets)
    {
        int removed = _workspace.RemoveMany(targets);
        CaptureStablePending();
        RaiseChanged(UnifiedPatchSessionChangeKind.PendingChangesChanged);
        return removed;
    }

    public void Reset()
    {
        if (!_workspace.HasChanges)
        {
            return;
        }

        _workspace.Reset();
        _stablePending = [];
        RebaseWarnings = [];
        RaiseChanged(UnifiedPatchSessionChangeKind.PendingChangesChanged);
    }

    public void RenamePendingSourceChannel(string deviceName, string oldName, string newName)
    {
        _workspace.RenamePendingSourceChannel(deviceName, oldName, newName);
        CaptureStablePending();
        RaiseChanged(UnifiedPatchSessionChangeKind.PendingChangesChanged);
    }

    public PatchRebaseResult Rebase(DanteProject project, bool preservePending = true)
    {
        ArgumentNullException.ThrowIfNull(project);
        IReadOnlyList<StablePendingPatchEdit> requested = preservePending ? _stablePending : [];
        List<PatchEditRequest> translated = [];
        List<StablePendingPatchEdit> preserved = [];
        List<string> warnings = [];

        foreach (StablePendingPatchEdit edit in requested)
        {
            if (!TryTranslate(edit, project, out PatchEditRequest? translatedEdit, out string? warning))
            {
                warnings.Add(warning ?? "Un changement Patch en attente n'existe plus dans le projet.");
                continue;
            }

            translated.Add(translatedEdit!);
            preserved.Add(edit);
        }

        _project = project;
        _sourceMatrix = project.PatchMatrix;
        _workspace = new PatchWorkspaceSession(project.PatchMatrix.Subscriptions, translated);
        _stablePending = preserved;
        RebaseWarnings = warnings;
        RaiseChanged(UnifiedPatchSessionChangeKind.Rebased);
        return new PatchRebaseResult(
            preserved.Count,
            requested.Count - preserved.Count,
            warnings);
    }

    public CommandExecutionResult Commit(ProjectSession projectSession)
    {
        ArgumentNullException.ThrowIfNull(projectSession);
        if (!ReferenceEquals(projectSession.Project, _project))
        {
            throw new InvalidOperationException(
                "La session Patch n'est pas rattachée au projet actif.");
        }

        if (_stablePending.Count == 0)
        {
            throw new InvalidOperationException("Aucun changement Patch à appliquer.");
        }

        ApplyPatchBatchCommand command = new(_stablePending.Select(edit =>
            new StableSubscriptionEdit(
                edit.RxDeviceStableIdentity,
                edit.RxDanteId,
                edit.TxDeviceStableIdentity,
                edit.TxDanteId)));
        CommandExecutionResult result = projectSession.CommandDispatcher.Execute(command);
        Rebase(projectSession.Project, preservePending: false);
        RaiseChanged(UnifiedPatchSessionChangeKind.Committed);
        return result;
    }

    private void CaptureStablePending()
    {
        _stablePending = _workspace.Edits.Select(ToStableEdit).ToArray();
        RebaseWarnings = [];
    }

    private StablePendingPatchEdit ToStableEdit(PatchEditRequest edit)
    {
        DanteDevice rxDevice = _project.FindDevice(edit.RxDeviceName)
            ?? throw new InvalidOperationException(
                $"La machine RX '{edit.RxDeviceName}' n'existe plus.");
        if (edit.IsRemoval)
        {
            return new StablePendingPatchEdit(
                rxDevice.StableIdentity,
                edit.RxDanteId,
                null,
                null);
        }

        DanteDevice txDevice = _project.FindDevice(edit.TxDeviceName!)
            ?? throw new InvalidOperationException(
                $"La machine TX '{edit.TxDeviceName}' n'existe plus.");
        DanteChannel txChannel = ResolveSourceChannel(
            txDevice,
            edit.TxDanteId,
            edit.TxChannelName);
        return new StablePendingPatchEdit(
            rxDevice.StableIdentity,
            edit.RxDanteId,
            txDevice.StableIdentity,
            txChannel.DanteId);
    }

    private void RestoreStablePending(IReadOnlyList<StablePendingPatchEdit> previous)
    {
        _stablePending = previous;
        Rebase(_project, preservePending: true);
    }

    private bool TryTranslate(
        StablePendingPatchEdit edit,
        DanteProject project,
        out PatchEditRequest? translated,
        out string? warning)
    {
        DanteDevice? rxDevice = project.FindDeviceByStableIdentity(edit.RxDeviceStableIdentity);
        if (rxDevice is null
            || rxDevice.RxChannels.All(channel => channel.DanteId != edit.RxDanteId))
        {
            translated = null;
            warning =
                $"Le RX {edit.RxDeviceStableIdentity}:{edit.RxDanteId} a été supprimé ; "
                + "son changement en attente a été écarté.";
            return false;
        }

        if (edit.TxDeviceStableIdentity is null)
        {
            translated = new PatchEditRequest(rxDevice.Name, edit.RxDanteId, null, null);
            warning = null;
            return true;
        }

        DanteDevice? txDevice = project.FindDeviceByStableIdentity(edit.TxDeviceStableIdentity);
        DanteChannel? txChannel = txDevice?.TxChannels
            .FirstOrDefault(channel => channel.DanteId == edit.TxDanteId);
        if (txDevice is null || txChannel is null)
        {
            translated = null;
            warning =
                $"Le TX {edit.TxDeviceStableIdentity}:{edit.TxDanteId} a été supprimé ; "
                + "son changement en attente a été écarté.";
            return false;
        }

        translated = new PatchEditRequest(
            rxDevice.Name,
            edit.RxDanteId,
            txDevice.Name,
            txChannel.DisplayName)
        {
            TxDanteId = txChannel.DanteId
        };
        warning = null;
        return true;
    }

    private PatchTargetDescriptor ResolveTarget(string deviceName, int danteId)
    {
        DanteDevice device = _project.FindDevice(deviceName)
            ?? throw new InvalidOperationException($"La machine RX '{deviceName}' n'existe pas.");
        DanteChannel channel = device.RxChannels.FirstOrDefault(candidate =>
                candidate.DanteId == danteId)
            ?? throw new InvalidOperationException(
                $"Le canal RX {deviceName}:{danteId} n'existe pas.");
        return new PatchTargetDescriptor(
            device.Name,
            channel.DanteId,
            channel.PositionIndex,
            channel.DisplayName);
    }

    private PatchSourceDescriptor ResolveSource(
        string deviceName,
        int? danteId,
        string? channelName)
    {
        DanteDevice device = _project.FindDevice(deviceName)
            ?? throw new InvalidOperationException($"La machine TX '{deviceName}' n'existe pas.");
        DanteChannel channel = ResolveSourceChannel(device, danteId, channelName);
        return new PatchSourceDescriptor(
            device.Name,
            channel.DanteId,
            channel.PositionIndex,
            channel.DisplayName);
    }

    private static DanteChannel ResolveSourceChannel(
        DanteDevice device,
        int? danteId,
        string? channelName)
    {
        if (danteId is int knownDanteId)
        {
            return device.TxChannels.FirstOrDefault(channel =>
                    channel.DanteId == knownDanteId)
                ?? throw new InvalidOperationException(
                    $"Le canal TX {device.Name}:{knownDanteId} n'existe pas.");
        }

        DanteChannel[] matchingNames = device.TxChannels
            .Where(channel => string.Equals(
                channel.DisplayName,
                channelName,
                StringComparison.Ordinal))
            .ToArray();
        return matchingNames.Length switch
        {
            1 => matchingNames[0],
            0 => throw new InvalidOperationException(
                $"Le canal TX '{channelName}' n'existe pas sur {device.Name}."),
            _ => throw new InvalidOperationException(
                $"Le label TX '{channelName}' est ambigu sur {device.Name}.")
        };
    }

    private void RaiseChanged(UnifiedPatchSessionChangeKind kind)
    {
        Revision++;
        Changed?.Invoke(
            this,
            new UnifiedPatchSessionChangedEventArgs(kind, Revision, PendingCount));
    }

    private sealed record StablePendingPatchEdit(
        string RxDeviceStableIdentity,
        int RxDanteId,
        string? TxDeviceStableIdentity,
        int? TxDanteId);
}
