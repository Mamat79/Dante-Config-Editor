using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Controls.Shapes;
using DanteConfigEditor.Services;

namespace DanteConfigEditor.Mac;

public sealed partial class SynopticPreviewWindow : Window
{
    private bool _opened;

    public SynopticPreviewWindow()
    {
        InitializeComponent();
    }

    public SynopticPreviewWindow(
        Visual source,
        double width,
        double height,
        UiLanguage language)
        : this()
    {
        Rectangle surface = FindControl<Rectangle>("PreviewSurface")!;
        surface.Width = Math.Max(1, width);
        surface.Height = Math.Max(1, height);
        surface.Fill = new VisualBrush(source)
        {
            Stretch = Stretch.Fill,
            AlignmentX = AlignmentX.Left,
            AlignmentY = AlignmentY.Top
        };
        ApplyLanguage(language);
        Opened += (_, _) =>
        {
            _opened = true;
            FitToWindow();
        };
    }

    private T? FindControl<T>(string name) where T : Control =>
        ControlExtensions.FindControl<T>(this, name);

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void ApplyLanguage(UiLanguage language)
    {
        bool english = language == UiLanguage.English;
        Title = english ? "Synoptic preview" : "Aperçu du synoptique";
        FindControl<Button>("CloseButton")!.Content = english ? "Close" : "Fermer";
        FindControl<Button>("FitButton")!.Content = english ? "Fit" : "Ajuster";
        ToolTip.SetTip(FindControl<Button>("ZoomOutButton")!, english ? "Zoom out" : "Réduire le zoom");
        ToolTip.SetTip(FindControl<Button>("ZoomInButton")!, english ? "Zoom in" : "Augmenter le zoom");
        ToolTip.SetTip(FindControl<Button>("ActualSizeButton")!, english ? "Return to actual size" : "Revenir à la taille réelle");
        ToolTip.SetTip(FindControl<Button>("FitButton")!, english ? "Fit the whole synoptic" : "Afficher tout le synoptique");
    }

    private void ZoomOutButton_Click(object? sender, RoutedEventArgs e) => SetZoom(GetZoomSlider().Value - 0.1);

    private void ZoomInButton_Click(object? sender, RoutedEventArgs e) => SetZoom(GetZoomSlider().Value + 0.1);

    private void ActualSizeButton_Click(object? sender, RoutedEventArgs e) => SetZoom(1);

    private void FitButton_Click(object? sender, RoutedEventArgs e) => FitToWindow();

    private void CloseButton_Click(object? sender, RoutedEventArgs e) => Close();

    private void ZoomSlider_ValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (!_opened)
        {
            return;
        }

        FindControl<LayoutTransformControl>("PreviewZoomHost")!.LayoutTransform =
            new ScaleTransform(e.NewValue, e.NewValue);
        FindControl<TextBlock>("ZoomText")!.Text = $"{Math.Round(e.NewValue * 100):0} %";
    }

    private void PreviewScrollViewer_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (!e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            return;
        }

        SetZoom(GetZoomSlider().Value + (e.Delta.Y > 0 ? 0.1 : -0.1));
        e.Handled = true;
    }

    private void FitToWindow()
    {
        ScrollViewer viewer = FindControl<ScrollViewer>("PreviewScrollViewer")!;
        Rectangle surface = FindControl<Rectangle>("PreviewSurface")!;
        double availableWidth = Math.Max(1, viewer.Viewport.Width - 48);
        double availableHeight = Math.Max(1, viewer.Viewport.Height - 48);
        SetZoom(Math.Min(
            availableWidth / Math.Max(1, surface.Width),
            availableHeight / Math.Max(1, surface.Height)));
        viewer.ScrollToHome();
    }

    private void SetZoom(double value)
    {
        Slider slider = GetZoomSlider();
        slider.Value = Math.Clamp(value, slider.Minimum, slider.Maximum);
    }

    private Slider GetZoomSlider() => FindControl<Slider>("ZoomSlider")!;
}
