using DanteConfigEditor.Models;

namespace DanteConfigEditor.Services;

public sealed record EffectivePatchAssignment(
    string RxDeviceName,
    int RxDanteId,
    string? TxDeviceName,
    string? TxChannelName,
    bool IsPending)
{
    public bool IsActive => !string.IsNullOrWhiteSpace(TxDeviceName);
}

public enum PatchPreviewAction
{
    Create,
    Replace,
    Unchanged
}

public enum PatchConflictResolution
{
    Cancel,
    Skip,
    Replace
}

public sealed record PatchPreviewItem(
    PlannedPatchAssignment Assignment,
    EffectivePatchAssignment Current,
    PatchPreviewAction Action);

public sealed record PatchBatchPreview(IReadOnlyList<PatchPreviewItem> Items)
{
    public int CreateCount => Items.Count(item => item.Action == PatchPreviewAction.Create);

    public int ReplaceCount => Items.Count(item => item.Action == PatchPreviewAction.Replace);

    public int UnchangedCount => Items.Count(item => item.Action == PatchPreviewAction.Unchanged);

    public bool HasConflicts => ReplaceCount > 0;
}

public sealed record PatchStageResult(
    int StagedCount,
    int SkippedConflictCount,
    int UnchangedCount,
    bool IsCancelled);

public sealed record PendingPatchChange(
    string RxDeviceName,
    int RxDanteId,
    string? OriginalTxDeviceName,
    string? OriginalTxChannelName,
    string? DesiredTxDeviceName,
    string? DesiredTxChannelName)
{
    public bool IsCreation => string.IsNullOrWhiteSpace(OriginalTxDeviceName)
        && !string.IsNullOrWhiteSpace(DesiredTxDeviceName);

    public bool IsRemoval => string.IsNullOrWhiteSpace(DesiredTxDeviceName);
}

public sealed class PatchWorkspaceSession
{
    private readonly Dictionary<PatchTargetKey, PatchSourceValue> _originalAssignments;
    private readonly Dictionary<PatchTargetKey, PatchEditRequest> _pendingEdits = new();

    public PatchWorkspaceSession(
        IEnumerable<DanteSubscription> subscriptions,
        IEnumerable<PatchEditRequest>? initialEdits = null)
    {
        ArgumentNullException.ThrowIfNull(subscriptions);

        _originalAssignments = subscriptions.ToDictionary(
            subscription => PatchTargetKey.Create(subscription.RxDevice, subscription.RxDanteId),
            subscription => OriginalSource(subscription));

        if (initialEdits is not null)
        {
            LoadInitialEdits(initialEdits);
        }
    }

    public bool HasChanges => _pendingEdits.Count > 0;

    public int PendingCount => _pendingEdits.Count;

    public IReadOnlyList<PatchEditRequest> Edits => _pendingEdits.Values
        .OrderBy(edit => edit.RxDeviceName, StringComparer.OrdinalIgnoreCase)
        .ThenBy(edit => edit.RxDanteId)
        .ToArray();

    public IReadOnlyList<PendingPatchChange> PendingChanges => _pendingEdits
        .Select(pair =>
        {
            PatchSourceValue original = _originalAssignments[pair.Key];
            PatchEditRequest desired = pair.Value;
            return new PendingPatchChange(
                desired.RxDeviceName,
                desired.RxDanteId,
                original.DeviceName,
                original.ChannelName,
                desired.TxDeviceName,
                desired.TxChannelName);
        })
        .OrderBy(change => change.RxDeviceName, StringComparer.OrdinalIgnoreCase)
        .ThenBy(change => change.RxDanteId)
        .ToArray();

    public EffectivePatchAssignment GetEffectiveAssignment(PatchTargetDescriptor target)
    {
        ArgumentNullException.ThrowIfNull(target);
        PatchTargetKey key = RequireKnownTarget(target);

        if (_pendingEdits.TryGetValue(key, out PatchEditRequest? pending))
        {
            return new EffectivePatchAssignment(
                target.DeviceName,
                target.DanteId,
                pending.TxDeviceName,
                pending.TxChannelName,
                IsPending: true);
        }

        PatchSourceValue original = _originalAssignments[key];
        return new EffectivePatchAssignment(
            target.DeviceName,
            target.DanteId,
            original.DeviceName,
            original.ChannelName,
            IsPending: false);
    }

