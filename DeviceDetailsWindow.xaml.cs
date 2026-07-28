using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using DanteConfigEditor.Models;
using DanteConfigEditor.Services;

namespace DanteConfigEditor;

public partial class DeviceDetailsWindow : Window
{
    private readonly UiLanguage _language;
    private readonly DanteProject _project;
    private readonly DanteDevice _device;
    private readonly bool _useLightTheme;
    private readonly ObservableCollection<DeviceChannelEditItem> _txChannels;
    private readonly ObservableCollection<DeviceChannelEditItem> _rxChannels;
    private readonly List<PatchEditRequest> _patchEdits = [];
    private readonly DeviceDetailsResult _initialState;
    private bool _updatingDeviceSelector;

    private readonly DeviceOption[] _latencies =
    [
        new("250", "0,25 ms"),
        new("1000", "1 ms"),
        new("2000", "2 ms"),
        new("5000", "5 ms")
    ];

    private readonly DeviceOption[] _sampleRates =
    [
        new("44100", "44,1 kHz"),
        new("48000", "48 kHz"),
        new("88200", "88,2 kHz"),
        new("96000", "96 kHz"),
        new("176400", "176,4 kHz"),
        new("192000", "192 kHz")
    ];

    private readonly DeviceOption[] _encodings =
    [
        new("16", "16 bit"),
        new("24", "24 bit"),
        new("32", "32 bit")
    ];

    public DeviceDetailsWindow(
        UiLanguage language,
        DanteProject project,
        DanteDevice device,
        bool useLightTheme)
    {
        InitializeComponent();
        DialogThemeService.Apply(this, useLightTheme);
        _language = language;
        _project = project ?? throw new ArgumentNullException(nameof(project));
        _device = device ?? throw new ArgumentNullException(nameof(device));
        _useLightTheme = useLightTheme;
        OriginalDeviceName = device.Name;
        Title = L("Détail machine", "Device details");
        TitleTextBlock.Text = Title;
        DeviceNameTextBox.Text = device.Name;
        RedundantRadioButton.IsChecked = device.IsRedundant;
        DaisychainRadioButton.IsChecked = !device.IsRedundant;
        PreferredMasterCheckBox.IsChecked = device.PreferredMaster;

        LatencyComboBox.ItemsSource = _latencies;
        SampleRateComboBox.ItemsSource = _sampleRates;
        EncodingComboBox.ItemsSource = _encodings;
        SelectOption(LatencyComboBox, _latencies, device.Latency);
        SelectOption(SampleRateComboBox, _sampleRates, device.Samplerate);
        SelectOption(EncodingComboBox, _encodings, device.Encoding);

        IpAutoRadioButton.IsChecked = !device.UsesStaticIp;
        IpStaticRadioButton.IsChecked = device.UsesStaticIp;
        IpAddressTextBox.Text = device.StaticIpAddress;
        IpNetmaskTextBox.Text = string.IsNullOrWhiteSpace(device.StaticIpNetmask) ? "255.255.255.0" : device.StaticIpNetmask;
        IpGatewayTextBox.Text = string.IsNullOrWhiteSpace(device.StaticIpGateway) ? "0.0.0.0" : device.StaticIpGateway;
        ConfigureCapabilityControls();
        UpdateIpStaticFieldsState();

        _txChannels = new ObservableCollection<DeviceChannelEditItem>(
            device.TxChannels.Select(channel => new DeviceChannelEditItem(channel.Index, channel.DisplayName)));
        _rxChannels = new ObservableCollection<DeviceChannelEditItem>(
            device.RxChannels.Select(channel => new DeviceChannelEditItem(channel.Index, channel.DisplayName)));
        TxChannelsGrid.ItemsSource = _txChannels;
        RxChannelsGrid.ItemsSource = _rxChannels;

        TranslateDependencyObject(this, []);
        PatchTab.Header = L("Patch RX", "Rx patch");
        PatchDescriptionTextBlock.Text = L(
            "Affectez des canaux TX disponibles aux entrées RX de cette machine.",
            "Assign available Tx channels to this device's Rx inputs.");
        OpenPatchWorkspaceButton.Content = L("Ouvrir Easy patch", "Open Easy patch");
        PatchSafetyTextBlock.Text = L(
            "Les changements de patch seront appliqués avec les autres réglages de cette fenêtre.",
            "Patch changes will be applied with the other settings in this window.");
        OpenPatchWorkspaceButton.IsEnabled = device.RxCount > 0 && _project.Devices.Any(candidate => candidate.TxCount > 0);
        UpdatePatchSummary();

        _updatingDeviceSelector = true;
        DeviceSelectorComboBox.ItemsSource = _project.Devices.Select(candidate => candidate.Name).ToArray();
        DeviceSelectorComboBox.SelectedItem = device.Name;
        _updatingDeviceSelector = false;
        DeviceSelectorLabel.Content = L("Machine", "Device");
        _initialState = BuildResult();
    }

