using DanteConfigEditor.Models;

namespace DanteConfigEditor.Services;

/// <summary>
/// Contrat commun aux représentations du Patch. Une matrice, une liste ou
/// Easy Patch peuvent ainsi partager exactement le même lot en attente.
/// </summary>
public interface IPatchWorkspaceSession
{
    bool HasChanges { get; }

    int PendingCount { get; }

    IReadOnlyList<PatchEditRequest> Edits { get; }

    IReadOnlyList<PendingPatchChange> PendingChanges { get; }

    EffectivePatchAssignment GetEffectiveAssignment(PatchTargetDescriptor target);

    EffectivePatchAssignment GetCommittedAssignment(PatchTargetDescriptor target);

    PatchBatchPreview BuildPreview(IEnumerable<PlannedPatchAssignment> assignments);

    PatchBatchPreview BuildCommittedPreview(IEnumerable<PlannedPatchAssignment> assignments);

    PatchStageResult StagePreview(
        PatchBatchPreview preview,
        PatchConflictResolution conflictResolution);

    void Remove(PatchTargetDescriptor target);

    int RemoveMany(IEnumerable<PatchTargetDescriptor> targets);

    void Reset();

    void RenamePendingSourceChannel(string deviceName, string oldName, string newName);
}
