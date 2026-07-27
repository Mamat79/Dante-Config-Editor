using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using Avalonia.Threading;
using DanteConfigEditor.Models;
using DanteConfigEditor.Infrastructure.Migration;
using DanteConfigEditor.Services;

namespace DanteConfigEditor.Mac;

public partial class MainWindow : Window
{
    private static readonly FilePickerFileType XmlFileType = new("Dante XML")
    {
        Patterns = ["*.xml"],
        AppleUniformTypeIdentifiers = ["public.xml"],
        MimeTypes = ["application/xml", "text/xml"]
    };

    private static readonly FilePickerFileType ChannelLabelFileType = new("Channel labels")
    {
        Patterns = ["*.json", "*.csv", "*.xlsx", "*.ods", "*.zip"]
    };

    private static readonly FilePickerFileType DmtWorkbookFileType = new("DMT workbook")
    {
        Patterns = ["*.xlsx", "*.ods"]
    };

    private static readonly FilePickerFileType AllenHeathCsvFileType = new("Allen & Heath console CSV")
    {
        Patterns = ["*.csv"]
    };

    private static readonly FilePickerFileType YamahaLabelFileType = new("Yamaha CL/QL label package")
    {
        Patterns = ["*.zip", "*.csv"]
    };

    private readonly DispatcherTimer _recoveryTimer;
    private readonly SupportReminderSettingsService _supportReminderSettings = new();
    private CancellationTokenSource? _recoveryCancellation;
    private DanteProject? _project;
    private UiLanguage _language;
    private bool _darkTheme;
    private bool _editEnabled;
    private bool _initializing = true;

    public MainWindow()
    {
        InitializeComponent();

        MigrateV36Settings();
        _language = LanguageSettingsService.Load();
        _darkTheme = LoadThemePreference();
        _recoveryTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(750) };
        _recoveryTimer.Tick += RecoveryTimer_Tick;

