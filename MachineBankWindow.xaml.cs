using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using DanteConfigEditor.Infrastructure.Migration;
using DanteConfigEditor.Models;
using DanteConfigEditor.Services;
using Microsoft.Win32;

namespace DanteConfigEditor;

public partial class MachineBankWindow : Window
{
    private readonly UiLanguage _language;
    private readonly bool _useLightTheme;
    private readonly HashSet<string> _usedDeviceNames;
    private readonly bool _canAddToProject;
    private readonly ObservableCollection<MachineBankRow> _visibleRows = [];
    private readonly MachineBankLocationService _locationService;
    private IReadOnlyList<MachineTemplateMetadata> _allTemplates = [];
    private MachineBankRepository _repository;
    private string _bankPath;
    private bool _updatingFilters;

    public MachineBankWindow(
        UiLanguage language,
        bool useLightTheme,
        IEnumerable<string> usedDeviceNames,
        bool canAddToProject,
        string? initialBankPath = null)
    {
        InitializeComponent();
        _language = language;
        _useLightTheme = useLightTheme;
        _usedDeviceNames = usedDeviceNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
        _canAddToProject = canAddToProject;
        _locationService = MachineBankLocationService.CreateDefault();
        _bankPath = string.IsNullOrWhiteSpace(initialBankPath)
            ? _locationService.Load()
            : Path.GetFullPath(initialBankPath);
        _repository = new MachineBankRepository(_bankPath);
        DialogThemeService.Apply(this, useLightTheme);
        TemplatesGrid.ItemsSource = _visibleRows;
        ApplyLanguage();
        RefreshBank();
    }

    public MachineTemplatePackage? SelectedPackageToAdd { get; private set; }

    public MachineInstanceOptions? SelectedInstanceOptions { get; private set; }

    public string CurrentBankPath => _bankPath;

    private string AllFilterLabel => L("Tous", "All");

