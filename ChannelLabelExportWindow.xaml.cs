using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using DanteConfigEditor.Models;
using DanteConfigEditor.Services;

namespace DanteConfigEditor;

public partial class ChannelLabelExportWindow : Window
{
    private readonly UiLanguage _language;
    private readonly DanteProject _project;
    private readonly ObservableCollection<ChannelLabelDeviceSelection> _devices;
    private readonly ObservableCollection<ChannelLabelExportPreviewRow> _preview = [];
    private bool _initializing = true;
    private bool _adaptationUserOverride;
    private int _collisionCount;

    public ChannelLabelExportWindow(UiLanguage language, DanteProject project, string? initiallySelectedDevice)
    {
        InitializeComponent();
        _language = language;
        _project = project;
        _devices = new ObservableCollection<ChannelLabelDeviceSelection>(project.Devices.Select(device =>
            new ChannelLabelDeviceSelection(device.Name, device.TxCount, device.RxCount,
                string.Equals(device.Name, initiallySelectedDevice, StringComparison.OrdinalIgnoreCase))));
        DevicesGrid.ItemsSource = _devices;
        PreviewGrid.ItemsSource = _preview;
        FormatComboBox.SelectedIndex = 1;
        DanteDevice? selectedDevice = project.Devices.FirstOrDefault(device =>
            string.Equals(device.Name, initiallySelectedDevice, StringComparison.OrdinalIgnoreCase));
        KindComboBox.SelectedIndex = selectedDevice is { TxCount: 0, RxCount: > 0 } ? 1 : 0;
        CaseModeComboBox.SelectedIndex = 0;
        ApplyLanguage();
        UpdateDeviceAvailability();
        _initializing = false;
        RefreshPreview();
    }

    public string Format { get; private set; } = "json";

    public IReadOnlyList<string> DeviceNames { get; private set; } = [];

    public DanteChannelKind Kind { get; private set; } = DanteChannelKind.Tx;

    public int StartChannel { get; private set; } = 1;

    public int? Count { get; private set; }

    public bool AdaptConsoleLabels { get; private set; }

    public ChannelLabelTransformOptions TransformOptions { get; private set; } =
        new(false, ChannelLabelCaseMode.Preserve, 0, 1, false);

    private bool IsEnglish => _language == UiLanguage.English;

    private void ApplyLanguage()
    {
        Title = L("Exporter des labels de canaux", "Export channel labels");
        IntroTextBlock.Text = L(
            "Sélectionnez les machines, les canaux TX/RX et le format. Les formats natifs sont créés directement depuis les modèles inclus dans DCE.",
            "Select devices, TX/RX channels and a format. Native files are created directly from the templates included in DCE.");
        DevicesGroupBox.Header = L("Machines à exporter", "Devices to export");
        UseDeviceColumn.Header = L("Utiliser", "Use");
        DeviceNameColumn.Header = L("Machine", "Device");
        OptionsGroupBox.Header = L("Options", "Options");
        FormatLabel.Content = L("Format", "Format");
        JsonFormatItem.Content = L("JSON générique - nouveau fichier", "Generic JSON - new file");
        CsvFormatItem.Content = L("CSV générique - nouveau fichier", "Generic CSV - new file");
        DmtDLiveFormatItem.Content = "DMT XLSX - dLive";
        DmtAvantisFormatItem.Content = "DMT XLSX - Avantis";
        DmtOdsDLiveFormatItem.Content = "DMT ODS - dLive";
        DmtOdsAvantisFormatItem.Content = "DMT ODS - Avantis";
        AllenHeathDLiveFormatItem.Content = L("A&H CSV natif - dLive", "Native A&H CSV - dLive");
        AllenHeathAvantisFormatItem.Content = L("A&H CSV natif - Avantis", "Native A&H CSV - Avantis");
        YamahaClFormatItem.Content = L("Yamaha ZIP natif - CL", "Native Yamaha ZIP - CL");
        YamahaQlFormatItem.Content = L("Yamaha ZIP natif - QL", "Native Yamaha ZIP - QL");
        KindLabel.Content = L("Canaux", "Channels");
        StartLabel.Content = L("Premier canal", "First channel");
        CountLabel.Content = L("Nombre (0 = tous)", "Count (0 = all)");
        MaximumLengthLabel.Content = L("Longueur maximale (0 = aucune)", "Maximum length (0 = none)");
        CaseModeLabel.Content = L("Casse", "Letter case");
        PreserveCaseItem.Content = L("Conserver", "Preserve");
        LowercaseItem.Content = L("Tout en minuscules", "Lowercase");
        UppercaseItem.Content = L("Tout en majuscules", "Uppercase");
        FirstUppercaseItem.Content = L("Première lettre en majuscule", "First letter uppercase");
        StartPositionLabel.Content = L("Commencer au caractère", "Start at character");
        FromEndCheckBox.Content = L("Compter depuis la fin", "Count from the end");
        AdaptDmtTextBlock.Text = L("Adapter au format console : ASCII compatible", "Adapt to console format: compatible ASCII");
        PreviewGroupBox.Header = L("Labels exportés", "Exported labels");
        PreviewDeviceColumn.Header = L("Machine", "Device");
        PreviewChannelColumn.Header = L("Canal", "Channel");
        PreviewOriginalColumn.Header = L("Label Dante", "Dante label");
        PreviewExportedColumn.Header = L("Label exporté", "Exported label");
        PreviewWarningColumn.Header = L("Contrôle", "Check");
        CancelButton.Content = L("Annuler", "Cancel");
        ExportButton.Content = L("Exporter", "Export");
        UpdateFormatHelp();
    }