    public EffectivePatchAssignment GetCommittedAssignment(PatchTargetDescriptor target)
    {
        ArgumentNullException.ThrowIfNull(target);
        PatchTargetKey key = RequireKnownTarget(target);
        PatchSourceValue original = _originalAssignments[key];
        return new EffectivePatchAssignment(
            target.DeviceName,
            target.DanteId,
            original.DeviceName,
            original.ChannelName,
            IsPending: false);
    }

    public void Assign(PlannedPatchAssignment assignment)
    {
        ArgumentNullException.ThrowIfNull(assignment);
        SetDesiredSource(
            assignment.Target,
            assignment.Source.DeviceName,
            assignment.Source.ChannelName);
    }

    public PatchBatchPreview BuildPreview(IEnumerable<PlannedPatchAssignment> assignments)
    {
        return BuildPreview(assignments, GetEffectiveAssignment);
    }

    public PatchBatchPreview BuildCommittedPreview(
        IEnumerable<PlannedPatchAssignment> assignments)
    {
        return BuildPreview(assignments, GetCommittedAssignment);
    }

    private PatchBatchPreview BuildPreview(
        IEnumerable<PlannedPatchAssignment> assignments,
        Func<PatchTargetDescriptor, EffectivePatchAssignment> assignmentResolver)
    {
        ArgumentNullException.ThrowIfNull(assignments);
        ArgumentNullException.ThrowIfNull(assignmentResolver);
        PlannedPatchAssignment[] requested = assignments.ToArray();
        if (requested.Length == 0)
        {
            throw new InvalidOperationException("Aucune affectation à prévisualiser.");
        }

        PatchTargetKey[] keys = requested
            .Select(assignment => RequireKnownTarget(assignment.Target))
            .ToArray();
        if (keys.Distinct().Count() != keys.Length)
        {
            throw new InvalidOperationException("Le même canal RX apparaît plusieurs fois dans le lot.");
        }

        PatchPreviewItem[] items = requested.Select(assignment =>
        {
            EffectivePatchAssignment current = assignmentResolver(assignment.Target);
            PatchPreviewAction action = !current.IsActive
                ? PatchPreviewAction.Create
                : SameSource(
                    new PatchSourceValue(current.TxDeviceName, current.TxChannelName),
                    new PatchSourceValue(assignment.Source.DeviceName, assignment.Source.ChannelName))
                    ? PatchPreviewAction.Unchanged
                    : PatchPreviewAction.Replace;
            return new PatchPreviewItem(assignment, current, action);
        }).ToArray();

        return new PatchBatchPreview(items);
    }

    public PatchStageResult StagePreview(PatchBatchPreview preview, PatchConflictResolution conflictResolution)
    {
        ArgumentNullException.ThrowIfNull(preview);

        // La prévisualisation est recalculée pour éviter d'appliquer un aperçu
        // devenu obsolète après une autre action dans la même fenêtre.
        PatchBatchPreview currentPreview = BuildPreview(preview.Items.Select(item => item.Assignment));
        if (currentPreview.HasConflicts && conflictResolution == PatchConflictResolution.Cancel)
        {
            return new PatchStageResult(0, 0, currentPreview.UnchangedCount, IsCancelled: true);
        }

        Dictionary<PatchTargetKey, PatchEditRequest> snapshot = new(_pendingEdits);
        int staged = 0;
        int skipped = 0;
        try
        {
            foreach (PatchPreviewItem item in currentPreview.Items)
            {
                if (item.Action == PatchPreviewAction.Unchanged)
                {
                    continue;
                }

                if (item.Action == PatchPreviewAction.Replace && conflictResolution == PatchConflictResolution.Skip)
                {
                    skipped++;
                    continue;
                }

                Assign(item.Assignment);
                staged++;
            }
        }
        catch
        {
            _pendingEdits.Clear();
            foreach ((PatchTargetKey key, PatchEditRequest edit) in snapshot)
            {
                _pendingEdits[key] = edit;
            }

            throw;
        }

        return new PatchStageResult(staged, skipped, currentPreview.UnchangedCount, IsCancelled: false);
    }

    public SequentialPatchPlan AssignSequential(
        IReadOnlyList<PatchSourceDescriptor> selectedSources,
        IReadOnlyList<PatchTargetDescriptor> availableTargets,
        PatchTargetDescriptor firstTarget)
    {
        SequentialPatchPlan plan = PatchAssignmentPlanner.PlanSequential(
            selectedSources,
            availableTargets,
            firstTarget);

        foreach (PlannedPatchAssignment assignment in plan.Assignments)
        {
            Assign(assignment);
        }

        return plan;
    }

    public void Remove(PatchTargetDescriptor target)
    {
        ArgumentNullException.ThrowIfNull(target);
        SetDesiredSource(target, null, null);
    }

