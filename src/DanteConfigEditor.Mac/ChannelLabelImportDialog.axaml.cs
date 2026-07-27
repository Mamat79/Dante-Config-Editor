using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using DanteConfigEditor.Models;
using DanteConfigEditor.Services;

namespace DanteConfigEditor.Mac;

internal sealed partial class ChannelLabelImportDialog : Window
{
    private readonly ObservableCollection<MacChannelLabelDeviceSelection> _targetDevices = [];
    private readonly ObservableCollection<MacChannelLabelPreviewRow> _previewRows = [];
    private DanteProject? _project;
    private ChannelLabelDocument? _document;
    private ChannelLabelImportReport? _importReport;
    private UiLanguage _language;

    public ChannelLabelImportDialog()
    {
        InitializeComponent();
    }

    private T? FindControl<T>(string name) where T : Control => ControlExtensions.FindControl<T>(this, name);

    public static Task<IReadOnlyList<ChannelLabelAssignment>?> ShowAsync(
        Window owner,
        DanteProject project,
        ChannelLabelDocument document,
        ChannelLabelImportReport importReport,
        UiLanguage language,
        string? initiallySelectedDevice)
    {
        ChannelLabelImportDialog dialog = new();
        dialog._project = project;
        dialog._document = document;
        dialog._importReport = importReport;
        dialog._language = language;
        dialog.Populate(initiallySelectedDevice);
        dialog.ApplyLanguage();
        return dialog.ShowDialog<IReadOnlyList<ChannelLabelAssignment>?>(owner);
    }

    private void Populate(string? initiallySelectedDevice)
    {
        ChannelLabelDocument document = _document!;
        foreach (DanteDevice device in _project!.Devices)
        {
            _targetDevices.Add(new MacChannelLabelDeviceSelection(
                device.Name,
                device.TxCount,
                device.RxCount,
                string.Equals(device.Name, initiallySelectedDevice, StringComparison.OrdinalIgnoreCase)
                    || string.IsNullOrWhiteSpace(initiallySelectedDevice) && _project.Devices.Count == 1));
        }

        FindControl<DataGrid>("TargetDevicesGrid")!.ItemsSource = _targetDevices;
        FindControl<DataGrid>("PreviewGrid")!.ItemsSource = _previewRows;
        FindControl<ComboBox>("SourceSetCombo")!.ItemsSource = document.Sets.Select(set => new MacChannelLabelSetChoice(set)).ToArray();
        FindControl<ComboBox>("SourceSetCombo")!.SelectedIndex = 0;
        FindControl<ComboBox>("KindCombo")!.ItemsSource = new ChoiceValue[]
        {
            new ChoiceValue("tx", "TX"),
            new ChoiceValue("rx", "RX")
        };
        FindControl<ComboBox>("KindCombo")!.SelectedIndex = document.Sets[0].Direction == ChannelLabelDirection.Rx ? 1 : 0;
        CheckBox autoMatch = FindControl<CheckBox>("AutoMatchCheckBox")!;
        autoMatch.IsEnabled = document.Sets.Any(set => !string.IsNullOrWhiteSpace(set.DeviceName));
        autoMatch.IsChecked = document.Sets.Count > 1
            && document.Sets.All(set => _project.Devices.Any(device =>
                string.Equals(device.Name, set.DeviceName, StringComparison.OrdinalIgnoreCase)));
        string source = $"{document.SourceApplication} {document.SourceVersion}".Trim();
        FindControl<TextBlock>("SourceInfoText")!.Text =
            $"{source} - {_importReport!.AdapterName}{Environment.NewLine}{_importReport.ToDisplayText(_language == UiLanguage.English)}";
        UpdateRangeDefaults();
        UpdateAutoMatchState();
    }

