using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using DanteConfigEditor.Models;
using DanteConfigEditor.Services;

namespace DanteConfigEditor;

public sealed class InlineChannelNavigationRequestEventArgs(
    DanteChannelKind kind,
    int danteId,
    bool matrix) : EventArgs
{
    public DanteChannelKind Kind { get; } = kind;

    public int DanteId { get; } = danteId;

    public bool Matrix { get; } = matrix;
}

public sealed class DirectPatchRequestEventArgs(
    IReadOnlyList<PatchEditRequest> edits) : EventArgs
{
    public IReadOnlyList<PatchEditRequest> Edits { get; } =
        edits ?? throw new ArgumentNullException(nameof(edits));
}

public sealed class PatchDeviceFocusChangedEventArgs(
    string deviceName,
    DanteChannelKind kind) : EventArgs
{
    public string DeviceName { get; } = deviceName;

    public DanteChannelKind Kind { get; } = kind;
}

public sealed record PatchMatrixOneToOneState(
    string CountText,
    string? TxDeviceName,
    int? TxDanteId,
    string? RxDeviceName,
    int? RxDanteId);

public partial class PatchWorkspaceView : UserControl
{
    private const double MinimumMatrixZoom = 0.5;
    private const double MaximumMatrixZoom = 2.0;
    private const double MatrixZoomStep = 0.1;

    private readonly UiLanguage _language;
    private readonly DanteProject _project;
    private readonly IPatchWorkspaceSession _session;
    private readonly bool _returnEditsOnly;
    private readonly bool _lockRxDeviceSelection;
    private readonly bool _embedded;
    private readonly Func<string, DanteChannelKind, int, string, bool>? _renameChannelAction;
    private readonly Func<string, DanteChannelKind, IReadOnlyList<int>, int, bool>? _extendChannelSeriesAction;
    private readonly HashSet<string> _ambiguousSourceNames = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<ToggleButton> _matrixGestureHighlights = [];
    private readonly ObservableCollection<PatchRxListItem> _rxRows = [];
    private readonly ObservableCollection<PatchMatrixRow> _matrixRows = [];
    private IReadOnlyList<PatchSourceDescriptor> _visibleSources = [];
    private IReadOnlyList<PatchTargetDescriptor> _visibleTargets = [];
    private PatchMatrixCell? _matrixGestureStart;
    private PatchMatrixCell? _matrixGestureCurrent;
    private PatchMatrixCell? _matrixOneToOneStart;
    private bool _matrixGestureActive;
    private string? _channelSeriesDeviceName;
    private DanteChannelKind? _channelSeriesKind;
    private int[] _channelSeriesSeeds = [];
    private string? _matrixSeriesDeviceName;
    private DanteChannelKind? _matrixSeriesKind;
    private int[] _matrixSeriesSeeds = [];
    private double _matrixZoom = 1.0;
    private bool _initializing = true;

    public PatchWorkspaceView(
        UiLanguage language,
        DanteProject project,
        bool useLightTheme,
        string? initialTxDeviceName = null,
        string? initialRxDeviceName = null,
        IEnumerable<PatchEditRequest>? initialEdits = null,
        bool returnEditsOnly = false,
        bool lockRxDeviceSelection = false,
        bool embedded = false,
        Func<string, DanteChannelKind, int, string, bool>? renameChannelAction = null,
        Func<string, DanteChannelKind, IReadOnlyList<int>, int, bool>? extendChannelSeriesAction = null,
        bool startInAssignmentMode = false,
        bool warnOnExistingPatch = true,
        IPatchWorkspaceSession? sharedSession = null)
    {
        InitializeComponent();
        // La grille est le mode principal d'Easy patch. La sélection par plage
        // reste immédiatement accessible dans le second onglet.
        PatchModeTabControl.Items.Remove(MatrixTab);
        PatchModeTabControl.Items.Insert(0, MatrixTab);
        if (startInAssignmentMode)
        {
            AssignmentTab.IsSelected = true;
        }
        else
        {
            MatrixTab.IsSelected = true;
        }
        _language = language;
        _project = project ?? throw new ArgumentNullException(nameof(project));
        _session = sharedSession
            ?? new PatchWorkspaceSession(project.PatchMatrix.Subscriptions, initialEdits);
        _returnEditsOnly = returnEditsOnly;
        _lockRxDeviceSelection = lockRxDeviceSelection;
        _embedded = embedded;
        _renameChannelAction = renameChannelAction;
        _extendChannelSeriesAction = extendChannelSeriesAction;
        WarnOnExistingPatchCheckBox.IsChecked = warnOnExistingPatch;
        RxChannelListBox.ItemsSource = _rxRows;
        MatrixGrid.ItemsSource = _matrixRows;

        ConfigureWorkflowPresentation();
        ApplyTheme(useLightTheme);
        ApplyLanguage();
        PopulateDeviceSelectors(initialTxDeviceName, initialRxDeviceName);
        _initializing = false;
        RefreshSourceChannelsAndMatrixColumns();
        RefreshTargetRows();
    }

    public IReadOnlyList<PatchEditRequest> Edits => _session.Edits;

    public bool HasChanges => _session.HasChanges;

    public bool CanRenameChannels => _renameChannelAction is not null;

    public bool IsAssignmentModeSelected => AssignmentTab.IsSelected;

    public bool IsMatrixModeSelected => MatrixTab.IsSelected;

    public void ShowMatrixMode()
    {
        MatrixTab.IsSelected = true;
        UpdateVisibleModePresentation();
    }

    public void ShowAssignmentMode()
    {
        AssignmentTab.IsSelected = true;
        UpdateVisibleModePresentation();
    }

    public bool WarnOnExistingPatch =>
        WarnOnExistingPatchCheckBox.IsChecked != false;

    public string? SelectedTxDeviceName => TxDeviceComboBox.SelectedItem as string;

    public string? SelectedRxDeviceName => RxDeviceComboBox.SelectedItem as string;

    public bool FocusDevice(string deviceName)
    {
        DanteDevice? device = _project.FindDevice(deviceName);
        if (device is null)
        {
            return false;
        }

        string? txDeviceName = device.TxCount > 0
            ? FindDeviceName(TxDeviceComboBox.Items.OfType<string>(), device.Name)
            : null;
        string? rxDeviceName = device.RxCount > 0
            ? FindDeviceName(RxDeviceComboBox.Items.OfType<string>(), device.Name)
            : null;
        if (txDeviceName is null && rxDeviceName is null)
        {
            return false;
        }

        bool wasInitializing = _initializing;
        _initializing = true;
        try
        {
            if (txDeviceName is not null)
            {
                TxDeviceComboBox.SelectedItem = txDeviceName;
            }

            if (rxDeviceName is not null && !_lockRxDeviceSelection)
            {
                RxDeviceComboBox.SelectedItem = rxDeviceName;
            }
        }
        finally
        {
            _initializing = wasInitializing;
        }

        RefreshSourceChannelsAndMatrixColumns();
        RefreshTargetRows();
        UpdateDeviceNavigationState();
        return true;
    }

    public event EventHandler? ApplyRequested;

    public event EventHandler<DirectPatchRequestEventArgs>? DirectApplyRequested;

    public event EventHandler<PatchDeviceFocusChangedEventArgs>? DeviceFocusChanged;

    public event EventHandler? CancelRequested;

    public event EventHandler<InlineChannelNavigationRequestEventArgs>? InlineChannelNavigationRequested;

