using Avalonia.Controls;
using Avalonia.Interactivity;
using DanteConfigEditor.Models;
using DanteConfigEditor.Services;

namespace DanteConfigEditor.Mac;

internal sealed partial class MachineCloneDialog : Window
{
    private UiLanguage _language;

    public MachineCloneDialog()
    {
        InitializeComponent();
    }

    private T? FindControl<T>(string name) where T : Control =>
        ControlExtensions.FindControl<T>(this, name);

    public static Task<MachineCloneOptions?> ShowAsync(
        Window owner,
        UiLanguage language,
        string sourceName,
        string suggestedName)
    {
        MachineCloneDialog dialog = new()
        {
            _language = language,
            Title = language == UiLanguage.English ? "Duplicate device" : "Dupliquer la machine"
        };
        dialog.FindControl<TextBox>("NameTextBox")!.Text = suggestedName;
        dialog.ApplyLanguage(sourceName);
        return dialog.ShowDialog<MachineCloneOptions?>(owner);
    }

    private void ApplyLanguage(string sourceName)
    {
        FindControl<TextBlock>("HeadingText")!.Text = Title;
        FindControl<TextBlock>("IntroText")!.Text = L(
            $"Créer un rôle indépendant à partir de « {sourceName} ».",
            $"Create an independent role from “{sourceName}”.");
        FindControl<TextBlock>("NameLabel")!.Text = L("Nouveau nom", "New name");
        FindControl<TextBlock>("NameHintText")!.Text = L(
            "31 caractères maximum : lettres, chiffres et tirets, sans espace.",
            "Maximum 31 characters: letters, digits and hyphens, without spaces.");
        FindControl<TextBlock>("LabelsHeading")!.Text = L("Labels de canaux", "Channel labels");
        FindControl<CheckBox>("PreserveTxLabelsCheckBox")!.Content = L(
            "Conserver les labels TX",
            "Keep Tx labels");
        FindControl<CheckBox>("PreserveRxLabelsCheckBox")!.Content = L(
            "Conserver les labels RX",
            "Keep Rx labels");
        FindControl<TextBlock>("SettingsHeading")!.Text = L("Paramètres", "Settings");
        FindControl<CheckBox>("PreserveSettingsCheckBox")!.Content = L(
            "Conserver sample rate, encodage et latence",
            "Keep sample rate, encoding and latency");
        FindControl<CheckBox>("PreservePreferredMasterCheckBox")!.Content = L(
            "Conserver Preferred Master",
            "Keep Preferred Master");
        FindControl<TextBlock>("AdvancedHeading")!.Text = L("Options avancées", "Advanced options");
        FindControl<CheckBox>("PreserveNetworkCheckBox")!.Content = L(
            "Conserver les interfaces et adresses réseau",
            "Keep network interfaces and addresses");
        FindControl<CheckBox>("PreserveSubscriptionsCheckBox")!.Content = L(
            "Conserver les subscriptions RX de cette machine",
            "Keep this device's Rx subscriptions");
        FindControl<CheckBox>("PreserveFlowsCheckBox")!.Content = L(
            "Conserver les flows multicast TX",
            "Keep Tx multicast flows");
        FindControl<TextBlock>("AdvancedWarningText")!.Text = L(
            "Ces options peuvent recopier des adresses ou références propres au projet source. "
            + "La validation bloquera les incohérences détectées.",
            "These options may copy addresses or references specific to the source project. "
            + "Validation will block detected inconsistencies.");
        FindControl<TextBlock>("IdentityNoticeText")!.Text = L(
            "L'identité matérielle n'est jamais copiée. La nouvelle machine sera un rôle générique hors ligne.",
            "Hardware identity is never copied. The new device will be a generic offline role.");
        FindControl<Button>("DuplicateButton")!.Content = L("Dupliquer", "Duplicate");
        FindControl<Button>("CancelButton")!.Content = L("Annuler", "Cancel");

        ToolTip.SetTip(
            FindControl<TextBox>("NameTextBox")!,
            L(
                "Nom unique de la nouvelle machine dans ce projet.",
                "Unique name of the new device in this project."));
        ToolTip.SetTip(
            FindControl<CheckBox>("PreserveRxLabelsCheckBox")!,
            L(
                "Recopie les noms des canaux RX de la machine source.",
                "Copies the Rx channel names from the source device."));
        ToolTip.SetTip(
            FindControl<CheckBox>("PreserveTxLabelsCheckBox")!,
            L(
                "Recopie les noms des canaux TX de la machine source.",
                "Copies the Tx channel names from the source device."));
        ToolTip.SetTip(
            FindControl<CheckBox>("PreserveSettingsCheckBox")!,
            L(
                "Recopie les paramètres audio généraux, sans recopier l'identité matérielle.",
                "Copies general audio settings without copying hardware identity."));
        ToolTip.SetTip(
            FindControl<CheckBox>("PreservePreferredMasterCheckBox")!,
            L(
                "À utiliser avec prudence : plusieurs Preferred Masters peuvent créer une alerte.",
                "Use with care: multiple Preferred Masters may trigger a warning."));
        ToolTip.SetTip(
            FindControl<CheckBox>("PreserveNetworkCheckBox")!,
            L(
                "Option avancée : recopie la structure réseau. Les identifiants et adresses à risque restent contrôlés.",
                "Advanced option: copies the network structure. Risky identities and addresses remain validated."));
        ToolTip.SetTip(
            FindControl<CheckBox>("PreserveSubscriptionsCheckBox")!,
            L(
                "Recopie uniquement les subscriptions RX qui restent cohérentes dans le projet.",
                "Copies only Rx subscriptions that remain consistent in the project."));
        ToolTip.SetTip(
            FindControl<CheckBox>("PreserveFlowsCheckBox")!,
            L(
                "Recopie les flows multicast TX lorsque leurs références restent valides.",
                "Copies Tx multicast flows when their references remain valid."));
        ToolTip.SetTip(
            FindControl<Button>("DuplicateButton")!,
            L(
                "Crée une nouvelle machine indépendante et génère de nouveaux identifiants.",
                "Creates an independent device and generates new identifiers."));
    }

