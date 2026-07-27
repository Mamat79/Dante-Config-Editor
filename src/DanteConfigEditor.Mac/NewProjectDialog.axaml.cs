using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using DanteConfigEditor.Models;
using DanteConfigEditor.Services;

namespace DanteConfigEditor.Mac;

internal sealed record MacNewProjectFormResult(
    string DestinationPath,
    string ProjectName,
    string Description,
    string DeviceName,
    int TxCount,
    int RxCount,
    int SampleRate,
    int Encoding,
    int UnicastLatency,
    Guid? TemplateId,
    MachineInstanceOptions? TemplateOptions,
    string BankPath);

internal sealed partial class NewProjectDialog : Window
{
    private static readonly FilePickerFileType XmlFileType = new("Dante XML")
    {
        Patterns = ["*.xml"],
        MimeTypes = ["application/xml", "text/xml"]
    };

    private UiLanguage _language;
    private string _bankPath = string.Empty;
    private MachineBankRepository? _repository;

    public NewProjectDialog()
    {
        InitializeComponent();
    }

    private T? FindControl<T>(string name) where T : Control =>
        ControlExtensions.FindControl<T>(this, name);

    public static Task<MacNewProjectFormResult?> ShowAsync(
        Window owner,
        UiLanguage language)
    {
        NewProjectDialog dialog = new()
        {
            _language = language,
            _bankPath = MachineBankLocationService.CreateDefault().Load(),
            Title = language == UiLanguage.English
                ? "Experimental new project"
                : "Nouveau projet expérimental"
        };
        dialog._repository = new MachineBankRepository(dialog._bankPath);
        dialog.ConfigureChoices();
        dialog.ApplyLanguage();
        dialog.LoadSources();
        return dialog.ShowDialog<MacNewProjectFormResult?>(owner);
    }

    private void ConfigureChoices()
    {
        ComboBox sampleRate = FindControl<ComboBox>("SampleRateComboBox")!;
        sampleRate.ItemsSource = new[] { 44100, 48000, 88200, 96000, 176400, 192000 };
        sampleRate.SelectedItem = 48000;
        ComboBox encoding = FindControl<ComboBox>("EncodingComboBox")!;
        encoding.ItemsSource = new[] { 16, 24, 32 };
        encoding.SelectedItem = 24;
        ComboBox latency = FindControl<ComboBox>("LatencyComboBox")!;
        latency.ItemsSource = new[] { 250, 1000, 2000, 5000 };
        latency.SelectedItem = 1000;
    }

    private void ApplyLanguage()
    {
        FindControl<TextBlock>("HeadingText")!.Text = Title;
        FindControl<TextBlock>("ExperimentalWarningText")!.Text = L(
            "La structure 3.0.0 suit les rôles personnalisés du Dante Preset Creator officiel, "
            + "mais doit encore être validée par un import réel dans Dante Controller avant un usage en production.",
            "The 3.0.0 structure follows custom roles from the official Dante Preset Creator, "
            + "but still requires a real Dante Controller import before production use.");
        FindControl<TextBlock>("PathLabel")!.Text = L("Fichier XML à créer", "XML file to create");
        FindControl<Button>("BrowsePathButton")!.Content = L("Choisir", "Browse");
        FindControl<TextBlock>("ProjectNameLabel")!.Text = L("Nom du projet", "Project name");
        FindControl<TextBlock>("DescriptionLabel")!.Text = L("Description", "Description");
        FindControl<TextBlock>("InitialDeviceHeading")!.Text = L("Première machine", "First device");
        FindControl<TextBlock>("SourceLabel")!.Text = L("Source", "Source");
        FindControl<TextBlock>("DeviceNameLabel")!.Text = L("Nom de la machine", "Device name");
        FindControl<TextBlock>("EncodingLabel")!.Text = L("Encodage", "Encoding");
        FindControl<TextBlock>("LatencyLabel")!.Text = L("Latence", "Latency");
        FindControl<CheckBox>("UseTemplateTxCheckBox")!.Content = L(
            "Labels TX du modèle",
            "Template Tx labels");
        FindControl<CheckBox>("UseTemplateRxCheckBox")!.Content = L(
            "Labels RX du modèle",
            "Template Rx labels");
        FindControl<Button>("CreateButton")!.Content = L("Créer le projet", "Create project");
        FindControl<Button>("CancelButton")!.Content = L("Annuler", "Cancel");
    }

