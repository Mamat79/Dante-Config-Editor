using Avalonia.Controls;
using Avalonia.Interactivity;
using DanteConfigEditor.Services;

namespace DanteConfigEditor.Mac;

internal sealed partial class SupportDceDialog : Window
{
    private readonly UiLanguage _language;

    public SupportDceDialog(UiLanguage language)
    {
        InitializeComponent();
        _language = language;
        ApplyLanguage();
    }

    public static Task ShowAsync(Window owner, UiLanguage language)
    {
        return new SupportDceDialog(language).ShowDialog(owner);
    }

    private T? FindControl<T>(string name) where T : Control =>
        ControlExtensions.FindControl<T>(this, name);

    private void ApplyLanguage()
    {
        Title = Text("Support.Title");
        FindControl<TextBlock>("TitleText")!.Text = Text("Support.Title");
        FindControl<TextBlock>("SubtitleText")!.Text = Text("Support.Subtitle");
        FindControl<TextBlock>("FreeText")!.Text = Text("Support.Free");
        FindControl<TextBlock>("SupportText")!.Text = Text("Support.PayPalDescription");
        FindControl<TextBlock>("OtherWaysText")!.Text = Text("Support.OtherWays");
        FindControl<Button>("PayPalMeButton")!.Content = Text("Support.PayPalMeButton");
        FindControl<Button>("CloseButton")!.Content = Text("Support.Close");
    }

    private async void PayPalMeButton_Click(object? sender, RoutedEventArgs e)
    {
        if (SupportLinksService.TryOpenPayPalMe(out string? error))
        {
            return;
        }

        await MessageDialog.ShowInfoAsync(
            this,
            Text("Support.OpenErrorTitle"),
            $"{Text("Support.OpenError")}\n\n{error}",
            Text("Support.Close"));
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private string Text(string key) => LocalizationService.Text(_language, key);
}