    public string OriginalDeviceName { get; }

    public DeviceDetailsResult? Result { get; private set; }

    public string? RequestedDeviceName { get; private set; }

    private void ApplyButton_Click(object sender, RoutedEventArgs e)
    {
        CommitChannelEdits();
        RequestedDeviceName = null;
        Result = BuildResult();
        DialogResult = true;
    }

    private DeviceDetailsResult BuildResult()
    {
        return new DeviceDetailsResult(
            DeviceNameTextBox.Text.Trim(),
            RedundantRadioButton.IsChecked == true,
            SelectedValue(LatencyComboBox),
            SelectedValue(SampleRateComboBox),
            SelectedValue(EncodingComboBox),
            PreferredMasterCheckBox.IsChecked == true,
            IpStaticRadioButton.IsChecked == true,
            IpAddressTextBox.Text.Trim(),
            IpNetmaskTextBox.Text.Trim(),
            IpGatewayTextBox.Text.Trim(),
            _txChannels.Select(channel => new DeviceChannelEdit(channel.Index, channel.Name.Trim())).ToArray(),
            _rxChannels.Select(channel => new DeviceChannelEdit(channel.Index, channel.Name.Trim())).ToArray(),
            _patchEdits.ToArray());
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        RequestedDeviceName = null;
        DialogResult = false;
    }

    private void DeviceSelectorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_updatingDeviceSelector
            || DeviceSelectorComboBox.SelectedItem is not string requestedName
            || string.Equals(requestedName, OriginalDeviceName, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        CommitChannelEdits();
        MessageBoxResult choice = MessageBoxResult.No;
        if (HasPendingChanges())
        {
            choice = MessageBox.Show(
                this,
                L(
                    "Cette machine contient des changements non appliqués. Oui = appliquer puis changer de machine, Non = abandonner ces changements, Annuler = rester sur cette machine.",
                    "This device has unapplied changes. Yes = apply and switch device, No = discard these changes, Cancel = stay on this device."),
                L("Changer de machine", "Switch device"),
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);
            if (choice == MessageBoxResult.Cancel)
            {
                _updatingDeviceSelector = true;
                DeviceSelectorComboBox.SelectedItem = OriginalDeviceName;
                _updatingDeviceSelector = false;
                return;
            }
        }

        RequestedDeviceName = requestedName;
        Result = choice == MessageBoxResult.Yes ? BuildResult() : null;
        DialogResult = choice == MessageBoxResult.Yes;
    }

