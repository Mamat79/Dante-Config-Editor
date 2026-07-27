using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using DanteConfigEditor.Models;
using DanteConfigEditor.Services;
using DanteConfigEditorV3.TestSupport;
using Xunit.Abstractions;

namespace DanteConfigEditor.Mac.Tests;

public sealed class PatchMatrixPerformanceTests(ITestOutputHelper output)
{
    [AvaloniaFact]
    public void Matrix64ClickPerformance()
    {
        MeasureSingleClick(64);
    }

    [AvaloniaFact]
    public void Matrix128ClickPerformance()
    {
        MeasureSingleClick(128);
    }

    [AvaloniaFact]
    public void Matrix128StagesOneHundredClicksWithoutRebuild()
    {
        MeasureClickSeries(128, 100);
    }

    private void MeasureSingleClick(int channelCount)
    {
        string path = Path.Combine(Path.GetTempPath(), $"dce-matrix-{channelCount}-{Guid.NewGuid():N}.xml");
        SyntheticPresetFactory.Create(path, deviceCount: 2, txPerDevice: channelCount, rxPerDevice: channelCount);
        DanteProject project = DanteProject.Load(path);
        PatchWorkspaceDialog dialog = new(
            UiLanguage.English,
            project,
            initialTxDeviceName: "DEVICE-001",
            initialRxDeviceName: "DEVICE-002");
        dialog.Show();

        try
        {
            Dispatcher.UIThread.RunJobs();
            Grid matrix = dialog.FindControl<Grid>("MatrixPanel")!;
            Button activeCell = matrix.Children
                .OfType<Button>()
                .First(button => Equals(button.Content, "●"));
            int matrixBuildCount = dialog.MatrixBuildCount;

            Stopwatch stopwatch = Stopwatch.StartNew();
            activeCell.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Dispatcher.UIThread.RunJobs();
            stopwatch.Stop();

            output.WriteLine(
                "Measured {0}x{0}: {1:F2} ms for one staged click; {2} visual children.",
                channelCount,
                stopwatch.Elapsed.TotalMilliseconds,
                matrix.Children.Count);
            Assert.True(
                stopwatch.Elapsed < TimeSpan.FromMilliseconds(500),
                $"Le clic {channelCount}x{channelCount} a pris {stopwatch.Elapsed.TotalMilliseconds:F2} ms.");
            Assert.Equal(matrixBuildCount, dialog.MatrixBuildCount);
            Assert.Single(dialog.Edits);
            Assert.False(project.IsModified);

            if (channelCount == 128)
            {
                ScrollViewer body = dialog.FindControl<ScrollViewer>("MatrixBodyScrollViewer")!;
                ScrollViewer txHeaders = dialog.FindControl<ScrollViewer>("MatrixTxHeaderScrollViewer")!;
                ScrollViewer rxHeaders = dialog.FindControl<ScrollViewer>("MatrixRxHeaderScrollViewer")!;
                body.Offset = new Vector(184, 76);
                Dispatcher.UIThread.RunJobs();

                Assert.Equal(body.Offset.X, txHeaders.Offset.X);
                Assert.Equal(body.Offset.Y, rxHeaders.Offset.Y);
                Assert.Equal(0, txHeaders.Offset.Y);
                Assert.Equal(0, rxHeaders.Offset.X);
            }
        }
        finally
        {
            dialog.Close();
            File.Delete(path);
        }
    }

    private void MeasureClickSeries(int channelCount, int clickCount)
    {
        string path = Path.Combine(Path.GetTempPath(), $"dce-matrix-series-{channelCount}-{Guid.NewGuid():N}.xml");
        SyntheticPresetFactory.Create(path, deviceCount: 2, txPerDevice: channelCount, rxPerDevice: channelCount);
        DanteProject project = DanteProject.Load(path);
        PatchWorkspaceDialog dialog = new(
            UiLanguage.English,
            project,
            initialTxDeviceName: "DEVICE-001",
            initialRxDeviceName: "DEVICE-002");
        dialog.Show();

        try
        {
            Dispatcher.UIThread.RunJobs();
            Grid matrix = dialog.FindControl<Grid>("MatrixPanel")!;
            Button[] activeCells = matrix.Children
                .OfType<Button>()
                .Where(button => Equals(button.Content, "●"))
                .Take(clickCount)
                .ToArray();
            Assert.Equal(clickCount, activeCells.Length);
            int matrixBuildCount = dialog.MatrixBuildCount;

            Stopwatch stopwatch = Stopwatch.StartNew();
            foreach (Button activeCell in activeCells)
            {
                activeCell.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
            Dispatcher.UIThread.RunJobs();
            stopwatch.Stop();

            output.WriteLine(
                "Measured {0} staged clicks in {1}x{1}: {2:F2} ms total ({3:F2} ms/click).",
                clickCount,
                channelCount,
                stopwatch.Elapsed.TotalMilliseconds,
                stopwatch.Elapsed.TotalMilliseconds / clickCount);
            Assert.True(
                stopwatch.Elapsed < TimeSpan.FromSeconds(2),
                $"{clickCount} clics ont pris {stopwatch.Elapsed.TotalMilliseconds:F2} ms.");
            Assert.Equal(matrixBuildCount, dialog.MatrixBuildCount);
            Assert.Equal(clickCount, dialog.Edits.Count);
            Assert.False(project.IsModified);
        }
        finally
        {
            dialog.Close();
            File.Delete(path);
        }
    }
}
