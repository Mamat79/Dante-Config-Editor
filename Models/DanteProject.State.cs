using System.Xml.Linq;
using DanteConfigEditor.Services;

namespace DanteConfigEditor.Models;

public sealed partial class DanteProject
{
    public DanteProjectState CaptureState()
    {
        XDocument snapshot = new(Document);
        MachineRoleIdentityService.PairEquivalentDocuments(Document, snapshot);
        return new DanteProjectState(
            snapshot,
            OriginalFilePath,
            LastSavedPath,
            IsModified,
            _changes.ToArray(),
            CaptureModifiedRxReferences(),
            CaptureAuthorizedDeviceAdditions());
    }

    public void RestoreState(DanteProjectState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        ReplaceCurrentDocument(new XDocument(state.Document));
        MachineRoleIdentityService.PairEquivalentDocuments(state.Document, Document);
        OriginalFilePath = state.OriginalFilePath;
        LastSavedPath = state.LastSavedPath;
        IsModified = state.WasModified;

        _changes.Clear();
        _changes.AddRange(state.Changes);
        _modifiedRxElements.Clear();
        RestoreAuthorizedDeviceAdditions(state.AuthorizedDeviceAdditions);
        ReloadModel();
        RestoreModifiedRxReferences(state.ModifiedRxReferences);
        ReloadModel();
    }

    public DanteDevice? FindDeviceByStableIdentity(string? stableIdentity)
    {
        if (string.IsNullOrWhiteSpace(stableIdentity))
        {
            return null;
        }

        return _devicesByStableIdentity.GetValueOrDefault(stableIdentity);
    }

    public sealed class DanteProjectState
    {
        internal DanteProjectState(
            XDocument document,
            string originalFilePath,
            string? lastSavedPath,
            bool wasModified,
            IReadOnlyList<ChangeRecord> changes,
            IReadOnlyList<ModifiedRxReference> modifiedRxReferences,
            IReadOnlyList<AuthorizedDeviceAdditionState> authorizedDeviceAdditions)
        {
            Document = document;
            OriginalFilePath = originalFilePath;
            LastSavedPath = lastSavedPath;
            WasModified = wasModified;
            Changes = changes;
            ModifiedRxReferences = modifiedRxReferences;
            AuthorizedDeviceAdditions = authorizedDeviceAdditions;
        }

        internal XDocument Document { get; }

        internal string OriginalFilePath { get; }

        internal string? LastSavedPath { get; }

        internal bool WasModified { get; }

        internal IReadOnlyList<ChangeRecord> Changes { get; }

        internal IReadOnlyList<ModifiedRxReference> ModifiedRxReferences { get; }

        internal IReadOnlyList<AuthorizedDeviceAdditionState> AuthorizedDeviceAdditions { get; }
    }
}