        Opened += MainWindow_Opened;
        Closing += MainWindow_Closing;
        ConfigureChoiceLists();
        RefreshRecentFiles();
        ApplyTheme();
        RefreshAll();
        SessionRecoveryService.CleanupOld(TimeSpan.FromDays(14));
    }

    private static void MigrateV36Settings()
    {
        try
        {
            _ = V36SettingsMigrationService.CreateDefault().Migrate();
        }
        catch (Exception ex) when (ex is IOException
                                   or UnauthorizedAccessException
                                   or InvalidDataException)
        {
            DiagnosticLogService.Default.Write(
                "Migration",
                "V3.6 settings could not be copied to the 2026.1 profile.",
                ex);
        }
    }

    private T? FindControl<T>(string name) where T : Control => ControlExtensions.FindControl<T>(this, name);

    public async Task OpenStartupFileAsync(string path)
    {
        if (File.Exists(path))
        {
            await LoadProjectAsync(path);
        }
    }

    private void MainWindow_Opened(object? sender, EventArgs e)
    {
        _initializing = true;
        FindControl<ComboBox>("LanguageCombo")!.SelectedIndex = _language == UiLanguage.English ? 1 : 0;
        ApplyLanguageToVisualTree();
        _initializing = false;
        InitializeSupportReminder();
    }

    private void MainWindow_Closing(object? sender, WindowClosingEventArgs e)
    {
        _recoveryTimer.Stop();
        _recoveryCancellation?.Cancel();
        if (_project?.IsModified == true)
        {
            try
            {
                SessionRecoveryService.Save(_project);
            }
            catch
            {
                // La fermeture ne doit pas être bloquée si le cache local est indisponible.
            }
        }
    }

    private async void OpenButton_Click(object? sender, RoutedEventArgs e)
    {
        string? path = await PickOpenPathAsync(L("Ouvrir une configuration Dante", "Open a Dante configuration"));
        if (path is not null)
        {
            await LoadProjectAsync(path);
        }
    }

    private async void NewProjectButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_project?.IsModified == true
            && !await ConfirmAsync(
                L("Changements non enregistrés", "Unsaved changes"),
                L(
                    "Le projet courant contient des changements non enregistrés. Créer un autre projet sans les sauvegarder ?",
                    "The current project has unsaved changes. Create another project without saving them?"),
                L("Continuer", "Continue")))
        {
            return;
        }

        MacNewProjectFormResult? form = await NewProjectDialog.ShowAsync(this, _language);
        if (form is null)
        {
            return;
        }

        try
        {
            DanteProject created;
            if (form.TemplateId.HasValue)
            {
                MachineTemplatePackage template =
                    new MachineBankRepository(form.BankPath).Load(form.TemplateId.Value);
                created = DanteProject.CreateNewFromTemplate(
                    form.DestinationPath,
                    form.ProjectName,
                    form.Description,
                    template,
                    form.TemplateOptions
                        ?? throw new InvalidOperationException(
                            "Les options d'instance du modèle sont absentes."));
            }
            else
            {
                created = DanteProject.CreateNew(
                    form.DestinationPath,
                    new NewProjectOptions
                    {
                        ProjectName = form.ProjectName,
                        Description = form.Description,
                        Machines =
                        [
                            new NewCustomMachineDefinition
                            {
                                Name = form.DeviceName,
                                TxCount = form.TxCount,
                                RxCount = form.RxCount,
                                SampleRate = form.SampleRate,
                                Encoding = form.Encoding,
                                UnicastLatency = form.UnicastLatency
                            }
                        ]
                    });
            }

            created.SaveAs(form.DestinationPath);
            _project = created;
            _editEnabled = true;
            RecentFilesService.Add(form.DestinationPath);
            RefreshRecentFiles();
            RefreshAll(form.DeviceName);
            SetStatus(L(
                "Projet expérimental créé. Validez-le dans Dante Controller.",
                "Experimental project created. Validate it in Dante Controller."));
            await ShowInfoAsync(
                L("Validation manuelle obligatoire", "Manual validation required"),
                L(
                    "Le XML a été créé puis rechargé par DCE. Cela valide seulement sa structure interne. "
                    + "Importez-le dans Dante Controller avant tout usage en production.",
                    "The XML was created and reloaded by DCE. This validates its internal structure only. "
                    + "Import it in Dante Controller before production use."));
        }
        catch (Exception exception)
        {
            await ShowErrorAsync(
                L("Création du projet impossible", "Unable to create project"),
                exception);
        }
    }

    private async void OpenMachineBankButton_Click(object? sender, RoutedEventArgs e)
    {
        MacMachineBankSelection? selection = await MachineBankDialog.ShowAsync(
            this,
            _language,
            _project?.Devices.Select(device => device.Name) ?? [],
            _project is not null && _editEnabled);
        if (selection is null || _project is null)
        {
            return;
        }

        await ExecuteMutationAsync(
            L(
                $"Machine ajoutée depuis la banque : {selection.Options.NewName}",
                $"Device added from bank: {selection.Options.NewName}"),
            L("Machine ajoutée depuis la banque.", "Device added from bank."),
            project => project.AddDeviceFromTemplate(selection.Package, selection.Options),
            selection.Options.NewName);
    }

    private async void OpenRecentButton_Click(object? sender, RoutedEventArgs e)
    {
        string? path = FindControl<ComboBox>("RecentCombo")!.SelectedItem as string;
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            await ShowInfoAsync(
                L("Fichier introuvable", "File not found"),
                L("Sélectionnez un fichier récent encore disponible.", "Select a recent file that is still available."));
            RefreshRecentFiles();
            return;
        }

        await LoadProjectAsync(path);
    }

    private async Task LoadProjectAsync(string path)
    {
        try
        {
            DanteProject loadedProject;
            RecoveryCandidate? recovery = SessionRecoveryService.Find(path);
            if (recovery is not null)
            {
                bool restore = await ConfirmAsync(
                    LocalizationService.Text(_language, "Dialog.RecoveryTitle"),
                    LocalizationService.Format(_language, "Dialog.RecoveryFound", recovery.SavedAtUtc.ToLocalTime()),
                    L("Récupérer", "Recover"));
                if (restore)
                {
                    loadedProject = DanteProject.LoadRecovered(path, recovery.RecoveryXmlPath);
                }
                else
                {
                    SessionRecoveryService.Delete(path);
                    loadedProject = DanteProject.Load(path);
                }
            }
            else
            {
                loadedProject = DanteProject.Load(path);
            }

            _project = loadedProject;
            _editEnabled = false;
            RecentFilesService.Add(path);
            RefreshRecentFiles();
            RefreshAll();
            SetStatus(LocalizationService.Text(_language, "Status.FileLoaded"));
        }
        catch (Exception exception)
        {
            await ShowErrorAsync(LocalizationService.Text(_language, "Dialog.OpenFailedTitle"), exception);
        }
    }

    private async void MergeButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!await EnsureEditableAsync())
        {
            return;
        }

        string? path = await PickOpenPathAsync(L("Ajouter un XML au projet", "Add XML to project"));
        if (path is null)
        {
            return;
        }

        try
        {
            IReadOnlyList<string> duplicates = _project!.FindDuplicateDeviceNamesInXml(path);
            IReadOnlyDictionary<string, string>? renameMap = null;
            if (duplicates.Count > 0)
            {
                DuplicateMergeDialogResult? result = await DuplicateMergeDialog.ShowAsync(
                    this,
                    duplicates,
                    suffix => _project.BuildAutomaticDuplicateRenameMap(path, suffix),
                    _language);
                if (result is null)
                {
                    return;
                }

                renameMap = result.RenameMap;
            }
            else if (!await ConfirmAsync(
                         L("Ajouter au projet", "Add to project"),
                         LocalizationService.Text(_language, "Dialog.MergeXmlWarning"),
                         L("Ajouter", "Add")))
            {
                return;
            }

            _project.PushUndoSnapshot(L("Import XML", "XML import"));
            DanteMergeResult mergeResult = _project.MergeDevicesFromXml(path, renameMap);
            RefreshAll();
            ScheduleRecovery();
            SetStatus(L(
                $"{mergeResult.ImportedDeviceCount} machine(s) importée(s), {mergeResult.RenamedDeviceCount} renommée(s).",
                $"{mergeResult.ImportedDeviceCount} device(s) imported, {mergeResult.RenamedDeviceCount} renamed."));
        }
        catch (Exception exception)
        {
            await RestoreSnapshotAfterFailureAsync(L("Import impossible", "Import failed"), exception);
        }
    }

    private async void SaveButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_project is null)
        {
            await ShowInfoAsync(
                LocalizationService.Text(_language, "Dialog.NoFileLoadedTitle"),
                LocalizationService.Text(_language, "Dialog.NoFileLoadedMessage"));
            return;
        }

        string suggestedPath = SafeFileService.BuildDefaultSavePath(_project.OriginalFilePath);
        IStorageFile? destination = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = LocalizationService.Text(_language, "Dialog.SaveXmlTitle"),
            SuggestedFileName = Path.GetFileName(suggestedPath),
            DefaultExtension = "xml",
            FileTypeChoices = [XmlFileType],
            ShowOverwritePrompt = true
        });
        string? path = destination?.TryGetLocalPath();
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        if (string.Equals(Path.GetFullPath(path), Path.GetFullPath(_project.OriginalFilePath), StringComparison.OrdinalIgnoreCase))
        {
            await ShowInfoAsync(
                LocalizationService.Text(_language, "Dialog.ChooseAnotherNameTitle"),
                LocalizationService.Text(_language, "Dialog.ChooseAnotherNameMessage"));
            return;
        }

        try
        {
            string previousReference = _project.OriginalFilePath;
            string backup = _project.SaveAs(path);
            SessionRecoveryService.Delete(previousReference);
            SessionRecoveryService.Delete(_project.OriginalFilePath);
            RecentFilesService.Add(_project.OriginalFilePath);
            RefreshRecentFiles();
            RefreshAll();
            SetStatus(L($"Fichier sauvegardé. Backup : {backup}", $"File saved. Backup: {backup}"));
        }
        catch (Exception exception)
        {
            await ShowErrorAsync(LocalizationService.Text(_language, "Dialog.SaveErrorTitle"), exception);
        }
    }

    private void EditButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_project is null)
        {
            return;
        }

        _editEnabled = true;
        UpdateUiState();
        SetStatus(LocalizationService.Text(_language, "Status.EditEnabled"));
    }

    private async void UndoButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!await EnsureEditableAsync() || _project?.CanUndo != true)
        {
            return;
        }

        try
        {
            string label = _project.UndoLastChange();
            RefreshAll();
            ScheduleRecovery();
            SetStatus(L($"Action annulée : {label}", $"Action undone: {label}"));
        }
        catch (Exception exception)
        {
            await ShowErrorAsync(LocalizationService.Text(_language, "Dialog.UndoErrorTitle"), exception);
        }
    }

    private async void RevertButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_project is null)
        {
            return;
        }

        if (!await ConfirmAsync(
                LocalizationService.Text(_language, "Dialog.RevertTitle"),
                LocalizationService.Text(_language, "Dialog.RevertMessage"),
                L("Recharger", "Reload")))
        {
            return;
        }

        string path = _project.OriginalFilePath;
        try
        {
            SessionRecoveryService.Delete(path);
            _project = DanteProject.Load(path);
            _editEnabled = false;
            RefreshAll();
            SetStatus(L("Changements annulés.", "Changes reverted."));
        }
        catch (Exception exception)
        {
            await ShowErrorAsync(LocalizationService.Text(_language, "Dialog.ReloadErrorTitle"), exception);
        }
    }

    private void DeviceGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        PopulateSelectedDevicePanel();
    }

    private async void DeviceGrid_DoubleTapped(object? sender, TappedEventArgs e)
    {
        await OpenDeviceDetailsAsync();
    }

    private async void ApplyDeviceButton_Click(object? sender, RoutedEventArgs e)
    {
        DeviceRow? row = SelectedDeviceRow();
        if (row is null || !await EnsureEditableAsync())
        {
            return;
        }

        string oldName = row.Name;
        string newName = FindControl<TextBox>("DeviceNameTextBox")!.Text?.Trim() ?? string.Empty;
        string network = SelectedValue("NetworkModeCombo");
        string latency = SelectedValue("LatencyCombo");
        string sampleRate = SelectedValue("SampleRateCombo");
        string bits = SelectedValue("BitsCombo");
        string ipMode = SelectedValue("IpModeCombo");
        bool preferred = FindControl<CheckBox>("PreferredCheckBox")!.IsChecked == true;

        DanteDevice device = _project!.FindDevice(oldName)!;
        bool hasAudioOrIpChange = !string.Equals(device.Samplerate, sampleRate, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(device.Encoding, bits, StringComparison.OrdinalIgnoreCase)
            || (ipMode == "static" && !device.UsesStaticIp)
            || (ipMode == "auto" && device.UsesStaticIp);
        if (hasAudioOrIpChange && !await ConfirmAsync(
                L("Paramètres Dante", "Dante settings"),
                L(
                    "Les formats audio ou l'adresse IP vont changer. Vérifiez le XML final dans Dante Controller.",
                    "Audio formats or the IP address will change. Verify the final XML in Dante Controller."),
                L("Appliquer", "Apply")))
        {
            return;
        }

        await ExecuteMutationAsync(
            L("Paramètres machine", "Device settings"),
            LocalizationService.Text(_language, "Action.DeviceSettingsUpdated"),
            project =>
            {
                string currentName = oldName;
                project.ApplyBatch(batch =>
                {
                    if (!string.Equals(oldName, newName, StringComparison.Ordinal))
                    {
                        batch.RenameDevice(oldName, newName);
                        currentName = newName;
                    }

                    DanteDevice current = batch.FindDevice(currentName)!;
                    if (current.IsRedundant != (network == "redundant"))
                    {
                        batch.SetNetworkMode(currentName, network == "redundant");
                    }

                    if (!string.Equals(current.Latency, latency, StringComparison.OrdinalIgnoreCase))
                    {
                        batch.SetLatency(currentName, latency);
                    }

                    if (!string.Equals(current.Samplerate, sampleRate, StringComparison.OrdinalIgnoreCase))
                    {
                        batch.SetSamplerate(currentName, sampleRate);
                    }

                    if (!string.Equals(current.Encoding, bits, StringComparison.OrdinalIgnoreCase))
                    {
                        batch.SetEncoding(currentName, bits);
                    }

                    if (current.PreferredMaster != preferred)
                    {
                        batch.SetPreferredMaster(currentName, preferred);
                    }

                    if (ipMode == "auto" && current.UsesStaticIp)
                    {
                        batch.SetIpAddressDynamic(currentName);
                    }
                    else if (ipMode == "static")
                    {
                        string address = FindControl<TextBox>("IpAddressTextBox")!.Text ?? string.Empty;
                        string netmask = FindControl<TextBox>("NetmaskTextBox")!.Text ?? string.Empty;
                        string gateway = FindControl<TextBox>("GatewayTextBox")!.Text ?? string.Empty;
                        if (!current.UsesStaticIp
                            || !string.Equals(current.StaticIpAddress, address, StringComparison.Ordinal)
                            || !string.Equals(current.StaticIpNetmask, netmask, StringComparison.Ordinal)
                            || !string.Equals(current.StaticIpGateway, gateway, StringComparison.Ordinal))
                        {
                            batch.SetIpAddressStatic(currentName, address, netmask, gateway);
                        }
                    }
                });
            },
            string.IsNullOrWhiteSpace(newName) ? oldName : newName);
    }

    private async void DeviceDetailsButton_Click(object? sender, RoutedEventArgs e)
    {
        await OpenDeviceDetailsAsync();
    }

    private async void DuplicateDeviceButton_Click(object? sender, RoutedEventArgs e)
    {
        DeviceRow? row = SelectedDeviceRow();
        if (row is null || !await EnsureEditableAsync() || _project is null)
        {
            return;
        }

        DanteDevice source = _project.FindDevice(row.Name)
            ?? throw new InvalidOperationException("La machine source est introuvable.");
        HashSet<string> usedNames = _project.Devices
            .Select(device => device.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        string suggestedName = DanteNameRules.BuildUniqueSuffixedDeviceName(
            source.Name,
            L("Copie", "Copy"),
            usedNames);
        MachineCloneOptions? options = await MachineCloneDialog.ShowAsync(
            this,
            _language,
            source.Name,
            suggestedName);
        if (options is null)
        {
            return;
        }

        await ExecuteMutationAsync(
            L(
                $"Machine dupliquée : {source.Name} -> {options.NewName}",
                $"Device duplicated: {source.Name} -> {options.NewName}"),
            L("Machine dupliquée.", "Device duplicated."),
            project => project.DuplicateDevice(source.Name, options),
            options.NewName);
    }

    private async void SaveDeviceToBankButton_Click(object? sender, RoutedEventArgs e)
    {
        DeviceRow? row = SelectedDeviceRow();
        DanteDevice? device = row is null ? null : _project?.FindDevice(row.Name);
        if (device is null || _project is null)
        {
            await ShowInfoAsync(
                L("Aucune machine", "No device"),
                L("Sélectionnez d'abord une machine.", "Select a device first."));
            return;
        }

        string manufacturer = device.Manufacturer;
        string model = device.Model;
        string suggestedTemplateName = string.Join(
            " ",
            new[] { manufacturer, model }.Where(value => !string.IsNullOrWhiteSpace(value)));
        if (string.IsNullOrWhiteSpace(suggestedTemplateName))
        {
            suggestedTemplateName = device.Name;
        }

        MacMachineTemplateFormResult? form = await MachineTemplateEditorDialog.ShowAsync(
            this,
            _language,
            L("Enregistrer dans la banque de machines", "Save to device bank"),
            L(
                $"Préparez un modèle réutilisable à partir de {device.Name}. L'identité matérielle, le réseau, les subscriptions et les flows multicast seront retirés.",
                $"Prepare a reusable template from {device.Name}. Hardware identity, network addresses, subscriptions and multicast flows will be removed."),
            suggestedTemplateName,
            manufacturer,
            model,
            string.Empty,
            string.Empty,
            [],
            device.TxChannels.Select(channel => channel.DisplayName),
            device.RxChannels.Select(channel => channel.DisplayName));
        if (form is null)
        {
            return;
        }

        try
        {
            MachineTemplatePackage package = MachineTemplateService.CreateFromDevice(
                device,
                _project.PresetVersion,
                new MachineTemplateCreateRequest
                {
                    TemplateName = form.TemplateName,
                    Manufacturer = form.Manufacturer,
                    Model = form.Model,
                    Description = form.Description,
                    Category = form.Category,
                    Tags = form.Tags,
                    TxLabels = form.TxLabels,
                    RxLabels = form.RxLabels,
                    ImageSourcePath = form.ImageSourcePath
                });
            string bankPath = MachineBankLocationService.CreateDefault().Load();
            MachineTemplateMetadata saved = new MachineBankRepository(bankPath).Save(package);
            SetStatus(L("Modèle enregistré dans la banque.", "Device template saved."));
            await ShowInfoAsync(
                L("Banque de machines", "Device bank"),
                L(
                    $"« {saved.TemplateName} » a été enregistré dans :{Environment.NewLine}{bankPath}",
                    $"“{saved.TemplateName}” was saved in:{Environment.NewLine}{bankPath}"));
        }
        catch (Exception exception)
        {
            await ShowErrorAsync(
                L("Enregistrement du modèle impossible", "Unable to save template"),
                exception);
        }
    }

    private async Task OpenDeviceDetailsAsync()
    {
        DeviceRow? row = SelectedDeviceRow();
        if (row is null || _project is null)
        {
            return;
        }

        DanteDevice device = _project.FindDevice(row.Name)!;
        DeviceDetailsResult? result = await DeviceDetailsDialog.ShowAsync(this, device, _language, _editEnabled);
        if (result is null || !_editEnabled)
        {
            return;
        }

        await ExecuteMutationAsync(
            L("Détail machine", "Device details"),
            LocalizationService.Text(_language, "Action.DeviceDetailsUpdated"),
            project => ApplyDeviceDetails(project, device.Name, result),
            result.DeviceName);
    }

    private static void ApplyDeviceDetails(DanteProject project, string originalName, DeviceDetailsResult result)
    {
        string currentName = originalName;
        project.ApplyBatch(batch =>
        {
            if (!string.Equals(originalName, result.DeviceName, StringComparison.Ordinal))
            {
                batch.RenameDevice(originalName, result.DeviceName);
                currentName = result.DeviceName;
            }

            DanteDevice current = batch.FindDevice(currentName)!;
            if (current.IsRedundant != result.IsRedundant)
            {
                batch.SetNetworkMode(currentName, result.IsRedundant);
            }

            if (!string.Equals(current.Latency, result.Latency, StringComparison.OrdinalIgnoreCase))
            {
                batch.SetLatency(currentName, result.Latency);
            }

            if (!string.Equals(current.Samplerate, result.SampleRate, StringComparison.OrdinalIgnoreCase))
            {
                batch.SetSamplerate(currentName, result.SampleRate);
            }

            if (!string.Equals(current.Encoding, result.Bits, StringComparison.OrdinalIgnoreCase))
            {
                batch.SetEncoding(currentName, result.Bits);
            }

            if (current.PreferredMaster != result.PreferredMaster)
            {
                batch.SetPreferredMaster(currentName, result.PreferredMaster);
            }

            if (result.UseStaticIp)
            {
                batch.SetIpAddressStatic(currentName, result.IpAddress, result.Netmask, result.Gateway);
            }
            else if (current.UsesStaticIp)
            {
                batch.SetIpAddressDynamic(currentName);
            }

            foreach (EditableChannelRow channel in result.TxChannels.Where(channel => channel.IsChanged))
            {
                batch.RenameChannel(currentName, DanteChannelKind.Tx, channel.Index, channel.Name);
            }

            foreach (EditableChannelRow channel in result.RxChannels.Where(channel => channel.IsChanged))
            {
                batch.RenameChannel(currentName, DanteChannelKind.Rx, channel.Index, channel.Name);
            }
        });
    }

    private async void ResetRxTxButton_Click(object? sender, RoutedEventArgs e)
    {
        await ResetSelectedDevicePatchesAsync("all");
    }

    private async void ResetRxButton_Click(object? sender, RoutedEventArgs e)
    {
        await ResetSelectedDevicePatchesAsync("rx");
    }

    private async void ResetTxButton_Click(object? sender, RoutedEventArgs e)
    {
        await ResetSelectedDevicePatchesAsync("tx");
    }

    private async Task ResetSelectedDevicePatchesAsync(string scope)
    {
        DeviceRow? row = SelectedDeviceRow();
        if (row is null || !await EnsureEditableAsync())
        {
            return;
        }

        string warning = scope switch
        {
            "rx" => L("Toutes les entrées RX de cette machine seront déconnectées.", "All Rx inputs on this device will be disconnected."),
            "tx" => L("Tous les patchs utilisant les TX de cette machine seront supprimés.", "All subscriptions using this device's Tx channels will be removed."),
            _ => LocalizationService.Format(_language, "Dialog.ResetDevicePatchesWarning", row.Name)
        };
        if (!await ConfirmAsync(L("Réinitialiser les patchs", "Reset subscriptions"), warning, L("Réinitialiser", "Reset")))
        {
            return;
        }

        await ExecuteMutationAsync(
            L("Reset patch machine", "Reset device subscriptions"),
            LocalizationService.Text(_language, "Action.DevicePatchesReset"),
            project =>
            {
                if (scope == "rx") project.ResetDeviceRxPatches(row.Name);
                else if (scope == "tx") project.ResetDeviceTxPatches(row.Name);
                else project.ResetDevicePatches(row.Name);
            },
            row.Name);
    }

    private async void DeleteDeviceButton_Click(object? sender, RoutedEventArgs e)
    {
        DeviceRow? row = SelectedDeviceRow();
        if (row is null || !await EnsureEditableAsync())
        {
            return;
        }

        if (!await ConfirmAsync(
                L("Supprimer la machine", "Delete device"),
                LocalizationService.Format(_language, "Dialog.DeleteDeviceWarning", row.Name),
                L("Supprimer", "Delete")))
        {
            return;
        }

        await ExecuteMutationAsync(
            L("Suppression machine", "Delete device"),
            LocalizationService.Text(_language, "Action.DeviceDeleted"),
            project => project.DeleteDevice(row.Name));
    }

    private void ChannelKindCombo_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        RefreshChannelChoices();
    }

    private void ChannelCombo_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        PopulateSelectedChannelName();
    }

    private void PopulateSelectedChannelName()
    {
        ChoiceValue? choice = FindControl<ComboBox>("ChannelCombo")!.SelectedItem as ChoiceValue;
        DeviceRow? row = SelectedDeviceRow();
        if (choice is null || row is null || _project is null || !int.TryParse(choice.Value, out int index))
        {
            return;
        }

        DanteDevice device = _project.FindDevice(row.Name)!;
        DanteChannelKind kind = SelectedTag("ChannelKindCombo") == "rx" ? DanteChannelKind.Rx : DanteChannelKind.Tx;
        DanteChannel? channel = (kind == DanteChannelKind.Tx ? device.TxChannels : device.RxChannels)
            .FirstOrDefault(item => item.Index == index);
        FindControl<TextBox>("ChannelNameTextBox")!.Text = channel?.Name ?? string.Empty;
    }

    private async void RenameChannelButton_Click(object? sender, RoutedEventArgs e)
    {
        DeviceRow? device = SelectedDeviceRow();
        ChoiceValue? channel = FindControl<ComboBox>("ChannelCombo")!.SelectedItem as ChoiceValue;
        if (device is null || channel is null || !int.TryParse(channel.Value, out int index))
        {
            return;
        }

        DanteChannelKind kind = SelectedTag("ChannelKindCombo") == "rx" ? DanteChannelKind.Rx : DanteChannelKind.Tx;
        string newName = FindControl<TextBox>("ChannelNameTextBox")!.Text?.Trim() ?? string.Empty;
        await ExecuteMutationAsync(
            L("Renommage canal", "Rename channel"),
            LocalizationService.Text(_language, "Action.ChannelRenamed"),
            project => project.RenameChannel(device.Name, kind, index, newName),
            device.Name);
    }

    private async void BatchRenameButton_Click(object? sender, RoutedEventArgs e)
    {
        DeviceRow? device = SelectedDeviceRow();
        ChoiceValue? start = FindControl<ComboBox>("StartChannelCombo")!.SelectedItem as ChoiceValue;
        ChoiceValue? end = FindControl<ComboBox>("EndChannelCombo")!.SelectedItem as ChoiceValue;
        if (device is null || start is null || end is null
            || !int.TryParse(start.Value, out int startIndex)
            || !int.TryParse(end.Value, out int endIndex)
            || !int.TryParse(FindControl<TextBox>("BatchNumberTextBox")!.Text, out int firstNumber))
        {
            await ShowInfoAsync(
                LocalizationService.Text(_language, "Dialog.InvalidRangeTitle"),
                LocalizationService.Text(_language, "Dialog.InvalidRangeMessage"));
            return;
        }

        if (endIndex < startIndex)
        {
            await ShowInfoAsync(
                LocalizationService.Text(_language, "Dialog.InvalidRangeTitle"),
                LocalizationService.Text(_language, "Dialog.InvalidRangeOrderMessage"));
            return;
        }

        DanteChannelKind kind = SelectedTag("ChannelKindCombo") == "rx" ? DanteChannelKind.Rx : DanteChannelKind.Tx;
        string prefix = FindControl<TextBox>("BatchPrefixTextBox")!.Text?.Trim() ?? string.Empty;
        if (!await ConfirmAsync(
                L("Renommage en série", "Batch rename"),
                LocalizationService.Format(_language, "Dialog.BatchRenameWarning", kind, startIndex, endIndex),
                L("Renommer", "Rename")))
        {
            return;
        }

        await ExecuteMutationAsync(
            L("Renommage en série", "Batch rename"),
            LocalizationService.Text(_language, "Action.BatchRenameApplied"),
            project => project.BatchRenameChannels(device.Name, kind, prefix, firstNumber, startIndex, endIndex),
            device.Name);
    }

    private async void ResetDeviceChannelsButton_Click(object? sender, RoutedEventArgs e)
    {
        DeviceRow? device = SelectedDeviceRow();
        if (device is null || !await ConfirmAsync(
                L("Réinitialiser les canaux", "Reset channels"),
                LocalizationService.Text(_language, "Dialog.ResetDeviceChannelsWarning"),
                L("Réinitialiser", "Reset")))
        {
            return;
        }

        await ExecuteMutationAsync(
            L("Réinitialisation canaux", "Reset channels"),
            LocalizationService.Text(_language, "Action.ChannelsReset"),
            project => project.ResetChannels(device.Name),
            device.Name);
    }

    private async void ApplyGlobalNetworkButton_Click(object? sender, RoutedEventArgs e)
    {
        string value = SelectedValue("GlobalNetworkCombo");
        await ExecuteGlobalMutationAsync(
            L("Mode réseau global", "Global network mode"),
            LocalizationService.Text(_language, "Action.AllNetworkModesApplied"),
            project => project.SetAllNetworkModes(value == "redundant"));
    }

    private async void ApplyGlobalLatencyButton_Click(object? sender, RoutedEventArgs e)
    {
        string value = SelectedValue("GlobalLatencyCombo");
        await ExecuteGlobalMutationAsync(
            L("Latence globale", "Global latency"),
            LocalizationService.Text(_language, "Action.AllLatenciesApplied"),
            project => project.SetAllLatencies(value));
    }

    private async void ApplyGlobalSampleRateButton_Click(object? sender, RoutedEventArgs e)
    {
        string value = SelectedValue("GlobalSampleRateCombo");
        await ExecuteGlobalMutationAsync(
            L("Sample rate globale", "Global sample rate"),
            LocalizationService.Text(_language, "Action.AllSampleRatesApplied"),
            project => project.SetAllSamplerates(value));
    }

    private async void ApplyGlobalBitsButton_Click(object? sender, RoutedEventArgs e)
    {
        string value = SelectedValue("GlobalBitsCombo");
        await ExecuteGlobalMutationAsync(
            L("Bits globaux", "Global bits"),
            LocalizationService.Text(_language, "Action.AllEncodingsApplied"),
            project => project.SetAllEncodings(value));
    }

    private async void ResetAllChannelsButton_Click(object? sender, RoutedEventArgs e)
    {
        await ExecuteGlobalMutationAsync(
            L("Réinitialiser tous les canaux", "Reset all channels"),
            LocalizationService.Text(_language, "Action.AllChannelsReset"),
            project => project.ResetAllChannels());
    }

    private async void GlobalIpAutoButton_Click(object? sender, RoutedEventArgs e)
    {
        await ExecuteGlobalMutationAsync(
            L("IP automatiques", "Automatic IPs"),
            LocalizationService.Text(_language, "Action.AllIpAutoApplied"),
            project => project.SetAllIpAddressesDynamic());
    }

    private async void GlobalStaticIpButton_Click(object? sender, RoutedEventArgs e)
    {
        string prefix = FindControl<TextBox>("GlobalIpPrefixTextBox")!.Text ?? string.Empty;
        string netmask = FindControl<TextBox>("GlobalNetmaskTextBox")!.Text ?? string.Empty;
        string gateway = FindControl<TextBox>("GlobalGatewayTextBox")!.Text ?? string.Empty;
        if (!int.TryParse(FindControl<TextBox>("GlobalIpStartTextBox")!.Text, out int start))
        {
            await ShowInfoAsync(
                LocalizationService.Text(_language, "Dialog.InvalidNumberTitle"),
                LocalizationService.Text(_language, "Dialog.InvalidNumberMessage"));
            return;
        }

        await ExecuteGlobalMutationAsync(
            L("IP fixes en série", "Static IP range"),
            LocalizationService.Text(_language, "Action.AllIpStaticApplied"),
            project => project.SetAllIpAddressesStaticSequential(prefix, start, netmask, gateway));
    }

    private async void ApplyProfileButton_Click(object? sender, RoutedEventArgs e)
    {
        ChoiceValue? selected = FindControl<ComboBox>("ProfileCombo")!.SelectedItem as ChoiceValue;
        DeviceProfile? profile = DeviceProfileCatalog.BuiltIn.FirstOrDefault(item => item.Key == selected?.Value);
        if (profile is null)
        {
            return;
        }

        await ExecuteGlobalMutationAsync(
            L("Profil rapide", "Quick profile"),
            LocalizationService.Text(_language, "Action.QuickProfileApplied"),
            project => project.ApplyDeviceProfile(project.Devices.Select(device => device.Name), profile));
    }

    private async void ApplyExclusivePreferredMasterButton_Click(object? sender, RoutedEventArgs e)
    {
        string? deviceName = FindControl<ComboBox>("ExclusivePreferredMasterCombo")!.SelectedItem as string;
        if (string.IsNullOrWhiteSpace(deviceName))
        {
            await ShowInfoAsync(
                L("Action impossible", "Action unavailable"),
                L("Choisissez la machine qui doit rester Preferred Master.", "Select the device that must remain Preferred Master."));
            return;
        }

        await ExecuteGlobalMutationAsync(
            L("Preferred Master unique", "Single Preferred Master"),
            L($"{deviceName} est maintenant le seul Preferred Master.", $"{deviceName} is now the only Preferred Master."),
            project => project.SetExclusivePreferredMaster(deviceName));
    }

    private async Task ExecuteGlobalMutationAsync(string undoLabel, string success, Action<DanteProject> mutation)
    {
        if (_project is null || !await EnsureEditableAsync())
        {
            return;
        }

        if (!await ConfirmAsync(
                L("Action globale", "Global action"),
                L(
                    "Cette action modifiera toutes les machines compatibles du projet. Vérifiez ensuite le rapport avant Dante.",
                    "This action will modify every compatible device in the project. Review the final Dante report afterwards."),
                L("Appliquer", "Apply")))
        {
            return;
        }

        await ExecuteMutationAsync(undoLabel, success, mutation);
    }

    private void IpModeCombo_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        UpdateIpFieldState();
    }

    private void SearchTextBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        RefreshDeviceRows(SelectedDeviceRow()?.Name);
    }

    private void DeviceFilterCombo_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        RefreshDeviceRows(SelectedDeviceRow()?.Name);
    }

    private void PatchSearchTextBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        RefreshPatchRows();
    }

    private void PatchStatusCombo_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        RefreshPatchRows();
    }

    private void SourceDeviceCombo_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        RefreshSourceChannels();
    }

    private async void ApplyPatchButton_Click(object? sender, RoutedEventArgs e)
    {
        PatchRow? patch = FindControl<DataGrid>("PatchGrid")!.SelectedItem as PatchRow;
        string? txDevice = FindControl<ComboBox>("SourceDeviceCombo")!.SelectedItem as string;
        ChoiceValue? txChannel = FindControl<ComboBox>("SourceChannelCombo")!.SelectedItem as ChoiceValue;
        if (patch is null || string.IsNullOrWhiteSpace(txDevice) || txChannel is null)
        {
            await ShowInfoAsync(
                LocalizationService.Text(_language, "Dialog.ActionImpossibleTitle"),
                LocalizationService.Text(_language, "Dialog.MissingTxMessage"));
            return;
        }

        await ExecuteMutationAsync(
            L("Appliquer patch", "Apply subscription"),
            LocalizationService.Text(_language, "Action.PatchApplied"),
            project => project.ApplyPatch(patch.RxDevice, patch.RxIndex, txDevice, txChannel.Value),
            patch.RxDevice);
    }

    private async void RemovePatchButton_Click(object? sender, RoutedEventArgs e)
    {
        PatchRow? patch = FindControl<DataGrid>("PatchGrid")!.SelectedItem as PatchRow;
        if (patch is null)
        {
            return;
        }

        if (!await ConfirmAsync(
                L("Supprimer le patch", "Remove subscription"),
                LocalizationService.Text(_language, "Dialog.RemovePatchWarning"),
                L("Supprimer", "Remove")))
        {
            return;
        }

        await ExecuteMutationAsync(
            L("Supprimer patch", "Remove subscription"),
            LocalizationService.Text(_language, "Action.PatchRemoved"),
            project => project.RemovePatch(patch.RxDevice, patch.RxIndex),
            patch.RxDevice);
    }

    private async void VisualPatchButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_project is null)
        {
            return;
        }

        if (!_project.Devices.Any(device => device.TxCount > 0)
            || !_project.Devices.Any(device => device.RxCount > 0))
        {
            await ShowInfoAsync(
                LocalizationService.Text(_language, "Dialog.ActionImpossibleTitle"),
                L(
                    "Le preset chargé doit contenir au moins un canal TX et un canal RX.",
                    "The loaded preset must contain at least one Tx channel and one Rx channel."));
            return;
        }

        string? initialTxDevice = FindControl<ComboBox>("SourceDeviceCombo")!.SelectedItem as string;
        string? initialRxDevice = (FindControl<DataGrid>("PatchGrid")!.SelectedItem as PatchRow)?.RxDevice;
        PatchWorkspaceDialog dialog = new(
            _language,
            _project,
            initialTxDevice,
            initialRxDevice,
            immediateApply: edits => ExecuteMutationAsync(
                L("Patch visuel", "Visual patch"),
                LocalizationService.Format(_language, "Action.VisualPatchesApplied", edits.Count),
                project => project.ApplyBatch(batch =>
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
                }),
                edits[0].RxDeviceName));

        await dialog.ShowDialog<bool>(this);
    }

    private void ValidateButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_project is null) return;
        FindControl<TextBox>("ReportTextBox")!.Text = _project.Validate().ToDisplayText();
    }

    private async void AtomicChaosButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_project is null)
        {
            return;
        }

        AtomicChaosOptions options = SelectedAtomicChaosOptions();
        if (!options.HasSelection)
        {
            await ShowInfoAsync(
                LocalizationService.Text(_language, "Dialog.AtomicChaosTitle"),
                LocalizationService.Text(_language, "Dialog.AtomicChaosNothingSelected"));
            return;
        }

        if (!await ConfirmAtomicChaosStageAsync("Dialog.AtomicChaosFirst")
            || !await ConfirmAtomicChaosStageAsync("Dialog.AtomicChaosSecond")
            || !await ConfirmAtomicChaosStageAsync("Dialog.AtomicChaosThird"))
        {
            return;
        }

        AtomicChaosResult? result = null;
        await ExecuteMutationAsync(
            LocalizationService.Text(_language, "Action.AtomicChaosApplied"),
            LocalizationService.Text(_language, "Action.AtomicChaosApplied"),
            project => result = project.ApplyAtomicChaos(options));
        if (result is null || _project is null)
        {
            return;
        }

        string summary = LocalizationService.Format(
            _language,
            "Dialog.AtomicChaosCompleted",
            result.Seed,
            result.DeviceCount,
            result.TxChannelCount,
            result.PatchedRxCount,
            result.DisconnectedRxCount,
            result.StaticIpCount,
            result.DynamicIpCount);
        FindControl<TextBox>("ReportTextBox")!.Text = summary + Environment.NewLine + Environment.NewLine + _project.BuildCompatibilityReport();
        await ShowInfoAsync(LocalizationService.Text(_language, "Dialog.AtomicChaosTitle"), summary);
    }

    private AtomicChaosOptions SelectedAtomicChaosOptions() => new(
        FindControl<CheckBox>("AtomicDeviceNamesCheckBox")!.IsChecked == true,
        FindControl<CheckBox>("AtomicTxLabelsCheckBox")!.IsChecked == true,
        FindControl<CheckBox>("AtomicRxLabelsCheckBox")!.IsChecked == true,
        FindControl<CheckBox>("AtomicPatchesCheckBox")!.IsChecked == true,
        FindControl<CheckBox>("AtomicNetworkModeCheckBox")!.IsChecked == true,
        FindControl<CheckBox>("AtomicPreferredMasterCheckBox")!.IsChecked == true,
        FindControl<CheckBox>("AtomicLatencyCheckBox")!.IsChecked == true,
        FindControl<CheckBox>("AtomicSampleRateCheckBox")!.IsChecked == true,
        FindControl<CheckBox>("AtomicEncodingCheckBox")!.IsChecked == true,
        FindControl<CheckBox>("AtomicIpCheckBox")!.IsChecked == true);

    private Task<bool> ConfirmAtomicChaosStageAsync(string key)
    {
        return ConfirmAsync(
            LocalizationService.Text(_language, "Dialog.AtomicChaosTitle"),
            LocalizationService.Text(_language, key),
            L("ATOMISER LA COPIE", "ATOMIZE THE COPY"));
    }

    private void CompatibilityButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_project is null) return;
        FindControl<TextBox>("ReportTextBox")!.Text = _project.BuildCompatibilityReport();
    }

    private void FinalReportButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_project is null) return;
        FindControl<TextBox>("ReportTextBox")!.Text = _project.BuildReportText();
    }

    private void TopologyButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_project is null) return;
        FindControl<TextBox>("ReportTextBox")!.Text = _project.BuildTopologyText();
    }

    private async void CompareButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_project is null) return;
        string? path = await PickOpenPathAsync(L("Comparer avec un XML", "Compare with XML"));
        if (path is null) return;
        try
        {
            FindControl<TextBox>("ReportTextBox")!.Text = _project.CompareWith(
                DanteProject.Load(path),
                _language);
        }
        catch (Exception exception)
        {
            await ShowErrorAsync(L("Comparaison impossible", "Comparison failed"), exception);
        }
    }

    private async void ExportTxtButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_project is null) return;
        await ExportTextAsync("rapport-dante.txt", _project.BuildReportText());
    }

    private async void ExportPdfButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_project is null) return;
        IStorageFile? file = await PickSaveFileAsync("rapport-dante.pdf", "pdf", "PDF", ["*.pdf"]);
        string? path = file?.TryGetLocalPath();
        if (path is null) return;
        try
        {
            ReportExportService.ExportPdf(path, "Dante Config Editor 2026.1 Beta", _project.BuildReportText());
            SetStatus(LocalizationService.Text(_language, "Status.PdfExported"));
        }
        catch (Exception exception)
        {
            await ShowErrorAsync(LocalizationService.Text(_language, "Dialog.ExportImpossibleTitle"), exception);
        }
    }

    private async void PatchbookTxtButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_project is null) return;
        await ExportTextAsync("patchbook-dante.txt", _project.BuildPatchbookText("all"));
    }

    private async void PatchbookCsvButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_project is null) return;
        IStorageFile? file = await PickSaveFileAsync("patchbook-dante.csv", "csv", "CSV", ["*.csv"]);
        string? path = file?.TryGetLocalPath();
        if (path is null) return;
        try
        {
            ReportExportService.ExportText(path, _project.BuildPatchbookCsv("all"));
            SetStatus(LocalizationService.Text(_language, "Status.PatchbookCsvExported"));
        }
        catch (Exception exception)
        {
            await ShowErrorAsync(LocalizationService.Text(_language, "Dialog.ExportPatchbookCsvImpossibleTitle"), exception);
        }
    }

    private async Task ExportTextAsync(string suggestedName, string content)
    {
        IStorageFile? file = await PickSaveFileAsync(suggestedName, "txt", "Text", ["*.txt"]);
        string? path = file?.TryGetLocalPath();
        if (path is null) return;
        try
        {
            ReportExportService.ExportText(path, content);
            SetStatus(LocalizationService.Text(_language, "Status.TxtExported"));
        }
        catch (Exception exception)
        {
            await ShowErrorAsync(LocalizationService.Text(_language, "Dialog.ExportImpossibleTitle"), exception);
        }
    }

    private void QuickStartButton_Click(object? sender, RoutedEventArgs e)
    {
        OpenBundledDocument($"QuickStart_DanteConfigEditorV3_{DocumentLanguageSuffix()}.pdf");
    }

    private void FullGuideButton_Click(object? sender, RoutedEventArgs e)
    {
        OpenBundledDocument($"Notice_DanteConfigEditorV3_{DocumentLanguageSuffix()}.pdf");
    }

    private async void SupportDceButton_Click(object? sender, RoutedEventArgs e)
    {
        await SupportDceDialog.ShowAsync(this, _language);
    }

    private async void SupportReminderOpenButton_Click(object? sender, RoutedEventArgs e)
    {
        PostponeSupportReminder();
        await SupportDceDialog.ShowAsync(this, _language);
    }

    private void SupportReminderLaterButton_Click(object? sender, RoutedEventArgs e)
    {
        PostponeSupportReminder();
    }

    private void SupportReminderNeverButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            _supportReminderSettings.Suppress();
        }
        catch (Exception exception)
        {
            DiagnosticLogService.Default.Write("Support", "Unable to save support reminder preference.", exception);
        }

        FindControl<Border>("SupportReminderBorder")!.IsVisible = false;
    }

    private void InitializeSupportReminder()
    {
        if (SupportReminderSettingsService.IsAutomatedTestProcess())
        {
            return;
        }

        Border reminder = FindControl<Border>("SupportReminderBorder")!;
        try
        {
            string version = GetType().Assembly.GetName().Version?.ToString(2) ?? "2026.1";
            reminder.IsVisible = _supportReminderSettings
                .RegisterSuccessfulLaunch(version)
                .ShouldShow;
        }
        catch (Exception exception)
        {
            // Le rappel reste strictement accessoire et ne doit jamais bloquer
            // l'ouverture de l'application.
            DiagnosticLogService.Default.Write("Support", "Support reminder unavailable.", exception);
            reminder.IsVisible = false;
        }
    }

    private void PostponeSupportReminder()
    {
        try
        {
            _supportReminderSettings.Postpone();
        }
        catch (Exception exception)
        {
            DiagnosticLogService.Default.Write("Support", "Unable to postpone support reminder.", exception);
        }

        FindControl<Border>("SupportReminderBorder")!.IsVisible = false;
    }

    private async void OpenDiagnosticLogsButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            string directory = DiagnosticLogService.Default.DirectoryPath;
            Directory.CreateDirectory(directory);
            Process.Start(new ProcessStartInfo(directory) { UseShellExecute = true });
        }
        catch (Exception exception)
        {
            await ShowErrorAsync(
                L("Ouverture des journaux impossible", "Unable to open diagnostic logs"),
                exception);
        }
    }

    private void OpenBundledDocument(string fileName)
    {
        // En développement les PDF sont à côté de l'exécutable. Dans le bundle
        // macOS ils suivent la convention Apple et vivent dans Contents/Resources.
        string[] candidates =
        [
            Path.Combine(AppContext.BaseDirectory, "Docs", fileName),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "Resources", "Docs", fileName))
        ];
        string path = candidates.FirstOrDefault(File.Exists) ?? candidates[0];
        if (!File.Exists(path))
        {
            _ = ShowInfoAsync(L("Document introuvable", "Document not found"), path);
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
        catch (Exception exception)
        {
            _ = ShowErrorAsync(L("Ouverture impossible", "Cannot open document"), exception);
        }
    }

    private void LanguageCombo_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_initializing) return;
        _language = SelectedTag("LanguageCombo") == "en" ? UiLanguage.English : UiLanguage.French;
        LanguageSettingsService.Save(_language);
        ConfigureChoiceLists();
        ApplyLanguageToVisualTree();
        RefreshAll(SelectedDeviceRow()?.Name);
    }

    private void ThemeButton_Click(object? sender, RoutedEventArgs e)
    {
        _darkTheme = !_darkTheme;
        SaveThemePreference(_darkTheme);
        ApplyTheme();
    }

    private void ConfigureChoiceLists()
    {
        SetChoices("NetworkModeCombo", NetworkChoices(), "daisychain");
        SetChoices("GlobalNetworkCombo", NetworkChoices(), "redundant");
        SetChoices("LatencyCombo", LatencyChoices(), "1000");
        SetChoices("GlobalLatencyCombo", LatencyChoices(), "1000");
        SetChoices("SampleRateCombo", SampleRateChoices(), "48000");
        SetChoices("GlobalSampleRateCombo", SampleRateChoices(), "48000");
        SetChoices("BitsCombo", BitsChoices(), "24");
        SetChoices("GlobalBitsCombo", BitsChoices(), "24");
        SetChoices("IpModeCombo", [new("auto", L("Automatique", "Automatic")), new("static", L("Fixe", "Static"))], "auto");
        SetChoices(
            "ProfileCombo",
            DeviceProfileCatalog.BuiltIn.Select(profile => new ChoiceValue(profile.Key, LocalizationService.Text(_language, profile.Key))).ToArray(),
            DeviceProfileCatalog.BuiltIn.First().Key);

        FindControl<ComboBox>("ChannelKindCombo")!.SelectedIndex = Math.Max(0, FindControl<ComboBox>("ChannelKindCombo")!.SelectedIndex);
        FindControl<ComboBox>("DeviceFilterCombo")!.SelectedIndex = Math.Max(0, FindControl<ComboBox>("DeviceFilterCombo")!.SelectedIndex);
        FindControl<ComboBox>("PatchStatusCombo")!.SelectedIndex = Math.Max(0, FindControl<ComboBox>("PatchStatusCombo")!.SelectedIndex);
    }

    private IReadOnlyList<ChoiceValue> NetworkChoices() =>
    [
        new("redundant", L("Redondant", "Redundant")),
        new("daisychain", "Daisychain")
    ];

    private IReadOnlyList<ChoiceValue> LatencyChoices() =>
    [
        new("250", L("0,25 ms", "0.25 ms")),
        new("500", L("0,5 ms", "0.5 ms")),
        new("1000", "1 ms"),
        new("2000", "2 ms"),
        new("5000", "5 ms"),
        new("10000", "10 ms")
    ];

    private static IReadOnlyList<ChoiceValue> SampleRateChoices() =>
    [
        new("44100", "44.1 kHz"),
        new("48000", "48 kHz"),
        new("88200", "88.2 kHz"),
        new("96000", "96 kHz"),
        new("176400", "176.4 kHz"),
        new("192000", "192 kHz")
    ];

    private IReadOnlyList<ChoiceValue> BitsChoices() =>
    [
        new("16", L("16 bit", "16 bit")),
        new("24", L("24 bit", "24 bit")),
        new("32", L("32 bit", "32 bit"))
    ];

    private void RefreshAll(string? preferredDeviceName = null)
    {
        RefreshDeviceRows(preferredDeviceName);
        RefreshPatchRows();
        RefreshHealthRows();
        RefreshSynopticWorkspace();
        RefreshSourceDevices();
        RefreshLog();
        RefreshSummary();
        UpdateUiState();
    }

    private void RefreshDeviceRows(string? preferredDeviceName = null)
    {
        DataGrid grid = FindControl<DataGrid>("DeviceGrid")!;
        if (_project is null)
        {
            grid.ItemsSource = Array.Empty<DeviceRow>();
            PopulateSelectedDevicePanel();
            return;
        }

        string search = FindControl<TextBox>("SearchTextBox")?.Text?.Trim() ?? string.Empty;
        string filter = SelectedTag("DeviceFilterCombo");
        IReadOnlySet<string> modified = _project.GetModifiedDeviceNames();
        IEnumerable<DanteDevice> devices = _project.Devices;
        if (!string.IsNullOrWhiteSpace(search))
        {
            devices = devices.Where(device =>
                Contains(device.Name, search)
                || Contains(device.FriendlyName, search)
                || device.TxChannels.Any(channel => Contains(channel.DisplayName, search))
                || device.RxChannels.Any(channel => Contains(channel.DisplayName, search)));
        }

        devices = filter switch
        {
            "modified" => devices.Where(device => modified.Contains(device.Name)),
            "static" => devices.Where(device => device.UsesStaticIp),
            "preferred" => devices.Where(device => device.PreferredMaster),
            "redundant" => devices.Where(device => device.IsRedundant),
            "daisychain" => devices.Where(device => !device.IsRedundant),
            _ => devices
        };

        DeviceRow[] rows = devices.Select(device => new DeviceRow(
            device.Name,
            device.FriendlyName,
            device.IsRedundant ? L("Redondant", "Redundant") : "Daisychain",
            device.LatencyDisplay,
            device.SampleRateDisplay,
            device.EncodingDisplay,
            device.IpModeDisplay,
            device.PreferredMaster,
            device.TxCount,
            device.RxCount)).ToArray();
        grid.ItemsSource = rows;

        ComboBox preferredMaster = FindControl<ComboBox>("ExclusivePreferredMasterCombo")!;
        string? selectedPreferredMaster = preferredMaster.SelectedItem as string
            ?? _project.Devices.FirstOrDefault(device => device.PreferredMaster)?.Name;
        string[] allDeviceNames = _project.Devices.Select(device => device.Name).ToArray();
        preferredMaster.ItemsSource = allDeviceNames;
        preferredMaster.SelectedItem = allDeviceNames.FirstOrDefault(name =>
            string.Equals(name, selectedPreferredMaster, StringComparison.OrdinalIgnoreCase)) ?? allDeviceNames.FirstOrDefault();

        string? targetName = preferredDeviceName ?? SelectedDeviceRow()?.Name;
        grid.SelectedItem = rows.FirstOrDefault(row => string.Equals(row.Name, targetName, StringComparison.OrdinalIgnoreCase))
            ?? rows.FirstOrDefault();
        PopulateSelectedDevicePanel();
    }

    private void PopulateSelectedDevicePanel()
    {
        DeviceRow? row = SelectedDeviceRow();
        DanteDevice? device = row is null ? null : _project?.FindDevice(row.Name);
        FindControl<TextBlock>("SelectedDeviceNameText")!.Text = device?.Name ?? "-";
        FindControl<TextBox>("DeviceNameTextBox")!.Text = device?.Name ?? string.Empty;
        if (device is not null)
        {
            SelectChoice("NetworkModeCombo", device.IsRedundant ? "redundant" : "daisychain");
            SelectChoice("LatencyCombo", device.Latency);
            SelectChoice("SampleRateCombo", device.Samplerate);
            SelectChoice("BitsCombo", device.Encoding);
            SelectChoice("IpModeCombo", device.UsesStaticIp ? "static" : "auto");
            FindControl<CheckBox>("PreferredCheckBox")!.IsChecked = device.PreferredMaster;
            FindControl<TextBox>("IpAddressTextBox")!.Text = device.StaticIpAddress;
            FindControl<TextBox>("NetmaskTextBox")!.Text = string.IsNullOrWhiteSpace(device.StaticIpNetmask) ? "255.255.255.0" : device.StaticIpNetmask;
            FindControl<TextBox>("GatewayTextBox")!.Text = string.IsNullOrWhiteSpace(device.StaticIpGateway) ? "0.0.0.0" : device.StaticIpGateway;
        }

        RefreshChannelChoices();
        UpdateIpFieldState();
    }

    private void RefreshChannelChoices()
    {
        DeviceRow? row = SelectedDeviceRow();
        DanteDevice? device = row is null ? null : _project?.FindDevice(row.Name);
        bool rx = SelectedTag("ChannelKindCombo") == "rx";
        IReadOnlyList<DanteChannel> channels = device is null
            ? []
            : rx ? device.RxChannels : device.TxChannels;
        ChoiceValue[] choices = channels.Select(channel => new ChoiceValue(channel.Index.ToString(), channel.DisplayLabel)).ToArray();
        SetComboItems("ChannelCombo", choices);
        SetComboItems("StartChannelCombo", choices);
        SetComboItems("EndChannelCombo", choices, selectLast: true);
        PopulateSelectedChannelName();
    }

    private void RefreshPatchRows()
    {
        DataGrid grid = FindControl<DataGrid>("PatchGrid")!;
        if (_project is null)
        {
            grid.ItemsSource = Array.Empty<PatchRow>();
            return;
        }

        string search = FindControl<TextBox>("PatchSearchTextBox")?.Text?.Trim() ?? string.Empty;
        string status = SelectedTag("PatchStatusCombo");
        IEnumerable<DanteSubscription> subscriptions = _project.PatchMatrix.Subscriptions;
        if (!string.IsNullOrWhiteSpace(search))
        {
            subscriptions = subscriptions.Where(subscription =>
                Contains(subscription.RxDevice, search)
                || Contains(subscription.RxChannelName, search)
                || Contains(subscription.DisplayTxDeviceName, search)
                || Contains(subscription.TxChannelName, search));
        }

        subscriptions = status switch
        {
            "active" => subscriptions.Where(subscription => subscription.IsActive),
            "free" => subscriptions.Where(subscription => !subscription.IsActive),
            "warnings" => subscriptions.Where(subscription => subscription.IsWarning || subscription.IsConflict),
            _ => subscriptions
        };

        grid.ItemsSource = subscriptions.Select(subscription => new PatchRow(
            subscription.RxDevice,
            subscription.RxIndex,
            subscription.RxDanteId,
            subscription.RxChannelName,
            subscription.DisplayTxDeviceName,
            subscription.TxChannelName,
            subscription.SourceFull,
            LocalizationService.TranslateLiteral(_language, subscription.TypeLabel),
            LocalizationService.TranslateLiteral(_language, subscription.Status))).ToArray();
    }

    private void RefreshHealthRows()
    {
        DataGrid grid = FindControl<DataGrid>("HealthGrid")!;
        if (_project is null)
        {
            grid.ItemsSource = Array.Empty<HealthRow>();
            FindControl<TextBlock>("HealthSummaryText")!.Text = LocalizationService.Text(_language, "Status.NoFileLoaded");
            return;
        }

        DanteValidationResult validation = _project.Validate();
        grid.ItemsSource = validation.Issues.Select(issue => new HealthRow(
            LocalizationService.TranslateLiteral(_language, issue.SeverityLabel),
            LocalizationService.TranslateLiteral(_language, issue.CategoryLabel),
            issue.DeviceName ?? "-",
            issue.ChannelName ?? (issue.DanteId.HasValue ? issue.DanteId.Value.ToString() : "-"),
            issue.Message)).ToArray();

        FindControl<TextBlock>("HealthSummaryText")!.Text = L(
            $"{validation.Errors.Count} erreur(s), {validation.Warnings.Count} avertissement(s), {validation.Infos.Count} information(s).",
            $"{validation.Errors.Count} error(s), {validation.Warnings.Count} warning(s), {validation.Infos.Count} information item(s)." );
    }

    private void RefreshSourceDevices()
    {
        ComboBox combo = FindControl<ComboBox>("SourceDeviceCombo")!;
        string? previous = combo.SelectedItem as string;
        string[] devices = _project?.Devices.Where(device => device.TxCount > 0).Select(device => device.Name).ToArray() ?? [];
        combo.ItemsSource = devices;
        combo.SelectedItem = devices.FirstOrDefault(device => string.Equals(device, previous, StringComparison.OrdinalIgnoreCase))
            ?? devices.FirstOrDefault();
        RefreshSourceChannels();
    }

    private void RefreshSourceChannels()
    {
        string? deviceName = FindControl<ComboBox>("SourceDeviceCombo")!.SelectedItem as string;
        DanteDevice? device = _project?.FindDevice(deviceName);
        ChoiceValue[] channels = device?.TxChannels
            .Select(channel => new ChoiceValue(channel.DisplayName, channel.DisplayLabel))
            .ToArray() ?? [];
        SetComboItems("SourceChannelCombo", channels);
    }

    private void RefreshLog()
    {
        FindControl<ListBox>("LogListBox")!.ItemsSource = _project?.Changes
            .Reverse()
            .Take(100)
            .Select(change => $"{change.Timestamp:HH:mm:ss}  {change.Action}\n{change.Details}")
            .ToArray() ?? [];
    }

    private void RefreshSummary()
    {
        if (_project is null)
        {
            FindControl<TextBlock>("ProjectSummaryText")!.Text = LocalizationService.Text(_language, "Status.LoadXmlToStart");
            FindControl<TextBlock>("FilePathText")!.Text = LocalizationService.Text(_language, "Status.NoFileOpen");
            FindControl<TextBlock>("CountText")!.Text = "0 device - 0 TX - 0 RX";
            FindControl<TextBlock>("DirtyStateText")!.Text = LocalizationService.Text(_language, "Status.Unmodified");
            FindControl<Border>("WarningBorder")!.IsVisible = false;
            FindControl<TextBox>("ReportTextBox")!.Text = string.Empty;
            return;
        }

        int tx = _project.Devices.Sum(device => device.TxCount);
        int rx = _project.Devices.Sum(device => device.RxCount);
        FindControl<TextBlock>("ProjectSummaryText")!.Text = L(
            $"{_project.Devices.Count} machines\n{tx} canaux TX\n{rx} canaux RX\n{_project.PatchMatrix.ActivePatchCount} patchs actifs",
            $"{_project.Devices.Count} devices\n{tx} TX channels\n{rx} RX channels\n{_project.PatchMatrix.ActivePatchCount} active subscriptions");
        FindControl<TextBlock>("FilePathText")!.Text = _project.OriginalFilePath;
        FindControl<TextBlock>("CountText")!.Text = $"{_project.Devices.Count} devices - {tx} TX - {rx} RX";
        FindControl<TextBlock>("DirtyStateText")!.Text = _project.IsModified
            ? LocalizationService.Text(_language, "Status.ModifiedUnsaved")
            : LocalizationService.Text(_language, "Status.Unmodified");
        FindControl<TextBlock>("DirtyStateText")!.Foreground = _project.IsModified
            ? ResourceBrush("DangerBrush")
            : ResourceBrush("MutedTextBrush");

        IReadOnlyList<DanteImportantWarning> warnings = _project.BuildImportantWarningDetails();
        FindControl<Border>("WarningBorder")!.IsVisible = warnings.Count > 0;
        FindControl<TextBlock>("WarningText")!.Text = warnings.Count == 0
            ? string.Empty
            : string.Join("\n\n", warnings.Take(3).Select(warning => warning.LocalizedMessage(_language == UiLanguage.English)));
        TextBox report = FindControl<TextBox>("ReportTextBox")!;
        if (string.IsNullOrWhiteSpace(report.Text))
        {
            report.Text = _project.BuildSaveSummary();
        }
    }

    private void UpdateUiState()
    {
        bool loaded = _project is not null;
        FindControl<Button>("MergeButton")!.IsEnabled = loaded;
        FindControl<Button>("SaveButton")!.IsEnabled = loaded;
        FindControl<Button>("EditButton")!.IsEnabled = loaded && !_editEnabled;
        FindControl<Button>("UndoButton")!.IsEnabled = loaded && _editEnabled && _project!.CanUndo;
        FindControl<Button>("RevertButton")!.IsEnabled = loaded && _project!.IsModified;
        FindControl<Button>("AtomicChaosButton")!.IsEnabled = loaded;
        FindControl<TabControl>("MainTabs")!.IsEnabled = loaded;
        FindControl<TextBlock>("ModeText")!.Text = _editEnabled
            ? LocalizationService.Text(_language, "Status.EditMode")
            : LocalizationService.Text(_language, "Status.ReadOnlyMode");
        FindControl<Button>("EditButton")!.Content = _editEnabled
            ? LocalizationService.Text(_language, "Status.EditActiveButton")
            : LocalizationService.Text(_language, "Status.ActivateEditButton");
    }

    private async Task ExecuteMutationAsync(
        string undoLabel,
        string success,
        Action<DanteProject> mutation,
        string? preferredDeviceName = null)
    {
        if (!await EnsureEditableAsync() || _project is null)
        {
            return;
        }

        _project.PushUndoSnapshot(undoLabel);
        try
        {
            mutation(_project);
            RefreshAll(preferredDeviceName);
            ScheduleRecovery();
            SetStatus(success);
        }
        catch (Exception exception)
        {
            await RestoreSnapshotAfterFailureAsync(LocalizationService.Text(_language, "Dialog.ActionImpossibleTitle"), exception);
        }
    }

    private async Task RestoreSnapshotAfterFailureAsync(string title, Exception exception)
    {
        if (_project?.CanUndo == true)
        {
            _project.RestoreLastUndoSnapshot();
        }

        RefreshAll();
        await ShowErrorAsync(title, exception);
    }

    private async Task<bool> EnsureEditableAsync()
    {
        if (_project is null)
        {
            await ShowInfoAsync(
                LocalizationService.Text(_language, "Dialog.NoFileLoadedTitle"),
                LocalizationService.Text(_language, "Dialog.NoFileLoadedMessage"));
            return false;
        }

        if (!_editEnabled)
        {
            // Le mode initial protège uniquement le fichier d'origine. Une
            // action dans l'interface active l'édition, tandis que Save As
            // reste obligatoire pour écrire le résultat ailleurs.
            _editEnabled = true;
            UpdateUiState();
            SetStatus(LocalizationService.Text(_language, "Status.EditEnabled"));
        }

        return true;
    }

    private async void ImportChannelLabelsButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_project is null || !await EnsureEditableAsync())
        {
            return;
        }

        string? path = await PickOpenPathAsync(
            L("Importer des labels de canaux", "Import channel labels"),
            ChannelLabelFileType);
        if (path is null)
        {
            return;
        }

        try
        {
            ChannelLabelReadResult readResult = ChannelLabelExchangeService.ReadWithReport(path);
            IReadOnlyList<ChannelLabelAssignment>? assignments = await ChannelLabelImportDialog.ShowAsync(
                this,
                _project,
                readResult.Document,
                readResult.Report,
                _language,
                SelectedDeviceRow()?.Name);
            if (assignments is null)
            {
                return;
            }

            await ExecuteMutationAsync(
                L("Import de labels", "Label import"),
                L($"{assignments.Count} label(s) de canal importé(s).", $"{assignments.Count} channel label(s) imported."),
                project => project.ApplyChannelLabels(assignments));
        }
        catch (Exception exception)
        {
            await ShowErrorAsync(L("Import de labels impossible", "Label import unavailable"), exception);
        }
    }

    private async void OpenDmtProjectButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo("https://github.com/togrupe/dlive-midi-tools")
            {
                UseShellExecute = true
            });
        }
        catch (Exception exception)
        {
            await ShowErrorAsync(
                L("Ouverture du projet DMT impossible", "Cannot open the DMT project"),
                exception);
        }
    }

    private async void ExportChannelLabelsButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_project is null)
        {
            return;
        }

        ChannelLabelExportDialogResult? result = await ChannelLabelExportDialog.ShowAsync(
            this,
            _project,
            _language,
            SelectedDeviceRow()?.Name);
        if (result is null)
        {
            return;
        }

        try
        {
            ChannelLabelDocument document = ChannelLabelExchangeService.CreateFromProject(
                _project,
                result.DeviceNames,
                result.Kind,
                result.StartChannel,
                result.Count);
            document = ChannelLabelTransformService.Transform(document, result.TransformOptions);
            if (BuiltInChannelLabelTemplateService.IsBuiltInFormat(result.Format))
            {
                await ExportBuiltInTemplateCopiesAsync(document, result.Format, adaptLabels: false);
                return;
            }

            string extension = result.Format == "csv" ? "csv" : "json";
            IStorageFile? file = await PickSaveFileAsync(
                $"{Path.GetFileNameWithoutExtension(_project.OriginalFilePath)}_labels.{extension}",
                extension,
                extension.ToUpperInvariant(),
                [$"*.{extension}"]);
            string? output = file?.TryGetLocalPath();
            if (output is null)
            {
                return;
            }
            ChannelLabelExchangeService.Write(output, document);
            SetStatus(L("Labels de canaux exportés.", "Channel labels exported."));
        }
        catch (Exception exception)
        {
            await ShowErrorAsync(L("Export de labels impossible", "Label export unavailable"), exception);
        }
    }

    private async Task ExportBuiltInTemplateCopiesAsync(ChannelLabelDocument document, string format, bool adaptLabels)
    {
        BuiltInChannelLabelTemplateInfo template = BuiltInChannelLabelTemplateService.Get(format);
        string extension = template.OutputExtension.TrimStart('.');
        string suggestedName = document.Sets.Count == 1
            ? $"{SafeFileNameSegment(document.Sets[0].DeviceName)}_{template.FileSuffix}.{extension}"
            : $"Dante_labels_{template.FileSuffix}.{extension}";
        IStorageFile? target = await PickSaveFileAsync(
            suggestedName,
            extension,
            template.DisplayName,
            [$"*.{extension}"]);
        string? selectedOutput = target?.TryGetLocalPath();
        if (selectedOutput is null)
        {
            return;
        }

        string directory = Path.GetDirectoryName(selectedOutput) ?? Environment.CurrentDirectory;
        string baseName = Path.GetFileNameWithoutExtension(selectedOutput);
        string[] outputs = document.Sets.Select(set => document.Sets.Count == 1
            ? selectedOutput
            : Path.Combine(directory, $"{baseName}-{SafeFileNameSegment(set.DeviceName)}.{extension}")).ToArray();
        int existing = outputs.Count(File.Exists);
        if (existing > 0 && !await ConfirmAsync(
                L("Remplacer les copies console", "Replace console copies"),
                L(
                    $"{existing} fichier(s) généré(s) existent déjà et seront remplacés.",
                    $"{existing} generated file(s) already exist and will be replaced."),
                L("Remplacer", "Replace")))
        {
            return;
        }

        for (int index = 0; index < document.Sets.Count; index++)
        {
            BuiltInChannelLabelTemplateService.Write(format, outputs[index], document.Sets[index], adaptLabels);
        }
        SetStatus(L(
            $"{outputs.Length} fichier(s) {template.DisplayName} exporté(s).",
            $"{outputs.Length} {template.DisplayName} file(s) exported."));
    }

    private void ScheduleRecovery()
    {
        _recoveryTimer.Stop();
        _recoveryTimer.Start();
    }

    private async void RecoveryTimer_Tick(object? sender, EventArgs e)
    {
        _recoveryTimer.Stop();
        if (_project?.IsModified != true)
        {
            return;
        }

        _recoveryCancellation?.Cancel();
        _recoveryCancellation?.Dispose();
        _recoveryCancellation = new CancellationTokenSource();
        try
        {
            await SessionRecoveryService.SaveAsync(_project, cancellationToken: _recoveryCancellation.Token);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception)
        {
            SetStatus(L($"Récupération automatique indisponible : {exception.Message}", $"Automatic recovery unavailable: {exception.Message}"));
        }
    }

    private void UpdateIpFieldState()
    {
        bool enabled = _project is not null && SelectedValue("IpModeCombo") == "static";
        FindControl<TextBox>("IpAddressTextBox")!.IsEnabled = enabled;
        FindControl<TextBox>("NetmaskTextBox")!.IsEnabled = enabled;
        FindControl<TextBox>("GatewayTextBox")!.IsEnabled = enabled;
    }

    private void ApplyLanguageToVisualTree()
    {
        IEnumerable<object> elements = this.GetLogicalDescendants().Prepend(this);
        foreach (object element in elements)
        {
            switch (element)
            {
                case HeaderedContentControl headered when headered.Header is string header:
                    headered.Header = LocalizationService.TranslateLiteral(_language, header);
                    break;
                case TextBlock text when !string.IsNullOrWhiteSpace(text.Text):
                    text.Text = LocalizationService.TranslateLiteral(_language, text.Text);
                    break;
                case ContentControl content when content.Content is string value:
                    content.Content = LocalizationService.TranslateLiteral(_language, value);
                    break;
                case ComboBox comboBox when !string.IsNullOrWhiteSpace(comboBox.PlaceholderText):
                    comboBox.PlaceholderText = LocalizationService.TranslateLiteral(_language, comboBox.PlaceholderText!);
                    break;
                case TextBox textBox when !string.IsNullOrWhiteSpace(textBox.Watermark):
                    textBox.Watermark = LocalizationService.TranslateLiteral(_language, textBox.Watermark!);
                    break;
            }

            if (element is Control control && ToolTip.GetTip(control) is string toolTip)
            {
                ToolTip.SetTip(control, LocalizationService.TranslateLiteral(_language, toolTip));
            }
        }

        foreach (DataGrid dataGrid in elements.OfType<DataGrid>())
        {
            foreach (DataGridColumn column in dataGrid.Columns)
            {
                if (column.Header is string header)
                {
                    column.Header = LocalizationService.TranslateLiteral(_language, header);
                }
            }
        }

        FindControl<Button>("MergeButton")!.Content = L("Ajouter XML", "Add XML");
        FindControl<TextBox>("SearchTextBox")!.Watermark = L("Machine ou canal", "Device or channel");
        ApplyDeviceFilterLanguage();

        Title = L("Dante Config Editor 2026.1 Beta - macOS", "Dante Config Editor 2026.1 Beta - macOS");
        FindControl<Button>("ThemeButton")!.Content = _darkTheme ? L("Thème clair", "Light theme") : L("Thème sombre", "Dark theme");
    }

    private void ApplyDeviceFilterLanguage()
    {
        Dictionary<string, string> labels = new(StringComparer.Ordinal)
        {
            ["all"] = LocalizationService.Text(_language, "DeviceFilter.All"),
            ["modified"] = LocalizationService.Text(_language, "DeviceFilter.Modified"),
            ["static"] = LocalizationService.Text(_language, "DeviceFilter.StaticIp"),
            ["preferred"] = LocalizationService.Text(_language, "DeviceFilter.PreferredMaster"),
            ["redundant"] = LocalizationService.Text(_language, "DeviceFilter.Redundant"),
            ["daisychain"] = LocalizationService.Text(_language, "DeviceFilter.Daisychain")
        };

        foreach (ComboBoxItem item in FindControl<ComboBox>("DeviceFilterCombo")!.Items.OfType<ComboBoxItem>())
        {
            string tag = item.Tag?.ToString() ?? string.Empty;
            if (labels.TryGetValue(tag, out string? label))
            {
                item.Content = label;
            }
        }
    }

    private void ApplyTheme()
    {
        if (Avalonia.Application.Current is null)
        {
            return;
        }

        Avalonia.Application.Current.RequestedThemeVariant = _darkTheme ? ThemeVariant.Dark : ThemeVariant.Light;
        Dictionary<string, string> colors = _darkTheme
            ? new()
            {
                ["AppBackgroundBrush"] = "#17191D",
                ["PanelBrush"] = "#22252B",
                ["PanelAltBrush"] = "#292D34",
                ["PanelBorderBrush"] = "#424853",
                ["AppTextBrush"] = "#F4F6F8",
                ["MutedTextBrush"] = "#AEB6C2"
            }
            : new()
            {
                ["AppBackgroundBrush"] = "#F4F6F8",
                ["PanelBrush"] = "#FFFFFF",
                ["PanelAltBrush"] = "#EBEFF4",
                ["PanelBorderBrush"] = "#C6CED8",
                ["AppTextBrush"] = "#1A1D22",
                ["MutedTextBrush"] = "#5D6673"
            };

        foreach ((string key, string color) in colors)
        {
            Avalonia.Application.Current.Resources[key] = new SolidColorBrush(Color.Parse(color));
        }

        if (FindControl<Button>("ThemeButton") is { } button)
        {
            button.Content = _darkTheme ? L("Thème clair", "Light theme") : L("Thème sombre", "Dark theme");
        }
    }

    private void RefreshRecentFiles()
    {
        ComboBox combo = FindControl<ComboBox>("RecentCombo")!;
        string? selected = combo.SelectedItem as string;
        string[] files = RecentFilesService.Load().ToArray();
        combo.ItemsSource = files;
        combo.SelectedItem = files.FirstOrDefault(file => string.Equals(file, selected, StringComparison.OrdinalIgnoreCase))
            ?? files.FirstOrDefault();
    }

    private Task<string?> PickOpenPathAsync(string title) => PickOpenPathAsync(title, XmlFileType);

    private async Task<string?> PickOpenPathAsync(string title, FilePickerFileType fileType)
    {
        IReadOnlyList<IStorageFile> files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
            FileTypeFilter = [fileType]
        });
        return files.FirstOrDefault()?.TryGetLocalPath();
    }

    private static string SafeFileNameSegment(string value)
    {
        HashSet<char> invalid = new(Path.GetInvalidFileNameChars());
        string safe = new string(value.Trim().Select(character => invalid.Contains(character) ? '_' : character).ToArray()).Trim();
        return string.IsNullOrWhiteSpace(safe) ? "device" : safe;
    }

    private Task<IStorageFile?> PickSaveFileAsync(string suggestedName, string extension, string label, IReadOnlyList<string> patterns)
    {
        return StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = L("Exporter", "Export"),
            SuggestedFileName = suggestedName,
            DefaultExtension = extension,
            ShowOverwritePrompt = true,
            FileTypeChoices = [new FilePickerFileType(label) { Patterns = patterns }]
        });
    }

    private Task<bool> ConfirmAsync(string title, string message, string primary)
    {
        return MessageDialog.ShowAsync(this, title, message, primary, L("Annuler", "Cancel"));
    }

    private Task<bool> ShowInfoAsync(string title, string message)
    {
        return MessageDialog.ShowInfoAsync(this, title, message, "OK");
    }

    private Task<bool> ShowErrorAsync(string title, Exception exception)
    {
        DiagnosticLogService.Default.Write("UI", title, exception);
        return ShowInfoAsync(title, exception.Message);
    }

    private void SetStatus(string status)
    {
        FindControl<TextBlock>("StatusText")!.Text = status;
        UpdateUiState();
    }

    private DeviceRow? SelectedDeviceRow() => FindControl<DataGrid>("DeviceGrid")?.SelectedItem as DeviceRow;

    private void SetChoices(string controlName, IReadOnlyList<ChoiceValue> choices, string selectedValue)
    {
        ComboBox combo = FindControl<ComboBox>(controlName)!;
        string current = (combo.SelectedItem as ChoiceValue)?.Value ?? selectedValue;
        combo.ItemsSource = choices;
        combo.SelectedItem = choices.FirstOrDefault(choice => choice.Value == current)
            ?? choices.FirstOrDefault(choice => choice.Value == selectedValue)
            ?? choices.FirstOrDefault();
    }

    private void SetComboItems(string controlName, IReadOnlyList<ChoiceValue> choices, bool selectLast = false)
    {
        ComboBox combo = FindControl<ComboBox>(controlName)!;
        string? current = (combo.SelectedItem as ChoiceValue)?.Value;
        combo.ItemsSource = choices;
        combo.SelectedItem = choices.FirstOrDefault(choice => choice.Value == current)
            ?? (selectLast ? choices.LastOrDefault() : choices.FirstOrDefault());
    }

    private void SelectChoice(string controlName, string value)
    {
        ComboBox combo = FindControl<ComboBox>(controlName)!;
        combo.SelectedItem = combo.ItemsSource?.OfType<ChoiceValue>().FirstOrDefault(choice => choice.Value == value)
            ?? combo.ItemsSource?.OfType<ChoiceValue>().FirstOrDefault();
    }

    private string SelectedValue(string controlName)
    {
        return (FindControl<ComboBox>(controlName)!.SelectedItem as ChoiceValue)?.Value ?? string.Empty;
    }

    private string SelectedTag(string controlName)
    {
        return (FindControl<ComboBox>(controlName)?.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? string.Empty;
    }

    private IBrush ResourceBrush(string key)
    {
        return Avalonia.Application.Current?.Resources[key] as IBrush ?? Brushes.Gray;
    }

    private string DocumentLanguageSuffix() => _language == UiLanguage.English ? "EN" : "FR";

    private string L(string french, string english) => _language == UiLanguage.English ? english : french;

    private static bool Contains(string? value, string search) =>
        !string.IsNullOrWhiteSpace(value) && value.Contains(search, StringComparison.OrdinalIgnoreCase);

    private static string ThemePreferencePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "DanteConfigEditorV3",
        "theme-macos.txt");

    private static bool LoadThemePreference()
    {
        try
        {
            return !File.Exists(ThemePreferencePath)
                || !string.Equals(File.ReadAllText(ThemePreferencePath).Trim(), "light", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return true;
        }
    }

    private static void SaveThemePreference(bool dark)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ThemePreferencePath)!);
            File.WriteAllText(ThemePreferencePath, dark ? "dark" : "light");
        }
        catch
        {
        }
    }
}
