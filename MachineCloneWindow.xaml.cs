using System.Windows;
using DanteConfigEditor.Models;
using DanteConfigEditor.Services;

namespace DanteConfigEditor;

public partial class MachineCloneWindow : Window
{
    private readonly UiLanguage _language;

    public MachineCloneWindow(
        UiLanguage language,
        bool useLightTheme,
        string sourceName,
        string suggestedName)
    {
        InitializeComponent();
        _language = language;
        DialogThemeService.Apply(this, useLightTheme);
        NewNameTextBox.Text = suggestedName;
        ApplyLanguage(sourceName);
        NewNameTextBox.SelectAll();
        NewNameTextBox.Focus();
    }

    public MachineCloneOptions? Options { get; private set; }

    private void ApplyLanguage(string sourceName)
    {
        Title = L("Dupliquer la machine", "Duplicate device");
        HeadingTextBlock.Text = Title;
        IntroTextBlock.Text = L(
            $"Créer un rôle indépendant à partir de « {sourceName} ».",
            $"Create an independent role from “{sourceName}”.");
        NewNameLabel.Content = L("Nouveau nom", "New name");
        NameRulesTextBlock.Text = L(
            "31 caractères maximum : lettres, chiffres et tirets, sans espace.",
            "Maximum 31 characters: letters, digits and hyphens, without spaces.");
        LabelsGroupBox.Header = L("Labels de canaux", "Channel labels");
        PreserveTxLabelsCheckBox.Content = L("Conserver les labels TX", "Keep Tx labels");
        PreserveRxLabelsCheckBox.Content = L("Conserver les labels RX", "Keep Rx labels");
        SettingsGroupBox.Header = L("Paramètres", "Settings");
        PreserveSettingsCheckBox.Content = L(
            "Conserver sample rate, encodage et latence",
            "Keep sample rate, encoding and latency");
        PreservePreferredMasterCheckBox.Content = L(
            "Conserver Preferred Master",
            "Keep Preferred Master");
        AdvancedGroupBox.Header = L("Options avancées", "Advanced options");
        PreserveNetworkCheckBox.Content = L(
            "Conserver les interfaces et adresses réseau",
            "Keep network interfaces and addresses");
        PreserveSubscriptionsCheckBox.Content = L(
            "Conserver les subscriptions RX de cette machine",
            "Keep this device's Rx subscriptions");
        PreserveFlowsCheckBox.Content = L(
            "Conserver les flows multicast TX",
            "Keep Tx multicast flows");
        AdvancedWarningTextBlock.Text = L(
            "Ces options peuvent recopier des adresses ou références propres au projet source. "
            + "La validation bloquera les incohérences détectées.",
            "These options may copy addresses or references specific to the source project. "
            + "Validation will block detected inconsistencies.");
        IdentityNoticeTextBlock.Text = L(
            "L'identité matérielle n'est jamais copiée. La nouvelle machine sera un rôle générique hors ligne.",
            "Hardware identity is never copied. The new device will be a generic offline role.");
        DuplicateButton.Content = L("Dupliquer", "Duplicate");
        CancelButton.Content = L("Annuler", "Cancel");
    }

    private void DuplicateButton_Click(object sender, RoutedEventArgs e)
    {
        string name = NewNameTextBox.Text.Trim();
        string? error = DanteNameRules.ValidateDeviceName(name);
        if (error is not null)
        {
            MessageBox.Show(
                this,
                _language == UiLanguage.English
                    ? TranslateNameError(error)
                    : error,
                L("Nom invalide", "Invalid name"),
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        Options = new MachineCloneOptions
        {
            NewName = name,
            PreserveTxLabels = PreserveTxLabelsCheckBox.IsChecked == true,
            PreserveRxLabels = PreserveRxLabelsCheckBox.IsChecked == true,
            PreserveDeviceSettings = PreserveSettingsCheckBox.IsChecked == true,
            PreserveNetworkConfiguration = PreserveNetworkCheckBox.IsChecked == true,
            PreserveSubscriptions = PreserveSubscriptionsCheckBox.IsChecked == true,
            PreserveMulticastFlows = PreserveFlowsCheckBox.IsChecked == true,
            PreservePreferredMaster = PreservePreferredMasterCheckBox.IsChecked == true
        };
        DialogResult = true;
    }

    private string TranslateNameError(string error)
    {
        if (error.Contains("obligatoire", StringComparison.OrdinalIgnoreCase))
        {
            return "The device name is required.";
        }

        if (error.Contains("31", StringComparison.Ordinal))
        {
            return "The device name must not exceed 31 characters.";
        }

        return "The device name may only contain letters, digits and hyphens, "
            + "and may not start or end with a hyphen.";
    }

    private string L(string french, string english)
    {
        return _language == UiLanguage.English ? english : french;
    }
}