    private void ApplyLanguage()
    {
        Title = L("Banque de machines", "Device bank");
        HeadingTextBlock.Text = Title;
        GithubBanksButton.Content = L("Banques GitHub", "GitHub banks");
        MigrateBankButton.Content = L("Migrer une copie", "Migrate a copy");
        ChangeBankButton.Content = L("Changer de banque", "Change bank");
        OpenBankFolderButton.Content = L("Ouvrir le dossier", "Open folder");
        SearchLabel.Content = L("Recherche", "Search");
        ManufacturerFilterLabel.Content = L("Fabricant", "Manufacturer");
        CategoryFilterLabel.Content = L("Catégorie", "Category");
        MinimumTxLabel.Content = L("TX min.", "Min. Tx");
        MinimumRxLabel.Content = L("RX min.", "Min. Rx");
        TemplateNameColumn.Header = L("Modèle de banque", "Bank template");
        ManufacturerColumn.Header = L("Fabricant", "Manufacturer");
        ModelColumn.Header = L("Matériel", "Hardware model");
        CategoryColumn.Header = L("Catégorie", "Category");
        NoImageTextBlock.Text = L("Aucune image", "No image");
        LabelsPreviewHeadingTextBlock.Text = L("Aperçu des labels", "Label preview");
        AddToProjectButton.Content = L("Ajouter au projet", "Add to project");
        EditTemplateButton.Content = L("Modifier", "Edit");
        DuplicateTemplateButton.Content = L("Dupliquer le modèle", "Duplicate template");
        DeleteTemplateButton.Content = L("Supprimer", "Delete");
        ImportTemplateButton.Content = L("Importer un modèle", "Import template");
        ExportTemplateButton.Content = L("Exporter le modèle", "Export template");
        BackupBankButton.Content = L("Exporter la banque", "Export bank");
        RestoreBankButton.Content = L("Importer une banque", "Import bank");
        CloseButton.Content = L("Fermer", "Close");
        AddToProjectButton.IsEnabled = _canAddToProject;
        AddToProjectButton.ToolTip = _canAddToProject
            ? L(
                "Ajoute une nouvelle instance indépendante au projet ouvert.",
                "Adds a new independent instance to the open project.")
            : L(
                "Ouvrez un projet et activez l'édition pour ajouter une machine.",
                "Open a project and enable editing to add a device.");
        ChangeBankButton.ToolTip = L(
            "Choisir un autre dossier de banque, local, partagé ou synchronisé.",
            "Choose another local, shared or synchronized bank folder.");
        GithubBanksButton.ToolTip = L(
            "Ouvre le catalogue public de banques DCE sur GitHub pour télécharger ou proposer une banque.",
            "Opens the public DCE bank catalog on GitHub to download or submit a bank.");
        MigrateBankButton.ToolTip = L(
            "Crée une banque au format 2026.1 dans un nouveau dossier, avec sauvegarde, sans modifier cette banque V3.6.",
            "Creates a 2026.1-format bank in a new folder with a backup, without modifying this V3.6 bank.");
        AutomationProperties.SetName(
            MigrateBankButton,
            MigrateBankButton.Content?.ToString() ?? string.Empty);
        AutomationProperties.SetHelpText(
            MigrateBankButton,
            MigrateBankButton.ToolTip?.ToString() ?? string.Empty);
        OpenBankFolderButton.ToolTip = L(
            "Ouvre le dossier actuellement utilisé pour stocker la banque.",
            "Opens the folder currently used to store the bank.");
        SearchTextBox.ToolTip = L(
            "Recherche dans le nom, le fabricant, le modèle, la catégorie, la description et les mots-clés.",
            "Searches names, manufacturers, models, categories, descriptions and keywords.");
        ManufacturerFilterComboBox.ToolTip = L(
            "Limite la liste à un fabricant.",
            "Limits the list to one manufacturer.");
        CategoryFilterComboBox.ToolTip = L(
            "Limite la liste à une catégorie.",
            "Limits the list to one category.");
        MinimumTxTextBox.ToolTip = L(
            "Nombre minimal de canaux TX que doit contenir le modèle.",
            "Minimum number of Tx channels required in the template.");
        MinimumRxTextBox.ToolTip = L(
            "Nombre minimal de canaux RX que doit contenir le modèle.",
            "Minimum number of Rx channels required in the template.");
        TemplatesGrid.ToolTip = L(
            "Sélectionnez un modèle pour afficher son image, ses informations et un aperçu de ses labels.",
            "Select a template to display its image, details and a label preview.");
        EditTemplateButton.ToolTip = L(
            "Modifie les informations et labels du modèle sélectionné.",
            "Edits the selected template's details and labels.");
        DuplicateTemplateButton.ToolTip = L(
            "Crée une nouvelle fiche de banque indépendante à partir du modèle sélectionné.",
            "Creates an independent bank entry from the selected template.");
        DeleteTemplateButton.ToolTip = L(
            "Supprime le modèle sélectionné après confirmation.",
            "Deletes the selected template after confirmation.");
        ImportTemplateButton.ToolTip = L(
            "Importe une archive de modèle DCE dans cette banque.",
            "Imports a DCE template archive into this bank.");
        ExportTemplateButton.ToolTip = L(
            "Exporte le modèle sélectionné dans une archive partageable.",
            "Exports the selected template to a shareable archive.");
        BackupBankButton.ToolTip = L(
            "Exporte toute la banque dans une archive vérifiée et partageable *.dce-bank.zip.",
            "Exports the complete bank as a verified, shareable *.dce-bank.zip archive.");
        RestoreBankButton.ToolTip = L(
            "Installe une banque téléchargée dans un nouveau dossier ou un dossier vide, sans écraser l'existant.",
            "Installs a downloaded bank into a new or empty folder without overwriting existing data.");
        CloseButton.ToolTip = L("Ferme la banque de machines.", "Closes the device bank.");
    }

    private void RefreshBank(Guid? selectTemplateId = null)
    {
        try
        {
            _allTemplates = _repository.List();
            int formatVersion = _repository.GetFormatVersion();
            BankPathTextBlock.Text = $"{_bankPath}  ·  format {formatVersion}";
            MigrateBankButton.Visibility = formatVersion
                < MachineBankMigrationService.CurrentBankFormatVersion
                ? Visibility.Visible
                : Visibility.Collapsed;
            RefreshFilterSources();
            ApplyFilters(selectTemplateId);
        }
        catch (Exception ex)
        {
            DiagnosticLogService.Default.Write(
                "MachineBank",
                $"Impossible de lire la banque {_bankPath}.",
                ex);
            _allTemplates = [];
            _visibleRows.Clear();
            ClearDetails();
            ShowError(L("Banque illisible", "Unreadable bank"), ex);
        }
    }

