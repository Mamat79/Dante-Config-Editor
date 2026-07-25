using System.Text.Json;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.VisualTree;
using DanteConfigEditor.Models;
using DanteConfigEditor.Services;

namespace DanteConfigEditor.Mac;

public sealed partial class PatchWorkspaceDialog : Window
{
    private const int MaxMatrixChannels = 128;
    private const double MinimumMatrixZoom = 0.5;
    private const double MaximumMatrixZoom = 2.0;
    private const double MatrixZoomStep = 0.1;
    private static readonly DataFormat<string> DragDataFormat =
        DataFormat.CreateStringApplicationFormat("DanteConfigEditor.PatchSources");

    private UiLanguage _language = UiLanguage.French;
    private DanteProject _project = null!;
    private PatchWorkspaceSession _session = null!;
    private Func<IReadOnlyList<PatchEditRequest>, Task>? _immediateApply;
    private readonly HashSet<string> _ambiguousSourceNames = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<MatrixCellKey, PatchMatrixCell> _matrixCells = [];
    private readonly Dictionary<int, TextBlock> _matrixRxHeaders = [];
    private IReadOnlyList<PatchSourceDescriptor> _visibleSources = [];
    private IReadOnlyList<PatchTargetDescriptor> _visibleTargets = [];
    private IReadOnlyList<PatchRxListItem> _rxRows = [];
    private Point _dragStart;
    private bool _dragReady;
    private bool _dragInProgress;
    private double _matrixZoom = 1.0;
    private bool _initializing = true;

    internal int MatrixBuildCount { get; private set; }

    public PatchWorkspaceDialog()
    {
        InitializeComponent();
        TabControl tabs = FindControl<TabControl>("PatchModeTabs")!;
        TabItem matrix = FindControl<TabItem>("MatrixTab")!;
        tabs.Items.Remove(matrix);
        tabs.Items.Insert(0, matrix);
        tabs.SelectedItem = matrix;
    }

    public PatchWorkspaceDialog(
        UiLanguage language,
        DanteProject project,
        string? initialTxDeviceName = null,
        string? initialRxDeviceName = null,
        Func<IReadOnlyList<PatchEditRequest>, Task>? immediateApply = null)
        : this()
    {
        _language = language;
        _project = project ?? throw new ArgumentNullException(nameof(project));
        _session = new PatchWorkspaceSession(project.PatchMatrix.Subscriptions);
        _immediateApply = immediateApply;

        FindControl<ListBox>("TxChannelList")!.SelectionChanged += TxChannelList_SelectionChanged;
        ConfigureWorkflowPresentation();
        ApplyLanguage();
        PopulateDeviceSelectors(initialTxDeviceName, initialRxDeviceName);
        _initializing = false;
        RefreshSourceChannels();
        RefreshTargetRows();
    }

    public IReadOnlyList<PatchEditRequest> Edits => _session.Edits;

    public bool WarnOnExistingPatch =>
        FindControl<CheckBox>("WarnOnExistingPatchCheckBox")!.IsChecked != false;

    private T? FindControl<T>(string name) where T : Control => ControlExtensions.FindControl<T>(this, name);

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void ConfigureWorkflowPresentation()
    {
        if (_immediateApply is null)
        {
            return;
        }

        FindControl<TextBlock>("PendingHeaderText")!.IsVisible = false;
        FindControl<TextBlock>("PendingFooterText")!.IsVisible = false;
        FindControl<Button>("ResetPendingButton")!.IsVisible = false;
        FindControl<Button>("ApplyButton")!.IsVisible = false;
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

        ComboBox txCombo = FindControl<ComboBox>("TxDeviceCombo")!;
        ComboBox rxCombo = FindControl<ComboBox>("RxDeviceCombo")!;
        txCombo.ItemsSource = txDevices;
        rxCombo.ItemsSource = rxDevices;
        txCombo.SelectedItem = FindDeviceName(txDevices, initialTxDeviceName) ?? txDevices.FirstOrDefault();
        rxCombo.SelectedItem = FindDeviceName(rxDevices, initialRxDeviceName) ?? rxDevices.FirstOrDefault();
    }

    private void SwapDeviceSelectionButton_Click(object? sender, RoutedEventArgs e)
    {
        ComboBox txCombo = FindControl<ComboBox>("TxDeviceCombo")!;
        ComboBox rxCombo = FindControl<ComboBox>("RxDeviceCombo")!;
        PatchDeviceSelectionSwapResult result = PatchDeviceSelectionSwapper.TrySwap(
            txCombo.SelectedItem as string,
            rxCombo.SelectedItem as string,
            txCombo.ItemsSource?.OfType<string>() ?? [],
            rxCombo.ItemsSource?.OfType<string>() ?? []);
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
        txCombo.SelectedItem = result.TxDeviceName;
        rxCombo.SelectedItem = result.RxDeviceName;
        _initializing = false;
        RefreshSourceChannels();
        RefreshTargetRows();
        SetInfo(L(
            "Machines TX et RX inversées. Le lot en attente est conservé.",
            "Tx and Rx devices swapped. The pending batch was preserved."));
    }