    private void LoadSources()
    {
        List<MacNewProjectSourceChoice> choices =
        [
            new(
                null,
                L("Machine personnalisée", "Custom device"),
                0,
                0,
                ProjectCreationService.PresetVersion)
        ];
        try
        {
            choices.AddRange(_repository!.List().Select(metadata =>
                new MacNewProjectSourceChoice(
                    metadata.TemplateId,
                    $"{metadata.TemplateName} · {metadata.TxCount} TX/{metadata.RxCount} RX",
                    metadata.TxCount,
                    metadata.RxCount,
                    metadata.SourcePresetVersion)));
        }
        catch (Exception exception)
        {
            DiagnosticLogService.Default.Write(
                "MachineBank",
                "Impossible de lire la banque dans l'assistant Nouveau projet.",
                exception);
        }

        ComboBox source = FindControl<ComboBox>("SourceComboBox")!;
        source.ItemsSource = choices;
        source.SelectedIndex = 0;
    }

    private async void BrowsePathButton_Click(object? sender, RoutedEventArgs e)
    {
        IStorageFile? file = await StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                Title = L("Créer le nouveau preset", "Create the new preset"),
                SuggestedFileName = "Dante_New_Project.xml",
                DefaultExtension = "xml",
                ShowOverwritePrompt = true,
                FileTypeChoices = [XmlFileType]
            });
        string? path = file?.TryGetLocalPath();
        if (path is null)
        {
            return;
        }

        FindControl<TextBox>("PathTextBox")!.Text = path;
        if (string.IsNullOrWhiteSpace(FindControl<TextBox>("ProjectNameTextBox")!.Text))
        {
            FindControl<TextBox>("ProjectNameTextBox")!.Text = Path.GetFileNameWithoutExtension(path);
        }
    }

    private void SourceComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        MacNewProjectSourceChoice? choice =
            FindControl<ComboBox>("SourceComboBox")!.SelectedItem as MacNewProjectSourceChoice;
        bool usesTemplate = choice?.TemplateId is not null;
        FindControl<Grid>("CustomDeviceGrid")!.IsVisible = !usesTemplate;
        FindControl<Grid>("TemplateLabelsGrid")!.IsVisible = usesTemplate;
        if (usesTemplate && choice is not null)
        {
            string candidate = DanteNameRules.NormalizeDeviceNamePart(choice.Display, "DEVICE");
            FindControl<TextBox>("DeviceNameTextBox")!.Text =
                candidate[..Math.Min(candidate.Length, DanteNameRules.MaximumNameLength)].Trim('-');
        }

        UpdateTemplatePrefixState();
    }

    private void TemplateLabelModeChanged(object? sender, RoutedEventArgs e)
    {
        UpdateTemplatePrefixState();
    }

    private void UpdateTemplatePrefixState()
    {
        FindControl<TextBox>("TxPrefixTextBox")!.IsEnabled =
            FindControl<CheckBox>("UseTemplateTxCheckBox")!.IsChecked != true;
        FindControl<TextBox>("RxPrefixTextBox")!.IsEnabled =
            FindControl<CheckBox>("UseTemplateRxCheckBox")!.IsChecked != true;
    }

    private async void CreateButton_Click(object? sender, RoutedEventArgs e)
    {
        string destination = FindControl<TextBox>("PathTextBox")!.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(destination))
        {
            await ShowValidationAsync(L("Choisissez le fichier XML à créer.", "Choose the XML file to create."));
            return;
        }

        destination = Path.GetFullPath(destination);
        if (File.Exists(destination))
        {
            await ShowValidationAsync(L(
                "Ce fichier existe déjà. Choisissez un nouveau nom : rien ne sera écrasé.",
                "This file already exists. Choose a new name: nothing will be overwritten."));
            return;
        }

        string projectName = FindControl<TextBox>("ProjectNameTextBox")!.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(projectName))
        {
            await ShowValidationAsync(L("Le nom du projet est obligatoire.", "The project name is required."));
            return;
        }

        string deviceName = FindControl<TextBox>("DeviceNameTextBox")!.Text?.Trim() ?? string.Empty;
        string? deviceNameError = DanteNameRules.ValidateDeviceName(deviceName);
        if (deviceNameError is not null)
        {
            await ShowValidationAsync(_language == UiLanguage.English
                ? "Use at most 31 letters, digits or hyphens."
                : deviceNameError);
            return;
        }

        MacNewProjectSourceChoice choice =
            (MacNewProjectSourceChoice)FindControl<ComboBox>("SourceComboBox")!.SelectedItem!;
        if (choice.TemplateId.HasValue)
        {
            if (!string.Equals(
                    choice.SourcePresetVersion,
                    ProjectCreationService.PresetVersion,
                    StringComparison.OrdinalIgnoreCase))
            {
                await ShowValidationAsync(L(
                    $"Ce modèle provient d'un preset {choice.SourcePresetVersion}. "
                    + $"Le nouveau projet cible {ProjectCreationService.PresetVersion}; aucune migration sûre n'est disponible.",
                    $"This template comes from preset {choice.SourcePresetVersion}. "
                    + $"The new project targets {ProjectCreationService.PresetVersion}; no safe migration is available."));
                return;
            }

            bool useTx = FindControl<CheckBox>("UseTemplateTxCheckBox")!.IsChecked == true;
            bool useRx = FindControl<CheckBox>("UseTemplateRxCheckBox")!.IsChecked == true;
            string txPrefix = FindControl<TextBox>("TxPrefixTextBox")!.Text?.Trim() ?? string.Empty;
            string rxPrefix = FindControl<TextBox>("RxPrefixTextBox")!.Text?.Trim() ?? string.Empty;
            if (!useTx && string.IsNullOrWhiteSpace(txPrefix)
                || !useRx && string.IsNullOrWhiteSpace(rxPrefix))
            {
                await ShowValidationAsync(L(
                    "Les préfixes remplacés doivent être renseignés.",
                    "Replacement prefixes must be provided."));
                return;
            }

            Close(new MacNewProjectFormResult(
                destination,
                projectName,
                FindControl<TextBox>("DescriptionTextBox")!.Text?.Trim() ?? string.Empty,
                deviceName,
                choice.TxCount,
                choice.RxCount,
                48000,
                24,
                1000,
                choice.TemplateId,
                new MachineInstanceOptions
                {
                    NewName = deviceName,
                    UseTemplateTxLabels = useTx,
                    UseTemplateRxLabels = useRx,
                    TxLabelPrefix = useTx ? null : txPrefix,
                    RxLabelPrefix = useRx ? null : rxPrefix
                },
                _bankPath));
            return;
        }

        if (!TryReadCount(FindControl<TextBox>("TxCountTextBox")!.Text, out int txCount)
            || !TryReadCount(FindControl<TextBox>("RxCountTextBox")!.Text, out int rxCount)
            || txCount + rxCount == 0)
        {
            await ShowValidationAsync(L(
                $"Les nombres TX/RX doivent être compris entre 0 et {ProjectCreationService.MaximumAudioChannelsPerDirection}, avec au moins un canal.",
                $"Tx/Rx counts must be between 0 and {ProjectCreationService.MaximumAudioChannelsPerDirection}, with at least one channel."));
            return;
        }

        Close(new MacNewProjectFormResult(
            destination,
            projectName,
            FindControl<TextBox>("DescriptionTextBox")!.Text?.Trim() ?? string.Empty,
            deviceName,
            txCount,
            rxCount,
            (int)FindControl<ComboBox>("SampleRateComboBox")!.SelectedItem!,
            (int)FindControl<ComboBox>("EncodingComboBox")!.SelectedItem!,
            (int)FindControl<ComboBox>("LatencyComboBox")!.SelectedItem!,
            null,
            null,
            _bankPath));
    }

    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        Close(null);
    }

    private static bool TryReadCount(string? text, out int count) =>
        int.TryParse(text, out count)
        && count >= 0
        && count <= ProjectCreationService.MaximumAudioChannelsPerDirection;

    private Task<bool> ShowValidationAsync(string message) =>
        MessageDialog.ShowInfoAsync(this, L("Valeur invalide", "Invalid value"), message, "OK");

    private string L(string french, string english) =>
        _language == UiLanguage.English ? english : french;

    private sealed record MacNewProjectSourceChoice(
        Guid? TemplateId,
        string Display,
        int TxCount,
        int RxCount,
        string SourcePresetVersion)
    {
        public override string ToString() => Display;
    }
}