    private bool HasPendingChanges()
    {
        DeviceDetailsResult current = BuildResult();
        return !string.Equals(current.DeviceName, _initialState.DeviceName, StringComparison.Ordinal)
            || current.IsRedundant != _initialState.IsRedundant
            || !string.Equals(current.Latency, _initialState.Latency, StringComparison.Ordinal)
            || !string.Equals(current.Samplerate, _initialState.Samplerate, StringComparison.Ordinal)
            || !string.Equals(current.Encoding, _initialState.Encoding, StringComparison.Ordinal)
            || current.PreferredMaster != _initialState.PreferredMaster
            || current.UsesStaticIp != _initialState.UsesStaticIp
            || !string.Equals(current.StaticIpAddress, _initialState.StaticIpAddress, StringComparison.Ordinal)
            || !string.Equals(current.StaticIpNetmask, _initialState.StaticIpNetmask, StringComparison.Ordinal)
            || !string.Equals(current.StaticIpGateway, _initialState.StaticIpGateway, StringComparison.Ordinal)
            || !current.TxChannels.SequenceEqual(_initialState.TxChannels)
            || !current.RxChannels.SequenceEqual(_initialState.RxChannels)
            || !current.PatchEdits.SequenceEqual(_initialState.PatchEdits);
    }

    private void CommitChannelEdits()
    {
        TxChannelsGrid.CommitEdit(DataGridEditingUnit.Cell, true);
        TxChannelsGrid.CommitEdit(DataGridEditingUnit.Row, true);
        RxChannelsGrid.CommitEdit(DataGridEditingUnit.Cell, true);
        RxChannelsGrid.CommitEdit(DataGridEditingUnit.Row, true);
    }

    private void OpenPatchWorkspaceButton_Click(object sender, RoutedEventArgs e)
    {
        string? initialTxDeviceName = _project.FindDevice(OriginalDeviceName)?.TxCount > 0
            ? OriginalDeviceName
            : _project.Devices.FirstOrDefault(device => device.TxCount > 0)?.Name;
        PatchWorkspaceWindow window = new(
            _language,
            _project,
            _useLightTheme,
            initialTxDeviceName,
            OriginalDeviceName,
            _patchEdits,
            returnEditsOnly: true,
            lockRxDeviceSelection: true)
        {
            Owner = this
        };

        if (window.ShowDialog() != true)
        {
            return;
        }

        _patchEdits.Clear();
        _patchEdits.AddRange(window.Edits);
        UpdatePatchSummary();
    }

    private void UpdatePatchSummary()
    {
        PatchSummaryTextBlock.Text = _patchEdits.Count == 0
            ? L("Aucun changement de patch en attente.", "No pending patch change.")
            : L(
                $"{_patchEdits.Count} changement(s) de patch en attente pour cette validation.",
                $"{_patchEdits.Count} patch change(s) pending for this confirmation.");
    }

    private void IpModeRadioButton_Checked(object sender, RoutedEventArgs e)
    {
        UpdateIpStaticFieldsState();
    }

    private void UpdateIpStaticFieldsState()
    {
        bool useStaticIp = _device.SupportsIpConfiguration
            && IpStaticRadioButton.IsChecked == true;
        IpAddressTextBox.IsEnabled = useStaticIp;
        IpNetmaskTextBox.IsEnabled = useStaticIp;
        IpGatewayTextBox.IsEnabled = useStaticIp;
    }

