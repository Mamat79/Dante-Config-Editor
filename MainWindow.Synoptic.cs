using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using DanteConfigEditor.Models;
using DanteConfigEditor.Services;
using Microsoft.Win32;

namespace DanteConfigEditor;

public partial class MainWindow
{
    private readonly ObservableCollection<SynopticDeviceRow> _synopticRows = [];
    private SynopticLayoutDocument? _synopticLayout;
    private string? _synopticLayoutPath;
    private readonly Dictionary<string, SynopticDeviceNode> _synopticPreviewNodes = new(StringComparer.OrdinalIgnoreCase);
    private Border? _draggedSynopticCard;
    private string? _draggedSynopticIdentity;
    private Point _synopticDragOffset;
    private SynopticPreviewWindow? _synopticPreviewWindow;

    private void RefreshSynopticWorkspace(bool persistCurrentRows = true)
    {
        if (_project is null)
        {
            _synopticRows.Clear();
            _synopticLayout = null;
            _synopticLayoutPath = null;
            SynopticCanvas.Children.Clear();
            SynopticLocationComboBox.ItemsSource = null;
            SynopticLocationComboBox.Text = string.Empty;
            SynopticSummaryTextBlock.Text = T("Status.NoFileLoaded");
            UpdateSynopticCommandState(false);
            return;
        }

        string expectedPath = SynopticExportService.ResolveLayoutPath(_project);
        if (_synopticLayout is null || !string.Equals(_synopticLayoutPath, expectedPath, StringComparison.OrdinalIgnoreCase))
        {
            _synopticLayout = SynopticExportService.LoadOrCreate(_project);
            _synopticLayoutPath = expectedPath;
        }
        else
        {
            if (persistCurrentRows)
            {
                CaptureSynopticRows();
            }
            _synopticLayout = SynopticExportService.Synchronize(_project, _synopticLayout);
        }

        HashSet<string> selectedIdentities = SynopticDeviceGrid.SelectedItems
            .OfType<SynopticDeviceRow>()
            .Select(row => row.DeviceIdentity)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, DanteDevice> devicesByIdentity = _project.Devices
            .GroupBy(device => device.StableIdentity, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        _synopticRows.Clear();
        foreach (SynopticDevicePlacement placement in _synopticLayout.Devices
                     .OrderBy(item => item.Location, StringComparer.CurrentCultureIgnoreCase)
                     .ThenBy(item => item.Order)
                     .ThenBy(item => item.DeviceName, StringComparer.CurrentCultureIgnoreCase))
        {
            if (devicesByIdentity.TryGetValue(placement.DeviceIdentity, out DanteDevice? device))
            {
                _synopticRows.Add(new SynopticDeviceRow(placement, device.TxCount, device.RxCount));
            }
        }

        SynopticDeviceGrid.SelectedItems.Clear();
        foreach (SynopticDeviceRow row in _synopticRows.Where(row => selectedIdentities.Contains(row.DeviceIdentity)))
        {
            SynopticDeviceGrid.SelectedItems.Add(row);
        }

        RefreshSynopticLocationChoices();
        RenderSynopticPreview();
        UpdateSynopticCommandState(true);
    }

    private void CaptureSynopticRows()
    {
        if (_synopticLayout is null || _synopticRows.Count == 0)
        {
            return;
        }

        Dictionary<string, SynopticDevicePlacement> placements = _synopticLayout.Devices
            .ToDictionary(item => item.DeviceIdentity, StringComparer.OrdinalIgnoreCase);
        foreach (SynopticDeviceRow row in _synopticRows)
        {
            if (placements.TryGetValue(row.DeviceIdentity, out SynopticDevicePlacement? placement))
            {
                placement.DeviceName = row.DeviceName;
                placement.Location = row.Location.Trim();
                placement.IsVisible = row.IsVisible;
                placement.Order = Math.Max(0, row.Order);
            }
        }
    }

    private void RenderSynopticPreview()
    {
        SynopticCanvas.Children.Clear();
        if (_project is null || _synopticLayout is null)
        {
            return;
        }

        SynopticDiagram diagram = SynopticExportService.BuildDiagram(_project, _synopticLayout, _language == UiLanguage.English);
        _synopticPreviewNodes.Clear();
        foreach (SynopticDeviceNode node in diagram.Devices)
        {
            _synopticPreviewNodes[node.Identity] = node;
        }
        SynopticCanvas.Width = diagram.Width;
        SynopticCanvas.Height = diagram.Height;

        foreach (SynopticLocationArea location in diagram.Locations)
        {
            Border area = new()
            {
                Width = location.Width,
                Height = location.Height,
                Background = Brushes.White,
                BorderBrush = BrushFromHex(location.Color),
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(7)
            };
            Canvas.SetLeft(area, location.X);
            Canvas.SetTop(area, location.Y);
            SynopticCanvas.Children.Add(area);

            Border header = new()
            {
                Width = location.Width,
                Height = 36,
                Background = BrushFromHex(location.Color),
                CornerRadius = new CornerRadius(6, 6, 0, 0),
                Child = new TextBlock
                {
                    Text = location.Name,
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.SemiBold,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(13, 0, 8, 0)
                }
            };
            Canvas.SetLeft(header, location.X);
            Canvas.SetTop(header, location.Y);
            Panel.SetZIndex(header, 1);
            SynopticCanvas.Children.Add(header);
        }

        foreach (SynopticCable cable in diagram.Cables)
        {
            System.Windows.Shapes.Path underlay = BuildPreviewCablePath(cable, Brushes.White, 8, showArrow: false);
            underlay.Tag = cable;
            Panel.SetZIndex(underlay, 1);
            SynopticCanvas.Children.Add(underlay);

            System.Windows.Shapes.Path path = BuildPreviewCablePath(cable, BrushFromHex(cable.Color), 3.5, showArrow: true);
            path.Tag = cable;
            Panel.SetZIndex(path, 2);
            SynopticCanvas.Children.Add(path);
        }

        foreach (SynopticDeviceNode node in diagram.Devices)
        {
            Border deviceCard = new()
            {
                Width = node.Width,
                Height = node.Height,
                Background = Brushes.White,
                BorderBrush = BrushFromHex(node.Color),
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(16, 10, 10, 8),
                Child = new StackPanel
                {
                    Children =
                    {
                        new TextBlock { Text = node.Name, Foreground = BrushFromHex("#172033"), FontWeight = FontWeights.Bold, FontSize = 14, TextTrimming = TextTrimming.CharacterEllipsis },
                        new TextBlock { Text = FriendlyLine(node), Foreground = BrushFromHex("#526070"), FontSize = 11, Margin = new Thickness(0, 3, 0, 0), TextTrimming = TextTrimming.CharacterEllipsis },
                        new TextBlock { Text = $"TX {node.TxCount}   RX {node.RxCount}", Foreground = BrushFromHex("#526070"), FontSize = 11, Margin = new Thickness(0, 3, 0, 0) }
                    }
                }
            };
            deviceCard.Tag = node.Identity;
            deviceCard.Cursor = Cursors.SizeAll;
            deviceCard.ToolTip = _language == UiLanguage.English
                ? "Drag this device to arrange the synoptic."
                : "Faites glisser cette machine pour organiser le synoptique.";
            deviceCard.MouseLeftButtonDown += SynopticDeviceCard_MouseLeftButtonDown;
            deviceCard.MouseMove += SynopticDeviceCard_MouseMove;
            deviceCard.MouseLeftButtonUp += SynopticDeviceCard_MouseLeftButtonUp;
            Canvas.SetLeft(deviceCard, node.X);
            Canvas.SetTop(deviceCard, node.Y);
            Panel.SetZIndex(deviceCard, 4);
            SynopticCanvas.Children.Add(deviceCard);
        }

        if (diagram.Cables.Count <= 18)
        {
            for (int index = 0; index < diagram.Cables.Count; index++)
            {
                SynopticCable cable = diagram.Cables[index];
                Border badge = new()
                {
                    Width = 18,
                    Height = 18,
                    Background = Brushes.White,
                    BorderBrush = BrushFromHex(cable.Color),
                    BorderThickness = new Thickness(2),
                    CornerRadius = new CornerRadius(9),
                    Child = new TextBlock
                    {
                        Text = (index + 1).ToString(),
                        Foreground = BrushFromHex("#172033"),
                        FontWeight = FontWeights.Bold,
                        FontSize = 8,
                        TextAlignment = TextAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                };
                Canvas.SetLeft(badge, Math.Max(4, cable.LabelX - 9));
                Canvas.SetTop(badge, Math.Max(4, cable.LabelY - 9));
                Panel.SetZIndex(badge, 5);
                SynopticCanvas.Children.Add(badge);
            }
        }

        RenderSynopticLegend(diagram);

        SynopticSummaryTextBlock.Text = _language == UiLanguage.English
            ? $"{diagram.Devices.Count} devices - {diagram.Cables.Count} grouped cables - {diagram.HiddenDeviceCount} hidden"
            : $"{diagram.Devices.Count} machines - {diagram.Cables.Count} câbles regroupés - {diagram.HiddenDeviceCount} masquée(s)";
    }

    private static System.Windows.Shapes.Path BuildPreviewCablePath(SynopticCable cable, Brush stroke, double thickness, bool showArrow)
    {
        IReadOnlyList<SynopticRoutePoint> points = cable.RoutePoints.Count == 0
            ? [new SynopticRoutePoint(cable.StartX, cable.StartY), new SynopticRoutePoint(cable.EndX, cable.EndY)]
            : cable.RoutePoints;

        return new System.Windows.Shapes.Path
        {
            Data = BuildPreviewCableGeometry(points, showArrow, showArrow && cable.IsBidirectional),
            Stroke = stroke,
            Fill = showArrow ? stroke : null,
            StrokeThickness = thickness,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round,
            StrokeLineJoin = PenLineJoin.Round,
            Opacity = showArrow ? 0.92 : 0.96
        };
    }

    private static PathGeometry BuildPreviewCableGeometry(
        IReadOnlyList<SynopticRoutePoint> points,
        bool arrowAtEnd,
        bool arrowAtStart)
    {
        PathGeometry geometry = new();
        if (points.Count == 0)
        {
            return geometry;
        }

        PathFigure line = new()
        {
            StartPoint = new Point(points[0].X, points[0].Y),
            IsFilled = false
        };
        if (points.Count > 1)
        {
            line.Segments.Add(new PolyLineSegment(points.Skip(1).Select(point => new Point(point.X, point.Y)), true));
        }
        geometry.Figures.Add(line);

        if (points.Count >= 2 && arrowAtEnd)
        {
            AddPreviewArrowFigure(geometry, points[^1], points[^2]);
        }
        if (points.Count >= 2 && arrowAtStart)
        {
            AddPreviewArrowFigure(geometry, points[0], points[1]);
        }
        return geometry;
    }

    private static void AddPreviewArrowFigure(PathGeometry geometry, SynopticRoutePoint tip, SynopticRoutePoint previous)
    {
        double dx = tip.X - previous.X;
        double dy = tip.Y - previous.Y;
        double length = Math.Sqrt(dx * dx + dy * dy);
        if (length < 0.01)
        {
            return;
        }

        dx /= length;
        dy /= length;
        double backX = tip.X - dx * 12;
        double backY = tip.Y - dy * 12;
        double perpendicularX = -dy * 5;
        double perpendicularY = dx * 5;
        PathFigure arrow = new()
        {
            StartPoint = new Point(tip.X, tip.Y),
            IsClosed = true,
            IsFilled = true
        };
        arrow.Segments.Add(new LineSegment(new Point(backX + perpendicularX, backY + perpendicularY), true));
        arrow.Segments.Add(new LineSegment(new Point(backX - perpendicularX, backY - perpendicularY), true));
        geometry.Figures.Add(arrow);
    }

    private void RenderSynopticLegend(SynopticDiagram diagram)
    {
        double legendX = diagram.LegendX;
        double topMargin = diagram.Locations.Count == 0 ? 88 : diagram.Locations.Min(location => location.Y);
        Border panel = new()
        {
            Width = diagram.LegendWidth,
            Height = diagram.Height - topMargin - 76,
            Background = Brushes.White,
            BorderBrush = BrushFromHex("#CBD5E1"),
            BorderThickness = new Thickness(1.5),
            CornerRadius = new CornerRadius(8)
        };
        Canvas.SetLeft(panel, legendX);
        Canvas.SetTop(panel, topMargin);
        Panel.SetZIndex(panel, 3);
        SynopticCanvas.Children.Add(panel);

        TextBlock title = new()
        {
            Text = _language == UiLanguage.English ? "Grouped subscriptions" : "Liaisons regroupées",
            Foreground = BrushFromHex("#172033"),
            FontSize = 15,
            FontWeight = FontWeights.Bold
        };
        Canvas.SetLeft(title, legendX + 18);
        Canvas.SetTop(title, topMargin + 16);
        Panel.SetZIndex(title, 6);
        SynopticCanvas.Children.Add(title);

        int columns = Math.Max(1, diagram.LegendColumns);
        double gap = 10;
        double itemWidth = (diagram.LegendWidth - 24 - (columns - 1) * gap) / columns;
        double y = topMargin + 48;
        for (int rowStart = 0; rowStart < diagram.Cables.Count; rowStart += columns)
        {
            SynopticCable[] row = diagram.Cables.Skip(rowStart).Take(columns).ToArray();
            double rowHeight = row.Max(cable => Math.Max(62, 50 + cable.Labels.Count * 16));
            for (int column = 0; column < row.Length; column++)
            {
                int index = rowStart + column;
                SynopticCable cable = row[column];
                StackPanel content = new();
                content.Children.Add(new TextBlock
                {
                    Text = $"{index + 1}. {cable.SourceDevice} → {cable.TargetDevice}",
                    Foreground = BrushFromHex("#172033"),
                    FontWeight = FontWeights.Bold,
                    FontSize = 11.5,
                    TextTrimming = TextTrimming.CharacterEllipsis
                });
                content.Children.Add(new TextBlock
                {
                    Text = string.Join(Environment.NewLine, cable.Labels),
                    Foreground = BrushFromHex("#526070"),
                    FontSize = 10.5,
                    Margin = new Thickness(0, 5, 0, 0),
                    TextWrapping = TextWrapping.Wrap
                });
                Border item = new()
                {
                    Width = itemWidth,
                    Height = rowHeight - 6,
                    Background = BrushFromHex("#F8FAFC"),
                    BorderBrush = BrushFromHex(cable.Color),
                    BorderThickness = new Thickness(5, 1, 1, 1),
                    CornerRadius = new CornerRadius(5),
                    Padding = new Thickness(10, 7, 8, 6),
                    Child = content
                };
                Canvas.SetLeft(item, legendX + 12 + column * (itemWidth + gap));
                Canvas.SetTop(item, y);
                Panel.SetZIndex(item, 6);
                SynopticCanvas.Children.Add(item);
            }
            y += rowHeight;
        }
    }

    private void ApplySynopticLocationButton_Click(object sender, RoutedEventArgs e)
    {
        SynopticDeviceRow[] selected = SynopticDeviceGrid.SelectedItems.OfType<SynopticDeviceRow>().ToArray();
        if (selected.Length == 0)
        {
            ShowError(T("Dialog.NoDeviceSelectedTitle"), T("Dialog.NoDeviceSelectedMessage"));
            return;
        }

        string location = SynopticLocationComboBox.Text.Trim();
        foreach (SynopticDeviceRow row in selected)
        {
            row.Location = location;
        }
        SaveAndRefreshSynoptic();
    }

    private void SynopticDeviceCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Border card || card.Tag is not string identity || !_synopticPreviewNodes.TryGetValue(identity, out SynopticDeviceNode? node))
        {
            return;
        }

        _draggedSynopticCard = card;
        _draggedSynopticIdentity = identity;
        Point pointer = e.GetPosition(SynopticCanvas);
        _synopticDragOffset = new Point(pointer.X - node.X, pointer.Y - node.Y);
        card.CaptureMouse();
        e.Handled = true;
    }