    private void ApplyLanguage()
    {
        Title = Local("Importer des labels de canaux", "Import channel labels");
        FindControl<TextBlock>("IntroText")!.Text = Local(
            "Cochez une ou plusieurs machines, réglez la plage, puis cliquez sur Prévisualiser. Aucun XML n'est modifié avant Appliquer.",
            "Select one or more devices, set the range, then click Preview. No XML is changed before Apply.");
        FindControl<TextBlock>("SourceTitle")!.Text = Local("Source", "Source");
        FindControl<TextBlock>("SourceSetLabel")!.Text = Local("Liste de labels", "Label set");
        FindControl<CheckBox>("AutoMatchCheckBox")!.Content = Local(
            "Associer les listes aux machines de même nom",
            "Match sets to devices with the same name");
        FindControl<TextBlock>("TargetsTitle")!.Text = Local("Machines Dante cibles", "Target Dante devices");
        FindControl<DataGrid>("TargetDevicesGrid")!.Columns[0].Header = Local("Utiliser", "Use");
        FindControl<DataGrid>("TargetDevicesGrid")!.Columns[1].Header = Local("Machine", "Device");
        FindControl<TextBlock>("MappingTitle")!.Text = Local("Correspondance", "Mapping");
        FindControl<TextBlock>("KindLabel")!.Text = Local("Type de canal Dante", "Dante channel type");
        FindControl<TextBlock>("SourceStartLabel")!.Text = Local("Premier canal source", "First source channel");
        FindControl<TextBlock>("TargetStartLabel")!.Text = Local("Premier canal Dante", "First Dante channel");
        FindControl<TextBlock>("CountLabel")!.Text = Local("Nombre de canaux", "Channel count");
        FindControl<Button>("PreviewButton")!.Content = Local("Prévisualiser", "Preview");
        FindControl<TextBlock>("PreviewTitle")!.Text = Local("Prévisualisation", "Preview");
        DataGrid preview = FindControl<DataGrid>("PreviewGrid")!;
        preview.Columns[0].Header = Local("Source", "Source");
        preview.Columns[1].Header = Local("Canal source", "Source channel");
        preview.Columns[2].Header = Local("Machine Dante", "Dante device");
        preview.Columns[3].Header = Local("Canal Dante", "Dante channel");
        preview.Columns[4].Header = Local("Avant", "Before");
        preview.Columns[5].Header = Local("Après", "After");
        preview.Columns[6].Header = Local("État", "Status");
        FindControl<TextBlock>("SafetyText")!.Text = DefaultSafetyText();
        FindControl<Button>("CancelButton")!.Content = Local("Annuler", "Cancel");
        FindControl<Button>("ApplyButton")!.Content = Local("Appliquer", "Apply");
        ClearPreview();
    }