    private async void DuplicateButton_Click(object? sender, RoutedEventArgs e)
    {
        string name = FindControl<TextBox>("NameTextBox")!.Text?.Trim() ?? string.Empty;
        string? error = DanteNameRules.ValidateDeviceName(name);
        if (error is not null)
        {
            await MessageDialog.ShowInfoAsync(
                this,
                L("Nom invalide", "Invalid name"),
                _language == UiLanguage.English
                    ? "Use at most 31 letters, digits or hyphens, without spaces."
                    : error,
                "OK");
            return;
        }

        Close(new MachineCloneOptions
        {
            NewName = name,
            PreserveTxLabels = FindControl<CheckBox>("PreserveTxLabelsCheckBox")!.IsChecked == true,
            PreserveRxLabels = FindControl<CheckBox>("PreserveRxLabelsCheckBox")!.IsChecked == true,
            PreserveDeviceSettings = FindControl<CheckBox>("PreserveSettingsCheckBox")!.IsChecked == true,
            PreserveNetworkConfiguration = FindControl<CheckBox>("PreserveNetworkCheckBox")!.IsChecked == true,
            PreserveSubscriptions = FindControl<CheckBox>("PreserveSubscriptionsCheckBox")!.IsChecked == true,
            PreserveMulticastFlows = FindControl<CheckBox>("PreserveFlowsCheckBox")!.IsChecked == true,
            PreservePreferredMaster = FindControl<CheckBox>("PreservePreferredMasterCheckBox")!.IsChecked == true
        });
    }

    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        Close(null);
    }

    private string L(string french, string english) =>
        _language == UiLanguage.English ? english : french;
}
