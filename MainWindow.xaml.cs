using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using DanteConfigEditor.Models;
using DanteConfigEditor.Services;
using Microsoft.Win32;

namespace DanteConfigEditor;

public partial class MainWindow : Window
{
    private string AllSendersItem => T("Filter.AllSenders");
    private string AllReceiversItem => T("Filter.AllReceivers");

    // Collections liées directement aux listes WPF. Quand on les modifie,
    // l'interface se met à jour sans recréer toute la fenêtre.
    private readonly ObservableCollection<DeviceRow> _deviceRows = [];
    private readonly ObservableCollection<DanteSubscription> _patchRows = [];
    private readonly ObservableCollection<string> _logs = [];
    private readonly ObservableCollection<GlobalSearchResult> _searchResults = [];
    private readonly ObservableCollection<DanteValidationIssue> _healthIssues = [];
    private readonly HashSet<string> _lockedDeviceNames = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _warningDeviceNames = new(StringComparer.OrdinalIgnoreCase);
    private readonly DispatcherTimer _recoveryTimer;
    private readonly SupportReminderSettingsService _supportReminderSettings = new();
    private CancellationTokenSource? _recoveryWriteCancellation;
    private string? _selectedWarningKey;
    private readonly LatencyChoice[] _latencies =
    [
        new("250", "0,25 ms"),
        new("1000", "1 ms"),
        new("2000", "2 ms"),
        new("5000", "5 ms")
    ];
    private readonly SampleRateChoice[] _sampleRates =
    [
        new("44100", "44,1 kHz"),
        new("48000", "48 kHz"),
        new("88200", "88,2 kHz"),
        new("96000", "96 kHz"),
        new("176400", "176,4 kHz"),
        new("192000", "192 kHz")
    ];
    private readonly EncodingChoice[] _encodings =
    [
        new("16", "16 bit"),
        new("24", "24 bit"),
        new("32", "32 bit")
    ];
    private readonly string[] _patchViewModeKeys = [PatchViewMode.SimpleKey, PatchViewMode.ExpertKey];
    private readonly string[] _patchStateFilterKeys =
    [
        "Filter.AllRx",
        "Filter.ActivePatches",
        "Filter.FreeRx",
        "Filter.LocalPatches",
        "Filter.MissingTxDevices",
        "Filter.MissingTxChannels",
        "Filter.Warnings",
        "Filter.Conflicts",
        "Filter.Modified"
    ];
    private readonly string[] _healthFilterKeys =
    [
        "Filter.All",
        "Filter.Info",
        "Filter.HealthWarnings",
        "Filter.Errors",
        "Filter.Patches",
        "Filter.Devices",
        "Filter.Clock",
        "Filter.Network",
        "Filter.XmlCompatibility"
    ];
    private readonly string[] _patchbookScopeKeys = ["Filter.AllRx", "Filter.ActivePatches", "Filter.WarningsConflicts"];
    private readonly string[] _deviceFilterKeys =
    [
        "DeviceFilter.All",
        "DeviceFilter.Locked",
        "DeviceFilter.StaticIp",
        "DeviceFilter.PreferredMaster",
        "DeviceFilter.Redundant",
        "DeviceFilter.Daisychain",
        "DeviceFilter.NoTx",
        "DeviceFilter.NoRx",
        "DeviceFilter.Modified",
        "DeviceFilter.WarningSelection",
        "DeviceFilter.SampleRateDifferent",
        "DeviceFilter.EncodingDifferent"
    ];
    private readonly string[] _targetScopeKeys =
    [
        "Target.AllUnlocked",
        "Target.SelectedUnlocked",
        "Target.FilteredUnlocked"
    ];
    private DanteProject? _project;
    private DanteProject? _easyPatchProject;
    private PatchWorkspaceView? _easyPatchWorkspace;
    private UiLanguage _language = UiLanguage.French;
    private bool _editModeEnabled;

    // Évite que les changements de sélection déclenchés par RefreshAll relancent
    // eux-mêmes des actions utilisateur.
    private bool _refreshingUi;
    private bool _committingPatchInlineRename;
    private string? _patchSeriesDeviceName;
    private DanteChannelKind? _patchSeriesKind;
    private int[] _patchSeriesSeeds = [];

    private sealed record ChannelChoice(DanteChannelKind Kind, int Index, string Name)
    {
        public override string ToString()
        {
            return $"{Index} - {Name}";
        }
    }

    private sealed record TxChannelChoice(string DeviceName, int DanteId, string ChannelName)
    {
        public override string ToString()
        {
            return $"{DanteId:000} - {ChannelName}";
        }
    }

    private sealed record LatencyChoice(string XmlValue, string Display)
    {
        public override string ToString()
        {
            return Display;
        }
    }

    private sealed record SampleRateChoice(string XmlValue, string Display)
    {
        public override string ToString()
        {
            return Display;
        }
    }

    private sealed record EncodingChoice(string XmlValue, string Display)
    {
        public override string ToString()
        {
            return Display;
        }
    }

    private sealed record LocalizedOption(string Key, string Display)
    {
        public override string ToString()
        {
            return Display;
        }
    }

    private sealed record ProfileChoice(DeviceProfile Profile, string Display)
    {
        public override string ToString()
        {
            return Display;
        }
    }

    private sealed record GlobalSearchResult(
        string Kind,
        string Label,
        string? DeviceName = null,
        DanteChannelKind? ChannelKind = null,
        int? ChannelIndex = null,
        string? RxDevice = null,
        int? RxIndex = null)
    {
        public override string ToString()
        {
            return $"{Kind} - {Label}";
        }
    }

    private sealed class DeviceRow
    {
        public DeviceRow(DanteDevice device, bool isLocked, bool isModified)
        {
            Device = device;
            IsLocked = isLocked;
            IsModified = isModified;
        }

        public DanteDevice Device { get; }

        public bool IsLocked { get; set; }

        public bool IsModified { get; }

        public string Name => Device.Name;

        public string FriendlyName => Device.FriendlyName;

        public string NetworkMode => Device.NetworkMode;

        public string LatencyDisplay => Device.LatencyDisplay;

        public string SampleRateDisplay => Device.SampleRateDisplay;

        public string EncodingDisplay => Device.EncodingDisplay;

        public string IpModeDisplay => Device.IpModeDisplay;

        public bool PreferredMaster => Device.PreferredMaster;

        public int TxCount => Device.TxCount;

        public int RxCount => Device.RxCount;
    }

    private sealed record TargetDeviceSet(DanteDevice[] Devices, int LockedSkippedCount, string ScopeLabel);