    private void ConfigureCapabilityControls()
    {
        SetCapabilityState(
            [RedundantRadioButton, DaisychainRadioButton],
            _device.SupportsNetworkMode,
            "le mode redondant",
            "redundancy mode",
            "redundancy");
        SetCapabilityState(
            [PreferredMasterCheckBox],
            _device.SupportsPreferredMaster,
            "Preferred Master",
            "Preferred Master",
            "preferred_master");
        SetCapabilityState(
            [LatencyComboBox],
            _device.SupportsLatency,
            "la latence unicast",
            "unicast latency",
            "unicast_latency");
        SetCapabilityState(
            [SampleRateComboBox],
            _device.SupportsSampleRate,
            "la fréquence d'échantillonnage",
            "sample rate",
            "samplerate");
        SetCapabilityState(
            [EncodingComboBox],
            _device.SupportsEncoding,
            "l'encodage",
            "encoding",
            "encoding");
        SetCapabilityState(
            [IpAutoRadioButton, IpStaticRadioButton],
            _device.SupportsIpConfiguration,
            "la configuration IPv4",
            "IPv4 configuration",
            "ipv4_address");

        List<string> unavailable = [];
        if (!_device.SupportsNetworkMode)
        {
            unavailable.Add(L("mode redondant", "redundancy mode"));
        }
        if (!_device.SupportsPreferredMaster)
        {
            unavailable.Add("Preferred Master");
        }
        if (!_device.SupportsLatency)
        {
            unavailable.Add(L("latence unicast", "unicast latency"));
        }
        if (!_device.SupportsSampleRate)
        {
            unavailable.Add(L("fréquence d'échantillonnage", "sample rate"));
        }
        if (!_device.SupportsEncoding)
        {
            unavailable.Add(L("encodage", "encoding"));
        }
        if (!_device.SupportsIpConfiguration)
        {
            unavailable.Add(L("configuration IPv4", "IPv4 configuration"));
        }

        CapabilityNoticeBorder.Visibility = unavailable.Count == 0
            ? Visibility.Collapsed
            : Visibility.Visible;
        CapabilityNoticeTextBlock.Text = unavailable.Count == 0
            ? string.Empty
            : L(
                $"Non pris en charge par ce rôle Dante : {string.Join(", ", unavailable)}. Ces champs sont désactivés et DCE ne créera aucune balise technique absente.",
                $"Unavailable for this Dante role: {string.Join(", ", unavailable)}. These fields are disabled and DCE will not create missing technical tags.");
    }

    private void SetCapabilityState(
        IEnumerable<Control> controls,
        bool supported,
        string frenchSetting,
        string englishSetting,
        string xmlElement)
    {
        string help = supported
            ? L(
                $"{frenchSetting} est disponible pour {_device.Name}.",
                $"{englishSetting} is available for {_device.Name}.")
            : L(
                $"Indisponible pour {_device.Name} : ce rôle Dante n'expose pas la balise <{xmlElement}>. DCE ne la créera pas.",
                $"Unavailable for {_device.Name}: this Dante role does not expose <{xmlElement}>. DCE will not create it.");

        foreach (Control control in controls)
        {
            control.IsEnabled = supported;
            control.ToolTip = help;
            ToolTipService.SetShowOnDisabled(control, true);
        }
    }

    private static void SelectOption(ComboBox comboBox, IEnumerable<DeviceOption> options, string value)
    {
        comboBox.SelectedItem = options.FirstOrDefault(option => string.Equals(option.Value, value, StringComparison.OrdinalIgnoreCase));
    }

    private static string SelectedValue(ComboBox comboBox)
    {
        return comboBox.SelectedItem is DeviceOption option ? option.Value : comboBox.Text.Trim();
    }

    private string LocalizeLiteral(string value)
    {
        return LocalizationService.TranslateLiteral(_language, value);
    }

    private string L(string french, string english)
    {
        return _language == UiLanguage.English ? english : french;
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

        foreach (object child in LogicalTreeHelper.GetChildren(dependencyObject))
        {
            if (child is DependencyObject childObject)
            {
                TranslateDependencyObject(childObject, visited);
            }
        }
    }

    private sealed record DeviceOption(string Value, string Display)
    {
        public override string ToString()
        {
            return Display;
        }
    }
}

public sealed class DeviceChannelEditItem
{
    public DeviceChannelEditItem(int index, string name)
    {
        Index = index;
        Name = name;
    }

    public int Index { get; }

    public string Name { get; set; }
}

public sealed record DeviceChannelEdit(int Index, string Name);

public sealed record DeviceDetailsResult(
    string DeviceName,
    bool IsRedundant,
    string Latency,
    string Samplerate,
    string Encoding,
    bool PreferredMaster,
    bool UsesStaticIp,
    string StaticIpAddress,
    string StaticIpNetmask,
    string StaticIpGateway,
    IReadOnlyList<DeviceChannelEdit> TxChannels,
    IReadOnlyList<DeviceChannelEdit> RxChannels,
    IReadOnlyList<PatchEditRequest> PatchEdits);
