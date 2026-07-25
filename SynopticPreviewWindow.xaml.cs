using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using DanteConfigEditor.Services;

namespace DanteConfigEditor;

public partial class SynopticPreviewWindow : Window
{
    private readonly ScaleTransform _zoomTransform = new(1, 1);
    private bool _loaded;

    public SynopticPreviewWindow(Visual source, double width, double height, UiLanguage language)
    {
        InitializeComponent();
        PreviewSurface.Width = Math.Max(1, width);
        PreviewSurface.Height = Math.Max(1, height);
        PreviewSurface.Fill = new VisualBrush(source)
        {
            Stretch = Stretch.Fill,
            AlignmentX = AlignmentX.Left,
            AlignmentY = AlignmentY.Top
        };
        PreviewSurface.LayoutTransform = _zoomTransform;
        ApplyLanguage(language);
    }

    private void ApplyLanguage(UiLanguage language)
    {
        bool english = language == UiLanguage.English;
        Title = english ? "Synoptic preview" : "Aperçu du synoptique";
        CloseButton.Content = english ? "Close" : "Fermer";
        FitButton.Content = english ? "Fit" : "Ajuster";
        ZoomOutButton.ToolTip = english ? "Zoom out" : "Réduire le zoom";
        ZoomInButton.ToolTip = english ? "Zoom in" : "Augmenter le zoom";
        ActualSizeButton.ToolTip = english ? "Return to actual size" : "Revenir à la taille réelle";
        FitButton.ToolTip = english ? "Fit the whole synoptic" : "Afficher tout le synoptique";
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        _loaded = true;
        FitToWindow();
    }

    private void ZoomOutButton_Click(object sender, RoutedEventArgs e) => SetZoom(ZoomSlider.Value - 0.1);

    private void ZoomInButton_Click(object sender, RoutedEventArgs e) => SetZoom(ZoomSlider.Value + 0.1);

    private void ActualSizeButton_Click(object sender, RoutedEventArgs e) => SetZoom(1);

    private void FitButton_Click(object sender, RoutedEventArgs e) => FitToWindow();

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

    private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_loaded)
        {
            return;
        }

        _zoomTransform.ScaleX = e.NewValue;
        _zoomTransform.ScaleY = e.NewValue;
        ZoomText.Text = $"{Math.Round(e.NewValue * 100):0} %";
    }

    private void PreviewScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            return;
        }

        e.Handled = true;
        SetZoom(ZoomSlider.Value + (e.Delta > 0 ? 0.1 : -0.1));
    }

    private void FitToWindow()
    {
        double availableWidth = Math.Max(1, PreviewScrollViewer.ViewportWidth - 48);
        double availableHeight = Math.Max(1, PreviewScrollViewer.ViewportHeight - 48);
        SetZoom(Math.Min(
            availableWidth / Math.Max(1, PreviewSurface.Width),
            availableHeight / Math.Max(1, PreviewSurface.Height)));
        PreviewScrollViewer.ScrollToHome();
    }

    private void SetZoom(double value)
    {
        ZoomSlider.Value = Math.Clamp(value, ZoomSlider.Minimum, ZoomSlider.Maximum);
    }
}