    private void RefreshFilterSources()
    {
        _updatingFilters = true;
        try
        {
            string selectedManufacturer = ManufacturerFilterComboBox.SelectedItem as string
                ?? AllFilterLabel;
            string selectedCategory = CategoryFilterComboBox.SelectedItem as string
                ?? AllFilterLabel;
            ManufacturerFilterComboBox.ItemsSource = new[] { AllFilterLabel }
                .Concat(_allTemplates
                    .Select(item => item.Manufacturer)
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(value => value, StringComparer.OrdinalIgnoreCase))
                .ToArray();
            CategoryFilterComboBox.ItemsSource = new[] { AllFilterLabel }
                .Concat(_allTemplates
                    .Select(item =>
                        MachineTemplateLocalizationService.Category(item, _language))
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(value => value, StringComparer.OrdinalIgnoreCase))
                .ToArray();
            ManufacturerFilterComboBox.SelectedItem =
                ManufacturerFilterComboBox.Items.Contains(selectedManufacturer)
                    ? selectedManufacturer
                    : AllFilterLabel;
            CategoryFilterComboBox.SelectedItem =
                CategoryFilterComboBox.Items.Contains(selectedCategory)
                    ? selectedCategory
                    : AllFilterLabel;
        }
        finally
        {
            _updatingFilters = false;
        }
    }

    private void FilterChanged(object sender, EventArgs e)
    {
        if (!_updatingFilters && TemplatesGrid is not null)
        {
            ApplyFilters();
        }
    }

    private void ApplyFilters(Guid? selectTemplateId = null)
    {
        string search = SearchTextBox.Text.Trim();
        string manufacturer = ManufacturerFilterComboBox.SelectedItem as string
            ?? AllFilterLabel;
        string category = CategoryFilterComboBox.SelectedItem as string
            ?? AllFilterLabel;
        int minimumTx = ParseMinimum(MinimumTxTextBox.Text);
        int minimumRx = ParseMinimum(MinimumRxTextBox.Text);
        MachineBankRow[] rows = _allTemplates
            .Where(item => manufacturer == AllFilterLabel
                || string.Equals(item.Manufacturer, manufacturer, StringComparison.OrdinalIgnoreCase))
            .Where(item => category == AllFilterLabel
                || string.Equals(
                    MachineTemplateLocalizationService.Category(item, _language),
                    category,
                    StringComparison.OrdinalIgnoreCase))
            .Where(item => item.TxCount >= minimumTx && item.RxCount >= minimumRx)
            .Where(item => string.IsNullOrWhiteSpace(search)
                || Contains(item.TemplateName, search)
                || Contains(item.Manufacturer, search)
                || Contains(item.Model, search)
                || Contains(item.Description, search)
                || Contains(
                    MachineTemplateLocalizationService.Description(item, _language),
                    search)
                || item.Tags.Any(tag => Contains(tag, search)))
            .Select(item => new MachineBankRow(
                item,
                MachineTemplateLocalizationService.Category(item, _language)))
            .ToArray();
        _visibleRows.Clear();
        foreach (MachineBankRow row in rows)
        {
            _visibleRows.Add(row);
        }

        MachineBankRow? selected = selectTemplateId.HasValue
            ? rows.FirstOrDefault(row => row.TemplateId == selectTemplateId.Value)
            : rows.FirstOrDefault();
        TemplatesGrid.SelectedItem = selected;
        if (selected is null)
        {
            ClearDetails();
        }
    }

