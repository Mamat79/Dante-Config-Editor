using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using DanteConfigEditor.Services;

namespace DanteConfigEditor.Mac;

internal sealed record MacMachineTemplateFormResult(
    string TemplateName,
    string Manufacturer,
    string Model,
    string Description,
    string Category,
    IReadOnlyList<string> Tags,
    IReadOnlyList<string> TxLabels,
    IReadOnlyList<string> RxLabels,
    string? ImageSourcePath,
    bool RemoveImage);

internal sealed partial class MachineTemplateEditorDialog : Window
{
    private static readonly FilePickerFileType ImageFileType = new("Images")
    {
        Patterns = ["*.png", "*.jpg", "*.jpeg", "*.webp"],
        MimeTypes = ["image/png", "image/jpeg", "image/webp"]
    };

    private UiLanguage _language;

    public MachineTemplateEditorDialog()
    {
        InitializeComponent();
    }

    private T? FindControl<T>(string name) where T : Control =>
        ControlExtensions.FindControl<T>(this, name);

    public ObservableCollection<MacMachineTemplateLabelRow> TxLabels { get; } = [];

    public ObservableCollection<MacMachineTemplateLabelRow> RxLabels { get; } = [];

    public static Task<MacMachineTemplateFormResult?> ShowAsync(
        Window owner,
        UiLanguage language,
        string title,
        string intro,
        string templateName,
        string manufacturer,
        string model,
        string description,
        string category,
        IEnumerable<string> tags,
        IEnumerable<string> txLabels,
        IEnumerable<string> rxLabels,
        string? imagePath = null,
        bool allowImageRemoval = false)
    {
        MachineTemplateEditorDialog dialog = new()
        {
            _language = language,
            Title = title
        };
        dialog.FindControl<TextBlock>("HeadingText")!.Text = title;
        dialog.FindControl<TextBlock>("IntroText")!.Text = intro;
        dialog.FindControl<TextBox>("TemplateNameTextBox")!.Text = templateName;
        dialog.FindControl<TextBox>("ManufacturerTextBox")!.Text = manufacturer;
        dialog.FindControl<TextBox>("ModelTextBox")!.Text = model;
        dialog.FindControl<TextBox>("DescriptionTextBox")!.Text = description;
        dialog.FindControl<TextBox>("CategoryTextBox")!.Text = category;
        dialog.FindControl<TextBox>("TagsTextBox")!.Text = string.Join(", ", tags);
        dialog.FillRows(dialog.TxLabels, txLabels);
        dialog.FillRows(dialog.RxLabels, rxLabels);
        dialog.FindControl<DataGrid>("TxLabelsGrid")!.ItemsSource = dialog.TxLabels;
        dialog.FindControl<DataGrid>("RxLabelsGrid")!.ItemsSource = dialog.RxLabels;
        dialog.FindControl<TextBox>("ImagePathTextBox")!.Text = imagePath ?? string.Empty;
        dialog.FindControl<CheckBox>("RemoveImageCheckBox")!.IsVisible = allowImageRemoval;
        dialog.ApplyLanguage();
        dialog.LoadImagePreview(imagePath);
        return dialog.ShowDialog<MacMachineTemplateFormResult?>(owner);
    }

    private void ApplyLanguage()
    {
        FindControl<TextBlock>("TemplateNameLabel")!.Text = L("Nom du modèle", "Template name");
        FindControl<TextBlock>("ManufacturerLabel")!.Text = L("Fabricant", "Manufacturer");
        FindControl<TextBlock>("ModelLabel")!.Text = L("Modèle", "Model");
        FindControl<TextBlock>("CategoryLabel")!.Text = L("Catégorie", "Category");
        FindControl<TextBlock>("TagsHintText")!.Text = L(
            "Séparez les tags par une virgule.",
            "Separate tags with commas.");
        FindControl<TextBlock>("ImageLabel")!.Text = L("Image facultative", "Optional image");
        FindControl<Button>("BrowseImageButton")!.Content = L("Choisir", "Browse");
        FindControl<TextBlock>("NoImageText")!.Text = L("Aucune image", "No image");
        FindControl<CheckBox>("RemoveImageCheckBox")!.Content = L(
            "Retirer l'image existante",
            "Remove existing image");
        FindControl<TextBlock>("TxHeading")!.Text = L("Labels TX proposés", "Default Tx labels");
        FindControl<TextBlock>("RxHeading")!.Text = L("Labels RX proposés", "Default Rx labels");
        FindControl<DataGrid>("TxLabelsGrid")!.Columns[1].Header = L("Label TX", "Tx label");
        FindControl<DataGrid>("RxLabelsGrid")!.Columns[1].Header = L("Label RX", "Rx label");
        FindControl<Button>("GenerateTxButton")!.Content = L("Générer TX 1, TX 2...", "Generate TX 1, TX 2...");
        FindControl<Button>("GenerateRxButton")!.Content = L("Générer RX 1, RX 2...", "Generate RX 1, RX 2...");
        FindControl<TextBlock>("DescriptionLabel")!.Text = L("Description", "Description");
        FindControl<TextBlock>("LabelsHintText")!.Text = L(
            "Les labels restent modifiables avant chaque insertion. Les identités matérielles, adresses, patchs et flows ne sont jamais stockés dans le modèle.",
            "Labels remain editable before each insertion. Hardware identities, addresses, subscriptions, and flows are never stored in the template.");
        FindControl<Button>("SaveButton")!.Content = L("Enregistrer", "Save");
        FindControl<Button>("CancelButton")!.Content = L("Annuler", "Cancel");
    }

