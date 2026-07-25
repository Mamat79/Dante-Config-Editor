using Avalonia.Controls;
using Avalonia.Interactivity;
using DanteConfigEditor.Models;
using DanteConfigEditor.Services;

namespace DanteConfigEditor.Mac;

internal sealed partial class MachineInstanceDialog : Window
{
    private UiLanguage _language;

    public MachineInstanceDialog()
    {
        InitializeComponent();
    }

    private T? FindControl<T>(string name) where T : Control =>
        ControlExtensions.FindControl<T>(this, name);

    public static Task<MachineInstanceOptions?> ShowAsync(
        Window owner,
        UiLanguage language,
        MachineTemplateMetadata metadata,
        string suggestedName,
        bool createProjectMode = false)
    {
        MachineInstanceDialog dialog = new()
        {
            _language = language,
            Title = createProjectMode
                ? (language == UiLanguage.English ? "First project device" : "Première machine du projet")
                : (language == UiLanguage.English ? "Add from device bank" : "Ajouter depuis la banque")
        };
        dialog.FindControl<TextBox>("NameTextBox")!.Text = suggestedName;
        dialog.ApplyLanguage(metadata, createProjectMode);
        dialog.UpdatePrefixState();
        return dialog.ShowDialog<MachineInstanceOptions?>(owner);
    }

    private void ApplyLanguage(MachineTemplateMetadata metadata, bool createProjectMode)
    {
        FindControl<TextBlock>("HeadingText")!.Text = Title;
        FindControl<TextBlock>("SummaryText")!.Text = string.Join(
            " · ",
            new[]
            {
                metadata.TemplateName,
                $"{metadata.TxCount} TX / {metadata.RxCount} RX",
                metadata.SourcePresetVersion
            }.Where(value => !string.IsNullOrWhiteSpace(value)));
        FindControl<TextBlock>("NameLabel")!.Text = L("Nom de la nouvelle machine", "New device name");
        FindControl<TextBlock>("NameHintText")!.Text = L(
            "31 caractères maximum : lettres, chiffres et tirets.",
            "Maximum 31 characters: letters, digits and hyphens.");
        FindControl<CheckBox>("UseTxLabelsCheckBox")!.Content = L(
            "Utiliser les labels du modèle",
            "Use template labels");
        FindControl<CheckBox>("UseRxLabelsCheckBox")!.Content =
            FindControl<CheckBox>("UseTxLabelsCheckBox")!.Content;
        FindControl<TextBlock>("TxPrefixLabel")!.Text = L("Préfixe numéroté", "Numbered prefix");
        FindControl<TextBlock>("RxPrefixLabel")!.Text = FindControl<TextBlock>("TxPrefixLabel")!.Text;
        FindControl<TextBlock>("SafetyText")!.Text = L(
            "Une nouvelle instance indépendante sera créée. Aucun identifiant matériel, réseau ou abonnement du projet source ne sera réutilisé.",
            "A new independent instance will be created. No hardware identity, network setting or subscription from the source project will be reused.");
        FindControl<Button>("ConfirmButton")!.Content = createProjectMode
            ? L("Continuer", "Continue")
            : L("Ajouter au projet", "Add to project");
        FindControl<Button>("CancelButton")!.Content = L("Annuler", "Cancel");
    }

    private void LabelModeChanged(object? sender, RoutedEventArgs e)
    {
        UpdatePrefixState();
    }

    private void UpdatePrefixState()
    {
        FindControl<TextBox>("TxPrefixTextBox")!.IsEnabled =
            FindControl<CheckBox>("UseTxLabelsCheckBox")!.IsChecked != true;
        FindControl<TextBox>("RxPrefixTextBox")!.IsEnabled =
            FindControl<CheckBox>("UseRxLabelsCheckBox")!.IsChecked != true;
    }

    private async void ConfirmButton_Click(object? sender, RoutedEventArgs e)
    {
        string name = FindControl<TextBox>("NameTextBox")!.Text?.Trim() ?? string.Empty;
        string? nameError = DanteNameRules.ValidateDeviceName(name);
        if (nameError is not null)
        {
            await ShowValidationAsync(_language == UiLanguage.English
                ? "Use at most 31 letters, digits or hyphens."
                : nameError);
            return;
        }

        bool useTxLabels = FindControl<CheckBox>("UseTxLabelsCheckBox")!.IsChecked == true;
        bool useRxLabels = FindControl<CheckBox>("UseRxLabelsCheckBox")!.IsChecked == true;
        string txPrefix = FindControl<TextBox>("TxPrefixTextBox")!.Text?.Trim() ?? string.Empty;
        string rxPrefix = FindControl<TextBox>("RxPrefixTextBox")!.Text?.Trim() ?? string.Empty;
        if (!useTxLabels && string.IsNullOrWhiteSpace(txPrefix))
        {
            await ShowValidationAsync(L("Le préfixe TX est obligatoire.", "The Tx prefix is required."));
            return;
        }

        if (!useRxLabels && string.IsNullOrWhiteSpace(rxPrefix))
        {
            await ShowValidationAsync(L("Le préfixe RX est obligatoire.", "The Rx prefix is required."));
            return;
        }

        Close(new MachineInstanceOptions
        {
            NewName = name,
            UseTemplateTxLabels = useTxLabels,
            UseTemplateRxLabels = useRxLabels,
            TxLabelPrefix = useTxLabels ? null : txPrefix,
            RxLabelPrefix = useRxLabels ? null : rxPrefix
        });
    }

    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        Close(null);
    }

    private Task<bool> ShowValidationAsync(string message) =>
        MessageDialog.ShowInfoAsync(this, L("Valeur invalide", "Invalid value"), message, "OK");

    private string L(string french, string english) =>
        _language == UiLanguage.English ? english : french;
}
