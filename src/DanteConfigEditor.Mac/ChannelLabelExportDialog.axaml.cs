using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using DanteConfigEditor.Models;
using DanteConfigEditor.Services;

namespace DanteConfigEditor.Mac;

internal sealed record ChannelLabelExportDialogResult(
    string Format,
    IReadOnlyList<string> DeviceNames,
    DanteChannelKind Kind,
    int StartChannel,
    int? Count,
    bool AdaptConsoleLabels,
    ChannelLabelTransformOptions TransformOptions);

internal sealed partial class ChannelLabelExportDialog : Window
{
    private readonly ObservableCollection<MacChannelLabelDeviceSelection> _devices = [];
    private readonly ObservableCollection<MacChannelLabelExportPreviewRow> _preview = [];
    private DanteProject? _project;
    private UiLanguage _language;
    private bool _initializing = true;
    private bool _adaptationUserOverride;
    private int _collisionCount;

    public ChannelLabelExportDialog()
    {
        InitializeComponent();
    }

    private T? FindControl<T>(string name) where T : Control => ControlExtensions.FindControl<T>(this, name);

    public static Task<ChannelLabelExportDialogResult?> ShowAsync(
        Window owner,
        DanteProject project,
        UiLanguage language,
        string? initiallySelectedDevice)
    {
        ChannelLabelExportDialog dialog = new();
        dialog._project = project;
        dialog._language = language;
        dialog.Populate(initiallySelectedDevice);
        dialog.ApplyLanguage();
        dialog._initializing = false;
        dialog.RefreshPreview();
        return dialog.ShowDialog<ChannelLabelExportDialogResult?>(owner);
    }

    private void Populate(string? initiallySelectedDevice)
    {
        foreach (DanteDevice device in _project!.Devices)
        {
            _devices.Add(new MacChannelLabelDeviceSelection(
                device.Name,
                device.TxCount,
                device.RxCount,
                string.Equals(device.Name, initiallySelectedDevice, StringComparison.OrdinalIgnoreCase)));
        }
        FindControl<DataGrid>("DevicesGrid")!.ItemsSource = _devices;
        FindControl<DataGrid>("PreviewGrid")!.ItemsSource = _preview;
        FindControl<ComboBox>("FormatCombo")!.ItemsSource = new ChoiceValue[]
        {
            new ChoiceValue("json", Local("JSON générique - nouveau fichier", "Generic JSON - new file")),
            new ChoiceValue("csv", Local("CSV générique - nouveau fichier", "Generic CSV - new file")),
            new ChoiceValue("dmt-dlive", "DMT XLSX - dLive"),
            new ChoiceValue("dmt-avantis", "DMT XLSX - Avantis"),
            new ChoiceValue("dmt-ods-dlive", "DMT ODS - dLive"),
            new ChoiceValue("dmt-ods-avantis", "DMT ODS - Avantis"),
            new ChoiceValue("ah-dlive", Local("A&H CSV natif - dLive", "Native A&H CSV - dLive")),
            new ChoiceValue("ah-avantis", Local("A&H CSV natif - Avantis", "Native A&H CSV - Avantis")),
            new ChoiceValue("yamaha-cl", Local("Yamaha ZIP natif - CL", "Native Yamaha ZIP - CL")),
            new ChoiceValue("yamaha-ql", Local("Yamaha ZIP natif - QL", "Native Yamaha ZIP - QL"))
        };
        FindControl<ComboBox>("FormatCombo")!.SelectedIndex = 1;
        FindControl<ComboBox>("KindCombo")!.ItemsSource = new ChoiceValue[]
        {
            new("tx", "TX"),
            new("rx", "RX")
        };
        FindControl<ComboBox>("CaseModeCombo")!.ItemsSource = new ChoiceValue[]
        {
            new("preserve", Local("Conserver", "Preserve")),
            new("lower", Local("Tout en minuscules", "Lowercase")),
            new("upper", Local("Tout en majuscules", "Uppercase")),
            new("first-upper", Local("Première lettre en majuscule", "First letter uppercase"))
        };
        FindControl<ComboBox>("CaseModeCombo")!.SelectedIndex = 0;
        DanteDevice? selectedDevice = _project.Devices.FirstOrDefault(device =>
            string.Equals(device.Name, initiallySelectedDevice, StringComparison.OrdinalIgnoreCase));
        FindControl<ComboBox>("KindCombo")!.SelectedIndex = selectedDevice is { TxCount: 0, RxCount: > 0 } ? 1 : 0;
        UpdateDeviceAvailability();
    }