    private void PatchWorkspaceView_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateResponsiveLayout();
    }

    private void UpdateResponsiveLayout()
    {
        // À 1366 x 768, une hauteur fixe de 132 px masque les premières
        // lignes RX. Les grands écrans gardent les labels TX complets, tandis
        // que les fenêtres compactes conservent au moins une ligne exploitable.
        if (MatrixGrid is null)
        {
            return;
        }

        bool compact = ActualHeight < 600;
        bool compactMatrix = _embedded && MatrixTab.IsSelected;
        IntroTextBlock.Visibility = compact || compactMatrix
            ? Visibility.Collapsed
            : Visibility.Visible;
        RxDeviceLabel.Visibility = Visibility.Visible;
        TxDeviceLabel.Visibility = Visibility.Visible;
        RxDeviceLabel.Content = compactMatrix
            ? "RX"
            : L("Machine réceptrice RX", "Receiving device (Rx)");
        TxDeviceLabel.Content = compactMatrix
            ? "TX"
            : L("Machine émettrice TX", "Transmitting device (Tx)");
        RxDeviceLabel.Padding = compactMatrix
            ? new Thickness(0, 0, 0, 2)
            : new Thickness(0, 8, 0, 4);
        TxDeviceLabel.Padding = RxDeviceLabel.Padding;
        RxDeviceLabel.FontWeight = compactMatrix ? FontWeights.SemiBold : FontWeights.Normal;
        TxDeviceLabel.FontWeight = RxDeviceLabel.FontWeight;
        MatrixOneToOneHintTextBlock.Visibility = ActualWidth < 1080
            ? Visibility.Collapsed
            : Visibility.Visible;
        TitleTextBlock.FontSize = compact || compactMatrix ? 16 : 20;
        WorkspaceHeaderBorder.Padding = compact || compactMatrix
            ? new Thickness(16, 6, 16, 6)
            : new Thickness(16, 12, 16, 12);
        DeviceSelectorGrid.Margin = compactMatrix
            ? new Thickness(16, 5, 16, 5)
            : new Thickness(16, 10, 16, 8);
        SwapDeviceSelectionButton.Margin = compactMatrix
            ? new Thickness(8, 0, 8, 0)
            : new Thickness(8, 22, 8, 0);
        SwapDeviceSelectionButton.Height = compactMatrix ? 34 : 42;
        WorkspaceFooterBorder.Padding = compact
            ? new Thickness(16, 5, 16, 5)
            : new Thickness(16, 10, 16, 10);
        MatrixGrid.ColumnHeaderHeight = ActualHeight switch
        {
            < 560 => 86,
            < 680 => 106,
            _ => 132
        };
        MatrixGrid.MinHeight = ActualHeight < 620 ? 156 : 230;
    }

    private void PatchModeTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!ReferenceEquals(e.OriginalSource, PatchModeTabControl))
        {
            return;
        }

        UpdateVisibleModePresentation();
    }

    public void FocusChannelEditor(DanteChannelKind kind, int danteId, bool matrix)
    {
        Dispatcher.BeginInvoke(new Action(() =>
            FocusInlineChannelEditor(new InlineChannelNavigationTarget(kind, danteId, matrix))));
    }

    public PatchMatrixOneToOneState CaptureMatrixOneToOneState()
    {
        return new PatchMatrixOneToOneState(
            MatrixOneToOneCountTextBox.Text,
            _matrixOneToOneStart?.Source.DeviceName,
            _matrixOneToOneStart?.Source.DanteId,
            _matrixOneToOneStart?.Target.DeviceName,
            _matrixOneToOneStart?.Target.DanteId);
    }

    public void RestoreMatrixOneToOneState(PatchMatrixOneToOneState? state)
    {
        if (state is null)
        {
            return;
        }

        // Un patch immédiat reconstruit Easy Patch : les identifiants stables
        // permettent de retrouver le départ sans dépendre du nom des canaux.
        bool wasInitializing = _initializing;
        _initializing = true;
        try
        {
            MatrixOneToOneCountTextBox.Text = state.CountText;
            _matrixOneToOneStart = state.TxDanteId is int txDanteId
                && state.RxDanteId is int rxDanteId
                && !string.IsNullOrWhiteSpace(state.TxDeviceName)
                && !string.IsNullOrWhiteSpace(state.RxDeviceName)
                ? _matrixRows
                    .Where(row =>
                        row.Target.DanteId == rxDanteId
                        && string.Equals(
                            row.Target.DeviceName,
                            state.RxDeviceName,
                            StringComparison.OrdinalIgnoreCase))
                    .SelectMany(row => row.Cells)
                    .FirstOrDefault(cell =>
                        cell.Source.DanteId == txDanteId
                        && string.Equals(
                            cell.Source.DeviceName,
                            state.TxDeviceName,
                            StringComparison.OrdinalIgnoreCase))
                : null;
        }
        finally
        {
            _initializing = wasInitializing;
        }

        UpdateCommandState();
    }

    public void ResetPendingChanges()
    {
        _session.Reset();
        RefreshAllTargetStates();
        SetInfo(L("Tous les changements Easy patch ont été appliqués.", "All Easy patch changes were applied."));
    }

    private void ConfigureWorkflowPresentation()
    {
        if (!_embedded)
        {
            return;
        }

        // Dans l'onglet Easy Patch, chaque commande est appliquée directement.
        // Les contrôles de lot restent réservés à la fenêtre autonome qui doit
        // renvoyer ses modifications à la fiche machine lors de sa fermeture.
        PreviewSelectionButton.Visibility = Visibility.Collapsed;
        PreviewRangeButton.Visibility = Visibility.Collapsed;
        PreviewGroupBox.Visibility = Visibility.Collapsed;
        PendingHeaderTextBlock.Visibility = Visibility.Collapsed;
        PendingFooterTextBlock.Visibility = Visibility.Collapsed;
        ResetPendingButton.Visibility = Visibility.Collapsed;
        CancelButton.Visibility = Visibility.Collapsed;
        ApplyButton.Visibility = Visibility.Collapsed;
        PatchModeTabControl.Template = (ControlTemplate)FindResource(
            "EmbeddedPatchModeTabControlTemplate");

        Grid.SetColumn(ApplySelectionDirectButton, 0);
        Grid.SetColumnSpan(ApplySelectionDirectButton, 2);
        ApplySelectionDirectButton.Margin = new Thickness(0, 0, 0, 6);
        Grid.SetColumn(ApplyRangeDirectButton, 0);
        Grid.SetColumnSpan(ApplyRangeDirectButton, 2);
        ApplyRangeDirectButton.Margin = new Thickness(0);
    }

    private void PopulateDeviceSelectors(string? initialTxDeviceName, string? initialRxDeviceName)
    {
        string[] txDevices = _project.Devices
            .Where(device => device.TxCount > 0)
            .Select(device => device.Name)
            .ToArray();
        string[] rxDevices = _project.Devices
            .Where(device => device.RxCount > 0)
            .Select(device => device.Name)
            .ToArray();

        TxDeviceComboBox.ItemsSource = txDevices;
        RxDeviceComboBox.ItemsSource = rxDevices;
        TxDeviceComboBox.SelectedItem = FindDeviceName(txDevices, initialTxDeviceName) ?? txDevices.FirstOrDefault();
        RxDeviceComboBox.SelectedItem = FindDeviceName(rxDevices, initialRxDeviceName) ?? rxDevices.FirstOrDefault();
        RxDeviceComboBox.IsEnabled = !_lockRxDeviceSelection;
        UpdateDeviceNavigationState();
    }

    private void PreviousRxDeviceButton_Click(object sender, RoutedEventArgs e)
    {
        MoveDeviceSelection(RxDeviceComboBox, -1);
    }

    private void NextRxDeviceButton_Click(object sender, RoutedEventArgs e)
    {
        MoveDeviceSelection(RxDeviceComboBox, 1);
    }

    private void PreviousTxDeviceButton_Click(object sender, RoutedEventArgs e)
    {
        MoveDeviceSelection(TxDeviceComboBox, -1);
    }

    private void NextTxDeviceButton_Click(object sender, RoutedEventArgs e)
    {
        MoveDeviceSelection(TxDeviceComboBox, 1);
    }

    private static void MoveDeviceSelection(ComboBox selector, int offset)
    {
        if (!selector.IsEnabled || selector.Items.Count <= 1)
        {
            return;
        }

        int currentIndex = selector.SelectedIndex >= 0 ? selector.SelectedIndex : 0;
        selector.SelectedIndex = (currentIndex + offset + selector.Items.Count) % selector.Items.Count;
    }

    private void UpdateDeviceNavigationState()
    {
        bool canMoveRx = !_lockRxDeviceSelection && RxDeviceComboBox.Items.Count > 1;
        PreviousRxDeviceButton.IsEnabled = canMoveRx;
        NextRxDeviceButton.IsEnabled = canMoveRx;

        bool canMoveTx = TxDeviceComboBox.Items.Count > 1;
        PreviousTxDeviceButton.IsEnabled = canMoveTx;
        NextTxDeviceButton.IsEnabled = canMoveTx;
        SwapDeviceSelectionButton.IsEnabled = !_lockRxDeviceSelection
            && TxDeviceComboBox.SelectedItem is string
            && RxDeviceComboBox.SelectedItem is string;
    }

    private void SwapDeviceSelectionButton_Click(object sender, RoutedEventArgs e)
    {
        PatchDeviceSelectionSwapResult result = PatchDeviceSelectionSwapper.TrySwap(
            TxDeviceComboBox.SelectedItem as string,
            RxDeviceComboBox.SelectedItem as string,
            TxDeviceComboBox.Items.OfType<string>(),
            RxDeviceComboBox.Items.OfType<string>(),
            _lockRxDeviceSelection);
        if (!result.Success)
        {
            SetInfo(
                _language == UiLanguage.English
                    ? TranslateSwapError(result.ErrorMessage)
                    : result.ErrorMessage ?? "Inversion impossible.",
                warning: true);
            return;
        }

        _initializing = true;
        TxDeviceComboBox.SelectedItem = result.TxDeviceName;
        RxDeviceComboBox.SelectedItem = result.RxDeviceName;
        _initializing = false;
        RefreshSourceChannelsAndMatrixColumns();
        RefreshTargetRows();
        UpdateDeviceNavigationState();
        NotifyDeviceFocus(result.RxDeviceName, DanteChannelKind.Rx);
        SetInfo(_embedded
            ? L("Machines TX et RX inversées.", "Tx and Rx devices swapped.")
            : L(
                "Machines TX et RX inversées. Le lot en attente est conservé.",
                "Tx and Rx devices swapped. The pending batch was preserved."));
    }

    private void TxDeviceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_initializing)
        {
            return;
        }

        RefreshSourceChannelsAndMatrixColumns();
        RefreshTargetRows();
        UpdateDeviceNavigationState();
        NotifyDeviceFocus(TxDeviceComboBox.SelectedItem as string, DanteChannelKind.Tx);
    }

    private void RxDeviceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_initializing)
        {
            RefreshTargetRows();
            UpdateDeviceNavigationState();
            NotifyDeviceFocus(RxDeviceComboBox.SelectedItem as string, DanteChannelKind.Rx);
        }
    }

    private void NotifyDeviceFocus(string? deviceName, DanteChannelKind kind)
    {
        if (!string.IsNullOrWhiteSpace(deviceName))
        {
            DeviceFocusChanged?.Invoke(
                this,
                new PatchDeviceFocusChangedEventArgs(deviceName, kind));
        }
    }

    private void TxChannelListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateCommandState();
    }

    private void RxChannelListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateCommandState();
    }

    private void RefreshSourceChannelsAndMatrixColumns()
    {
        _matrixOneToOneStart = null;
        int? selectedRangeDanteId = (RangeStartTxComboBox.SelectedItem as PatchSourceDescriptor)?.DanteId;
        DanteDevice? device = _project.FindDevice(TxDeviceComboBox.SelectedItem as string);
        _visibleSources = device?.TxChannels
            .Select(channel => new PatchSourceDescriptor(
                device.Name,
                channel.DanteId,
                channel.PositionIndex,
                channel.DisplayName))
            .OrderBy(channel => channel.PositionIndex)
            .ToArray() ?? [];

        TxChannelListBox.ItemsSource = _visibleSources.Select(source => new PatchTxListItem(source)).ToArray();
        RangeStartTxComboBox.ItemsSource = _visibleSources;
        RangeStartTxComboBox.SelectedItem = _visibleSources.FirstOrDefault(source => source.DanteId == selectedRangeDanteId)
            ?? _visibleSources.FirstOrDefault();
        _ambiguousSourceNames.Clear();
        foreach (IGrouping<string, PatchSourceDescriptor> duplicate in _visibleSources
                     .GroupBy(source => source.ChannelName, StringComparer.OrdinalIgnoreCase)
                     .Where(group => group.Count() > 1))
        {
            _ambiguousSourceNames.Add(duplicate.Key);
        }

        BuildMatrixColumns();
        if (_ambiguousSourceNames.Count > 0)
        {
            SetInfo(
                L(
                    "Cette machine contient des noms TX en double. Renommez-les avant de les utiliser dans la grille.",
                    "This device contains duplicate Tx names. Rename them before using them in the matrix."),
                warning: true);
        }
        else
        {
            SetInfo(_embedded
                ? L(
                    "Sélectionnez les TX et les RX, puis appliquez la sélection ou une plage.",
                    "Select Tx and Rx channels, then apply the selection or a range.")
                : L(
                    "Sélectionnez les TX et les RX, puis prévisualisez la sélection ou une plage.",
                    "Select the Tx and Rx channels, then preview the selection or a range."));
        }
    }

    private void RefreshTargetRows()
    {
        _matrixOneToOneStart = null;
        string? requestedDeviceName = RxDeviceComboBox.SelectedItem as string;
        HashSet<int> selectedDanteIds = RxChannelListBox.SelectedItems
            .OfType<PatchRxListItem>()
            .Where(row => string.Equals(row.Target.DeviceName, requestedDeviceName, StringComparison.OrdinalIgnoreCase))
            .Select(row => row.Target.DanteId)
            .ToHashSet();
        int? selectedRangeDanteId = (RangeStartRxComboBox.SelectedItem as PatchTargetDescriptor)?.DanteId;
        DanteDevice? device = _project.FindDevice(RxDeviceComboBox.SelectedItem as string);
        _visibleTargets = device?.RxChannels
            .Select(channel => new PatchTargetDescriptor(
                device.Name,
                channel.DanteId,
                channel.PositionIndex,
                channel.DisplayName))
            .OrderBy(channel => channel.PositionIndex)
            .ToArray() ?? [];

        _rxRows.Clear();
        foreach (PatchTargetDescriptor target in _visibleTargets)
        {
            _rxRows.Add(BuildRxListItem(target));
        }

        foreach (PatchRxListItem row in _rxRows.Where(row => selectedDanteIds.Contains(row.Target.DanteId)))
        {
            RxChannelListBox.SelectedItems.Add(row);
        }

        if (RxChannelListBox.SelectedItems.Count == 0 && _rxRows.Count > 0)
        {
            RxChannelListBox.SelectedItem = _rxRows[0];
        }

        RangeStartRxComboBox.ItemsSource = _visibleTargets;
        RangeStartRxComboBox.SelectedItem = _visibleTargets.FirstOrDefault(target => target.DanteId == selectedRangeDanteId)
            ?? _visibleTargets.FirstOrDefault();

        _matrixRows.Clear();
        for (int targetIndex = 0; targetIndex < _visibleTargets.Count; targetIndex++)
        {
            _matrixRows.Add(BuildMatrixRow(_visibleTargets[targetIndex], targetIndex));
        }

        RefreshPendingPreview();
        UpdateCommandState();
    }

    private PatchRxListItem BuildRxListItem(PatchTargetDescriptor target)
    {
        EffectivePatchAssignment assignment = _session.GetEffectiveAssignment(target);
        string source = assignment.IsActive
            ? $"{assignment.TxDeviceName} / {assignment.TxChannelName}"
            : L("Libre", "Free");
        string marker = assignment.IsPending ? L("  [modifié]", "  [changed]") : string.Empty;
        return new PatchRxListItem(target, target.ChannelName, source + marker);
    }

    private PatchMatrixRow BuildMatrixRow(PatchTargetDescriptor target, int targetIndex)
    {
        EffectivePatchAssignment assignment = _session.GetEffectiveAssignment(target);
        int assignedSourceIndex = FindAssignedSourceIndex(assignment);
        PatchMatrixCell[] cells = _visibleSources
            .Select((source, index) => new PatchMatrixCell(
                source,
                target,
                index,
                targetIndex,
                index == assignedSourceIndex,
                assignment.IsPending,
                index == assignedSourceIndex ? "●" : string.Empty,
                BuildCellToolTip(source, target, index == assignedSourceIndex, assignment.IsPending),
                BuildCellAutomationName(source, target, index == assignedSourceIndex)))
            .ToArray();

        return new PatchMatrixRow(target, assignment.IsPending, cells);
    }

    private void RefreshTargetStates(IEnumerable<PatchTargetDescriptor> targets)
    {
        string? visibleDeviceName = RxDeviceComboBox.SelectedItem as string;
        HashSet<int> danteIds = targets
            .Where(target => string.Equals(
                target.DeviceName,
                visibleDeviceName,
                StringComparison.OrdinalIgnoreCase))
            .Select(target => target.DanteId)
            .ToHashSet();

        for (int targetIndex = 0; targetIndex < _visibleTargets.Count; targetIndex++)
        {
            if (danteIds.Contains(_visibleTargets[targetIndex].DanteId))
            {
                RefreshTargetState(targetIndex);
            }
        }

        RefreshPendingPreview();
        UpdateCommandState();
    }

    private void RefreshAllTargetStates()
    {
        for (int targetIndex = 0; targetIndex < _visibleTargets.Count; targetIndex++)
        {
            RefreshTargetState(targetIndex);
        }

        RefreshPendingPreview();
        UpdateCommandState();
    }

    private void RefreshTargetState(int targetIndex)
    {
        if (targetIndex < 0
            || targetIndex >= _visibleTargets.Count
            || targetIndex >= _matrixRows.Count
            || targetIndex >= _rxRows.Count)
        {
            return;
        }

        PatchTargetDescriptor target = _visibleTargets[targetIndex];
        EffectivePatchAssignment assignment = _session.GetEffectiveAssignment(target);
        string source = assignment.IsActive
            ? $"{assignment.TxDeviceName} / {assignment.TxChannelName}"
            : L("Libre", "Free");
        string marker = assignment.IsPending ? L("  [modifié]", "  [changed]") : string.Empty;
        _rxRows[targetIndex].SourceDisplay = source + marker;

        PatchMatrixRow row = _matrixRows[targetIndex];
        row.IsPending = assignment.IsPending;
        int assignedSourceIndex = FindAssignedSourceIndex(assignment);
        for (int sourceIndex = 0; sourceIndex < row.Cells.Count; sourceIndex++)
        {
            PatchMatrixCell cell = row.Cells[sourceIndex];
            bool isAssigned = sourceIndex == assignedSourceIndex;
            cell.Update(
                isAssigned,
                assignment.IsPending,
                isAssigned ? "●" : string.Empty,
                BuildCellToolTip(cell.Source, cell.Target, isAssigned, assignment.IsPending),
                BuildCellAutomationName(cell.Source, cell.Target, isAssigned));
        }
    }

    private int FindAssignedSourceIndex(EffectivePatchAssignment assignment)
    {
        if (!assignment.IsActive)
        {
            return -1;
        }

        for (int index = 0; index < _visibleSources.Count; index++)
        {
            PatchSourceDescriptor source = _visibleSources[index];
            if (string.Equals(source.DeviceName, assignment.TxDeviceName, StringComparison.OrdinalIgnoreCase)
                && string.Equals(source.ChannelName, assignment.TxChannelName, StringComparison.Ordinal))
            {
                // Le XML référence le nom du TX et non son numéro. En cas de
                // doublon, une seule cellule est montrée et toute nouvelle
                // affectation ambiguë reste bloquée.
                return index;
            }
        }

        return -1;
    }

    private void BuildMatrixColumns()
    {
        MatrixGrid.Columns.Clear();
        FrameworkElementFactory rxPanel = new(typeof(Grid));
        FrameworkElementFactory rxId = new(typeof(TextBlock));
        rxId.SetBinding(TextBlock.TextProperty, new Binding("Target.DanteId") { StringFormat = "{0:000} - " });
        rxId.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
        rxPanel.AppendChild(rxId);

        FrameworkElementFactory rxName = new(typeof(TextBox));
        rxName.SetBinding(TextBox.TextProperty, new Binding("Target.ChannelName") { Mode = BindingMode.OneWay });
        rxName.SetBinding(FrameworkElement.TagProperty, new Binding("Target"));
        rxName.SetValue(FrameworkElement.MarginProperty, new Thickness(44, 0, 20, 0));
        rxName.SetValue(Control.PaddingProperty, new Thickness(2, 0, 2, 0));
        rxName.SetValue(Control.BorderThicknessProperty, new Thickness(0));
        rxName.SetValue(Control.BackgroundProperty, Brushes.Transparent);
        rxName.SetValue(Control.ForegroundProperty, (Brush)FindResource("TextBrush"));
        rxName.SetValue(TextBox.IsReadOnlyProperty, _renameChannelAction is null);
        rxName.SetValue(FrameworkElement.CursorProperty, Cursors.IBeam);
        rxName.AddHandler(UIElement.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(InlineChannelNameTextBox_PreviewMouseLeftButtonDown));
        rxName.AddHandler(UIElement.KeyDownEvent, new KeyEventHandler(InlineChannelNameTextBox_KeyDown));
        rxName.AddHandler(UIElement.GotKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(MatrixInlineTextBox_GotKeyboardFocus));
        rxName.AddHandler(UIElement.LostKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(InlineChannelNameTextBox_LostKeyboardFocus));
        rxPanel.AppendChild(rxName);

        FrameworkElementFactory rxSeries = BuildMatrixSeriesThumbFactory("Target", Cursors.SizeNS);
        rxSeries.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Right);
        rxSeries.SetBinding(
            UIElement.VisibilityProperty,
            new Binding("Target.CanExtendNameSeries")
            {
                Converter = new BooleanToVisibilityConverter()
            });
        rxPanel.AppendChild(rxSeries);

        MatrixGrid.Columns.Add(new DataGridTemplateColumn
        {
            Header = L("Canal RX", "Rx channel"),
            CellTemplate = new DataTemplate { VisualTree = rxPanel },
            Width = new DataGridLength(190 * _matrixZoom),
            IsReadOnly = true
        });

        Style cellStyle = (Style)FindResource("MatrixCellToggleStyle");
        for (int index = 0; index < _visibleSources.Count; index++)
        {
            PatchSourceDescriptor source = _visibleSources[index];
            FrameworkElementFactory toggle = new(typeof(ToggleButton));
            toggle.SetValue(FrameworkElement.StyleProperty, cellStyle);
            toggle.SetBinding(FrameworkElement.TagProperty, new Binding($"Cells[{index}]"));
            toggle.SetBinding(ToggleButton.IsCheckedProperty, new Binding($"Cells[{index}].IsAssigned") { Mode = BindingMode.OneWay });
            toggle.SetBinding(ContentControl.ContentProperty, new Binding($"Cells[{index}].Marker"));
            toggle.SetBinding(ToolTipService.ToolTipProperty, new Binding($"Cells[{index}].ToolTip"));
            toggle.SetBinding(AutomationProperties.NameProperty, new Binding($"Cells[{index}].AutomationName"));
            toggle.SetValue(FrameworkElement.WidthProperty, 28 * _matrixZoom);
            toggle.SetValue(FrameworkElement.HeightProperty, 22 * _matrixZoom);
            toggle.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(MatrixCellButton_Click));

            MatrixGrid.Columns.Add(new DataGridTemplateColumn
            {
                Header = BuildMatrixHeader(source),
                CellTemplate = new DataTemplate { VisualTree = toggle },
                Width = new DataGridLength(32 * _matrixZoom),
                IsReadOnly = true
            });
        }

        MatrixGrid.RowHeight = 26 * _matrixZoom;
        MatrixGrid.ColumnHeaderHeight = 132 * _matrixZoom;
    }

    private FrameworkElement BuildMatrixHeader(PatchSourceDescriptor source)
    {
        Grid panel = new()
        {
            Width = 28 * _matrixZoom,
            Height = 126 * _matrixZoom,
            Tag = source
        };
        Button label = new()
        {
            Content = source.Display,
            Tag = source,
            Width = 112 * _matrixZoom,
            FontSize = Math.Max(8, 10 * _matrixZoom),
            FontWeight = FontWeights.SemiBold,
            Cursor = _renameChannelAction is null ? Cursors.Arrow : Cursors.IBeam,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10 * _matrixZoom),
            Padding = new Thickness(0),
            Background = Brushes.Transparent,
            BorderBrush = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Foreground = (Brush)FindResource("TextBrush"),
            LayoutTransform = new RotateTransform(-90),
            IsEnabled = _renameChannelAction is not null
        };
        label.Click += MatrixTxHeader_Click;
        panel.Children.Add(label);

        Thumb series = BuildMatrixSeriesThumb(source, Cursors.SizeWE);
        series.Visibility = source.CanExtendNameSeries ? Visibility.Visible : Visibility.Collapsed;
        series.HorizontalAlignment = HorizontalAlignment.Stretch;
        series.VerticalAlignment = VerticalAlignment.Bottom;
        panel.Children.Add(series);

        ToolTipService.SetToolTip(panel, L(
            $"{source.FullDisplay}. Cliquez pour renommer ; tirez la poignée si le nom se termine par un numéro.",
            $"{source.FullDisplay}. Click to rename; drag the handle when the name ends with a number."));
        AutomationProperties.SetName(panel, source.FullDisplay);
        return panel;
    }

    private FrameworkElementFactory BuildMatrixSeriesThumbFactory(string tagPath, Cursor cursor)
    {
        FrameworkElementFactory thumb = new(typeof(Thumb));
        thumb.SetBinding(FrameworkElement.TagProperty, new Binding(tagPath));
        thumb.SetValue(FrameworkElement.WidthProperty, 16d);
        thumb.SetValue(FrameworkElement.HeightProperty, 18d);
        thumb.SetValue(FrameworkElement.CursorProperty, cursor);
        thumb.SetValue(ToolTipService.ToolTipProperty, L("Étendre la série de noms", "Extend the name series"));
        FrameworkElementFactory frame = new(typeof(Border));
        frame.SetValue(Border.BackgroundProperty, (Brush)FindResource("AccentBrush"));
        frame.SetValue(Border.CornerRadiusProperty, new CornerRadius(2));
        FrameworkElementFactory glyph = new(typeof(TextBlock));
        glyph.SetValue(TextBlock.TextProperty, "↕");
        glyph.SetValue(TextBlock.ForegroundProperty, Brushes.White);
        glyph.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        glyph.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
        frame.AppendChild(glyph);
        thumb.SetValue(Control.TemplateProperty, new ControlTemplate(typeof(Thumb)) { VisualTree = frame });
        thumb.AddHandler(Thumb.DragStartedEvent, new DragStartedEventHandler(MatrixSeriesThumb_DragStarted));
        thumb.AddHandler(Thumb.DragCompletedEvent, new DragCompletedEventHandler(MatrixSeriesThumb_DragCompleted));
        return thumb;
    }

    private Thumb BuildMatrixSeriesThumb(PatchSourceDescriptor source, Cursor cursor)
    {
        Thumb thumb = new()
        {
            Tag = source,
            Height = 8,
            Cursor = cursor,
            ToolTip = L("Étendre la série de noms", "Extend the name series"),
            Background = (Brush)FindResource("AccentBrush")
        };
        thumb.DragStarted += MatrixSeriesThumb_DragStarted;
        thumb.DragCompleted += MatrixSeriesThumb_DragCompleted;
        return thumb;
    }

    private void MatrixTxHeader_Click(object sender, RoutedEventArgs e)
    {
        if (_renameChannelAction is not null
            && sender is Button { Tag: PatchSourceDescriptor source } label)
        {
            e.Handled = true;
            Dispatcher.BeginInvoke(new Action(() => OpenMatrixTxRenameEditor(source, label)));
        }
    }

    private void OpenMatrixTxRenameEditor(PatchSourceDescriptor source, FrameworkElement placementTarget)
    {
        TextBox editor = new()
        {
            Text = source.ChannelName,
            MinWidth = 180,
            Padding = new Thickness(6, 4, 6, 4),
            Background = Brushes.White,
            Foreground = Brushes.Black
        };
        Border frame = new()
        {
            Child = editor,
            Background = Brushes.White,
            BorderBrush = (Brush)FindResource("AccentBrush"),
            BorderThickness = new Thickness(2),
            Padding = new Thickness(2)
        };
        Popup popup = new()
        {
            PlacementTarget = placementTarget,
            Placement = PlacementMode.Bottom,
            StaysOpen = false,
            AllowsTransparency = true,
            Child = frame,
            IsOpen = true
        };
        bool cancelled = false;
        bool committed = false;
        void Commit()
        {
            if (committed || cancelled)
            {
                return;
            }

            committed = true;
            RenameMatrixChannel(source.DeviceName, DanteChannelKind.Tx, source.DanteId, source.ChannelName, editor.Text);
        }

        editor.KeyDown += (_, args) =>
        {
            if (args.Key == Key.Escape)
            {
                cancelled = true;
                popup.IsOpen = false;
                args.Handled = true;
            }
            else if (args.Key == Key.Enter)
            {
                Commit();
                popup.IsOpen = false;
                args.Handled = true;
            }
            else if (args.Key == Key.Tab)
            {
                InlineChannelNavigationTarget? target = BuildInlineNavigationTarget(
                    source,
                    backwards: (Keyboard.Modifiers & ModifierKeys.Shift) != 0);
                if (target is null)
                {
                    return;
                }

                Commit();
                popup.IsOpen = false;
                args.Handled = true;
                RequestInlineChannelNavigation(target);
            }
        };
        popup.Closed += (_, _) => Commit();
        editor.Loaded += (_, _) =>
        {
            editor.Focus();
            editor.SelectAll();
        };
    }

    private void RenameMatrixChannel(
        string deviceName,
        DanteChannelKind kind,
        int channelIndex,
        string oldName,
        string proposedName)
    {
        string newName = proposedName.Trim();
        if (_renameChannelAction is null || string.Equals(oldName, newName, StringComparison.Ordinal))
        {
            return;
        }

        if (kind == DanteChannelKind.Tx)
        {
            _session.RenamePendingSourceChannel(deviceName, oldName, newName);
        }

        if (!_renameChannelAction(deviceName, kind, channelIndex, newName)
            && kind == DanteChannelKind.Tx)
        {
            _session.RenamePendingSourceChannel(deviceName, newName, oldName);
        }
    }

    private void MatrixInlineTextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (sender is not TextBox editor)
        {
            return;
        }

        editor.Background = Brushes.White;
        editor.Foreground = new SolidColorBrush(Color.FromRgb(17, 24, 39));
        editor.BorderBrush = (Brush)FindResource("AccentBrush");
        editor.BorderThickness = new Thickness(1);
    }

    private void PreviewSelectionButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            StagePlanAsPreview(BuildSelectionPlan());
        }
        catch (Exception exception)
        {
            SetInfo(exception.Message, warning: true);
        }
    }

    private void ApplySelectionDirectButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            ApplyPlanDirectly(BuildSelectionPlan());
        }
        catch (Exception exception)
        {
            SetInfo(exception.Message, warning: true);
        }
    }

    private PatchAssignmentPlan BuildSelectionPlan()
    {
        return PatchAssignmentPlanner.PlanSelection(SelectedSources(), SelectedTargets());
    }

    private void PreviewRangeButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            StagePlanAsPreview(BuildRangePlan());
        }
        catch (Exception exception)
        {
            SetInfo(exception.Message, warning: true);
        }
    }

    private void ApplyRangeDirectButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            ApplyPlanDirectly(BuildRangePlan());
        }
        catch (Exception exception)
        {
            SetInfo(exception.Message, warning: true);
        }
    }

    private PatchAssignmentPlan BuildRangePlan()
    {
        if (RangeStartTxComboBox.SelectedItem is not PatchSourceDescriptor firstSource
            || RangeStartRxComboBox.SelectedItem is not PatchTargetDescriptor firstTarget
            || !int.TryParse(RangeCountTextBox.Text, out int count))
        {
            throw new InvalidOperationException(L(
                "Renseignez le premier TX, le premier RX et un nombre de canaux valide.",
                "Choose the first Tx, first Rx and a valid channel count."));
        }

        return PatchAssignmentPlanner.PlanOneToOne(
            _visibleSources,
            firstSource,
            _visibleTargets,
            firstTarget,
            count);
    }

    private void StagePlanAsPreview(PatchAssignmentPlan plan)
    {
        if (!SourcesAreUnambiguous(plan.Assignments.Select(assignment => assignment.Source)))
        {
            return;
        }

        try
        {
            PatchBatchPreview preview = _session.BuildPreview(plan.Assignments);
            PatchConflictResolution resolution = ChooseConflictResolution(preview);
            PatchStageResult result = _session.StagePreview(preview, resolution);
            if (result.IsCancelled)
            {
                SetInfo(L("Prévisualisation annulée : le lot reste inchangé.", "Preview cancelled: the batch is unchanged."), warning: true);
                return;
            }

            string message = L(
                $"{result.StagedCount} changement(s) prévisualisé(s) et ajouté(s) au lot, {result.SkippedConflictCount} conflit(s) ignoré(s), {result.UnchangedCount} ligne(s) inchangée(s).",
                $"{result.StagedCount} change(s) previewed and added to the batch, {result.SkippedConflictCount} conflict(s) skipped, {result.UnchangedCount} row(s) unchanged.");
            RefreshTargetStates(plan.Assignments.Select(assignment => assignment.Target));
            SetInfo(message, warning: result.SkippedConflictCount > 0);
        }
        catch (Exception exception)
        {
            SetInfo(exception.Message, warning: true);
        }
    }

    private void RefreshPendingPreview()
    {
        IReadOnlyList<PendingPatchChange> changes = _session.PendingChanges;
        if (changes.Count == 0)
        {
            PreviewGrid.ItemsSource = Array.Empty<PatchPreviewRow>();
            PreviewGroupBox.Visibility = Visibility.Collapsed;
            PreviewSummaryTextBlock.Text = L("Aucun changement dans le lot.", "No changes in the batch.");
            return;
        }

        PatchPreviewRow[] rows = changes.Select(BuildPendingPreviewRow).ToArray();
        PreviewGrid.ItemsSource = rows;
        PreviewGroupBox.Visibility = Visibility.Visible;
        int creations = changes.Count(change => change.IsCreation);
        int removals = changes.Count(change => change.IsRemoval);
        int replacements = changes.Count - creations - removals;
        PreviewSummaryTextBlock.Text = L(
            $"Lot cumulatif : {changes.Count} changement(s), {creations} création(s), {replacements} remplacement(s), {removals} déconnexion(s). Le XML n'est pas encore modifié.",
            $"Cumulative batch: {changes.Count} change(s), {creations} new, {replacements} replacement(s), {removals} disconnection(s). The XML is not modified yet.");
    }

    private PatchPreviewRow BuildPendingPreviewRow(PendingPatchChange change)
    {
        string action = change.IsRemoval
            ? L("Déconnecter", "Disconnect")
            : change.IsCreation
                ? L("Créer", "Create")
                : L("Remplacer", "Replace");
        string actionKey = change.IsRemoval
            ? "Remove"
            : change.IsCreation ? "Create" : "Replace";

        return new PatchPreviewRow(
            DescribeTarget(change),
            DescribeSource(change.OriginalTxDeviceName, change.OriginalTxChannelName),
            DescribeSource(change.DesiredTxDeviceName, change.DesiredTxChannelName),
            action,
            actionKey);
    }

    private string DescribeTarget(PendingPatchChange change)
    {
        DanteChannel? channel = _project.FindDevice(change.RxDeviceName)?.RxChannels
            .FirstOrDefault(item => item.DanteId == change.RxDanteId);
        return channel is null
            ? $"{change.RxDeviceName} / {change.RxDanteId:000}"
            : $"{change.RxDeviceName} / {change.RxDanteId:000} - {channel.DisplayName}";
    }

    private string DescribeSource(string? deviceName, string? channelName)
    {
        return string.IsNullOrWhiteSpace(deviceName)
            ? L("Libre", "Free")
            : $"{deviceName} / {channelName}";
    }

    private void ApplyPlanDirectly(PatchAssignmentPlan plan)
    {
        if (_embedded)
        {
            ApplyPlanImmediately(plan);
            return;
        }

        if (!SourcesAreUnambiguous(plan.Assignments.Select(assignment => assignment.Source)))
        {
            return;
        }

        if (_session.HasChanges)
        {
            MessageBoxResult pendingChoice = ShowConfirmation(
                L(
                    $"Cette action appliquera aussi les {_session.PendingCount} changement(s) déjà ajouté(s) au lot. Continuer ?",
                    $"This action will also apply the {_session.PendingCount} change(s) already added to the batch. Continue?"),
                L("Appliquer directement", "Apply directly"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (pendingChoice != MessageBoxResult.Yes)
            {
                return;
            }
        }

        PatchBatchPreview preview = _session.BuildPreview(plan.Assignments);
        PatchConflictResolution resolution = ChooseConflictResolution(preview);
        PatchStageResult result = _session.StagePreview(preview, resolution);
        if (result.IsCancelled)
        {
            SetInfo(L("Application directe annulée.", "Direct apply cancelled."), warning: true);
            return;
        }

        RefreshTargetStates(plan.Assignments.Select(assignment => assignment.Target));
        if (!_session.HasChanges)
        {
            SetInfo(L(
                $"Aucun changement à appliquer ({result.SkippedConflictCount} conflit(s) ignoré(s), {result.UnchangedCount} ligne(s) inchangée(s)).",
                $"No change to apply ({result.SkippedConflictCount} conflict(s) skipped, {result.UnchangedCount} row(s) unchanged)."),
                warning: result.SkippedConflictCount > 0);
            return;
        }

        SetInfo(L("Application directe en cours.", "Applying changes directly."));
        ApplyRequested?.Invoke(this, EventArgs.Empty);
    }

    private void ApplyPlanImmediately(PatchAssignmentPlan plan)
    {
        if (!SourcesAreUnambiguous(plan.Assignments.Select(assignment => assignment.Source)))
        {
            return;
        }

        PatchBatchPreview preview = _session.BuildCommittedPreview(plan.Assignments);
        PatchConflictResolution resolution = ChooseConflictResolution(preview);
        if (resolution == PatchConflictResolution.Cancel)
        {
            SetInfo(L("Patch annulé.", "Patch cancelled."), warning: true);
            return;
        }

        PatchEditRequest[] edits = preview.Items
            .Where(item => item.Action != PatchPreviewAction.Unchanged)
            .Where(item => item.Action != PatchPreviewAction.Replace
                || resolution == PatchConflictResolution.Replace)
            .Select(item => PatchEditRequest.Apply(item.Assignment))
            .ToArray();
        if (edits.Length == 0)
        {
            SetInfo(
                resolution == PatchConflictResolution.Skip
                    ? L("Aucun patch appliqué : les RX déjà utilisés ont été ignorés.", "No patch applied: assigned Rx channels were skipped.")
                    : L("Le patch demandé est déjà en place.", "The requested patch is already active."),
                warning: resolution == PatchConflictResolution.Skip);
            return;
        }

        DirectApplyRequested?.Invoke(this, new DirectPatchRequestEventArgs(edits));
    }

    private PatchConflictResolution ChooseConflictResolution(PatchBatchPreview preview)
    {
        if (!preview.HasConflicts)
        {
            return PatchConflictResolution.Replace;
        }

        if (!WarnOnExistingPatch)
        {
            return PatchConflictResolution.Replace;
        }

        MessageBoxResult choice = ShowConfirmation(
            L(
                $"{preview.ReplaceCount} RX possède(nt) déjà une source. Oui = remplacer, Non = ignorer ces conflits, Annuler = ne rien appliquer.",
                $"{preview.ReplaceCount} Rx channel(s) already have a source. Yes = replace, No = skip these conflicts, Cancel = apply nothing."),
            L("Conflits de patch", "Patch conflicts"),
            MessageBoxButton.YesNoCancel,
            MessageBoxImage.Warning);
        return choice switch
        {
            MessageBoxResult.Yes => PatchConflictResolution.Replace,
            MessageBoxResult.No => PatchConflictResolution.Skip,
            _ => PatchConflictResolution.Cancel
        };
    }

    private void RemoveSelectedRxButton_Click(object sender, RoutedEventArgs e)
    {
        PatchTargetDescriptor[] targets = SelectedTargets();
        if (targets.Length == 0)
        {
            SetInfo(L("Sélectionnez au moins un canal RX à déconnecter.", "Select at least one Rx channel to disconnect."), warning: true);
            return;
        }

        MessageBoxResult confirmation = ShowConfirmation(
            _embedded
                ? L(
                    $"Déconnecter immédiatement {targets.Length} canal(aux) RX sélectionné(s) ?",
                    $"Immediately disconnect {targets.Length} selected Rx channel(s)?")
                : L(
                    $"Ajouter la déconnexion de {targets.Length} canal(aux) RX sélectionné(s) au lot ?",
                    $"Add the disconnection of {targets.Length} selected Rx channel(s) to the batch?"),
            L("Confirmer la déconnexion", "Confirm disconnection"),
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (confirmation != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            if (_embedded)
            {
                PatchEditRequest[] edits = targets
                    .Where(target => _session.GetCommittedAssignment(target).IsActive)
                    .Select(PatchEditRequest.Remove)
                    .ToArray();
                if (edits.Length == 0)
                {
                    SetInfo(L("Les RX sélectionnés sont déjà libres.", "The selected Rx channels are already free."));
                    return;
                }

                DirectApplyRequested?.Invoke(this, new DirectPatchRequestEventArgs(edits));
                return;
            }

            int removed = _session.RemoveMany(targets);
            RefreshTargetStates(targets);
            SetInfo(L($"{removed} déconnexion(s) ajoutée(s) au lot.", $"{removed} disconnection(s) added to the batch."));
        }
        catch (Exception exception)
        {
            SetInfo(exception.Message, warning: true);
        }
    }

    private bool SourcesAreUnambiguous(IEnumerable<PatchSourceDescriptor> sources)
    {
        string[] ambiguous = sources
            .Where(source => _ambiguousSourceNames.Contains(source.ChannelName))
            .Select(source => source.ChannelName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (ambiguous.Length == 0)
        {
            return true;
        }

        SetInfo(
            L(
                $"Nom(s) TX ambigu(s) : {string.Join(", ", ambiguous)}. Renommez-les avant le patch.",
                $"Ambiguous Tx name(s): {string.Join(", ", ambiguous)}. Rename them before patching."),
            warning: true);
        return false;
    }

    private void MatrixCellButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton { Tag: PatchMatrixCell cell })
        {
            return;
        }

        SelectMatrixOneToOneStart(cell);
        ExecuteMatrixGesture(cell, cell);
    }

    private void MatrixGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (!TryGetMatrixCell(e.GetPosition(MatrixGrid), out PatchMatrixCell cell))
        {
            return;
        }

        e.Handled = true;
        _matrixGestureActive = true;
        _matrixGestureStart = cell;
        _matrixGestureCurrent = cell;
        SelectMatrixOneToOneStart(cell);
        MatrixGrid.CaptureMouse();
        UpdateMatrixGestureHighlight();
    }

    private void MatrixGrid_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (!_matrixGestureActive)
        {
            return;
        }

        if (e.LeftButton != MouseButtonState.Pressed)
        {
            CancelMatrixGesture();
            return;
        }

        if (TryGetMatrixCell(e.GetPosition(MatrixGrid), out PatchMatrixCell cell)
            && (_matrixGestureCurrent?.SourceIndex != cell.SourceIndex
                || _matrixGestureCurrent.TargetIndex != cell.TargetIndex))
        {
            _matrixGestureCurrent = cell;
            UpdateMatrixGestureHighlight();
        }

        e.Handled = true;
    }

    private void MatrixGrid_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_matrixGestureActive || _matrixGestureStart is null)
        {
            return;
        }

        e.Handled = true;
        PatchMatrixCell start = _matrixGestureStart;
        PatchMatrixCell end = TryGetMatrixCell(e.GetPosition(MatrixGrid), out PatchMatrixCell releasedCell)
            ? releasedCell
            : _matrixGestureCurrent ?? start;

        _matrixGestureActive = false;
        _matrixGestureStart = null;
        _matrixGestureCurrent = null;
        ClearMatrixGestureHighlight();
        MatrixGrid.ReleaseMouseCapture();
        ExecuteMatrixGesture(start, end);
    }

    private void MatrixGrid_LostMouseCapture(object sender, MouseEventArgs e)
    {
        if (_matrixGestureActive)
        {
            CancelMatrixGesture();
        }
    }

    private void MatrixGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if ((Keyboard.Modifiers & ModifierKeys.Control) == 0)
        {
            return;
        }

        SetMatrixZoom(_matrixZoom + (e.Delta > 0 ? MatrixZoomStep : -MatrixZoomStep));
        e.Handled = true;
    }

    private void MatrixZoomOutButton_Click(object sender, RoutedEventArgs e)
    {
        SetMatrixZoom(_matrixZoom - MatrixZoomStep);
    }

    private void MatrixZoomResetButton_Click(object sender, RoutedEventArgs e)
    {
        SetMatrixZoom(1.0);
    }

    private void MatrixZoomInButton_Click(object sender, RoutedEventArgs e)
    {
        SetMatrixZoom(_matrixZoom + MatrixZoomStep);
    }

    private void MatrixZoomFitButton_Click(object sender, RoutedEventArgs e)
    {
        double unscaledWidth = 190 + (_visibleSources.Count * 32d);
        double unscaledHeight = 168 + (_visibleTargets.Count * 26d);
        if (MatrixGrid.ActualWidth <= 0 || MatrixGrid.ActualHeight <= 0)
        {
            SetMatrixZoom(1.0);
            return;
        }

        double fit = Math.Min(
            (MatrixGrid.ActualWidth - 20) / unscaledWidth,
            (MatrixGrid.ActualHeight - 20) / unscaledHeight);
        SetMatrixZoom(fit);
    }

    private void SetMatrixZoom(double requestedZoom)
    {
        double zoom = Math.Clamp(Math.Round(requestedZoom, 2), MinimumMatrixZoom, MaximumMatrixZoom);
        if (Math.Abs(zoom - _matrixZoom) < 0.001)
        {
            return;
        }

        ScrollViewer? scroller = FindVisualChildren<ScrollViewer>(MatrixGrid).FirstOrDefault();
        double horizontalOffset = scroller?.HorizontalOffset ?? 0;
        double verticalOffset = scroller?.VerticalOffset ?? 0;
        _matrixZoom = zoom;
        BuildMatrixColumns();
        MatrixGrid.UpdateLayout();
        scroller?.ScrollToHorizontalOffset(horizontalOffset);
        scroller?.ScrollToVerticalOffset(verticalOffset);
        MatrixZoomTextBlock.Text = $"{Math.Round(_matrixZoom * 100):0} %";
    }

    private void ExecuteMatrixGesture(PatchMatrixCell start, PatchMatrixCell end)
    {
        bool singleCell = start.SourceIndex == end.SourceIndex
            && start.TargetIndex == end.TargetIndex;
        if (_embedded && DirectApplyRequested is not null)
        {
            try
            {
                if (singleCell)
                {
                    ApplyMatrixCellDirectly(start);
                }
                else
                {
                    PatchAssignmentPlan immediatePlan = PatchAssignmentPlanner.PlanMatrixGesture(
                        _visibleSources,
                        _visibleTargets,
                        start.SourceIndex,
                        start.TargetIndex,
                        end.SourceIndex,
                        end.TargetIndex);
                    ApplyPlanImmediately(immediatePlan);
                }
            }
            catch (Exception exception)
            {
                SetInfo(exception.Message, warning: true);
            }

            return;
        }

        if (singleCell && start.IsAssigned)
        {
            _session.Remove(start.Target);
            RefreshTargetStates([start.Target]);
            SetInfo(L("Déconnexion ajoutée au lot.", "Disconnection added to the batch."));
            return;
        }

        try
        {
            PatchAssignmentPlan plan = PatchAssignmentPlanner.PlanMatrixGesture(
                _visibleSources,
                _visibleTargets,
                start.SourceIndex,
                start.TargetIndex,
                end.SourceIndex,
                end.TargetIndex);
            StagePlanAsPreview(plan);
        }
        catch (Exception exception)
        {
            SetInfo(exception.Message, warning: true);
        }
    }

    private void UpdateMatrixGestureHighlight()
    {
        ClearMatrixGestureHighlight();
        if (_matrixGestureStart is null || _matrixGestureCurrent is null)
        {
            return;
        }

        try
        {
            PatchAssignmentPlan plan = PatchAssignmentPlanner.PlanMatrixGesture(
                _visibleSources,
                _visibleTargets,
                _matrixGestureStart.SourceIndex,
                _matrixGestureStart.TargetIndex,
                _matrixGestureCurrent.SourceIndex,
                _matrixGestureCurrent.TargetIndex);
            HashSet<(int SourceId, int TargetId)> plannedCells = plan.Assignments
                .Select(item => (item.Source.DanteId, item.Target.DanteId))
                .ToHashSet();
            foreach (ToggleButton button in FindVisualChildren<ToggleButton>(MatrixGrid)
                         .Where(button => button.Tag is PatchMatrixCell cell
                             && plannedCells.Contains((cell.Source.DanteId, cell.Target.DanteId))))
            {
                HighlightMatrixButton(button, invalid: false);
            }
        }
        catch (InvalidOperationException)
        {
            HighlightMatrixCell(_matrixGestureStart, invalid: true);
            HighlightMatrixCell(_matrixGestureCurrent, invalid: true);
        }
    }

    private void HighlightMatrixCell(PatchMatrixCell cell, bool invalid)
    {
        ToggleButton? button = FindVisualChildren<ToggleButton>(MatrixGrid)
            .FirstOrDefault(candidate => candidate.Tag is PatchMatrixCell visible
                && visible.SourceIndex == cell.SourceIndex
                && visible.TargetIndex == cell.TargetIndex);
        if (button is not null)
        {
            HighlightMatrixButton(button, invalid);
        }
    }

    private void HighlightMatrixButton(ToggleButton button, bool invalid)
    {
        Color background = invalid ? Color.FromRgb(127, 29, 29) : Color.FromRgb(180, 83, 9);
        button.Background = new SolidColorBrush(background);
        button.Foreground = Brushes.White;
        button.BorderBrush = (Brush)FindResource("WarningBrush");
        button.BorderThickness = new Thickness(2);
        _matrixGestureHighlights.Add(button);
    }

    private void ClearMatrixGestureHighlight()
    {
        foreach (ToggleButton button in _matrixGestureHighlights)
        {
            button.ClearValue(Control.BackgroundProperty);
            button.ClearValue(Control.ForegroundProperty);
            button.ClearValue(Control.BorderBrushProperty);
            button.ClearValue(Control.BorderThicknessProperty);
        }

        _matrixGestureHighlights.Clear();
    }

    private void CancelMatrixGesture()
    {
        _matrixGestureActive = false;
        _matrixGestureStart = null;
        _matrixGestureCurrent = null;
        ClearMatrixGestureHighlight();
        if (MatrixGrid.IsMouseCaptured)
        {
            MatrixGrid.ReleaseMouseCapture();
        }
    }

    private void ApplyMatrixCellDirectly(PatchMatrixCell cell)
    {
        if (!SourcesAreUnambiguous([cell.Source]))
        {
            return;
        }

        PatchEditRequest edit;
        if (cell.IsAssigned)
        {
            edit = PatchEditRequest.Remove(cell.Target);
        }
        else
        {
            PlannedPatchAssignment assignment = new(cell.Source, cell.Target);
            PatchBatchPreview preview = _session.BuildCommittedPreview([assignment]);
            PatchConflictResolution resolution = ChooseConflictResolution(preview);
            if (resolution is PatchConflictResolution.Cancel or PatchConflictResolution.Skip)
            {
                SetInfo(
                    L("Patch immédiat annulé.", "Immediate patch cancelled."),
                    warning: resolution == PatchConflictResolution.Cancel);
                return;
            }

            edit = PatchEditRequest.Apply(assignment);
        }

        DirectApplyRequested?.Invoke(this, new DirectPatchRequestEventArgs([edit]));
    }

    private void SelectMatrixOneToOneStart(PatchMatrixCell cell)
    {
        _matrixOneToOneStart = cell;
        UpdateCommandState();
    }

    private void MatrixOneToOneCountTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!_initializing)
        {
            UpdateCommandState();
        }
    }

    private void MatrixOneToOneButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_matrixOneToOneStart is null
                || !int.TryParse(MatrixOneToOneCountTextBox.Text, out int count)
                || count <= 0)
            {
                throw new InvalidOperationException(L(
                    "Cliquez sur le premier point TX/RX de la grille et indiquez un nombre valide.",
                    "Click the first Tx/Rx point in the matrix and enter a valid count."));
            }

            PatchAssignmentPlan plan = PatchAssignmentPlanner.PlanOneToOne(
                _visibleSources,
                _matrixOneToOneStart.Source,
                _visibleTargets,
                _matrixOneToOneStart.Target,
                count);
            if (_embedded)
            {
                ApplyPlanImmediately(plan);
            }
            else
            {
                StagePlanAsPreview(plan);
            }
        }
        catch (Exception exception)
        {
            SetInfo(exception.Message, warning: true);
        }
    }

    private bool TryGetMatrixCell(Point point, out PatchMatrixCell cell)
    {
        DependencyObject? hit = MatrixGrid.InputHitTest(point) as DependencyObject;
        ToggleButton? button = FindVisualParent<ToggleButton>(hit);
        if (button?.Tag is PatchMatrixCell match)
        {
            cell = match;
            return true;
        }

        DataGridCell? dataGridCell = FindVisualParent<DataGridCell>(hit);
        if (dataGridCell?.DataContext is PatchMatrixRow row)
        {
            int sourceIndex = MatrixGrid.Columns.IndexOf(dataGridCell.Column) - 1;
            if (sourceIndex >= 0 && sourceIndex < row.Cells.Count)
            {
                cell = row.Cells[sourceIndex];
                return true;
            }
        }

        cell = null!;
        return false;
    }

    private static T? FindVisualParent<T>(DependencyObject? current)
        where T : DependencyObject
    {
        while (current is not null)
        {
            if (current is T match)
            {
                return match;
            }

            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent)
        where T : DependencyObject
    {
        for (int index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(parent, index);
            if (child is T match)
            {
                yield return match;
            }

            foreach (T descendant in FindVisualChildren<T>(child))
            {
                yield return descendant;
            }
        }
    }

    private void SelectAllTxButton_Click(object sender, RoutedEventArgs e)
    {
        TxChannelListBox.SelectAll();
    }

    private void SelectAllRxButton_Click(object sender, RoutedEventArgs e)
    {
        RxChannelListBox.SelectAll();
    }

    private void ClearSelectionButton_Click(object sender, RoutedEventArgs e)
    {
        TxChannelListBox.UnselectAll();
        RxChannelListBox.UnselectAll();
        UpdateCommandState();
    }

    private void RangeControl_Changed(object sender, RoutedEventArgs e)
    {
        if (!_initializing)
        {
            UpdateCommandState();
        }
    }

    private PatchSourceDescriptor[] SelectedSources()
    {
        return TxChannelListBox.SelectedItems
            .OfType<PatchTxListItem>()
            .Select(row => row.Source)
            .OrderBy(source => source.PositionIndex)
            .ToArray();
    }

    private void InlineChannelNameTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (sender is not TextBox editor)
        {
            return;
        }

        if (e.Key == Key.Escape)
        {
            editor.Text = OriginalInlineChannelName(editor.Tag);
            Keyboard.ClearFocus();
            e.Handled = true;
        }
        else if (e.Key == Key.Enter)
        {
            Keyboard.ClearFocus();
            e.Handled = true;
        }
        else if (e.Key == Key.Tab && _renameChannelAction is not null)
        {
            InlineChannelNavigationTarget? target = BuildInlineNavigationTarget(
                editor.Tag,
                backwards: (Keyboard.Modifiers & ModifierKeys.Shift) != 0);
            if (target is null)
            {
                return;
            }

            CommitInlineChannelRename(editor);
            editor.Tag = null;
            e.Handled = true;
            RequestInlineChannelNavigation(target);
        }
    }

    private void RequestInlineChannelNavigation(InlineChannelNavigationTarget target)
    {
        if (InlineChannelNavigationRequested is not null)
        {
            InlineChannelNavigationRequested.Invoke(
                this,
                new InlineChannelNavigationRequestEventArgs(target.Kind, target.DanteId, target.Matrix));
            return;
        }

        Dispatcher.BeginInvoke(new Action(() => FocusInlineChannelEditor(target)));
    }

    private InlineChannelNavigationTarget? BuildInlineNavigationTarget(object? item, bool backwards)
    {
        DanteChannelKind kind;
        int currentDanteId;
        bool matrix;
        switch (item)
        {
            case PatchRxListItem rx:
                kind = DanteChannelKind.Rx;
                currentDanteId = rx.Target.DanteId;
                matrix = false;
                break;
            case PatchTxListItem tx:
                kind = DanteChannelKind.Tx;
                currentDanteId = tx.Source.DanteId;
                matrix = false;
                break;
            case PatchTargetDescriptor target:
                kind = DanteChannelKind.Rx;
                currentDanteId = target.DanteId;
                matrix = true;
                break;
            case PatchSourceDescriptor source:
                kind = DanteChannelKind.Tx;
                currentDanteId = source.DanteId;
                matrix = true;
                break;
            default:
                return null;
        }

        int[] channelIds = (kind == DanteChannelKind.Tx
                ? _visibleSources.Select(source => source.DanteId)
                : _visibleTargets.Select(target => target.DanteId))
            .ToArray();
        int currentIndex = Array.IndexOf(channelIds, currentDanteId);
        if (currentIndex < 0 || channelIds.Length == 0)
        {
            return null;
        }

        int offset = backwards ? -1 : 1;
        int nextIndex = (currentIndex + offset + channelIds.Length) % channelIds.Length;
        return new InlineChannelNavigationTarget(kind, channelIds[nextIndex], matrix);
    }

    private void FocusInlineChannelEditor(InlineChannelNavigationTarget target)
    {
        if (target.Matrix)
        {
            if (target.Kind == DanteChannelKind.Tx)
            {
                FocusMatrixTxChannelEditor(target.DanteId);
                return;
            }

            PatchMatrixRow? row = _matrixRows.FirstOrDefault(candidate =>
                candidate.Target.DanteId == target.DanteId);
            if (row is null)
            {
                return;
            }

            MatrixGrid.ScrollIntoView(row);
            MatrixGrid.UpdateLayout();
            if (MatrixGrid.ItemContainerGenerator.ContainerFromItem(row) is DataGridRow container)
            {
                TextBox? editor = FindVisualChildren<TextBox>(container).FirstOrDefault();
                editor?.Focus();
                editor?.SelectAll();
            }
            return;
        }

        ListBox list = target.Kind == DanteChannelKind.Tx ? TxChannelListBox : RxChannelListBox;
        object? rowItem = target.Kind == DanteChannelKind.Tx
            ? list.Items.OfType<PatchTxListItem>().FirstOrDefault(row => row.Source.DanteId == target.DanteId)
            : list.Items.OfType<PatchRxListItem>().FirstOrDefault(row => row.Target.DanteId == target.DanteId);
        if (rowItem is null)
        {
            return;
        }

        list.ScrollIntoView(rowItem);
        list.UpdateLayout();
        if (list.ItemContainerGenerator.ContainerFromItem(rowItem) is ListBoxItem containerItem)
        {
            TextBox? editor = FindVisualChildren<TextBox>(containerItem).FirstOrDefault();
            editor?.Focus();
            editor?.SelectAll();
        }
    }

    private void FocusMatrixTxChannelEditor(int danteId)
    {
        int sourceIndex = -1;
        for (int index = 0; index < _visibleSources.Count; index++)
        {
            if (_visibleSources[index].DanteId == danteId)
            {
                sourceIndex = index;
                break;
            }
        }

        if (sourceIndex < 0 || sourceIndex + 1 >= MatrixGrid.Columns.Count)
        {
            return;
        }

        PatchSourceDescriptor source = _visibleSources[sourceIndex];
        DataGridColumn column = MatrixGrid.Columns[sourceIndex + 1];
        if (_matrixRows.Count > 0)
        {
            MatrixGrid.ScrollIntoView(_matrixRows[0], column);
        }

        MatrixGrid.UpdateLayout();
        DataGridColumnHeader? header = FindVisualChildren<DataGridColumnHeader>(MatrixGrid)
            .FirstOrDefault(candidate => ReferenceEquals(candidate.Column, column));
        TextBlock? label = header is null
            ? null
            : FindVisualChildren<TextBlock>(header)
                .FirstOrDefault(candidate =>
                    candidate.Parent is FrameworkElement { Tag: PatchSourceDescriptor descriptor }
                    && descriptor.DanteId == danteId);
        if (label is not null)
        {
            OpenMatrixTxRenameEditor(source, label);
        }
    }

    private void InlineChannelNameTextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not TextBox editor || editor.IsReadOnly)
        {
            return;
        }

        if (FindVisualParent<ListBoxItem>(editor) is ListBoxItem row)
        {
            row.IsSelected = true;
        }

        if (!editor.IsKeyboardFocusWithin)
        {
            e.Handled = true;
            editor.Focus();
            editor.SelectAll();
        }
    }

    private void InlineChannelNameTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (sender is TextBox editor)
        {
            CommitInlineChannelRename(editor);
            if (editor.Tag is PatchTargetDescriptor)
            {
                editor.Background = Brushes.Transparent;
                editor.Foreground = (Brush)FindResource("TextBrush");
                editor.BorderThickness = new Thickness(0);
            }
        }
    }

    private void CommitInlineChannelRename(TextBox editor)
    {
        if (_renameChannelAction is null)
        {
            editor.Text = OriginalInlineChannelName(editor.Tag);
            return;
        }

        string newName = editor.Text.Trim();
        string oldName = OriginalInlineChannelName(editor.Tag);
        if (string.Equals(oldName, newName, StringComparison.Ordinal))
        {
            return;
        }

        string deviceName;
        DanteChannelKind kind;
        int channelIndex;
        if (editor.Tag is PatchRxListItem rx)
        {
            deviceName = rx.Target.DeviceName;
            kind = DanteChannelKind.Rx;
            channelIndex = rx.Target.DanteId;
        }
        else if (editor.Tag is PatchTxListItem tx)
        {
            deviceName = tx.Source.DeviceName;
            kind = DanteChannelKind.Tx;
            channelIndex = tx.Source.DanteId;
        }
        else if (editor.Tag is PatchTargetDescriptor target)
        {
            deviceName = target.DeviceName;
            kind = DanteChannelKind.Rx;
            channelIndex = target.DanteId;
        }
        else if (editor.Tag is PatchSourceDescriptor source)
        {
            deviceName = source.DeviceName;
            kind = DanteChannelKind.Tx;
            channelIndex = source.DanteId;
        }
        else
        {
            return;
        }

        RenameMatrixChannel(deviceName, kind, channelIndex, oldName, newName);
    }

    private static string OriginalInlineChannelName(object? item)
    {
        return item switch
        {
            PatchRxListItem rx => rx.ChannelName,
            PatchTxListItem tx => tx.ChannelName,
            PatchTargetDescriptor target => target.ChannelName,
            PatchSourceDescriptor source => source.ChannelName,
            _ => string.Empty
        };
    }

    private void ChannelSeriesThumb_DragStarted(object sender, DragStartedEventArgs e)
    {
        _channelSeriesDeviceName = null;
        _channelSeriesKind = null;
        _channelSeriesSeeds = [];
        if (_extendChannelSeriesAction is null || sender is not Thumb thumb)
        {
            return;
        }

        if (thumb.Tag is PatchRxListItem rx)
        {
            PatchRxListItem[] selected = RxChannelListBox.SelectedItems
                .OfType<PatchRxListItem>()
                .Where(item => string.Equals(item.Target.DeviceName, rx.Target.DeviceName, StringComparison.OrdinalIgnoreCase))
                .OrderBy(item => item.Target.DanteId)
                .ToArray();
            if (!selected.Contains(rx))
            {
                selected = [rx];
            }
            _channelSeriesDeviceName = rx.Target.DeviceName;
            _channelSeriesKind = DanteChannelKind.Rx;
            _channelSeriesSeeds = selected.Select(item => item.Target.DanteId).ToArray();
        }
        else if (thumb.Tag is PatchTxListItem tx)
        {
            PatchTxListItem[] selected = TxChannelListBox.SelectedItems
                .OfType<PatchTxListItem>()
                .Where(item => string.Equals(item.Source.DeviceName, tx.Source.DeviceName, StringComparison.OrdinalIgnoreCase))
                .OrderBy(item => item.Source.DanteId)
                .ToArray();
            if (!selected.Contains(tx))
            {
                selected = [tx];
            }
            _channelSeriesDeviceName = tx.Source.DeviceName;
            _channelSeriesKind = DanteChannelKind.Tx;
            _channelSeriesSeeds = selected.Select(item => item.Source.DanteId).ToArray();
        }

        if (_channelSeriesSeeds.Length == 0)
        {
            SetInfo(
                L(
                    "Sélectionnez au moins un canal numéroté avant de tirer la poignée de série.",
                    "Select at least one numbered channel before dragging the series handle."),
                warning: true);
            return;
        }

        SetInfo(
            L(
                "Aperçu de série actif : déposez la poignée sur le dernier canal à renommer. Échap annule.",
                "Series preview active: drop the handle on the last channel to rename. Escape cancels."),
            warning: false);
    }

    private void ChannelSeriesThumb_DragCompleted(object sender, DragCompletedEventArgs e)
    {
        if (e.Canceled)
        {
            SetInfo(L("Renommage en série annulé.", "Series rename cancelled."), warning: false);
            return;
        }

        if (_extendChannelSeriesAction is null || _channelSeriesKind is null
            || string.IsNullOrWhiteSpace(_channelSeriesDeviceName) || _channelSeriesSeeds.Length == 0)
        {
            return;
        }

        ListBox list = _channelSeriesKind == DanteChannelKind.Tx ? TxChannelListBox : RxChannelListBox;
        DependencyObject? hit = list.InputHitTest(Mouse.GetPosition(list)) as DependencyObject;
        ListBoxItem? targetRow = FindVisualParent<ListBoxItem>(hit);
        int? targetIndex = targetRow?.DataContext switch
        {
            PatchTxListItem tx => tx.Source.DanteId,
            PatchRxListItem rx => rx.Target.DanteId,
            _ => null
        };
        if (targetIndex is null)
        {
            SetInfo(L("Déposez la poignée sur un canal de la même liste.", "Drop the handle on a channel in the same list."), warning: true);
            return;
        }

        _extendChannelSeriesAction(
            _channelSeriesDeviceName,
            _channelSeriesKind.Value,
            _channelSeriesSeeds,
            targetIndex.Value);
    }

    private void MatrixSeriesThumb_DragStarted(object sender, DragStartedEventArgs e)
    {
        _matrixSeriesDeviceName = null;
        _matrixSeriesKind = null;
        _matrixSeriesSeeds = [];
        if (_extendChannelSeriesAction is null || sender is not Thumb thumb)
        {
            return;
        }

        switch (thumb.Tag)
        {
            case PatchTargetDescriptor target:
                _matrixSeriesDeviceName = target.DeviceName;
                _matrixSeriesKind = DanteChannelKind.Rx;
                _matrixSeriesSeeds = [target.DanteId];
                break;
            case PatchSourceDescriptor source:
                _matrixSeriesDeviceName = source.DeviceName;
                _matrixSeriesKind = DanteChannelKind.Tx;
                _matrixSeriesSeeds = [source.DanteId];
                break;
        }

        if (_matrixSeriesSeeds.Length > 0)
        {
            SetInfo(
                L(
                    "Aperçu de série actif dans la grille : déposez sur le dernier canal à renommer. Échap annule.",
                    "Matrix series preview active: drop on the last channel to rename. Escape cancels."),
                warning: false);
        }
    }

    private void MatrixSeriesThumb_DragCompleted(object sender, DragCompletedEventArgs e)
    {
        if (e.Canceled)
        {
            SetInfo(L("Renommage en série annulé.", "Series rename cancelled."), warning: false);
            return;
        }

        if (_extendChannelSeriesAction is null || _matrixSeriesKind is null
            || string.IsNullOrWhiteSpace(_matrixSeriesDeviceName) || _matrixSeriesSeeds.Length == 0)
        {
            return;
        }

        DependencyObject? hit = MatrixGrid.InputHitTest(Mouse.GetPosition(MatrixGrid)) as DependencyObject;
        int? targetIndex = null;
        string? targetDevice = null;
        if (_matrixSeriesKind == DanteChannelKind.Rx
            && FindVisualParent<DataGridRow>(hit)?.Item is PatchMatrixRow row)
        {
            targetIndex = row.Target.DanteId;
            targetDevice = row.Target.DeviceName;
        }
        else if (_matrixSeriesKind == DanteChannelKind.Tx
                 && FindTaggedVisualParent<PatchSourceDescriptor>(hit) is PatchSourceDescriptor source)
        {
            targetIndex = source.DanteId;
            targetDevice = source.DeviceName;
        }

        if (targetIndex is null
            || !string.Equals(targetDevice, _matrixSeriesDeviceName, StringComparison.OrdinalIgnoreCase))
        {
            SetInfo(
                L("Déposez la poignée sur un canal du même axe et de la même machine.",
                    "Drop the handle on a channel on the same axis and device."),
                warning: true);
            return;
        }

        _extendChannelSeriesAction(
            _matrixSeriesDeviceName,
            _matrixSeriesKind.Value,
            _matrixSeriesSeeds,
            targetIndex.Value);
    }

    private static TTag? FindTaggedVisualParent<TTag>(DependencyObject? current)
        where TTag : class
    {
        while (current is not null)
        {
            if (current is FrameworkElement { Tag: TTag match })
            {
                return match;
            }

            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }

    private PatchTargetDescriptor[] SelectedTargets()
    {
        return RxChannelListBox.SelectedItems
            .OfType<PatchRxListItem>()
            .Select(row => row.Target)
            .OrderBy(target => target.PositionIndex)
            .ToArray();
    }

    private void ResetPendingButton_Click(object sender, RoutedEventArgs e)
    {
        _session.Reset();
        RefreshAllTargetStates();
        SetInfo(L("Tous les changements visuels ont été annulés.", "All visual changes were discarded."));
    }

    private void ApplyButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_session.HasChanges)
        {
            if (_returnEditsOnly)
            {
                // Dans le détail machine, une liste vide est un résultat utile :
                // elle permet d'effacer des patchs précédemment préparés.
                ApplyRequested?.Invoke(this, EventArgs.Empty);
                return;
            }

            SetInfo(L("Aucun changement à appliquer.", "No changes to apply."), warning: true);
            return;
        }

        ApplyRequested?.Invoke(this, EventArgs.Empty);
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        CancelRequested?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateCommandState()
    {
        bool hasRxSelection = RxChannelListBox.SelectedItems.Count > 0;
        bool hasTxSelection = TxChannelListBox.SelectedItems.Count > 0;
        bool hasValidRangeCount = int.TryParse(RangeCountTextBox.Text, out int rangeCount) && rangeCount > 0;
        bool hasValidMatrixCount = int.TryParse(MatrixOneToOneCountTextBox.Text, out int matrixCount)
            && matrixCount > 0;
        PreviewSelectionButton.IsEnabled = hasRxSelection && hasTxSelection;
        ApplySelectionDirectButton.IsEnabled = hasRxSelection && hasTxSelection;
        RemoveSelectedRxButton.IsEnabled = hasRxSelection;
        ClearSelectionButton.IsEnabled = hasRxSelection || hasTxSelection;
        SelectAllTxButton.IsEnabled = _visibleSources.Count > 0;
        SelectAllRxButton.IsEnabled = _visibleTargets.Count > 0;
        PreviewRangeButton.IsEnabled = RangeStartTxComboBox.SelectedItem is PatchSourceDescriptor
            && RangeStartRxComboBox.SelectedItem is PatchTargetDescriptor
            && hasValidRangeCount;
        ApplyRangeDirectButton.IsEnabled = PreviewRangeButton.IsEnabled;
        MatrixOneToOneButton.IsEnabled = _matrixOneToOneStart is not null && hasValidMatrixCount;
        ResetPendingButton.IsEnabled = _session.HasChanges;
        ApplyButton.IsEnabled = _session.HasChanges || _returnEditsOnly;

        string pending = _session.PendingCount == 0
            ? L("Aucun changement en attente", "No pending changes")
            : L(
                $"{_session.PendingCount} changement(s) en attente",
                $"{_session.PendingCount} pending change(s)");
        PendingHeaderTextBlock.Text = pending;
        PendingFooterTextBlock.Text = pending;
        UpdateRangeCapacityStatus();
        UpdateMatrixOneToOneStatus();
    }

    private void UpdateMatrixOneToOneStatus()
    {
        if (_matrixOneToOneStart is null)
        {
            MatrixOneToOneHintTextBlock.Text = L(
                "Cliquez sur le premier point de patch dans la grille.",
                "Click the first patch point in the matrix.");
            MatrixOneToOneHintTextBlock.Foreground = (Brush)FindResource("MutedTextBrush");
            return;
        }

        try
        {
            PatchRangeCapacity capacity = PatchAssignmentPlanner.GetRangeCapacity(
                _visibleSources,
                _matrixOneToOneStart.Source,
                _visibleTargets,
                _matrixOneToOneStart.Target);
            bool validCount = int.TryParse(MatrixOneToOneCountTextBox.Text, out int count)
                && count > 0
                && count <= capacity.MaximumCount;
            MatrixOneToOneHintTextBlock.Text = L(
                $"Départ : {_matrixOneToOneStart.Source.Display} vers {_matrixOneToOneStart.Target.Display}. Maximum : {capacity.MaximumCount}.",
                $"Start: {_matrixOneToOneStart.Source.Display} to {_matrixOneToOneStart.Target.Display}. Maximum: {capacity.MaximumCount}.");
            MatrixOneToOneHintTextBlock.Foreground = validCount
                ? (Brush)FindResource("MutedTextBrush")
                : (Brush)FindResource("WarningBrush");
        }
        catch (InvalidOperationException exception)
        {
            MatrixOneToOneHintTextBlock.Text = exception.Message;
            MatrixOneToOneHintTextBlock.Foreground = (Brush)FindResource("WarningBrush");
        }
    }

    private void UpdateRangeCapacityStatus()
    {
        if (RangeStartTxComboBox.SelectedItem is not PatchSourceDescriptor firstSource
            || RangeStartRxComboBox.SelectedItem is not PatchTargetDescriptor firstTarget)
        {
            RangeCapacityTextBlock.Text = string.Empty;
            return;
        }

        try
        {
            PatchRangeCapacity capacity = PatchAssignmentPlanner.GetRangeCapacity(
                _visibleSources,
                firstSource,
                _visibleTargets,
                firstTarget);
            bool validCount = int.TryParse(RangeCountTextBox.Text, out int count)
                && count > 0
                && count <= capacity.MaximumCount;
            RangeCapacityTextBlock.Text = L(
                $"Capacité : {capacity.MaximumCount} patch(s) 1:1 ({capacity.TxAvailable} TX / {capacity.RxAvailable} RX disponibles).",
                $"Capacity: {capacity.MaximumCount} one-to-one subscription(s) ({capacity.TxAvailable} Tx / {capacity.RxAvailable} Rx available).");
            RangeCapacityTextBlock.Foreground = validCount
                ? (Brush)FindResource("MutedTextBrush")
                : (Brush)FindResource("WarningBrush");
        }
        catch (InvalidOperationException exception)
        {
            RangeCapacityTextBlock.Text = exception.Message;
            RangeCapacityTextBlock.Foreground = (Brush)FindResource("WarningBrush");
        }
    }

    private void ApplyLanguage()
    {
        UpdateVisibleModePresentation();
        TxDeviceLabel.Content = L("Machine émettrice TX", "Transmitting device (Tx)");
        RxDeviceLabel.Content = L("Machine réceptrice RX", "Receiving device (Rx)");
        TxDeviceComboBox.ToolTip = L(
            "Machine qui fournit les canaux TX affichés dans la grille et la liste.",
            "Device providing the Tx channels displayed in the matrix and list.");
        RxDeviceComboBox.ToolTip = L(
            "Machine dont les canaux RX recevront les patchs.",
            "Device whose Rx channels will receive subscriptions.");
        PreviousRxDeviceButton.ToolTip = L("Machine RX précédente", "Previous Rx device");
        NextRxDeviceButton.ToolTip = L("Machine RX suivante", "Next Rx device");
        PreviousTxDeviceButton.ToolTip = L("Machine TX précédente", "Previous Tx device");
        NextTxDeviceButton.ToolTip = L("Machine TX suivante", "Next Tx device");
        SwapDeviceSelectionButton.Content = L("FLIP TX ⇄ RX", "FLIP TX ⇄ RX");
        SwapDeviceSelectionButton.ToolTip = L(
            "Inverser les machines TX et RX sans créer de patch inverse",
            "Swap the Tx and Rx devices without creating reverse subscriptions");
        AssignmentTab.Header = L("Sélection et plage", "Selection and range");
        AssignmentTab.ToolTip = _embedded
            ? L(
                "Sélectionner plusieurs TX et RX ou une plage, puis les appliquer immédiatement.",
                "Select several Tx and Rx channels or a range, then apply them immediately.")
            : L(
                "Sélectionner plusieurs TX et RX et préparer une série.",
                "Select several Tx and Rx channels and stage a range.");
        MatrixTab.Header = L("Grille de patch", "Patch matrix");
        MatrixTab.ToolTip = _embedded
            ? L(
                "Cliquer ou glisser pour appliquer immédiatement un ou plusieurs patchs.",
                "Click or drag to apply one or more patches immediately.")
            : L(
                "Cliquer ou glisser pour préparer un ou plusieurs patchs.",
                "Click or drag to stage one or more patches.");
        TxListHeadingTextBlock.Text = L("Canaux TX disponibles", "Available Tx channels");
        RxListHeadingTextBlock.Text = L("Canaux RX et source actuelle", "Rx channels and current source");
        TxChannelListBox.ToolTip = L(
            "Sélection multiple possible avec Ctrl ou Maj. Les noms de canaux sont modifiables directement.",
            "Use Ctrl or Shift for multiple selection. Channel names can be edited directly.");
        RxChannelListBox.ToolTip = L(
            "Affiche chaque RX et sa source actuelle. Les noms de canaux sont modifiables directement.",
            "Shows every Rx channel and its current source. Channel names can be edited directly.");
        SelectAllTxButton.Content = L("Tout sélectionner", "Select all");
        SelectAllTxButton.ToolTip = L(
            "Sélectionne tous les TX visibles.",
            "Selects all visible Tx channels.");
        SelectAllRxButton.Content = L("Tout sélectionner", "Select all");
        SelectAllRxButton.ToolTip = L(
            "Sélectionne tous les RX visibles.",
            "Selects all visible Rx channels.");
        SelectionHintTextBlock.Text = L(
            "Sélection",
            "Selection");
        PreviewSelectionButton.Content = L("Prévisualiser", "Preview");
        PreviewSelectionButton.ToolTip = L(
            "Ajoute cette sélection au lot sans modifier immédiatement le XML.",
            "Adds this selection to the batch without immediately changing the XML.");
        ApplySelectionDirectButton.Content = L("Patcher la sélection", "Patch selection");
        ApplySelectionDirectButton.ToolTip = _embedded
            ? L(
                "Applique immédiatement les TX sélectionnés aux RX sélectionnés.",
                "Immediately applies the selected Tx channels to the selected Rx channels.")
            : L(
                "Valide cette sélection dans le lot de la fiche machine.",
                "Adds this selection to the device-details batch.");
        RemoveSelectedRxButton.Content = L("Déconnecter", "Disconnect");
        RemoveSelectedRxButton.ToolTip = _embedded
            ? L(
                "Déconnecte immédiatement les RX sélectionnés après confirmation.",
                "Immediately disconnects the selected Rx channels after confirmation.")
            : L(
                "Ajoute au lot la déconnexion des RX sélectionnés.",
                "Adds the disconnection of selected Rx channels to the batch.");
        ClearSelectionButton.Content = L("Effacer", "Clear");
        ClearSelectionButton.ToolTip = L(
            "Efface uniquement la sélection des listes, sans modifier le patch.",
            "Clears only the list selection without changing subscriptions.");
        RangeHeadingTextBlock.Text = L("Patch 1:1", "One-to-one patch");
        RangeStartTxLabel.Content = L("Premier TX", "First Tx");
        RangeStartRxLabel.Content = L("Premier RX", "First Rx");
        RangeCountLabel.Content = L("Nombre", "Count");
        PreviewRangeButton.Content = L("Prévisualiser", "Preview");
        PreviewRangeButton.ToolTip = L(
            "Ajoute la plage 1:1 au lot sans modifier immédiatement le XML.",
            "Adds the one-to-one range to the batch without immediately changing the XML.");
        ApplyRangeDirectButton.Content = L("Patch 1:1", "Patch 1:1");
        ApplyRangeDirectButton.ToolTip = _embedded
            ? L(
                "Applique immédiatement cette plage de patchs 1:1.",
                "Immediately applies this one-to-one patch range.")
            : L(
                "Valide cette plage dans le lot de la fiche machine.",
                "Adds this range to the device-details batch.");
        PreviewGroupBox.Header = L("Lot prévisualisé", "Previewed batch");
        PreviewTargetColumn.Header = L("RX cible", "Target Rx");
        PreviewCurrentColumn.Header = L("Source actuelle", "Current source");
        PreviewProposedColumn.Header = L("Nouvelle source", "New source");
        PreviewActionColumn.Header = L("Action", "Action");
        PreviewSummaryTextBlock.Text = L("Aucun changement dans le lot.", "No changes in the batch.");
        MatrixHintTextBlock.Text = _embedded
            ? L(
                "RX en lignes, TX en colonnes. Un clic ou un glissement applique immédiatement le patch.",
                "Rx channels are rows and Tx channels are columns. A click or drag applies the patch immediately.")
            : L(
                "RX en lignes, TX en colonnes. Un clic ou un glissement prépare le patch.",
                "Rx channels are rows and Tx channels are columns. A click or drag stages the patch.");
        WarnOnExistingPatchCheckBox.Content = L(
            "M'avertir si le RX est déjà patché",
            "Warn me when the Rx channel is already patched");
        WarnOnExistingPatchCheckBox.ToolTip = L(
            "Coché par défaut : demande confirmation avant de remplacer la source actuelle d'un RX. Décochez pour remplacer sans message.",
            "Checked by default: asks for confirmation before replacing an Rx channel's current source. Clear it to replace without a prompt.");
        MatrixOneToOneHeadingTextBlock.Text = L("Patch 1:1 depuis la grille", "One-to-one patch from the matrix");
        MatrixOneToOneCountLabel.Text = L("Nombre", "Count");
        MatrixOneToOneButton.Content = L("PATCH 1:1", "PATCH 1:1");
        MatrixOneToOneButton.ToolTip = _embedded
            ? L(
                "Cliquez sur le premier point TX/RX, choisissez le nombre, puis appliquez immédiatement la série 1:1.",
                "Click the first Tx/Rx point, choose the count, then immediately apply the one-to-one range.")
            : L(
                "Cliquez sur le premier point TX/RX, choisissez le nombre, puis préparez la série 1:1.",
                "Click the first Tx/Rx point, choose the count, then stage the one-to-one range.");
        MatrixZoomOutButton.ToolTip = L("Réduire le zoom de la grille", "Zoom out of the matrix");
        MatrixZoomResetButton.ToolTip = L("Revenir au zoom 100 %", "Reset the matrix to 100% zoom");
        MatrixZoomInButton.ToolTip = L("Augmenter le zoom de la grille", "Zoom into the matrix");
        MatrixZoomFitButton.ToolTip = L("Ajuster toute la grille à la zone visible", "Fit the entire matrix to the visible area");
        ResetPendingButton.Content = L("Annuler les changements visuels", "Discard visual changes");
        ResetPendingButton.ToolTip = L(
            "Vide uniquement le lot prévisualisé. Les patchs déjà appliqués immédiatement restent dans le projet et peuvent être annulés avec Annuler action.",
            "Clears only the previewed batch. Immediately applied subscriptions remain in the project and can be reverted with Undo.");
        CancelButton.Content = L("Fermer sans appliquer", "Close without applying");
        CancelButton.Visibility = _embedded ? Visibility.Collapsed : Visibility.Visible;
        ApplyButton.Content = _embedded
            ? L("Appliquer tout le lot", "Apply entire batch")
            : _returnEditsOnly
            ? L("Valider dans le détail machine", "Return to device details")
            : L("Appliquer tout le lot", "Apply entire batch");
        ApplyButton.ToolTip = L(
            "Applique uniquement les changements actuellement regroupés dans le lot.",
            "Applies only the changes currently grouped in the batch.");

        AutomationProperties.SetName(TxDeviceComboBox, TxDeviceLabel.Content.ToString()!);
        AutomationProperties.SetName(RxDeviceComboBox, RxDeviceLabel.Content.ToString()!);
        AutomationProperties.SetName(PreviousRxDeviceButton, PreviousRxDeviceButton.ToolTip.ToString()!);
        AutomationProperties.SetName(NextRxDeviceButton, NextRxDeviceButton.ToolTip.ToString()!);
        AutomationProperties.SetName(PreviousTxDeviceButton, PreviousTxDeviceButton.ToolTip.ToString()!);
        AutomationProperties.SetName(NextTxDeviceButton, NextTxDeviceButton.ToolTip.ToString()!);
        AutomationProperties.SetName(SwapDeviceSelectionButton, SwapDeviceSelectionButton.ToolTip.ToString()!);
        AutomationProperties.SetName(TxChannelListBox, TxListHeadingTextBlock.Text);
        AutomationProperties.SetName(RxChannelListBox, RxListHeadingTextBlock.Text);
        AutomationProperties.SetName(ApplySelectionDirectButton, ApplySelectionDirectButton.Content.ToString()!);
        AutomationProperties.SetName(ApplyRangeDirectButton, ApplyRangeDirectButton.Content.ToString()!);
        AutomationProperties.SetName(
            WarnOnExistingPatchCheckBox,
            WarnOnExistingPatchCheckBox.Content.ToString()!);
        AutomationProperties.SetName(MatrixGrid, MatrixTab.Header.ToString()!);
        AutomationProperties.SetName(PreviewGrid, PreviewGroupBox.Header.ToString()!);
        AutomationProperties.SetName(MatrixZoomOutButton, L("Réduire le zoom de la grille", "Zoom out of the matrix"));
        AutomationProperties.SetName(MatrixZoomResetButton, L("Réinitialiser le zoom de la grille", "Reset matrix zoom"));
        AutomationProperties.SetName(MatrixZoomInButton, L("Augmenter le zoom de la grille", "Zoom into the matrix"));
        AutomationProperties.SetName(MatrixZoomFitButton, L("Ajuster la grille à la fenêtre", "Fit matrix to window"));
        AutomationProperties.SetName(
            MatrixOneToOneButton,
            _embedded
                ? L("Appliquer un patch un pour un depuis le dernier point cliqué", "Apply a one-to-one patch from the last clicked point")
                : L("Préparer un patch un pour un depuis le dernier point cliqué", "Stage a one-to-one patch from the last clicked point"));
    }

    private void UpdateVisibleModePresentation()
    {
        bool matrix = MatrixTab.IsSelected;
        TitleTextBlock.Text = _embedded && matrix
            ? L("Matrice de patch", "Patch matrix")
            : "Easy patch";
        IntroTextBlock.Text = _embedded
            ? matrix
                ? L(
                    "Un clic ou un glissement modifie immédiatement le patch.",
                    "A click or drag immediately updates the patch.")
                : L(
                    "Sélectionnez des RX et des TX, puis patchez la sélection ou une plage 1:1.",
                    "Select Rx and Tx channels, then patch the selection or a one-to-one range.")
            : L(
                "Préparez les affectations puis validez-les dans la fiche machine.",
                "Stage assignments, then return them to the device details.");

        if (_embedded)
        {
            InfoTextBlock.Text = matrix
                ? L(
                    "Les points de la matrice sont appliqués immédiatement.",
                    "Matrix points are applied immediately.")
                : L(
                    "Les commandes de sélection et de plage sont appliquées immédiatement.",
                    "Selection and range commands are applied immediately.");
        }

        UpdateResponsiveLayout();
    }

    private string TranslateSwapError(string? french)
    {
        if (string.IsNullOrWhiteSpace(french))
        {
            return "Unable to swap the selected devices.";
        }
        if (french.Contains("verrouillée", StringComparison.OrdinalIgnoreCase))
        {
            return "The Rx device is locked in this window.";
        }
        if (french.Contains("Sélectionnez", StringComparison.OrdinalIgnoreCase))
        {
            return "Select a Tx device and an Rx device before swapping them.";
        }
        return "The selected devices cannot be swapped because one of them does not support the required channel direction.";
    }

    private MessageBoxResult ShowConfirmation(
        string message,
        string title,
        MessageBoxButton buttons,
        MessageBoxImage image)
    {
        Window? owner = Window.GetWindow(this);
        return owner is null
            ? MessageBox.Show(message, title, buttons, image)
            : MessageBox.Show(owner, message, title, buttons, image);
    }

    private void ApplyTheme(bool useLightTheme)
    {
        SetBrush("WindowBackgroundBrush", useLightTheme ? Color.FromRgb(244, 247, 251) : Color.FromRgb(16, 20, 31));
        SetBrush("SurfaceBrush", useLightTheme ? Colors.White : Color.FromRgb(23, 29, 43));
        SetBrush("SurfaceAltBrush", useLightTheme ? Color.FromRgb(233, 238, 246) : Color.FromRgb(32, 40, 56));
        SetBrush("TextBrush", useLightTheme ? Color.FromRgb(17, 24, 39) : Color.FromRgb(246, 248, 251));
        SetBrush("MutedTextBrush", useLightTheme ? Color.FromRgb(71, 85, 105) : Color.FromRgb(170, 180, 197));
        SetBrush("BorderLineBrush", useLightTheme ? Color.FromRgb(203, 213, 225) : Color.FromRgb(51, 64, 87));
        SetBrush("WarningBrush", useLightTheme ? Color.FromRgb(180, 83, 9) : Color.FromRgb(245, 158, 11));
    }

    private void SetBrush(string key, Color color)
    {
        Resources[key] = new SolidColorBrush(color);
    }

    private void SetInfo(string message, bool warning = false)
    {
        InfoTextBlock.Text = message;
        InfoTextBlock.Foreground = warning
            ? (Brush)FindResource("WarningBrush")
            : (Brush)FindResource("MutedTextBrush");
    }

    private string BuildCellToolTip(
        PatchSourceDescriptor source,
        PatchTargetDescriptor target,
        bool isAssigned,
        bool isPending)
    {
        string action = _embedded
            ? isAssigned
                ? L("Cliquer pour déconnecter immédiatement, ou glisser pour appliquer une série", "Click to disconnect immediately, or drag to apply a range")
                : L("Cliquer pour affecter immédiatement, ou glisser pour appliquer une série", "Click to assign immediately, or drag to apply a range")
            : isAssigned
                ? L("Cliquer pour préparer la déconnexion, ou glisser pour préparer une série", "Click to stage a disconnection, or drag to stage a range")
                : L("Cliquer pour préparer l'affectation, ou glisser pour préparer une série", "Click to stage an assignment, or drag to stage a range");
        string pending = isPending ? L(" - changement en attente", " - pending change") : string.Empty;
        return $"{target.FullDisplay} <- {source.FullDisplay}\n{action}{pending}";
    }

    private string BuildCellAutomationName(
        PatchSourceDescriptor source,
        PatchTargetDescriptor target,
        bool isAssigned)
    {
        return isAssigned
            ? L($"Patch actif : {source.FullDisplay} vers {target.FullDisplay}", $"Active subscription: {source.FullDisplay} to {target.FullDisplay}")
            : L($"Affecter {source.FullDisplay} vers {target.FullDisplay}", $"Assign {source.FullDisplay} to {target.FullDisplay}");
    }

    private static string? FindDeviceName(IEnumerable<string> names, string? requested)
    {
        return names.FirstOrDefault(name => string.Equals(name, requested, StringComparison.OrdinalIgnoreCase));
    }

    private string L(string french, string english)
    {
        return _language == UiLanguage.English ? english : french;
    }

    private sealed class PatchRxListItem(
        PatchTargetDescriptor target,
        string channelName,
        string sourceDisplay) : INotifyPropertyChanged
    {
        private string _sourceDisplay = sourceDisplay;

        public PatchTargetDescriptor Target { get; } = target;

        public string ChannelName { get; } = channelName;

        public string SourceDisplay
        {
            get => _sourceDisplay;
            set
            {
                if (string.Equals(_sourceDisplay, value, StringComparison.Ordinal))
                {
                    return;
                }

                _sourceDisplay = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SourceDisplay)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    private sealed record PatchTxListItem(PatchSourceDescriptor Source)
    {
        public string ChannelName => Source.ChannelName;

        public bool CanExtendNameSeries => Source.CanExtendNameSeries;
    }

    private sealed record PatchPreviewRow(
        string Target,
        string Current,
        string Proposed,
        string Action,
        string ActionKey);

    private sealed record InlineChannelNavigationTarget(
        DanteChannelKind Kind,
        int DanteId,
        bool Matrix);

    private sealed class PatchMatrixRow(
        PatchTargetDescriptor target,
        bool isPending,
        IReadOnlyList<PatchMatrixCell> cells) : INotifyPropertyChanged
    {
        private bool _isPending = isPending;

        public PatchTargetDescriptor Target { get; } = target;

        public IReadOnlyList<PatchMatrixCell> Cells { get; } = cells;

        public bool IsPending
        {
            get => _isPending;
            set
            {
                if (_isPending == value)
                {
                    return;
                }

                _isPending = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsPending)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    private sealed class PatchMatrixCell(
        PatchSourceDescriptor source,
        PatchTargetDescriptor target,
        int sourceIndex,
        int targetIndex,
        bool isAssigned,
        bool isPending,
        string marker,
        string toolTip,
        string automationName) : INotifyPropertyChanged
    {
        private bool _isAssigned = isAssigned;
        private bool _isPending = isPending;
        private string _marker = marker;
        private string _toolTip = toolTip;
        private string _automationName = automationName;

        public PatchSourceDescriptor Source { get; } = source;

        public PatchTargetDescriptor Target { get; } = target;

        public int SourceIndex { get; } = sourceIndex;

        public int TargetIndex { get; } = targetIndex;

        public bool IsAssigned
        {
            get => _isAssigned;
            private set => SetField(ref _isAssigned, value, nameof(IsAssigned));
        }

        public bool IsPending
        {
            get => _isPending;
            private set => SetField(ref _isPending, value, nameof(IsPending));
        }

        public string Marker
        {
            get => _marker;
            private set => SetField(ref _marker, value, nameof(Marker));
        }

        public string ToolTip
        {
            get => _toolTip;
            private set => SetField(ref _toolTip, value, nameof(ToolTip));
        }

        public string AutomationName
        {
            get => _automationName;
            private set => SetField(ref _automationName, value, nameof(AutomationName));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void Update(
            bool assigned,
            bool pending,
            string markerValue,
            string toolTipValue,
            string automationNameValue)
        {
            IsAssigned = assigned;
            IsPending = pending;
            Marker = markerValue;
            ToolTip = toolTipValue;
            AutomationName = automationNameValue;
        }

        private void SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return;
            }

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
