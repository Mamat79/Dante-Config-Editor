using System.Windows;
using System.Windows.Automation;
using DanteConfigEditor.Services;

namespace DanteConfigEditor;

public partial class SupportDceWindow : Window
{
    private readonly UiLanguage _language;

    public SupportDceWindow(UiLanguage language, bool useLightTheme)
    {
        InitializeComponent();
        _language = language;
        DialogThemeService.Apply(this, useLightTheme);
        ApplyLanguage();
    }

    private void ApplyLanguage()
    {
        Title = Text("Support.Title");
        TitleTextBlock.Text = Text("Support.Title");
        SubtitleTextBlock.Text = Text("Support.Subtitle");
        FreeTextBlock.Text = Text("Support.Free");
        SupportTextBlock.Text = Text("Support.PayPalDescription");
        OtherWaysTextBlock.Text = Text("Support.OtherWays");
        PayPalMeButton.Content = Text("Support.PayPalMeButton");
        CloseButton.Content = Text("Support.Close");
        AutomationProperties.SetName(PayPalMeButton, Text("Support.PayPalMeAutomation"));
        AutomationProperties.SetHelpText(PayPalMeButton, Text("Support.PayPalMeHelp"));
        AutomationProperties.SetName(CloseButton, Text("Support.CloseAutomation"));
    }

    private void PayPalMeButton_Click(object sender, RoutedEventArgs e)
    {
        if (SupportLinksService.TryOpenPayPalMe(out string? error))
        {
            return;
        }

        MessageBox.Show(
            this,
            $"{Text("Support.OpenError")}\n\n{error}",
            Text("Support.OpenErrorTitle"),
            MessageBoxButton.OK,
            MessageBoxImage.Warning);
    }

    private string Text(string key) => LocalizationService.Text(_language, key);
}
