using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using DanteConfigEditor.Models;
using DanteConfigEditor.Services;

namespace DanteConfigEditor.Mac;

internal sealed record MacMachineBankSelection(
    MachineTemplatePackage Package,
    MachineInstanceOptions Options);

internal sealed partial class MachineBankDialog : Window
{
    private static readonly FilePickerFileType TemplateArchiveType = new("DCE machine template")
    {
        Patterns = ["*.dce-machine.zip", "*.zip"]
    };

    private static readonly FilePickerFileType BankArchiveType = new("DCE machine bank")
    {
        Patterns = ["*.dce-bank.zip", "*.zip"]
    };

    private readonly ObservableCollection<MacMachineBankRow> _visibleRows = [];
    private readonly MachineBankLocationService _locationService = MachineBankLocationService.CreateDefault();
    private UiLanguage _language;
    private HashSet<string> _usedDeviceNames = new(StringComparer.OrdinalIgnoreCase);
    private bool _canAddToProject;
    private bool _updatingFilters;
    private string _bankPath = string.Empty;
    private MachineBankRepository? _repository;
    private IReadOnlyList<MachineTemplateMetadata> _allTemplates = [];

    public MachineBankDialog()
    {
        InitializeComponent();
    }

    private T? FindControl<T>(string name) where T : Control =>
        ControlExtensions.FindControl<T>(this, name);

    private MachineBankRepository Repository =>
        _repository ?? throw new InvalidOperationException("La banque n'est pas initialisée.");

    private string AllFilterLabel => L("Tous", "All");

    public static Task<MacMachineBankSelection?> ShowAsync(
        Window owner,
        UiLanguage language,
        IEnumerable<string> usedDeviceNames,
        bool canAddToProject)
    {
        MachineBankDialog dialog = new()
        {
            _language = language,
            _usedDeviceNames = usedDeviceNames.ToHashSet(StringComparer.OrdinalIgnoreCase),
            _canAddToProject = canAddToProject,
            Title = language == UiLanguage.English ? "Device bank" : "Banque de machines"
        };
        dialog._bankPath = dialog._locationService.Load();
        dialog._repository = new MachineBankRepository(dialog._bankPath);
        dialog.FindControl<DataGrid>("TemplatesGrid")!.ItemsSource = dialog._visibleRows;
        dialog.ApplyLanguage();
        dialog.RefreshBank();
        return dialog.ShowDialog<MacMachineBankSelection?>(owner);
    }

    private void ApplyLanguage()
    {
        FindControl<TextBlock>("HeadingText")!.Text = Title;
        Button githubBanksButton = FindControl<Button>("GithubBanksButton")!;
        githubBanksButton.Content = L("Banques GitHub", "GitHub banks");
        FindControl<Button>("ChangeBankButton")!.Content = L("Changer de banque", "Change bank");
        FindControl<Button>("OpenBankFolderButton")!.Content = L("Ouvrir le dossier", "Open folder");
        FindControl<TextBlock>("SearchLabel")!.Text = L("Recherche", "Search");
        FindControl<TextBlock>("ManufacturerFilterLabel")!.Text = L("Fabricant", "Manufacturer");
        FindControl<TextBlock>("CategoryFilterLabel")!.Text = L("Catégorie", "Category");
        FindControl<TextBlock>("MinimumTxLabel")!.Text = L("TX min.", "Min. Tx");
        FindControl<TextBlock>("MinimumRxLabel")!.Text = L("RX min.", "Min. Rx");
        DataGrid grid = FindControl<DataGrid>("TemplatesGrid")!;
        grid.Columns[0].Header = L("Modèle de banque", "Bank template");
        grid.Columns[1].Header = L("Fabricant", "Manufacturer");
        grid.Columns[2].Header = L("Matériel", "Hardware model");
        grid.Columns[3].Header = L("Catégorie", "Category");
        FindControl<TextBlock>("NoImageText")!.Text = L("Aucune image", "No image");
        FindControl<TextBlock>("LabelsPreviewHeadingText")!.Text = L("Aperçu des labels", "Label preview");
        FindControl<Button>("ImportTemplateButton")!.Content = L("Importer un modèle", "Import template");
        FindControl<Button>("ExportTemplateButton")!.Content = L("Exporter le modèle", "Export template");
        FindControl<Button>("BackupBankButton")!.Content = L("Exporter la banque", "Export bank");
        FindControl<Button>("RestoreBankButton")!.Content = L("Importer une banque", "Import bank");
        FindControl<Button>("EditTemplateButton")!.Content = L("Modifier", "Edit");
        FindControl<Button>("DuplicateTemplateButton")!.Content = L("Dupliquer le modèle", "Duplicate template");
        FindControl<Button>("DeleteTemplateButton")!.Content = L("Supprimer", "Delete");
        FindControl<Button>("AddToProjectButton")!.Content = L("Ajouter au projet", "Add to project");
        FindControl<Button>("CloseButton")!.Content = L("Fermer", "Close");
        ToolTip.SetTip(
            githubBanksButton,
            L(
                "Ouvre le catalogue public de banques DCE sur GitHub.",
                "Opens the public DCE bank catalog on GitHub."));
        ToolTip.SetTip(
            FindControl<Button>("BackupBankButton")!,
            L(
                "Exporte toute la banque dans une archive vérifiée et partageable *.dce-bank.zip.",
                "Exports the complete bank as a verified, shareable *.dce-bank.zip archive."));
        ToolTip.SetTip(
            FindControl<Button>("RestoreBankButton")!,
            L(
                "Installe une banque téléchargée dans un dossier neuf ou vide, sans écraser l'existant.",
                "Installs a downloaded bank into a new or empty folder without overwriting existing data."));
    }