    public int RemoveMany(IEnumerable<PatchTargetDescriptor> targets)
    {
        ArgumentNullException.ThrowIfNull(targets);
        PatchTargetDescriptor[] requested = targets
            .GroupBy(target => PatchTargetKey.Create(target.DeviceName, target.DanteId))
            .Select(group => group.First())
            .ToArray();
        if (requested.Length == 0)
        {
            throw new InvalidOperationException("Sélectionnez au moins un canal RX à déconnecter.");
        }

        foreach (PatchTargetDescriptor target in requested)
        {
            RequireKnownTarget(target);
        }

        Dictionary<PatchTargetKey, PatchEditRequest> snapshot = new(_pendingEdits);
        try
        {
            foreach (PatchTargetDescriptor target in requested)
            {
                Remove(target);
            }
        }
        catch
        {
            _pendingEdits.Clear();
            foreach ((PatchTargetKey key, PatchEditRequest edit) in snapshot)
            {
                _pendingEdits[key] = edit;
            }

            throw;
        }

        return requested.Length;
    }

    public void Reset()
    {
        _pendingEdits.Clear();
    }

    public void RenamePendingSourceChannel(string deviceName, string oldName, string newName)
    {
        foreach (PatchTargetKey key in _pendingEdits.Keys.ToArray())
        {
            PatchEditRequest edit = _pendingEdits[key];
            if (string.Equals(edit.TxDeviceName, deviceName, StringComparison.OrdinalIgnoreCase)
                && string.Equals(edit.TxChannelName, oldName, StringComparison.Ordinal))
            {
                _pendingEdits[key] = edit with { TxChannelName = newName };
            }
        }
    }

    private void SetDesiredSource(PatchTargetDescriptor target, string? txDeviceName, string? txChannelName)
    {
        PatchTargetKey key = RequireKnownTarget(target);
        PatchSourceValue original = _originalAssignments[key];
        PatchSourceValue desired = new(NormalizeOptional(txDeviceName), NormalizeOptional(txChannelName));

        // Revenir exactement à l'affectation d'origine retire l'édition en
        // attente au lieu de produire une écriture XML inutile.
        if (SameSource(original, desired))
        {
            _pendingEdits.Remove(key);
            return;
        }

        _pendingEdits[key] = string.IsNullOrWhiteSpace(desired.DeviceName)
            ? PatchEditRequest.Remove(target)
            : new PatchEditRequest(
                target.DeviceName,
                target.DanteId,
                desired.DeviceName,
                desired.ChannelName);
    }

    private void LoadInitialEdits(IEnumerable<PatchEditRequest> initialEdits)
    {
        foreach (PatchEditRequest edit in initialEdits)
        {
            PatchTargetDescriptor target = new(edit.RxDeviceName, edit.RxDanteId, 0, string.Empty);
            SetDesiredSource(target, edit.TxDeviceName, edit.TxChannelName);
        }
    }

    private PatchTargetKey RequireKnownTarget(PatchTargetDescriptor target)
    {
        PatchTargetKey key = PatchTargetKey.Create(target.DeviceName, target.DanteId);
        if (!_originalAssignments.ContainsKey(key))
        {
            throw new InvalidOperationException("Le canal RX sélectionné n'existe pas dans le projet chargé.");
        }

        return key;
    }

    private static PatchSourceValue OriginalSource(DanteSubscription subscription)
    {
        if (!subscription.IsActive)
        {
            return new PatchSourceValue(null, null);
        }

        string? deviceName = subscription.IsLocalSubscription
            ? subscription.RxDevice
            : FirstNonEmpty(subscription.ResolvedTxDeviceName, subscription.RawTxDeviceName);

        return new PatchSourceValue(
            NormalizeOptional(deviceName),
            NormalizeOptional(subscription.TxChannelName));
    }

    private static bool SameSource(PatchSourceValue left, PatchSourceValue right)
    {
        return string.Equals(left.DeviceName, right.DeviceName, StringComparison.OrdinalIgnoreCase)
            && string.Equals(left.ChannelName, right.ChannelName, StringComparison.Ordinal);
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private readonly record struct PatchTargetKey(string NormalizedDeviceName, int DanteId)
    {
        public static PatchTargetKey Create(string deviceName, int danteId)
        {
            return new PatchTargetKey(deviceName.Trim().ToUpperInvariant(), danteId);
        }
    }

    private readonly record struct PatchSourceValue(string? DeviceName, string? ChannelName);
}