    private void ApplyLanguage()
    {
        Title = Local("Exporter des labels de canaux", "Export channel labels");
        FindControl<TextBlock>("IntroText")!.Text = Local(
            "Sélectionnez les machines, les canaux TX/RX et le format. Les formats natifs sont créés directement depuis les modèles inclus dans DCE.",
            "Select devices, TX/RX channels and a format. Native files are created directly from the templates included in DCE.");
        FindControl<TextBlock>("DevicesTitle")!.Text = Local("Machines à exporter", "Devices to export");
        DataGrid devices = FindControl<DataGrid>("DevicesGrid")!;
        devices.Columns[0].Header = Local("Utiliser", "Use");
        devices.Columns[1].Header = Local("Machine", "Device");
        FindControl<TextBlock>("OptionsTitle")!.Text = Local("Options", "Options");
        FindControl<TextBlock>("FormatLabel")!.Text = Local("Format", "Format");
        FindControl<TextBlock>("KindLabel")!.Text = Local("Canaux", "Channels");
        FindControl<TextBlock>("StartLabel")!.Text = Local("Premier canal", "First channel");
        FindControl<TextBlock>("CountLabel")!.Text = Local("Nombre (0 = tous)", "Count (0 = all)");
        FindControl<TextBlock>("MaximumLengthLabel")!.Text = Local("Longueur maximale (0 = aucune)", "Maximum length (0 = none)");
        FindControl<TextBlock>("CaseModeLabel")!.Text = Local("Casse", "Letter case");
        FindControl<TextBlock>("StartPositionLabel")!.Text = Local("Commencer au caractère", "Start at character");
        FindControl<CheckBox>("FromEndCheckBox")!.Content = Local("Compter depuis la fin", "Count from the end");
        FindControl<TextBlock>("AdaptDmtText")!.Text = Local(
            "Adapter au format console : ASCII compatible",
            "Adapt to console format: compatible ASCII");
        FindControl<Button>("PreviewButton")!.Content = Local("Actualiser l'aperçu", "Refresh preview");
        FindControl<TextBlock>("PreviewTitle")!.Text = Local("Labels exportés", "Exported labels");
        DataGrid preview = FindControl<DataGrid>("PreviewGrid")!;
        preview.Columns[0].Header = Local("Machine", "Device");
        preview.Columns[2].Header = Local("Canal", "Channel");
        preview.Columns[3].Header = Local("Label Dante", "Dante label");
        preview.Columns[4].Header = Local("Label exporté", "Exported label");
        preview.Columns[5].Header = Local("Contrôle", "Check");
        FindControl<Button>("CancelButton")!.Content = Local("Annuler", "Cancel");
        FindControl<Button>("ExportButton")!.Content = Local("Exporter", "Export");
        UpdateFormatHelp();
    }

