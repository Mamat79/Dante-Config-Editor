using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using DanteConfigEditor.Models;
using DanteConfigEditor.Services;

namespace DanteConfigEditor;

public partial class ChannelLabelImportWindow : Window
{
    private readonly UiLanguage _language;
    private readonly DanteProject _project;
    private readonly ChannelLabelDocument _document;
    private readonly ChannelLabelImportReport _importReport;
    private readonly ObservableCollection<ChannelLabelTransferPreviewRow> _previewRows = [];
    private readonly ObservableCollection<ChannelLabelDeviceSelection> _targetDevices;

    public ChannelLabelImportWindow(
        UiLanguage language,
        DanteProject project,
        ChannelLabelDocument document,
        ChannelLabelImportReport importReport,
        string? initiallySelectedDevice)
    {
        InitializeComponent();
        _language = language;
        _project = project;
        _document = document;
        _importReport = importReport;
        _targetDevices = new ObservableCollection<ChannelLabelDeviceSelection>(project.Devices.Select(device =>
            new ChannelLabelDeviceSelection(
                device.Name,
                device.TxCount,
                device.RxCount,
                string.Equals(device.Name, initiallySelectedDevice, StringComparison.OrdinalIgnoreCase)
                    || string.IsNullOrWhiteSpace(initiallySelectedDevice) && project.Devices.Count == 1)));

        SourceSetComboBox.ItemsSource = document.Sets;
        SourceSetComboBox.SelectedIndex = 0;
        TargetDevicesGrid.ItemsSource = _targetDevices;
        PreviewGrid.ItemsSource = _previewRows;
        bool canMatchByName = document.Sets.Any(set => !string.IsNullOrWhiteSpace(set.DeviceName));
        AutoMatchCheckBox.IsEnabled = canMatchByName;
        AutoMatchCheckBox.IsChecked = document.Sets.Count > 1
            && document.Sets.All(set => project.Devices.Any(device =>
                string.Equals(device.Name, set.DeviceName, StringComparison.OrdinalIgnoreCase)));
        TargetKindComboBox.SelectedIndex = document.Sets[0].Direction == ChannelLabelDirection.Tx ? 0 : 1;
        SourceInfoTextBlock.Text = BuildSourceInfo();
        ApplyLanguage();
        UpdateRangeDefaults();
    }

    public IReadOnlyList<ChannelLabelAssignment> Assignments { get; private set; } = [];

    private bool IsEnglish => _language == UiLanguage.English;

    private string BuildSourceInfo()
    {
        string source = $"{_document.SourceApplication} {_document.SourceVersion}".Trim();
        return $"{source} - {_importReport.AdapterName}{Environment.NewLine}{_importReport.ToDisplayText(IsEnglish)}";
    }

