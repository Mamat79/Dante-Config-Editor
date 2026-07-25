using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using DanteConfigEditor.Services;
using Microsoft.Win32;

namespace DanteConfigEditor;

public sealed record MachineTemplateFormResult(
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

public partial class MachineTemplateEditorWindow : Window
{
    private readonly UiLanguage _language;

    public MachineTemplateEditorWindow(
        UiLanguage language,
        bool useLightTheme,
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
        InitializeComponent();
        _language = language;
        DialogThemeService.Apply(this, useLightTheme);
        Title = title;
        HeadingTextBlock.Text = title;
        IntroTextBlock.Text = intro;
        TemplateNameTextBox.Text = templateName;
        ManufacturerTextBox.Text = manufacturer;
        ModelTextBox.Text = model;
        DescriptionTextBox.Text = description;
        CategoryTextBox.Text = category;
        TagsTextBox.Text = string.Join(", ", tags);
        TxLabels = BuildRows(txLabels);
        RxLabels = BuildRows(rxLabels);
        TxLabelsGrid.ItemsSource = TxLabels;
        RxLabelsGrid.ItemsSource = RxLabels;
        ImagePathTextBox.Text = imagePath ?? string.Empty;
        RemoveImageCheckBox.Visibility = allowImageRemoval
            ? Visibility.Visible
            : Visibility.Collapsed;
        LoadImagePreview(imagePath);
        ApplyLanguage();
    }

    public ObservableCollection<MachineTemplateLabelRow> TxLabels { get; }

    public ObservableCollection<MachineTemplateLabelRow> RxLabels { get; }

    public MachineTemplateFormResult? Result { get; private set; }

    private void ApplyLanguage()
    {
        TemplateNameLabel.Content = L("Nom du modèle", "Template name");
        ManufacturerLabel.Content = L("Fabricant", "Manufacturer");
        ModelLabel.Content = L("Modèle", "Model");
        CategoryLabel.Content = L("Catégorie", "Category");
        TagsLabel.Content = "Tags";
        TagsHintTextBlock.Text = L(
            "Séparez les tags par une virgule.",
            "Separate tags with commas.");
        DescriptionLabel.Content = L("Description", "Description");
        ImageLabel.Content = L("Image facultative", "Optional image");
        BrowseImageButton.Content = L("Choisir", "Browse");
        RemoveImageCheckBox.Content = L(
            "Retirer l'image existante",
            "Remove existing image");
        LabelsHeadingTextBlock.Text = L("Labels proposés par défaut", "Default labels");
        LabelsIntroTextBlock.Text = L(
            "Ces labels restent modifiables avant chaque insertion dans un projet.",
            "These labels can still be changed before each insertion into a project.");
        TxLabelColumn.Header = L("Label TX", "Tx label");
        RxLabelColumn.Header = L("Label RX", "Rx label");
        GenerateTxButton.Content = L("Générer TX 1, TX 2...", "Generate TX 1, TX 2...");
        GenerateRxButton.Content = L("Générer RX 1, RX 2...", "Generate RX 1, RX 2...");
        SaveButton.Content = L("Enregistrer", "Save");
        CancelButton.Content = L("Annuler", "Cancel");
    }

    private void BrowseImageButton_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog dialog = new()
        {
            Filter = L(
                "Images|*.png;*.jpg;*.jpeg;*.webp",
                "Images|*.png;*.jpg;*.jpeg;*.webp"),
            Title = L("Choisir l'image du modèle", "Choose template image")
        };
        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        ImagePathTextBox.Text = dialog.FileName;
        RemoveImageCheckBox.IsChecked = false;
        LoadImagePreview(dialog.FileName);
    }

    private void GenerateTxButton_Click(object sender, RoutedEventArgs e)
    {
        GenerateLabels(TxLabels, "TX");
        TxLabelsGrid.Items.Refresh();
    }

    private void GenerateRxButton_Click(object sender, RoutedEventArgs e)
    {
        GenerateLabels(RxLabels, "RX");
        RxLabelsGrid.Items.Refresh();
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        TxLabelsGrid.CommitEdit(DataGridEditingUnit.Cell, exitEditingMode: true);
        RxLabelsGrid.CommitEdit(DataGridEditingUnit.Cell, exitEditingMode: true);
        string templateName = TemplateNameTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(templateName))
        {
            ShowValidation(L("Le nom du modèle est obligatoire.", "The template name is required."));
            return;
        }

        string? invalidTx = FirstInvalidLabel(TxLabels);
        if (invalidTx is not null)
        {
            ShowValidation(L(
                $"Label TX invalide : {invalidTx}",
                $"Invalid Tx label: {invalidTx}"));
            return;
        }

        string? invalidRx = FirstInvalidLabel(RxLabels);
        if (invalidRx is not null)
        {
            ShowValidation(L(
                $"Label RX invalide : {invalidRx}",
                $"Invalid Rx label: {invalidRx}"));
            return;
        }

        Result = new MachineTemplateFormResult(
            templateName,
            ManufacturerTextBox.Text.Trim(),
            ModelTextBox.Text.Trim(),
            DescriptionTextBox.Text.Trim(),
            CategoryTextBox.Text.Trim(),
            TagsTextBox.Text
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            TxLabels.Select(row => row.Label.Trim()).ToArray(),
            RxLabels.Select(row => row.Label.Trim()).ToArray(),
            string.IsNullOrWhiteSpace(ImagePathTextBox.Text)
                ? null
                : Path.GetFullPath(ImagePathTextBox.Text),
            RemoveImageCheckBox.IsChecked == true);
        DialogResult = true;
    }

    private void LoadImagePreview(string? path)
    {
        ImagePreview.Source = null;
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return;
        }

        try
        {
            BitmapImage image = new();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.DecodePixelWidth = 420;
            image.UriSource = new Uri(Path.GetFullPath(path), UriKind.Absolute);
            image.EndInit();
            image.Freeze();
            ImagePreview.Source = image;
        }
        catch
        {
            // La validation détaillée du format est effectuée par le dépôt au
            // moment de l'enregistrement. L'aperçu reste simplement vide.
        }
    }

    private static ObservableCollection<MachineTemplateLabelRow> BuildRows(
        IEnumerable<string> labels)
    {
        return new ObservableCollection<MachineTemplateLabelRow>(
            labels.Select((label, index) => new MachineTemplateLabelRow(index + 1, label)));
    }

    private static void GenerateLabels(
        IEnumerable<MachineTemplateLabelRow> rows,
        string prefix)
    {
        foreach (MachineTemplateLabelRow row in rows)
        {
            row.Label = $"{prefix} {row.Number}";
        }
    }

    private static string? FirstInvalidLabel(
        IEnumerable<MachineTemplateLabelRow> rows)
    {
        foreach (MachineTemplateLabelRow row in rows)
        {
            string? error = DanteNameRules.ValidateChannelName(row.Label);
            if (error is not null)
            {
                return $"#{row.Number} « {row.Label} » - {error}";
            }
        }

        return null;
    }

    private void ShowValidation(string message)
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

public sealed class MachineTemplateLabelRow
{
    public MachineTemplateLabelRow(int number, string label)
    {
        Number = number;
        Label = label;
    }

    public int Number { get; }

    public string Label { get; set; }
}
