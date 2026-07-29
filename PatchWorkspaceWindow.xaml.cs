using System.Windows;
using DanteConfigEditor.Models;
using DanteConfigEditor.Services;

namespace DanteConfigEditor;

public partial class PatchWorkspaceWindow : Window
{
    private readonly PatchWorkspaceView _workspace;
    private readonly bool _immediateMode;

    public PatchWorkspaceWindow(
        UiLanguage language,
        DanteProject project,
        bool useLightTheme,
        string? initialTxDeviceName = null,
        string? initialRxDeviceName = null,
        IEnumerable<PatchEditRequest>? initialEdits = null,
        bool returnEditsOnly = false,
        bool lockRxDeviceSelection = false,
        bool immediateMode = false,
        Func<string, DanteChannelKind, int, string, bool>? renameChannelAction = null,
        Func<string, DanteChannelKind, IReadOnlyList<int>, int, bool>? extendChannelSeriesAction = null,
        bool warnOnExistingPatch = true)
    {
        InitializeComponent();
        _immediateMode = immediateMode;
        PatchWorkspaceView? workspace = null;
        bool RenameAndReload(
            string deviceName,
            DanteChannelKind kind,
            int channelIndex,
            string newName)
        {
            bool changed = renameChannelAction?.Invoke(
                deviceName,
                kind,
                channelIndex,
                newName) == true;
            if (changed)
            {
                Dispatcher.BeginInvoke(new Action(
                    () => workspace?.ReloadCommittedProjectState()));
            }

            return changed;
        }

        bool ExtendAndReload(
            string deviceName,
            DanteChannelKind kind,
            IReadOnlyList<int> seedDanteIds,
            int targetDanteId)
        {
            bool changed = extendChannelSeriesAction?.Invoke(
                deviceName,
                kind,
                seedDanteIds,
                targetDanteId) == true;
            if (changed)
            {
                Dispatcher.BeginInvoke(new Action(
                    () => workspace?.ReloadCommittedProjectState()));
            }

            return changed;
        }

        workspace = new PatchWorkspaceView(
            language,
            project,
            useLightTheme,
            initialTxDeviceName,
            initialRxDeviceName,
            initialEdits,
            returnEditsOnly,
            lockRxDeviceSelection,
            embedded: immediateMode,
            renameChannelAction: renameChannelAction is null
                ? null
                : RenameAndReload,
            extendChannelSeriesAction: extendChannelSeriesAction is null
                ? null
                : ExtendAndReload,
            startInAssignmentMode: false,
            warnOnExistingPatch: warnOnExistingPatch,
            allowDetach: false);
        _workspace = workspace;
        _workspace.ApplyRequested += Workspace_ApplyRequested;
        _workspace.CancelRequested += Workspace_CancelRequested;
        _workspace.DirectApplyRequested += Workspace_DirectApplyRequested;
        _workspace.DeviceFocusChanged += Workspace_DeviceFocusChanged;
        WorkspaceHost.Content = _workspace;
        if (_immediateMode)
        {
            _workspace.ShowMatrixMode();
            Title = language == UiLanguage.English
                ? "Detached patch matrix"
                : "Matrice de patch détachée";
        }
    }

    public IReadOnlyList<PatchEditRequest> Edits => _workspace.Edits;

    public event EventHandler<DirectPatchRequestEventArgs>? DirectApplyRequested;

    public event EventHandler<PatchDeviceFocusChangedEventArgs>? DeviceFocusChanged;

    private void Workspace_ApplyRequested(object? sender, EventArgs e)
    {
        if (!_immediateMode)
        {
            DialogResult = true;
        }
    }

    private void Workspace_CancelRequested(object? sender, EventArgs e)
    {
        if (_immediateMode)
        {
            Close();
        }
        else
        {
            DialogResult = false;
        }
    }

    private void Workspace_DirectApplyRequested(
        object? sender,
        DirectPatchRequestEventArgs e)
    {
        DirectApplyRequested?.Invoke(sender ?? _workspace, e);
        Dispatcher.BeginInvoke(
            new Action(_workspace.ReloadCommittedProjectState));
    }

    private void Workspace_DeviceFocusChanged(
        object? sender,
        PatchDeviceFocusChangedEventArgs e)
    {
        DeviceFocusChanged?.Invoke(sender ?? _workspace, e);
    }
}