    private void SourceSetCombo_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (FindControl<ComboBox>("SourceSetCombo")?.SelectedItem is MacChannelLabelSetChoice choice)
        {
            FindControl<ComboBox>("KindCombo")!.SelectedIndex = choice.Set.Direction == ChannelLabelDirection.Rx ? 1 : 0;
            UpdateRangeDefaults();
        }
    }

    private void AutoMatchCheckBox_Changed(object? sender, RoutedEventArgs e)
    {
        UpdateAutoMatchState();
        ClearPreview();
    }

    private void TargetDeviceCheckBox_Click(object? sender, RoutedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            UpdateSuggestedCount();
            ClearPreview();
        });
    }

    private void KindCombo_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_project is null)
        {
            return;
        }
        UpdateSuggestedCount();
        ClearPreview();
    }

    private void UpdateAutoMatchState()
    {
        bool automatic = FindControl<CheckBox>("AutoMatchCheckBox")?.IsChecked == true;
        FindControl<ComboBox>("SourceSetCombo")!.IsEnabled = !automatic;
        FindControl<DataGrid>("TargetDevicesGrid")!.IsEnabled = !automatic;
    }

    private void UpdateRangeDefaults()
    {
        if (FindControl<ComboBox>("SourceSetCombo")?.SelectedItem is not MacChannelLabelSetChoice choice
            || choice.Set.Channels.Count == 0)
        {
            return;
        }

        FindControl<TextBox>("SourceStartTextBox")!.Text = choice.Set.Channels.Min(channel => channel.ChannelNumber).ToString();
        FindControl<TextBox>("TargetStartTextBox")!.Text = "1";
        UpdateSuggestedCount();
        ClearPreview();
    }

    private void UpdateSuggestedCount()
    {
        if (FindControl<ComboBox>("SourceSetCombo")?.SelectedItem is not MacChannelLabelSetChoice choice
            || choice.Set.Channels.Count == 0)
        {
            return;
        }

        int suggested = choice.Set.Channels.Count;
        if (FindControl<CheckBox>("AutoMatchCheckBox")?.IsChecked != true)
        {
            int targetStart = int.TryParse(FindControl<TextBox>("TargetStartTextBox")?.Text, out int parsedStart) && parsedStart > 0 ? parsedStart : 1;
            bool tx = SelectedValue("KindCombo") != "rx";
            int[] capacities = _targetDevices
                .Where(device => device.IsSelected)
                .Select(device => tx ? device.TxCount : device.RxCount)
                .ToArray();
            if (capacities.Length > 0)
            {
                suggested = Math.Min(suggested, Math.Max(1, capacities.Min() - targetStart + 1));
            }
        }

        FindControl<TextBox>("CountTextBox")!.Text = Math.Max(1, suggested).ToString();
    }

    private async void PreviewButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            IReadOnlyList<ChannelLabelTransferPreviewRow> rows = BuildPreview();
            _previewRows.Clear();
            foreach (ChannelLabelTransferPreviewRow row in rows)
            {
                _previewRows.Add(new MacChannelLabelPreviewRow(row, _language));
            }

            int changes = rows.Count(row => row.WillChange);
            int errors = rows.Count(row => !row.CanApply);
            int unchanged = rows.Count(row => row.Status == ChannelLabelTransferStatus.Unchanged);
            FindControl<TextBlock>("PreviewSummaryText")!.Text = _language == UiLanguage.English
                ? $"{rows.Count} row(s): {changes} change(s), {unchanged} unchanged, {errors} issue(s)."
                : $"{rows.Count} ligne(s) : {changes} changement(s), {unchanged} inchangée(s), {errors} problème(s).";
            FindControl<Button>("ApplyButton")!.IsEnabled = changes > 0 && errors == 0;
            FindControl<TextBlock>("SafetyText")!.Text = errors > 0
                ? Local(
                    "Appliquer est indisponible : corrigez d'abord les problèmes affichés dans l'aperçu.",
                    "Apply is unavailable: fix the issues shown in the preview first.")
                : changes == 0
                    ? Local(
                        "Rien à appliquer : ces labels correspondent déjà aux canaux Dante sélectionnés.",
                        "Nothing to apply: these labels already match the selected Dante channels.")
                    : DefaultSafetyText();
        }
        catch (Exception exception)
        {
            ClearPreview();
            await MessageDialog.ShowInfoAsync(this, Local("Prévisualisation impossible", "Preview unavailable"), exception.Message, "OK");
        }
    }

    private IReadOnlyList<ChannelLabelTransferPreviewRow> BuildPreview()
    {
        int sourceStart = LocalizedNumberParser.ParsePositive(
            FindControl<TextBox>("SourceStartTextBox")!.Text,
            Local("premier canal source", "first source channel"),
            _language);
        int targetStart = LocalizedNumberParser.ParsePositive(
            FindControl<TextBox>("TargetStartTextBox")!.Text,
            Local("premier canal Dante", "first Dante channel"),
            _language);
        int count = LocalizedNumberParser.ParsePositive(
            FindControl<TextBox>("CountTextBox")!.Text,
            Local("nombre de canaux", "channel count"),
            _language);
        DanteChannelKind selectedKind = SelectedValue("KindCombo") == "rx" ? DanteChannelKind.Rx : DanteChannelKind.Tx;
        List<ChannelLabelTransferPreviewRow> rows = [];

        if (FindControl<CheckBox>("AutoMatchCheckBox")!.IsChecked == true)
        {
            foreach (ChannelLabelSet set in _document!.Sets)
            {
                DanteChannelKind kind = set.Direction switch
                {
                    ChannelLabelDirection.Tx => DanteChannelKind.Tx,
                    ChannelLabelDirection.Rx => DanteChannelKind.Rx,
                    _ => selectedKind
                };
                rows.AddRange(ChannelLabelTransferPlanner.BuildPreview(
                    _project!, set, [set.DeviceName], kind, sourceStart, targetStart, Math.Min(count, set.Channels.Count)));
            }
        }
        else
        {
            ChannelLabelSet source = (FindControl<ComboBox>("SourceSetCombo")!.SelectedItem as MacChannelLabelSetChoice)?.Set
                ?? throw new InvalidOperationException(Local("Sélectionnez une liste source.", "Select a source set."));
            string[] targets = _targetDevices.Where(device => device.IsSelected).Select(device => device.Name).ToArray();
            rows.AddRange(ChannelLabelTransferPlanner.BuildPreview(
                _project!, source, targets, selectedKind, sourceStart, targetStart, count));
        }

        return rows;
    }

    private async void ApplyButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            IReadOnlyList<ChannelLabelAssignment> assignments = ChannelLabelTransferPlanner.BuildAssignments(_previewRows.Select(row => row.Source));
            if (assignments.Count == 0)
            {
                throw new InvalidOperationException(Local("Aucun changement à appliquer.", "No change to apply."));
            }
            Close(assignments);
        }
        catch (Exception exception)
        {
            await MessageDialog.ShowInfoAsync(this, Local("Import impossible", "Import unavailable"), exception.Message, "OK");
        }
    }

    private void CancelButton_Click(object? sender, RoutedEventArgs e) => Close(null);

    private void ClearPreview()
    {
        _previewRows.Clear();
        if (FindControl<TextBlock>("PreviewSummaryText") is { } summary)
        {
            summary.Text = Local(
                "Prévisualisez le transfert pour vérifier chaque correspondance.",
                "Preview the transfer to verify each mapping.");
        }
        if (FindControl<Button>("ApplyButton") is { } apply)
        {
            apply.IsEnabled = false;
        }
        if (FindControl<TextBlock>("SafetyText") is { } safety)
        {
            safety.Text = DefaultSafetyText();
        }
    }

    private string DefaultSafetyText() => Local(
        "Les renommages TX mettent à jour les subscriptions reconnues.",
        "TX renames update recognized subscriptions.");

    private string SelectedValue(string controlName) =>
        (FindControl<ComboBox>(controlName)!.SelectedItem as ChoiceValue)?.Value ?? string.Empty;

    private string Local(string french, string english) => _language == UiLanguage.English ? english : french;
}

internal sealed record MacChannelLabelSetChoice(ChannelLabelSet Set)
{
    public override string ToString() => Set.DisplayName;
}

internal sealed class MacChannelLabelDeviceSelection : INotifyPropertyChanged
{
    private bool _isSelected;
    private bool _isAvailable = true;

    public MacChannelLabelDeviceSelection(string name, int txCount, int rxCount, bool isSelected)
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

internal sealed record MacChannelLabelPreviewRow(ChannelLabelTransferPreviewRow Source, UiLanguage Language)
{
    public string SourceDevice => Source.SourceDevice;
    public int SourceChannel => Source.SourceChannel;
    public string TargetDevice => Source.TargetDevice;
    public int TargetDanteId => Source.TargetDanteId;
    public string CurrentLabel => Source.CurrentLabel;
    public string NewLabel => Source.NewLabel;
    public string LocalizedStatus => Language == UiLanguage.English ? Source.StatusDisplayEnglish : Source.StatusDisplay;
}