    private void FormatCombo_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_initializing)
        {
            return;
        }

        if (FindControl<CheckBox>("AdaptDmtCheckBox") is not { } adapt)
        {
            return;
        }
        bool consoleTemplate = RequiresConsoleCompatibility(SelectedValue("FormatCombo"));
        adapt.IsEnabled = consoleTemplate;
        _adaptationUserOverride = false;
        adapt.IsChecked = consoleTemplate;
        FindControl<TextBox>("MaximumLengthTextBox")!.Text = consoleTemplate ? "8" : "0";
        UpdateFormatHelp();
        RefreshPreview();
    }

    private void DeviceSelectionCheckBox_Click(object? sender, RoutedEventArgs e)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(RefreshPreview);
    }

    private void CaseModeCombo_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!_initializing)
        {
            RefreshPreview();
        }
    }

    private void AdaptDmtCheckBox_Click(object? sender, RoutedEventArgs e)
    {
        _adaptationUserOverride = true;
        RefreshPreview();
    }

    private void OptionsChanged(object? sender, RoutedEventArgs e)
    {
        if (!_initializing)
        {
            RefreshPreview();
        }
    }

    private void KindCombo_SelectionChanged(object? sender, SelectionChangedEventArgs e)
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
        if (FindControl<ComboBox>("KindCombo") is not { } kindCombo)
        {
            return;
        }

        bool rx = SelectedValue("KindCombo") == "rx" || kindCombo.SelectedIndex == 1;
        foreach (MacChannelLabelDeviceSelection device in _devices)
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
        if (FindControl<TextBlock>("FormatHelpText") is not { } help)
        {
            return;
        }

        help.Text = RequiresConsoleCompatibility(SelectedValue("FormatCombo"))
            ? Local(
                "Le modèle dLive, Avantis, CL ou QL est inclus dans DCE. Seuls le nom et le dossier du nouveau fichier seront demandés.",
                "The dLive, Avantis, CL or QL template is included in DCE. Only the new file name and folder will be requested.")
            : Local(
                "Le CSV générique sert aux échanges avec DCE ; il ne doit pas être importé directement dans une console.",
                "Generic CSV is intended for DCE exchanges; do not import it directly into a console.");
    }

    private async void PreviewButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            RefreshPreview();
        }
        catch (Exception exception)
        {
            await MessageDialog.ShowInfoAsync(this, Local("Aperçu impossible", "Preview unavailable"), exception.Message, "OK");
        }
    }

    private void RefreshPreview()
    {
        if (_initializing)
        {
            return;
        }

        _preview.Clear();
        string[] selected = _devices.Where(device => device.IsSelected && device.IsAvailable).Select(device => device.Name).ToArray();
        if (selected.Length == 0)
        {
            FindControl<TextBlock>("SummaryText")!.Text = Local(
                "Sélectionnez au moins une machine disposant de canaux dans le sens TX/RX choisi.",
                "Select at least one device with channels in the chosen TX/RX direction.");
            FindControl<Button>("ExportButton")!.IsEnabled = false;
            return;
        }

        int start = int.TryParse(FindControl<TextBox>("StartTextBox")!.Text, out int parsedStart) && parsedStart > 0 ? parsedStart : 1;
        int? count = int.TryParse(FindControl<TextBox>("CountTextBox")!.Text, out int parsedCount) && parsedCount > 0 ? parsedCount : null;
        DanteChannelKind kind = SelectedValue("KindCombo") == "rx" ? DanteChannelKind.Rx : DanteChannelKind.Tx;
        bool consoleTemplate = RequiresConsoleCompatibility(SelectedValue("FormatCombo"));
        ChannelLabelDocument document;
        try
        {
            document = ChannelLabelExchangeService.CreateFromProject(_project!, selected, kind, start, count);
        }
        catch (InvalidOperationException)
        {
            FindControl<TextBlock>("SummaryText")!.Text = Local(
                "Aucun canal TX/RX ne correspond aux machines et à la plage sélectionnées.",
                "No TX/RX channel matches the selected devices and range.");
            FindControl<Button>("ExportButton")!.IsEnabled = false;
            return;
        }

        bool requiresAdaptation = consoleTemplate && document.Sets
            .SelectMany(set => set.Channels)
            .Any(channel => !DmtChannelWorkbookService.CheckCompatibility(channel.Label).IsCompatible);
        if (consoleTemplate && !_adaptationUserOverride)
        {
            FindControl<CheckBox>("AdaptDmtCheckBox")!.IsChecked = requiresAdaptation;
        }

        ChannelLabelTransformOptions transformOptions;
        try
        {
            transformOptions = ReadTransformOptions();
        }
        catch (InvalidOperationException exception)
        {
            FindControl<TextBlock>("SummaryText")!.Text = exception.Message;
            FindControl<Button>("ExportButton")!.IsEnabled = false;
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
                    warnings.Add(Local(
                        $"Doublon après transformation : canaux {string.Join(", ", collision.Channels)}",
                        $"Duplicate after transformation: channels {string.Join(", ", collision.Channels)}"));
                }
                if (consoleTemplate && !compatibility.IsCompatible && !adapt)
                {
                    incompatible++;
                }
                _preview.Add(new MacChannelLabelExportPreviewRow(
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
            FindControl<TextBlock>("SummaryText")!.Text = Local(
                "Aucun canal ne correspond à la plage demandée.",
                "No channel matches the requested range.");
            FindControl<Button>("ExportButton")!.IsEnabled = false;
            return;
        }

        FindControl<TextBlock>("SummaryText")!.Text = _language == UiLanguage.English
            ? $"{document.Sets.Count} device(s), {_preview.Count} label(s), {incompatible} console-incompatible label(s), {_collisionCount} collision(s)."
            : $"{document.Sets.Count} machine(s), {_preview.Count} label(s), {incompatible} label(s) incompatible(s) avec la console, {_collisionCount} collision(s).";
        FindControl<Button>("ExportButton")!.IsEnabled = true;
    }

    private async void ExportButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            RefreshPreview();
            string[] devices = _devices.Where(device => device.IsSelected && device.IsAvailable).Select(device => device.Name).ToArray();
            if (devices.Length == 0)
            {
                throw new InvalidOperationException(Local("Sélectionnez au moins une machine.", "Select at least one device."));
            }
            if (_preview.Count == 0)
            {
                throw new InvalidOperationException(FindControl<TextBlock>("SummaryText")!.Text);
            }

            string format = SelectedValue("FormatCombo");
            bool adapt = FindControl<CheckBox>("AdaptDmtCheckBox")!.IsChecked == true;
            if (RequiresConsoleCompatibility(format) && !adapt && _preview.Any(row => !string.IsNullOrWhiteSpace(row.Warning)))
            {
                throw new InvalidOperationException(Local(
                    "Certains labels ne sont pas compatibles avec le format console. Activez l'adaptation explicite ou choisissez JSON/CSV générique.",
                    "Some labels are not console-compatible. Enable explicit adaptation or choose generic JSON/CSV."));
            }
            if (_collisionCount > 0)
            {
                throw new InvalidOperationException(Local(
                    "La transformation crée des labels en doublon. Modifiez la casse, la position de départ ou la longueur avant l'export.",
                    "The transformation creates duplicate labels. Change the letter case, start position, or length before exporting."));
            }

            Close(new ChannelLabelExportDialogResult(
                format,
                devices,
                SelectedValue("KindCombo") == "rx" ? DanteChannelKind.Rx : DanteChannelKind.Tx,
                LocalizedNumberParser.ParsePositive(
                    FindControl<TextBox>("StartTextBox")!.Text,
                    Local("premier canal", "first channel"),
                    _language),
                LocalizedNumberParser.ParseOptionalCount(
                    FindControl<TextBox>("CountTextBox")!.Text,
                    _language),
                adapt,
                ReadTransformOptions()));
        }
        catch (Exception exception)
        {
            await MessageDialog.ShowInfoAsync(this, Local("Export impossible", "Export unavailable"), exception.Message, "OK");
        }
    }

    private void CancelButton_Click(object? sender, RoutedEventArgs e) => Close(null);

    private string SelectedValue(string controlName) =>
        (FindControl<ComboBox>(controlName)!.SelectedItem as ChoiceValue)?.Value ?? string.Empty;

    private static bool RequiresConsoleCompatibility(string format) => BuiltInChannelLabelTemplateService.IsBuiltInFormat(format);

    private ChannelLabelTransformOptions ReadTransformOptions()
    {
        int maximumLength = int.TryParse(FindControl<TextBox>("MaximumLengthTextBox")!.Text?.Trim(), out int parsedLength)
            && parsedLength >= 0
                ? parsedLength
                : throw new InvalidOperationException(Local("Longueur maximale invalide.", "Invalid maximum length."));
        int startPosition = int.TryParse(FindControl<TextBox>("StartPositionTextBox")!.Text?.Trim(), out int parsedStart)
            && parsedStart > 0
                ? parsedStart
                : throw new InvalidOperationException(Local("Position de départ invalide.", "Invalid start position."));
        ChannelLabelCaseMode caseMode = SelectedValue("CaseModeCombo") switch
        {
            "lower" => ChannelLabelCaseMode.Lowercase,
            "upper" => ChannelLabelCaseMode.Uppercase,
            "first-upper" => ChannelLabelCaseMode.FirstLetterUppercase,
            _ => ChannelLabelCaseMode.Preserve
        };
        return new ChannelLabelTransformOptions(
            FindControl<CheckBox>("AdaptDmtCheckBox")!.IsChecked == true,
            caseMode,
            maximumLength,
            startPosition,
            FindControl<CheckBox>("FromEndCheckBox")!.IsChecked == true);
    }

    private string Local(string french, string english) => _language == UiLanguage.English ? english : french;
}

internal sealed record MacChannelLabelExportPreviewRow(
    string DeviceName,
    DanteChannelKind Kind,
    int ChannelNumber,
    string OriginalLabel,
    string ExportedLabel,
    string Warning);