    private void FormatComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_initializing)
        {
            return;
        }

        bool consoleTemplate = RequiresConsoleCompatibility(SelectedFormat());
        AdaptDmtCheckBox.IsEnabled = consoleTemplate;
        _adaptationUserOverride = false;
        AdaptDmtCheckBox.IsChecked = consoleTemplate;
        MaximumLengthTextBox.Text = consoleTemplate ? "8" : "0";
        UpdateFormatHelp();
        RefreshPreview();
    }

    private void CaseModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_initializing)
        {
            RefreshPreview();
        }
    }

    private void AdaptDmtCheckBox_Click(object sender, RoutedEventArgs e)
    {
        _adaptationUserOverride = true;
        RefreshPreview();
    }

    private void DeviceSelectionCheckBox_Click(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(RefreshPreview);
    }

    private void KindComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_initializing)
        {
            return;
        }

        UpdateDeviceAvailability();
        RefreshPreview();
    }

    private void UpdateDeviceAvailability()
    {
        if (!IsInitialized || KindComboBox is null)
        {
            return;
        }

        bool rx = KindComboBox.SelectedIndex == 1;
        foreach (ChannelLabelDeviceSelection device in _devices)
        {
            device.IsAvailable = rx ? device.RxCount > 0 : device.TxCount > 0;
            if (!device.IsAvailable)
            {
                device.IsSelected = false;
            }
        }
    }

    private void UpdateFormatHelp()
    {
        if (!IsInitialized || FormatHelpTextBlock is null)
        {
            return;
        }

        FormatHelpTextBlock.Text = RequiresConsoleCompatibility(SelectedFormat())
            ? L(
                "Le modèle dLive, Avantis, CL ou QL est inclus dans DCE. Seuls le nom et le dossier du nouveau fichier seront demandés.",
                "The dLive, Avantis, CL or QL template is included in DCE. Only the new file name and folder will be requested.")
            : L(
                "Le CSV générique sert aux échanges avec DCE ; il ne doit pas être importé directement dans une console.",
                "Generic CSV is intended for DCE exchanges; do not import it directly into a console.");
    }

    private void OptionsChanged(object sender, RoutedEventArgs e) => RefreshPreview();

    private void DevicesGrid_CurrentCellChanged(object? sender, EventArgs e) => RefreshPreview();

    private void RefreshPreview()
    {
        if (_initializing || !IsInitialized)
        {
            return;
        }

        DevicesGrid.CommitEdit(DataGridEditingUnit.Cell, true);
        DevicesGrid.CommitEdit(DataGridEditingUnit.Row, true);
        _preview.Clear();
        string[] selected = _devices.Where(device => device.IsSelected && device.IsAvailable).Select(device => device.Name).ToArray();
        if (selected.Length == 0)
        {
            SummaryTextBlock.Text = L(
                "Sélectionnez au moins une machine disposant de canaux dans le sens TX/RX choisi.",
                "Select at least one device with channels in the chosen TX/RX direction.");
            ExportButton.IsEnabled = false;
            return;
        }

        int start = int.TryParse(StartTextBox.Text, out int parsedStart) && parsedStart > 0 ? parsedStart : 1;
        int? count = int.TryParse(CountTextBox.Text, out int parsedCount) && parsedCount > 0 ? parsedCount : null;
        DanteChannelKind kind = KindComboBox.SelectedIndex == 1 ? DanteChannelKind.Rx : DanteChannelKind.Tx;
        bool consoleTemplate = RequiresConsoleCompatibility(SelectedFormat());
        ChannelLabelDocument document;
        try
        {
            document = ChannelLabelExchangeService.CreateFromProject(_project, selected, kind, start, count);
        }
        catch (InvalidOperationException)
        {
            SummaryTextBlock.Text = L(
                "Aucun canal TX/RX ne correspond aux machines et à la plage sélectionnées.",
                "No TX/RX channel matches the selected devices and range.");
            ExportButton.IsEnabled = false;
            return;
        }
        bool requiresAdaptation = consoleTemplate && document.Sets
            .SelectMany(set => set.Channels)
            .Any(channel => !DmtChannelWorkbookService.CheckCompatibility(channel.Label).IsCompatible);
        if (consoleTemplate && !_adaptationUserOverride)
        {
            AdaptDmtCheckBox.IsChecked = requiresAdaptation;
        }

        ChannelLabelTransformOptions transformOptions;
        try
        {
            transformOptions = ReadTransformOptions();
        }
        catch (InvalidOperationException ex)
        {
            SummaryTextBlock.Text = ex.Message;
            ExportButton.IsEnabled = false;
            return;
        }

        bool adapt = transformOptions.AsciiOnly;
        int incompatible = 0;
        _collisionCount = 0;
        foreach (ChannelLabelSet set in document.Sets)
        {
            ChannelLabelTransformResult transformedSet = ChannelLabelTransformService.Transform(set, transformOptions);
            _collisionCount += transformedSet.Collisions.Count;
            Dictionary<string, ChannelLabelCollision> collisions = transformedSet.Collisions
                .ToDictionary(collision => collision.Label, StringComparer.OrdinalIgnoreCase);
            foreach ((ChannelLabelEntry channel, ChannelLabelEntry transformed) in set.Channels.Zip(transformedSet.Labels.Channels))
            {
                DmtLabelCompatibility compatibility = DmtChannelWorkbookService.CheckCompatibility(transformed.Label);
                List<string> warnings = [];
                if (consoleTemplate && !compatibility.IsCompatible && !adapt)
                {
                    warnings.AddRange(compatibility.Warnings.Select(warning =>
                        LocalizationService.TranslateLiteral(_language, warning)));
                }
                if (collisions.TryGetValue(transformed.Label, out ChannelLabelCollision? collision))
                {
                    warnings.Add(L(
                        $"Doublon après transformation : canaux {string.Join(", ", collision.Channels)}",
                        $"Duplicate after transformation: channels {string.Join(", ", collision.Channels)}"));
                }
                if (consoleTemplate && !compatibility.IsCompatible && !adapt)
                {
                    incompatible++;
                }

                _preview.Add(new ChannelLabelExportPreviewRow(
                    set.DeviceName,
                    kind,
                    channel.ChannelNumber,
                    channel.Label,
                    transformed.Label,
                    string.Join("; ", warnings)));
            }
        }

        if (_preview.Count == 0)
        {
            SummaryTextBlock.Text = L(
                "Aucun canal ne correspond à la plage demandée.",
                "No channel matches the requested range.");
            ExportButton.IsEnabled = false;
            return;
        }

        SummaryTextBlock.Text = IsEnglish
            ? $"{document.Sets.Count} device(s), {_preview.Count} label(s), {incompatible} console-incompatible label(s), {_collisionCount} collision(s)."
            : $"{document.Sets.Count} machine(s), {_preview.Count} label(s), {incompatible} label(s) incompatible(s) avec la console, {_collisionCount} collision(s).";
        ExportButton.IsEnabled = true;
    }

    private void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            RefreshPreview();
            DeviceNames = _devices.Where(device => device.IsSelected && device.IsAvailable).Select(device => device.Name).ToArray();
            if (DeviceNames.Count == 0)
            {
                throw new InvalidOperationException(L("Sélectionnez au moins une machine.", "Select at least one device."));
            }
            if (_preview.Count == 0)
            {
                throw new InvalidOperationException(SummaryTextBlock.Text);
            }

            Format = SelectedFormat();
            Kind = KindComboBox.SelectedIndex == 1 ? DanteChannelKind.Rx : DanteChannelKind.Tx;
            StartChannel = LocalizedNumberParser.ParsePositive(
                StartTextBox.Text,
                L("premier canal", "first channel"),
                _language);
            Count = LocalizedNumberParser.ParseOptionalCount(CountTextBox.Text, _language);
            AdaptConsoleLabels = AdaptDmtCheckBox.IsChecked == true;
            TransformOptions = ReadTransformOptions();
            if (RequiresConsoleCompatibility(Format) && !AdaptConsoleLabels && _preview.Any(row => !string.IsNullOrWhiteSpace(row.Warning)))
            {
                throw new InvalidOperationException(L(
                    "Certains labels ne sont pas compatibles avec le format console. Activez l'adaptation explicite ou choisissez JSON/CSV générique.",
                    "Some labels are not console-compatible. Enable explicit adaptation or choose generic JSON/CSV."));
            }
            if (_collisionCount > 0)
            {
                throw new InvalidOperationException(L(
                    "La transformation crée des labels en doublon. Modifiez la casse, la position de départ ou la longueur avant l'export.",
                    "The transformation creates duplicate labels. Change the letter case, start position, or length before exporting."));
            }

            DialogResult = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, L("Export impossible", "Export unavailable"), MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private string SelectedFormat() => (FormatComboBox.SelectedItem as ComboBoxItem)?.Tag as string ?? "json";

    private static bool RequiresConsoleCompatibility(string format) => BuiltInChannelLabelTemplateService.IsBuiltInFormat(format);

    private ChannelLabelTransformOptions ReadTransformOptions()
    {
        int maximumLength = int.TryParse(MaximumLengthTextBox.Text.Trim(), out int parsedLength) && parsedLength >= 0
            ? parsedLength
            : throw new InvalidOperationException(L("Longueur maximale invalide.", "Invalid maximum length."));
        int startPosition = int.TryParse(StartPositionTextBox.Text.Trim(), out int parsedStart) && parsedStart > 0
            ? parsedStart
            : throw new InvalidOperationException(L("Position de départ invalide.", "Invalid start position."));
        ChannelLabelCaseMode caseMode = CaseModeComboBox.SelectedIndex switch
        {
            1 => ChannelLabelCaseMode.Lowercase,
            2 => ChannelLabelCaseMode.Uppercase,
            3 => ChannelLabelCaseMode.FirstLetterUppercase,
            _ => ChannelLabelCaseMode.Preserve
        };
        return new ChannelLabelTransformOptions(
            AdaptDmtCheckBox.IsChecked == true,
            caseMode,
            maximumLength,
            startPosition,
            FromEndCheckBox.IsChecked == true);
    }

    private string L(string french, string english) => IsEnglish ? english : french;
}

public sealed record ChannelLabelExportPreviewRow(
    string DeviceName,
    DanteChannelKind Kind,
    int ChannelNumber,
    string OriginalLabel,
    string ExportedLabel,
    string Warning);