    private async void BrowseImageButton_Click(object? sender, RoutedEventArgs e)
    {
        IReadOnlyList<IStorageFile> files = await StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = L("Choisir l'image du modèle", "Choose template image"),
                AllowMultiple = false,
                FileTypeFilter = [ImageFileType]
            });
        string? path = files.FirstOrDefault()?.TryGetLocalPath();
        if (path is null)
        {
            return;
        }

        FindControl<TextBox>("ImagePathTextBox")!.Text = path;
        FindControl<CheckBox>("RemoveImageCheckBox")!.IsChecked = false;
        LoadImagePreview(path);
    }

    private void GenerateTxButton_Click(object? sender, RoutedEventArgs e)
    {
        GenerateLabels(TxLabels, "TX");
        FindControl<DataGrid>("TxLabelsGrid")!.ItemsSource = null;
        FindControl<DataGrid>("TxLabelsGrid")!.ItemsSource = TxLabels;
    }

    private void GenerateRxButton_Click(object? sender, RoutedEventArgs e)
    {
        GenerateLabels(RxLabels, "RX");
        FindControl<DataGrid>("RxLabelsGrid")!.ItemsSource = null;
        FindControl<DataGrid>("RxLabelsGrid")!.ItemsSource = RxLabels;
    }

    private async void SaveButton_Click(object? sender, RoutedEventArgs e)
    {
        string templateName = FindControl<TextBox>("TemplateNameTextBox")!.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(templateName))
        {
            await ShowValidationAsync(L("Le nom du modèle est obligatoire.", "The template name is required."));
            return;
        }

        string? invalidTx = FirstInvalidLabel(TxLabels);
        if (invalidTx is not null)
        {
            await ShowValidationAsync(L($"Label TX invalide : {invalidTx}", $"Invalid Tx label: {invalidTx}"));
            return;
        }

        string? invalidRx = FirstInvalidLabel(RxLabels);
        if (invalidRx is not null)
        {
            await ShowValidationAsync(L($"Label RX invalide : {invalidRx}", $"Invalid Rx label: {invalidRx}"));
            return;
        }

        string imagePath = FindControl<TextBox>("ImagePathTextBox")!.Text?.Trim() ?? string.Empty;
        Close(new MacMachineTemplateFormResult(
            templateName,
            FindControl<TextBox>("ManufacturerTextBox")!.Text?.Trim() ?? string.Empty,
            FindControl<TextBox>("ModelTextBox")!.Text?.Trim() ?? string.Empty,
            FindControl<TextBox>("DescriptionTextBox")!.Text?.Trim() ?? string.Empty,
            FindControl<TextBox>("CategoryTextBox")!.Text?.Trim() ?? string.Empty,
            (FindControl<TextBox>("TagsTextBox")!.Text ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            TxLabels.Select(row => row.Label.Trim()).ToArray(),
            RxLabels.Select(row => row.Label.Trim()).ToArray(),
            string.IsNullOrWhiteSpace(imagePath) ? null : Path.GetFullPath(imagePath),
            FindControl<CheckBox>("RemoveImageCheckBox")!.IsChecked == true));
    }

    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        Close(null);
    }

    private void FillRows(
        ObservableCollection<MacMachineTemplateLabelRow> rows,
        IEnumerable<string> labels)
    {
        foreach ((string label, int index) in labels.Select((label, index) => (label, index)))
        {
            rows.Add(new MacMachineTemplateLabelRow(index + 1, label));
        }
    }

    private static void GenerateLabels(
        IEnumerable<MacMachineTemplateLabelRow> rows,
        string prefix)
    {
        foreach (MacMachineTemplateLabelRow row in rows)
        {
            row.Label = $"{prefix} {row.Number}";
        }
    }

    private static string? FirstInvalidLabel(
        IEnumerable<MacMachineTemplateLabelRow> rows)
    {
        foreach (MacMachineTemplateLabelRow row in rows)
        {
            string? error = DanteNameRules.ValidateChannelName(row.Label);
            if (error is not null)
            {
                return $"#{row.Number} « {row.Label} » - {error}";
            }
        }

        return null;
    }

    private void LoadImagePreview(string? path)
    {
        Image preview = FindControl<Image>("ImagePreview")!;
        TextBlock noImage = FindControl<TextBlock>("NoImageText")!;
        preview.Source = null;
        noImage.IsVisible = true;
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return;
        }

        try
        {
            using FileStream stream = File.OpenRead(path);
            preview.Source = new Bitmap(stream);
            noImage.IsVisible = false;
        }
        catch
        {
            noImage.Text = L("Image illisible", "Unreadable image");
        }
    }

    private Task<bool> ShowValidationAsync(string message) =>
        MessageDialog.ShowInfoAsync(this, L("Valeur invalide", "Invalid value"), message, "OK");

    private string L(string french, string english) =>
        _language == UiLanguage.English ? english : french;
}

internal sealed class MacMachineTemplateLabelRow
{
    public MacMachineTemplateLabelRow(int number, string label)
    {
        Number = number;
        Label = label;
    }

    public int Number { get; }

    public string Label { get; set; }
}
