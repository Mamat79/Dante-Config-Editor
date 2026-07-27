using System.Collections.ObjectModel;
using System.Windows;
using DanteConfigEditor.Services;

namespace DanteConfigEditor;

public partial class ComparisonResultWindow : Window
{
    public ComparisonResultWindow(
        UiLanguage language,
        bool useLightTheme,
        IEnumerable<ComparisonDisplayRow> rows)
    {
        InitializeComponent();
        DialogThemeService.Apply(this, useLightTheme);
        bool english = language == UiLanguage.English;
        ObservableCollection<ComparisonDisplayRow> materializedRows = new(rows);
        ComparisonGrid.ItemsSource = materializedRows;
        Title = english ? "XML comparison" : "Comparaison XML";
        SummaryTextBlock.Text = english
            ? $"{materializedRows.Count} difference(s) displayed."
            : $"{materializedRows.Count} différence(s) affichée(s).";
        ItemColumn.Header = english ? "Item" : "Élément";
        OpenFileColumn.Header = english ? "Open file" : "Fichier ouvert";
        ComparedFileColumn.Header = english ? "Compared file" : "Fichier comparé";
        StatusColumn.Header = english ? "Status" : "État";
        CloseButton.Content = english ? "Close" : "Fermer";
    }
}

public sealed record ComparisonDisplayRow(string Item, string OpenValue, string ComparedValue, string Status);
