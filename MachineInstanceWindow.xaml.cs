using System.Windows;
using DanteConfigEditor.Models;
using DanteConfigEditor.Services;

namespace DanteConfigEditor;

public partial class MachineInstanceWindow : Window
{
    private readonly UiLanguage _language;

    public MachineInstanceWindow(
        UiLanguage language,
        bool useLightTheme,
        MachineTemplateMetadata metadata,
        string suggestedName,
        bool createProjectMode = false)
    {
        InitializeComponent();
        _language = language;
        DialogThemeService.Apply(this, useLightTheme);
        NameTextBox.Text = suggestedName;
        ApplyLanguage(metadata, createProjectMode);
        UpdatePrefixState();
        NameTextBox.SelectAll();
        NameTextBox.Focus();
    }

    public MachineInstanceOptions? Options { get; private set; }

    private void ApplyLanguage(
        MachineTemplateMetadata metadata,
        bool createProjectMode)
    {
        Title = createProjectMode
            ? L("Première machine du projet", "First project device")
            : L("Ajouter depuis la banque", "Add from device bank");
        HeadingTextBlock.Text = Title;
        TemplateSummaryTextBlock.Text = string.Join(
            " · ",
            new[]
            {
                metadata.TemplateName,
                $"{metadata.TxCount} TX / {metadata.RxCount} RX",
                metadata.SourcePresetVersion
            }.Where(value => !string.IsNullOrWhiteSpace(value)));
        NameLabel.Content = L("Nom de la nouvelle machine", "New device name");
        NameHintTextBlock.Text = L(
            "31 caractères maximum : lettres, chiffres et tirets.",
            "Maximum 31 characters: letters, digits and hyphens.");
        TxGroupBox.Header = L("Canaux TX", "Tx channels");
        RxGroupBox.Header = L("Canaux RX", "Rx channels");
        UseTxLabelsCheckBox.Content = L(
            "Utiliser les labels du modèle",
            "Use template labels");
        UseRxLabelsCheckBox.Content = UseTxLabelsCheckBox.Content;
        TxPrefixLabel.Content = L("Préfixe numéroté", "Numbered prefix");
        RxPrefixLabel.Content = TxPrefixLabel.Content;
        SafetyTextBlock.Text = L(
            "Une nouvelle instance indépendante sera créée. Aucun identifiant matériel, réseau ou abonnement du projet source ne sera réutilisé.",
            "A new independent instance will be created. No hardware identity, network setting or subscription from the source project will be reused.");
        ConfirmButton.Content = createProjectMode
            ? L("Continuer", "Continue")
            : L("Ajouter au projet", "Add to project");
        CancelButton.Content = L("Annuler", "Cancel");

        NameTextBox.ToolTip = L(
            "Nom unique de la machine qui sera créée dans le projet.",
            "Unique name of the device that will be created in the project.");
        UseTxLabelsCheckBox.ToolTip = L(
            "Coché : conserve les labels TX du modèle. Décoché : génère des labels numérotés avec le préfixe saisi.",
            "Checked: keeps the template Tx labels. Cleared: generates numbered labels with the entered prefix.");
        UseRxLabelsCheckBox.ToolTip = L(
            "Coché : conserve les labels RX du modèle. Décoché : génère des labels numérotés avec le préfixe saisi.",
            "Checked: keeps the template Rx labels. Cleared: generates numbered labels with the entered prefix.");
        TxPrefixTextBox.ToolTip = L(
            "Préfixe utilisé pour générer TX 1, TX 2, etc.",
            "Prefix used to generate TX 1, TX 2, and so on.");
        RxPrefixTextBox.ToolTip = L(
            "Préfixe utilisé pour générer RX 1, RX 2, etc.",
            "Prefix used to generate RX 1, RX 2, and so on.");
        ConfirmButton.ToolTip = createProjectMode
            ? L("Valide cette première machine et poursuit la création du projet.", "Confirms this first device and continues project creation.")
            : L("Ajoute une instance indépendante du modèle au projet ouvert.", "Adds an independent instance of the template to the open project.");
        CancelButton.ToolTip = L("Ferme sans ajouter de machine.", "Closes without adding a device.");
    }

    private void LabelModeChanged(object sender, RoutedEventArgs e)
    {
        UpdatePrefixState();
    }

    private void UpdatePrefixState()
    {
        if (TxPrefixTextBox is null || RxPrefixTextBox is null)
        {
            return;
        }

        TxPrefixTextBox.IsEnabled = UseTxLabelsCheckBox.IsChecked != true;
        RxPrefixTextBox.IsEnabled = UseRxLabelsCheckBox.IsChecked != true;
    }

    private void ConfirmButton_Click(object sender, RoutedEventArgs e)
    {
        string name = NameTextBox.Text.Trim();
        string? nameError = DanteNameRules.ValidateDeviceName(name);
        if (nameError is not null)
        {
            ShowError(_language == UiLanguage.English
                ? "The device name is invalid. Use at most 31 letters, digits or hyphens."
                : nameError);
            return;
        }

        if (UseTxLabelsCheckBox.IsChecked != true
            && string.IsNullOrWhiteSpace(TxPrefixTextBox.Text))
        {
            ShowError(L("Le préfixe TX est obligatoire.", "The Tx prefix is required."));
            return;
        }

        if (UseRxLabelsCheckBox.IsChecked != true
            && string.IsNullOrWhiteSpace(RxPrefixTextBox.Text))
        {
            ShowError(L("Le préfixe RX est obligatoire.", "The Rx prefix is required."));
            return;
        }

        Options = new MachineInstanceOptions
        {
            NewName = name,
            UseTemplateTxLabels = UseTxLabelsCheckBox.IsChecked == true,
            UseTemplateRxLabels = UseRxLabelsCheckBox.IsChecked == true,
            TxLabelPrefix = UseTxLabelsCheckBox.IsChecked == true
                ? null
                : TxPrefixTextBox.Text.Trim(),
            RxLabelPrefix = UseRxLabelsCheckBox.IsChecked == true
                ? null
                : RxPrefixTextBox.Text.Trim()
        };
        DialogResult = true;
    }

    private void ShowError(string message)
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
}
