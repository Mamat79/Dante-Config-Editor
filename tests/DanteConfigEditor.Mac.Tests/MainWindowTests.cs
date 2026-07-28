using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Threading;
using DanteConfigEditor.Models;
using DanteConfigEditor.Services;

namespace DanteConfigEditor.Mac.Tests;

public sealed class MainWindowTests
{
    [AvaloniaFact]
    public void Beta2026VersionIsShownInMacApplication()
    {
        string version = typeof(MainWindow).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? string.Empty;
        MainWindow window = new();
        window.Show();
        try
        {
            ApplyLanguage(window, UiLanguage.English);
            Assert.Equal("2026.1.0-beta.1", version);
            Assert.Equal("Dante Config Editor 2026.1 Beta - macOS", window.Title);
            Assert.Equal("Add XML", LocalizationService.TranslateLiteral(UiLanguage.English, "Ajouter XML"));
            Assert.Equal("Device or channel", LocalizationService.TranslateLiteral(UiLanguage.English, "Machine ou canal"));
            Assert.Equal("All", LocalizationService.TranslateLiteral(UiLanguage.English, "Toutes"));
            Assert.Equal(
                "All",
                (window.FindControl<ComboBox>("DeviceFilterCombo")!.SelectedItem as ComboBoxItem)?.Content);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ConfigurationEditorsAreVisibleOnFirstLaunch()
    {
        MainWindow window = new();
        window.Show();
        try
        {
            Grid editors = window.FindControl<Grid>("ConfigurationEditorsGrid")!;
            Button reveal = window.FindControl<Button>("ConfigurationEditorsRevealButton")!;
            Assert.True(window.FindControl<TabItem>("ConfigurationTab")!.IsSelected);
            Assert.True(editors.IsEffectivelyVisible);
            Assert.Equal("▲", reveal.Content);

            reveal.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Dispatcher.UIThread.RunJobs();
            Assert.False(editors.IsVisible);
            Assert.Equal("▼", reveal.Content);
            Assert.True(reveal.IsEffectivelyVisible);

            reveal.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Dispatcher.UIThread.RunJobs();
            Assert.True(editors.IsEffectivelyVisible);
            Assert.Equal("▲", reveal.Content);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void DeviceActionsRemainInAVisibleToolbarBelowTheEditors()
    {
        MainWindow window = new();
        window.Show();
        try
        {
            Button duplicate = window.FindControl<Button>("DuplicateDeviceButton")!;
            Border toolbar = duplicate.GetLogicalAncestors().OfType<Border>().First();

            Assert.Equal(2, Grid.GetRow(toolbar));
            Assert.Equal(3, Grid.GetRow(window.FindControl<DataGrid>("DeviceGrid")!));
            Assert.True(duplicate.IsEffectivelyVisible);
            Assert.True(window.FindControl<Button>("SaveDeviceToBankButton")!.IsEffectivelyVisible);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void SynopticPreviewCanOpenInASeparateScalableWindow()
    {
        Canvas source = new()
        {
            Width = 900,
            Height = 560,
            Background = Avalonia.Media.Brushes.White
        };
        SynopticPreviewWindow preview = new(source, source.Width, source.Height, UiLanguage.French);
        preview.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("Aperçu du synoptique", preview.Title);
            Assert.True(preview.FindControl<Control>("PreviewSurface")!.IsEffectivelyVisible);
            Assert.True(preview.FindControl<Slider>("ZoomSlider")!.Value > 0);
            Assert.Equal("Fermer", preview.FindControl<Button>("CloseButton")!.Content);
        }
        finally
        {
            preview.Close();
        }
    }

    [AvaloniaFact]
    public async Task VisualPatchOpenedFromTheMacAppUsesImmediateMode()
    {
        string source = Path.Combine(AppContext.BaseDirectory, "Fixtures", "representative-preset.xml");
        DanteProject project = DanteProject.Load(source);
        IReadOnlyList<PatchEditRequest>? applied = null;
        PatchWorkspaceDialog dialog = new(
            UiLanguage.French,
            project,
            initialTxDeviceName: "DEVICE-A",
            initialRxDeviceName: "DEVICE-B",
            immediateApply: edits =>
            {
                applied = edits.ToArray();
                project.ApplyBatch(batch =>
                {
                    foreach (PatchEditRequest edit in edits)
                    {
                        if (edit.IsRemoval)
                        {
                            batch.RemovePatch(edit.RxDeviceName, edit.RxDanteId);
                        }
                        else
                        {
                            batch.ApplyPatch(
                                edit.RxDeviceName,
                                edit.RxDanteId,
                                edit.TxDeviceName!,
                                edit.TxChannelName ?? string.Empty);
                        }
                    }
                });
                return Task.CompletedTask;
            });
        dialog.Show();
        try
        {
            Assert.True(dialog.FindControl<CheckBox>("WarnOnExistingPatchCheckBox")!.IsChecked);
            Assert.False(dialog.FindControl<Button>("ResetPendingButton")!.IsVisible);
            Assert.False(dialog.FindControl<Button>("ApplyButton")!.IsVisible);
            Assert.Equal("Appliquer", dialog.FindControl<Button>("PreviewOneToOneButton")!.Content);
            Assert.Contains("Cliquez", dialog.FindControl<TextBlock>("InfoText")!.Text);

            dialog.FindControl<TabItem>("AssignmentTab")!.IsSelected = true;
            Dispatcher.UIThread.RunJobs();
            Assert.Contains("Sélectionnez", dialog.FindControl<TextBlock>("InfoText")!.Text);
            dialog.FindControl<TabItem>("MatrixTab")!.IsSelected = true;

            Grid matrix = dialog.FindControl<Grid>("MatrixPanel")!;
            Button activeCell = matrix.Children.OfType<Button>()
                .First(button => Equals(button.Content, "●"));
            activeCell.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Dispatcher.UIThread.RunJobs();
            await Task.Yield();

            PatchEditRequest edit = Assert.Single(Assert.IsAssignableFrom<IReadOnlyList<PatchEditRequest>>(applied));
            Assert.True(edit.IsRemoval);
            Assert.True(project.IsModified);
            Assert.Empty(dialog.Edits);
        }
        finally
        {
            dialog.Close();
        }
    }

    [AvaloniaFact]
    public void LanguageSwitchTranslatesTabHeadersAndComboBoxPlaceholdersBothWays()
    {
        MainWindow window = new();
        window.Show();
        try
        {
            ApplyLanguage(window, UiLanguage.English);

            Assert.Equal("Devices", window.FindControl<TabItem>("ConfigurationTab")!.Header);
            Assert.Equal("Reports and patchbook", window.FindControl<TabItem>("ReportsExportTab")!.Header);
            Assert.Equal("Synoptic", window.FindControl<TabItem>("SynopticTab")!.Header);
            Assert.Equal("Validation center", window.FindControl<TabItem>("HealthTab")!.Header);
            Assert.Equal("Items to check", window.FindControl<TabItem>("ValidationIssuesTab")!.Header);
            Assert.Equal("Reports", window.FindControl<TabItem>("ValidationReportsTab")!.Header);
            Assert.Equal("History", window.FindControl<TabItem>("SafetyTab")!.Header);
            Assert.Equal("Advanced tools", window.FindControl<TabItem>("AtomicTab")!.Header);
            Assert.Equal("Rx to Tx list", window.FindControl<TextBlock>("PatchListHeading")!.Text);
            Assert.Equal("Easy patch / Matrix", window.FindControl<Button>("VisualPatchButton")!.Content);
            Assert.Equal("Recent files", window.FindControl<ComboBox>("RecentCombo")!.PlaceholderText);
            Assert.Equal("Start channel", window.FindControl<ComboBox>("StartChannelCombo")!.PlaceholderText);
            Assert.Equal("Tx source to apply", window.FindControl<ComboBox>("SourceDeviceCombo")!.PlaceholderText);
            Assert.Equal("Add XML", window.FindControl<Button>("MergeButton")!.Content);
            Assert.Equal("New project", window.FindControl<Button>("NewProjectButton")!.Content);
            Assert.Equal("Device bank", window.FindControl<Button>("OpenMachineBankButton")!.Content);
            Assert.Equal("Duplicate", window.FindControl<Button>("DuplicateDeviceButton")!.Content);
            Assert.Equal("Save to device bank", window.FindControl<Button>("SaveDeviceToBankButton")!.Content);
            Assert.Equal("Open logs", window.FindControl<Button>("OpenDiagnosticLogsButton")!.Content);
            Assert.Equal(
                ["Français", "English"],
                window.FindControl<ComboBox>("LanguageCombo")!.Items
                    .OfType<ComboBoxItem>()
                    .Select(item => item.Content?.ToString() ?? string.Empty)
                    .ToArray());

            ApplyLanguage(window, UiLanguage.French);

            Assert.Equal("Machines", window.FindControl<TabItem>("ConfigurationTab")!.Header);
            Assert.Equal("Rapports et patchbook", window.FindControl<TabItem>("ReportsExportTab")!.Header);
            Assert.Equal("Synoptique", window.FindControl<TabItem>("SynopticTab")!.Header);
            Assert.Equal("Centre de validation", window.FindControl<TabItem>("HealthTab")!.Header);
            Assert.Equal("Historique", window.FindControl<TabItem>("SafetyTab")!.Header);
            Assert.Equal("Outils avancés", window.FindControl<TabItem>("AtomicTab")!.Header);
            Assert.Equal("Easy patch / Matrice", window.FindControl<Button>("VisualPatchButton")!.Content);
            Assert.Equal("Fichiers récents", window.FindControl<ComboBox>("RecentCombo")!.PlaceholderText);
            Assert.Equal("Nouveau projet", window.FindControl<Button>("NewProjectButton")!.Content);
            Assert.Equal("Banque de machines", window.FindControl<Button>("OpenMachineBankButton")!.Content);
            Assert.Equal(
                ["Français", "English"],
                window.FindControl<ComboBox>("LanguageCombo")!.Items
                    .OfType<ComboBoxItem>()
                    .Select(item => item.Content?.ToString() ?? string.Empty)
                    .ToArray());
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void SecondaryMacViewsUsePolishedEnglishLabels()
    {
        string source = Path.Combine(AppContext.BaseDirectory, "Fixtures", "representative-preset.xml");
        DanteProject project = DanteProject.Load(source);
        PatchWorkspaceDialog patch = new(
            UiLanguage.English,
            project,
            initialTxDeviceName: "DEVICE-A",
            initialRxDeviceName: "DEVICE-B");
        MachineBankDialog bank = new();
        Canvas synopticSource = new()
        {
            Width = 900,
            Height = 560,
            Background = Avalonia.Media.Brushes.White
        };
        SynopticPreviewWindow synoptic = new(
            synopticSource,
            synopticSource.Width,
            synopticSource.Height,
            UiLanguage.English);

        typeof(MachineBankDialog)
            .GetField("_language", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(bank, UiLanguage.English);
        bank.Title = "Device bank";
        typeof(MachineBankDialog)
            .GetMethod("ApplyLanguage", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(bank, null);

        patch.Show();
        bank.Show();
        synoptic.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("Easy patch / Matrix", patch.Title);
            Assert.Equal("Transmitting device (Tx)", patch.FindControl<TextBlock>("TxDeviceLabel")!.Text);
            Assert.Equal("Receiving device (Rx)", patch.FindControl<TextBlock>("RxDeviceLabel")!.Text);
            Assert.Equal("Selection and one-to-one patch", patch.FindControl<TabItem>("AssignmentTab")!.Header);
            Assert.Equal("Patch matrix", patch.FindControl<TabItem>("MatrixTab")!.Header);
            Assert.Equal("GitHub banks", bank.FindControl<Button>("GithubBanksButton")!.Content);
            Assert.Equal("Export bank", bank.FindControl<Button>("BackupBankButton")!.Content);
            Assert.Equal("Import bank", bank.FindControl<Button>("RestoreBankButton")!.Content);
            Assert.Equal("Synoptic preview", synoptic.Title);
            Assert.Equal("Close", synoptic.FindControl<Button>("CloseButton")!.Content);
        }
        finally
        {
            synoptic.Close();
            bank.Close();
            patch.Close();
        }
    }

    [AvaloniaFact]
    public void SynopticRemainsInsideImportExportAndPatchHasOneVisibleEntryPoint()
    {
        MainWindow window = new();
        window.Show();
        try
        {
            TabControl mainTabs = window.FindControl<TabControl>("MainTabs")!;
            TabItem exports = window.FindControl<TabItem>("ExportsTab")!;
            TabItem synoptic = window.FindControl<TabItem>("SynopticTab")!;
            Button patchWorkspace = window.FindControl<Button>("VisualPatchButton")!;

            Assert.Contains(synoptic.GetLogicalAncestors(), ancestor => ReferenceEquals(ancestor, exports));
            Assert.DoesNotContain(mainTabs.Items.OfType<TabItem>(), item => ReferenceEquals(item, synoptic));
            Assert.Equal(
                1,
                window.GetLogicalDescendants()
                    .OfType<Button>()
                    .Count(button => ReferenceEquals(button, patchWorkspace)));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ImportantWarnings_AreKeptInsideProjectSidebar()
    {
        MainWindow window = new();
        window.Show();
        try
        {
            Border sidebar = window.FindControl<Border>("ProjectSidebar")!;
            Border warning = window.FindControl<Border>("WarningBorder")!;
            TabControl tabs = window.FindControl<TabControl>("MainTabs")!;

            Assert.Contains(warning.GetLogicalAncestors(), ancestor => ReferenceEquals(ancestor, sidebar));
            Assert.Equal(0, Grid.GetColumn(sidebar));
            Assert.Equal(1, Grid.GetColumn(tabs));
            Assert.False(warning.IsVisible);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task MixedAudioFormats_ShowWarningInProjectSidebar()
    {
        string source = Path.Combine(AppContext.BaseDirectory, "Fixtures", "representative-preset.xml");
        string temporaryXml = Path.Combine(Path.GetTempPath(), $"dante-mac-ui-{Guid.NewGuid():N}.xml");
        File.Copy(source, temporaryXml);

        MainWindow window = new();
        window.Show();
        try
        {
            await window.OpenStartupFileAsync(temporaryXml);
            ApplyLanguage(window, UiLanguage.English);

            Border sidebar = window.FindControl<Border>("ProjectSidebar")!;
            Border warning = window.FindControl<Border>("WarningBorder")!;
            TextBlock warningText = window.FindControl<TextBlock>("WarningText")!;
            TextBlock summary = window.FindControl<TextBlock>("ProjectSummaryText")!;

            Assert.True(warning.IsVisible);
            Assert.False(string.IsNullOrWhiteSpace(warningText.Text));
            Assert.Contains("3", summary.Text);
            Assert.Contains(warning.GetLogicalAncestors(), ancestor => ReferenceEquals(ancestor, sidebar));
            Assert.Contains(
                window.FindControl<DataGrid>("DeviceGrid")!.ItemsSource!.Cast<DeviceRow>(),
                row => row.Ip.StartsWith("Static (", StringComparison.Ordinal));
            Assert.Contains(
                window.FindControl<DataGrid>("PatchGrid")!.ItemsSource!.Cast<PatchRow>(),
                row => row.Status == "Active subscription");
            Assert.Contains(
                window.FindControl<DataGrid>("PatchGrid")!.ItemsSource!.Cast<PatchRow>(),
                row => row.Source == "No source");
            Assert.DoesNotContain(
                window.FindControl<DataGrid>("PatchGrid")!.ItemsSource!.Cast<PatchRow>(),
                row => row.Source == "Aucune source");
            HealthRow[] healthRows = window.FindControl<DataGrid>("HealthGrid")!
                .ItemsSource!
                .Cast<HealthRow>()
                .ToArray();
            Assert.Contains(healthRows, row => row.Message == "DEVICE-B has no TX channel.");
            Assert.Contains(healthRows, row => row.Message.Contains("uses the local source", StringComparison.Ordinal));
            Assert.DoesNotContain(
                healthRows,
                row => row.Message.Contains("présent", StringComparison.OrdinalIgnoreCase)
                    || row.Message.Contains("contient aucun", StringComparison.OrdinalIgnoreCase)
                    || row.Message.Contains("est libre", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            window.Close();
            SessionRecoveryService.Delete(temporaryXml);
            File.Delete(temporaryXml);
        }
    }

    [AvaloniaFact]
    public async Task Compact1366By768LayoutKeepsPrimaryAreasInsideTheWindow()
    {
        await AssertPrimaryAreasFitAsync(1366, 768);
    }

    [AvaloniaFact]
    public async Task FullHdLayoutKeepsPrimaryAreasInsideTheWindow()
    {
        await AssertPrimaryAreasFitAsync(1920, 1080);
    }

    [AvaloniaFact]
    public async Task CompactPatchToolbarKeepsVisualWorkspaceButtonVisible()
    {
        string source = Path.Combine(AppContext.BaseDirectory, "Fixtures", "representative-preset.xml");
        string temporaryXml = Path.Combine(Path.GetTempPath(), $"dante-mac-patch-layout-{Guid.NewGuid():N}.xml");
        File.Copy(source, temporaryXml);

        MainWindow window = new() { Width = 1366, Height = 768 };
        window.Show();
        try
        {
            await window.OpenStartupFileAsync(temporaryXml);
            AssertControlFits(window, window.FindControl<Button>("NewProjectButton")!);
            AssertControlFits(window, window.FindControl<Button>("OpenMachineBankButton")!);
            window.FindControl<TabItem>("PatchTab")!.IsSelected = true;
            Dispatcher.UIThread.RunJobs();

            AssertControlFits(window, window.FindControl<Button>("VisualPatchButton")!);
            AssertControlFits(window, window.FindControl<DataGrid>("PatchGrid")!);
        }
        finally
        {
            window.Close();
            SessionRecoveryService.Delete(temporaryXml);
            File.Delete(temporaryXml);
        }
    }

    [AvaloniaFact]
    public async Task AtomicPanelUnlocksFireOnlyAfterFullSequence()
    {
        string source = Path.Combine(AppContext.BaseDirectory, "Fixtures", "representative-preset.xml");
        string temporaryXml = Path.Combine(Path.GetTempPath(), $"dante-mac-atomic-layout-{Guid.NewGuid():N}.xml");
        File.Copy(source, temporaryXml);

        MainWindow window = new() { Width = 1366, Height = 768 };
        window.Show();
        try
        {
            Button fireButton = window.FindControl<Button>("AtomicChaosButton")!;
            Button keyButton = window.FindControl<Button>("AtomicKeyButton")!;
            Border cover = window.FindControl<Border>("AtomicSafetyCover")!;
            Button armButton = window.FindControl<Button>("AtomicArmButton")!;
            Button lockButton = window.FindControl<Button>("AtomicLockButton")!;
            Assert.False(keyButton.IsEnabled);
            Assert.False(fireButton.IsEnabled);

            await window.OpenStartupFileAsync(temporaryXml);
            TabItem safetyTab = window.FindControl<TabItem>("SafetyTab")!;
            TabItem atomicTab = window.FindControl<TabItem>("AtomicTab")!;
            atomicTab.IsSelected = true;
            Dispatcher.UIThread.RunJobs();

            Assert.True(keyButton.IsEnabled);
            Assert.False(fireButton.IsEnabled);
            Assert.Equal("OFF", window.FindControl<TextBlock>("AtomicKeyPositionTextBlock")!.Text);
            keyButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.False(cover.IsVisible);
            Assert.Equal("ON", window.FindControl<TextBlock>("AtomicKeyPositionTextBlock")!.Text);
            RotateTransform turnedKey = Assert.IsType<RotateTransform>(
                window.FindControl<Grid>("AtomicKeyVisual")!.RenderTransform);
            Assert.Equal(90, turnedKey.Angle);
            Assert.True(armButton.IsEnabled);

            keyButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.True(cover.IsVisible);
            Assert.Equal("OFF", window.FindControl<TextBlock>("AtomicKeyPositionTextBlock")!.Text);
            Assert.False(armButton.IsEnabled);

            keyButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.False(cover.IsVisible);
            armButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.True(lockButton.IsEnabled);
            Assert.False(fireButton.IsEnabled);
            lockButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.True(fireButton.IsEnabled);
            Assert.True(MainTabs(window).Items.IndexOf(atomicTab) > MainTabs(window).Items.IndexOf(safetyTab));
            AssertControlFits(window, fireButton);
            foreach (string checkBoxName in new[]
            {
                "AtomicDeviceNamesCheckBox", "AtomicTxLabelsCheckBox", "AtomicRxLabelsCheckBox",
                "AtomicPatchesCheckBox", "AtomicNetworkModeCheckBox", "AtomicPreferredMasterCheckBox",
                "AtomicLatencyCheckBox", "AtomicSampleRateCheckBox", "AtomicEncodingCheckBox", "AtomicIpCheckBox"
            })
            {
                CheckBox checkBox = window.FindControl<CheckBox>(checkBoxName)!;
                Assert.True(checkBox.IsChecked);
                AssertControlFits(window, checkBox);
            }
        }
        finally
        {
            window.Close();
            SessionRecoveryService.Delete(temporaryXml);
            File.Delete(temporaryXml);
        }
    }

    [AvaloniaFact]
    public async Task LabelExportForRxOnlyDeviceOpensDirectlyOnRx()
    {
        string source = Path.Combine(AppContext.BaseDirectory, "Fixtures", "representative-preset.xml");
        string temporaryXml = Path.Combine(Path.GetTempPath(), $"dante-mac-label-export-{Guid.NewGuid():N}.xml");
        File.Copy(source, temporaryXml);

        MainWindow window = new() { Width = 1366, Height = 768 };
        window.Show();
        try
        {
            await window.OpenStartupFileAsync(temporaryXml);
            DataGrid devices = window.FindControl<DataGrid>("DeviceGrid")!;
            devices.SelectedIndex = 1; // DEVICE-B : 0 TX, 2 RX.

            window.FindControl<Button>("ExportChannelLabelsButton")!
                .RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Dispatcher.UIThread.RunJobs();

            Window dialog = Assert.Single(window.OwnedWindows);
            Assert.Equal(1, dialog.FindControl<ComboBox>("KindCombo")!.SelectedIndex);
            Assert.True(dialog.FindControl<Button>("ExportButton")!.IsEnabled);
            Assert.Contains("2", dialog.FindControl<TextBlock>("SummaryText")!.Text);
            dialog.Close();
        }
        finally
        {
            window.Close();
            SessionRecoveryService.Delete(temporaryXml);
            File.Delete(temporaryXml);
        }
    }

    [AvaloniaFact]
    public async Task ReimportingIdenticalLabelsExplainsWhyApplyIsDisabledAndKeepsButtonsVisible()
    {
        string source = Path.Combine(AppContext.BaseDirectory, "Fixtures", "representative-preset.xml");
        DanteProject project = DanteProject.Load(source);
        ChannelLabelDocument document = ChannelLabelExchangeService.CreateFromProject(
            project,
            ["DEVICE-A"],
            DanteChannelKind.Tx);
        MainWindow owner = new() { Width = 1920, Height = 1080 };
        owner.Show();

        Task<IReadOnlyList<ChannelLabelAssignment>?> resultTask = ChannelLabelImportDialog.ShowAsync(
            owner,
            project,
            document,
            new ChannelLabelImportReport(
                "Test JSON",
                document.SourceApplication,
                document.SourceVersion,
                document.Sets.Count,
                1,
                document.Sets.Sum(set => set.Channels.Count),
                0,
                0,
                0,
                [],
                []),
            UiLanguage.English,
            "DEVICE-A");
        Dispatcher.UIThread.RunJobs();

        Window dialog = Assert.Single(owner.OwnedWindows);
        try
        {
            Button preview = dialog.FindControl<Button>("PreviewButton")!;
            Button apply = dialog.FindControl<Button>("ApplyButton")!;
            dialog.Width = 900;
            dialog.Height = 620;
            Dispatcher.UIThread.RunJobs();
            preview.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Dispatcher.UIThread.RunJobs();

            AssertControlFits(dialog, preview);
            AssertControlFits(dialog, apply);
            AssertControlFitsInside(
                dialog.FindControl<TextBlock>("MappingTitle")!.GetLogicalAncestors().OfType<Border>().First(),
                preview);
            Assert.True(preview.Bounds.Height >= 34);
            Assert.False(apply.IsEnabled);
            Assert.Contains("Test JSON", dialog.FindControl<TextBlock>("SourceInfoText")!.Text);
            Assert.Contains("0 change", dialog.FindControl<TextBlock>("PreviewSummaryText")!.Text);
            Assert.Contains("already match", dialog.FindControl<TextBlock>("SafetyText")!.Text);
        }
        finally
        {
            dialog.Close();
            owner.Close();
        }

        Assert.Null(await resultTask);
    }

    [AvaloniaFact]
    public async Task LabelExportPreviewActionBelongsToPreviewPanelAndFitsCompactWindow()
    {
        string source = Path.Combine(AppContext.BaseDirectory, "Fixtures", "representative-preset.xml");
        DanteProject project = DanteProject.Load(source);
        MainWindow owner = new() { Width = 1366, Height = 768 };
        owner.Show();

        Task<ChannelLabelExportDialogResult?> resultTask = ChannelLabelExportDialog.ShowAsync(
            owner,
            project,
            UiLanguage.English,
            "DEVICE-A");
        Dispatcher.UIThread.RunJobs();

        Window dialog = Assert.Single(owner.OwnedWindows);
        try
        {
            dialog.Width = 900;
            dialog.Height = 650;
            Dispatcher.UIThread.RunJobs();

            Button preview = dialog.FindControl<Button>("PreviewButton")!;
            TextBlock title = dialog.FindControl<TextBlock>("PreviewTitle")!;
            Border previewPanel = title.GetLogicalAncestors().OfType<Border>().First();

            Assert.Equal("Refresh preview", preview.Content);
            Assert.Contains(previewPanel, preview.GetLogicalAncestors());
            AssertControlFits(dialog, preview);
            AssertControlFitsInside(previewPanel, preview);
            AssertControlFits(dialog, dialog.FindControl<Button>("ExportButton")!);
        }
        finally
        {
            dialog.Close();
            owner.Close();
        }

        Assert.Null(await resultTask);
    }

    private static TabControl MainTabs(MainWindow window) => window.FindControl<TabControl>("MainTabs")!;

    [AvaloniaFact]
    public async Task TabKeyMovesFocusFromOpenToMergeAfterProjectLoad()
    {
        string source = Path.Combine(AppContext.BaseDirectory, "Fixtures", "representative-preset.xml");
        string temporaryXml = Path.Combine(Path.GetTempPath(), $"dante-mac-keyboard-{Guid.NewGuid():N}.xml");
        File.Copy(source, temporaryXml);

        MainWindow window = new() { Width = 1366, Height = 768 };
        window.Show();
        try
        {
            await window.OpenStartupFileAsync(temporaryXml);
            Button open = window.FindControl<Button>("OpenButton")!;
            Button merge = window.FindControl<Button>("MergeButton")!;

            Assert.True(open.Focus());
            Assert.True(open.IsFocused);
            window.KeyPressQwerty(PhysicalKey.Tab, RawInputModifiers.None);
            window.KeyReleaseQwerty(PhysicalKey.Tab, RawInputModifiers.None);

            Assert.True(merge.IsFocused);
        }
        finally
        {
            window.Close();
            SessionRecoveryService.Delete(temporaryXml);
            File.Delete(temporaryXml);
        }
    }

    [AvaloniaFact]
    public void VisualPatchDialogStagesMatrixEditWithoutChangingProject()
    {
        string source = Path.Combine(AppContext.BaseDirectory, "Fixtures", "representative-preset.xml");
        DanteProject project = DanteProject.Load(source);
        PatchWorkspaceDialog dialog = new(
            UiLanguage.French,
            project,
            initialTxDeviceName: "DEVICE-A",
            initialRxDeviceName: "DEVICE-B")
        {
            Width = 960,
            Height = 640
        };
        dialog.Show();

        try
        {
            ListBox txList = dialog.FindControl<ListBox>("TxChannelList")!;
            ListBox rxList = dialog.FindControl<ListBox>("RxChannelList")!;
            Grid matrix = dialog.FindControl<Grid>("MatrixPanel")!;
            Grid txHeaders = dialog.FindControl<Grid>("MatrixTxHeaderPanel")!;
            Grid rxHeaders = dialog.FindControl<Grid>("MatrixRxHeaderPanel")!;
            Button apply = dialog.FindControl<Button>("ApplyButton")!;

            Assert.Equal(2, txList.ItemCount);
            Assert.Equal(2, rxList.ItemCount);
            Assert.Equal(2, matrix.ColumnDefinitions.Count);
            Assert.Equal(2, matrix.RowDefinitions.Count);
            Assert.Equal(2, txHeaders.ColumnDefinitions.Count);
            Assert.Equal(2, rxHeaders.RowDefinitions.Count);
            Assert.Equal(1, Grid.GetColumn(dialog.FindControl<ScrollViewer>("MatrixTxHeaderScrollViewer")!));
            Assert.Equal(1, Grid.GetRow(dialog.FindControl<ScrollViewer>("MatrixRxHeaderScrollViewer")!));
            Assert.False(apply.IsEnabled);
            Assert.Empty(dialog.Edits);
            Assert.False(project.IsModified);

            int matrixBuildCount = dialog.MatrixBuildCount;
            Button activeCell = matrix.Children.OfType<Button>().First(button => Equals(button.Content, "●"));
            activeCell.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            Assert.True(apply.IsEnabled);
            Assert.Equal(matrixBuildCount, dialog.MatrixBuildCount);
            PatchEditRequest edit = Assert.Single(dialog.Edits);
            Assert.True(edit.IsRemoval);
            Assert.Equal("DEVICE-B", edit.RxDeviceName);
            Assert.False(project.IsModified);

            dialog.FindControl<TabItem>("MatrixTab")!.IsSelected = true;
            Dispatcher.UIThread.RunJobs();
            AssertControlFits(dialog, matrix);
        }
        finally
        {
            dialog.Close();
        }
    }

    private static async Task AssertPrimaryAreasFitAsync(double width, double height)
    {
        string source = Path.Combine(AppContext.BaseDirectory, "Fixtures", "representative-preset.xml");
        string temporaryXml = Path.Combine(Path.GetTempPath(), $"dante-mac-layout-{Guid.NewGuid():N}.xml");
        File.Copy(source, temporaryXml);

        MainWindow window = new() { Width = width, Height = height };
        window.Show();
        try
        {
            await window.OpenStartupFileAsync(temporaryXml);

            AssertControlFits(window, window.FindControl<Border>("ProjectSidebar")!);
            AssertControlFits(window, window.FindControl<TabControl>("MainTabs")!);
            AssertControlFits(window, window.FindControl<DataGrid>("DeviceGrid")!);

            window.FindControl<TabItem>("ExportsTab")!.IsSelected = true;
            window.FindControl<TabItem>("ChannelLabelsTab")!.IsSelected = true;
            Dispatcher.UIThread.RunJobs();
            AssertControlFits(window, window.FindControl<Button>("ImportChannelLabelsButton")!);
            AssertControlFits(window, window.FindControl<Button>("ExportChannelLabelsButton")!);
        }
        finally
        {
            window.Close();
            SessionRecoveryService.Delete(temporaryXml);
            File.Delete(temporaryXml);
        }
    }

    private static void AssertControlFits(Window window, Control control)
    {
        Point? origin = control.TranslatePoint(default, window);
        Assert.NotNull(origin);
        Assert.True(control.IsEffectivelyVisible, $"{control.Name} devrait être visible.");
        Assert.True(control.Bounds.Width > 0, $"{control.Name} devrait avoir une largeur positive.");
        Assert.True(control.Bounds.Height > 0, $"{control.Name} devrait avoir une hauteur positive.");
        Assert.InRange(origin.Value.X, -0.5, window.ClientSize.Width + 0.5);
        Assert.InRange(origin.Value.Y, -0.5, window.ClientSize.Height + 0.5);
        Assert.True(origin.Value.X + control.Bounds.Width <= window.ClientSize.Width + 0.5, $"{control.Name} dépasse horizontalement.");
        Assert.True(origin.Value.Y + control.Bounds.Height <= window.ClientSize.Height + 0.5, $"{control.Name} dépasse verticalement.");
    }

    private static void AssertControlFitsInside(Control container, Control control)
    {
        Point? origin = control.TranslatePoint(default, container);
        Assert.NotNull(origin);
        Assert.InRange(origin.Value.X, -0.5, container.Bounds.Width + 0.5);
        Assert.InRange(origin.Value.Y, -0.5, container.Bounds.Height + 0.5);
        Assert.True(origin.Value.X + control.Bounds.Width <= container.Bounds.Width + 0.5,
            $"{control.Name} dépasse horizontalement de {container.Name}.");
        Assert.True(origin.Value.Y + control.Bounds.Height <= container.Bounds.Height + 0.5,
            $"{control.Name} dépasse verticalement de {container.Name}.");
    }

    private static void ApplyLanguage(MainWindow window, UiLanguage language)
    {
        ComboBox combo = window.FindControl<ComboBox>("LanguageCombo")!;
        int expectedIndex = language == UiLanguage.English ? 1 : 0;
        if (combo.SelectedIndex != expectedIndex)
        {
            combo.SelectedIndex = expectedIndex;
        }
        else
        {
            typeof(MainWindow)
                .GetMethod("LanguageCombo_SelectionChanged", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(window, [combo, null]);
        }
        Dispatcher.UIThread.RunJobs();
    }
}
