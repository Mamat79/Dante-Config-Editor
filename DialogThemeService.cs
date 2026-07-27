using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DanteConfigEditor.Services;

public static class DialogThemeService
{
    public static void Apply(Window window, bool useLightTheme)
    {
        ArgumentNullException.ThrowIfNull(window);
        SetBrush(window, "DialogWindowBrush", useLightTheme ? "#F3F6FA" : "#10141F");
        SetBrush(window, "DialogSurfaceBrush", useLightTheme ? "#FFFFFF" : "#171D2B");
        SetBrush(window, "DialogTextBrush", useLightTheme ? "#111827" : "#F6F8FB");
        SetBrush(window, "DialogMutedBrush", useLightTheme ? "#4B5563" : "#AAB4C5");
        SetBrush(window, "DialogBorderBrush", useLightTheme ? "#CBD5E1" : "#334057");
        SetBrush(window, "DialogAccentBrush", useLightTheme ? "#1D4ED8" : "#2F80ED");

        // Une ressource déclarée dans la fenêtre ne peut pas appliquer son style
        // implicite à cette même fenêtre. On lie donc explicitement le fond et
        // le texte aux couleurs calculées pour éviter du blanc sur blanc.
        window.SetResourceReference(Control.BackgroundProperty, "DialogWindowBrush");
        window.SetResourceReference(Control.ForegroundProperty, "DialogTextBrush");
        window.FontFamily = new FontFamily("Segoe UI");
        window.FontSize = 13;
    }

    private static void SetBrush(FrameworkElement element, string key, string color)
    {
        element.Resources[key] = new SolidColorBrush(
            (Color)ColorConverter.ConvertFromString(color));
    }
}