    private void RefreshBank(Guid? selectTemplateId = null)
    {
        try
        {
            _allTemplates = Repository.List();
            FindControl<TextBlock>("BankPathText")!.Text = _bankPath;
            RefreshFilterSources();
            ApplyFilters(selectTemplateId);
        }
        catch (Exception exception)
        {
            Log("Impossible de lire la banque.", exception);
            _allTemplates = [];
            _visibleRows.Clear();
            ClearDetails();
            _ = ShowErrorAsync(L("Banque illisible", "Unreadable bank"), exception);
        }
    }

    private void RefreshFilterSources()
    {
        _updatingFilters = true;
        try
        {
            ComboBox manufacturerCombo = FindControl<ComboBox>("ManufacturerFilterComboBox")!;
            ComboBox categoryCombo = FindControl<ComboBox>("CategoryFilterComboBox")!;
            string selectedManufacturer = manufacturerCombo.SelectedItem as string ?? AllFilterLabel;
            string selectedCategory = categoryCombo.SelectedItem as string ?? AllFilterLabel;
            string[] manufacturers =
            [
                AllFilterLabel,
                .. _allTemplates
                    .Select(item => item.Manufacturer)
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            ];
            string[] categories =
            [
                AllFilterLabel,
                .. _allTemplates
                    .Select(item =>
                        MachineTemplateLocalizationService.Category(item, _language))
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            ];
            manufacturerCombo.ItemsSource = manufacturers;
            categoryCombo.ItemsSource = categories;
            manufacturerCombo.SelectedItem = manufacturers.Contains(selectedManufacturer)
                ? selectedManufacturer
                : AllFilterLabel;
            categoryCombo.SelectedItem = categories.Contains(selectedCategory)
                ? selectedCategory
                : AllFilterLabel;
        }
        finally
        {
            _updatingFilters = false;
        }
    }