    private void ApplyLanguage()
    {
        Title = L("Importer des labels de canaux", "Import channel labels");
        IntroTextBlock.Text = L(
            "Cochez une ou plusieurs machines, réglez la plage, puis cliquez sur Prévisualiser. Aucun XML n'est modifié avant Appliquer.",
            "Select one or more devices, set the range, then click Preview. No XML is changed before Apply.");
        SourceGroupBox.Header = L("Source", "Source");
        SourceSetLabel.Content = L("Liste de labels", "Label set");
        AutoMatchCheckBox.Content = L("Associer automatiquement les listes aux machines de même nom", "Automatically match sets to devices with the same name");
        TargetsGroupBox.Header = L("Machines Dante cibles", "Target Dante devices");
        UseTargetColumn.Header = L("Utiliser", "Use");
        TargetNameColumn.Header = L("Machine", "Device");
        MappingGroupBox.Header = L("Correspondance", "Mapping");
        TargetKindLabel.Content = L("Type de canal Dante", "Dante channel type");
        SourceStartLabel.Content = L("Premier canal source", "First source channel");
        TargetStartLabel.Content = L("Premier canal Dante", "First Dante channel");
        CountLabel.Content = L("Nombre de canaux", "Channel count");
        PreviewButton.Content = L("Prévisualiser", "Preview");
        PreviewGroupBox.Header = L("Prévisualisation", "Preview");
        PreviewSourceDeviceColumn.Header = L("Source", "Source");
        PreviewSourceChannelColumn.Header = L("Canal source", "Source channel");
        PreviewTargetDeviceColumn.Header = L("Machine Dante", "Dante device");
        PreviewTargetChannelColumn.Header = L("Canal Dante", "Dante channel");
        PreviewBeforeColumn.Header = L("Avant", "Before");
        PreviewAfterColumn.Header = L("Après", "After");
        PreviewStatusColumn.Header = L("État", "Status");
        PreviewStatusColumn.Binding = new Binding(IsEnglish
            ? nameof(ChannelLabelTransferPreviewRow.StatusDisplayEnglish)
            : nameof(ChannelLabelTransferPreviewRow.StatusDisplay));
        SafetyTextBlock.Text = L(
            "Les renommages TX mettront à jour les subscriptions reconnues.",
            "Tx renames will update recognized subscriptions.");
        CancelButton.Content = L("Annuler", "Cancel");
        ApplyButton.Content = L("Appliquer", "Apply");
    }