    public MainWindow()
    {
        InitializeComponent();
        _recoveryTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(750)
        };
        _recoveryTimer.Tick += RecoveryTimer_Tick;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // Initialisation des sources de données utilisées par les contrôles.
        _language = LanguageSettingsService.Load();
        ConfigurationEditorsGrid.Visibility = InterfaceSettingsService.LoadConfigurationEditorsExpanded()
            ? Visibility.Visible
            : Visibility.Collapsed;
        SessionRecoveryService.CleanupOld(TimeSpan.FromDays(30));
        SetupLanguageComboBox();
        LatencyComboBox.ItemsSource = _latencies;
        GlobalLatencyComboBox.ItemsSource = _latencies;
        GlobalSampleRateComboBox.ItemsSource = _sampleRates;
        GlobalEncodingComboBox.ItemsSource = _encodings;
        ChannelKindComboBox.ItemsSource = new[] { "TX", "RX" };
        ChannelKindComboBox.SelectedItem = "TX";
        RefreshLocalizedOptionSources();
        RefreshQuickProfileOptions();
        PatchGrid.ItemsSource = _patchRows;
        DeviceGrid.ItemsSource = _deviceRows;
        LogListBox.ItemsSource = _logs;
        GlobalSearchListBox.ItemsSource = _searchResults;
        HealthIssuesGrid.ItemsSource = _healthIssues;
        SynopticDeviceGrid.ItemsSource = _synopticRows;
        GlobalDaisychainRadioButton.IsChecked = true;
        DaisychainRadioButton.IsChecked = true;
        SetTheme(useLightTheme: false);
        ApplyLanguageToInterface();
        RefreshRecentFiles();
        RefreshAll();
        UpdateResponsiveConfigurationLayout(ActualWidth, ActualHeight);
        InitializeSupportReminder();
    }

    private void OpenButton_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog dialog = new()
        {
            Filter = T("Dialog.XmlFilter"),
            Title = T("Dialog.OpenXmlTitle")
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        LoadProjectFromPath(dialog.FileName);
    }

    private void NewProjectButton_Click(object sender, RoutedEventArgs e)
    {
        if (_project?.IsModified == true)
        {
            MessageBoxResult abandon = MessageBox.Show(
                this,
                _language == UiLanguage.English
                    ? "The current project has unsaved changes. Create another project without saving them?"
                    : "Le projet courant contient des changements non enregistrés. Créer un autre projet sans les sauvegarder ?",
                T("Dialog.ConfirmTitle"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (abandon != MessageBoxResult.Yes)
            {
                return;
            }
        }

        NewProjectWindow window = new(
            _language,
            ThemeToggleButton.IsChecked == true)
        {
            Owner = this
        };
        if (window.ShowDialog() != true || window.Result is null)
        {
            return;
        }

        try
        {
            NewProjectFormResult form = window.Result;
            DanteProject created;
            if (form.TemplateId.HasValue)
            {
                MachineTemplatePackage template = new MachineBankRepository(
                    form.BankPath).Load(form.TemplateId.Value);
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
            _editModeEnabled = true;
            _logs.Clear();
            RecentFilesService.Add(form.DestinationPath);
            RefreshRecentFiles();
            AddLog(_language == UiLanguage.English
                ? $"Experimental project created: {form.DestinationPath}"
                : $"Projet expérimental créé : {form.DestinationPath}");
            RefreshAll();
            SetStatus(_language == UiLanguage.English
                ? "Experimental project created. Validate it in Dante Controller."
                : "Projet expérimental créé. Validez-le dans Dante Controller.");
            MessageBox.Show(
                this,
                _language == UiLanguage.English
                    ? "The XML was created and reloaded by DCE. This validates its internal structure only. "
                    + "Import it in Dante Controller before production use."
                    : "Le XML a été créé puis rechargé par DCE. Cela valide seulement sa structure interne. "
                    + "Importez-le dans Dante Controller avant tout usage en production.",
                _language == UiLanguage.English
                    ? "Manual validation required"
                    : "Validation manuelle obligatoire",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            ShowError(
                _language == UiLanguage.English
                    ? "Unable to create project"
                    : "Création du projet impossible",
                ex);
        }
    }

    private void MergeXmlButton_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureProjectLoaded())
        {
            return;
        }

        OpenFileDialog dialog = new()
        {
            Filter = T("Dialog.XmlFilter"),
            Title = T("Dialog.MergeXmlTitle")
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        IReadOnlyDictionary<string, string> renameMap = new Dictionary<string, string>();
        IReadOnlyList<string> duplicateNames;
        try
        {
            duplicateNames = _project!.FindDuplicateDeviceNamesInXml(dialog.FileName);
        }
        catch (Exception ex)
        {
            ShowError(T("Dialog.OpenFailedTitle"), ex);
            return;
        }

        if (duplicateNames.Count > 0)
        {
            DuplicateDeviceRenameWindow duplicateWindow = new(
                _language,
                duplicateNames,
                suffix => _project!.BuildAutomaticDuplicateRenameMap(dialog.FileName, suffix))
            {
                Owner = this
            };

            if (duplicateWindow.ShowDialog() != true || duplicateWindow.Choice == DuplicateDeviceImportChoice.Cancel)
            {
                return;
            }

            renameMap = duplicateWindow.RenameMap;
        }
        else
        {
            MessageBoxResult confirm = MessageBox.Show(this, T("Dialog.MergeXmlWarning"), T("Dialog.ConfirmTitle"), MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.Yes)
            {
                return;
            }
        }

        DanteMergeResult? mergeResult = null;
        RunProjectAction(
            T("Action.XmlMerged"),
            () => mergeResult = _project!.MergeDevicesFromXml(dialog.FileName, renameMap));

        if (mergeResult is not null)
        {
            AddLog(BuildMergeResultLog(dialog.FileName, mergeResult));
            SetStatus(BuildMergeResultStatus(mergeResult));
        }
    }

    private void OpenRecentButton_Click(object sender, RoutedEventArgs e)
    {
        if (RecentFilesComboBox.SelectedItem is not string path || string.IsNullOrWhiteSpace(path))
        {
            ShowError(T("Dialog.NoRecentFileTitle"), T("Dialog.NoRecentFileMessage"));
            return;
        }

        if (!File.Exists(path))
        {
            ShowError(T("Dialog.FileMissingTitle"), T("Dialog.FileMissingMessage"));
            RefreshRecentFiles();
            return;
        }

        LoadProjectFromPath(path);
    }

    public void LoadProjectFromPath(string path)
    {
        try
        {
            // DanteProject contient toute la logique XML. La fenêtre ne garde que
            // l'état d'affichage et les actions utilisateur.
            RecoveryCandidate? recovery = SessionRecoveryService.Find(path);
            bool recovered = false;
            DanteProject? loadedProject = null;
            if (recovery is not null)
            {
                string sourceWarning = recovery.SourceMatches
                    ? string.Empty
                    : Environment.NewLine + Environment.NewLine + T("Dialog.RecoverySourceChanged");
                MessageBoxResult restore = MessageBox.Show(
                    this,
                    Tf("Dialog.RecoveryFound", recovery.SavedAtUtc.ToLocalTime()) + sourceWarning,
                    T("Dialog.RecoveryTitle"),
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                if (restore == MessageBoxResult.Yes)
                {
                    loadedProject = DanteProject.LoadRecovered(path, recovery.RecoveryXmlPath);
                    recovered = true;
                }
                else
                {
                    SessionRecoveryService.Delete(path);
                }
            }

            _project = loadedProject ?? DanteProject.Load(path);
            _editModeEnabled = true;
            _logs.Clear();
            RecentFilesService.Add(path);
            RefreshRecentFiles();
            AddLog(Tf("Log.FileLoaded", path));
            if (recovered)
            {
                AddLog(T("Log.RecoveryRestored"));
            }
            RefreshAll();
            SetStatus(T(recovered ? "Status.RecoveryRestored" : "Status.FileLoaded"));
        }
        catch (Exception ex)
        {
            ShowError(T("Dialog.OpenFailedTitle"), ex);
        }
    }

    private void ActivateEditButton_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureProjectLoaded())
        {
            return;
        }

        MessageBoxResult confirm = MessageBox.Show(
            this,
            "Vous allez activer l'édition du fichier XML hors ligne. Travaillez toujours sur une copie et vérifiez le fichier final dans Dante Controller avant production.",
            T("Status.ActivateEditButton"),
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes)
        {
            return;
        }

        _editModeEnabled = true;
        AddLog(T("Log.EditEnabled"));
        RefreshAll();
        SetStatus(T("Status.EditEnabled"));
    }

    private void SaveAsButton_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureProjectLoaded())
        {
            return;
        }

        if (!EnsureEditMode())
        {
            return;
        }

        // La validation est relancée juste avant la sauvegarde pour éviter de
        // créer un fichier final manifestement invalide.
        DanteValidationResult validation = _project!.Validate();
        if (validation.HasErrors)
        {
            ShowError(T("Dialog.SaveImpossibleTitle"), validation.ToDisplayText());
            return;
        }

        SaveFileDialog dialog = new()
        {
            Filter = T("Dialog.XmlFilter"),
            Title = T("Dialog.SaveXmlTitle"),
            FileName = Path.GetFileName(SafeFileService.BuildDefaultSavePath(_project.OriginalFilePath)),
            InitialDirectory = Path.GetDirectoryName(_project.OriginalFilePath)
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        if (IsOriginalProjectPath(dialog.FileName))
        {
            ShowError(
                T("Dialog.ChooseAnotherNameTitle"),
                T("Dialog.ChooseAnotherNameMessage"));
            return;
        }

        if (File.Exists(dialog.FileName))
        {
            MessageBoxResult overwrite = MessageBox.Show(
                this,
                T("Dialog.OverwriteMessage"),
                T("Dialog.ConfirmTitle"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (overwrite != MessageBoxResult.Yes)
            {
                return;
            }
        }

        string summary = _project.BuildSaveSummary();
        MessageBoxResult confirm = MessageBox.Show(
            this,
            summary + Environment.NewLine + Environment.NewLine + T("Dialog.OriginalBackupMessage"),
            T("Dialog.SaveSummaryTitle"),
            MessageBoxButton.YesNo,
            validation.HasWarnings ? MessageBoxImage.Warning : MessageBoxImage.Information);

        if (confirm != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            CancelPendingRecoveryWrite();
            string previousFilePath = _project.OriginalFilePath;
            string backupPath = _project.SaveAs(dialog.FileName);
            SessionRecoveryService.Delete(previousFilePath);
            SessionRecoveryService.Delete(_project.OriginalFilePath);
            RecentFilesService.Add(_project.OriginalFilePath);
            AddLog(Tf("Log.OriginalBackupCreated", backupPath));
            AddLog(Tf("Log.FileSaved", dialog.FileName));
            RefreshAll();
            SetStatus(T("Status.FileSaved"));
        }
        catch (Exception ex)
        {
            ShowError(T("Dialog.SaveErrorTitle"), ex);
        }
    }

    private void RevertButton_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureProjectLoaded())
        {
            return;
        }

        if (_project!.IsModified)
        {
            MessageBoxResult confirm = MessageBox.Show(
                this,
                T("Dialog.RevertMessage"),
                T("Dialog.RevertTitle"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes)
            {
                return;
            }
        }

        try
        {
            string path = _project.OriginalFilePath;
            SessionRecoveryService.Delete(path);
            _project = DanteProject.Load(path);
            AddLog(T("Log.ReloadOriginal"));
            RefreshAll();
            SetStatus(T("Log.ReloadOriginal"));
        }
        catch (Exception ex)
        {
            ShowError(T("Dialog.ReloadErrorTitle"), ex);
        }
    }

    private void UndoLastButton_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureProjectLoaded())
        {
            return;
        }

        try
        {
            string label = _project!.UndoLastChange();
            AddLog(Tf("Log.ActionUndone", label));
            RefreshAll();
            ScheduleRecoverySnapshot();
            SetStatus(T("Status.LastActionUndone"));
        }
        catch (Exception ex)
        {
            ShowError(T("Dialog.UndoErrorTitle"), ex);
        }
    }

    private void DeviceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_refreshingUi || _project is null)
        {
            return;
        }

        DanteDevice? device = _project.FindDevice(DeviceComboBox.SelectedItem as string);
        if (device is null)
        {
            return;
        }

        NewNameTextBox.Text = device.Name;
        RedundantRadioButton.IsChecked = device.IsRedundant;
        DaisychainRadioButton.IsChecked = !device.IsRedundant;
        SelectLatency(LatencyComboBox, device.Latency);
        PreferredMasterCheckBox.IsChecked = device.PreferredMaster;
        RefreshChannelSelector();
    }

    private void ApplyDeviceSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureProjectLoaded())
        {
            return;
        }

        string originalName = SelectedDeviceName();
        DanteDevice device = _project!.FindDevice(originalName) ?? throw new InvalidOperationException("Device introuvable.");
        string newName = NewNameTextBox.Text.Trim();
        bool isRedundant = RedundantRadioButton.IsChecked == true;
        string latency = SelectedLatencyXmlValue(LatencyComboBox);
        bool preferredMaster = PreferredMasterCheckBox.IsChecked == true;

        bool latencyChanged = !string.Equals(device.Latency, latency, StringComparison.OrdinalIgnoreCase);
        bool hasChanges = !string.Equals(device.Name, newName, StringComparison.Ordinal)
            || device.IsRedundant != isRedundant
            || latencyChanged
            || device.PreferredMaster != preferredMaster;

        if (!hasChanges)
        {
            SetStatus(T("Status.NoDeviceSettingsChanged"));
            return;
        }

        RunProjectAction(
            T("Action.DeviceSettingsUpdated"),
            () => _project!.ApplyBatch(_ => ApplySelectedDeviceSettings(originalName, newName, isRedundant, latency, preferredMaster)),
            latencyChanged ? T("Dialog.LatencyWarning") : null);
    }

    private void ResetDevicePatchesButton_Click(object sender, RoutedEventArgs e)
    {
        string deviceName = SelectedDeviceName();
        RunProjectAction(
            T("Action.DevicePatchesReset"),
            () => _project!.ResetDevicePatches(deviceName),
            Tf("Dialog.ResetDevicePatchesWarning", deviceName));
    }

    private void ResetDeviceRxPatchesButton_Click(object sender, RoutedEventArgs e)
    {
        string deviceName = SelectedDeviceName();
        RunProjectAction(
            T("Action.DeviceRxPatchesReset"),
            () => _project!.ResetDeviceRxPatches(deviceName),
            $"Les entrées RX de la machine '{deviceName}' seront déconnectées. Continuer ?");
    }

    private void ResetDeviceTxPatchesButton_Click(object sender, RoutedEventArgs e)
    {
        string deviceName = SelectedDeviceName();
        RunProjectAction(
            T("Action.DeviceTxPatchesReset"),
            () => _project!.ResetDeviceTxPatches(deviceName),
            $"Tous les patchs qui utilisent les TX de la machine '{deviceName}' seront supprimés. Continuer ?");
    }

    private void ApplySelectedDeviceSettings(
        string originalName,
        string newName,
        bool isRedundant,
        string latency,
        bool preferredMaster)
    {
        DanteDevice originalDevice = _project!.FindDevice(originalName) ?? throw new InvalidOperationException("Device introuvable.");
        string currentName = originalDevice.Name;

        // Une seule validation utilisateur applique tous les champs visibles de la machine.
        if (!string.Equals(originalDevice.Name, newName, StringComparison.Ordinal))
        {
            _project.RenameDevice(currentName, newName);
            currentName = newName;
        }

        if (originalDevice.IsRedundant != isRedundant)
        {
            _project.SetNetworkMode(currentName, isRedundant);
        }

        if (!string.Equals(originalDevice.Latency, latency, StringComparison.OrdinalIgnoreCase))
        {
            _project.SetLatency(currentName, latency);
        }

        if (originalDevice.PreferredMaster != preferredMaster)
        {
            _project.SetPreferredMaster(currentName, preferredMaster);
        }
    }

    private void DeleteDeviceButton_Click(object sender, RoutedEventArgs e)
    {
        string deviceName = SelectedDeviceName();
        RunProjectAction(
            T("Action.DeviceDeleted"),
            () => _project!.DeleteDevice(deviceName),
            Tf("Dialog.DeleteDeviceWarning", deviceName));
    }

    private void OpenMachineBankButton_Click(object sender, RoutedEventArgs e)
    {
        MachineBankWindow window = new(
            _language,
            ThemeToggleButton.IsChecked == true,
            _project?.Devices.Select(device => device.Name) ?? [],
            _project is not null && _editModeEnabled)
        {
            Owner = this
        };
        if (window.ShowDialog() != true
            || window.SelectedPackageToAdd is null
            || window.SelectedInstanceOptions is null
            || _project is null)
        {
            return;
        }

        MachineTemplatePackage package = window.SelectedPackageToAdd;
        MachineInstanceOptions options = window.SelectedInstanceOptions;
        bool completed = RunProjectAction(
            _language == UiLanguage.English
                ? $"Device added from bank: {options.NewName}"
                : $"Machine ajoutée depuis la banque : {options.NewName}",
            () => _project.AddDeviceFromTemplate(package, options));
        if (completed)
        {
            DeviceComboBox.SelectedItem = options.NewName;
        }
    }

    private void DuplicateDeviceButton_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureProjectLoaded())
        {
            return;
        }

        string? sourceName = DeviceComboBox.SelectedItem as string;
        DanteDevice? source = _project!.FindDevice(sourceName);
        if (source is null)
        {
            ShowError(
                LocalizeLiteral("Action impossible"),
                LocalizeLiteral("Sélectionnez d'abord une machine."));
            return;
        }

        HashSet<string> usedNames = _project.Devices
            .Select(device => device.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        string suggestedName = DanteNameRules.BuildUniqueSuffixedDeviceName(
            source.Name,
            _language == UiLanguage.English ? "Copy" : "Copie",
            usedNames);
        MachineCloneWindow window = new(
            _language,
            ThemeToggleButton.IsChecked == true,
            source.Name,
            suggestedName)
        {
            Owner = this
        };
        if (window.ShowDialog() != true || window.Options is null)
        {
            return;
        }

        string newName = window.Options.NewName;
        bool completed = RunProjectAction(
            _language == UiLanguage.English
                ? $"Device duplicated: {source.Name} -> {newName}"
                : $"Machine dupliquée : {source.Name} -> {newName}",
            () => _project.DuplicateDevice(source.Name, window.Options));
        if (completed)
        {
            DeviceComboBox.SelectedItem = newName;
        }
    }

    private void SaveDeviceToBankButton_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureProjectLoaded())
        {
            return;
        }

        string? deviceName = DeviceComboBox.SelectedItem as string;
        DanteDevice? device = _project!.FindDevice(deviceName);
        if (device is null)
        {
            ShowError(
                LocalizeLiteral("Action impossible"),
                LocalizeLiteral("Sélectionnez d'abord une machine."));
            return;
        }

        string manufacturer = device.Manufacturer;
        string model = device.Model;
        string suggestedTemplateName = string.Join(
            " ",
            new[] { manufacturer, model }
                .Where(value => !string.IsNullOrWhiteSpace(value)));
        if (string.IsNullOrWhiteSpace(suggestedTemplateName))
        {
            suggestedTemplateName = device.Name;
        }

        MachineTemplateEditorWindow window = new(
            _language,
            ThemeToggleButton.IsChecked == true,
            _language == UiLanguage.English
                ? "Save to device bank"
                : "Enregistrer dans la banque de machines",
            _language == UiLanguage.English
                ? $"Prepare a reusable template from {device.Name}. Hardware identity, network addresses, subscriptions and multicast flows will be removed."
                : $"Préparez un modèle réutilisable à partir de {device.Name}. L'identité matérielle, le réseau, les subscriptions et les flows multicast seront retirés.",
            suggestedTemplateName,
            manufacturer,
            model,
            string.Empty,
            string.Empty,
            [],
            device.TxChannels.Select(channel => channel.DisplayName),
            device.RxChannels.Select(channel => channel.DisplayName))
        {
            Owner = this
        };
        if (window.ShowDialog() != true || window.Result is null)
        {
            return;
        }

        try
        {
            MachineTemplateFormResult form = window.Result;
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
            AddLog((_language == UiLanguage.English
                ? "Device template saved: "
                : "Modèle de machine enregistré : ") + saved.TemplateName);
            SetStatus(_language == UiLanguage.English
                ? "Device template saved."
                : "Modèle enregistré dans la banque.");
            MessageBox.Show(
                this,
                _language == UiLanguage.English
                    ? $"“{saved.TemplateName}” was saved in:{Environment.NewLine}{bankPath}"
                    : $"« {saved.TemplateName} » a été enregistré dans :{Environment.NewLine}{bankPath}",
                _language == UiLanguage.English ? "Device bank" : "Banque de machines",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            DiagnosticLogService.Default.Write(
                "MachineBank",
                "Impossible d'enregistrer un modèle depuis la machine sélectionnée.",
                ex);
            ShowError(
                _language == UiLanguage.English
                    ? "Unable to save template"
                    : "Enregistrement du modèle impossible",
                ex);
        }
    }

    private void ResetDeviceChannelsButton_Click(object sender, RoutedEventArgs e)
    {
        RunProjectAction(
            T("Action.ChannelsReset"),
            () => _project!.ResetChannels(SelectedDeviceName()),
            T("Dialog.ResetDeviceChannelsWarning"));
    }

    private void RenameChannelButton_Click(object sender, RoutedEventArgs e)
    {
        if (ChannelComboBox.SelectedItem is not ChannelChoice channel)
        {
            ShowError(T("Dialog.NoChannelTitle"), T("Dialog.NoChannelMessage"));
            return;
        }

        RunProjectAction(T("Action.ChannelRenamed"), () =>
        {
            _project!.RenameChannel(SelectedDeviceName(), channel.Kind, channel.Index, NewChannelNameTextBox.Text);
        });
    }

    private void BatchRenameButton_Click(object sender, RoutedEventArgs e)
    {
        // Le type TX/RX choisi dans l'écran sert aussi aux listes de plage.
        DanteChannelKind kind = string.Equals(ChannelKindComboBox.SelectedItem as string, "RX", StringComparison.OrdinalIgnoreCase)
            ? DanteChannelKind.Rx
            : DanteChannelKind.Tx;

        if (BatchRenameStartChannelComboBox.SelectedItem is not ChannelChoice startChannel
            || BatchRenameEndChannelComboBox.SelectedItem is not ChannelChoice endChannel)
        {
            ShowError(T("Dialog.InvalidRangeTitle"), T("Dialog.InvalidRangeMessage"));
            return;
        }

        if (startChannel.Kind != kind || endChannel.Kind != kind || startChannel.Index > endChannel.Index)
        {
            ShowError(T("Dialog.InvalidRangeTitle"), T("Dialog.InvalidRangeOrderMessage"));
            return;
        }

        if (!int.TryParse(BatchRenameStartTextBox.Text.Trim(), out int firstNumber))
        {
            ShowError(T("Dialog.InvalidNumberTitle"), T("Dialog.InvalidNumberMessage"));
            return;
        }

        RunProjectAction(
            T("Action.BatchRenameApplied"),
            () => _project!.BatchRenameChannels(SelectedDeviceName(), kind, BatchRenamePrefixTextBox.Text, firstNumber, startChannel.Index, endChannel.Index),
            Tf("Dialog.BatchRenameWarning", kind, startChannel.Index, endChannel.Index));
    }

    private void ApplyAllNetworkButton_Click(object sender, RoutedEventArgs e)
    {
        TargetDeviceSet? target = GetTargetDeviceSet();
        if (target is null)
        {
            return;
        }

        bool redundant = GlobalRedundantRadioButton.IsChecked == true;
        string targetLabel = redundant ? "Redondant" : "Daisychain";
        RunProjectAction(
            T("Action.AllNetworkModesApplied"),
            () => _project!.ApplyBatch(project =>
            {
                foreach (DanteDevice device in target.Devices)
                {
                    project.SetNetworkMode(device.Name, redundant);
                }
            }),
            BuildTargetPreview(
                $"appliquer le mode réseau {targetLabel}",
                target,
                target.Devices.Select(device => (
                    Device: device,
                    Before: device.NetworkMode,
                    After: targetLabel,
                    Changed: device.IsRedundant != redundant))) + Environment.NewLine + T("Dialog.Continue"));
    }

    private void ApplyAllLatencyButton_Click(object sender, RoutedEventArgs e)
    {
        TargetDeviceSet? target = GetTargetDeviceSet();
        if (target is null)
        {
            return;
        }

        string latency = SelectedLatencyXmlValue(GlobalLatencyComboBox);
        string latencyDisplay = DanteLatencyFormatter.FormatLatencyDisplay(latency);
        RunProjectAction(
            T("Action.AllLatenciesApplied"),
            () => _project!.ApplyBatch(project =>
            {
                foreach (DanteDevice device in target.Devices)
                {
                    project.SetLatency(device.Name, latency);
                }
            }),
            BuildTargetPreview(
                $"appliquer la latence {latencyDisplay}",
                target,
                target.Devices.Select(device => (
                    Device: device,
                    Before: device.LatencyDisplay,
                    After: latencyDisplay,
                    Changed: !string.Equals(device.Latency, latency, StringComparison.OrdinalIgnoreCase))))
                + Environment.NewLine
                + T("Dialog.LatencyWarningContinue"));
    }

    private void ApplyAllSampleRateButton_Click(object sender, RoutedEventArgs e)
    {
        TargetDeviceSet? target = GetTargetDeviceSet();
        if (target is null)
        {
            return;
        }

        string samplerate = SelectedSampleRateXmlValue(GlobalSampleRateComboBox);
        string samplerateDisplay = GlobalSampleRateComboBox.Text;
        RunProjectAction(
            T("Action.AllSampleRatesApplied"),
            () => _project!.ApplyBatch(project =>
            {
                foreach (DanteDevice device in target.Devices)
                {
                    project.SetSamplerate(device.Name, samplerate);
                }
            }),
            BuildTargetPreview(
                $"appliquer la sample rate {samplerateDisplay}",
                target,
                target.Devices.Select(device => (
                    Device: device,
                    Before: device.SampleRateDisplay,
                    After: samplerateDisplay,
                    Changed: !string.Equals(device.Samplerate, samplerate, StringComparison.OrdinalIgnoreCase))))
                + Environment.NewLine
                + T("Dialog.AudioFormatWarningContinue"));
    }

    private void ApplyAllEncodingButton_Click(object sender, RoutedEventArgs e)
    {
        TargetDeviceSet? target = GetTargetDeviceSet();
        if (target is null)
        {
            return;
        }

        string encoding = SelectedEncodingXmlValue(GlobalEncodingComboBox);
        string encodingDisplay = GlobalEncodingComboBox.Text;
        RunProjectAction(
            T("Action.AllEncodingsApplied"),
            () => _project!.ApplyBatch(project =>
            {
                foreach (DanteDevice device in target.Devices)
                {
                    project.SetEncoding(device.Name, encoding);
                }
            }),
            BuildTargetPreview(
                $"appliquer les bits par échantillon {encodingDisplay}",
                target,
                target.Devices.Select(device => (
                    Device: device,
                    Before: device.EncodingDisplay,
                    After: encodingDisplay,
                    Changed: !string.Equals(device.Encoding, encoding, StringComparison.OrdinalIgnoreCase))))
                + Environment.NewLine
                + T("Dialog.AudioFormatWarningContinue"));
    }

    private void ApplyQuickProfileButton_Click(object sender, RoutedEventArgs e)
    {
        if (QuickProfileComboBox.SelectedItem is not ProfileChoice choice)
        {
            ShowError(T("Dialog.ActionImpossibleTitle"), T("Dialog.SelectProfile"));
            return;
        }

        TargetDeviceSet? target = GetTargetDeviceSet();
        if (target is null)
        {
            return;
        }

        DeviceProfile profile = choice.Profile;
        (DanteDevice Device, string Before, string After, bool Changed)[] previewRows = target.Devices
            .Select(device => (
                Device: device,
                Before: BuildDeviceProfileState(device),
                After: BuildTargetProfileState(device, profile),
                Changed: DeviceDiffersFromProfile(device, profile)))
            .ToArray();
        if (!previewRows.Any(row => row.Changed))
        {
            SetStatus(T("Status.ProfileAlreadyApplied"));
            return;
        }

        RunProjectAction(
            T("Action.QuickProfileApplied"),
            () => _project!.ApplyDeviceProfile(target.Devices.Select(device => device.Name), profile),
            BuildTargetPreview(
                $"appliquer le profil {choice.Display}",
                target,
                previewRows)
                + Environment.NewLine
                + T("Dialog.ProfileWarningContinue"));
    }

    private void ApplyExclusivePreferredMasterButton_Click(object sender, RoutedEventArgs e)
    {
        if (ExclusivePreferredMasterComboBox.SelectedItem is not string deviceName
            || string.IsNullOrWhiteSpace(deviceName))
        {
            ShowError(
                T("Dialog.ActionImpossibleTitle"),
                _language == UiLanguage.English
                    ? "Select the device that must remain Preferred Master."
                    : "Choisissez la machine qui doit rester Preferred Master.");
            return;
        }

        RunProjectAction(
            _language == UiLanguage.English
                ? $"{deviceName} is now the only Preferred Master."
                : $"{deviceName} est maintenant le seul Preferred Master.",
            () => _project!.SetExclusivePreferredMaster(deviceName));
    }

    private static bool DeviceDiffersFromProfile(DanteDevice device, DeviceProfile profile)
    {
        return !string.Equals(device.Samplerate, profile.Samplerate, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(device.Encoding, profile.Encoding, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(device.Latency, profile.Latency, StringComparison.OrdinalIgnoreCase)
            || profile.IsRedundant.HasValue && device.IsRedundant != profile.IsRedundant.Value
            || profile.SetIpAutomatic && device.UsesStaticIp;
    }

    private static string BuildDeviceProfileState(DanteDevice device)
    {
        return $"{device.SampleRateDisplay} / {device.EncodingDisplay} / {device.LatencyDisplay} / {device.NetworkMode} / {device.IpModeDisplay}";
    }

    private static string BuildTargetProfileState(DanteDevice device, DeviceProfile profile)
    {
        string samplerate = int.TryParse(profile.Samplerate, out int samplerateValue)
            ? $"{samplerateValue / 1000m:0.#} kHz"
            : profile.Samplerate;
        string networkMode = profile.IsRedundant.HasValue
            ? profile.IsRedundant.Value ? "Redondant" : "Daisychain"
            : device.NetworkMode;
        string ipMode = profile.SetIpAutomatic ? "Auto" : device.IpModeDisplay;
        return $"{samplerate} / {profile.Encoding} bit / {DanteLatencyFormatter.FormatLatencyDisplay(profile.Latency)} / {networkMode} / {ipMode}";
    }

    private void ApplyAllIpAutoButton_Click(object sender, RoutedEventArgs e)
    {
        TargetDeviceSet? target = GetTargetDeviceSet();
        if (target is null)
        {
            return;
        }

        RunProjectAction(
            T("Action.AllIpAutoApplied"),
            () => _project!.ApplyBatch(project =>
            {
                foreach (DanteDevice device in target.Devices)
                {
                    project.SetIpAddressDynamic(device.Name);
                }
            }),
            BuildTargetPreview(
                "mettre les adresses IPv4 en automatique",
                target,
                target.Devices.Select(device => (
                    Device: device,
                    Before: device.IpModeDisplay,
                    After: "Auto",
                    Changed: device.UsesStaticIp))) + Environment.NewLine + T("Dialog.Continue"));
    }

    private void ApplyAllIpStaticButton_Click(object sender, RoutedEventArgs e)
    {
        TargetDeviceSet? target = GetTargetDeviceSet();
        if (target is null)
        {
            return;
        }

        if (!int.TryParse(GlobalIpStartTextBox.Text.Trim(), out int startHost))
        {
            ShowError(T("Dialog.InvalidNumberTitle"), T("Dialog.InvalidNumberMessage"));
            return;
        }

        string prefix = GlobalIpPrefixTextBox.Text.Trim();
        string netmask = string.IsNullOrWhiteSpace(GlobalIpNetmaskTextBox.Text) ? "255.255.255.0" : GlobalIpNetmaskTextBox.Text.Trim();
        string gateway = string.IsNullOrWhiteSpace(GlobalIpGatewayTextBox.Text) ? "0.0.0.0" : GlobalIpGatewayTextBox.Text.Trim();
        if (!IsValidIpv4(netmask) || !IsValidIpv4(gateway))
        {
            ShowError(T("Dialog.ActionImpossibleTitle"), "Le masque ou la passerelle n'est pas une adresse IPv4 valide.");
            return;
        }

        DanteDevice[] configurableDevices = target.Devices
            .Where(device => _project!.SupportsIpConfiguration(device.Name))
            .ToArray();
        if (configurableDevices.Length == 0)
        {
            ShowError(T("Dialog.ActionImpossibleTitle"), "Aucune machine de la cible ne contient d'interface IPv4 modifiable.");
            return;
        }

        if (!TryBuildIpAddress(prefix, startHost + configurableDevices.Length - 1, out _, out string rangeError))
        {
            ShowError(T("Dialog.ActionImpossibleTitle"), rangeError);
            return;
        }

        Dictionary<string, string> targetAddressByDevice = configurableDevices
            .Select((device, index) => new { device.Name, Host = startHost + index })
            .ToDictionary(
                item => item.Name,
                item =>
                {
                    TryBuildIpAddress(prefix, item.Host, out string address, out _);
                    return address;
                },
                StringComparer.OrdinalIgnoreCase);

        RunProjectAction(
            T("Action.AllIpStaticApplied"),
            () => _project!.ApplyBatch(project =>
            {
                foreach (DanteDevice device in configurableDevices)
                {
                    project.SetIpAddressStatic(device.Name, targetAddressByDevice[device.Name], netmask, gateway);
                }
            }),
            BuildTargetPreview(
                $"fixer les IP depuis {prefix}.{startHost} / {netmask} / gateway {gateway}",
                target,
                target.Devices.Select(device =>
                {
                    bool configurable = targetAddressByDevice.TryGetValue(device.Name, out string? address);
                    return (
                        Device: device,
                        Before: device.IpModeDisplay,
                        After: configurable ? $"Fixe ({address})" : "non modifiable",
                        Changed: configurable && (!string.Equals(device.StaticIpAddress, address, StringComparison.OrdinalIgnoreCase)
                            || !string.Equals(device.StaticIpNetmask, netmask, StringComparison.OrdinalIgnoreCase)
                            || !string.Equals(device.StaticIpGateway, gateway, StringComparison.OrdinalIgnoreCase)));
                })) + Environment.NewLine + T("Dialog.IpStaticWarningContinue"));
    }

    private void ResetAllChannelsButton_Click(object sender, RoutedEventArgs e)
    {
        TargetDeviceSet? target = GetTargetDeviceSet();
        if (target is null)
        {
            return;
        }

        RunProjectAction(
            T("Action.AllChannelsReset"),
            () => _project!.ApplyBatch(project =>
            {
                foreach (DanteDevice device in target.Devices)
                {
                    project.ResetChannels(device.Name);
                }
            }),
            BuildTargetPreview(
                "réinitialiser les noms de canaux TX/RX",
                target,
                target.Devices.Select(device => (
                    Device: device,
                    Before: $"{device.TxCount} TX / {device.RxCount} RX",
                    After: "noms de canaux par défaut",
                    Changed: device.TxCount > 0 || device.RxCount > 0))) + Environment.NewLine + T("Dialog.Continue"));
    }

    private void ListRedundantButton_Click(object sender, RoutedEventArgs e)
    {
        ShowProjectList("Machines redondantes", project => project.ListRedundantDevices());
    }

    private void ListDaisychainButton_Click(object sender, RoutedEventArgs e)
    {
        ShowProjectList("Machines en daisychain", project => project.ListDaisychainDevices());
    }

    private void ListLatenciesButton_Click(object sender, RoutedEventArgs e)
    {
        ShowProjectList("Latences", project => project.ListLatencies());
    }

    private void ListSampleRatesButton_Click(object sender, RoutedEventArgs e)
    {
        ShowProjectList("Sample rates", project => project.ListSamplerates());
    }

    private void ListEncodingsButton_Click(object sender, RoutedEventArgs e)
    {
        ShowProjectList("Bits par échantillon", project => project.ListEncodings());
    }

    private void ListStaticIpsButton_Click(object sender, RoutedEventArgs e)
    {
        ShowProjectList("IP fixes", project => project.ListStaticIpDevices());
    }

    private void ListPreferredMastersButton_Click(object sender, RoutedEventArgs e)
    {
        ShowProjectList("Preferred masters", project => project.ListPreferredMasters());
    }

    private void SenderDeviceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_refreshingUi || SenderDeviceList.SelectedItem is not string deviceName)
        {
            return;
        }

        if (!string.Equals(deviceName, AllSendersItem, StringComparison.OrdinalIgnoreCase))
        {
            SourceDeviceComboBox.SelectedItem = deviceName;
        }

        RefreshPatchRows();
    }

    private void ReceiverDeviceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_refreshingUi || ReceiverDeviceList.SelectedItem is not string deviceName)
        {
            return;
        }

        RefreshPatchRows();
    }

    private void PatchFilter_Changed(object sender, RoutedEventArgs e)
    {
        if (_refreshingUi)
        {
            return;
        }

        RefreshPatchRows();
    }

    private void PatchFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_refreshingUi)
        {
            return;
        }

        RefreshPatchRows();
    }

    private void PatchViewModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ApplyPatchViewMode();
    }

    private void HealthFilter_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (_refreshingUi)
        {
            return;
        }

        RefreshHealthPage();
    }

    private void DeviceFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_refreshingUi)
        {
            return;
        }

        RefreshAll();
    }

    private void ToggleConfigurationEditorsButton_Click(object sender, RoutedEventArgs e)
    {
        ConfigurationEditorsGrid.Visibility = ConfigurationEditorsGrid.Visibility == Visibility.Visible
            ? Visibility.Collapsed
            : Visibility.Visible;
        InterfaceSettingsService.SaveConfigurationEditorsExpanded(ConfigurationEditorsGrid.Visibility == Visibility.Visible);
        UpdateConfigurationEditorsToggleText();
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (!IsLoaded)
        {
            return;
        }

        UpdateResponsiveConfigurationLayout(e.NewSize.Width, e.NewSize.Height);
    }

    private void UpdateResponsiveConfigurationLayout(double width, double height)
    {
        // Le facteur DPI ne doit jamais cacher les réglages au premier lancement.
        // L'utilisateur garde la main avec le bouton Réduire/Afficher.
        UpdateConfigurationEditorsToggleText();
    }

    private void UpdateConfigurationEditorsToggleText()
    {
        bool collapsed = ConfigurationEditorsGrid.Visibility == Visibility.Collapsed;
        ToggleConfigurationEditorsButton.Content = LocalizeLiteral(collapsed ? "Afficher les réglages" : "Réduire les réglages");
        ToggleConfigurationEditorsButton.ToolTip = LocalizeLiteral(collapsed
            ? "Affiche les panneaux de réglage de la configuration."
            : "Masque les panneaux de réglage pour agrandir le tableau des machines.");
    }

    private void ImportantWarningsDetailsButton_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureProjectLoaded())
        {
            return;
        }

        IReadOnlyList<DanteImportantWarning> warnings = _project!.BuildImportantWarningDetails();
        if (warnings.Count == 0)
        {
            SetStatus(T("Status.NoImportantWarning"));
            return;
        }

        DanteImportantWarning? selectedWarning;
        if (warnings.Count == 1)
        {
            selectedWarning = warnings[0];
        }
        else
        {
            ImportantWarningsWindow window = new(_language, warnings)
            {
                Owner = this
            };
            selectedWarning = window.ShowDialog() == true ? window.SelectedWarning : null;
        }

        if (selectedWarning is not null)
        {
            ApplyImportantWarningFilter(selectedWarning);
        }
    }

    private void ApplyImportantWarningFilter(DanteImportantWarning warning)
    {
        _selectedWarningKey = warning.Key;
        _warningDeviceNames.Clear();
        _warningDeviceNames.UnionWith(warning.DeviceNames);
        if (_warningDeviceNames.Count == 0)
        {
            MessageBox.Show(this, LocalizedWarningMessage(warning), LocalizeLiteral("POINTS À VÉRIFIER"), MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        bool previousRefreshing = _refreshingUi;
        _refreshingUi = true;
        DeviceFilterComboBox.SelectedItem = (DeviceFilterComboBox.ItemsSource as IEnumerable<LocalizedOption>)
            ?.FirstOrDefault(option => option.Key == "DeviceFilter.WarningSelection");
        _refreshingUi = previousRefreshing;
        RefreshAll();
        DeviceGrid.SelectedItems.Clear();
        foreach (DeviceRow row in _deviceRows)
        {
            DeviceGrid.SelectedItems.Add(row);
        }

        if (_deviceRows.FirstOrDefault() is DeviceRow firstRow)
        {
            DeviceGrid.ScrollIntoView(firstRow);
        }

        SetStatus(Tf("Status.WarningDevicesDisplayed", _deviceRows.Count));
    }

    private void ShowDeviceChangesButton_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureProjectLoaded())
        {
            return;
        }

        IReadOnlyList<DeviceChangeRow> changes = _project!.BuildDeviceChangeRows();
        if (changes.Count == 0)
        {
            MessageBox.Show(this, T("Dialog.NoDeviceChanges"), T("Dialog.DeviceChangesTitle"), MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        DeviceChangesWindow window = new(_language, changes)
        {
            Owner = this
        };
        window.Show();
    }

    private void SelectVisibleDevicesButton_Click(object sender, RoutedEventArgs e)
    {
        DeviceGrid.SelectedItems.Clear();
        foreach (DeviceRow row in _deviceRows)
        {
            DeviceGrid.SelectedItems.Add(row);
        }

        SetStatus($"{_deviceRows.Count} machine(s) visible(s) sélectionnée(s).");
    }

    private void ClearDeviceSelectionButton_Click(object sender, RoutedEventArgs e)
    {
        DeviceGrid.SelectedItems.Clear();
        SetStatus("Sélection machines vidée.");
    }

    private void LockSelectedDevicesButton_Click(object sender, RoutedEventArgs e)
    {
        int count = SetSelectedDeviceLockState(true);
        SetStatus($"{count} machine(s) verrouillée(s).");
    }

    private void UnlockSelectedDevicesButton_Click(object sender, RoutedEventArgs e)
    {
        int count = SetSelectedDeviceLockState(false);
        SetStatus($"{count} machine(s) déverrouillée(s).");
    }

    private int SetSelectedDeviceLockState(bool locked)
    {
        DeviceRow[] rows = DeviceGrid.SelectedItems.OfType<DeviceRow>().ToArray();
        foreach (DeviceRow row in rows)
        {
            if (locked)
            {
                _lockedDeviceNames.Add(row.Name);
            }
            else
            {
                _lockedDeviceNames.Remove(row.Name);
            }
        }

        RefreshAll();
        return rows.Length;
    }

    private void GlobalSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        RefreshGlobalSearchResults();
    }

    private void GlobalSearchListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_refreshingUi || GlobalSearchListBox.SelectedItem is not GlobalSearchResult result)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(result.RxDevice) && result.RxIndex.HasValue)
        {
            MainTabs.SelectedIndex = 1;
            SenderDeviceList.SelectedItem = AllSendersItem;
            ReceiverDeviceList.SelectedItem = AllReceiversItem;
            RefreshPatchRows();
            DanteSubscription? subscription = _patchRows.FirstOrDefault(row =>
                string.Equals(row.RxDevice, result.RxDevice, StringComparison.OrdinalIgnoreCase)
                && row.RxIndex == result.RxIndex.Value);
            if (subscription is not null)
            {
                PatchGrid.SelectedItem = subscription;
                PatchGrid.ScrollIntoView(subscription);
            }

            return;
        }

        if (!string.IsNullOrWhiteSpace(result.DeviceName))
        {
            MainTabs.SelectedIndex = 0;
            DeviceComboBox.SelectedItem = result.DeviceName;

            if (result.ChannelKind.HasValue && result.ChannelIndex.HasValue)
            {
                ChannelKindComboBox.SelectedItem = result.ChannelKind.Value == DanteChannelKind.Rx ? "RX" : "TX";
                RefreshChannelSelector();
                ChannelChoice? channel = (ChannelComboBox.ItemsSource as IEnumerable<ChannelChoice>)
                    ?.FirstOrDefault(choice => choice.Kind == result.ChannelKind.Value && choice.Index == result.ChannelIndex.Value);
                if (channel is not null)
                {
                    ChannelComboBox.SelectedItem = channel;
                }
            }
        }
    }

    private void DeviceGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (DeviceGrid.SelectedItem is not DeviceRow row)
        {
            return;
        }

        OpenDeviceDetailsWindow(row.Name);
    }

    private void DeviceLockCheckBox_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not CheckBox checkBox || checkBox.DataContext is not DeviceRow row)
        {
            return;
        }

        if (checkBox.IsChecked == true)
        {
            _lockedDeviceNames.Add(row.Name);
        }
        else
        {
            _lockedDeviceNames.Remove(row.Name);
        }

        row.IsLocked = _lockedDeviceNames.Contains(row.Name);
        RefreshAll();
    }

    private void DevicePreferredMasterCheckBox_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not CheckBox checkBox || checkBox.DataContext is not DeviceRow row)
        {
            return;
        }

        RunProjectAction(
            T("Action.PreferredMasterUpdated"),
            () => _project!.SetPreferredMaster(row.Name, checkBox.IsChecked == true));
    }

    private void OpenDeviceDetailsButton_Click(object sender, RoutedEventArgs e)
    {
        OpenDeviceDetailsWindow(SelectedDeviceName());
    }

    private void OpenDeviceDetailsWindow(string deviceName)
    {
        if (!EnsureProjectLoaded())
        {
            return;
        }

        string currentDeviceName = deviceName;
        while (true)
        {
            DanteDevice device = _project!.FindDevice(currentDeviceName)
                ?? throw new InvalidOperationException("Device introuvable.");
            DeviceDetailsWindow window = new(
                _language,
                _project,
                device,
                ThemeToggleButton.IsChecked == true)
            {
                Owner = this
            };

            bool? accepted = window.ShowDialog();
            if (accepted == true && window.Result is not null)
            {
                string? confirmationMessage = string.IsNullOrWhiteSpace(window.RequestedDeviceName)
                    ? T("Dialog.DeviceDetailsWarning")
                    : null;
                bool applied = RunProjectAction(
                    T("Action.DeviceDetailsUpdated"),
                    () => _project!.ApplyBatch(_ => ApplyDeviceDetails(window.OriginalDeviceName, window.Result)),
                    confirmationMessage);
                if (!applied)
                {
                    return;
                }
            }

            if (string.IsNullOrWhiteSpace(window.RequestedDeviceName))
            {
                return;
            }

            currentDeviceName = window.RequestedDeviceName;
        }
    }

    private void ApplyDeviceDetails(string originalDeviceName, DeviceDetailsResult result)
    {
        // Les patchs utilisent les noms actuels du XML. Ils sont donc appliqués
        // avant les renommages ; les méthodes de renommage propagent ensuite les
        // nouveaux noms dans toutes les subscriptions concernées.
        foreach (PatchEditRequest edit in result.PatchEdits)
        {
            if (edit.IsRemoval)
            {
                _project!.RemovePatch(edit.RxDeviceName, edit.RxDanteId);
            }
            else
            {
                _project!.ApplyPatch(
                    edit.RxDeviceName,
                    edit.RxDanteId,
                    edit.TxDeviceName!,
                    edit.TxChannelName ?? string.Empty);
            }
        }

        DanteDevice originalDevice = _project!.FindDevice(originalDeviceName) ?? throw new InvalidOperationException("Device introuvable.");
        string currentName = originalDevice.Name;

        if (!string.Equals(currentName, result.DeviceName, StringComparison.Ordinal))
        {
            _project.RenameDevice(currentName, result.DeviceName);
            currentName = result.DeviceName;
        }

        if (originalDevice.IsRedundant != result.IsRedundant)
        {
            _project.SetNetworkMode(currentName, result.IsRedundant);
        }

        if (!string.Equals(originalDevice.Latency, result.Latency, StringComparison.OrdinalIgnoreCase))
        {
            _project.SetLatency(currentName, result.Latency);
        }

        if (!string.Equals(originalDevice.Samplerate, result.Samplerate, StringComparison.OrdinalIgnoreCase))
        {
            _project.SetSamplerate(currentName, result.Samplerate);
        }

        if (!string.Equals(originalDevice.Encoding, result.Encoding, StringComparison.OrdinalIgnoreCase))
        {
            _project.SetEncoding(currentName, result.Encoding);
        }

        if (originalDevice.PreferredMaster != result.PreferredMaster)
        {
            _project.SetPreferredMaster(currentName, result.PreferredMaster);
        }

        if (result.UsesStaticIp)
        {
            if (!originalDevice.UsesStaticIp
                || !string.Equals(originalDevice.StaticIpAddress, result.StaticIpAddress, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(originalDevice.StaticIpNetmask, result.StaticIpNetmask, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(originalDevice.StaticIpGateway, result.StaticIpGateway, StringComparison.OrdinalIgnoreCase))
            {
                _project.SetIpAddressStatic(currentName, result.StaticIpAddress, result.StaticIpNetmask, result.StaticIpGateway);
            }
        }
        else if (originalDevice.UsesStaticIp)
        {
            _project.SetIpAddressDynamic(currentName);
        }

        foreach (DeviceChannelEdit channel in result.TxChannels)
        {
            DanteDevice? currentDevice = _project.FindDevice(currentName);
            DanteChannel? currentChannel = currentDevice?.TxChannels.FirstOrDefault(candidate => candidate.Index == channel.Index);
            if (currentChannel is not null && !string.Equals(currentChannel.DisplayName, channel.Name, StringComparison.Ordinal))
            {
                _project.RenameChannel(currentName, DanteChannelKind.Tx, channel.Index, channel.Name);
            }
        }

        foreach (DeviceChannelEdit channel in result.RxChannels)
        {
            DanteDevice? currentDevice = _project.FindDevice(currentName);
            DanteChannel? currentChannel = currentDevice?.RxChannels.FirstOrDefault(candidate => candidate.Index == channel.Index);
            if (currentChannel is not null && !string.Equals(currentChannel.DisplayName, channel.Name, StringComparison.Ordinal))
            {
                _project.RenameChannel(currentName, DanteChannelKind.Rx, channel.Index, channel.Name);
            }
        }
    }

    private void ChannelSelector_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (_refreshingUi)
        {
            return;
        }

        RefreshChannelSelector();
    }

    private void ChannelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_refreshingUi)
        {
            return;
        }

        if (ChannelComboBox.SelectedItem is ChannelChoice channel)
        {
            NewChannelNameTextBox.Text = channel.Name;
        }
    }

    private void SourceDeviceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RefreshSourceChannels();
    }

    private void SourceChannelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Le canal choisi reste la source à appliquer au patch. Les noms se
        // modifient désormais directement dans la liste.
    }

    private void PatchGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_refreshingUi || PatchGrid.SelectedItem is not DanteSubscription subscription)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(subscription.ResolvedTxDeviceName))
        {
            SourceDeviceComboBox.SelectedItem = subscription.ResolvedTxDeviceName;
            RefreshSourceChannels();
            SelectSourceChannel(subscription.TxChannelName);
        }

        if (subscription.IsExternalMissingDevice)
        {
            SetStatus(T("Dialog.ExternalPatchStatus"));
        }
    }

    private void ApplyPatchButton_Click(object sender, RoutedEventArgs e)
    {
        if (PatchGrid.SelectedItem is not DanteSubscription subscription)
        {
            ShowError(T("Dialog.NoRxTitle"), T("Dialog.NoRxMessage"));
            return;
        }

        RunProjectAction(
            T("Action.PatchApplied"),
            () =>
            {
                string txDevice = SourceDeviceComboBox.SelectedItem as string ?? string.Empty;
                string txChannel = SelectedSourceChannelName();
                _project!.ApplyPatch(subscription.RxDevice, subscription.RxIndex, txDevice, txChannel);
            },
            subscription.IsExternalMissingDevice
                ? T("Dialog.ExternalPatchWarning")
                : null);
    }

    private void RemovePatchButton_Click(object sender, RoutedEventArgs e)
    {
        if (PatchGrid.SelectedItem is not DanteSubscription subscription)
        {
            ShowError(T("Dialog.NoRxTitle"), T("Dialog.NoRxMessage"));
            return;
        }

        RunProjectAction(
            T("Action.PatchRemoved"),
            () => _project!.RemovePatch(subscription.RxDevice, subscription.RxIndex),
            T("Dialog.RemovePatchWarning"));
    }

    private void OpenVisualPatchButton_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureProjectLoaded())
        {
            return;
        }

        if (!_project!.Devices.Any(device => device.TxCount > 0)
            || !_project.Devices.Any(device => device.RxCount > 0))
        {
            ShowError(
                T("Dialog.ActionImpossibleTitle"),
                _language == UiLanguage.English
                    ? "The loaded preset must contain at least one Tx channel and one Rx channel."
                    : "Le preset chargé doit contenir au moins un canal TX et un canal RX.");
            return;
        }

        EasyPatchTab.IsSelected = true;
        RefreshEasyPatchWorkspace();
    }

    private void MainTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MainTabs is not null
            && EasyPatchTab is not null
            && ReferenceEquals(e.OriginalSource, MainTabs)
            && EasyPatchTab.IsSelected)
        {
            RefreshEasyPatchWorkspace();
        }
    }

    private void RefreshEasyPatchWorkspace()
    {
        if (EasyPatchHost is null || EasyPatchTab is null)
        {
            return;
        }

        if (_project is null)
        {
            _easyPatchProject = null;
            _easyPatchWorkspace = null;
            ShowEasyPatchPlaceholder(
                _language == UiLanguage.English
                    ? "Load an XML file to use Easy patch."
                    : "Chargez un XML pour utiliser Easy patch.");
            return;
        }

        if (!EasyPatchTab.IsSelected && _easyPatchWorkspace is null)
        {
            return;
        }

        if (!_project.Devices.Any(device => device.TxCount > 0)
            || !_project.Devices.Any(device => device.RxCount > 0))
        {
            _easyPatchProject = _project;
            _easyPatchWorkspace = null;
            ShowEasyPatchPlaceholder(
                _language == UiLanguage.English
                    ? "Easy patch requires at least one Tx channel and one Rx channel."
                    : "Easy patch nécessite au moins un canal TX et un canal RX.");
            return;
        }

        bool sameProject = ReferenceEquals(_easyPatchProject, _project);
        bool startInAssignmentMode = sameProject
            && _easyPatchWorkspace?.IsAssignmentModeSelected == true;
        bool warnOnExistingPatch = !sameProject
            || _easyPatchWorkspace?.WarnOnExistingPatch != false;
        PatchMatrixOneToOneState? matrixOneToOneState = sameProject
            ? _easyPatchWorkspace?.CaptureMatrixOneToOneState()
            : null;
        string? initialTxDevice = sameProject
            ? _easyPatchWorkspace?.SelectedTxDeviceName
            : SourceDeviceComboBox.SelectedItem as string;
        string? initialRxDevice = sameProject
            ? _easyPatchWorkspace?.SelectedRxDeviceName
            : (PatchGrid.SelectedItem as DanteSubscription)?.RxDevice;

        if (string.IsNullOrWhiteSpace(initialRxDevice)
            && ReceiverDeviceList.SelectedItem is string receiverName
            && _project.FindDevice(receiverName)?.RxCount > 0)
        {
            initialRxDevice = receiverName;
        }

        try
        {
            PatchWorkspaceView workspace = new(
                _language,
                _project,
                ThemeToggleButton.IsChecked == true,
                initialTxDevice,
                initialRxDevice,
                initialEdits: null,
                embedded: true,
                renameChannelAction: RenameEasyPatchChannel,
                extendChannelSeriesAction: ExtendEasyPatchChannelSeries,
                startInAssignmentMode: startInAssignmentMode,
                warnOnExistingPatch: warnOnExistingPatch);
            workspace.RestoreMatrixOneToOneState(matrixOneToOneState);
            workspace.DirectApplyRequested += EasyPatchWorkspace_DirectApplyRequested;
            workspace.InlineChannelNavigationRequested += EasyPatchWorkspace_InlineChannelNavigationRequested;
            _easyPatchProject = _project;
            _easyPatchWorkspace = workspace;
            EasyPatchHost.Content = workspace;
        }
        catch (Exception exception)
        {
            _easyPatchWorkspace = null;
            ShowEasyPatchPlaceholder(exception.Message);
        }
    }

    private void EasyPatchWorkspace_InlineChannelNavigationRequested(
        object? sender,
        InlineChannelNavigationRequestEventArgs e)
    {
        Dispatcher.BeginInvoke(new Action(() =>
            _easyPatchWorkspace?.FocusChannelEditor(e.Kind, e.DanteId, e.Matrix)));
    }

    private void EasyPatchWorkspace_DirectApplyRequested(
        object? sender,
        DirectPatchRequestEventArgs e)
    {
        if (sender is not PatchWorkspaceView workspace || e.Edits.Count == 0)
        {
            return;
        }

        RunProjectAction(
            Tf("Action.VisualPatchesApplied", e.Edits.Count),
            () => ApplyPatchEdits(e.Edits));
    }

    private void ApplyPatchEdits(IEnumerable<PatchEditRequest> edits)
    {
        _project!.ApplyBatch(batch =>
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
    }

    private void ShowEasyPatchPlaceholder(string message)
    {
        EasyPatchHost.Content = new TextBlock
        {
            Text = message,
            Foreground = Resources["MutedTextBrush"] as Brush,
            Margin = new Thickness(20),
            TextWrapping = TextWrapping.Wrap
        };
    }

    private void PatchGrid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
    {
        if (FindVisualChild<TextBox>(e.EditingElement) is TextBox editor)
        {
            editor.Focus();
            editor.SelectAll();
        }
    }

    private void PatchSeriesThumb_DragStarted(object sender, DragStartedEventArgs e)
    {
        _patchSeriesDeviceName = null;
        _patchSeriesKind = null;
        _patchSeriesSeeds = [];
        if (sender is not Thumb { Tag: DanteSubscription subscription } thumb)
        {
            return;
        }

        if (!PatchGrid.SelectedItems.Contains(subscription))
        {
            PatchGrid.SelectedItems.Clear();
            PatchGrid.SelectedItem = subscription;
        }

        DanteChannelKind kind = string.Equals(thumb.Uid, "Tx", StringComparison.Ordinal)
            ? DanteChannelKind.Tx
            : DanteChannelKind.Rx;
        string? deviceName = kind == DanteChannelKind.Tx
            ? ResolvePatchTxDeviceName(subscription)
            : subscription.RxDevice;
        if (string.IsNullOrWhiteSpace(deviceName))
        {
            return;
        }

        int[] seeds = PatchGrid.SelectedItems
            .OfType<DanteSubscription>()
            .Where(item => string.Equals(
                kind == DanteChannelKind.Tx ? ResolvePatchTxDeviceName(item) : item.RxDevice,
                deviceName,
                StringComparison.OrdinalIgnoreCase))
            .Select(item => kind == DanteChannelKind.Tx ? item.TxDanteId : item.RxDanteId)
            .Where(index => index.HasValue)
            .Select(index => index!.Value)
            .Distinct()
            .OrderBy(index => index)
            .ToArray();

        _patchSeriesDeviceName = deviceName;
        _patchSeriesKind = kind;
        _patchSeriesSeeds = seeds;
    }

    private void PatchSeriesThumb_DragCompleted(object sender, DragCompletedEventArgs e)
    {
        if (_project is null || _patchSeriesKind is null
            || string.IsNullOrWhiteSpace(_patchSeriesDeviceName)
            || _patchSeriesSeeds.Length == 0)
        {
            return;
        }

        DependencyObject? hit = PatchGrid.InputHitTest(Mouse.GetPosition(PatchGrid)) as DependencyObject;
        DataGridRow? targetRow = FindVisualParent<DataGridRow>(hit);
        if (targetRow?.Item is not DanteSubscription targetSubscription)
        {
            ShowError(T("Dialog.ActionImpossibleTitle"),
                _language == UiLanguage.English
                    ? "Drop the series handle on a channel row."
                    : "Déposez la poignée de série sur une ligne de canal.");
            return;
        }

        string? targetDeviceName = _patchSeriesKind == DanteChannelKind.Tx
            ? ResolvePatchTxDeviceName(targetSubscription)
            : targetSubscription.RxDevice;
        int? targetChannelIndex = _patchSeriesKind == DanteChannelKind.Tx
            ? targetSubscription.TxDanteId
            : targetSubscription.RxDanteId;
        if (!string.Equals(targetDeviceName, _patchSeriesDeviceName, StringComparison.OrdinalIgnoreCase)
            || targetChannelIndex is null)
        {
            ShowError(T("Dialog.ActionImpossibleTitle"),
                _language == UiLanguage.English
                    ? "The series must stay on the same device."
                    : "La série doit rester sur la même machine.");
            return;
        }

        DanteChannelKind kind = _patchSeriesKind.Value;
        string deviceName = _patchSeriesDeviceName;
        int[] seeds = _patchSeriesSeeds;
        RunProjectAction(
            _language == UiLanguage.English ? "Channel name series extended." : "Série de noms de canaux étendue.",
            () => _project.ExtendChannelNameSeries(deviceName, kind, seeds, targetChannelIndex.Value));
    }

    private static string? ResolvePatchTxDeviceName(DanteSubscription subscription)
    {
        return subscription.IsLocalSubscription
            ? subscription.RxDevice
            : string.IsNullOrWhiteSpace(subscription.ResolvedTxDeviceName)
                ? null
                : subscription.ResolvedTxDeviceName;
    }

    private bool RenameEasyPatchChannel(string deviceName, DanteChannelKind kind, int channelIndex, string newName)
    {
        string action = kind == DanteChannelKind.Tx
            ? T("Action.TxChannelRenamed")
            : T("Action.RxChannelRenamed");
        return RunProjectAction(action, () => _project!.RenameChannel(deviceName, kind, channelIndex, newName));
    }

    private bool ExtendEasyPatchChannelSeries(
        string deviceName,
        DanteChannelKind kind,
        IReadOnlyList<int> seedChannelIndexes,
        int targetChannelIndex)
    {
        string action = _language == UiLanguage.English
            ? "Channel name series extended."
            : "Série de noms de canaux étendue.";
        return RunProjectAction(action, () =>
            _project!.ExtendChannelNameSeries(deviceName, kind, seedChannelIndexes, targetChannelIndex));
    }

    private void PatchGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        if (_committingPatchInlineRename || e.EditAction != DataGridEditAction.Commit
            || e.Row.Item is not DanteSubscription subscription
            || FindVisualChild<TextBox>(e.EditingElement) is not TextBox editor)
        {
            return;
        }

        string newName = editor.Text.Trim();
        DataGridColumn column = e.Column;
        e.Cancel = true;
        _committingPatchInlineRename = true;
        Dispatcher.BeginInvoke(() =>
        {
            try
            {
                RenamePatchCell(subscription, column, newName);
            }
            finally
            {
                _committingPatchInlineRename = false;
                PatchGrid.CancelEdit(DataGridEditingUnit.Cell);
            }
        });
    }

    private void RenamePatchCell(DanteSubscription subscription, DataGridColumn column, string newName)
    {
        if (column == PatchRxDeviceColumn)
        {
            if (!string.Equals(subscription.RxDevice, newName, StringComparison.Ordinal))
            {
                RunProjectAction(T("Action.DeviceRenamed"), () => _project!.RenameDevice(subscription.RxDevice, newName));
            }
            return;
        }

        if (column == PatchRxChannelColumn)
        {
            if (!string.Equals(subscription.RxChannelName, newName, StringComparison.Ordinal))
            {
                RunProjectAction(T("Action.RxChannelRenamed"), () =>
                    _project!.RenameChannel(subscription.RxDevice, DanteChannelKind.Rx, subscription.RxIndex, newName));
            }
            return;
        }

        string txDeviceName = subscription.IsLocalSubscription
            ? subscription.RxDevice
            : subscription.ResolvedTxDeviceName;
        if (string.IsNullOrWhiteSpace(txDeviceName) || _project!.FindDevice(txDeviceName) is not DanteDevice txDevice)
        {
            ShowError(
                T("Dialog.MissingTxTitle"),
                _language == UiLanguage.English
                    ? "This external Tx device is not present in the loaded XML and cannot be renamed here."
                    : "Cette machine TX externe n'est pas présente dans le XML chargé et ne peut pas être renommée ici.");
            return;
        }

        if (column == PatchDisplayTxColumn)
        {
            if (!string.Equals(txDeviceName, newName, StringComparison.Ordinal))
            {
                RunProjectAction(T("Action.DeviceRenamed"), () => _project.RenameDevice(txDeviceName, newName));
            }
            return;
        }

        if (column != PatchTxChannelColumn || string.Equals(subscription.TxChannelName, newName, StringComparison.Ordinal))
        {
            return;
        }

        DanteChannel[] matchingChannels = txDevice.TxChannels
            .Where(channel => string.Equals(channel.DisplayName, subscription.TxChannelName, StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (matchingChannels.Length != 1)
        {
            ShowError(
                T("Dialog.MissingTxTitle"),
                _language == UiLanguage.English
                    ? "The Tx channel cannot be identified unambiguously. Open the device details to rename it."
                    : "Le canal TX ne peut pas être identifié sans ambiguïté. Ouvrez le détail de la machine pour le renommer.");
            return;
        }

        RunProjectAction(T("Action.TxChannelRenamed"), () =>
            _project.RenameChannel(txDeviceName, DanteChannelKind.Tx, matchingChannels[0].Index, newName));
    }

    private void ValidateButton_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureProjectLoaded())
        {
            return;
        }

        DanteValidationResult validation = _project!.Validate();
        SaveSummaryTextBox.Text = _project.BuildCompatibilityReport();
        MessageBox.Show(this, validation.ToDisplayText(), LocalizeLiteral("Vérification"), MessageBoxButton.OK, validation.HasErrors ? MessageBoxImage.Error : MessageBoxImage.Information);
    }

    private void ImportChannelLabelsButton_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureProjectLoaded() || !EnsureEditMode())
        {
            return;
        }

        OpenFileDialog dialog = new()
        {
            Filter = _language == UiLanguage.English
                ? "Channel labels (*.json;*.csv;*.xlsx;*.ods;*.zip)|*.json;*.csv;*.xlsx;*.ods;*.zip|JSON (*.json)|*.json|CSV (*.csv)|*.csv|DMT workbook (*.xlsx;*.ods)|*.xlsx;*.ods|Yamaha CL/QL package (*.zip)|*.zip|All files (*.*)|*.*"
                : "Labels de canaux (*.json;*.csv;*.xlsx;*.ods;*.zip)|*.json;*.csv;*.xlsx;*.ods;*.zip|JSON (*.json)|*.json|CSV (*.csv)|*.csv|Classeur DMT (*.xlsx;*.ods)|*.xlsx;*.ods|Package Yamaha CL/QL (*.zip)|*.zip|Tous les fichiers (*.*)|*.*",
            Title = _language == UiLanguage.English ? "Import channel labels" : "Importer des labels de canaux",
            InitialDirectory = Path.GetDirectoryName(_project!.OriginalFilePath)
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        try
        {
            ChannelLabelReadResult readResult = ChannelLabelExchangeService.ReadWithReport(dialog.FileName);
            ChannelLabelImportWindow window = new(
                _language,
                _project!,
                readResult.Document,
                readResult.Report,
                DeviceComboBox.SelectedItem as string)
            {
                Owner = this
            };
            if (window.ShowDialog() != true)
            {
                return;
            }

            int requested = window.Assignments.Count;
            string success = _language == UiLanguage.English
                ? $"{requested} channel label(s) imported."
                : $"{requested} label(s) de canal importé(s).";
            RunProjectAction(success, () =>
            {
                int changed = _project!.ApplyChannelLabels(window.Assignments);
                if (changed == 0)
                {
                    throw new InvalidOperationException(_language == UiLanguage.English
                        ? "No channel label needed to be changed."
                        : "Aucun label de canal ne nécessitait de modification.");
                }
            });
        }
        catch (Exception ex)
        {
            ShowError(_language == UiLanguage.English ? "Label import unavailable" : "Import de labels impossible", ex);
        }
    }

    private void OpenDmtProjectButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo("https://github.com/togrupe/dlive-midi-tools")
            {
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            ShowError(
                LocalizeLiteral("Ouverture du projet DMT impossible"),
                ex);
        }
    }

    private void ExportChannelLabelsButton_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureProjectLoaded())
        {
            return;
        }

        ChannelLabelExportWindow window = new(_language, _project!, DeviceComboBox.SelectedItem as string)
        {
            Owner = this
        };
        if (window.ShowDialog() != true)
        {
            return;
        }

        try
        {
            ChannelLabelDocument document = ChannelLabelExchangeService.CreateFromProject(
                _project!,
                window.DeviceNames,
                window.Kind,
                window.StartChannel,
                window.Count);
            document = ChannelLabelTransformService.Transform(document, window.TransformOptions);

            if (BuiltInChannelLabelTemplateService.IsBuiltInFormat(window.Format))
            {
                ExportBuiltInTemplateCopies(document, window.Format, adaptLabels: false);
                return;
            }

            string extension = window.Format == "csv" ? ".csv" : ".json";
            SaveFileDialog saveDialog = new()
            {
                Filter = window.Format == "csv" ? "CSV (*.csv)|*.csv" : "JSON (*.json)|*.json",
                DefaultExt = extension,
                AddExtension = true,
                FileName = $"{Path.GetFileNameWithoutExtension(_project!.OriginalFilePath)}_labels{extension}",
                InitialDirectory = Path.GetDirectoryName(_project.OriginalFilePath),
                Title = _language == UiLanguage.English ? "Export channel labels" : "Exporter les labels de canaux"
            };
            if (saveDialog.ShowDialog(this) != true)
            {
                return;
            }

            ChannelLabelExchangeService.Write(saveDialog.FileName, document);
            AddLog((_language == UiLanguage.English ? "Channel labels exported: " : "Labels de canaux exportés : ") + saveDialog.FileName);
            SetStatus(_language == UiLanguage.English ? "Channel labels exported." : "Labels de canaux exportés.");
        }
        catch (Exception ex)
        {
            ShowError(_language == UiLanguage.English ? "Label export unavailable" : "Export de labels impossible", ex);
        }
    }

    private void ExportBuiltInTemplateCopies(ChannelLabelDocument document, string format, bool adaptLabels)
    {
        BuiltInChannelLabelTemplateInfo template = BuiltInChannelLabelTemplateService.Get(format);
        string extension = template.OutputExtension;
        string filter = extension switch
        {
            ".xlsx" => "DMT Excel workbook (*.xlsx)|*.xlsx",
            ".ods" => "DMT OpenDocument workbook (*.ods)|*.ods",
            ".csv" => "Console CSV (*.csv)|*.csv",
            ".zip" => "Yamaha package (*.zip)|*.zip",
            _ => "All files (*.*)|*.*"
        };
        SaveFileDialog saveDialog = new()
        {
            Filter = filter,
            DefaultExt = extension,
            AddExtension = true,
            FileName = document.Sets.Count == 1
                ? $"{SafeFileNameSegment(document.Sets[0].DeviceName)}_{template.FileSuffix}{extension}"
                : $"Dante_labels_{template.FileSuffix}{extension}",
            InitialDirectory = Path.GetDirectoryName(_project!.OriginalFilePath),
            Title = _language == UiLanguage.English
                ? $"Export {template.DisplayName} labels"
                : $"Exporter les labels {template.DisplayName}"
        };
        if (saveDialog.ShowDialog(this) != true)
        {
            return;
        }

        string directory = Path.GetDirectoryName(saveDialog.FileName) ?? Environment.CurrentDirectory;
        string baseName = Path.GetFileNameWithoutExtension(saveDialog.FileName);
        string[] outputs = document.Sets.Select(set => document.Sets.Count == 1
            ? saveDialog.FileName
            : Path.Combine(directory, $"{baseName}-{SafeFileNameSegment(set.DeviceName)}{extension}")).ToArray();
        int existing = outputs.Count(File.Exists);
        if (existing > 0)
        {
            MessageBoxResult overwrite = MessageBox.Show(
                this,
                _language == UiLanguage.English
                    ? $"{existing} generated file(s) already exist and will be replaced. Continue?"
                    : $"{existing} fichier(s) généré(s) existent déjà et seront remplacés. Continuer ?",
                _language == UiLanguage.English ? "Replace generated files" : "Remplacer les fichiers générés",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (overwrite != MessageBoxResult.Yes)
            {
                return;
            }
        }

        for (int index = 0; index < document.Sets.Count; index++)
        {
            BuiltInChannelLabelTemplateService.Write(format, outputs[index], document.Sets[index], adaptLabels);
        }
        AddLog((_language == UiLanguage.English ? "Native label files exported: " : "Fichiers de labels natifs exportés : ") + outputs.Length);
        SetStatus(_language == UiLanguage.English
            ? $"{outputs.Length} {template.DisplayName} file(s) exported."
            : $"{outputs.Length} fichier(s) {template.DisplayName} exporté(s).");
    }

    private static string SafeFileNameSegment(string value)
    {
        HashSet<char> invalid = new(Path.GetInvalidFileNameChars());
        string safe = new string(value.Trim().Select(character => invalid.Contains(character) ? '_' : character).ToArray()).Trim();
        return string.IsNullOrWhiteSpace(safe) ? "device" : safe;
    }

    private void AtomicChaosButton_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureProjectLoaded())
        {
            return;
        }

        AtomicChaosOptions options = SelectedAtomicChaosOptions();
        if (!options.HasSelection)
        {
            MessageBox.Show(this, T("Dialog.AtomicChaosNothingSelected"), T("Dialog.AtomicChaosTitle"), MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (!ConfirmAtomicChaosStage("Dialog.AtomicChaosFirst")
            || !ConfirmAtomicChaosStage("Dialog.AtomicChaosSecond")
            || !ConfirmAtomicChaosStage("Dialog.AtomicChaosThird"))
        {
            return;
        }

        AtomicChaosResult? result = null;
        bool applied = RunProjectAction(
            T("Action.AtomicChaosApplied"),
            () => result = _project!.ApplyAtomicChaos(options));
        if (!applied || result is null)
        {
            return;
        }

        string summary = Tf(
            "Dialog.AtomicChaosCompleted",
            result.Seed,
            result.DeviceCount,
            result.TxChannelCount,
            result.PatchedRxCount,
            result.DisconnectedRxCount,
            result.StaticIpCount,
            result.DynamicIpCount);
        AddLog(summary);
        SaveSummaryTextBox.Text = summary + Environment.NewLine + Environment.NewLine + _project!.BuildCompatibilityReport();
        MessageBox.Show(this, summary, T("Dialog.AtomicChaosTitle"), MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private AtomicChaosOptions SelectedAtomicChaosOptions() => new(
        AtomicDeviceNamesCheckBox.IsChecked == true,
        AtomicTxLabelsCheckBox.IsChecked == true,
        AtomicRxLabelsCheckBox.IsChecked == true,
        AtomicPatchesCheckBox.IsChecked == true,
        AtomicNetworkModeCheckBox.IsChecked == true,
        AtomicPreferredMasterCheckBox.IsChecked == true,
        AtomicLatencyCheckBox.IsChecked == true,
        AtomicSampleRateCheckBox.IsChecked == true,
        AtomicEncodingCheckBox.IsChecked == true,
        AtomicIpCheckBox.IsChecked == true);

    private bool ConfirmAtomicChaosStage(string key)
    {
        return MessageBox.Show(
            this,
            T(key),
            T("Dialog.AtomicChaosTitle"),
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning) == MessageBoxResult.Yes;
    }

    private void CompatibilityReportButton_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureProjectLoaded())
        {
            return;
        }

        SaveSummaryTextBox.Text = _project!.BuildCompatibilityReport();
        MainTabs.SelectedItem = SafetyTab;
        SetStatus(LocalizeLiteral("Rapport compatibilité Dante Controller affiché."));
    }

    private void FinalDanteCheckButton_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureProjectLoaded())
        {
            return;
        }

        DanteValidationResult validation = _project!.Validate();
        string[] importantWarnings = _project.BuildImportantWarnings().ToArray();
        StringBuilder builder = new();
        builder.AppendLine("RAPPORT FINAL AVANT IMPORT DANTE");
        builder.AppendLine("================================");
        builder.AppendLine();
        builder.AppendLine(validation.HasErrors ? "STATUT : ERREURS A CORRIGER" : importantWarnings.Length > 0 || validation.Warnings.Count > 0 ? "STATUT : POINTS A VERIFIER" : "STATUT : OK");
        builder.AppendLine($"Fichier : {_project.OriginalFilePath}");
        builder.AppendLine($"Machines : {_project.Devices.Count}");
        builder.AppendLine($"Patchs actifs : {_project.PatchMatrix.ActivePatchCount}");
        builder.AppendLine();

        if (importantWarnings.Length > 0)
        {
            builder.AppendLine("Points importants :");
            foreach (string warning in importantWarnings)
            {
                builder.AppendLine("- " + warning);
            }

            builder.AppendLine();
        }

        builder.AppendLine(validation.ToDisplayText());
        builder.AppendLine();
        builder.AppendLine(_project.BuildCompatibilityReport());
        SaveSummaryTextBox.Text = builder.ToString();
        MainTabs.SelectedItem = SafetyTab;
        SetStatus(LocalizeLiteral("Rapport final avant Dante affiché."));
    }

    private void RefreshSummaryButton_Click(object sender, RoutedEventArgs e)
    {
        SaveSummaryTextBox.Text = _project?.BuildSaveSummary() ?? T("Status.NoFileLoaded");
    }

    private void ActionHistoryButton_Click(object sender, RoutedEventArgs e)
    {
        StringBuilder builder = new();
        builder.AppendLine("HISTORIQUE DES ACTIONS");
        builder.AppendLine("======================");
        builder.AppendLine();

        if (_logs.Count == 0)
        {
            builder.AppendLine("Aucune action enregistrée dans cette session.");
        }
        else
        {
            foreach (string log in _logs)
            {
                builder.AppendLine("- " + log);
            }
        }

        SaveSummaryTextBox.Text = builder.ToString();
        MainTabs.SelectedItem = SafetyTab;
        SetStatus(LocalizeLiteral("Historique des actions affiché."));
    }

    private void OpenQuickStartButton_Click(object sender, RoutedEventArgs e)
    {
        OpenBundledDocument($"QuickStart_DanteConfigEditorV3_{DocumentLanguageSuffix()}.pdf");
    }

    private void OpenFullNoticeButton_Click(object sender, RoutedEventArgs e)
    {
        OpenBundledDocument($"Notice_DanteConfigEditorV3_{DocumentLanguageSuffix()}.pdf");
    }

    private void SupportDceButton_Click(object sender, RoutedEventArgs e)
    {
        ShowSupportDceWindow();
    }

    private void SupportReminderOpenButton_Click(object sender, RoutedEventArgs e)
    {
        PostponeSupportReminder();
        ShowSupportDceWindow();
    }

    private void SupportReminderLaterButton_Click(object sender, RoutedEventArgs e)
    {
        PostponeSupportReminder();
    }

    private void SupportReminderNeverButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _supportReminderSettings.Suppress();
        }
        catch (Exception exception)
        {
            DiagnosticLogService.Default.Write("Support", "Impossible de mémoriser la désactivation du rappel.", exception);
        }

        SupportReminderBorder.Visibility = Visibility.Collapsed;
    }

    private void InitializeSupportReminder()
    {
        if (SupportReminderSettingsService.IsAutomatedTestProcess())
        {
            return;
        }

        try
        {
            string version = GetType().Assembly.GetName().Version?.ToString(2) ?? "3.6";
            SupportReminderDecision decision = _supportReminderSettings.RegisterSuccessfulLaunch(version);
            SupportReminderBorder.Visibility = decision.ShouldShow
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
        catch (Exception exception)
        {
            // Le rappel est accessoire : une préférence locale inaccessible
            // ne doit jamais perturber l'ouverture de l'éditeur.
            DiagnosticLogService.Default.Write("Support", "Rappel de soutien indisponible.", exception);
            SupportReminderBorder.Visibility = Visibility.Collapsed;
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
            DiagnosticLogService.Default.Write("Support", "Impossible de reporter le rappel.", exception);
        }

        SupportReminderBorder.Visibility = Visibility.Collapsed;
    }

    private void ShowSupportDceWindow()
    {
        SupportDceWindow dialog = new(_language, ThemeToggleButton.IsChecked == true)
        {
            Owner = this
        };
        dialog.ShowDialog();
    }

    private void OpenDiagnosticLogsButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string directory = DiagnosticLogService.Default.DirectoryPath;
            Directory.CreateDirectory(directory);
            Process.Start(new ProcessStartInfo(directory)
            {
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            ShowError(
                _language == UiLanguage.English
                    ? "Unable to open diagnostic logs"
                    : "Ouverture des journaux impossible",
                ex);
        }
    }

    private string DocumentLanguageSuffix()
    {
        return _language == UiLanguage.English ? "EN" : "FR";
    }

    private void ExportTxtButton_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureProjectLoaded())
        {
            return;
        }

        SaveFileDialog dialog = new()
        {
            Filter = T("Dialog.TxtFilter"),
            Title = T("Dialog.ExportTxtTitle"),
            FileName = BuildDefaultReportFileName(".txt"),
            InitialDirectory = Path.GetDirectoryName(_project!.OriginalFilePath)
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        try
        {
            ReportExportService.ExportText(dialog.FileName, _project!.BuildReportText());
            AddLog(Tf("Log.TxtExported", dialog.FileName));
            SetStatus(T("Status.TxtExported"));
        }
        catch (Exception ex)
        {
            ShowError(T("Dialog.ExportImpossibleTitle"), ex);
        }
    }

    private void ExportPdfButton_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureProjectLoaded())
        {
            return;
        }

        SaveFileDialog dialog = new()
        {
            Filter = T("Dialog.PdfFilter"),
            Title = T("Dialog.ExportPdfTitle"),
            FileName = BuildDefaultReportFileName(".pdf"),
            InitialDirectory = Path.GetDirectoryName(_project!.OriginalFilePath)
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        try
        {
            ReportExportService.ExportPdf(dialog.FileName, "Dante Config Editor", _project!.BuildReportText());
            AddLog(Tf("Log.PdfExported", dialog.FileName));
            SetStatus(T("Status.PdfExported"));
        }
        catch (Exception ex)
        {
            ShowError(T("Dialog.ExportImpossibleTitle"), ex);
        }
    }

    private void ExportPatchbookTxtButton_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureProjectLoaded())
        {
            return;
        }

        SaveFileDialog dialog = new()
        {
            Filter = T("Dialog.PatchbookTxtFilter"),
            Title = T("Dialog.ExportPatchbookTxtTitle"),
            FileName = BuildDefaultReportFileName("_patchbook.txt"),
            InitialDirectory = Path.GetDirectoryName(_project!.OriginalFilePath)
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        try
        {
            string scope = SelectedOptionKey(PatchbookScopeComboBox, _patchbookScopeKeys[0]);
            string scopeDisplay = SelectedOptionDisplay(PatchbookScopeComboBox, T(_patchbookScopeKeys[0]));
            ReportExportService.ExportText(dialog.FileName, _project!.BuildPatchbookText(scope, scopeDisplay));
            AddLog(Tf("Log.PatchbookTxtExported", dialog.FileName));
            SetStatus(T("Status.PatchbookTxtExported"));
        }
        catch (Exception ex)
        {
            ShowError(T("Dialog.ExportPatchbookImpossibleTitle"), ex);
        }
    }

    private void ExportPatchbookCsvButton_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureProjectLoaded())
        {
            return;
        }

        SaveFileDialog dialog = new()
        {
            Filter = T("Dialog.PatchbookCsvFilter"),
            Title = T("Dialog.ExportPatchbookCsvTitle"),
            FileName = BuildDefaultReportFileName("_patchbook.csv"),
            InitialDirectory = Path.GetDirectoryName(_project!.OriginalFilePath)
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        try
        {
            string scope = SelectedOptionKey(PatchbookScopeComboBox, _patchbookScopeKeys[0]);
            ReportExportService.ExportText(dialog.FileName, _project!.BuildPatchbookCsv(scope), includeSignature: false);
            AddLog(Tf("Log.PatchbookCsvExported", dialog.FileName));
            SetStatus(T("Status.PatchbookCsvExported"));
        }
        catch (Exception ex)
        {
            ShowError(T("Dialog.ExportPatchbookCsvImpossibleTitle"), ex);
        }
    }

    private void TopologyButton_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureProjectLoaded())
        {
            return;
        }

        SaveSummaryTextBox.Text = _project!.BuildTopologyText();
        MainTabs.SelectedItem = SafetyTab;
        SetStatus(T("Status.TopologyDisplayed"));
    }

    private void CompareXmlButton_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureProjectLoaded())
        {
            return;
        }

        OpenFileDialog dialog = new()
        {
            Filter = T("Dialog.XmlFilter"),
            Title = _language == UiLanguage.English
                ? "Compare with another XML file"
                : "Comparer avec un autre XML"
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        try
        {
            DanteProject otherProject = DanteProject.Load(dialog.FileName);
            ComparisonDisplayRow[] comparisonRows = BuildComparisonRows(otherProject);
            SaveSummaryTextBox.Text = _project!.CompareWith(otherProject, _language);
            MainTabs.SelectedItem = SafetyTab;
            ComparisonResultWindow window = new(
                _language,
                ThemeToggleButton.IsChecked == true,
                comparisonRows)
            {
                Owner = this
            };
            window.Show();
            AddLog((_language == UiLanguage.English
                ? "XML comparison completed: "
                : "Comparaison XML effectuée : ") + dialog.FileName);
            SetStatus(_language == UiLanguage.English
                ? "XML comparison displayed."
                : "Comparaison XML affichée.");
        }
        catch (Exception ex)
        {
            ShowError(
                _language == UiLanguage.English ? "Comparison failed" : "Comparaison impossible",
                ex);
        }
    }

    private ComparisonDisplayRow[] BuildComparisonRows(DanteProject otherProject)
    {
        if (_project is null)
        {
            return [];
        }

        List<ComparisonDisplayRow> rows = [];
        Dictionary<string, DanteDevice> currentDevices = _project.Devices
            .Where(device => !string.IsNullOrWhiteSpace(device.Name))
            .GroupBy(device => device.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
        Dictionary<string, DanteDevice> otherDevices = otherProject.Devices
            .Where(device => !string.IsNullOrWhiteSpace(device.Name))
            .GroupBy(device => device.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        foreach (string deviceName in currentDevices.Keys.Except(otherDevices.Keys, StringComparer.OrdinalIgnoreCase).OrderBy(name => name, StringComparer.OrdinalIgnoreCase))
        {
            rows.Add(new ComparisonDisplayRow(
                (_language == UiLanguage.English ? "Device / " : "Machine / ") + deviceName,
                _language == UiLanguage.English ? "present" : "présente",
                _language == UiLanguage.English ? "absent" : "absente",
                _language == UiLanguage.English ? "Open file only" : "Seulement fichier ouvert"));
        }

        foreach (string deviceName in otherDevices.Keys.Except(currentDevices.Keys, StringComparer.OrdinalIgnoreCase).OrderBy(name => name, StringComparer.OrdinalIgnoreCase))
        {
            rows.Add(new ComparisonDisplayRow(
                (_language == UiLanguage.English ? "Device / " : "Machine / ") + deviceName,
                _language == UiLanguage.English ? "absent" : "absente",
                _language == UiLanguage.English ? "present" : "présente",
                _language == UiLanguage.English ? "Compared file only" : "Seulement fichier comparé"));
        }

        foreach (string deviceName in currentDevices.Keys.Intersect(otherDevices.Keys, StringComparer.OrdinalIgnoreCase).OrderBy(name => name, StringComparer.OrdinalIgnoreCase))
        {
            DanteDevice current = currentDevices[deviceName];
            DanteDevice compared = otherDevices[deviceName];
            AddComparisonRow(rows, $"{deviceName} / Friendly name", current.FriendlyName, compared.FriendlyName);
            AddComparisonRow(rows, $"{deviceName} / {(_language == UiLanguage.English ? "Network mode" : "Mode réseau")}", current.NetworkMode, compared.NetworkMode);
            AddComparisonRow(rows, $"{deviceName} / {(_language == UiLanguage.English ? "Latency" : "Latence")}", current.LatencyDisplay, compared.LatencyDisplay);
            AddComparisonRow(rows, $"{deviceName} / Sample rate", current.SampleRateDisplay, compared.SampleRateDisplay);
            AddComparisonRow(rows, $"{deviceName} / Bits", current.EncodingDisplay, compared.EncodingDisplay);
            AddComparisonRow(rows, $"{deviceName} / IP", current.IpModeDisplay, compared.IpModeDisplay);
            AddComparisonRow(
                rows,
                $"{deviceName} / Preferred Master",
                current.PreferredMaster
                    ? (_language == UiLanguage.English ? "yes" : "oui")
                    : (_language == UiLanguage.English ? "no" : "non"),
                compared.PreferredMaster
                    ? (_language == UiLanguage.English ? "yes" : "oui")
                    : (_language == UiLanguage.English ? "no" : "non"));
            AddChannelComparisonRows(rows, deviceName, "TX", current.TxChannels, compared.TxChannels);
            AddChannelComparisonRows(rows, deviceName, "RX", current.RxChannels, compared.RxChannels);
        }

        Dictionary<string, DanteSubscription> currentPatches = _project.PatchMatrix.Subscriptions
            .GroupBy(SubscriptionComparisonKey, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
        Dictionary<string, DanteSubscription> otherPatches = otherProject.PatchMatrix.Subscriptions
            .GroupBy(SubscriptionComparisonKey, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        foreach (string patchKey in currentPatches.Keys.Except(otherPatches.Keys, StringComparer.OrdinalIgnoreCase).OrderBy(key => key, StringComparer.OrdinalIgnoreCase))
        {
            rows.Add(new ComparisonDisplayRow(
                "Patch / " + patchKey,
                FormatSubscriptionForComparison(currentPatches[patchKey]),
                "absent",
                _language == UiLanguage.English ? "Open file only" : "Seulement fichier ouvert"));
        }

        foreach (string patchKey in otherPatches.Keys.Except(currentPatches.Keys, StringComparer.OrdinalIgnoreCase).OrderBy(key => key, StringComparer.OrdinalIgnoreCase))
        {
            rows.Add(new ComparisonDisplayRow(
                "Patch / " + patchKey,
                "absent",
                FormatSubscriptionForComparison(otherPatches[patchKey]),
                _language == UiLanguage.English ? "Compared file only" : "Seulement fichier comparé"));
        }

        foreach (string patchKey in currentPatches.Keys.Intersect(otherPatches.Keys, StringComparer.OrdinalIgnoreCase).OrderBy(key => key, StringComparer.OrdinalIgnoreCase))
        {
            AddComparisonRow(rows, "Patch / " + patchKey, FormatSubscriptionForComparison(currentPatches[patchKey]), FormatSubscriptionForComparison(otherPatches[patchKey]));
        }

        if (rows.Count == 0)
        {
            rows.Add(new ComparisonDisplayRow(
                _language == UiLanguage.English ? "Known fields" : "Champs connus",
                _language == UiLanguage.English ? "identical" : "identiques",
                _language == UiLanguage.English ? "identical" : "identiques",
                _language == UiLanguage.English ? "No difference detected" : "Aucune différence détectée"));
        }

        return rows.Take(1000).ToArray();
    }

    private void AddChannelComparisonRows(
        List<ComparisonDisplayRow> rows,
        string deviceName,
        string channelKind,
        IReadOnlyList<DanteChannel> currentChannels,
        IReadOnlyList<DanteChannel> comparedChannels)
    {
        Dictionary<int, DanteChannel> currentByIndex = currentChannels
            .GroupBy(channel => channel.Index)
            .ToDictionary(group => group.Key, group => group.First());
        Dictionary<int, DanteChannel> comparedByIndex = comparedChannels
            .GroupBy(channel => channel.Index)
            .ToDictionary(group => group.Key, group => group.First());

        foreach (int index in currentByIndex.Keys.Except(comparedByIndex.Keys).Order())
        {
            rows.Add(new ComparisonDisplayRow(
                $"{deviceName} / {channelKind} {index}",
                currentByIndex[index].DisplayName,
                "absent",
                _language == UiLanguage.English ? "Open file only" : "Seulement fichier ouvert"));
        }

        foreach (int index in comparedByIndex.Keys.Except(currentByIndex.Keys).Order())
        {
            rows.Add(new ComparisonDisplayRow(
                $"{deviceName} / {channelKind} {index}",
                "absent",
                comparedByIndex[index].DisplayName,
                _language == UiLanguage.English ? "Compared file only" : "Seulement fichier comparé"));
        }

        foreach (int index in currentByIndex.Keys.Intersect(comparedByIndex.Keys).Order())
        {
            AddComparisonRow(rows, $"{deviceName} / {channelKind} {index}", currentByIndex[index].DisplayName, comparedByIndex[index].DisplayName);
        }
    }

    private void AddComparisonRow(List<ComparisonDisplayRow> rows, string item, string currentValue, string comparedValue)
    {
        if (!string.Equals(currentValue, comparedValue, StringComparison.OrdinalIgnoreCase))
        {
            rows.Add(new ComparisonDisplayRow(
                item,
                Blank(currentValue),
                Blank(comparedValue),
                _language == UiLanguage.English ? "Different" : "Différent"));
        }
    }

    private static string SubscriptionComparisonKey(DanteSubscription subscription)
    {
        return $"{subscription.RxDevice} / RX {subscription.RxDanteId}";
    }

    private string FormatSubscriptionForComparison(DanteSubscription subscription)
    {
        return subscription.IsActive
            ? subscription.SourceFull
            : (_language == UiLanguage.English ? "Free" : "Libre");
    }

    private void ThemeToggleButton_Checked(object sender, RoutedEventArgs e)
    {
        SetTheme(useLightTheme: true);
        ThemeToggleButton.Content = LocalizeLiteral("Thème sombre");
        RefreshEasyPatchWorkspace();
    }

    private void ThemeToggleButton_Unchecked(object sender, RoutedEventArgs e)
    {
        SetTheme(useLightTheme: false);
        ThemeToggleButton.Content = LocalizeLiteral("Thème clair");
        RefreshEasyPatchWorkspace();
    }

    private void RefreshAll()
    {
        // Point central de rafraîchissement : après une modification XML, on
        // reconstruit les listes et on conserve autant que possible la sélection.
        _refreshingUi = true;
        try
        {
            if (_project is null)
            {
                FilePathTextBlock.Text = T("Status.NoFileOpen");
                ProjectSummaryTextBlock.Text = T("Status.LoadXmlToStart");
                ImportantWarningsBorder.Visibility = Visibility.Collapsed;
                ImportantWarningsTextBlock.Text = string.Empty;
                _warningDeviceNames.Clear();
                _selectedWarningKey = null;
                DirtyStateTextBlock.Text = T("Status.Unmodified");
                ModeTextBlock.Text = T("Status.ReadOnlyMode");
                CountsTextBlock.Text = "0 device - 0 TX - 0 RX";
                _deviceRows.Clear();
                DeviceComboBox.ItemsSource = null;
                SenderDeviceList.ItemsSource = new[] { AllSendersItem };
                SenderDeviceList.SelectedItem = AllSendersItem;
                ReceiverDeviceList.ItemsSource = new[] { AllReceiversItem };
                ReceiverDeviceList.SelectedItem = AllReceiversItem;
                SourceDeviceComboBox.ItemsSource = null;
                ExclusivePreferredMasterComboBox.ItemsSource = null;
                SaveSummaryTextBox.Text = T("Status.NoFileLoaded");
                HealthSummaryTextBlock.Text = T("Status.NoFileLoaded");
                _searchResults.Clear();
                UpdateGlobalSearchHint(T("Search.NoFileLoaded"));
                _patchRows.Clear();
                _healthIssues.Clear();
                RefreshSynopticWorkspace();
                RefreshEasyPatchWorkspace();
                UpdateCommandState();
                return;
            }

            IReadOnlyList<DanteDevice> devices = _project.Devices;
            string[] deviceNames = devices.Select(device => device.Name).Where(name => !string.IsNullOrWhiteSpace(name)).ToArray();
            string selectedDevice = DeviceComboBox.SelectedItem as string ?? deviceNames.FirstOrDefault() ?? string.Empty;
            HashSet<string> selectedDeviceGridNames = DeviceGrid.SelectedItems
                .OfType<DeviceRow>()
                .Select(row => row.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            string selectedSenderFilter = SenderDeviceList.SelectedItem as string ?? string.Empty;
            string selectedReceiverFilter = ReceiverDeviceList.SelectedItem as string ?? string.Empty;
            string selectedSourceDevice = SourceDeviceComboBox.SelectedItem as string ?? deviceNames.FirstOrDefault() ?? string.Empty;

            FilePathTextBlock.Text = _project.OriginalFilePath;
            DirtyStateTextBlock.Text = _project.IsModified ? T("Status.ModifiedUnsaved") : T("Status.Unmodified");
            DirtyStateTextBlock.Foreground = _project.IsModified ? Resources["DangerBrush"] as Brush : Resources["MutedTextBrush"] as Brush;
            ModeTextBlock.Text = _editModeEnabled ? T("Status.EditMode") : T("Status.ReadOnlyMode");

            int txCount = devices.Sum(device => device.TxCount);
            int rxCount = devices.Sum(device => device.RxCount);
            ProjectSummaryTextBlock.Text = _language == UiLanguage.English
                ? $"{devices.Count} devices\n{txCount} TX channels\n{rxCount} RX channels\n{_project.PatchMatrix.ActivePatchCount} active subscriptions"
                : $"{devices.Count} devices\n{txCount} canaux TX\n{rxCount} canaux RX\n{_project.PatchMatrix.ActivePatchCount} patchs actifs";
            CountsTextBlock.Text = $"{devices.Count} devices - {txCount} TX - {rxCount} RX";

            IReadOnlyList<DanteImportantWarning> importantWarnings = _project.BuildImportantWarningDetails();
            string fullWarningText = string.Join(Environment.NewLine, importantWarnings.Select(LocalizedWarningMessage));
            ImportantWarningsTextBlock.Text = importantWarnings.Count <= 1
                ? fullWarningText
                : $"{LocalizedWarningMessage(importantWarnings[0])}  (+{importantWarnings.Count - 1} {(_language == UiLanguage.English ? "more" : "autre(s)")})";
            ImportantWarningsTextBlock.ToolTip = fullWarningText;
            ImportantWarningsBorder.Visibility = importantWarnings.Count == 0 ? Visibility.Collapsed : Visibility.Visible;

            if (!string.IsNullOrWhiteSpace(_selectedWarningKey))
            {
                DanteImportantWarning? activeWarning = importantWarnings.FirstOrDefault(warning => warning.Key == _selectedWarningKey);
                _warningDeviceNames.Clear();
                if (activeWarning is not null)
                {
                    _warningDeviceNames.UnionWith(activeWarning.DeviceNames);
                }
                else
                {
                    _selectedWarningKey = null;
                    if (SelectedOptionKey(DeviceFilterComboBox, _deviceFilterKeys[0]) == "DeviceFilter.WarningSelection")
                    {
                        DeviceFilterComboBox.SelectedItem = (DeviceFilterComboBox.ItemsSource as IEnumerable<LocalizedOption>)
                            ?.FirstOrDefault(option => option.Key == "DeviceFilter.All");
                    }
                }
            }

            _lockedDeviceNames.RemoveWhere(name => !deviceNames.Contains(name, StringComparer.OrdinalIgnoreCase));
            _warningDeviceNames.RemoveWhere(name => !deviceNames.Contains(name, StringComparer.OrdinalIgnoreCase));
            IReadOnlySet<string> modifiedDeviceNames = _project.GetModifiedDeviceNames();
            _deviceRows.Clear();
            foreach (DanteDevice device in ApplyDeviceFilter(devices))
            {
                _deviceRows.Add(new DeviceRow(
                    device,
                    _lockedDeviceNames.Contains(device.Name),
                    modifiedDeviceNames.Contains(device.Name)));
            }

            DeviceGrid.SelectedItems.Clear();
            foreach (DeviceRow row in _deviceRows.Where(row => selectedDeviceGridNames.Contains(row.Name)))
            {
                DeviceGrid.SelectedItems.Add(row);
            }

            DeviceComboBox.ItemsSource = deviceNames;
            DeviceComboBox.SelectedItem = deviceNames.Contains(selectedDevice) ? selectedDevice : deviceNames.FirstOrDefault();
            SelectLatency(GlobalLatencyComboBox, _latencies.First().XmlValue);
            SelectSampleRate(GlobalSampleRateComboBox, devices.Select(device => device.Samplerate).FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? _sampleRates.First().XmlValue);
            SelectEncoding(GlobalEncodingComboBox, devices.Select(device => device.Encoding).FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? _encodings.First().XmlValue);

            string[] senderFilterItems = new[] { AllSendersItem }.Concat(deviceNames).ToArray();
            string[] receiverFilterItems = new[] { AllReceiversItem }.Concat(deviceNames).ToArray();
            SenderDeviceList.ItemsSource = senderFilterItems;
            SenderDeviceList.SelectedItem = deviceNames.Contains(selectedSenderFilter) ? selectedSenderFilter : AllSendersItem;
            ReceiverDeviceList.ItemsSource = receiverFilterItems;
            ReceiverDeviceList.SelectedItem = deviceNames.Contains(selectedReceiverFilter) ? selectedReceiverFilter : AllReceiversItem;
            SourceDeviceComboBox.ItemsSource = deviceNames;
            SourceDeviceComboBox.SelectedItem = deviceNames.Contains(selectedSourceDevice) ? selectedSourceDevice : deviceNames.FirstOrDefault();
            string selectedPreferredMaster = ExclusivePreferredMasterComboBox.SelectedItem as string
                ?? devices.FirstOrDefault(device => device.PreferredMaster)?.Name
                ?? deviceNames.FirstOrDefault()
                ?? string.Empty;
            ExclusivePreferredMasterComboBox.ItemsSource = deviceNames;
            ExclusivePreferredMasterComboBox.SelectedItem = deviceNames.Contains(selectedPreferredMaster, StringComparer.OrdinalIgnoreCase)
                ? deviceNames.First(name => string.Equals(name, selectedPreferredMaster, StringComparison.OrdinalIgnoreCase))
                : deviceNames.FirstOrDefault();

            SaveSummaryTextBox.Text = _project.BuildSaveSummary();
            RefreshGlobalSearchResults();
            RefreshHealthPage();
            RefreshSynopticWorkspace();
            UpdateCommandState();
        }
        finally
        {
            _refreshingUi = false;
        }

        DeviceComboBox_SelectionChanged(DeviceComboBox, new SelectionChangedEventArgs(Selector.SelectionChangedEvent, new List<object>(), new List<object>()));
        RefreshSourceChannels();
        RefreshChannelSelector();
        RefreshPatchRows();
        ApplyPatchViewMode();
        RefreshEasyPatchWorkspace();
        UpdateCommandState();
    }

    private IEnumerable<DanteDevice> ApplyDeviceFilter(IEnumerable<DanteDevice> devices)
    {
        DanteDevice[] materializedDevices = devices.ToArray();
        string filter = SelectedOptionKey(DeviceFilterComboBox, _deviceFilterKeys[0]);
        string majoritySamplerate = MostCommonValue(materializedDevices.Select(device => device.Samplerate));
        string majorityEncoding = MostCommonValue(materializedDevices.Select(device => device.Encoding));
        bool hasMultipleSamplerates = materializedDevices.Select(device => device.Samplerate).Where(value => !string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.OrdinalIgnoreCase).Skip(1).Any();
        bool hasMultipleEncodings = materializedDevices.Select(device => device.Encoding).Where(value => !string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.OrdinalIgnoreCase).Skip(1).Any();
        IReadOnlySet<string> modifiedDeviceNames = filter == "DeviceFilter.Modified" && _project is not null
            ? _project.GetModifiedDeviceNames()
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        return filter switch
        {
            "DeviceFilter.Locked" => materializedDevices.Where(device => _lockedDeviceNames.Contains(device.Name)),
            "DeviceFilter.StaticIp" => materializedDevices.Where(device => device.UsesStaticIp),
            "DeviceFilter.PreferredMaster" => materializedDevices.Where(device => device.PreferredMaster),
            "DeviceFilter.Redundant" => materializedDevices.Where(device => device.IsRedundant),
            "DeviceFilter.Daisychain" => materializedDevices.Where(device => !device.IsRedundant),
            "DeviceFilter.NoTx" => materializedDevices.Where(device => device.TxCount == 0),
            "DeviceFilter.NoRx" => materializedDevices.Where(device => device.RxCount == 0),
            "DeviceFilter.Modified" => materializedDevices.Where(device => modifiedDeviceNames.Contains(device.Name)),
            "DeviceFilter.WarningSelection" => materializedDevices.Where(device => _warningDeviceNames.Contains(device.Name)),
            "DeviceFilter.SampleRateDifferent" => hasMultipleSamplerates
                ? materializedDevices.Where(device => !string.Equals(device.Samplerate, majoritySamplerate, StringComparison.OrdinalIgnoreCase))
                : [],
            "DeviceFilter.EncodingDifferent" => hasMultipleEncodings
                ? materializedDevices.Where(device => !string.Equals(device.Encoding, majorityEncoding, StringComparison.OrdinalIgnoreCase))
                : [],
            _ => materializedDevices
        };
    }

    private static string MostCommonValue(IEnumerable<string> values)
    {
        return values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .GroupBy(value => value, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.Key)
            .FirstOrDefault() ?? string.Empty;
    }

    private TargetDeviceSet? GetTargetDeviceSet()
    {
        if (!EnsureProjectLoaded())
        {
            return null;
        }

        string scopeKey = SelectedOptionKey(TargetScopeComboBox, _targetScopeKeys[0]);
        string scopeLabel = SelectedOptionDisplay(TargetScopeComboBox, T(_targetScopeKeys[0]));
        IEnumerable<DeviceRow> sourceRows;
        int lockedSkipped;

        if (scopeKey == "Target.SelectedUnlocked")
        {
            DeviceRow[] selectedRows = DeviceGrid.SelectedItems.OfType<DeviceRow>().ToArray();
            if (selectedRows.Length == 0)
            {
                ShowError("Aucune machine sélectionnée", "Sélectionnez une ou plusieurs machines dans le tableau, ou choisissez une autre cible.");
                return null;
            }

            sourceRows = selectedRows;
            lockedSkipped = selectedRows.Count(row => _lockedDeviceNames.Contains(row.Name));
        }
        else if (scopeKey == "Target.FilteredUnlocked")
        {
            sourceRows = _deviceRows;
            lockedSkipped = _deviceRows.Count(row => _lockedDeviceNames.Contains(row.Name));
        }
        else
        {
            DanteDevice[] allDevices = _project!.Devices.ToArray();
            lockedSkipped = allDevices.Count(device => _lockedDeviceNames.Contains(device.Name));
            DanteDevice[] unlockedDevices = allDevices
                .Where(device => !_lockedDeviceNames.Contains(device.Name))
                .ToArray();
            if (unlockedDevices.Length == 0)
            {
                ShowError("Aucune machine modifiable", "Toutes les machines de cette cible sont verrouillées.");
                return null;
            }

            return new TargetDeviceSet(unlockedDevices, lockedSkipped, scopeLabel);
        }

        DanteDevice[] devices = sourceRows
            .Where(row => !_lockedDeviceNames.Contains(row.Name))
            .Select(row => row.Device)
            .GroupBy(device => device.Name, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToArray();

        if (devices.Length == 0)
        {
            ShowError("Aucune machine modifiable", "La cible ne contient aucune machine non verrouillée.");
            return null;
        }

        return new TargetDeviceSet(devices, lockedSkipped, scopeLabel);
    }

    private string BuildTargetPreview(
        string action,
        TargetDeviceSet target,
        IEnumerable<(DanteDevice Device, string Before, string After, bool Changed)> rows)
    {
        (DanteDevice Device, string Before, string After, bool Changed)[] materializedRows = rows.ToArray();
        StringBuilder builder = new();
        builder.AppendLine("Prévisualisation");
        builder.AppendLine("----------------");
        builder.AppendLine("Action : " + action);
        builder.AppendLine("Cible : " + target.ScopeLabel);
        builder.AppendLine($"Machines modifiables : {target.Devices.Length}");
        if (target.LockedSkippedCount > 0)
        {
            builder.AppendLine($"Machines verrouillées ignorées : {target.LockedSkippedCount}");
        }

        builder.AppendLine();
        builder.AppendLine("Avant / après :");
        foreach ((DanteDevice device, string before, string after, bool changed) in materializedRows.Take(80))
        {
            builder.AppendLine($"- {device.Name} : {Blank(before)} -> {(changed ? Blank(after) : "inchangé")}");
        }

        if (materializedRows.Length > 80)
        {
            builder.AppendLine($"- {materializedRows.Length - 80} machine(s) supplémentaire(s) non affichée(s).");
        }

        builder.AppendLine();
        builder.AppendLine("Résumé :");
        builder.AppendLine($"- {materializedRows.Count(row => row.Changed)} machine(s) modifiée(s)");
        builder.AppendLine($"- {materializedRows.Count(row => !row.Changed)} machine(s) inchangée(s) ou ignorée(s)");
        return builder.ToString();
    }

    private static bool TryBuildIpAddress(string prefix, int host, out string address, out string error)
    {
        address = string.Empty;
        error = string.Empty;

        string[] parts = prefix.Trim().Split('.');
        if (parts.Length != 3 || parts.Any(part => !int.TryParse(part, out int value) || value < 0 || value > 255))
        {
            error = "Le préfixe IP doit être au format 192.168.1";
            return false;
        }

        if (host < 1 || host > 254)
        {
            error = "Le numéro IP doit être compris entre 1 et 254.";
            return false;
        }

        address = $"{parts[0]}.{parts[1]}.{parts[2]}.{host}";
        return true;
    }

    private static bool IsValidIpv4(string value)
    {
        return IPAddress.TryParse(value.Trim(), out IPAddress? address)
            && address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork;
    }

    private void RefreshChannelSelector()
    {
        // Les trois listes de canaux partagent les mêmes choix : le canal à
        // renommer, le début de plage et la fin de plage.
        if (_project is null)
        {
            ChannelComboBox.ItemsSource = null;
            BatchRenameStartChannelComboBox.ItemsSource = null;
            BatchRenameEndChannelComboBox.ItemsSource = null;
            NewChannelNameTextBox.Text = string.Empty;
            return;
        }

        DanteDevice? device = _project.FindDevice(DeviceComboBox.SelectedItem as string);
        if (device is null)
        {
            ChannelComboBox.ItemsSource = null;
            BatchRenameStartChannelComboBox.ItemsSource = null;
            BatchRenameEndChannelComboBox.ItemsSource = null;
            NewChannelNameTextBox.Text = string.Empty;
            return;
        }

        DanteChannelKind kind = string.Equals(ChannelKindComboBox.SelectedItem as string, "RX", StringComparison.OrdinalIgnoreCase)
            ? DanteChannelKind.Rx
            : DanteChannelKind.Tx;

        string previousKey = ChannelComboBox.SelectedItem is ChannelChoice previous
            ? $"{previous.Kind}:{previous.Index}"
            : string.Empty;
        string previousStartKey = BatchRenameStartChannelComboBox.SelectedItem is ChannelChoice previousStart
            ? $"{previousStart.Kind}:{previousStart.Index}"
            : string.Empty;
        string previousEndKey = BatchRenameEndChannelComboBox.SelectedItem is ChannelChoice previousEnd
            ? $"{previousEnd.Kind}:{previousEnd.Index}"
            : string.Empty;

        ChannelChoice[] choices = (kind == DanteChannelKind.Tx ? device.TxChannels : device.RxChannels)
            .Select(channel => new ChannelChoice(kind, channel.Index, channel.DisplayName))
            .ToArray();

        ChannelComboBox.ItemsSource = choices;
        ChannelChoice? selected = choices.FirstOrDefault(choice => $"{choice.Kind}:{choice.Index}" == previousKey) ?? choices.FirstOrDefault();
        ChannelComboBox.SelectedItem = selected;
        BatchRenameStartChannelComboBox.ItemsSource = choices;
        BatchRenameEndChannelComboBox.ItemsSource = choices;
        BatchRenameStartChannelComboBox.SelectedItem = choices.FirstOrDefault(choice => $"{choice.Kind}:{choice.Index}" == previousStartKey) ?? choices.FirstOrDefault();
        BatchRenameEndChannelComboBox.SelectedItem = choices.FirstOrDefault(choice => $"{choice.Kind}:{choice.Index}" == previousEndKey) ?? choices.LastOrDefault();
        NewChannelNameTextBox.Text = selected?.Name ?? string.Empty;
    }

    private void RefreshPatchRows()
    {
        // La table Patch est filtrée côté interface, sans modifier le projet.
        _patchRows.Clear();

        if (_project is null)
        {
            PatchSummaryTextBlock.Text = T("Status.NoFileLoaded");
            return;
        }

        string search = PatchSearchTextBox.Text.Trim();
        string sender = SenderDeviceList.SelectedItem as string ?? AllSendersItem;
        string receiver = ReceiverDeviceList.SelectedItem as string ?? AllReceiversItem;
        string stateFilter = SelectedOptionKey(PatchStateFilterComboBox, _patchStateFilterKeys[0]);
        bool conflictsOnly = ConflictsOnlyCheckBox.IsChecked == true;

        IEnumerable<DanteSubscription> subscriptions = _project.PatchMatrix.Subscriptions;

        if (!string.IsNullOrWhiteSpace(search))
        {
            subscriptions = subscriptions.Where(subscription =>
                Contains(subscription.RxDevice, search)
                || Contains(subscription.RxChannelName, search)
                || Contains(subscription.DisplayTxDeviceName, search)
                || Contains(subscription.RawTxDeviceName, search)
                || Contains(subscription.ResolvedTxDeviceName, search)
                || Contains(subscription.TxChannelName, search));
        }

        if (!string.Equals(sender, AllSendersItem, StringComparison.OrdinalIgnoreCase))
        {
            subscriptions = subscriptions.Where(subscription => string.Equals(subscription.ResolvedTxDeviceName, sender, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.Equals(receiver, AllReceiversItem, StringComparison.OrdinalIgnoreCase))
        {
            subscriptions = subscriptions.Where(subscription => string.Equals(subscription.RxDevice, receiver, StringComparison.OrdinalIgnoreCase));
        }

        if (conflictsOnly)
        {
            subscriptions = subscriptions.Where(subscription => subscription.IsConflict);
        }

        subscriptions = stateFilter switch
        {
            "Filter.ActivePatches" => subscriptions.Where(subscription => subscription.IsActive),
            "Filter.FreeRx" => subscriptions.Where(subscription => !subscription.IsActive),
            "Filter.LocalPatches" => subscriptions.Where(subscription => subscription.IsLocalSubscription),
            "Filter.MissingTxDevices" => subscriptions.Where(subscription => subscription.IsExternalMissingDevice),
            "Filter.MissingTxChannels" => subscriptions.Where(subscription => subscription.IsTxChannelMissing),
            "Filter.Warnings" => subscriptions.Where(subscription => subscription.IsWarning),
            "Filter.Conflicts" => subscriptions.Where(subscription => subscription.IsConflict),
            "Filter.Modified" => subscriptions.Where(subscription => subscription.IsModified),
            _ => subscriptions
        };

        foreach (DanteSubscription subscription in subscriptions)
        {
            _patchRows.Add(subscription);
        }

        PatchSummaryTextBlock.Text = Tf(
            "Summary.PatchRows",
            _patchRows.Count,
            _project.PatchMatrix.ActivePatchCount,
            _project.PatchMatrix.LocalPatchCount,
            _project.PatchMatrix.WarningCount,
            _project.PatchMatrix.ConflictCount);
    }

    private void RefreshHealthPage()
    {
        _healthIssues.Clear();

        if (_project is null)
        {
            HealthSummaryTextBlock.Text = T("Status.NoFileLoaded");
            return;
        }

        DanteValidationResult validation = _project.Validate();
        string filter = SelectedOptionKey(HealthFilterComboBox, _healthFilterKeys[0]);
        IEnumerable<DanteValidationIssue> issues = validation.Issues;
        issues = filter switch
        {
            "Filter.Info" => issues.Where(issue => issue.Severity == DanteIssueSeverity.Info),
            "Filter.HealthWarnings" => issues.Where(issue => issue.Severity == DanteIssueSeverity.Warning),
            "Filter.Errors" => issues.Where(issue => issue.Severity == DanteIssueSeverity.Error),
            "Filter.Patches" => issues.Where(issue => issue.Category == DanteIssueCategory.Patch),
            "Filter.Devices" => issues.Where(issue => issue.Category == DanteIssueCategory.Device),
            "Filter.Clock" => issues.Where(issue => issue.Category == DanteIssueCategory.Clock),
            "Filter.Network" => issues.Where(issue => issue.Category == DanteIssueCategory.Network),
            "Filter.XmlCompatibility" => issues.Where(issue => issue.Category == DanteIssueCategory.XmlCompatibility),
            _ => issues
        };

        foreach (DanteValidationIssue issue in issues.OrderByDescending(issue => issue.Severity).ThenBy(issue => issue.CategoryLabel).Take(500))
        {
            _healthIssues.Add(issue);
        }

        HealthSummaryTextBlock.Text = Tf(
            "Summary.Health",
            _project.PresetName,
            Blank(_project.PresetVersion),
            _editModeEnabled ? T("Status.EditMode").Replace("Mode : ", string.Empty).Replace("Mode: ", string.Empty) : T("Status.ReadOnlyMode").Replace("Mode : ", string.Empty).Replace("Mode: ", string.Empty),
            _project.OriginalFilePath,
            _project.Devices.Count,
            _project.Devices.Sum(device => device.TxCount),
            _project.Devices.Sum(device => device.RxCount),
            _project.PatchMatrix.ActivePatchCount,
            _project.PatchMatrix.FreeRxCount,
            _project.PatchMatrix.LocalPatchCount,
            _project.PatchMatrix.ExternalMissingDeviceCount,
            _project.PatchMatrix.MissingTxChannelCount,
            _project.Devices.Count(device => device.PreferredMaster),
            DistinctDeviceValues("samplerate"),
            DistinctDeviceValues("encoding"),
            DistinctLatencies(),
            _project.Devices.Count(device => device.IsRedundant),
            _project.Devices.Count(device => !device.IsRedundant),
            _project.Devices.Count(device => device.UsesStaticIp),
            validation.Errors.Count,
            validation.Warnings.Count);
    }

    private void RefreshSourceChannels()
    {
        if (_project is null)
        {
            SourceChannelComboBox.ItemsSource = null;
            return;
        }

        DanteDevice? sourceDevice = _project.FindDevice(SourceDeviceComboBox.SelectedItem as string);
        object[] channels = sourceDevice?.TxChannels
            .Select(channel => (object)new TxChannelChoice(sourceDevice.Name, channel.DanteId, channel.DisplayName))
            .Prepend(string.Empty)
            .ToArray() ?? new object[] { string.Empty };
        string previous = SelectedSourceChannelName();
        SourceChannelComboBox.ItemsSource = channels;
        SourceChannelComboBox.SelectedItem = channels.OfType<TxChannelChoice>().FirstOrDefault(choice => string.Equals(choice.ChannelName, previous, StringComparison.OrdinalIgnoreCase))
            ?? channels.FirstOrDefault();
    }

    private void RefreshGlobalSearchResults()
    {
        // Recherche simple dans les champs déjà interprétés par l'application.
        string search = GlobalSearchTextBox.Text.Trim();
        _searchResults.Clear();

        if (_project is null)
        {
            UpdateGlobalSearchHint(T("Search.NoFileLoaded"));
            return;
        }

        if (search.Length < 2)
        {
            UpdateGlobalSearchHint(T("Search.Hint"));
            return;
        }

        foreach (DanteDevice device in _project.Devices)
        {
            if (Contains(device.Name, search) || Contains(device.FriendlyName, search))
            {
                _searchResults.Add(new GlobalSearchResult("Machine", device.Name, DeviceName: device.Name));
            }

            foreach (DanteChannel channel in device.TxChannels)
            {
                if (Contains(channel.DisplayName, search))
                {
                    _searchResults.Add(new GlobalSearchResult("Canal TX", $"{device.Name} / {channel.Index} - {channel.DisplayName}", device.Name, DanteChannelKind.Tx, channel.Index));
                }
            }

            foreach (DanteChannel channel in device.RxChannels)
            {
                if (Contains(channel.DisplayName, search))
                {
                    _searchResults.Add(new GlobalSearchResult("Canal RX", $"{device.Name} / {channel.Index} - {channel.DisplayName}", device.Name, DanteChannelKind.Rx, channel.Index));
                }
            }
        }

        foreach (DanteSubscription subscription in _project.PatchMatrix.Subscriptions)
        {
            if (Contains(subscription.RxDevice, search)
                || Contains(subscription.RxChannelName, search)
                || Contains(subscription.DisplayTxDeviceName, search)
                || Contains(subscription.RawTxDeviceName, search)
                || Contains(subscription.ResolvedTxDeviceName, search)
                || Contains(subscription.TxChannelName, search)
                || Contains(subscription.Status, search))
            {
                _searchResults.Add(new GlobalSearchResult(
                    "Patch",
                    $"{subscription.RxDevice} / {subscription.RxChannelName} -> {Blank(subscription.DisplayTxDeviceName)} / {Blank(subscription.TxChannelName)}",
                    RxDevice: subscription.RxDevice,
                    RxIndex: subscription.RxIndex));
            }
        }

        while (_searchResults.Count > 80)
        {
            _searchResults.RemoveAt(_searchResults.Count - 1);
        }

        UpdateGlobalSearchHint(_searchResults.Count == 0 ? T("Search.NoResult") : string.Empty);
    }

    private void UpdateGlobalSearchHint(string message)
    {
        GlobalSearchHintTextBlock.Text = message;
        GlobalSearchHintTextBlock.Visibility = string.IsNullOrWhiteSpace(message) ? Visibility.Collapsed : Visibility.Visible;
    }

    private void RefreshRecentFiles()
    {
        IReadOnlyList<string> recentFiles = RecentFilesService.Load();
        RecentFilesComboBox.ItemsSource = recentFiles;
        RecentFilesComboBox.SelectedItem = recentFiles.FirstOrDefault();
    }

    private void SetupLanguageComboBox()
    {
        bool previousRefreshing = _refreshingUi;
        _refreshingUi = true;
        try
        {
            LocalizedOption[] languages =
            [
                new("Language.French", T("Language.French")),
                new("Language.English", T("Language.English"))
            ];

            string selectedKey = _language == UiLanguage.English ? "Language.English" : "Language.French";
            LanguageComboBox.ItemsSource = languages;
            LanguageComboBox.SelectedItem = languages.First(option => option.Key == selectedKey);
        }
        finally
        {
            _refreshingUi = previousRefreshing;
        }
    }

    private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_refreshingUi || LanguageComboBox.SelectedItem is not LocalizedOption option)
        {
            return;
        }

        _language = option.Key == "Language.English" ? UiLanguage.English : UiLanguage.French;
        LanguageSettingsService.Save(_language);
        SetupLanguageComboBox();
        RefreshLocalizedOptionSources();
        ApplyLanguageToInterface();
        RefreshAll();
        SetStatus(T("Status.Ready"));
    }

    private void RefreshLocalizedOptionSources()
    {
        bool previousRefreshing = _refreshingUi;
        _refreshingUi = true;
        try
        {
            SetOptions(PatchStateFilterComboBox, _patchStateFilterKeys, SelectedOptionKey(PatchStateFilterComboBox, _patchStateFilterKeys[0]));
            SetOptions(PatchViewModeComboBox, _patchViewModeKeys, SelectedOptionKey(PatchViewModeComboBox, _patchViewModeKeys[0]));
            SetOptions(HealthFilterComboBox, _healthFilterKeys, SelectedOptionKey(HealthFilterComboBox, _healthFilterKeys[0]));
            SetOptions(PatchbookScopeComboBox, _patchbookScopeKeys, SelectedOptionKey(PatchbookScopeComboBox, _patchbookScopeKeys[0]));
            SetOptions(DeviceFilterComboBox, _deviceFilterKeys, SelectedOptionKey(DeviceFilterComboBox, _deviceFilterKeys[0]));
            SetOptions(TargetScopeComboBox, _targetScopeKeys, SelectedOptionKey(TargetScopeComboBox, _targetScopeKeys[0]));
            RefreshQuickProfileOptions();
        }
        finally
        {
            _refreshingUi = previousRefreshing;
        }
    }

    private void RefreshQuickProfileOptions()
    {
        string selectedKey = (QuickProfileComboBox.SelectedItem as ProfileChoice)?.Profile.Key
            ?? DeviceProfileCatalog.BuiltIn[0].Key;
        ProfileChoice[] profiles = DeviceProfileCatalog.BuiltIn
            .Select(profile => new ProfileChoice(profile, T(profile.Key)))
            .ToArray();
        QuickProfileComboBox.ItemsSource = profiles;
        QuickProfileComboBox.SelectedItem = profiles.FirstOrDefault(choice => choice.Profile.Key == selectedKey)
            ?? profiles.FirstOrDefault();
    }

    private void SetOptions(ComboBox comboBox, IEnumerable<string> keys, string selectedKey)
    {
        LocalizedOption[] options = keys.Select(key => new LocalizedOption(key, T(key))).ToArray();
        comboBox.ItemsSource = options;
        comboBox.SelectedItem = options.FirstOrDefault(option => option.Key == selectedKey) ?? options.FirstOrDefault();
    }

    private string SelectedOptionKey(ComboBox comboBox, string fallback)
    {
        return comboBox.SelectedItem is LocalizedOption option ? option.Key : fallback;
    }

    private string SelectedOptionDisplay(ComboBox comboBox, string fallback)
    {
        return comboBox.SelectedItem is LocalizedOption option ? option.Display : fallback;
    }

    private void ApplyLanguageToInterface()
    {
        LanguageLabelTextBlock.Text = T("Language.Label");
        TranslateDependencyObject(this, []);
        UpdateConfigurationEditorsToggleText();
        ApplyDataGridColumnHeaders();
        RefreshGlobalSearchResults();
        UpdateCommandState();
    }

    private void TranslateDependencyObject(DependencyObject dependencyObject, HashSet<DependencyObject> visited)
    {
        if (!visited.Add(dependencyObject))
        {
            return;
        }

        switch (dependencyObject)
        {
            case HeaderedContentControl headeredContentControl when headeredContentControl.Header is string header:
                headeredContentControl.Header = LocalizeLiteral(header);
                break;
            case ContentControl contentControl when contentControl.Content is string content:
                contentControl.Content = LocalizeLiteral(content);
                break;
            case TextBlock textBlock:
                textBlock.Text = LocalizeLiteral(textBlock.Text);
                break;
        }

        // Un bouton est aussi un ContentControl : son info-bulle doit donc
        // être traduite séparément du texte qu'il affiche.
        if (dependencyObject is FrameworkElement frameworkElement && frameworkElement.ToolTip is string tooltip)
        {
            frameworkElement.ToolTip = LocalizeLiteral(tooltip);
        }

        string automationName = AutomationProperties.GetName(dependencyObject);
        if (!string.IsNullOrWhiteSpace(automationName))
        {
            AutomationProperties.SetName(dependencyObject, LocalizeLiteral(automationName));
        }

        string automationHelpText = AutomationProperties.GetHelpText(dependencyObject);
        if (!string.IsNullOrWhiteSpace(automationHelpText))
        {
            AutomationProperties.SetHelpText(dependencyObject, LocalizeLiteral(automationHelpText));
        }

        foreach (object child in LogicalTreeHelper.GetChildren(dependencyObject))
        {
            if (child is DependencyObject childObject)
            {
                TranslateDependencyObject(childObject, visited);
            }
        }
    }

    private void ApplyDataGridColumnHeaders()
    {
        foreach (DataGrid dataGrid in new[] { DeviceGrid, PatchGrid, HealthIssuesGrid, SynopticDeviceGrid })
        {
            foreach (DataGridColumn column in dataGrid.Columns)
            {
                if (column.Header is string header)
                {
                    column.Header = LocalizeLiteral(header);
                }
            }
        }
    }

    private void UpdateCommandState()
    {
        bool hasProject = _project is not null;
        bool canUseProjectActions = hasProject;

        ActivateEditButton.IsEnabled = hasProject && !_editModeEnabled;
        ActivateEditButton.Content = hasProject && _editModeEnabled ? T("Status.EditActiveButton") : T("Status.ActivateEditButton");
        SaveAsButton.IsEnabled = hasProject;
        RevertButton.IsEnabled = hasProject;
        ShowDeviceChangesButton.IsEnabled = hasProject;
        UndoLastButton.IsEnabled = hasProject && _project?.CanUndo == true;
        UndoLastButton.Content = LocalizeLiteral("Annuler action");

        foreach (Control control in EditableControls())
        {
            control.IsEnabled = canUseProjectActions;
        }
    }

    private IEnumerable<Control> EditableControls()
    {
        yield return MergeXmlButton;
        yield return ApplyDeviceSettingsButton;
        yield return DeleteDeviceButton;
        yield return DuplicateDeviceButton;
        yield return ResetDevicePatchesButton;
        yield return ResetDeviceRxPatchesButton;
        yield return ResetDeviceTxPatchesButton;
        yield return OpenDeviceDetailsButton;
        yield return RenameChannelButton;
        yield return ResetDeviceChannelsButton;
        yield return BatchRenameButton;
        yield return ImportChannelLabelsButton;
        yield return ExportChannelLabelsButton;
        yield return ApplyAllNetworkButton;
        yield return ApplyAllLatencyButton;
        yield return ApplyAllSampleRateButton;
        yield return ApplyAllEncodingButton;
        yield return ApplyQuickProfileButton;
        yield return ApplyAllIpAutoButton;
        yield return ApplyAllIpStaticButton;
        yield return ResetAllChannelsButton;
        yield return ApplyPatchButton;
        yield return RemovePatchButton;
        yield return OpenVisualPatchButton;
        yield return ApplyExclusivePreferredMasterButton;
        yield return AtomicChaosButton;
    }

    private string BuildDefaultReportFileName(string extension)
    {
        string source = _project?.OriginalFilePath ?? "rapport";
        string name = Path.GetFileNameWithoutExtension(source);
        return $"{name}_rapport_DanteConfigEditor{extension}";
    }

    private string BuildMergeResultLog(string path, DanteMergeResult result)
    {
        string details = _language == UiLanguage.English
            ? $"XML import: {Path.GetFileName(path)} - {result.ImportedDeviceCount} device(s) imported, {result.RenamedDeviceCount} renamed, {result.SkippedDuplicateDeviceCount} duplicate(s) skipped."
            : $"Import XML : {Path.GetFileName(path)} - {result.ImportedDeviceCount} machine(s) importée(s), {result.RenamedDeviceCount} renommée(s), {result.SkippedDuplicateDeviceCount} doublon(s) ignoré(s).";

        if (result.RenamedDevices.Count > 0)
        {
            details += " " + string.Join(", ", result.RenamedDevices.Select(item => $"{item.Key} -> {item.Value}"));
        }

        return details;
    }

    private string BuildMergeResultStatus(DanteMergeResult result)
    {
        return _language == UiLanguage.English
            ? $"XML added: {result.ImportedDeviceCount} imported, {result.RenamedDeviceCount} renamed, {result.SkippedDuplicateDeviceCount} skipped."
            : $"XML ajouté : {result.ImportedDeviceCount} importée(s), {result.RenamedDeviceCount} renommée(s), {result.SkippedDuplicateDeviceCount} ignorée(s).";
    }

    private bool RunProjectAction(string successMessage, Action action, string? confirmationMessage = null)
    {
        // Toutes les actions qui modifient le XML passent ici : confirmation,
        // copie d'annulation, exécution, rafraîchissement et retour arrière si erreur.
        if (!EnsureProjectLoaded())
        {
            return false;
        }

        if (!EnsureEditMode())
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(confirmationMessage))
        {
            MessageBoxResult confirm = MessageBox.Show(this, confirmationMessage, T("Dialog.ConfirmTitle"), MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.Yes)
            {
                return false;
            }
        }

        try
        {
            _project!.PushUndoSnapshot(successMessage);
            action();
            AddLog(successMessage);
            RefreshAll();
            ScheduleRecoverySnapshot();
            SetStatus(successMessage);
            return true;
        }
        catch (Exception ex)
        {
            _project?.RestoreLastUndoSnapshot();
            RefreshAll();
            ScheduleRecoverySnapshot();
            ShowError("Action impossible", ex);
            return false;
        }
    }

    private static T? FindVisualChild<T>(DependencyObject? parent)
        where T : DependencyObject
    {
        if (parent is null)
        {
            return null;
        }

        if (parent is T match)
        {
            return match;
        }

        for (int index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++)
        {
            if (FindVisualChild<T>(VisualTreeHelper.GetChild(parent, index)) is T descendant)
            {
                return descendant;
            }
        }

        return null;
    }

    private static T? FindVisualParent<T>(DependencyObject? child)
        where T : DependencyObject
    {
        while (child is not null)
        {
            if (child is T match)
            {
                return match;
            }

            child = VisualTreeHelper.GetParent(child);
        }

        return null;
    }

    private string SelectedDeviceName()
    {
        return DeviceComboBox.SelectedItem as string ?? throw new InvalidOperationException("Aucun device sélectionné.");
    }

    private bool EnsureProjectLoaded()
    {
        if (_project is not null)
        {
            return true;
        }

        ShowError("Aucun fichier chargé", "Ouvrez d'abord un fichier XML de configuration Dante.");
        return false;
    }

    private bool EnsureEditMode()
    {
        if (_editModeEnabled)
        {
            return true;
        }

        _editModeEnabled = true;
        AddLog("Mode édition activé automatiquement.");
        UpdateCommandState();
        SetStatus("Mode édition activé.");
        return true;
    }

    private bool IsOriginalProjectPath(string candidatePath)
    {
        if (_project is null || string.IsNullOrWhiteSpace(candidatePath))
        {
            return false;
        }

        string originalPath = Path.GetFullPath(_project.OriginalFilePath);
        string selectedPath = Path.GetFullPath(candidatePath);
        return string.Equals(originalPath, selectedPath, StringComparison.OrdinalIgnoreCase);
    }

    private string SelectedLatencyXmlValue(ComboBox comboBox)
    {
        return comboBox.SelectedItem is LatencyChoice latencyChoice
            ? latencyChoice.XmlValue
            : comboBox.SelectedItem as string ?? string.Empty;
    }

    private string SelectedSampleRateXmlValue(ComboBox comboBox)
    {
        return comboBox.SelectedItem is SampleRateChoice sampleRateChoice
            ? sampleRateChoice.XmlValue
            : !string.IsNullOrWhiteSpace(comboBox.Text)
                ? comboBox.Text
                : comboBox.SelectedItem as string ?? string.Empty;
    }

    private string SelectedEncodingXmlValue(ComboBox comboBox)
    {
        return comboBox.SelectedItem is EncodingChoice encodingChoice
            ? encodingChoice.XmlValue
            : !string.IsNullOrWhiteSpace(comboBox.Text)
                ? comboBox.Text
                : comboBox.SelectedItem as string ?? string.Empty;
    }

    private void SelectLatency(ComboBox comboBox, string xmlValue)
    {
        comboBox.SelectedItem = _latencies.FirstOrDefault(latency => string.Equals(latency.XmlValue, xmlValue, StringComparison.OrdinalIgnoreCase));
    }

    private void SelectSampleRate(ComboBox comboBox, string xmlValue)
    {
        SampleRateChoice? choice = _sampleRates.FirstOrDefault(sampleRate => string.Equals(sampleRate.XmlValue, xmlValue, StringComparison.OrdinalIgnoreCase));
        comboBox.SelectedItem = choice;
        comboBox.Text = choice?.Display ?? xmlValue;
    }

    private void SelectEncoding(ComboBox comboBox, string xmlValue)
    {
        EncodingChoice? choice = _encodings.FirstOrDefault(encoding => string.Equals(encoding.XmlValue, xmlValue, StringComparison.OrdinalIgnoreCase));
        comboBox.SelectedItem = choice;
        comboBox.Text = choice?.Display ?? xmlValue;
    }

    private string SelectedSourceChannelName()
    {
        return SourceChannelComboBox.SelectedItem switch
        {
            TxChannelChoice choice => choice.ChannelName,
            string value => value,
            _ => string.Empty
        };
    }

    private void SelectSourceChannel(string channelName)
    {
        if (SourceChannelComboBox.ItemsSource is IEnumerable<object> items)
        {
            SourceChannelComboBox.SelectedItem = (object?)items.OfType<TxChannelChoice>().FirstOrDefault(choice => string.Equals(choice.ChannelName, channelName, StringComparison.OrdinalIgnoreCase))
                ?? items.OfType<string>().FirstOrDefault(value => string.Equals(value, channelName, StringComparison.OrdinalIgnoreCase));
        }
    }

    private void ApplyPatchViewMode()
    {
        string selectedKey = SelectedOptionKey(PatchViewModeComboBox, PatchViewMode.SimpleKey);
        bool expert = PatchViewMode.IsExpert(selectedKey);
        Visibility expertVisibility = expert ? Visibility.Visible : Visibility.Collapsed;

        PatchDisplayTxColumn.Visibility = Visibility.Visible;
        PatchTxDanteIdColumn.Visibility = Visibility.Visible;
        PatchRawTxColumn.Visibility = expertVisibility;
        PatchResolvedTxColumn.Visibility = expertVisibility;
        PatchTxChannelColumn.Visibility = Visibility.Visible;
        PatchTypeColumn.Visibility = expertVisibility;
        PatchActiveColumn.Visibility = expertVisibility;
        PatchModifiedColumn.Visibility = expertVisibility;
        PatchSourceFullColumn.Visibility = expertVisibility;
    }

    private string DistinctDeviceValues(string elementName)
    {
        string[] values = _project?.Devices
            .Select(device => device.Element.Element(elementName)?.Value.Trim() ?? string.Empty)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray() ?? [];

        return values.Length == 0 ? "-" : string.Join(", ", values);
    }

    private string DistinctLatencies()
    {
        string[] values = _project?.Devices
            .Select(device => device.Latency)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(DanteLatencyFormatter.FormatLatencyDisplay)
            .ToArray() ?? [];

        return values.Length == 0 ? "-" : string.Join(", ", values);
    }

    private void ShowProjectList(string title, Func<DanteProject, string> contentFactory)
    {
        if (!EnsureProjectLoaded())
        {
            return;
        }

        MessageBox.Show(this, contentFactory(_project!), title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void AddLog(string message)
    {
        _logs.Insert(0, $"{DateTime.Now:HH:mm:ss} - {message}");
    }

    private void SetStatus(string message)
    {
        StatusTextBlock.Text = message;
    }

    private void ShowError(string title, string message)
    {
        AddLog(title + " - " + message);
        SetStatus(title);
        DiagnosticLogService.Default.Write("UI", $"{title}: {message}");
        MessageBox.Show(this, message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void ShowError(string title, Exception exception)
    {
        AddLog(title + " - " + exception.Message);
        SetStatus(title);
        DiagnosticLogService.Default.Write("UI", title, exception);
        MessageBox.Show(this, exception.Message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void OpenBundledDocument(string fileName)
    {
        string[] candidates =
        [
            Path.Combine(AppContext.BaseDirectory, fileName),
            Path.Combine(AppContext.BaseDirectory, "docs", fileName),
            Path.Combine(Environment.CurrentDirectory, fileName),
            Path.Combine(Environment.CurrentDirectory, "docs", fileName)
        ];

        string? path = candidates.FirstOrDefault(File.Exists);
        if (path is null)
        {
            ShowError("Notice introuvable", $"Le fichier {fileName} est introuvable.");
            return;
        }

        Process.Start(new ProcessStartInfo(path)
        {
            UseShellExecute = true
        });
    }

    private void SetTheme(bool useLightTheme)
    {
        if (useLightTheme)
        {
            SetBrush("WindowBackgroundBrush", "#F3F6FA");
            SetBrush("SurfaceBrush", "#FFFFFF");
            SetBrush("SurfaceAltBrush", "#EAF0F7");
            SetBrush("TextBrush", "#111827");
            SetBrush("MutedTextBrush", "#4B5563");
            SetBrush("BorderLineBrush", "#CBD5E1");
            SetBrush("AccentBrush", "#1D4ED8");
            SetBrush("AccentDarkBrush", "#1E40AF");
            SetBrush("DangerBrush", "#B91C1C");
            SetBrush("SuccessBrush", "#047857");
        }
        else
        {
            SetBrush("WindowBackgroundBrush", "#10141F");
            SetBrush("SurfaceBrush", "#171D2B");
            SetBrush("SurfaceAltBrush", "#202838");
            SetBrush("TextBrush", "#F6F8FB");
            SetBrush("MutedTextBrush", "#AAB4C5");
            SetBrush("BorderLineBrush", "#334057");
            SetBrush("AccentBrush", "#2F80ED");
            SetBrush("AccentDarkBrush", "#1D5FB8");
            SetBrush("DangerBrush", "#D64545");
            SetBrush("SuccessBrush", "#2E9D62");
        }
    }

    private void SetBrush(string key, string hex)
    {
        Resources[key] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
    }

    private static bool Contains(string value, string search)
    {
        return value.Contains(search, StringComparison.OrdinalIgnoreCase);
    }

    private string T(string key)
    {
        return LocalizationService.Text(_language, key);
    }

    private string Tf(string key, params object[] args)
    {
        return LocalizationService.Format(_language, key, args);
    }

    private string LocalizeLiteral(string value)
    {
        return LocalizationService.TranslateLiteral(_language, value);
    }

    private string LocalizedWarningMessage(DanteImportantWarning warning)
    {
        return warning.LocalizedMessage(_language == UiLanguage.English);
    }

    private string Blank(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? T("Blank") : value;
    }
}