    private void FilterTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (!_updatingFilters)
        {
            ApplyFilters();
        }
    }

    private void FilterSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!_updatingFilters)
        {
            ApplyFilters();
        }
    }

    private void ApplyFilters(Guid? selectTemplateId = null)
    {
        string search = FindControl<TextBox>("SearchTextBox")!.Text?.Trim() ?? string.Empty;
        string manufacturer = FindControl<ComboBox>("ManufacturerFilterComboBox")!.SelectedItem as string
            ?? AllFilterLabel;
        string category = FindControl<ComboBox>("CategoryFilterComboBox")!.SelectedItem as string
            ?? AllFilterLabel;
        int minimumTx = ParseMinimum(FindControl<TextBox>("MinimumTxTextBox")!.Text);
        int minimumRx = ParseMinimum(FindControl<TextBox>("MinimumRxTextBox")!.Text);
        MacMachineBankRow[] rows = _allTemplates
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
            .Select(item => new MacMachineBankRow(
                item,
                MachineTemplateLocalizationService.Category(item, _language)))
            .ToArray();

        _visibleRows.Clear();
        foreach (MacMachineBankRow row in rows)
        {
            _visibleRows.Add(row);
        }

        DataGrid grid = FindControl<DataGrid>("TemplatesGrid")!;
        grid.SelectedItem = selectTemplateId.HasValue
            ? rows.FirstOrDefault(row => row.TemplateId == selectTemplateId.Value)
            : rows.FirstOrDefault();
        if (grid.SelectedItem is null)
        {
            ClearDetails();
        }
    }

    private void TemplatesGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        RefreshSelectedDetails();
    }

    private void RefreshSelectedDetails()
    {
        MacMachineBankRow? row = SelectedRow();
        SetSelectionButtons(row is not null);
        if (row is null)
        {
            ClearDetails();
            return;
        }

        try
        {
            MachineTemplatePackage package = Repository.Load(row.TemplateId);
            MachineTemplateMetadata metadata = package.Metadata;
            FindControl<TextBlock>("SelectedTemplateNameText")!.Text = metadata.TemplateName;
            FindControl<TextBlock>("SelectedHardwareText")!.Text = string.Join(
                " · ",
                new[]
                {
                    metadata.Manufacturer,
                    metadata.Model,
                    MachineTemplateLocalizationService.Category(metadata, _language)
                }
                    .Where(value => !string.IsNullOrWhiteSpace(value)));
            FindControl<TextBlock>("SelectedCountsText")!.Text =
                $"{metadata.TxCount} TX / {metadata.RxCount} RX · preset {Blank(metadata.SourcePresetVersion)}";
            FindControl<TextBlock>("SelectedDescriptionText")!.Text =
                MachineTemplateLocalizationService.Description(metadata, _language);
            FindControl<TextBlock>("SelectedTagsText")!.Text = metadata.Tags.Count == 0
                ? string.Empty
                : "Tags: " + string.Join(", ", metadata.Tags);
            FindControl<TextBox>("LabelsPreviewTextBox")!.Text = BuildLabelPreview(
                ReadLabels(package, "txchannel"),
                ReadLabels(package, "rxchannel"));
            LoadImage(package.ImagePath);
        }
        catch (Exception exception)
        {
            Log($"Le modèle {row.TemplateId:D} est illisible.", exception);
            ClearDetails();
            _ = ShowErrorAsync(L("Modèle illisible", "Unreadable template"), exception);
        }
    }

    private async void AddToProjectButton_Click(object? sender, RoutedEventArgs e)
    {
        MacMachineBankRow? row = await RequireSelectionAsync();
        if (row is null || !_canAddToProject)
        {
            return;
        }

        try
        {
            MachineTemplatePackage package = Repository.Load(row.TemplateId);
            MachineInstanceOptions? options = await MachineInstanceDialog.ShowAsync(
                this,
                _language,
                package.Metadata,
                BuildSuggestedDeviceName(package.Metadata));
            if (options is not null)
            {
                Close(new MacMachineBankSelection(package, options));
            }
        }
        catch (Exception exception)
        {
            await ShowErrorAsync(L("Ajout impossible", "Unable to add device"), exception);
        }
    }

    private async void EditTemplateButton_Click(object? sender, RoutedEventArgs e)
    {
        MacMachineBankRow? row = await RequireSelectionAsync();
        if (row is null)
        {
            return;
        }

        try
        {
            MachineTemplatePackage package = Repository.Load(row.TemplateId);
            MacMachineTemplateFormResult? form = await OpenEditorAsync(
                package,
                L("Modifier le modèle", "Edit template"),
                L(
                    "Les modifications concernent uniquement la banque, jamais les machines déjà ajoutées aux projets.",
                    "Changes only affect the bank, never devices already added to projects."),
                allowImageRemoval: true);
            if (form is null)
            {
                return;
            }

            MachineTemplateMetadata saved = Repository.Update(
                MachineTemplateService.Update(package, BuildEditRequest(form)));
            RefreshBank(saved.TemplateId);
        }
        catch (Exception exception)
        {
            await ShowErrorAsync(L("Modification impossible", "Unable to edit template"), exception);
        }
    }

    private async void DuplicateTemplateButton_Click(object? sender, RoutedEventArgs e)
    {
        MacMachineBankRow? row = await RequireSelectionAsync();
        if (row is null)
        {
            return;
        }

        try
        {
            MachineTemplatePackage package = Repository.Load(row.TemplateId);
            MacMachineTemplateFormResult? form = await OpenEditorAsync(
                package,
                L("Dupliquer le modèle", "Duplicate template"),
                L("Une copie indépendante sera ajoutée à la banque.", "An independent copy will be added to the bank."),
                allowImageRemoval: false,
                package.Metadata.TemplateName + L(" copie", " copy"));
            if (form is null)
            {
                return;
            }

            MachineTemplateMetadata saved = Repository.Save(
                MachineTemplateService.Duplicate(package, BuildEditRequest(form)));
            RefreshBank(saved.TemplateId);
        }
        catch (Exception exception)
        {
            await ShowErrorAsync(L("Duplication impossible", "Unable to duplicate template"), exception);
        }
    }

    private async void DeleteTemplateButton_Click(object? sender, RoutedEventArgs e)
    {
        MacMachineBankRow? row = await RequireSelectionAsync();
        if (row is null)
        {
            return;
        }

        bool confirmed = await MessageDialog.ShowAsync(
            this,
            L("Confirmer la suppression", "Confirm deletion"),
            L(
                $"Supprimer « {row.TemplateName} » de la banque ? Une copie récupérable sera conservée dans Backups.",
                $"Delete “{row.TemplateName}” from the bank? A recoverable copy will remain in Backups."),
            L("Supprimer", "Delete"),
            L("Annuler", "Cancel"));
        if (!confirmed)
        {
            return;
        }

        try
        {
            Repository.Delete(row.TemplateId);
            RefreshBank();
        }
        catch (Exception exception)
        {
            await ShowErrorAsync(L("Suppression impossible", "Unable to delete template"), exception);
        }
    }

    private async void ImportTemplateButton_Click(object? sender, RoutedEventArgs e)
    {
        string? path = await PickOpenFileAsync(
            L("Importer un modèle", "Import template"),
            TemplateArchiveType);
        if (path is null)
        {
            return;
        }

        try
        {
            MachineTemplateMetadata imported = Repository.Import(path);
            RefreshBank(imported.TemplateId);
        }
        catch (Exception exception)
        {
            await ShowErrorAsync(L("Import impossible", "Unable to import template"), exception);
        }
    }

    private async void ExportTemplateButton_Click(object? sender, RoutedEventArgs e)
    {
        MacMachineBankRow? row = await RequireSelectionAsync();
        if (row is null)
        {
            return;
        }

        string? path = await PickSaveFileAsync(
            SafeFileName(row.TemplateName) + ".dce-machine.zip",
            "dce-machine.zip",
            TemplateArchiveType);
        if (path is null)
        {
            return;
        }

        try
        {
            Repository.Export(row.TemplateId, path);
        }
        catch (Exception exception)
        {
            await ShowErrorAsync(L("Export impossible", "Unable to export template"), exception);
        }
    }

    private async void BackupBankButton_Click(object? sender, RoutedEventArgs e)
    {
        string? path = await PickSaveFileAsync(
            $"DCE_MachineBank_{DateTime.Now:yyyyMMdd}.dce-bank.zip",
            "dce-bank.zip",
            BankArchiveType);
        if (path is null)
        {
            return;
        }

        try
        {
            MachineBankArchiveService.ExportBank(_bankPath, path);
        }
        catch (Exception exception)
        {
            await ShowErrorAsync(L("Export impossible", "Unable to export bank"), exception);
        }
    }

    private async void RestoreBankButton_Click(object? sender, RoutedEventArgs e)
    {
        string? archive = await PickOpenFileAsync(
            L("Choisir la banque téléchargée", "Choose downloaded bank"),
            BankArchiveType);
        if (archive is null)
        {
            return;
        }

        string? folder = await PickFolderAsync(L(
            "Choisir un dossier neuf ou vide pour restaurer la banque",
            "Choose a new or empty folder for the restored bank"));
        if (folder is null)
        {
            return;
        }

        try
        {
            SwitchBank(MachineBankArchiveService.RestoreBank(archive, folder));
        }
        catch (Exception exception)
        {
            await ShowErrorAsync(L("Import impossible", "Unable to import bank"), exception);
        }
    }

    private async void GithubBanksButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo(MachineBankDistributionService.GitHubBanksUrl)
            {
                UseShellExecute = true
            });
        }
        catch (Exception exception)
        {
            await ShowErrorAsync(L("Ouverture impossible", "Unable to open GitHub"), exception);
        }
    }

    private async void ChangeBankButton_Click(object? sender, RoutedEventArgs e)
    {
        string? folder = await PickFolderAsync(L(
            "Choisir le dossier de la banque de machines",
            "Choose the device bank folder"));
        if (folder is not null)
        {
            SwitchBank(folder);
        }
    }

    private async void OpenBankFolderButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            Directory.CreateDirectory(_bankPath);
            Process.Start(new ProcessStartInfo(_bankPath) { UseShellExecute = true });
        }
        catch (Exception exception)
        {
            await ShowErrorAsync(L("Ouverture impossible", "Unable to open folder"), exception);
        }
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        Close(null);
    }

    private void SwitchBank(string path)
    {
        try
        {
            string fullPath = Path.GetFullPath(path);
            _locationService.Save(fullPath);
            _bankPath = fullPath;
            _repository = new MachineBankRepository(fullPath);
            RefreshBank();
        }
        catch (Exception exception)
        {
            _ = ShowErrorAsync(L("Changement impossible", "Unable to change bank"), exception);
        }
    }

    private Task<MacMachineTemplateFormResult?> OpenEditorAsync(
        MachineTemplatePackage package,
        string title,
        string intro,
        bool allowImageRemoval,
        string? templateName = null)
    {
        MachineTemplateMetadata metadata = package.Metadata;
        return MachineTemplateEditorDialog.ShowAsync(
            this,
            _language,
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
            allowImageRemoval);
    }

    private static MachineTemplateEditRequest BuildEditRequest(MacMachineTemplateFormResult form)
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
        clean = clean[..Math.Min(clean.Length, DanteNameRules.MaximumNameLength)].Trim('-');
        if (!_usedDeviceNames.Contains(clean) && DanteNameRules.ValidateDeviceName(clean) is null)
        {
            return clean;
        }

        return DanteNameRules.BuildUniqueSuffixedDeviceName(clean, "2", _usedDeviceNames);
    }

    private static string[] ReadLabels(MachineTemplatePackage package, string channelElementName)
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

    private string BuildLabelPreview(IReadOnlyList<string> txLabels, IReadOnlyList<string> rxLabels)
    {
        const int maximum = 12;
        List<string> lines = ["TX"];
        lines.AddRange(txLabels.Take(maximum).Select((label, index) => $"{index + 1:000}  {label}"));
        if (txLabels.Count > maximum)
        {
            lines.Add(L($"… {txLabels.Count - maximum} autre(s)", $"… {txLabels.Count - maximum} more"));
        }

        lines.Add(string.Empty);
        lines.Add("RX");
        lines.AddRange(rxLabels.Take(maximum).Select((label, index) => $"{index + 1:000}  {label}"));
        if (rxLabels.Count > maximum)
        {
            lines.Add(L($"… {rxLabels.Count - maximum} autre(s)", $"… {rxLabels.Count - maximum} more"));
        }

        return string.Join(Environment.NewLine, lines);
    }

    private void LoadImage(string? path)
    {
        Image image = FindControl<Image>("TemplateImage")!;
        TextBlock noImage = FindControl<TextBlock>("NoImageText")!;
        image.Source = null;
        noImage.IsVisible = true;
        noImage.Text = L("Aucune image", "No image");
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return;
        }

        try
        {
            using FileStream stream = File.OpenRead(path);
            image.Source = new Bitmap(stream);
            noImage.IsVisible = false;
        }
        catch
        {
            noImage.Text = L("Image illisible", "Unreadable image");
        }
    }

    private void ClearDetails()
    {
        FindControl<TextBlock>("SelectedTemplateNameText")!.Text = string.Empty;
        FindControl<TextBlock>("SelectedHardwareText")!.Text = string.Empty;
        FindControl<TextBlock>("SelectedCountsText")!.Text = string.Empty;
        FindControl<TextBlock>("SelectedDescriptionText")!.Text = string.Empty;
        FindControl<TextBlock>("SelectedTagsText")!.Text = string.Empty;
        FindControl<TextBox>("LabelsPreviewTextBox")!.Text = string.Empty;
        FindControl<Image>("TemplateImage")!.Source = null;
        FindControl<TextBlock>("NoImageText")!.IsVisible = true;
        SetSelectionButtons(false);
    }

    private void SetSelectionButtons(bool hasSelection)
    {
        FindControl<Button>("AddToProjectButton")!.IsEnabled = hasSelection && _canAddToProject;
        FindControl<Button>("EditTemplateButton")!.IsEnabled = hasSelection;
        FindControl<Button>("DuplicateTemplateButton")!.IsEnabled = hasSelection;
        FindControl<Button>("DeleteTemplateButton")!.IsEnabled = hasSelection;
        FindControl<Button>("ExportTemplateButton")!.IsEnabled = hasSelection;
    }

    private MacMachineBankRow? SelectedRow() =>
        FindControl<DataGrid>("TemplatesGrid")!.SelectedItem as MacMachineBankRow;

    private async Task<MacMachineBankRow?> RequireSelectionAsync()
    {
        MacMachineBankRow? row = SelectedRow();
        if (row is null)
        {
            await MessageDialog.ShowInfoAsync(
                this,
                Title ?? L("Banque de machines", "Device bank"),
                L("Sélectionnez d'abord un modèle.", "Select a template first."),
                "OK");
        }

        return row;
    }

    private async Task<string?> PickOpenFileAsync(string title, FilePickerFileType fileType)
    {
        IReadOnlyList<IStorageFile> files = await StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
                FileTypeFilter = [fileType]
            });
        return files.FirstOrDefault()?.TryGetLocalPath();
    }

    private async Task<string?> PickSaveFileAsync(
        string suggestedName,
        string extension,
        FilePickerFileType fileType)
    {
        IStorageFile? file = await StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                Title = L("Exporter", "Export"),
                SuggestedFileName = suggestedName,
                DefaultExtension = extension,
                ShowOverwritePrompt = true,
                FileTypeChoices = [fileType]
            });
        return file?.TryGetLocalPath();
    }

    private async Task<string?> PickFolderAsync(string title)
    {
        IReadOnlyList<IStorageFolder> folders = await StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                Title = title,
                AllowMultiple = false
            });
        return folders.FirstOrDefault()?.TryGetLocalPath();
    }

    private Task<bool> ShowErrorAsync(string title, Exception exception)
    {
        Log(title, exception);
        return MessageDialog.ShowInfoAsync(this, title, exception.Message, "OK");
    }

    private static void Log(string message, Exception exception)
    {
        DiagnosticLogService.Default.Write("MachineBank", message, exception);
    }

    private static int ParseMinimum(string? value) =>
        int.TryParse(value, out int parsed) && parsed > 0 ? parsed : 0;

    private static bool Contains(string value, string search) =>
        value.Contains(search, StringComparison.OrdinalIgnoreCase);

    private static string SafeFileName(string value)
    {
        string clean = string.Join(
            "_",
            value.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
        return string.IsNullOrWhiteSpace(clean) ? "machine" : clean;
    }

    private static string Blank(string value) =>
        string.IsNullOrWhiteSpace(value) ? "?" : value;

    private string L(string french, string english) =>
        _language == UiLanguage.English ? english : french;
}

internal sealed class MacMachineBankRow
{
    public MacMachineBankRow(
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