    private void SynopticDeviceCard_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed
            || _draggedSynopticCard is null
            || _draggedSynopticIdentity is null
            || !_synopticPreviewNodes.TryGetValue(_draggedSynopticIdentity, out SynopticDeviceNode? node)
            || _synopticLayout is null)
        {
            return;
        }

        Point pointer = e.GetPosition(SynopticCanvas);
        double x = Math.Max(4, pointer.X - _synopticDragOffset.X);
        double y = Math.Max(74, pointer.Y - _synopticDragOffset.Y);
        Canvas.SetLeft(_draggedSynopticCard, x);
        Canvas.SetTop(_draggedSynopticCard, y);
        SynopticDeviceNode movedNode = node with { X = x, Y = y };
        _synopticPreviewNodes[_draggedSynopticIdentity] = movedNode;

        SynopticDevicePlacement? placement = _synopticLayout.Devices.FirstOrDefault(item =>
            string.Equals(item.DeviceIdentity, _draggedSynopticIdentity, StringComparison.OrdinalIgnoreCase));
        if (placement is not null)
        {
            placement.ManualX = x;
            placement.ManualY = y;
        }
        UpdateSynopticCablesDuringDrag();
        e.Handled = true;
    }

    private void SynopticDeviceCard_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_draggedSynopticCard is null)
        {
            return;
        }

        _draggedSynopticCard.ReleaseMouseCapture();
        _draggedSynopticCard = null;
        _draggedSynopticIdentity = null;
        SaveAndRefreshSynoptic();
        e.Handled = true;
    }

    private void UpdateSynopticCablesDuringDrag()
    {
        Dictionary<string, SynopticDeviceNode> nodesByName = _synopticPreviewNodes.Values
            .GroupBy(node => node.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
        foreach (System.Windows.Shapes.Path path in SynopticCanvas.Children.OfType<System.Windows.Shapes.Path>())
        {
            if (path.Tag is not SynopticCable cable
                || !nodesByName.TryGetValue(cable.SourceDevice, out SynopticDeviceNode? source)
                || !nodesByName.TryGetValue(cable.TargetDevice, out SynopticDeviceNode? target))
            {
                continue;
            }

            bool forward = source.X + source.Width / 2 <= target.X + target.Width / 2;
            double startX = forward ? source.X + source.Width : source.X;
            double endX = forward ? target.X : target.X + target.Width;
            double startY = source.Y + source.Height / 2;
            double endY = target.Y + target.Height / 2;
            double laneX = (startX + endX) / 2;
            IReadOnlyList<SynopticRoutePoint> points =
            [
                new SynopticRoutePoint(startX, startY),
                new SynopticRoutePoint(laneX, startY),
                new SynopticRoutePoint(laneX, endY),
                new SynopticRoutePoint(endX, endY)
            ];
            path.Data = BuildPreviewCableGeometry(
                points,
                arrowAtEnd: path.Fill is not null,
                arrowAtStart: path.Fill is not null && cable.IsBidirectional);
        }
    }

    private void SynopticVisibilityCheckBox_Click(object sender, RoutedEventArgs e)
    {
        // Le clic doit rester immédiat : on attend seulement que le binding ait
        // copié la nouvelle valeur avant de sauvegarder le fichier annexe.
        Dispatcher.BeginInvoke(SaveAndRefreshSynoptic);
    }

    private void RefreshSynopticLocationChoices()
    {
        string currentText = SynopticLocationComboBox.Text;
        string[] locations = _synopticRows
            .Select(row => row.Location.Trim())
            .Where(location => !string.IsNullOrWhiteSpace(location))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(location => location, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
        SynopticLocationComboBox.ItemsSource = locations;
        SynopticLocationComboBox.Text = currentText;
    }

    private void ShowAllSynopticDevicesButton_Click(object sender, RoutedEventArgs e)
    {
        foreach (SynopticDeviceRow row in _synopticRows)
        {
            row.IsVisible = true;
        }
        SaveAndRefreshSynoptic();
    }

    private void HideSelectedSynopticDevicesButton_Click(object sender, RoutedEventArgs e)
    {
        foreach (SynopticDeviceRow row in SynopticDeviceGrid.SelectedItems.OfType<SynopticDeviceRow>())
        {
            row.IsVisible = false;
        }
        SaveAndRefreshSynoptic();
    }

    private void MoveSynopticDeviceUpButton_Click(object sender, RoutedEventArgs e) => MoveSelectedSynopticRow(-1);

    private void MoveSynopticDeviceDownButton_Click(object sender, RoutedEventArgs e) => MoveSelectedSynopticRow(1);

    private void MoveSelectedSynopticRow(int offset)
    {
        if (SynopticDeviceGrid.SelectedItem is not SynopticDeviceRow selected)
        {
            return;
        }

        SynopticDeviceRow[] siblings = _synopticRows
            .Where(row => string.Equals(row.Location, selected.Location, StringComparison.OrdinalIgnoreCase))
            .OrderBy(row => row.Order)
            .ThenBy(row => row.DeviceName, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
        int oldIndex = Array.IndexOf(siblings, selected);
        int newIndex = Math.Clamp(oldIndex + offset, 0, siblings.Length - 1);
        if (oldIndex < 0 || newIndex == oldIndex)
        {
            return;
        }

        SynopticDeviceRow other = siblings[newIndex];
        (selected.Order, other.Order) = (other.Order, selected.Order);
        SaveAndRefreshSynoptic();
        SynopticDeviceGrid.SelectedItem = selected;
    }

    private void ResetSynopticLayoutButton_Click(object sender, RoutedEventArgs e)
    {
        if (_synopticLayout is null)
        {
            return;
        }

        foreach (SynopticDevicePlacement placement in _synopticLayout.Devices)
        {
            placement.ManualX = null;
            placement.ManualY = null;
        }

        foreach (IGrouping<string, SynopticDevicePlacement> location in _synopticLayout.Devices
                     .GroupBy(item => item.Location?.Trim() ?? string.Empty, StringComparer.OrdinalIgnoreCase))
        {
            int order = 0;
            foreach (SynopticDevicePlacement placement in location.OrderBy(item => item.DeviceName, StringComparer.CurrentCultureIgnoreCase))
            {
                placement.Order = order++;
            }
        }
        SaveAndRefreshSynoptic();
    }

    private void SynopticZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (SynopticCanvas is null || SynopticZoomTextBlock is null)
        {
            return;
        }

        double scale = Math.Clamp(e.NewValue, 0.1, 2.5);
        SynopticCanvas.LayoutTransform = new ScaleTransform(scale, scale);
        SynopticZoomTextBlock.Text = $"{Math.Round(scale * 100):0} %";
    }

    private void ResetSynopticZoomButton_Click(object sender, RoutedEventArgs e)
    {
        SetSynopticZoomPreservingCenter(1);
    }

    private void ZoomOutSynopticButton_Click(object sender, RoutedEventArgs e)
    {
        SetSynopticZoomPreservingCenter(SynopticZoomSlider.Value - 0.25);
    }

    private void ZoomInSynopticButton_Click(object sender, RoutedEventArgs e)
    {
        SetSynopticZoomPreservingCenter(SynopticZoomSlider.Value + 0.25);
    }

    private void SynopticScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if ((Keyboard.Modifiers & ModifierKeys.Control) == 0)
        {
            return;
        }

        SetSynopticZoomPreservingCenter(SynopticZoomSlider.Value + (e.Delta > 0 ? 0.1 : -0.1));
        e.Handled = true;
    }

    private void SetSynopticZoomPreservingCenter(double requestedValue)
    {
        double horizontalRatio = SynopticScrollViewer.ExtentWidth <= 0
            ? 0.5
            : (SynopticScrollViewer.HorizontalOffset + (SynopticScrollViewer.ViewportWidth / 2))
                / SynopticScrollViewer.ExtentWidth;
        double verticalRatio = SynopticScrollViewer.ExtentHeight <= 0
            ? 0.5
            : (SynopticScrollViewer.VerticalOffset + (SynopticScrollViewer.ViewportHeight / 2))
                / SynopticScrollViewer.ExtentHeight;
        SynopticZoomSlider.Value = Math.Clamp(
            requestedValue,
            SynopticZoomSlider.Minimum,
            SynopticZoomSlider.Maximum);
        SynopticScrollViewer.UpdateLayout();
        SynopticScrollViewer.ScrollToHorizontalOffset(
            (horizontalRatio * SynopticScrollViewer.ExtentWidth) - (SynopticScrollViewer.ViewportWidth / 2));
        SynopticScrollViewer.ScrollToVerticalOffset(
            (verticalRatio * SynopticScrollViewer.ExtentHeight) - (SynopticScrollViewer.ViewportHeight / 2));
    }

    private void FitSynopticZoomButton_Click(object sender, RoutedEventArgs e)
    {
        if (SynopticCanvas.Width <= 0 || SynopticCanvas.Height <= 0)
        {
            return;
        }

        double availableWidth = Math.Max(1, SynopticScrollViewer.ViewportWidth - 24);
        double availableHeight = Math.Max(1, SynopticScrollViewer.ViewportHeight - 24);
        SynopticZoomSlider.Value = Math.Clamp(
            Math.Min(availableWidth / SynopticCanvas.Width, availableHeight / SynopticCanvas.Height),
            SynopticZoomSlider.Minimum,
            1);
        SynopticScrollViewer.ScrollToHome();
    }

    private void OpenSynopticPreviewWindowButton_Click(object sender, RoutedEventArgs e)
    {
        if (_project is null || _synopticLayout is null)
        {
            return;
        }

        RenderSynopticPreview();
        if (_synopticPreviewWindow?.IsVisible == true)
        {
            _synopticPreviewWindow.Activate();
            return;
        }

        _synopticPreviewWindow = new SynopticPreviewWindow(
            SynopticCanvas,
            SynopticCanvas.Width,
            SynopticCanvas.Height,
            _language)
        {
            Owner = this
        };
        _synopticPreviewWindow.Closed += (_, _) => _synopticPreviewWindow = null;
        _synopticPreviewWindow.Show();
    }

    private void SynopticDeviceGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        Dispatcher.BeginInvoke(() => SaveAndRefreshSynoptic());
    }

    private void RefreshSynopticButton_Click(object sender, RoutedEventArgs e)
    {
        SaveAndRefreshSynoptic();
    }

    private void SaveAndRefreshSynoptic()
    {
        if (_project is null || _synopticLayout is null)
        {
            return;
        }

        try
        {
            CaptureSynopticRows();
            SynopticExportService.SaveLayout(_project, _synopticLayout);
            RefreshSynopticLocationChoices();
            RenderSynopticPreview();
        }
        catch (Exception ex)
        {
            ShowError(T("Dialog.SynopticLayoutErrorTitle"), ex.Message);
        }
    }

    private void ExportSynopticButton_Click(object sender, RoutedEventArgs e)
        => ExportSynoptic("svg");

    private void ExportSynopticPdfButton_Click(object sender, RoutedEventArgs e)
        => ExportSynoptic("pdf");

    private void ExportSynoptic(string format)
    {
        if (!EnsureProjectLoaded() || _synopticLayout is null)
        {
            return;
        }

        bool pdf = string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase);
        string extension = pdf ? ".pdf" : ".svg";
        string defaultName = Path.GetFileNameWithoutExtension(_project!.OriginalFilePath) + "_synoptique" + extension;
        SaveFileDialog dialog = new()
        {
            Filter = pdf
                ? "Document PDF (*.pdf)|*.pdf|Tous les fichiers (*.*)|*.*"
                : "Image vectorielle SVG (*.svg)|*.svg|Tous les fichiers (*.*)|*.*",
            DefaultExt = extension,
            AddExtension = true,
            Title = pdf ? "Exporter le synoptique PDF" : "Exporter le synoptique SVG",
            FileName = defaultName,
            InitialDirectory = Path.GetDirectoryName(_project.OriginalFilePath)
        };
        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        try
        {
            CaptureSynopticRows();
            SynopticExportService.SaveLayout(_project, _synopticLayout);
            SynopticDiagram diagram = SynopticExportService.BuildDiagram(_project, _synopticLayout, _language == UiLanguage.English);
            if (pdf)
            {
                SynopticExportService.ExportPdf(dialog.FileName, diagram, _language == UiLanguage.English);
            }
            else
            {
                SynopticExportService.ExportSvg(dialog.FileName, diagram, _language == UiLanguage.English);
            }
            AddLog((_language == UiLanguage.English ? "Synoptic exported: " : "Synoptique exporté : ") + dialog.FileName);
            SetStatus(_language == UiLanguage.English
                ? $"Synoptic {format.ToUpperInvariant()} exported."
                : $"Synoptique {format.ToUpperInvariant()} exporté.");
            if (OpenSynopticAfterExportCheckBox.IsChecked == true)
            {
                Process.Start(new ProcessStartInfo(dialog.FileName) { UseShellExecute = true });
            }
        }
        catch (Exception ex)
        {
            ShowError(_language == UiLanguage.English ? "Synoptic export unavailable" : "Export du synoptique impossible", ex.Message);
        }
    }

    private void UpdateSynopticCommandState(bool enabled)
    {
        SynopticDeviceGrid.IsEnabled = enabled;
        SynopticLocationComboBox.IsEnabled = enabled;
        ApplySynopticLocationButton.IsEnabled = enabled;
        ShowAllSynopticDevicesButton.IsEnabled = enabled;
        HideSelectedSynopticDevicesButton.IsEnabled = enabled;
        MoveSynopticDeviceUpButton.IsEnabled = enabled;
        MoveSynopticDeviceDownButton.IsEnabled = enabled;
        RefreshSynopticButton.IsEnabled = enabled;
        ExportSynopticButton.IsEnabled = enabled;
        ExportSynopticPdfButton.IsEnabled = enabled;
        OpenSynopticPreviewWindowButton.IsEnabled = enabled;
    }

    private static string FriendlyLine(SynopticDeviceNode node)
    {
        return string.IsNullOrWhiteSpace(node.FriendlyName) || string.Equals(node.FriendlyName, node.Name, StringComparison.OrdinalIgnoreCase)
            ? node.Location
            : node.FriendlyName;
    }

    private static SolidColorBrush BrushFromHex(string color)
    {
        return new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
    }

    private sealed class SynopticDeviceRow : INotifyPropertyChanged
    {
        private bool _isVisible;
        private string _location;
        private int _order;

        public SynopticDeviceRow(SynopticDevicePlacement placement, int txCount, int rxCount)
        {
            DeviceIdentity = placement.DeviceIdentity;
            DeviceName = placement.DeviceName;
            _location = placement.Location;
            _isVisible = placement.IsVisible;
            _order = placement.Order;
            TxCount = txCount;
            RxCount = rxCount;
        }

        public string DeviceIdentity { get; }
        public string DeviceName { get; }
        public int TxCount { get; }
        public int RxCount { get; }
        public string ChannelCount => $"{TxCount}/{RxCount}";

        public int Order
        {
            get => _order;
            set => SetField(ref _order, Math.Max(0, value));
        }

        public bool IsVisible
        {
            get => _isVisible;
            set => SetField(ref _isVisible, value);
        }

        public string Location
        {
            get => _location;
            set => SetField(ref _location, value ?? string.Empty);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return;
            }
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