    private void TemplatesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RefreshSelectedDetails();
    }

    private void RefreshSelectedDetails()
    {
        MachineBankRow? row = SelectedRow();
        SetSelectionButtons(row is not null);
        if (row is null)
        {
            ClearDetails();
            return;
        }

        try
        {
            MachineTemplatePackage package = _repository.Load(row.TemplateId);
            MachineTemplateMetadata metadata = package.Metadata;
            SelectedTemplateNameTextBlock.Text = metadata.TemplateName;
            SelectedHardwareTextBlock.Text = string.Join(
                " · ",
                new[]
                {
                    metadata.Manufacturer,
                    metadata.Model,
                    MachineTemplateLocalizationService.Category(metadata, _language)
                }
                    .Where(value => !string.IsNullOrWhiteSpace(value)));
            SelectedCountsTextBlock.Text =
                $"{metadata.TxCount} TX / {metadata.RxCount} RX · preset {Blank(metadata.SourcePresetVersion)}";
            SelectedDescriptionTextBlock.Text =
                MachineTemplateLocalizationService.Description(metadata, _language);
            SelectedTagsTextBlock.Text = metadata.Tags.Count == 0
                ? string.Empty
                : "Tags: " + string.Join(", ", metadata.Tags);
            string[] tx = ReadLabels(package, "txchannel");
            string[] rx = ReadLabels(package, "rxchannel");
            LabelsPreviewTextBlock.Text = BuildLabelPreview(tx, rx);
            LoadImage(package.ImagePath);
        }
        catch (Exception ex)
        {
            DiagnosticLogService.Default.Write(
                "MachineBank",
                $"Le modèle {row.TemplateId:D} est illisible.",
                ex);
            ClearDetails();
            ShowError(L("Modèle illisible", "Unreadable template"), ex);
        }
    }

    private void AddToProjectButton_Click(object sender, RoutedEventArgs e)
    {
        MachineBankRow? row = RequireSelection();
        if (row is null || !_canAddToProject)
        {
            return;
        }

        try
        {
            MachineTemplatePackage package = _repository.Load(row.TemplateId);
            string suggestedName = BuildSuggestedDeviceName(package.Metadata);
            MachineInstanceWindow window = new(
                _language,
                _useLightTheme,
                package.Metadata,
                suggestedName)
            {
                Owner = this
            };
            if (window.ShowDialog() != true || window.Options is null)
            {
                return;
            }

            SelectedPackageToAdd = package;
            SelectedInstanceOptions = window.Options;
            DialogResult = true;
        }
        catch (Exception ex)
        {
            ShowError(L("Ajout impossible", "Unable to add device"), ex);
        }
    }

    private void EditTemplateButton_Click(object sender, RoutedEventArgs e)
    {
        MachineBankRow? row = RequireSelection();
        if (row is null)
        {
            return;
        }

        try
        {
            MachineTemplatePackage package = _repository.Load(row.TemplateId);
            MachineTemplateEditorWindow window = BuildEditor(
                package,
                L("Modifier le modèle", "Edit template"),
                L(
                    "Les modifications concernent uniquement la banque, jamais les machines déjà ajoutées aux projets.",
                    "Changes only affect the bank, never devices already added to projects."),
                allowImageRemoval: true);
            if (window.ShowDialog() != true || window.Result is null)
            {
                return;
            }

            MachineTemplatePackage edited = MachineTemplateService.Update(
                package,
                BuildEditRequest(window.Result));
            MachineTemplateMetadata saved = _repository.Update(edited);
            RefreshBank(saved.TemplateId);
        }
        catch (Exception ex)
        {
            ShowError(L("Modification impossible", "Unable to edit template"), ex);
        }
    }

    private void DuplicateTemplateButton_Click(object sender, RoutedEventArgs e)
    {
        MachineBankRow? row = RequireSelection();
        if (row is null)
        {
            return;
        }

        try
        {
            MachineTemplatePackage package = _repository.Load(row.TemplateId);
            MachineTemplateEditorWindow window = BuildEditor(
                package,
                L("Dupliquer le modèle", "Duplicate template"),
                L(
                    "Une copie indépendante sera ajoutée à la banque.",
                    "An independent copy will be added to the bank."),
                allowImageRemoval: false,
                templateName: package.Metadata.TemplateName + L(" copie", " copy"));
            if (window.ShowDialog() != true || window.Result is null)
            {
                return;
            }

            MachineTemplatePackage duplicate = MachineTemplateService.Duplicate(
                package,
                BuildEditRequest(window.Result));
            MachineTemplateMetadata saved = _repository.Save(duplicate);
            RefreshBank(saved.TemplateId);
        }
        catch (Exception ex)
        {
            ShowError(L("Duplication impossible", "Unable to duplicate template"), ex);
        }
    }

    private void DeleteTemplateButton_Click(object sender, RoutedEventArgs e)
    {
        MachineBankRow? row = RequireSelection();
        if (row is null)
        {
            return;
        }

        MessageBoxResult confirm = MessageBox.Show(
            this,
            L(
                $"Supprimer « {row.TemplateName} » de la banque ? Une copie récupérable sera conservée dans Backups.",
                $"Delete “{row.TemplateName}” from the bank? A recoverable copy will remain in Backups."),
            L("Confirmer la suppression", "Confirm deletion"),
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (confirm != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            _repository.Delete(row.TemplateId);
            RefreshBank();
        }
        catch (Exception ex)
        {
            ShowError(L("Suppression impossible", "Unable to delete template"), ex);
        }
    }

    private void ImportTemplateButton_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog dialog = new()
        {
            Filter = L(
                "Modèle DCE (*.dce-machine.zip)|*.dce-machine.zip|Archives ZIP (*.zip)|*.zip",
                "DCE template (*.dce-machine.zip)|*.dce-machine.zip|ZIP archives (*.zip)|*.zip"),
            Title = L("Importer un modèle", "Import template")
        };
        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        try
        {
            MachineTemplateMetadata imported = _repository.Import(dialog.FileName);
            RefreshBank(imported.TemplateId);
        }
        catch (Exception ex)
        {
            ShowError(L("Import impossible", "Unable to import template"), ex);
        }
    }

    private void ExportTemplateButton_Click(object sender, RoutedEventArgs e)
    {
        MachineBankRow? row = RequireSelection();
        if (row is null)
        {
            return;
        }

        SaveFileDialog dialog = new()
        {
            Filter = "DCE machine template (*.dce-machine.zip)|*.dce-machine.zip",
            Title = L("Exporter le modèle", "Export template"),
            FileName = SafeFileName(row.TemplateName) + ".dce-machine.zip"
        };
        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        try
        {
            _repository.Export(row.TemplateId, dialog.FileName);
        }
        catch (Exception ex)
        {
            ShowError(L("Export impossible", "Unable to export template"), ex);
        }
    }

    private void BackupBankButton_Click(object sender, RoutedEventArgs e)
    {
        SaveFileDialog dialog = new()
        {
            Filter = "DCE machine bank (*.dce-bank.zip)|*.dce-bank.zip",
            Title = L("Exporter la banque", "Export bank"),
            FileName = $"DCE_MachineBank_{DateTime.Now:yyyyMMdd}.dce-bank.zip"
        };
        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        try
        {
            MachineBankArchiveService.ExportBank(_bankPath, dialog.FileName);
        }
        catch (Exception ex)
        {
            ShowError(L("Export impossible", "Unable to export bank"), ex);
        }
    }

    private void RestoreBankButton_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog archiveDialog = new()
        {
            Filter = "DCE machine bank (*.dce-bank.zip)|*.dce-bank.zip",
            Title = L("Choisir la banque téléchargée", "Choose downloaded bank")
        };
        if (archiveDialog.ShowDialog(this) != true)
        {
            return;
        }

        OpenFolderDialog folderDialog = new()
        {
            Title = L(
                "Choisir un dossier neuf ou vide pour restaurer la banque",
                "Choose a new or empty folder for the imported bank"),
            Multiselect = false
        };
        if (folderDialog.ShowDialog(this) != true)
        {
            return;
        }

        try
        {
            string restored = MachineBankArchiveService.RestoreBank(
                archiveDialog.FileName,
                folderDialog.FolderName);
            SwitchBank(restored);
        }
        catch (Exception ex)
        {
            ShowError(L("Import impossible", "Unable to import bank"), ex);
        }
    }

    private void GithubBanksButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo(MachineBankDistributionService.GitHubBanksUrl)
            {
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            ShowError(L("Ouverture impossible", "Unable to open GitHub"), ex);
        }
    }

    private void MigrateBankButton_Click(object sender, RoutedEventArgs e)
    {
        OpenFolderDialog dialog = new()
        {
            Title = L(
                "Choisir un dossier neuf ou vide pour la copie 2026.1",
                "Choose a new or empty folder for the 2026.1 copy"),
            InitialDirectory = Path.GetDirectoryName(_bankPath)
                ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            Multiselect = false
        };
        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        try
        {
            MachineBankV2MigrationResult result =
                new MachineBankV2MigrationService().Migrate(
                    _bankPath,
                    dialog.FolderName);
            SwitchBank(result.DestinationBankPath);
            MessageBox.Show(
                this,
                L(
                    $"La copie 2026.1 est prête. La banque V3.6 est intacte.{Environment.NewLine}{Environment.NewLine}Sauvegarde : {result.BackupArchivePath}",
                    $"The 2026.1 copy is ready. The V3.6 bank is unchanged.{Environment.NewLine}{Environment.NewLine}Backup: {result.BackupArchivePath}"),
                L("Migration terminée", "Migration complete"),
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            ShowError(L("Migration impossible", "Unable to migrate bank"), ex);
        }
    }

    private void ChangeBankButton_Click(object sender, RoutedEventArgs e)
    {
        OpenFolderDialog dialog = new()
        {
            Title = L(
                "Choisir le dossier de la banque de machines",
                "Choose the device bank folder"),
            InitialDirectory = Directory.Exists(_bankPath)
                ? _bankPath
                : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            Multiselect = false
        };
        if (dialog.ShowDialog(this) == true)
        {
            SwitchBank(dialog.FolderName);
        }
    }

    private void OpenBankFolderButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Directory.CreateDirectory(_bankPath);
            Process.Start(new ProcessStartInfo(_bankPath)
            {
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            ShowError(L("Ouverture impossible", "Unable to open folder"), ex);
        }
    }

    private void SwitchBank(string path)
    {
        try
        {
            string fullPath = Path.GetFullPath(path);
            _locationService.Save(fullPath);
            _bankPath = fullPath;
            _repository = new MachineBankRepository(_bankPath);
            RefreshBank();
        }
        catch (Exception ex)
        {
            ShowError(L("Changement impossible", "Unable to change bank"), ex);
        }
    }

    private MachineTemplateEditorWindow BuildEditor(
        MachineTemplatePackage package,
        string title,
        string intro,
        bool allowImageRemoval,
        string? templateName = null)
    {
        MachineTemplateMetadata metadata = package.Metadata;
        return new MachineTemplateEditorWindow(
            _language,
            _useLightTheme,
            title,
            intro,
            templateName ?? metadata.TemplateName,
            metadata.Manufacturer,
            metadata.Model,
            metadata.Description,
            metadata.Category,
            metadata.Tags,
            ReadLabels(package, "txchannel"),
            ReadLabels(package, "rxchannel"),
            package.ImagePath,
            allowImageRemoval)
        {
            Owner = this
        };
    }

    private static MachineTemplateEditRequest BuildEditRequest(
        MachineTemplateFormResult form)
    {
        return new MachineTemplateEditRequest
        {
            TemplateName = form.TemplateName,
            Manufacturer = form.Manufacturer,
            Model = form.Model,
            Description = form.Description,
            Category = form.Category,
            Tags = form.Tags,
            TxLabels = form.TxLabels,
            RxLabels = form.RxLabels,
            ImageSourcePath = form.ImageSourcePath,
            RemoveImage = form.RemoveImage
        };
    }

    private string BuildSuggestedDeviceName(MachineTemplateMetadata metadata)
    {
        string clean = DanteNameRules.NormalizeDeviceNamePart(
            string.IsNullOrWhiteSpace(metadata.Model)
                ? metadata.TemplateName
                : metadata.Model,
            "DEVICE");
        clean = clean[..Math.Min(clean.Length, DanteNameRules.MaximumNameLength)]
            .Trim('-');
        if (!_usedDeviceNames.Contains(clean)
            && DanteNameRules.ValidateDeviceName(clean) is null)
        {
            return clean;
        }

        return DanteNameRules.BuildUniqueSuffixedDeviceName(
            clean,
            "2",
            _usedDeviceNames);
    }

    private static string[] ReadLabels(
        MachineTemplatePackage package,
        string channelElementName)
    {
        return package.TemplateDocument.Root!.Elements()
            .Where(element => element.Name.LocalName == channelElementName)
            .Select(channel => channel.Elements()
                .FirstOrDefault(element => element.Name.LocalName is "label" or "name" or "channel_name")?
                .Value
                ?? channel.Attributes()
                    .FirstOrDefault(attribute => attribute.Name.LocalName is "label" or "name" or "channel_name")?
                    .Value
                ?? string.Empty)
            .ToArray();
    }

    private string BuildLabelPreview(
        IReadOnlyList<string> txLabels,
        IReadOnlyList<string> rxLabels)
    {
        const int maximumPerDirection = 12;
        List<string> lines = [];
        lines.Add("TX");
        lines.AddRange(txLabels
            .Take(maximumPerDirection)
            .Select((label, index) => $"{index + 1:000}  {label}"));
        if (txLabels.Count > maximumPerDirection)
        {
            lines.Add(L(
                $"… {txLabels.Count - maximumPerDirection} autre(s)",
                $"… {txLabels.Count - maximumPerDirection} more"));
        }

        lines.Add(string.Empty);
        lines.Add("RX");
        lines.AddRange(rxLabels
            .Take(maximumPerDirection)
            .Select((label, index) => $"{index + 1:000}  {label}"));
        if (rxLabels.Count > maximumPerDirection)
        {
            lines.Add(L(
                $"… {rxLabels.Count - maximumPerDirection} autre(s)",
                $"… {rxLabels.Count - maximumPerDirection} more"));
        }

        return string.Join(Environment.NewLine, lines);
    }

    private void LoadImage(string? path)
    {
        TemplateImage.Source = null;
        NoImageTextBlock.Visibility = Visibility.Visible;
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return;
        }

        try
        {
            BitmapImage image = new();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.DecodePixelWidth = 500;
            image.UriSource = new Uri(Path.GetFullPath(path), UriKind.Absolute);
            image.EndInit();
            image.Freeze();
            TemplateImage.Source = image;
            NoImageTextBlock.Visibility = Visibility.Collapsed;
        }
        catch
        {
            NoImageTextBlock.Text = L("Image illisible", "Unreadable image");
        }
    }

    private void ClearDetails()
    {
        SelectedTemplateNameTextBlock.Text = string.Empty;
        SelectedHardwareTextBlock.Text = string.Empty;
        SelectedCountsTextBlock.Text = string.Empty;
        SelectedDescriptionTextBlock.Text = string.Empty;
        SelectedTagsTextBlock.Text = string.Empty;
        LabelsPreviewTextBlock.Text = string.Empty;
        TemplateImage.Source = null;
        NoImageTextBlock.Visibility = Visibility.Visible;
        SetSelectionButtons(false);
    }

    private void SetSelectionButtons(bool hasSelection)
    {
        AddToProjectButton.IsEnabled = hasSelection && _canAddToProject;
        EditTemplateButton.IsEnabled = hasSelection;
        DuplicateTemplateButton.IsEnabled = hasSelection;
        DeleteTemplateButton.IsEnabled = hasSelection;
        ExportTemplateButton.IsEnabled = hasSelection;
    }

    private MachineBankRow? SelectedRow()
    {
        return TemplatesGrid.SelectedItem as MachineBankRow;
    }

    private MachineBankRow? RequireSelection()
    {
        MachineBankRow? row = SelectedRow();
        if (row is null)
        {
            MessageBox.Show(
                this,
                L("Sélectionnez d'abord un modèle.", "Select a template first."),
                Title,
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        return row;
    }

    private void ShowError(string title, Exception exception)
    {
        DiagnosticLogService.Default.Write("MachineBank", title, exception);
        MessageBox.Show(
            this,
            exception.Message,
            title,
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }

    private static int ParseMinimum(string value)
    {
        return int.TryParse(value, out int parsed) && parsed > 0 ? parsed : 0;
    }

    private static bool Contains(string value, string search)
    {
        return value.Contains(search, StringComparison.OrdinalIgnoreCase);
    }

    private static string SafeFileName(string value)
    {
        string clean = string.Join(
            "_",
            value.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
        return string.IsNullOrWhiteSpace(clean) ? "machine" : clean;
    }

    private static string Blank(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "?" : value;
    }

    private string L(string french, string english)
    {
        return _language == UiLanguage.English ? english : french;
    }
}

public sealed class MachineBankRow
{
    public MachineBankRow(
        MachineTemplateMetadata metadata,
        string displayCategory)
    {
        Metadata = metadata;
        Category = displayCategory;
    }

    public MachineTemplateMetadata Metadata { get; }

    public Guid TemplateId => Metadata.TemplateId;

    public string TemplateName => Metadata.TemplateName;

    public string Manufacturer => Metadata.Manufacturer;

    public string Model => Metadata.Model;

    public string Category { get; }

    public int TxCount => Metadata.TxCount;

    public int RxCount => Metadata.RxCount;
}
