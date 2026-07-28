using System.IO;
using System.Windows;
using DanteConfigEditor.Models;
using DanteConfigEditor.Services;
using Microsoft.Win32;

namespace DanteConfigEditor;

public sealed record NewProjectFormResult(
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

public partial class NewProjectWindow : Window
{
    private readonly UiLanguage _language;
    private readonly bool _useLightTheme;
    private int _availableTemplateCount;

    public NewProjectWindow(UiLanguage language, bool useLightTheme)
    {
        InitializeComponent();
        _language = language;
        _useLightTheme = useLightTheme;
        DialogThemeService.Apply(this, useLightTheme);
        SampleRateComboBox.ItemsSource = new[] { 44100, 48000, 88200, 96000, 176400, 192000 };
        SampleRateComboBox.SelectedItem = 48000;
        EncodingComboBox.ItemsSource = new[] { 16, 24, 32 };
        EncodingComboBox.SelectedItem = 24;
        LatencyComboBox.ItemsSource = new[] { 250, 1000, 2000, 5000 };
        LatencyComboBox.SelectedItem = 1000;
        ApplyLanguage();
        LoadSources();
    }

    public NewProjectFormResult? Result { get; private set; }

    private void ApplyLanguage()
    {
        Title = L("Nouveau projet expérimental", "Experimental new project");
        HeadingTextBlock.Text = Title;
        ExperimentalWarningTextBlock.Text = L(
            "La structure 3.0.0 suit les rôles personnalisés du Dante Preset Creator officiel, "
            + "mais doit encore être validée par un import réel dans Dante Controller avant un usage en production.",
            "The 3.0.0 structure follows custom roles from the official Dante Preset Creator, "
            + "but still requires a real Dante Controller import before production use.");
        PathLabel.Content = L("Fichier XML à créer", "XML file to create");
        BrowsePathButton.Content = L("Choisir", "Browse");
        ProjectNameLabel.Content = L("Nom du projet", "Project name");
        DescriptionLabel.Content = L("Description", "Description");
        ProjectGroupBox.Header = L("Projet", "Project");
        InitialDeviceGroupBox.Header = L("Première machine", "First device");
        SourceLabel.Content = L("Source", "Source");
        OpenBanksButton.Content = L("Gérer", "Manage");
        OpenBanksButton.ToolTip = L(
            "Ouvre la gestion des banques de machines.",
            "Opens device bank management.");
        DeviceNameLabel.Content = L("Nom de la machine", "Device name");
        EncodingLabel.Content = L("Encodage", "Encoding");
        LatencyLabel.Content = L("Latence", "Latency");
        UseTemplateTxCheckBox.Content = L("Labels TX du modèle", "Template Tx labels");
        UseTemplateRxCheckBox.Content = L("Labels RX du modèle", "Template Rx labels");
        CreateButton.Content = L("Créer le projet", "Create project");
        CancelButton.Content = L("Annuler", "Cancel");
    }

    private void LoadSources()
    {
        List<NewProjectSourceChoice> choices =
        [
            new(
                null,
                L("Machine personnalisée", "Custom device"),
                "DEVICE",
                0,
                0,
                ProjectCreationService.PresetVersion,
                null)
        ];
        string activeBankPath = MachineBankLocationService.CreateDefault().Load();
        List<string> bankPaths =
        [
            activeBankPath,
            .. MachineBankDistributionService.DiscoverIncludedBankPaths()
        ];
        HashSet<string> knownTemplates = new(StringComparer.OrdinalIgnoreCase);
        int loadedBanks = 0;
        foreach (string bankPath in bankPaths
                     .Select(Path.GetFullPath)
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                IReadOnlyList<MachineTemplateMetadata> templates =
                    new MachineBankRepository(bankPath).List();
                loadedBanks++;
                string bankName = string.Equals(
                    bankPath,
                    activeBankPath,
                    StringComparison.OrdinalIgnoreCase)
                    ? L("Ma banque", "My bank")
                    : Path.GetFileName(bankPath);
                foreach (MachineTemplateMetadata metadata in templates)
                {
                    string identity = string.Join(
                        "|",
                        metadata.Manufacturer,
                        metadata.Model,
                        metadata.TemplateName,
                        metadata.TxCount,
                        metadata.RxCount);
                    if (!knownTemplates.Add(identity))
                    {
                        continue;
                    }

                    choices.Add(new NewProjectSourceChoice(
                        metadata.TemplateId,
                        $"{bankName} › {metadata.TemplateName} · {metadata.TxCount} TX/{metadata.RxCount} RX",
                        metadata.TemplateName,
                        metadata.TxCount,
                        metadata.RxCount,
                        metadata.SourcePresetVersion,
                        bankPath));
                }
            }
            catch (Exception ex)
            {
                DiagnosticLogService.Default.Write(
                    "MachineBank",
                    $"Impossible de lire la banque {bankPath} dans l'assistant Nouveau projet.",
                    ex);
            }
        }

        _availableTemplateCount = choices.Count - 1;
        SourceComboBox.ItemsSource = choices;
        SourceComboBox.SelectedIndex = 0;
        SourceHelpTextBlock.Text = _language == UiLanguage.English
            ? $"{_availableTemplateCount} reusable template(s) found across {loadedBanks} bank(s), including installed templates."
            : $"{_availableTemplateCount} modèle(s) réutilisable(s) trouvé(s) dans {loadedBanks} banque(s), dont les modèles installés.";
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        Rect workArea = SystemParameters.WorkArea;
        MaxWidth = Math.Max(MinWidth, workArea.Width - 32);
        MaxHeight = Math.Max(MinHeight, workArea.Height - 32);
        Width = Math.Min(Width, MaxWidth);
        Height = Math.Min(Height, MaxHeight);
    }

    private void OpenBanksButton_Click(object sender, RoutedEventArgs e)
    {
        string activeBankPath = MachineBankLocationService.CreateDefault().Load();
        MachineBankWindow window = new(
            _language,
            _useLightTheme,
            [],
            canAddToProject: false,
            activeBankPath)
        {
            Owner = this
        };
        window.ShowDialog();
        LoadSources();
    }

    private void BrowsePathButton_Click(object sender, RoutedEventArgs e)
    {
        SaveFileDialog dialog = new()
        {
            Filter = "Dante XML (*.xml)|*.xml",
            Title = L("Créer le nouveau preset", "Create the new preset"),
            FileName = "Dante_New_Project.xml"
        };
        if (dialog.ShowDialog(this) == true)
        {
            PathTextBox.Text = dialog.FileName;
            if (string.IsNullOrWhiteSpace(ProjectNameTextBox.Text))
            {
                ProjectNameTextBox.Text = Path.GetFileNameWithoutExtension(dialog.FileName);
            }
        }
    }

    private void SourceComboBox_SelectionChanged(object sender, RoutedEventArgs e)
    {
        NewProjectSourceChoice? choice = SourceComboBox.SelectedItem as NewProjectSourceChoice;
        bool usesTemplate = choice?.TemplateId is not null;
        CustomDeviceGrid.Visibility = usesTemplate ? Visibility.Collapsed : Visibility.Visible;
        TemplateLabelsGrid.Visibility = usesTemplate ? Visibility.Visible : Visibility.Collapsed;
        if (usesTemplate && choice is not null)
        {
            string candidate = DanteNameRules.NormalizeDeviceNamePart(choice.SuggestedName, "DEVICE");
            DeviceNameTextBox.Text = candidate[..Math.Min(
                candidate.Length,
                DanteNameRules.MaximumNameLength)].Trim('-');
        }

        UpdateTemplatePrefixState();
    }

    private void TemplateLabelModeChanged(object sender, RoutedEventArgs e)
    {
        UpdateTemplatePrefixState();
    }

    private void UpdateTemplatePrefixState()
    {
        if (TxPrefixTextBox is null || RxPrefixTextBox is null)
        {
            return;
        }

        TxPrefixTextBox.IsEnabled = UseTemplateTxCheckBox.IsChecked != true;
        RxPrefixTextBox.IsEnabled = UseTemplateRxCheckBox.IsChecked != true;
    }

    private void CreateButton_Click(object sender, RoutedEventArgs e)
    {
        string destination = PathTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(destination))
        {
            ShowValidation(L("Choisissez le fichier XML à créer.", "Choose the XML file to create."));
            return;
        }

        destination = Path.GetFullPath(destination);
        if (File.Exists(destination))
        {
            ShowValidation(L(
                "Ce fichier existe déjà. Choisissez un nouveau nom : rien ne sera écrasé.",
                "This file already exists. Choose a new name: nothing will be overwritten."));
            return;
        }

        string projectName = ProjectNameTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(projectName))
        {
            ShowValidation(L("Le nom du projet est obligatoire.", "The project name is required."));
            return;
        }

        string deviceName = DeviceNameTextBox.Text.Trim();
        string? deviceNameError = DanteNameRules.ValidateDeviceName(deviceName);
        if (deviceNameError is not null)
        {
            ShowValidation(_language == UiLanguage.English
                ? "The device name is invalid. Use at most 31 letters, digits or hyphens."
                : deviceNameError);
            return;
        }

        NewProjectSourceChoice choice = (NewProjectSourceChoice)SourceComboBox.SelectedItem;
        if (choice.TemplateId.HasValue)
        {
            if (!string.Equals(
                    choice.SourcePresetVersion,
                    ProjectCreationService.PresetVersion,
                    StringComparison.OrdinalIgnoreCase))
            {
                ShowValidation(L(
                    $"Ce modèle provient d'un preset {choice.SourcePresetVersion}. "
                    + $"Le nouveau projet cible {ProjectCreationService.PresetVersion}; aucune migration sûre n'est disponible.",
                    $"This template comes from preset {choice.SourcePresetVersion}. "
                    + $"The new project targets {ProjectCreationService.PresetVersion}; no safe migration is available."));
                return;
            }

            MachineInstanceOptions options = new()
            {
                NewName = deviceName,
                UseTemplateTxLabels = UseTemplateTxCheckBox.IsChecked == true,
                UseTemplateRxLabels = UseTemplateRxCheckBox.IsChecked == true,
                TxLabelPrefix = UseTemplateTxCheckBox.IsChecked == true
                    ? null
                    : TxPrefixTextBox.Text.Trim(),
                RxLabelPrefix = UseTemplateRxCheckBox.IsChecked == true
                    ? null
                    : RxPrefixTextBox.Text.Trim()
            };
            Result = new NewProjectFormResult(
                destination,
                projectName,
                DescriptionTextBox.Text.Trim(),
                deviceName,
                choice.TxCount,
                choice.RxCount,
                48000,
                24,
                1000,
                choice.TemplateId,
                options,
                choice.BankPath
                    ?? MachineBankLocationService.CreateDefault().Load());
            DialogResult = true;
            return;
        }

        if (!TryReadCount(TxCountTextBox.Text, out int txCount)
            || !TryReadCount(RxCountTextBox.Text, out int rxCount)
            || txCount + rxCount == 0)
        {
            ShowValidation(L(
                $"Les nombres TX/RX doivent être compris entre 0 et {ProjectCreationService.MaximumAudioChannelsPerDirection}, avec au moins un canal.",
                $"Tx/Rx counts must be between 0 and {ProjectCreationService.MaximumAudioChannelsPerDirection}, with at least one channel."));
            return;
        }

        Result = new NewProjectFormResult(
            destination,
            projectName,
            DescriptionTextBox.Text.Trim(),
            deviceName,
            txCount,
            rxCount,
            (int)SampleRateComboBox.SelectedItem,
            (int)EncodingComboBox.SelectedItem,
            (int)LatencyComboBox.SelectedItem,
            null,
            null,
            MachineBankLocationService.CreateDefault().Load());
        DialogResult = true;
    }

    private static bool TryReadCount(string text, out int count)
    {
        return int.TryParse(text, out count)
            && count >= 0
            && count <= ProjectCreationService.MaximumAudioChannelsPerDirection;
    }

    private void ShowValidation(string message)
    {
        MessageBox.Show(
            this,
            message,
            L("Valeur invalide", "Invalid value"),
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }

    private string L(string french, string english)
    {
        return _language == UiLanguage.English ? english : french;
    }

    private sealed record NewProjectSourceChoice(
        Guid? TemplateId,
        string Display,
        string SuggestedName,
        int TxCount,
        int RxCount,
        string SourcePresetVersion,
        string? BankPath)
    {
        public override string ToString()
        {
            return Display;
        }
    }
}
