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

    public static Task<MachineInstanceBatchRequest?> ShowAsync(
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
        dialog.FindControl<Grid>("QuantityPanel")!.IsVisible = !createProjectMode;
        dialog.UpdatePrefixState();
        dialog.UpdateNamesPreview();
        return dialog.ShowDialog<MachineInstanceBatchRequest?>(owner);
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
        FindControl<TextBlock>("QuantityLabel")!.Text = L("Nombre de machines", "Number of devices");
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

        ToolTip.SetTip(
            FindControl<TextBox>("NameTextBox")!,
            L(
                "Nom de la première machine. Pour un lot, les suivantes utilisent -2, -3, etc.",
                "Name of the first device. In a batch, subsequent devices use -2, -3, and so on."));
        ToolTip.SetTip(
            FindControl<TextBox>("QuantityTextBox")!,
            L(
                $"Nombre de machines indépendantes à ajouter, de 1 à {MachineInstanceBatchRequest.MaximumQuantity}.",
                $"Number of independent devices to add, from 1 to {MachineInstanceBatchRequest.MaximumQuantity}."));
        ToolTip.SetTip(
            FindControl<CheckBox>("UseRxLabelsCheckBox")!,
            L(
                "Conserve les labels RX proposés par le modèle de banque.",
                "Keeps the Rx labels supplied by the bank template."));
        ToolTip.SetTip(
            FindControl<TextBox>("RxPrefixTextBox")!,
            L(
                "Préfixe utilisé pour générer RX 1, RX 2, etc. lorsque les labels du modèle sont désactivés.",
                "Prefix used to generate Rx 1, Rx 2, and so on when template labels are disabled."));
        ToolTip.SetTip(
            FindControl<CheckBox>("UseTxLabelsCheckBox")!,
            L(
                "Conserve les labels TX proposés par le modèle de banque.",
                "Keeps the Tx labels supplied by the bank template."));
        ToolTip.SetTip(
            FindControl<TextBox>("TxPrefixTextBox")!,
            L(
                "Préfixe utilisé pour générer TX 1, TX 2, etc. lorsque les labels du modèle sont désactivés.",
                "Prefix used to generate Tx 1, Tx 2, and so on when template labels are disabled."));
        ToolTip.SetTip(
            FindControl<Button>("ConfirmButton")!,
            L(
                "Ajoute une instance indépendante avec de nouveaux identifiants techniques.",
                "Adds an independent instance with new technical identifiers."));
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

    private void InstancePreviewChanged(object? sender, TextChangedEventArgs e)
    {
        UpdateNamesPreview();
    }

    private void UpdateNamesPreview()
    {
        TextBlock? preview = FindControl<TextBlock>("NamesPreviewText");
        TextBox? quantityBox = FindControl<TextBox>("QuantityTextBox");
        TextBox? nameBox = FindControl<TextBox>("NameTextBox");
        if (preview is null || quantityBox is null || nameBox is null)
        {
            return;
        }

        string name = nameBox.Text?.Trim() ?? string.Empty;
        if (!int.TryParse(quantityBox.Text?.Trim(), out int quantity)
            || quantity is < 1 or > MachineInstanceBatchRequest.MaximumQuantity
            || DanteNameRules.ValidateDeviceName(name) is not null)
        {
            preview.Text = L("Saisissez un nombre valide.", "Enter a valid number.");
            return;
        }

        try
        {
            IReadOnlyList<string> names = MachineInstanceNameService.BuildNames(name, quantity, []);
            preview.Text = quantity == 1
                ? names[0]
                : L("Aperçu : ", "Preview: ") + string.Join(", ", names.Take(4))
                    + (quantity > 4 ? ", …" : string.Empty);
        }
        catch
        {
            preview.Text = L("Nom de série invalide.", "Invalid series name.");
        }
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

        if (!int.TryParse(FindControl<TextBox>("QuantityTextBox")!.Text?.Trim(), out int quantity)
            || quantity is < 1 or > MachineInstanceBatchRequest.MaximumQuantity)
        {
            await ShowValidationAsync(L(
                $"Le nombre de machines doit être compris entre 1 et {MachineInstanceBatchRequest.MaximumQuantity}.",
                $"The number of devices must be between 1 and {MachineInstanceBatchRequest.MaximumQuantity}."));
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

        Close(new MachineInstanceBatchRequest
        {
            Quantity = quantity,
            Options = new MachineInstanceOptions
            {
                NewName = name,
                UseTemplateTxLabels = useTxLabels,
                UseTemplateRxLabels = useRxLabels,
                TxLabelPrefix = useTxLabels ? null : txPrefix,
                RxLabelPrefix = useRxLabels ? null : rxPrefix
            }
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