    private void TxDeviceCombo_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_initializing)
        {
            return;
        }

        RefreshSourceChannels();
        RefreshTargetRows();
    }

    private void RxDeviceCombo_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!_initializing)
        {
            RefreshTargetRows();
        }
    }

    private void TxChannelList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        UpdateCommandState();
    }

    private void RxChannelList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        UpdateCommandState();
    }

    private void RefreshSourceChannels()
    {
        ComboBox rangeCombo = FindControl<ComboBox>("OneToOneFirstTxCombo")!;
        int? selectedRangeDanteId = (rangeCombo.SelectedItem as PatchSourceDescriptor)?.DanteId;
        string? deviceName = FindControl<ComboBox>("TxDeviceCombo")!.SelectedItem as string;
        DanteDevice? device = _project.FindDevice(deviceName);
        _visibleSources = device?.TxChannels
            .Select(channel => new PatchSourceDescriptor(
                device.Name,
                channel.DanteId,
                channel.PositionIndex,
                channel.DisplayName))
            .OrderBy(channel => channel.PositionIndex)
            .ToArray() ?? [];
        FindControl<ListBox>("TxChannelList")!.ItemsSource = _visibleSources;
        rangeCombo.ItemsSource = _visibleSources;
        rangeCombo.SelectedItem = _visibleSources.FirstOrDefault(source => source.DanteId == selectedRangeDanteId)
            ?? _visibleSources.FirstOrDefault();

        _ambiguousSourceNames.Clear();
        foreach (IGrouping<string, PatchSourceDescriptor> duplicate in _visibleSources
                     .GroupBy(source => source.ChannelName, StringComparer.OrdinalIgnoreCase)
                     .Where(group => group.Count() > 1))
        {
            _ambiguousSourceNames.Add(duplicate.Key);
        }

        if (_ambiguousSourceNames.Count > 0)
        {
            SetInfo(
                L(
                    "Cette machine contient des noms TX en double. Renommez-les avant le patch.",
                    "This device contains duplicate Tx names. Rename them before patching."),
                warning: true);
        }
        else
        {
            SetInfo(L(
                "Sélectionnez plusieurs TX avec Cmd, Ctrl ou Maj, puis choisissez le premier RX.",
                "Select multiple Tx channels with Cmd, Ctrl, or Shift, then choose the first Rx."));
        }
    }

    private void RefreshTargetRows()
    {
        ListBox rxList = FindControl<ListBox>("RxChannelList")!;
        int? selectedDanteId = (rxList.SelectedItem as PatchRxListItem)?.Target.DanteId;
        ComboBox rangeCombo = FindControl<ComboBox>("OneToOneFirstRxCombo")!;
        int? selectedRangeDanteId = (rangeCombo.SelectedItem as PatchTargetDescriptor)?.DanteId;
        string? deviceName = FindControl<ComboBox>("RxDeviceCombo")!.SelectedItem as string;
        DanteDevice? device = _project.FindDevice(deviceName);
        _visibleTargets = device?.RxChannels
            .Select(channel => new PatchTargetDescriptor(
                device.Name,
                channel.DanteId,
                channel.PositionIndex,
                channel.DisplayName))
            .OrderBy(channel => channel.PositionIndex)
            .ToArray() ?? [];

        _rxRows = _visibleTargets.Select(BuildRxListItem).ToArray();
        rxList.ItemsSource = _rxRows;
        rxList.SelectedItem = _rxRows.FirstOrDefault(row => row.Target.DanteId == selectedDanteId)
            ?? _rxRows.FirstOrDefault();
        rangeCombo.ItemsSource = _visibleTargets;
        rangeCombo.SelectedItem = _visibleTargets.FirstOrDefault(target => target.DanteId == selectedRangeDanteId)
            ?? _visibleTargets.FirstOrDefault();

        BuildMatrix();
        UpdateCommandState();
    }

    private PatchRxListItem BuildRxListItem(PatchTargetDescriptor target)
    {
        EffectivePatchAssignment assignment = _session.GetEffectiveAssignment(target);
        return new PatchRxListItem(target, BuildRxDisplay(target, assignment));
    }

    private void BuildMatrix()
    {
        MatrixBuildCount++;
        Grid matrix = FindControl<Grid>("MatrixPanel")!;
        Grid txHeaders = FindControl<Grid>("MatrixTxHeaderPanel")!;
        Grid rxHeaders = FindControl<Grid>("MatrixRxHeaderPanel")!;
        matrix.Children.Clear();
        matrix.RowDefinitions.Clear();
        matrix.ColumnDefinitions.Clear();
        txHeaders.Children.Clear();
        txHeaders.RowDefinitions.Clear();
        txHeaders.ColumnDefinitions.Clear();
        rxHeaders.Children.Clear();
        rxHeaders.RowDefinitions.Clear();
        rxHeaders.ColumnDefinitions.Clear();
        _matrixCells.Clear();
        _matrixRxHeaders.Clear();

        PatchSourceDescriptor[] sources = _visibleSources.Take(MaxMatrixChannels).ToArray();
        PatchTargetDescriptor[] targets = _visibleTargets.Take(MaxMatrixChannels).ToArray();
        txHeaders.RowDefinitions.Add(new RowDefinition { Height = new GridLength(54 * _matrixZoom) });
        foreach (PatchSourceDescriptor _ in sources)
        {
            matrix.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(92 * _matrixZoom) });
            txHeaders.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(92 * _matrixZoom) });
        }

        double rxHeaderWidth = Math.Max(150, 220 * _matrixZoom);
        rxHeaders.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(rxHeaderWidth) });
        FindControl<Grid>("MatrixViewport")!.ColumnDefinitions[0].Width = new GridLength(rxHeaderWidth);
        for (int sourceIndex = 0; sourceIndex < sources.Length; sourceIndex++)
        {
            PatchSourceDescriptor source = sources[sourceIndex];
            TextBlock header = AddMatrixText(
                txHeaders,
                $"TX {source.DanteId}\n{source.ChannelName}",
                0,
                sourceIndex,
                FontWeight.SemiBold,
                pending: false);
            header.Width = 86 * _matrixZoom;
            header.FontSize = Math.Max(9, 12 * _matrixZoom);
            header.TextAlignment = TextAlignment.Center;
            header.TextTrimming = TextTrimming.CharacterEllipsis;
            ToolTip.SetTip(header, source.FullDisplay);
            AutomationProperties.SetName(header, source.FullDisplay);
        }

        for (int targetIndex = 0; targetIndex < targets.Length; targetIndex++)
        {
            PatchTargetDescriptor target = targets[targetIndex];
            EffectivePatchAssignment assignment = _session.GetEffectiveAssignment(target);
            int assignedSourceIndex = FindAssignedSourceIndex(assignment, sources);
            matrix.RowDefinitions.Add(new RowDefinition { Height = new GridLength(38 * _matrixZoom) });
            rxHeaders.RowDefinitions.Add(new RowDefinition { Height = new GridLength(38 * _matrixZoom) });
            TextBlock rxHeader = AddMatrixText(
                rxHeaders,
                target.Display,
                targetIndex,
                0,
                FontWeight.Normal,
                assignment.IsPending);
            ToolTip.SetTip(rxHeader, target.FullDisplay);
            AutomationProperties.SetName(rxHeader, target.FullDisplay);
            _matrixRxHeaders[targetIndex] = rxHeader;

            for (int sourceIndex = 0; sourceIndex < sources.Length; sourceIndex++)
            {
                PatchSourceDescriptor source = sources[sourceIndex];
                bool isAssigned = sourceIndex == assignedSourceIndex;
                PatchMatrixCell cell = new(source, target, sourceIndex, targetIndex, isAssigned);
                Button button = new()
                {
                    Content = isAssigned ? "●" : string.Empty,
                    DataContext = cell,
                    Width = 84 * _matrixZoom,
                    Height = 30 * _matrixZoom,
                    FontSize = Math.Max(9, 12 * _matrixZoom),
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                };
                button.Classes.Add("matrixCell");
                if (isAssigned)
                {
                    button.Classes.Add("assigned");
                }

                button.Click += MatrixCellButton_Click;
                ToolTip.SetTip(button, BuildCellToolTip(source, target, isAssigned, assignment.IsPending));
                AutomationProperties.SetName(button, BuildCellAutomationName(source, target, isAssigned));
                Grid.SetRow(button, targetIndex);
                Grid.SetColumn(button, sourceIndex);
                matrix.Children.Add(button);
                cell.Button = button;
                _matrixCells[new MatrixCellKey(sourceIndex, targetIndex)] = cell;
            }
        }

        FindControl<TextBlock>("MatrixCornerHeader")!.Text = L("Canal RX", "Rx channel");
        UpdateMatrixHint(sources.Length, targets.Length);
    }

    private void RefreshTargetStates(IEnumerable<PatchTargetDescriptor> targets)
    {
        HashSet<int> danteIds = targets
            .Where(target => string.Equals(
                target.DeviceName,
                FindControl<ComboBox>("RxDeviceCombo")!.SelectedItem as string,
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

        UpdateCommandState();
    }

    private void RefreshAllTargetStates()
    {
        for (int targetIndex = 0; targetIndex < _visibleTargets.Count; targetIndex++)
        {
            RefreshTargetState(targetIndex);
        }

        UpdateCommandState();
    }

    private void RefreshTargetState(int targetIndex)
    {
        if (targetIndex < 0 || targetIndex >= _visibleTargets.Count)
        {
            return;
        }

        PatchTargetDescriptor target = _visibleTargets[targetIndex];
        EffectivePatchAssignment assignment = _session.GetEffectiveAssignment(target);
        if (targetIndex < _rxRows.Count)
        {
            _rxRows[targetIndex].Display = BuildRxDisplay(target, assignment);
        }

        if (targetIndex >= MaxMatrixChannels)
        {
            return;
        }

        if (_matrixRxHeaders.TryGetValue(targetIndex, out TextBlock? rowHeader))
        {
            SetPendingRowClass(rowHeader, assignment.IsPending);
        }

        int assignedSourceIndex = FindAssignedSourceIndex(
            assignment,
            _visibleSources.Take(MaxMatrixChannels).ToArray());
        for (int sourceIndex = 0;
             sourceIndex < Math.Min(_visibleSources.Count, MaxMatrixChannels);
             sourceIndex++)
        {
            if (!_matrixCells.TryGetValue(new MatrixCellKey(sourceIndex, targetIndex), out PatchMatrixCell? cell))
            {
                continue;
            }

            bool isAssigned = sourceIndex == assignedSourceIndex;
            cell.IsAssigned = isAssigned;
            cell.Button.Content = isAssigned ? "●" : string.Empty;
            SetAssignedCellClass(cell.Button, isAssigned);
            ToolTip.SetTip(
                cell.Button,
                BuildCellToolTip(cell.Source, cell.Target, isAssigned, assignment.IsPending));
            AutomationProperties.SetName(
                cell.Button,
                BuildCellAutomationName(cell.Source, cell.Target, isAssigned));
        }
    }

    private string BuildRxDisplay(PatchTargetDescriptor target, EffectivePatchAssignment assignment)
    {
        string source = assignment.IsActive
            ? $"{assignment.TxDeviceName} / {assignment.TxChannelName}"
            : L("Libre", "Free");
        string marker = assignment.IsPending ? L("  [modifié]", "  [changed]") : string.Empty;
        return $"{target.Display}   <-   {source}{marker}";
    }

    private static void SetAssignedCellClass(Button button, bool assigned)
    {
        if (assigned)
        {
            if (!button.Classes.Contains("assigned"))
            {
                button.Classes.Add("assigned");
            }
        }
        else
        {
            button.Classes.Remove("assigned");
        }
    }

    private static void SetPendingRowClass(TextBlock rowHeader, bool pending)
    {
        if (pending)
        {
            if (!rowHeader.Classes.Contains("pendingRow"))
            {
                rowHeader.Classes.Add("pendingRow");
            }
        }
        else
        {
            rowHeader.Classes.Remove("pendingRow");
        }
    }

    private TextBlock AddMatrixText(
        Grid matrix,
        string text,
        int row,
        int column,
        FontWeight weight,
        bool pending)
    {
        TextBlock block = new()
        {
            Text = text,
            FontWeight = weight,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = new Thickness(8, 4),
            TextTrimming = TextTrimming.CharacterEllipsis
        };
        if (pending)
        {
            block.Classes.Add("pendingRow");
        }

        Grid.SetRow(block, row);
        Grid.SetColumn(block, column);
        matrix.Children.Add(block);
        return block;
    }

    private static int FindAssignedSourceIndex(
        EffectivePatchAssignment assignment,
        IReadOnlyList<PatchSourceDescriptor> sources)
    {
        if (!assignment.IsActive)
        {
            return -1;
        }

        for (int index = 0; index < sources.Count; index++)
        {
            PatchSourceDescriptor source = sources[index];
            if (string.Equals(source.DeviceName, assignment.TxDeviceName, StringComparison.OrdinalIgnoreCase)
                && string.Equals(source.ChannelName, assignment.TxChannelName, StringComparison.Ordinal))
            {
                return index;
            }
        }

        return -1;
    }

    private async void AssignSequentialButton_Click(object? sender, RoutedEventArgs e)
    {
        PatchSourceDescriptor[] sources = SelectedSources();
        if (sources.Length == 0)
        {
            SetInfo(L("Sélectionnez au moins un canal TX.", "Select at least one Tx channel."), warning: true);
            return;
        }

        if (FindControl<ListBox>("RxChannelList")!.SelectedItem is not PatchRxListItem targetRow)
        {
            SetInfo(L("Sélectionnez le premier canal RX.", "Select the first Rx channel."), warning: true);
            return;
        }

        await AssignSourcesAsync(sources, targetRow.Target);
    }

    private async void RemoveSelectedRxButton_Click(object? sender, RoutedEventArgs e)
    {
        if (FindControl<ListBox>("RxChannelList")!.SelectedItem is not PatchRxListItem targetRow)
        {
            SetInfo(L("Sélectionnez un canal RX à déconnecter.", "Select an Rx channel to disconnect."), warning: true);
            return;
        }

        if (_immediateApply is not null)
        {
            if (!_session.GetCommittedAssignment(targetRow.Target).IsActive)
            {
                SetInfo(L("Ce canal RX est déjà libre.", "This Rx channel is already free."));
                return;
            }

            await ApplyImmediateEditsAsync([PatchEditRequest.Remove(targetRow.Target)]);
            return;
        }

        _session.Remove(targetRow.Target);
        RefreshTargetStates([targetRow.Target]);
        SetInfo(L("Déconnexion préparée.", "Disconnection staged."));
    }

    private async Task AssignSourcesAsync(
        IReadOnlyList<PatchSourceDescriptor> sources,
        PatchTargetDescriptor firstTarget)
    {
        if (!SourcesAreUnambiguous(sources))
        {
            return;
        }

        try
        {
            SequentialPatchPlan sequential = PatchAssignmentPlanner.PlanSequential(sources, _visibleTargets, firstTarget);
            bool staged = await PreviewAndStagePlanAsync(
                new PatchAssignmentPlan(sequential.Assignments),
                requireConfirmation: true);
            if (staged && sequential.UnassignedSources.Count > 0)
            {
                SetInfo(L(
                    $"{sequential.Assignments.Count} affectation(s) ajoutée(s), {sequential.UnassignedSources.Count} TX sans RX disponible.",
                    $"{sequential.Assignments.Count} assignment(s) added, {sequential.UnassignedSources.Count} Tx channel(s) without an available Rx."),
                    warning: true);
            }
        }
        catch (Exception exception)
        {
            SetInfo(exception.Message, warning: true);
        }
    }

    private void OneToOneControl_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!_initializing)
        {
            UpdateCommandState();
        }
    }

    private void OneToOneCountTextBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (!_initializing)
        {
            UpdateCommandState();
        }
    }

    private async void PreviewOneToOneButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            await PreviewAndStagePlanAsync(BuildOneToOnePlan(), requireConfirmation: true);
        }
        catch (Exception exception)
        {
            SetInfo(exception.Message, warning: true);
        }
    }

    private PatchAssignmentPlan BuildOneToOnePlan()
    {
        if (FindControl<ComboBox>("OneToOneFirstTxCombo")!.SelectedItem is not PatchSourceDescriptor firstSource
            || FindControl<ComboBox>("OneToOneFirstRxCombo")!.SelectedItem is not PatchTargetDescriptor firstTarget
            || !int.TryParse(FindControl<TextBox>("OneToOneCountTextBox")!.Text, out int count))
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

    private async Task<bool> PreviewAndStagePlanAsync(
        PatchAssignmentPlan plan,
        bool requireConfirmation)
    {
        if (!SourcesAreUnambiguous(plan.Assignments.Select(assignment => assignment.Source)))
        {
            return false;
        }

        if (_immediateApply is not null)
        {
            return await ApplyPlanImmediatelyAsync(plan);
        }

        PatchBatchPreview preview = _session.BuildPreview(plan.Assignments);
        PatchConflictResolution resolution;
        if (preview.HasConflicts)
        {
            resolution = await ChooseConflictResolutionAsync(preview);
        }
        else
        {
            if (requireConfirmation)
            {
                PlannedPatchAssignment first = plan.Assignments[0];
                PlannedPatchAssignment last = plan.Assignments[^1];
                bool confirmed = await MessageDialog.ShowAsync(
                    this,
                    L("Prévisualisation du patch", "Patch preview"),
                    L(
                        $"{plan.Assignments.Count} affectation(s) seront ajoutée(s) au lot.\n\n" +
                        $"Début : {first.Source.FullDisplay} -> {first.Target.FullDisplay}\n" +
                        $"Fin : {last.Source.FullDisplay} -> {last.Target.FullDisplay}\n\n" +
                        "Le XML ne sera modifié qu'avec « Appliquer au projet ».",
                        $"{plan.Assignments.Count} assignment(s) will be added to the batch.\n\n" +
                        $"First: {first.Source.FullDisplay} -> {first.Target.FullDisplay}\n" +
                        $"Last: {last.Source.FullDisplay} -> {last.Target.FullDisplay}\n\n" +
                        "The XML will only change after choosing “Apply to project”."),
                    L("Ajouter au lot", "Add to batch"),
                    L("Annuler", "Cancel"));
                if (!confirmed)
                {
                    SetInfo(L(
                        "Prévisualisation annulée : le lot reste inchangé.",
                        "Preview cancelled: the batch is unchanged."));
                    return false;
                }
            }

            resolution = PatchConflictResolution.Replace;
        }

        PatchStageResult result = _session.StagePreview(preview, resolution);
        if (result.IsCancelled)
        {
            SetInfo(L(
                "Prévisualisation annulée : le lot reste inchangé.",
                "Preview cancelled: the batch is unchanged."));
            return false;
        }

        RefreshTargetStates(plan.Assignments.Select(assignment => assignment.Target));
        SetInfo(L(
            $"{result.StagedCount} changement(s) ajouté(s) au lot, {result.SkippedConflictCount} conflit(s) ignoré(s), {result.UnchangedCount} ligne(s) inchangée(s).",
            $"{result.StagedCount} change(s) added to the batch, {result.SkippedConflictCount} conflict(s) skipped, {result.UnchangedCount} row(s) unchanged."),
            warning: result.SkippedConflictCount > 0);
        return true;
    }

    private async Task<bool> ApplyPlanImmediatelyAsync(PatchAssignmentPlan plan)
    {
        PatchBatchPreview preview = _session.BuildCommittedPreview(plan.Assignments);
        PatchConflictResolution resolution = preview.HasConflicts && WarnOnExistingPatch
            ? await ChooseConflictResolutionAsync(preview)
            : PatchConflictResolution.Replace;
        if (resolution == PatchConflictResolution.Cancel)
        {
            SetInfo(L("Patch annulé.", "Patch cancelled."), warning: true);
            return false;
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
            return false;
        }

        await ApplyImmediateEditsAsync(edits);
        return true;
    }

    private async Task ApplyImmediateEditsAsync(IReadOnlyList<PatchEditRequest> edits)
    {
        if (_immediateApply is null || edits.Count == 0)
        {
            return;
        }

        await _immediateApply(edits);
        _session = new PatchWorkspaceSession(_project.PatchMatrix.Subscriptions);
        RefreshSourceChannels();
        RefreshTargetRows();
    }

    private async Task<PatchConflictResolution> ChooseConflictResolutionAsync(PatchBatchPreview preview)
    {
        MessageDialogChoice choice = await MessageDialog.ShowChoiceAsync(
            this,
            L("Conflits de patch", "Patch conflicts"),
            L(
                $"{preview.ReplaceCount} RX possède(nt) déjà une source.\n\n" +
                "Remplacer = remplacer ces abonnements.\n" +
                "Ignorer = conserver les abonnements existants.\n" +
                (_immediateApply is null
                    ? "Annuler = ne rien ajouter au lot."
                    : "Annuler = ne rien modifier."),
                $"{preview.ReplaceCount} Rx channel(s) already have a source.\n\n" +
                "Replace = replace those subscriptions.\n" +
                "Skip = preserve the existing subscriptions.\n" +
                (_immediateApply is null
                    ? "Cancel = add nothing to the batch."
                    : "Cancel = change nothing.")),
            L("Remplacer", "Replace"),
            L("Ignorer", "Skip"),
            L("Annuler", "Cancel"));
        return choice switch
        {
            MessageDialogChoice.Primary => PatchConflictResolution.Replace,
            MessageDialogChoice.Secondary => PatchConflictResolution.Skip,
            _ => PatchConflictResolution.Cancel
        };
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

    private async void MatrixCellButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: PatchMatrixCell cell })
        {
            return;
        }

        if (cell.IsAssigned)
        {
            if (_immediateApply is not null)
            {
                await ApplyImmediateEditsAsync([PatchEditRequest.Remove(cell.Target)]);
                return;
            }

            _session.Remove(cell.Target);
            SetInfo(L("Déconnexion préparée.", "Disconnection staged."));
        }
        else
        {
            if (!SourcesAreUnambiguous([cell.Source]))
            {
                return;
            }

            await PreviewAndStagePlanAsync(
                new PatchAssignmentPlan([new PlannedPatchAssignment(cell.Source, cell.Target)]),
                requireConfirmation: false);
            return;
        }

        RefreshTargetStates([cell.Target]);
    }

    private void MatrixBodyScrollViewer_ScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        ScrollViewer body = FindControl<ScrollViewer>("MatrixBodyScrollViewer")!;
        FindControl<ScrollViewer>("MatrixTxHeaderScrollViewer")!.Offset =
            new Vector(body.Offset.X, 0);
        FindControl<ScrollViewer>("MatrixRxHeaderScrollViewer")!.Offset =
            new Vector(0, body.Offset.Y);
    }

    private void MatrixViewport_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (!e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            return;
        }

        SetMatrixZoom(_matrixZoom + (e.Delta.Y > 0 ? MatrixZoomStep : -MatrixZoomStep));
        e.Handled = true;
    }

    private void MatrixZoomOutButton_Click(object? sender, RoutedEventArgs e)
    {
        SetMatrixZoom(_matrixZoom - MatrixZoomStep);
    }

    private void MatrixZoomResetButton_Click(object? sender, RoutedEventArgs e)
    {
        SetMatrixZoom(1.0);
    }

    private void MatrixZoomInButton_Click(object? sender, RoutedEventArgs e)
    {
        SetMatrixZoom(_matrixZoom + MatrixZoomStep);
    }

    private void MatrixZoomFitButton_Click(object? sender, RoutedEventArgs e)
    {
        ScrollViewer body = FindControl<ScrollViewer>("MatrixBodyScrollViewer")!;
        double unscaledWidth = Math.Max(1, Math.Min(_visibleSources.Count, MaxMatrixChannels)) * 92d;
        double unscaledHeight = Math.Max(1, Math.Min(_visibleTargets.Count, MaxMatrixChannels)) * 38d;
        if (body.Bounds.Width <= 0 || body.Bounds.Height <= 0)
        {
            SetMatrixZoom(1.0);
            return;
        }

        SetMatrixZoom(Math.Min(
            (body.Bounds.Width - 12) / unscaledWidth,
            (body.Bounds.Height - 12) / unscaledHeight));
    }

    private void SetMatrixZoom(double requestedZoom)
    {
        double zoom = Math.Clamp(Math.Round(requestedZoom, 2), MinimumMatrixZoom, MaximumMatrixZoom);
        if (Math.Abs(zoom - _matrixZoom) < 0.001)
        {
            return;
        }

        _matrixZoom = zoom;
        double columnWidth = 92 * zoom;
        double rowHeight = 38 * zoom;
        double rxHeaderWidth = Math.Max(150, 220 * zoom);
        Grid matrix = FindControl<Grid>("MatrixPanel")!;
        Grid txHeaders = FindControl<Grid>("MatrixTxHeaderPanel")!;
        Grid rxHeaders = FindControl<Grid>("MatrixRxHeaderPanel")!;
        foreach (ColumnDefinition column in matrix.ColumnDefinitions)
        {
            column.Width = new GridLength(columnWidth);
        }
        foreach (ColumnDefinition column in txHeaders.ColumnDefinitions)
        {
            column.Width = new GridLength(columnWidth);
        }
        foreach (RowDefinition row in matrix.RowDefinitions)
        {
            row.Height = new GridLength(rowHeight);
        }
        foreach (RowDefinition row in rxHeaders.RowDefinitions)
        {
            row.Height = new GridLength(rowHeight);
        }
        if (txHeaders.RowDefinitions.Count > 0)
        {
            txHeaders.RowDefinitions[0].Height = new GridLength(54 * zoom);
        }
        if (rxHeaders.ColumnDefinitions.Count > 0)
        {
            rxHeaders.ColumnDefinitions[0].Width = new GridLength(rxHeaderWidth);
        }
        FindControl<Grid>("MatrixViewport")!.ColumnDefinitions[0].Width = new GridLength(rxHeaderWidth);

        foreach (TextBlock header in txHeaders.Children.OfType<TextBlock>())
        {
            header.Width = 86 * zoom;
            header.FontSize = Math.Max(9, 12 * zoom);
        }
        foreach (PatchMatrixCell cell in _matrixCells.Values)
        {
            cell.Button.Width = 84 * zoom;
            cell.Button.Height = 30 * zoom;
            cell.Button.FontSize = Math.Max(9, 12 * zoom);
        }

        FindControl<TextBlock>("MatrixZoomText")!.Text = $"{Math.Round(zoom * 100):0} %";
    }

    private void TxChannelList_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not ListBox listBox
            || !e.GetCurrentPoint(listBox).Properties.IsLeftButtonPressed)
        {
            return;
        }

        _dragStart = e.GetPosition(listBox);
        _dragReady = true;
    }

    private async void TxChannelList_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (sender is not ListBox listBox || !_dragReady || _dragInProgress)
        {
            return;
        }

        if (!e.GetCurrentPoint(listBox).Properties.IsLeftButtonPressed)
        {
            _dragReady = false;
            return;
        }

        Point current = e.GetPosition(listBox);
        if (Math.Abs(current.X - _dragStart.X) < 5 && Math.Abs(current.Y - _dragStart.Y) < 5)
        {
            return;
        }

        PatchSourceDescriptor[] sources = SelectedSources();
        if (sources.Length == 0 || !SourcesAreUnambiguous(sources))
        {
            _dragReady = false;
            return;
        }

        _dragReady = false;
        _dragInProgress = true;
        try
        {
            DataTransfer data = new();
            data.Add(DataTransferItem.Create(
                DragDataFormat,
                JsonSerializer.Serialize(sources)));
            await DragDrop.DoDragDropAsync(e, data, DragDropEffects.Copy);
        }
        finally
        {
            _dragInProgress = false;
        }
    }

    private void TxChannelList_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _dragReady = false;
    }

    private void RxChannelList_DragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = e.DataTransfer.Contains(DragDataFormat)
            ? DragDropEffects.Copy
            : DragDropEffects.None;
        FindControl<Border>("RxDropBorder")!.BorderBrush = e.DragEffects == DragDropEffects.Copy
            ? ResourceBrush("AccentBrush")
            : ResourceBrush("PanelBorderBrush");
    }

    private void RxChannelList_DragLeave(object? sender, DragEventArgs e)
    {
        FindControl<Border>("RxDropBorder")!.BorderBrush = ResourceBrush("PanelBorderBrush");
    }

    private async void RxChannelList_Drop(object? sender, DragEventArgs e)
    {
        FindControl<Border>("RxDropBorder")!.BorderBrush = ResourceBrush("PanelBorderBrush");
        string? payload = e.DataTransfer.TryGetValue(DragDataFormat);
        PatchSourceDescriptor[]? sources = string.IsNullOrWhiteSpace(payload)
            ? null
            : JsonSerializer.Deserialize<PatchSourceDescriptor[]>(payload);
        if (sources is null || sources.Length == 0)
        {
            e.DragEffects = DragDropEffects.None;
            return;
        }

        Control? sourceControl = e.Source as Control;
        ListBoxItem? container = sourceControl as ListBoxItem
            ?? sourceControl?.FindAncestorOfType<ListBoxItem>();
        PatchRxListItem? targetRow = container?.Content as PatchRxListItem
            ?? container?.DataContext as PatchRxListItem;
        if (targetRow is null)
        {
            SetInfo(L("Déposez les TX sur une ligne RX.", "Drop the Tx channels on an Rx row."), warning: true);
            e.DragEffects = DragDropEffects.None;
            return;
        }

        FindControl<ListBox>("RxChannelList")!.SelectedItem = targetRow;
        await AssignSourcesAsync(sources, targetRow.Target);
        e.DragEffects = DragDropEffects.Copy;
    }

    private PatchSourceDescriptor[] SelectedSources()
    {
        return FindControl<ListBox>("TxChannelList")!.SelectedItems?
            .OfType<PatchSourceDescriptor>()
            .OrderBy(source => source.PositionIndex)
            .ToArray() ?? [];
    }

    private void ResetPendingButton_Click(object? sender, RoutedEventArgs e)
    {
        _session.Reset();
        RefreshAllTargetStates();
        SetInfo(L("Tous les changements visuels ont été annulés.", "All visual changes were discarded."));
    }

    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }

    private void ApplyButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!_session.HasChanges)
        {
            SetInfo(L("Aucun changement à appliquer.", "No changes to apply."), warning: true);
            return;
        }

        Close(true);
    }

    private void UpdateCommandState()
    {
        bool hasTxSelection = FindControl<ListBox>("TxChannelList")!.SelectedItems?.Count > 0;
        bool hasRxSelection = FindControl<ListBox>("RxChannelList")!.SelectedItem is PatchRxListItem;
        FindControl<Button>("AssignSequentialButton")!.IsEnabled = hasTxSelection && hasRxSelection;
        FindControl<Button>("RemoveSelectedRxButton")!.IsEnabled = hasRxSelection;
        FindControl<Button>("SwapDeviceSelectionButton")!.IsEnabled =
            FindControl<ComboBox>("TxDeviceCombo")!.SelectedItem is string
            && FindControl<ComboBox>("RxDeviceCombo")!.SelectedItem is string;
        FindControl<Button>("ResetPendingButton")!.IsEnabled = _session.HasChanges;
        FindControl<Button>("ApplyButton")!.IsEnabled = _session.HasChanges;
        UpdateOneToOneState();

        string pending = _session.PendingCount == 0
            ? L("Aucun changement en attente", "No pending changes")
            : L(
                $"{_session.PendingCount} changement(s) en attente",
                $"{_session.PendingCount} pending change(s)");
        FindControl<TextBlock>("PendingHeaderText")!.Text = pending;
        FindControl<TextBlock>("PendingFooterText")!.Text = pending;
    }

    private void UpdateOneToOneState()
    {
        ComboBox txCombo = FindControl<ComboBox>("OneToOneFirstTxCombo")!;
        ComboBox rxCombo = FindControl<ComboBox>("OneToOneFirstRxCombo")!;
        TextBox countTextBox = FindControl<TextBox>("OneToOneCountTextBox")!;
        TextBlock capacityText = FindControl<TextBlock>("OneToOneCapacityText")!;
        Button previewButton = FindControl<Button>("PreviewOneToOneButton")!;
        if (txCombo.SelectedItem is not PatchSourceDescriptor firstSource
            || rxCombo.SelectedItem is not PatchTargetDescriptor firstTarget)
        {
            capacityText.Text = string.Empty;
            previewButton.IsEnabled = false;
            return;
        }

        try
        {
            PatchRangeCapacity capacity = PatchAssignmentPlanner.GetRangeCapacity(
                _visibleSources,
                firstSource,
                _visibleTargets,
                firstTarget);
            bool validCount = int.TryParse(countTextBox.Text, out int count)
                && count > 0
                && count <= capacity.MaximumCount;
            previewButton.IsEnabled = validCount;
            capacityText.Text = L(
                $"Capacité : {capacity.MaximumCount} patch(s) 1:1 ({capacity.TxAvailable} TX / {capacity.RxAvailable} RX).",
                $"Capacity: {capacity.MaximumCount} one-to-one subscription(s) ({capacity.TxAvailable} Tx / {capacity.RxAvailable} Rx).");
            capacityText.Foreground = validCount ? ResourceBrush("MutedTextBrush") : ResourceBrush("WarningBrush");
        }
        catch (InvalidOperationException exception)
        {
            previewButton.IsEnabled = false;
            capacityText.Text = exception.Message;
            capacityText.Foreground = ResourceBrush("WarningBrush");
        }
    }

    private void UpdateMatrixHint(int displayedSourceCount, int displayedTargetCount)
    {
        bool truncated = displayedSourceCount < _visibleSources.Count || displayedTargetCount < _visibleTargets.Count;
        FindControl<TextBlock>("MatrixHintText")!.Text = truncated
            ? L(
                "RX en lignes, TX en colonnes. Affichage limité aux 128 premiers canaux de chaque côté.",
                "Rx channels are rows and Tx channels are columns. Display is limited to the first 128 channels on each side.")
            : _immediateApply is not null
                ? L(
                    "RX en lignes, TX en colonnes. Un clic applique ou retire immédiatement le patch.",
                    "Rx channels are rows and Tx channels are columns. A click immediately assigns or removes the subscription.")
                : L(
                    "RX en lignes, TX en colonnes. Cliquez dans une case pour préparer ou retirer un patch.",
                    "Rx channels are rows and Tx channels are columns. Click a cell to stage or remove a subscription.");
    }

    private void ApplyLanguage()
    {
        Title = L("Patch visuel", "Visual patch");
        FindControl<TextBlock>("TitleText")!.Text = Title;
        FindControl<TextBlock>("IntroText")!.Text = _immediateApply is not null
            ? L(
                "Chaque clic, sélection, glisser-déposer ou plage modifie immédiatement le patch.",
                "Every click, selection, drag-and-drop or range immediately updates the patch.")
            : L(
                "Préparez les affectations puis appliquez-les en une seule opération.",
                "Stage assignments, then apply them in a single operation.");
        FindControl<TextBlock>("TxDeviceLabel")!.Text = L("Machine émettrice TX", "Tx transmitting device");
        FindControl<TextBlock>("RxDeviceLabel")!.Text = L("Machine réceptrice RX", "Rx receiving device");
        FindControl<TabItem>("AssignmentTab")!.Header = L("Sélection et Patch 1:1", "Selection and one-to-one patch");
        FindControl<TabItem>("MatrixTab")!.Header = L("Grille de patch", "Patch matrix");
        FindControl<TextBlock>("TxListHeading")!.Text = L("Canaux TX disponibles", "Available Tx channels");
        FindControl<TextBlock>("RxListHeading")!.Text = L("Canaux RX et source actuelle", "Rx channels and current source");
        FindControl<TextBlock>("DragHintText")!.Text = L(
            "Sélectionnez un ou plusieurs TX puis déposez-les sur le premier RX.",
            "Select one or more Tx channels, then drop them on the first Rx.");
        FindControl<Button>("AssignSequentialButton")!.Content = L("Affecter à partir du RX sélectionné", "Assign from selected Rx");
        FindControl<Button>("RemoveSelectedRxButton")!.Content = L("Déconnecter le RX sélectionné", "Disconnect selected Rx");
        FindControl<TextBlock>("OneToOneHeading")!.Text = L("Patch 1:1", "One-to-one patch");
        FindControl<TextBlock>("OneToOneFirstRxLabel")!.Text = L("Premier RX", "First Rx");
        FindControl<TextBlock>("OneToOneFirstTxLabel")!.Text = L("Premier TX", "First Tx");
        FindControl<TextBlock>("OneToOneCountLabel")!.Text = L("Nombre", "Count");
        FindControl<Button>("PreviewOneToOneButton")!.Content = _immediateApply is not null
            ? L("Appliquer", "Apply now")
            : L("Prévisualiser et ajouter au lot", "Preview and add to batch");
        FindControl<Button>("MatrixZoomFitButton")!.Content = L("Ajuster", "Fit");
        FindControl<Button>("ResetPendingButton")!.Content = L("Annuler les changements visuels", "Discard visual changes");
        FindControl<Button>("CancelButton")!.Content = _immediateApply is not null
            ? L("Fermer", "Close")
            : L("Fermer sans appliquer", "Close without applying");
        FindControl<Button>("ApplyButton")!.Content = L("Appliquer au projet", "Apply to project");
        CheckBox warnCheckBox = FindControl<CheckBox>("WarnOnExistingPatchCheckBox")!;
        warnCheckBox.Content = L(
            "M'avertir si le RX est déjà patché",
            "Warn me when the Rx channel is already patched");
        string warnHelp = L(
            "Coché par défaut : demande confirmation avant de remplacer la source actuelle d'un RX. Décochez pour remplacer sans message.",
            "Checked by default: asks for confirmation before replacing an Rx channel's current source. Clear it to replace without a prompt.");
        ToolTip.SetTip(warnCheckBox, warnHelp);
        AutomationProperties.SetName(warnCheckBox, warnCheckBox.Content?.ToString() ?? string.Empty);

        AutomationProperties.SetName(FindControl<ComboBox>("TxDeviceCombo")!, FindControl<TextBlock>("TxDeviceLabel")!.Text);
        AutomationProperties.SetName(FindControl<ComboBox>("RxDeviceCombo")!, FindControl<TextBlock>("RxDeviceLabel")!.Text);
        string swapName = L(
            "Inverser les machines TX et RX sans créer de patch inverse",
            "Swap the Tx and Rx devices without creating reverse subscriptions");
        ToolTip.SetTip(FindControl<Button>("SwapDeviceSelectionButton")!, swapName);
        AutomationProperties.SetName(FindControl<Button>("SwapDeviceSelectionButton")!, swapName);
        ToolTip.SetTip(
            FindControl<Button>("AssignSequentialButton")!,
            L("Applique les TX sélectionnés à partir du RX choisi.", "Applies the selected Tx channels starting at the chosen Rx channel."));
        ToolTip.SetTip(
            FindControl<Button>("RemoveSelectedRxButton")!,
            L("Déconnecte le RX sélectionné.", "Disconnects the selected Rx channel."));
        ToolTip.SetTip(
            FindControl<Button>("PreviewOneToOneButton")!,
            _immediateApply is not null
                ? L("Applique immédiatement cette plage de patchs 1:1.", "Immediately applies this one-to-one patch range.")
                : L("Ajoute cette plage au lot.", "Adds this range to the batch."));
        AutomationProperties.SetName(FindControl<ListBox>("TxChannelList")!, FindControl<TextBlock>("TxListHeading")!.Text);
        AutomationProperties.SetName(FindControl<ListBox>("RxChannelList")!, FindControl<TextBlock>("RxListHeading")!.Text);
        AutomationProperties.SetName(FindControl<Grid>("MatrixPanel")!, FindControl<TabItem>("MatrixTab")!.Header?.ToString() ?? string.Empty);
        AutomationProperties.SetName(FindControl<Button>("PreviewOneToOneButton")!, FindControl<Button>("PreviewOneToOneButton")!.Content?.ToString() ?? string.Empty);
        AutomationProperties.SetName(FindControl<Button>("MatrixZoomOutButton")!, L("Réduire le zoom de la grille", "Zoom out of the matrix"));
        AutomationProperties.SetName(FindControl<Button>("MatrixZoomResetButton")!, L("Réinitialiser le zoom de la grille", "Reset matrix zoom"));
        AutomationProperties.SetName(FindControl<Button>("MatrixZoomInButton")!, L("Augmenter le zoom de la grille", "Zoom into the matrix"));
        AutomationProperties.SetName(FindControl<Button>("MatrixZoomFitButton")!, L("Ajuster la grille à la fenêtre", "Fit matrix to window"));
    }

    private void SetInfo(string message, bool warning = false)
    {
        TextBlock info = FindControl<TextBlock>("InfoText")!;
        info.Text = message;
        info.Foreground = warning ? ResourceBrush("WarningBrush") : ResourceBrush("MutedTextBrush");
    }

    private IBrush ResourceBrush(string key)
    {
        return Application.Current?.Resources[key] as IBrush ?? Brushes.Gray;
    }

    private string BuildCellToolTip(
        PatchSourceDescriptor source,
        PatchTargetDescriptor target,
        bool isAssigned,
        bool isPending)
    {
        string action = isAssigned
            ? _immediateApply is not null
                ? L("Cliquer pour déconnecter immédiatement", "Click to disconnect immediately")
                : L("Cliquer pour préparer la déconnexion", "Click to stage the disconnection")
            : _immediateApply is not null
                ? L("Cliquer pour affecter immédiatement", "Click to assign immediately")
                : L("Cliquer pour préparer l'affectation", "Click to stage the assignment");
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

    private string L(string french, string english) => _language == UiLanguage.English ? english : french;

    private static string TranslateSwapError(string? french)
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

    private sealed class PatchRxListItem(PatchTargetDescriptor target, string display)
        : System.ComponentModel.INotifyPropertyChanged
    {
        private string _display = display;

        public PatchTargetDescriptor Target { get; } = target;

        public string Display
        {
            get => _display;
            set
            {
                if (string.Equals(_display, value, StringComparison.Ordinal))
                {
                    return;
                }

                _display = value;
                PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(Display)));
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    }

    private sealed class PatchMatrixCell(
        PatchSourceDescriptor source,
        PatchTargetDescriptor target,
        int sourceIndex,
        int targetIndex,
        bool isAssigned)
    {
        public PatchSourceDescriptor Source { get; } = source;

        public PatchTargetDescriptor Target { get; } = target;

        public int SourceIndex { get; } = sourceIndex;

        public int TargetIndex { get; } = targetIndex;

        public bool IsAssigned { get; set; } = isAssigned;

        public Button Button { get; set; } = null!;
    }

    private readonly record struct MatrixCellKey(int SourceIndex, int TargetIndex);
}
