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
    public void MainWindowKeepsPatchAndAddsEmbeddedEasyPatchTab()
    {
        string xaml = File.ReadAllText(RepositoryFile("MainWindow.xaml"));
        string codeBehind = File.ReadAllText(RepositoryFile("MainWindow.xaml.cs"));

        Assert.Contains("x:Name=\"ClassicPatchTab\" Header=\"Patch\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"EasyPatchTab\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Header=\"Easy patch\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"EasyPatchHost\"", xaml, StringComparison.Ordinal);
        Assert.Contains("embedded: true", codeBehind, StringComparison.Ordinal);
        Assert.Contains("EasyPatchWorkspace_DirectApplyRequested", codeBehind, StringComparison.Ordinal);
        Assert.DoesNotContain("EasyPatchWorkspace_ApplyRequested", codeBehind, StringComparison.Ordinal);
    }

    [Fact]
    public void EasyPatchOpensOnTheMatrixThenOffersSelectionAndInlineRename()
    {
        string xaml = File.ReadAllText(RepositoryFile("PatchWorkspaceView.xaml"));
        string codeBehind = File.ReadAllText(RepositoryFile("PatchWorkspaceView.xaml.cs"));

        Assert.Contains("PatchModeTabControl.Items.Insert(0, MatrixTab)", codeBehind, StringComparison.Ordinal);
        Assert.Contains("MatrixTab.IsSelected = true", codeBehind, StringComparison.Ordinal);
        Assert.Contains("startInAssignmentMode", codeBehind, StringComparison.Ordinal);
        Assert.Contains("IsAssignmentModeSelected", codeBehind, StringComparison.Ordinal);
        Assert.Contains("InlineChannelNameTextBox_LostKeyboardFocus", xaml, StringComparison.Ordinal);
        Assert.Equal(2, CountOccurrences(xaml, "PreviewMouseLeftButtonDown=\"InlineChannelNameTextBox_PreviewMouseLeftButtonDown\""));
        Assert.Contains("ChannelSeriesThumb_DragStarted", xaml, StringComparison.Ordinal);
        Assert.Contains("ChannelSeriesThumb_DragCompleted", xaml, StringComparison.Ordinal);
        Assert.Contains("MatrixTxHeader_Click", codeBehind, StringComparison.Ordinal);
        Assert.Contains("MinHeight=\"230\"", xaml, StringComparison.Ordinal);
        Assert.Contains("ColumnHeaderHeight=\"132\"", xaml, StringComparison.Ordinal);
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
        Assert.Contains("ApplyMatrixCellDirectly", codeBehind, StringComparison.Ordinal);
        Assert.Contains("ApplyPlanImmediately", codeBehind, StringComparison.Ordinal);
        Assert.Contains("BuildCommittedPreview", codeBehind, StringComparison.Ordinal);
        Assert.Contains("DirectApplyRequested", codeBehind, StringComparison.Ordinal);
        Assert.Contains("WarnOnExistingPatch", codeBehind, StringComparison.Ordinal);
        Assert.Contains("if (!WarnOnExistingPatch)", codeBehind, StringComparison.Ordinal);

        string mainCode = File.ReadAllText(RepositoryFile("MainWindow.xaml.cs"));
        Assert.Contains("EasyPatchWorkspace_DirectApplyRequested", mainCode, StringComparison.Ordinal);
        Assert.DoesNotContain("pendingEdits", mainCode[mainCode.IndexOf("private void RefreshEasyPatchWorkspace", StringComparison.Ordinal)..mainCode.IndexOf("private void ApplyPatchEdits", StringComparison.Ordinal)], StringComparison.Ordinal);
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
        XElement quickLists = editors.Descendants().Single(element =>
            element.Name.LocalName == "GroupBox" && element.Attribute("Header")?.Value == "Listes rapides");
        XElement device = editors.Descendants().Single(element =>
            element.Name.LocalName == "GroupBox" && element.Attribute("Header")?.Value == "Machine sélectionnée");
        XElement channels = editors.Descendants().Single(element =>
            element.Name.LocalName == "GroupBox" && element.Attribute("Header")?.Value == "Canaux de la machine");

        Assert.Equal("0", quickLists.Parent?.Attribute("Grid.Column")?.Value);
        Assert.Equal("1", device.Attribute("Grid.Column")?.Value);
        Assert.Equal("2", channels.Attribute("Grid.Column")?.Value);
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
