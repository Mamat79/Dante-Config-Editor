using System.Xml.Linq;

namespace DanteConfigEditorV3.Tests;

public sealed class PatchWorkspaceUiContractTests
{
    [Fact]
    public void WindowsPatchWorkspaceUsesSelectionRangeAndMatrixControls()
    {
        string xaml = File.ReadAllText(RepositoryFile("PatchWorkspaceView.xaml"));
        string codeBehind = File.ReadAllText(RepositoryFile("PatchWorkspaceView.xaml.cs"));

        Assert.Contains("x:Name=\"TxChannelListBox\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"RxChannelListBox\"", xaml, StringComparison.Ordinal);
        Assert.Equal(2, CountOccurrences(xaml, "SelectionMode=\"Extended\""));
        Assert.Contains("x:Name=\"PreviewGrid\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("x:Name=\"ConflictResolutionComboBox\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"RangeStartTxComboBox\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"RangeStartRxComboBox\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"RangeCountTextBox\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"MatrixGrid\"", xaml, StringComparison.Ordinal);
        Assert.Contains("<Trigger Property=\"IsSelected\" Value=\"True\">", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"PreviousRxDeviceButton\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"NextRxDeviceButton\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"PreviousTxDeviceButton\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"NextTxDeviceButton\"", xaml, StringComparison.Ordinal);
        Assert.Contains("PreviewMouseLeftButtonDown=\"MatrixGrid_PreviewMouseLeftButtonDown\"", xaml, StringComparison.Ordinal);
        Assert.Contains("PreviewMouseMove=\"MatrixGrid_PreviewMouseMove\"", xaml, StringComparison.Ordinal);
        Assert.Contains("PreviewMouseLeftButtonUp=\"MatrixGrid_PreviewMouseLeftButtonUp\"", xaml, StringComparison.Ordinal);
        Assert.Contains("PlanMatrixGesture", codeBehind, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"SwapDeviceSelectionButton\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Content=\"FLIP TX ⇄ RX\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Style=\"{StaticResource FlipButtonStyle}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("PatchDeviceSelectionSwapper.ResolveInitialPair", codeBehind, StringComparison.Ordinal);
        Assert.Contains("<Setter Property=\"Background\" Value=\"{DynamicResource WarningBrush}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"RangeCapacityTextBlock\"", xaml, StringComparison.Ordinal);
        Assert.Contains("PlanOneToOne", codeBehind, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"MatrixOneToOneButton\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"MatrixOneToOneCountTextBox\"", xaml, StringComparison.Ordinal);
        Assert.Contains("MatrixOneToOneButton_Click", xaml, StringComparison.Ordinal);
        Assert.Contains("_matrixOneToOneStart.Source", codeBehind, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"MatrixZoomOutButton\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"MatrixZoomResetButton\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"MatrixZoomInButton\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"MatrixZoomFitButton\"", xaml, StringComparison.Ordinal);
        Assert.Contains("PreviewMouseWheel=\"MatrixGrid_PreviewMouseWheel\"", xaml, StringComparison.Ordinal);

        string mainWindowXaml = File.ReadAllText(RepositoryFile("MainWindow.xaml"));
        Assert.DoesNotContain("glisser-déposer", mainWindowXaml, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void WindowsImmediatePatchRefreshPreservesMatrixOneToOneStartAndCount()
    {
        string workspaceCode = File.ReadAllText(RepositoryFile("PatchWorkspaceView.xaml.cs"));
        string mainWindowCode = File.ReadAllText(RepositoryFile("MainWindow.xaml.cs"));

        Assert.Contains("CaptureMatrixOneToOneState()", workspaceCode, StringComparison.Ordinal);
        Assert.Contains("MatrixOneToOneCountTextBox.Text", workspaceCode, StringComparison.Ordinal);
        Assert.Contains("_matrixOneToOneStart?.Source.DanteId", workspaceCode, StringComparison.Ordinal);
        Assert.Contains("_matrixOneToOneStart?.Target.DanteId", workspaceCode, StringComparison.Ordinal);
        Assert.Contains("RestoreMatrixOneToOneState", workspaceCode, StringComparison.Ordinal);
        Assert.Contains(
            "_easyPatchWorkspace?.CaptureMatrixOneToOneState()",
            mainWindowCode,
            StringComparison.Ordinal);
        Assert.Contains(
            "workspace.RestoreMatrixOneToOneState(matrixOneToOneState)",
            mainWindowCode,
            StringComparison.Ordinal);
        Assert.Contains(
            "_easyPatchWorkspace.IsAssignmentModeSelected",
            mainWindowCode,
            StringComparison.Ordinal);
        Assert.Contains(
            "ReferenceEquals(_easyPatchProject, _project)",
            mainWindowCode,
            StringComparison.Ordinal);
    }

    [Fact]
    public void WindowsPatchWorkspacePlacesRxOnTheLeftAndTxOnTheRight()
    {
        string xaml = File.ReadAllText(RepositoryFile("PatchWorkspaceView.xaml"));
        XDocument document = XDocument.Parse(xaml);
        XNamespace xamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";

        XElement rxPanel = NamedElement(document, xamlNamespace, "RxDevicePanel");
        XElement txPanel = NamedElement(document, xamlNamespace, "TxDevicePanel");
        XElement rxList = NamedElement(document, xamlNamespace, "RxChannelListBox");
        XElement txList = NamedElement(document, xamlNamespace, "TxChannelListBox");

        Assert.Equal("0", rxPanel.Attribute("Grid.Column")?.Value);
        Assert.Equal("2", txPanel.Attribute("Grid.Column")?.Value);
        Assert.Equal("0", rxList.Attribute("Grid.Column")?.Value);
        Assert.Equal("2", txList.Attribute("Grid.Column")?.Value);
    }

    [Fact]
    public void MainWindowUsesOnePatchWorkspaceWithThreeVisibleModes()
    {
        string xaml = File.ReadAllText(RepositoryFile("MainWindow.xaml"));
        string codeBehind = File.ReadAllText(RepositoryFile("MainWindow.xaml.cs"));
        XDocument document = XDocument.Parse(xaml);
        XNamespace xamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";

        Assert.Contains("x:Name=\"ClassicPatchTab\" Header=\"Patch\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("x:Name=\"EasyPatchTab\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"PatchMatrixModeButton\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"PatchEasyModeButton\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"PatchListModeButton\"", xaml, StringComparison.Ordinal);
        Assert.Equal(
            "Collapsed",
            NamedElement(document, xamlNamespace, "PatchPerDeviceModeButton")
                .Attribute("Visibility")?.Value);
        Assert.Equal(
            "Collapsed",
            NamedElement(document, xamlNamespace, "PatchPendingModeButton")
                .Attribute("Visibility")?.Value);
        Assert.Equal(
            "Collapsed",
            NamedElement(document, xamlNamespace, "OpenVisualPatchButton")
                .Attribute("Visibility")?.Value);
        Assert.Contains("x:Name=\"PendingPatchGrid\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"UnifiedPatchPendingCountTextBlock\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"EasyPatchHost\"", xaml, StringComparison.Ordinal);
        Assert.Contains("embedded: true", codeBehind, StringComparison.Ordinal);
        Assert.Contains("sharedSession: _unifiedPatchSession", codeBehind, StringComparison.Ordinal);
        Assert.Contains("ShowPatchWorkspaceMode", codeBehind, StringComparison.Ordinal);
        Assert.Contains("RefreshPendingPatchWorkspace", codeBehind, StringComparison.Ordinal);
        Assert.Contains("EasyPatchWorkspace_DirectApplyRequested", codeBehind, StringComparison.Ordinal);
        Assert.DoesNotContain("EasyPatchWorkspace_ApplyRequested", codeBehind, StringComparison.Ordinal);
    }

    [Fact]
    public void PatchDeviceSelectionFeedsTheSharedInspectorContext()
    {
        string workspaceCode = File.ReadAllText(RepositoryFile("PatchWorkspaceView.xaml.cs"));
        string mainWindowCode = File.ReadAllText(RepositoryFile("MainWindow.xaml.cs"));

        Assert.Contains("PatchDeviceFocusChangedEventArgs", workspaceCode, StringComparison.Ordinal);
        Assert.Contains("DeviceFocusChanged?.Invoke", workspaceCode, StringComparison.Ordinal);
        Assert.Contains(
            "workspace.DeviceFocusChanged += EasyPatchWorkspace_DeviceFocusChanged",
            mainWindowCode,
            StringComparison.Ordinal);
        Assert.Contains(
            "SynchronizeSelectedDeviceContext(subscription.RxDevice",
            mainWindowCode,
            StringComparison.Ordinal);
        Assert.Contains(
            "_project.FindDeviceByStableIdentity",
            ExtractMethod(mainWindowCode, "private DanteDevice? SelectedInspectorDevice"),
            StringComparison.Ordinal);
        Assert.Contains(
            "candidate.Device.StableIdentity",
            ExtractMethod(mainWindowCode, "private void SynchronizeSelectedDeviceContext"),
            StringComparison.Ordinal);
        Assert.Contains("public bool FocusDevice(string deviceName)", workspaceCode, StringComparison.Ordinal);
        Assert.Contains(
            "_easyPatchWorkspace?.FocusDevice(device.Name)",
            ExtractMethod(mainWindowCode, "private void InspectorOpenPatchButton_Click"),
            StringComparison.Ordinal);
        Assert.Contains(
            "PatchMatrixModeButton.IsChecked = true",
            ExtractMethod(mainWindowCode, "private void InspectorOpenPatchButton_Click"),
            StringComparison.Ordinal);
    }

    [Fact]
    public void EasyPatchOpensOnTheMatrixThenOffersSelectionAndInlineRename()
    {
        string xaml = File.ReadAllText(RepositoryFile("PatchWorkspaceView.xaml"));
        string codeBehind = File.ReadAllText(RepositoryFile("PatchWorkspaceView.xaml.cs"));
        XDocument document = XDocument.Parse(xaml);
        XNamespace xamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";

        Assert.Contains("PatchModeTabControl.Items.Insert(0, MatrixTab)", codeBehind, StringComparison.Ordinal);
        Assert.Contains("MatrixTab.IsSelected = true", codeBehind, StringComparison.Ordinal);
        Assert.Contains("startInAssignmentMode", codeBehind, StringComparison.Ordinal);
        Assert.Contains("IsAssignmentModeSelected", codeBehind, StringComparison.Ordinal);
        Assert.Contains("IsMatrixModeSelected", codeBehind, StringComparison.Ordinal);
        Assert.Contains("EmbeddedPatchModeTabControlTemplate", xaml, StringComparison.Ordinal);
        Assert.Contains(
            "PatchModeTabControl.Template = (ControlTemplate)FindResource",
            codeBehind,
            StringComparison.Ordinal);
        Assert.Contains("InlineChannelNameTextBox_LostKeyboardFocus", xaml, StringComparison.Ordinal);
        Assert.Equal(2, CountOccurrences(xaml, "PreviewMouseLeftButtonDown=\"InlineChannelNameTextBox_PreviewMouseLeftButtonDown\""));
        Assert.Contains("ChannelSeriesThumb_DragStarted", xaml, StringComparison.Ordinal);
        Assert.Contains("ChannelSeriesThumb_DragCompleted", xaml, StringComparison.Ordinal);
        Assert.Contains("MatrixTxHeader_Click", codeBehind, StringComparison.Ordinal);
        Assert.Contains("MinHeight=\"230\"", xaml, StringComparison.Ordinal);
        Assert.Contains("ColumnHeaderHeight=\"132\"", xaml, StringComparison.Ordinal);
        Assert.Contains("SizeChanged=\"PatchWorkspaceView_SizeChanged\"", xaml, StringComparison.Ordinal);
        Assert.Contains("MatrixGrid.ColumnHeaderHeight = ActualHeight switch", codeBehind, StringComparison.Ordinal);
        Assert.Contains("MatrixGrid.MinHeight = ActualHeight < 620 ? 156 : 230", codeBehind, StringComparison.Ordinal);
        Assert.Contains("IntroTextBlock.Visibility = compact", codeBehind, StringComparison.Ordinal);
        Assert.Contains("WorkspaceHeaderBorder.Padding = compact", codeBehind, StringComparison.Ordinal);
        Assert.Contains("WorkspaceFooterBorder.Padding = compact", codeBehind, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"DeviceSelectorGrid\"", xaml, StringComparison.Ordinal);
        Assert.Contains("compactMatrix = _embedded && MatrixTab.IsSelected", codeBehind, StringComparison.Ordinal);
        Assert.Equal(
            "1",
            NamedElement(document, xamlNamespace, "MatrixGrid").Attribute("Grid.Row")?.Value);
        Assert.Contains("Content = source.Display", codeBehind, StringComparison.Ordinal);
        Assert.Contains("LayoutTransform = new RotateTransform(-90)", codeBehind, StringComparison.Ordinal);
        Assert.Contains("MatrixSeriesThumb_DragStarted", codeBehind, StringComparison.Ordinal);
        Assert.Contains("MatrixSeriesThumb_DragCompleted", codeBehind, StringComparison.Ordinal);
        Assert.Contains("RenameMatrixChannel", codeBehind, StringComparison.Ordinal);
        Assert.Contains("e.Key == Key.Tab", codeBehind, StringComparison.Ordinal);
        Assert.Contains("FocusInlineChannelEditor", codeBehind, StringComparison.Ordinal);
        Assert.Contains("InlineChannelNavigationRequested", codeBehind, StringComparison.Ordinal);
        Assert.Contains("RequestInlineChannelNavigation(target)", codeBehind, StringComparison.Ordinal);
        Assert.Contains("case PatchSourceDescriptor source:", codeBehind, StringComparison.Ordinal);
        Assert.Contains("FindVisualParent<DataGridCell>(hit)", codeBehind, StringComparison.Ordinal);
        Assert.Contains("row.Cells[sourceIndex]", codeBehind, StringComparison.Ordinal);
        Assert.Contains("EasyPatchWorkspace_InlineChannelNavigationRequested", File.ReadAllText(RepositoryFile("MainWindow.xaml.cs")), StringComparison.Ordinal);
        Assert.Contains("ExtendEasyPatchChannelSeries", File.ReadAllText(RepositoryFile("MainWindow.xaml.cs")), StringComparison.Ordinal);
        Assert.Contains("label.Click += MatrixTxHeader_Click", codeBehind, StringComparison.Ordinal);
        Assert.Contains("sender is Button { Tag: PatchSourceDescriptor source }", codeBehind, StringComparison.Ordinal);
        Assert.Contains("OpenMatrixTxRenameEditor", codeBehind, StringComparison.Ordinal);
        Assert.Contains("Dispatcher.BeginInvoke(new Action(() => OpenMatrixTxRenameEditor", codeBehind, StringComparison.Ordinal);
        Assert.Contains("args.Key == Key.Enter", codeBehind, StringComparison.Ordinal);
        Assert.Contains("args.Key == Key.Tab", codeBehind, StringComparison.Ordinal);
        Assert.Contains("ModifierKeys.Shift", codeBehind, StringComparison.Ordinal);
        Assert.Contains("ChannelSeriesHandleVisibilityConverter", xaml, StringComparison.Ordinal);
        Assert.Contains("source.CanExtendNameSeries ? Visibility.Visible : Visibility.Collapsed", codeBehind, StringComparison.Ordinal);
        Assert.Contains("e.Canceled", codeBehind, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"WarnOnExistingPatchCheckBox\"", xaml, StringComparison.Ordinal);
        Assert.Contains("IsChecked=\"True\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Patcher la sélection", codeBehind, StringComparison.Ordinal);
        Assert.Contains("Patch selection", codeBehind, StringComparison.Ordinal);
        Assert.Contains("UpdateVisibleModePresentation", codeBehind, StringComparison.Ordinal);
        Assert.Contains("Matrice de patch", codeBehind, StringComparison.Ordinal);
        Assert.Contains("ApplyMatrixCellDirectly", codeBehind, StringComparison.Ordinal);
        Assert.Contains("ApplyPlanImmediately", codeBehind, StringComparison.Ordinal);
        Assert.Contains("BuildCommittedPreview", codeBehind, StringComparison.Ordinal);
        Assert.Contains("DirectApplyRequested", codeBehind, StringComparison.Ordinal);
        Assert.Contains("WarnOnExistingPatch", codeBehind, StringComparison.Ordinal);
        Assert.Contains("if (!WarnOnExistingPatch)", codeBehind, StringComparison.Ordinal);

        string mainCode = File.ReadAllText(RepositoryFile("MainWindow.xaml.cs"));
        Assert.Contains("EasyPatchWorkspace_DirectApplyRequested", mainCode, StringComparison.Ordinal);
        Assert.DoesNotContain(
            "pendingEdits",
            ExtractMethod(mainCode, "private void RefreshEasyPatchWorkspace"),
            StringComparison.Ordinal);
    }

    [Fact]
    public void DevicePatchButtonsUseTheRequestedTwoByTwoOrder()
    {
        XDocument document = XDocument.Parse(File.ReadAllText(RepositoryFile("MainWindow.xaml")));
        XNamespace xamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";
        XElement actionGrid = NamedElement(document, xamlNamespace, "DevicePatchActionGrid");

        string[] names = actionGrid.Elements()
            .Select(element => element.Attribute(xamlNamespace + "Name")?.Value)
            .Where(name => name is not null)
            .Cast<string>()
            .ToArray();
        string[] labels = actionGrid.Elements()
            .Select(element => element.Attribute("Content")?.Value)
            .Where(label => label is not null)
            .Cast<string>()
            .ToArray();

        Assert.Equal(
            ["ResetDeviceRxPatchesButton", "ResetDeviceTxPatchesButton", "ResetDevicePatchesButton", "DeleteDeviceButton"],
            names);
        Assert.Equal(["Reset RX", "Reset TX", "Reset RX/TX", "Supprimer"], labels);
        Assert.Equal("2", actionGrid.Attribute("Columns")?.Value);
        Assert.Contains("ChannelSeriesHandleVisibilityConverter", File.ReadAllText(RepositoryFile("MainWindow.xaml")), StringComparison.Ordinal);
    }

    [Fact]
    public void PatchViewUsesRxFilterFirstAndEditableDeviceAndChannelColumns()
    {
        string xaml = File.ReadAllText(RepositoryFile("MainWindow.xaml"));

        Assert.True(
            xaml.IndexOf("Filtre récepteur RX", StringComparison.Ordinal) < xaml.IndexOf("Filtre émetteur TX", StringComparison.Ordinal));
        Assert.Contains("x:Name=\"PatchRxDeviceColumn\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"PatchRxChannelColumn\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"PatchDisplayTxColumn\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"PatchTxDanteIdColumn\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"PatchTxChannelColumn\"", xaml, StringComparison.Ordinal);
        Assert.Contains("CellEditEnding=\"PatchGrid_CellEditEnding\"", xaml, StringComparison.Ordinal);
        Assert.Equal(2, CountOccurrences(xaml, "DragStarted=\"PatchSeriesThumb_DragStarted\""));
        Assert.Contains("ExtendChannelNameSeries(deviceName, kind, seeds", File.ReadAllText(RepositoryFile("MainWindow.xaml.cs")), StringComparison.Ordinal);
    }

    [Fact]
    public void ConfigurationPlacesGlobalToolsBeforeLinkedDeviceAndChannelPanels()
    {
        XDocument document = XDocument.Parse(File.ReadAllText(RepositoryFile("MainWindow.xaml")));
        XNamespace xamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";

        XElement editors = NamedElement(document, xamlNamespace, "ConfigurationEditorsGrid");
        XElement globalActions = editors.Descendants().Single(element =>
            element.Name.LocalName == "GroupBox" && element.Attribute("Header")?.Value == "Actions globales");
        XElement networkAudioTab = globalActions.Descendants().Single(element =>
            element.Name.LocalName == "TabItem" && element.Attribute("Header")?.Value == "Réseau / audio");
        XElement quickListLabel = globalActions.Descendants().Single(element =>
            element.Name.LocalName == "TextBlock" && element.Attribute("Text")?.Value == "Liste rapide");
        XElement quickLists = quickListLabel.Parent
            ?? throw new InvalidOperationException("Barre de liste rapide introuvable.");
        XElement device = editors.Descendants().Single(element =>
            element.Name.LocalName == "GroupBox" && element.Attribute("Header")?.Value == "Machine sélectionnée");
        XElement channels = editors.Descendants().Single(element =>
            element.Name.LocalName == "GroupBox" && element.Attribute("Header")?.Value == "Canaux de la machine");

        Assert.Contains(quickLists.Ancestors(), element => ReferenceEquals(element, globalActions));
        Assert.Equal("1", quickLists.Parent?.Attribute("Grid.Row")?.Value);
        Assert.Null(globalActions.Attribute("Grid.Row"));
        Assert.Null(globalActions.Attribute("Height"));
        Assert.Equal("Stretch", globalActions.Attribute("VerticalAlignment")?.Value);
        Assert.DoesNotContain(
            networkAudioTab.Descendants(),
            element => element.Name.LocalName == "ScrollViewer");
        Assert.Equal("1", device.Attribute("Grid.Column")?.Value);
        Assert.Equal("2", channels.Attribute("Grid.Column")?.Value);
        _ = NamedElement(document, xamlNamespace, "QuickListComboBox");
        Assert.Contains(
            quickLists.Descendants(),
            element =>
                element.Name.LocalName == "Button"
                && element.Attribute("Click")?.Value == "ShowQuickListButton_Click");
        Assert.Contains(editors.Elements(), element =>
            element.Name.LocalName == "Border"
            && element.Attribute("Grid.Column")?.Value == "1"
            && element.Attribute("Grid.ColumnSpan")?.Value == "2");
    }

    [Fact]
    public void EmbeddedEasyPatchKeepsDeviceSelectorsOutsideAnyPageScroller()
    {
        XDocument document = XDocument.Parse(File.ReadAllText(RepositoryFile("MainWindow.xaml")));
        XNamespace xamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";

        XElement host = NamedElement(document, xamlNamespace, "EasyPatchHost");

        Assert.DoesNotContain(
            host.Ancestors(),
            ancestor => string.Equals(ancestor.Name.LocalName, "ScrollViewer", StringComparison.Ordinal));
    }

    [Fact]
    public void SynopticHasOneVisibleEntryInsideImportExportAndHistoryIsBilingual()
    {
        string xaml = File.ReadAllText(RepositoryFile("MainWindow.xaml"));
        string codeBehind = File.ReadAllText(RepositoryFile("MainWindow.xaml.cs"));
        XDocument document = XDocument.Parse(xaml);
        XNamespace xamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";

        Assert.Equal(
            "Collapsed",
            NamedElement(document, xamlNamespace, "SynopticNavigationButton")
                .Attribute("Visibility")?.Value);
        Assert.Contains(
            "WorkspaceSection.Synoptic => ImportExportNavigationButton",
            codeBehind,
            StringComparison.Ordinal);
        Assert.Contains("x:Name=\"ExportsTab\" Header=\"Import / Export\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"SynopticTab\" Header=\"Synoptique\"", xaml, StringComparison.Ordinal);
        Assert.Contains("\"ACTION HISTORY\"", codeBehind, StringComparison.Ordinal);
        Assert.Contains("\"No action has been recorded in this session.\"", codeBehind, StringComparison.Ordinal);
        Assert.Contains("BuildLocalizedSaveSummary", codeBehind, StringComparison.Ordinal);
        Assert.Contains("_project.BuildSaveSummary(_language)", codeBehind, StringComparison.Ordinal);
        Assert.Contains("\"FINAL REPORT BEFORE DANTE IMPORT\"", codeBehind, StringComparison.Ordinal);
        Assert.Contains("\"Important items:\"", codeBehind, StringComparison.Ordinal);
        Assert.Contains("_project.BuildCompatibilityReport(_language)", codeBehind, StringComparison.Ordinal);
        Assert.Contains("LocalizeLiteral(change.Action)", codeBehind, StringComparison.Ordinal);
        Assert.Contains(
            "LocalizationService.TranslateHistoryDetail(_language, change.Details)",
            codeBehind,
            StringComparison.Ordinal);
        string localization = File.ReadAllText(RepositoryFile("Services", "LocalizationService.cs"));
        Assert.Contains(
            "Add(map, \"Récupération automatique\", \"Automatic recovery\")",
            localization,
            StringComparison.Ordinal);
    }

    [Fact]
    public void InspectorUsesOnePersistentArrowAndStartsExpanded()
    {
        string xaml = File.ReadAllText(RepositoryFile("MainWindow.xaml"));
        string codeBehind = File.ReadAllText(RepositoryFile("MainWindow.xaml.cs"));
        XDocument document = XDocument.Parse(xaml);
        XNamespace xamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";

        XElement reveal = NamedElement(document, xamlNamespace, "InspectorRevealButton");
        XElement toolbarToggle = NamedElement(document, xamlNamespace, "InspectorToggleButton");

        Assert.Equal("Visible", reveal.Attribute("Visibility")?.Value);
        Assert.Equal(">", reveal.Attribute("Content")?.Value);
        Assert.Equal("Collapsed", toolbarToggle.Attribute("Visibility")?.Value);
        Assert.DoesNotContain("x:Name=\"InspectorCloseButton\"", xaml, StringComparison.Ordinal);
        Assert.Contains(
            "InspectorRevealButton.Visibility = Visibility.Visible",
            codeBehind,
            StringComparison.Ordinal);
        Assert.Contains(
            "InspectorSplitterColumn.Width = new GridLength(44)",
            codeBehind,
            StringComparison.Ordinal);
        Assert.Equal(
            "{StaticResource PanelRevealButtonStyle}",
            reveal.Attribute("Style")?.Value);
        Assert.Contains(
            "SetInspectorExpanded(true)",
            codeBehind,
            StringComparison.Ordinal);
    }

    [Fact]
    public void SidePanelsAndDeviceListUseConsistentPersistentArrowHandles()
    {
        string xaml = File.ReadAllText(RepositoryFile("MainWindow.xaml"));
        string codeBehind = File.ReadAllText(RepositoryFile("MainWindow.xaml.cs"));
        XDocument document = XDocument.Parse(xaml);
        XNamespace xamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";

        XElement navigationReveal =
            NamedElement(document, xamlNamespace, "NavigationRevealButton");
        XElement toolbarToggle =
            NamedElement(document, xamlNamespace, "NavigationToggleButton");
        XElement settingsToggle =
            NamedElement(document, xamlNamespace, "ToggleConfigurationEditorsButton");
        XElement deviceListToggle =
            NamedElement(document, xamlNamespace, "ToggleDeviceListButton");
        XElement deviceListPanel =
            NamedElement(document, xamlNamespace, "DeviceListPanel");
        XElement deviceListTitle =
            NamedElement(document, xamlNamespace, "DeviceListTitleTextBlock");
        XElement settingsRegion =
            NamedElement(document, xamlNamespace, "ConfigurationEditorsRegion");
        XElement settingsScroller =
            NamedElement(document, xamlNamespace, "ConfigurationEditorsScrollViewer");

        Assert.Equal("Visible", navigationReveal.Attribute("Visibility")?.Value);
        Assert.Equal("<", navigationReveal.Attribute("Content")?.Value);
        Assert.Equal("Collapsed", toolbarToggle.Attribute("Visibility")?.Value);
        Assert.DoesNotContain("x:Name=\"NavigationCloseButton\"", xaml, StringComparison.Ordinal);
        Assert.Equal("Collapsed", settingsToggle.Attribute("Visibility")?.Value);
        Assert.Equal("1", settingsRegion.Attribute("Grid.Row")?.Value);
        Assert.Equal("Auto", settingsScroller.Attribute("VerticalScrollBarVisibility")?.Value);
        Assert.Contains(
            "NavigationRevealButton.Visibility = Visibility.Visible",
            codeBehind,
            StringComparison.Ordinal);
        Assert.Contains(
            "NavigationSplitterColumn.Width = new GridLength(44)",
            codeBehind,
            StringComparison.Ordinal);
        Assert.Equal(
            "{StaticResource PanelRevealButtonStyle}",
            navigationReveal.Attribute("Style")?.Value);
        Assert.Equal(
            "{StaticResource SecondaryButtonStyle}",
            deviceListToggle.Attribute("Style")?.Value);
        Assert.Equal("1", deviceListToggle.Attribute("Grid.Column")?.Value);
        Assert.Equal("48", deviceListToggle.Attribute("Width")?.Value);
        Assert.Equal("26", deviceListToggle.Attribute("Height")?.Value);
        Assert.Equal("Center", deviceListToggle.Attribute("HorizontalAlignment")?.Value);
        Assert.Null(deviceListToggle.Attribute("DockPanel.Dock"));
        Assert.Equal("▼", deviceListToggle.Attribute("Content")?.Value);
        Assert.Equal("Liste des machines", deviceListTitle.Attribute("Text")?.Value);
        Assert.Equal("Collapsed", deviceListPanel.Attribute("Visibility")?.Value);
        Assert.Contains(
            "ToggleConfigurationEditorsButton.Content = collapsed ? \"\\u25BC\" : \"\\u25B2\"",
            codeBehind,
            StringComparison.Ordinal);
        Assert.Contains(
            "ConfigurationEditorsGrid.Visibility = Visibility.Visible",
            codeBehind,
            StringComparison.Ordinal);
        Assert.Contains("SetDeviceListExpanded(false)", codeBehind, StringComparison.Ordinal);
        Assert.Contains(
            "ConfigurationEditorsRow.Height = _deviceListExpanded",
            codeBehind,
            StringComparison.Ordinal);
        Assert.Contains(
            "? new GridLength(0)",
            codeBehind,
            StringComparison.Ordinal);
        Assert.Contains(
            "DeviceListRow.Height = _deviceListExpanded",
            codeBehind,
            StringComparison.Ordinal);
        Assert.Contains(
            "? new GridLength(1, GridUnitType.Star)",
            codeBehind,
            StringComparison.Ordinal);
        Assert.Contains(
            "ToggleDeviceListButton.Content = expanded ? \"\\u25B2\" : \"\\u25BC\"",
            codeBehind,
            StringComparison.Ordinal);
        Assert.Contains("SetNavigationExpanded(true)", codeBehind, StringComparison.Ordinal);
        Assert.DoesNotContain(
            "InterfaceSettingsService.LoadConfigurationEditorsExpanded()",
            codeBehind,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "InterfaceSettingsService.SaveConfigurationEditorsExpanded(",
            codeBehind,
            StringComparison.Ordinal);
    }

    [Fact]
    public void DeviceBankActionsAreOwnedByTheMachinesPage()
    {
        string xaml = File.ReadAllText(RepositoryFile("MainWindow.xaml"));
        string codeBehind = File.ReadAllText(RepositoryFile("MainWindow.xaml.cs"));
        string bankXaml = File.ReadAllText(RepositoryFile("MachineBankWindow.xaml"));
        string bankCode = File.ReadAllText(RepositoryFile("MachineBankWindow.xaml.cs"));
        XDocument document = XDocument.Parse(xaml);
        XDocument bankDocument = XDocument.Parse(bankXaml);
        XNamespace xamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";

        XElement bankNavigation =
            NamedElement(document, xamlNamespace, "DeviceLibraryNavigationButton");
        XElement bankTab =
            NamedElement(document, xamlNamespace, "DeviceLibraryShellTab");

        Assert.Equal("Collapsed", bankNavigation.Attribute("Visibility")?.Value);
        Assert.Equal("Collapsed", bankTab.Attribute("Visibility")?.Value);
        _ = NamedElement(document, xamlNamespace, "AddDeviceFromBankButton");
        _ = NamedElement(document, xamlNamespace, "ManageMachineBankButton");
        Assert.DoesNotContain("MachineBankSourceComboBox", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("SelectedMachineBankPath", codeBehind, StringComparison.Ordinal);
        Assert.Contains(
            "MachineBankCatalogService.Load(_bankPath)",
            bankCode,
            StringComparison.Ordinal);
        Assert.Contains(
            "OpenMachineBankWindow(null)",
            codeBehind,
            StringComparison.Ordinal);
        Assert.Equal("1500", bankDocument.Root?.Attribute("Width")?.Value);
        Assert.Equal("900", bankDocument.Root?.Attribute("Height")?.Value);
        Assert.Equal("Window_Loaded", bankDocument.Root?.Attribute("Loaded")?.Value);
        Assert.Contains(
            "MaxHeight = Math.Max(MinHeight, workArea.Height - 32)",
            bankCode,
            StringComparison.Ordinal);
    }

    [Fact]
    public void NewProjectDialogIsResponsiveAndListsInstalledBanks()
    {
        string xaml = File.ReadAllText(RepositoryFile("NewProjectWindow.xaml"));
        string codeBehind = File.ReadAllText(RepositoryFile("NewProjectWindow.xaml.cs"));
        XDocument document = XDocument.Parse(xaml);
        XNamespace xamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";

        XElement window = document.Root!;
        Assert.Equal("920", window.Attribute("Width")?.Value);
        Assert.Equal("660", window.Attribute("Height")?.Value);
        Assert.Equal("Window_Loaded", window.Attribute("Loaded")?.Value);
        _ = NamedElement(document, xamlNamespace, "OpenBanksButton");
        _ = NamedElement(document, xamlNamespace, "SourceHelpTextBlock");
        Assert.Contains(
            "MachineBankDistributionService.DiscoverIncludedBankPaths()",
            codeBehind,
            StringComparison.Ordinal);
        Assert.Contains(
            "MaxHeight = Math.Max(MinHeight, workArea.Height - 32)",
            codeBehind,
            StringComparison.Ordinal);
    }

    [Fact]
    public void MacMachinesPageOffersInstalledBanksWithoutReplacingTheActiveBank()
    {
        string xaml = File.ReadAllText(RepositoryFile(
            "src",
            "DanteConfigEditor.Mac",
            "MainWindow.axaml"));
        string codeBehind = File.ReadAllText(RepositoryFile(
            "src",
            "DanteConfigEditor.Mac",
            "MainWindow.axaml.cs"));
        string dialogCode = File.ReadAllText(RepositoryFile(
            "src",
            "DanteConfigEditor.Mac",
            "MachineBankDialog.axaml.cs"));

        Assert.Contains("x:Name=\"AddDeviceFromBankButton\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"ManageMachineBankButton\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("MachineBankSourceComboBox", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("SelectedMachineBankPath", codeBehind, StringComparison.Ordinal);
        Assert.Contains(
            "MachineBankCatalogService.Load(_bankPath)",
            dialogCode,
            StringComparison.Ordinal);
        Assert.Contains(
            "await OpenMachineBankAsync(null)",
            codeBehind,
            StringComparison.Ordinal);
        Assert.Contains("string? initialBankPath = null", dialogCode, StringComparison.Ordinal);
        Assert.Contains("Path.GetFullPath(initialBankPath)", dialogCode, StringComparison.Ordinal);
    }

    [Fact]
    public void EmbeddedEasyPatchIsImmediateWhileStandaloneDialogKeepsItsReturnBatch()
    {
        string xaml = File.ReadAllText(RepositoryFile("PatchWorkspaceView.xaml"));
        string codeBehind = File.ReadAllText(RepositoryFile("PatchWorkspaceView.xaml.cs"));
        XDocument document = XDocument.Parse(xaml);
        XNamespace xamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";

        Assert.Contains("x:Name=\"ApplySelectionDirectButton\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"ApplyRangeDirectButton\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("x:Name=\"AddPreviewToBatchButton\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("x:Name=\"ApplyPreviewButton\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Lot prévisualisé", xaml, StringComparison.Ordinal);
        Assert.Contains("StagePlanAsPreview", codeBehind, StringComparison.Ordinal);
        Assert.Contains("PendingChanges", codeBehind, StringComparison.Ordinal);
        Assert.Contains("ApplyPlanDirectly", codeBehind, StringComparison.Ordinal);
        Assert.Contains("ApplyPlanImmediately", codeBehind, StringComparison.Ordinal);
        Assert.Contains("PreviewSelectionButton.Visibility = Visibility.Collapsed", codeBehind, StringComparison.Ordinal);
        Assert.Contains("PreviewRangeButton.Visibility = Visibility.Collapsed", codeBehind, StringComparison.Ordinal);
        Assert.Contains("PreviewGroupBox.Visibility = Visibility.Collapsed", codeBehind, StringComparison.Ordinal);
        Assert.Contains("ApplyButton.Visibility = Visibility.Collapsed", codeBehind, StringComparison.Ordinal);
        Assert.Contains("if (_embedded)", codeBehind, StringComparison.Ordinal);

        XElement preview = NamedElement(document, xamlNamespace, "PreviewGroupBox");
        Assert.Equal("Collapsed", preview.Attribute("Visibility")?.Value);

        XElement previewGrid = NamedElement(document, xamlNamespace, "PreviewGrid");
        Assert.Equal("Disabled", previewGrid.Attribute("ScrollViewer.HorizontalScrollBarVisibility")?.Value);
        Assert.All(
            previewGrid.Elements().Single(element => element.Name.LocalName == "DataGrid.Columns").Elements(),
            column => Assert.NotNull(column.Attribute("MinWidth")));
    }

    [Fact]
    public void PatchMatrixUsesCompactCells()
    {
        XDocument document = XDocument.Parse(File.ReadAllText(RepositoryFile("PatchWorkspaceView.xaml")));
        XNamespace xamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";
        XElement style = document.Descendants()
            .Single(element => string.Equals((string?)element.Attribute(xamlNamespace + "Key"), "MatrixCellToggleStyle", StringComparison.Ordinal));
        Dictionary<string, double> setters = style.Elements()
            .Where(element => element.Name.LocalName == "Setter")
            .Where(element => element.Attribute("Property") is not null)
            .ToDictionary(
                element => element.Attribute("Property")!.Value,
                element => double.TryParse(element.Attribute("Value")?.Value, out double value) ? value : double.NaN,
                StringComparer.Ordinal);

        Assert.True(setters["Width"] <= 30);
        Assert.True(setters["Height"] <= 24);
    }

    [Fact]
    public void MacPatchWorkspaceOffersOneToOneSwapAndZoomWithoutRebuildingOnZoom()
    {
        string xaml = File.ReadAllText(RepositoryFile(
            "src",
            "DanteConfigEditor.Mac",
            "PatchWorkspaceDialog.axaml"));
        string codeBehind = File.ReadAllText(RepositoryFile(
            "src",
            "DanteConfigEditor.Mac",
            "PatchWorkspaceDialog.axaml.cs"));

        Assert.Contains("x:Name=\"SwapDeviceSelectionButton\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"OneToOneFirstTxCombo\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"OneToOneFirstRxCombo\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"OneToOneCountTextBox\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"PreviewOneToOneButton\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"WarnOnExistingPatchCheckBox\"", xaml, StringComparison.Ordinal);
        Assert.Contains("PlanOneToOne", codeBehind, StringComparison.Ordinal);
        Assert.Contains("ShowChoiceAsync", codeBehind, StringComparison.Ordinal);
        Assert.Contains("ApplyPlanImmediatelyAsync", codeBehind, StringComparison.Ordinal);
        Assert.Contains("_immediateApply", codeBehind, StringComparison.Ordinal);
        Assert.Contains(
            "immediateApply: edits => ExecuteMutationAsync",
            File.ReadAllText(RepositoryFile("src", "DanteConfigEditor.Mac", "MainWindow.axaml.cs")),
            StringComparison.Ordinal);
        Assert.Contains("x:Name=\"MatrixZoomFitButton\"", xaml, StringComparison.Ordinal);
        Assert.Contains("PointerWheelChanged=\"MatrixViewport_PointerWheelChanged\"", xaml, StringComparison.Ordinal);

        int zoomStart = codeBehind.IndexOf("private void SetMatrixZoom", StringComparison.Ordinal);
        int nextMethod = codeBehind.IndexOf("private void TxChannelList_PointerPressed", zoomStart, StringComparison.Ordinal);
        Assert.True(zoomStart >= 0 && nextMethod > zoomStart);
        Assert.DoesNotContain("BuildMatrix()", codeBehind[zoomStart..nextMethod], StringComparison.Ordinal);
    }

    [Fact]
    public void PatchAndSynopticZoomSupportButtonsAndControlMouseWheel()
    {
        string patchXaml = File.ReadAllText(RepositoryFile("PatchWorkspaceView.xaml"));
        string mainXaml = File.ReadAllText(RepositoryFile("MainWindow.xaml"));
        string macPatchXaml = File.ReadAllText(RepositoryFile(
            "src",
            "DanteConfigEditor.Mac",
            "PatchWorkspaceDialog.axaml"));
        string macMainXaml = File.ReadAllText(RepositoryFile(
            "src",
            "DanteConfigEditor.Mac",
            "MainWindow.axaml"));

        Assert.Contains("PreviewMouseWheel=\"MatrixGrid_PreviewMouseWheel\"", patchXaml, StringComparison.Ordinal);
        Assert.Contains("PreviewMouseWheel=\"SynopticScrollViewer_PreviewMouseWheel\"", mainXaml, StringComparison.Ordinal);
        Assert.Contains("PointerWheelChanged=\"MatrixViewport_PointerWheelChanged\"", macPatchXaml, StringComparison.Ordinal);
        Assert.Contains("PointerWheelChanged=\"SynopticScrollViewer_PointerWheelChanged\"", macMainXaml, StringComparison.Ordinal);
    }

    [Fact]
    public void WindowsAndMacOfferManualAndAutomaticApplicationUpdates()
    {
        string windowsXaml = File.ReadAllText(RepositoryFile("MainWindow.xaml"));
        string windowsCode = File.ReadAllText(RepositoryFile("MainWindow.xaml.cs"));
        string macXaml = File.ReadAllText(RepositoryFile(
            "src",
            "DanteConfigEditor.Mac",
            "MainWindow.axaml"));
        string macCode = File.ReadAllText(RepositoryFile(
            "src",
            "DanteConfigEditor.Mac",
            "MainWindow.axaml.cs"));

        Assert.Contains("x:Name=\"CheckUpdatesMenuItem\"", windowsXaml, StringComparison.Ordinal);
        Assert.Contains("CheckForApplicationUpdateAsync(silentWhenCurrent: true)", windowsCode, StringComparison.Ordinal);
        Assert.Contains("ApplicationUpdateService", windowsCode, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"CheckUpdatesMenuItem\"", macXaml, StringComparison.Ordinal);
        Assert.Contains("CheckForApplicationUpdateAsync(silentWhenCurrent: true)", macCode, StringComparison.Ordinal);
        Assert.Contains("ApplicationUpdateService", macCode, StringComparison.Ordinal);
    }

    [Fact]
    public void PatchMatrixNavigatesBetweenRxSourcesAndTxDestinations()
    {
        string windowsXaml = File.ReadAllText(RepositoryFile("PatchWorkspaceView.xaml"));
        string windowsCode = File.ReadAllText(RepositoryFile("PatchWorkspaceView.xaml.cs"));
        string mainWindowCode = File.ReadAllText(RepositoryFile("MainWindow.xaml.cs"));
        string detachedWindowCode = File.ReadAllText(RepositoryFile("PatchWorkspaceWindow.xaml.cs"));
        string macCode = File.ReadAllText(RepositoryFile(
            "src",
            "DanteConfigEditor.Mac",
            "PatchWorkspaceDialog.axaml.cs"));

        Assert.Contains("x:Name=\"DetachMatrixButton\"", windowsXaml, StringComparison.Ordinal);
        Assert.Contains("MatrixRxSourceButton_Click", windowsCode, StringComparison.Ordinal);
        Assert.Contains("MatrixTxDestinationsButton_Click", windowsCode, StringComparison.Ordinal);
        Assert.Contains("ShowDestinationPicker", windowsCode, StringComparison.Ordinal);
        Assert.Contains("ScrollToAndHighlightConnection", windowsCode, StringComparison.Ordinal);
        Assert.Contains("BuildSourceNavigationToolTip", windowsCode, StringComparison.Ordinal);
        Assert.Contains("BuildDestinationNavigationToolTip", windowsCode, StringComparison.Ordinal);
        Assert.Contains("DetachRequested?.Invoke", windowsCode, StringComparison.Ordinal);
        Assert.Contains(
            "workspace.DetachRequested += EasyPatchWorkspace_DetachRequested",
            mainWindowCode,
            StringComparison.Ordinal);
        Assert.Contains("immediateMode: true", mainWindowCode, StringComparison.Ordinal);
        Assert.Contains("allowDetach: false", detachedWindowCode, StringComparison.Ordinal);
        Assert.Contains("if (_immediateMode)", detachedWindowCode, StringComparison.Ordinal);
        Assert.Contains("_workspace.ShowMatrixMode()", detachedWindowCode, StringComparison.Ordinal);

        Assert.Contains("MatrixRxSourceButton_Click", macCode, StringComparison.Ordinal);
        Assert.Contains("MatrixTxDestinationsButton_Click", macCode, StringComparison.Ordinal);
        Assert.Contains("ShowDestinationPickerAsync", macCode, StringComparison.Ordinal);
        Assert.Contains("ScrollToAndHighlightConnection", macCode, StringComparison.Ordinal);
        Assert.Contains("BuildSourceNavigationAutomationName", macCode, StringComparison.Ordinal);
        Assert.Contains("BuildDestinationNavigationAutomationName", macCode, StringComparison.Ordinal);
    }

    [Fact]
    public void PatchMatrixNavigationArrowsBorderTheGridAndSeriesHandlesStayByLabels()
    {
        string windowsCode = File.ReadAllText(RepositoryFile("PatchWorkspaceView.xaml.cs"));
        string macCode = File.ReadAllText(RepositoryFile(
            "src",
            "DanteConfigEditor.Mac",
            "PatchWorkspaceDialog.axaml.cs"));
        string columns = ExtractMethod(windowsCode, "private void BuildMatrixColumns()");
        string txHeader = ExtractMethod(
            windowsCode,
            "private FrameworkElement BuildMatrixHeader(PatchSourceDescriptor source)");

        Assert.Contains(
            "rxSeries.SetValue(FrameworkElement.MarginProperty, new Thickness(0, 0, 26, 0))",
            columns,
            StringComparison.Ordinal);
        Assert.Contains(
            "locateSource.SetValue(FrameworkElement.MarginProperty, new Thickness(0))",
            columns,
            StringComparison.Ordinal);
        Assert.True(
            columns.IndexOf("rxPanel.AppendChild(rxSeries)", StringComparison.Ordinal)
            < columns.IndexOf("rxPanel.AppendChild(locateSource)", StringComparison.Ordinal));
        Assert.Contains(
            "VerticalAlignment = VerticalAlignment.Bottom",
            txHeader,
            StringComparison.Ordinal);
        Assert.Contains(
            "series.VerticalAlignment = VerticalAlignment.Top",
            txHeader,
            StringComparison.Ordinal);
        Assert.Contains(
            "Grid.SetRow(series, 0)",
            txHeader,
            StringComparison.Ordinal);
        Assert.Contains(
            "Grid.SetRow(label, 1)",
            txHeader,
            StringComparison.Ordinal);
        Assert.Contains(
            "Grid.SetRow(destinationsButton, 2)",
            txHeader,
            StringComparison.Ordinal);
        Assert.Contains(
            "RenderTransform = new TranslateTransform(0, -10 * _matrixZoom)",
            txHeader,
            StringComparison.Ordinal);

        Assert.Contains(
            "VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom",
            macCode,
            StringComparison.Ordinal);
        Assert.Contains(
            "HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right",
            macCode,
            StringComparison.Ordinal);
    }

    [Fact]
    public void PatchMatrixKeepsHeadersFixedAndUsesIncrementalRowUpdates()
    {
        string xaml = File.ReadAllText(RepositoryFile("PatchWorkspaceView.xaml"));
        string codeBehind = File.ReadAllText(RepositoryFile("PatchWorkspaceView.xaml.cs"));
        XDocument document = XDocument.Parse(xaml);
        XNamespace xamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";
        XElement matrix = NamedElement(document, xamlNamespace, "MatrixGrid");

        Assert.Equal("1", matrix.Attribute("FrozenColumnCount")?.Value);
        Assert.Equal("Column", matrix.Attribute("HeadersVisibility")?.Value);
        Assert.Equal("True", matrix.Attribute("EnableRowVirtualization")?.Value);
        Assert.Equal("True", matrix.Attribute("EnableColumnVirtualization")?.Value);
        Assert.Contains("ObservableCollection<PatchMatrixRow>", codeBehind, StringComparison.Ordinal);
        Assert.Contains("RefreshTargetStates", codeBehind, StringComparison.Ordinal);
        Assert.Contains("RefreshTargetState(targetIndex)", codeBehind, StringComparison.Ordinal);

        int gestureStart = codeBehind.IndexOf("private void ExecuteMatrixGesture", StringComparison.Ordinal);
        int nextMethod = codeBehind.IndexOf("private void UpdateMatrixGestureHighlight", gestureStart, StringComparison.Ordinal);
        string gestureMethod = codeBehind[gestureStart..nextMethod];
        Assert.DoesNotContain("RefreshTargetRows()", gestureMethod, StringComparison.Ordinal);
    }

    [Fact]
    public void DeviceDetailsExposesRxPatchWorkspaceAndAppliesPatchesBeforeRenames()
    {
        string xaml = File.ReadAllText(RepositoryFile("DeviceDetailsWindow.xaml"));
        string codeBehind = File.ReadAllText(RepositoryFile("DeviceDetailsWindow.xaml.cs"));
        string mainWindow = File.ReadAllText(RepositoryFile("MainWindow.xaml.cs"));

        Assert.Contains("x:Name=\"PatchTab\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"OpenPatchWorkspaceButton\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"DeviceSelectorComboBox\"", xaml, StringComparison.Ordinal);
        Assert.Contains("returnEditsOnly: true", codeBehind, StringComparison.Ordinal);
        Assert.Contains("lockRxDeviceSelection: true", codeBehind, StringComparison.Ordinal);
        Assert.Contains("RequestedDeviceName", codeBehind, StringComparison.Ordinal);

        int patchLoop = mainWindow.IndexOf("foreach (PatchEditRequest edit in result.PatchEdits)", StringComparison.Ordinal);
        int rename = mainWindow.IndexOf("_project.RenameDevice(currentName, result.DeviceName)", StringComparison.Ordinal);
        Assert.True(patchLoop >= 0 && rename > patchLoop, "Les patchs du détail machine doivent être appliqués avant les renommages.");
    }

    private static int CountOccurrences(string value, string expected)
    {
        int count = 0;
        int offset = 0;
        while ((offset = value.IndexOf(expected, offset, StringComparison.Ordinal)) >= 0)
        {
            count++;
            offset += expected.Length;
        }

        return count;
    }

    private static string ExtractMethod(string source, string signature)
    {
        int start = source.IndexOf(signature, StringComparison.Ordinal);
        Assert.True(start >= 0, $"Méthode introuvable : {signature}");

        int bodyStart = source.IndexOf('{', start);
        Assert.True(bodyStart >= 0, $"Corps de méthode introuvable : {signature}");

        int depth = 0;
        for (int index = bodyStart; index < source.Length; index++)
        {
            depth += source[index] switch
            {
                '{' => 1,
                '}' => -1,
                _ => 0
            };

            if (depth == 0)
            {
                return source[start..(index + 1)];
            }
        }

        throw new InvalidOperationException($"Fin de méthode introuvable : {signature}");
    }

    private static XElement NamedElement(XDocument document, XNamespace xamlNamespace, string name)
    {
        return document.Descendants()
            .Single(element => string.Equals((string?)element.Attribute(xamlNamespace + "Name"), name, StringComparison.Ordinal));
    }

    private static string RepositoryFile(params string[] relativeParts)
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "DanteConfigEditorV3.csproj")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);
        return Path.Combine([directory!.FullName, .. relativeParts]);
    }
}