    private void SourceSetComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SourceSetComboBox.SelectedItem is ChannelLabelSet set)
        {
            TargetKindComboBox.SelectedIndex = set.Direction == ChannelLabelDirection.Tx ? 0 : 1;
            UpdateRangeDefaults();
        }
    }

    private void AutoMatchCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        bool automatic = AutoMatchCheckBox.IsChecked == true;
        SourceSetComboBox.IsEnabled = !automatic;
        TargetDevicesGrid.IsEnabled = !automatic;
        ClearPreview();
    }

    private void TargetDeviceCheckBox_Click(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(() =>
        {
            UpdateSuggestedCount();
            ClearPreview();
        });
    }

    private void TargetKindComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsInitialized)
        {
            return;
        }

        UpdateSuggestedCount();
        ClearPreview();
    }

    private void UpdateRangeDefaults()
    {
        if (SourceSetComboBox.SelectedItem is not ChannelLabelSet set || set.Channels.Count == 0)
        {
            return;
        }

        SourceStartTextBox.Text = set.Channels.Min(channel => channel.ChannelNumber).ToString();
        TargetStartTextBox.Text = "1";
        UpdateSuggestedCount();
        ClearPreview();
    }

    private void UpdateSuggestedCount()
    {
        if (SourceSetComboBox.SelectedItem is not ChannelLabelSet set || set.Channels.Count == 0)
        {
            return;
        }

        int suggested = set.Channels.Count;
        if (AutoMatchCheckBox.IsChecked != true)
        {
            int targetStart = int.TryParse(TargetStartTextBox.Text, out int parsedStart) && parsedStart > 0 ? parsedStart : 1;
            bool tx = TargetKindComboBox.SelectedIndex == 0;
            int[] capacities = _targetDevices
                .Where(device => device.IsSelected)
                .Select(device => tx ? device.TxCount : device.RxCount)
                .ToArray();
            if (capacities.Length > 0)
            {
                int available = Math.Max(1, capacities.Min() - targetStart + 1);
                suggested = Math.Min(suggested, available);
            }
        }

        CountTextBox.Text = Math.Max(1, suggested).ToString();
    }

    private void PreviewButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            TargetDevicesGrid.CommitEdit(DataGridEditingUnit.Cell, true);
            TargetDevicesGrid.CommitEdit(DataGridEditingUnit.Row, true);
            int sourceStart = LocalizedNumberParser.ParsePositive(
                SourceStartTextBox.Text,
                L("premier canal source", "first source channel"),
                _language);
            int targetStart = LocalizedNumberParser.ParsePositive(
                TargetStartTextBox.Text,
                L("premier canal Dante", "first Dante channel"),
                _language);
            int count = LocalizedNumberParser.ParsePositive(
                CountTextBox.Text,
                L("nombre de canaux", "channel count"),
                _language);
            DanteChannelKind selectedKind = TargetKindComboBox.SelectedIndex == 0 ? DanteChannelKind.Tx : DanteChannelKind.Rx;
            List<ChannelLabelTransferPreviewRow> rows = [];

            if (AutoMatchCheckBox.IsChecked == true)
            {
                foreach (ChannelLabelSet set in _document.Sets)
                {
                    DanteChannelKind kind = set.Direction switch
                    {
                        ChannelLabelDirection.Tx => DanteChannelKind.Tx,
                        ChannelLabelDirection.Rx => DanteChannelKind.Rx,
                        _ => selectedKind
                    };
                    rows.AddRange(ChannelLabelTransferPlanner.BuildPreview(
                        _project,
                        set,
                        [set.DeviceName],
                        kind,
                        sourceStart,
                        targetStart,
                        Math.Min(count, set.Channels.Count)));
                }
            }
            else
            {
                ChannelLabelSet source = SourceSetComboBox.SelectedItem as ChannelLabelSet
                    ?? throw new InvalidOperationException(L("Sélectionnez une liste source.", "Select a source set."));
                string[] targets = _targetDevices.Where(device => device.IsSelected).Select(device => device.Name).ToArray();
                rows.AddRange(ChannelLabelTransferPlanner.BuildPreview(
                    _project,
                    source,
                    targets,
                    selectedKind,
                    sourceStart,
                    targetStart,
                    count));
            }

            _previewRows.Clear();
            foreach (ChannelLabelTransferPreviewRow row in rows)
            {
                _previewRows.Add(row);
            }

            int changes = rows.Count(row => row.WillChange);
            int errors = rows.Count(row => !row.CanApply);
            int unchanged = rows.Count(row => row.Status == ChannelLabelTransferStatus.Unchanged);
            PreviewSummaryTextBlock.Text = IsEnglish
                ? $"{rows.Count} row(s): {changes} change(s), {unchanged} unchanged, {errors} issue(s)."
                : $"{rows.Count} ligne(s) : {changes} changement(s), {unchanged} inchangée(s), {errors} problème(s).";
            ApplyButton.IsEnabled = changes > 0 && errors == 0;
        }
        catch (Exception ex)
        {
            ClearPreview();
            MessageBox.Show(this, ex.Message, L("Prévisualisation impossible", "Preview unavailable"), MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ApplyButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Assignments = ChannelLabelTransferPlanner.BuildAssignments(_previewRows);
            if (Assignments.Count == 0)
            {
                throw new InvalidOperationException(L("Aucun changement à appliquer.", "No change to apply."));
            }

            DialogResult = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, L("Import impossible", "Import unavailable"), MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ClearPreview()
    {
        _previewRows.Clear();
        PreviewSummaryTextBlock.Text = L(
            "Prévisualisez le transfert pour vérifier chaque correspondance.",
            "Preview the transfer to verify each mapping.");
        ApplyButton.IsEnabled = false;
    }

    private string L(string french, string english) => IsEnglish ? english : french;
}

public sealed class ChannelLabelDeviceSelection : INotifyPropertyChanged
{
    private bool _isSelected;
    private bool _isAvailable = true;

    public ChannelLabelDeviceSelection(string name, int txCount, int rxCount, bool isSelected)
    {
        Name = name;
        TxCount = txCount;
        RxCount = rxCount;
        _isSelected = isSelected;
    }

    public string Name { get; }

    public int TxCount { get; }

    public int RxCount { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value)
            {
                return;
            }

            _isSelected = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
        }
    }

    public bool IsAvailable
    {
        get => _isAvailable;
        set
        {
            if (_isAvailable == value)
            {
                return;
            }

            _isAvailable = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsAvailable)));
        }
    }

    public string Counts => $"{TxCount} / {RxCount}";

    public event PropertyChangedEventHandler? PropertyChanged;
}
