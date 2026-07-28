using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Linq;
using DanteConfigEditor;
using DanteConfigEditor.Models;
using DanteConfigEditor.Services;
using WpfApplication = System.Windows.Application;

namespace DanteConfigEditor.ScreenshotTool;

internal static class Program
{
    private const int DefaultCaptureWidth = 1920;
    private const int DefaultCaptureHeight = 1024;
    private static int _captureWidth = DefaultCaptureWidth;
    private static int _captureHeight = DefaultCaptureHeight;
    private static bool _captureDarkTheme;

    [STAThread]
    private static int Main(string[] args)
    {
        string repositoryRoot = FindRepositoryRoot();
        string outputRoot = args.Length > 0
            ? Path.GetFullPath(args[0])
            : Path.Combine(repositoryRoot, "docs", "media", "2026.1");
        using LocalProfileFileSnapshot profileSnapshot = new(
            ApplicationStoragePaths.RootPath,
            "language.txt",
            "theme.txt",
            "recent-files.txt",
            "support-reminder.json");
        if (args.Length > 2
            && int.TryParse(args[1], out int requestedWidth)
            && int.TryParse(args[2], out int requestedHeight)
            && requestedWidth >= 1120
            && requestedHeight >= 720)
        {
            _captureWidth = requestedWidth;
            _captureHeight = requestedHeight;
        }
        _captureDarkTheme = args.Length > 3
            && string.Equals(args[3], "dark", StringComparison.OrdinalIgnoreCase);
        string fixture = Path.Combine(
            repositoryRoot,
            "tests",
            "DanteConfigEditorV3.Tests",
            "Fixtures",
            "representative-preset.xml");
        string publicBank = Path.Combine(
            repositoryRoot,
            "Resources",
            "MachineBanks",
            "Bundled",
            "DCE Community Devices 2026.1");

        Directory.CreateDirectory(outputRoot);
        WpfApplication application = new()
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown
        };
        application.Resources.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = new Uri(
                "pack://application:,,,/DanteConfigEditorV3;component/Resources/DesignSystem2026.xaml",
                UriKind.Absolute)
        });

        foreach ((UiLanguage language, string folder) in new[]
                 {
                     (UiLanguage.French, "fr"),
                     (UiLanguage.English, "en")
                 })
        {
            string languageOutput = Path.Combine(outputRoot, folder);
            Directory.CreateDirectory(languageOutput);
            using TemporaryPreset temporaryPreset = new(fixture, language);
            CaptureMainWindow(temporaryPreset.Path, language, languageOutput);
            CaptureNewProject(language, languageOutput);
            CaptureBank(publicBank, language, languageOutput);
            CaptureSupport(language, languageOutput);
        }

        application.Shutdown();
        return 0;
    }

    private static void CaptureMainWindow(
        string fixturePath,
        UiLanguage language,
        string outputDirectory)
    {
        MainWindow window = new()
        {
            WindowStartupLocation = WindowStartupLocation.Manual,
            WindowState = WindowState.Normal,
            Width = _captureWidth,
            Height = _captureHeight,
            Left = -10000,
            Top = 0,
            ShowInTaskbar = false
        };

        window.Show();
        PumpDispatcher();
        ((ToggleButton)window.FindName("ThemeToggleButton")).IsChecked = !_captureDarkTheme;
        PumpDispatcher();
        SetLanguage(window, language);
        window.LoadProjectFromPath(fixturePath);
        PumpDispatcher();
        SetSyntheticPath(window, language);

        Select(window, "OverviewNavigationButton");
        Capture(window, Path.Combine(outputDirectory, "overview.png"));

        Select(window, "MachinesNavigationButton");
        Capture(window, Path.Combine(outputDirectory, "devices.png"));
        InvokePrivate(window, "SetNavigationExpanded", [false]);
        InvokePrivate(window, "SetInspectorExpanded", [false]);
        Capture(window, Path.Combine(outputDirectory, "devices-collapsed-sidebars.png"));
        InvokePrivate(window, "SetNavigationExpanded", [true]);
        InvokePrivate(window, "SetInspectorExpanded", [true]);

        Select(window, "PatchNavigationButton");
        Select(window, "PatchMatrixModeButton");
        Capture(window, Path.Combine(outputDirectory, "patch.png"));

        Select(window, "PatchEasyModeButton");
        Capture(window, Path.Combine(outputDirectory, "easy-patch.png"));

        Select(window, "PatchListModeButton");
        Capture(window, Path.Combine(outputDirectory, "patch-list.png"));

        Select(window, "ImportExportNavigationButton");
        SelectTab(window, "ChannelLabelsTab");
        Capture(window, Path.Combine(outputDirectory, "labels.png"));

        SelectTab(window, "SynopticTab");
        Capture(window, Path.Combine(outputDirectory, "synoptic.png"));

        Select(window, "ValidationNavigationButton");
        Capture(window, Path.Combine(outputDirectory, "validation.png"));

        Select(window, "AdvancedToolsNavigationButton");
        Capture(window, Path.Combine(outputDirectory, "atomic-bomb.png"));

        window.Close();
        PumpDispatcher();
    }

    private static void CaptureBank(
        string bankPath,
        UiLanguage language,
        string outputDirectory)
    {
        MachineBankWindow window = new(
            language,
            useLightTheme: !_captureDarkTheme,
            usedDeviceNames: Array.Empty<string>(),
            canAddToProject: true)
        {
            WindowStartupLocation = WindowStartupLocation.Manual,
            Width = 1500,
            Height = 900,
            Left = -10000,
            Top = 0,
            ShowInTaskbar = false
        };

        SetPrivateField(window, "_bankPath", bankPath);
        SetPrivateField(window, "_repository", new MachineBankRepository(bankPath));
        InvokePrivate(window, "RefreshBank", [null]);
        ((TextBlock)window.FindName("BankPathTextBlock")).Text = language == UiLanguage.English
            ? "Bundled sanitized demonstration bank"
            : "Banque de démonstration assainie incluse";

        window.Show();
        PumpDispatcher();
        Capture(window, Path.Combine(outputDirectory, "device-bank.png"));
        window.Close();
        PumpDispatcher();
    }

    private static void CaptureNewProject(UiLanguage language, string outputDirectory)
    {
        NewProjectWindow window = new(language, useLightTheme: !_captureDarkTheme)
        {
            WindowStartupLocation = WindowStartupLocation.Manual,
            Width = 920,
            Height = 660,
            Left = 0,
            Top = 0,
            ShowInTaskbar = false
        };

        window.Show();
        PumpDispatcher();
        CaptureWholeWindow(window, Path.Combine(outputDirectory, "new-project.png"));
        window.Close();
        PumpDispatcher();
    }

    private static void CaptureWholeWindow(Window window, string outputPath)
    {
        window.UpdateLayout();
        PumpDispatcher();

        int width = Math.Max(1, (int)Math.Ceiling(window.ActualWidth));
        int height = Math.Max(1, (int)Math.Ceiling(window.ActualHeight));
        RenderTargetBitmap bitmap = new(
            width,
            height,
            96,
            96,
            PixelFormats.Pbgra32);
        bitmap.Render(window);

        PngBitmapEncoder encoder = new();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using FileStream output = File.Create(outputPath);
        encoder.Save(output);
    }

    private static void CaptureSupport(UiLanguage language, string outputDirectory)
    {
        SupportDceWindow window = new(language, useLightTheme: !_captureDarkTheme)
        {
            WindowStartupLocation = WindowStartupLocation.Manual,
            Width = 920,
            Height = 700,
            Left = -10000,
            Top = 0,
            ShowInTaskbar = false
        };
        string qrPath = Path.Combine(
            FindRepositoryRoot(),
            "Resources",
            "Support",
            "paypal-support-qr.png");
        BitmapImage qrCode = new();
        qrCode.BeginInit();
        qrCode.CacheOption = BitmapCacheOption.OnLoad;
        qrCode.UriSource = new Uri(qrPath, UriKind.Absolute);
        qrCode.EndInit();
        qrCode.Freeze();
        ((Image)window.FindName("PayPalQrImage")).Source = qrCode;

        window.Show();
        PumpDispatcher();
        Capture(window, Path.Combine(outputDirectory, "support.png"));
        window.Close();
        PumpDispatcher();
    }

    private static void SetLanguage(MainWindow window, UiLanguage language)
    {
        ComboBox combo = (ComboBox)window.FindName("LanguageComboBox");
        combo.SelectedIndex = language == UiLanguage.English ? 1 : 0;
        PumpDispatcher();
    }

    private static void SetSyntheticPath(MainWindow window, UiLanguage language)
    {
        ((TextBlock)window.FindName("FilePathTextBlock")).Text =
            language == UiLanguage.English
                ? "Synthetic demonstration preset"
                : "Preset synthétique de démonstration";
    }

    private static void Select(MainWindow window, string controlName)
    {
        if (window.FindName(controlName) is RadioButton radioButton)
        {
            radioButton.IsChecked = true;
        }
        else
        {
            throw new InvalidOperationException($"Contrôle introuvable ou non sélectionnable : {controlName}");
        }

        PumpDispatcher();
    }

    private static void SelectTab(MainWindow window, string controlName)
    {
        if (window.FindName(controlName) is not TabItem tab)
        {
            throw new InvalidOperationException($"Onglet introuvable : {controlName}");
        }

        tab.IsSelected = true;
        PumpDispatcher();
    }

    private static void Capture(Window window, string outputPath)
    {
        window.UpdateLayout();
        PumpDispatcher();

        FrameworkElement visual = window.Content as FrameworkElement ?? window;
        visual.UpdateLayout();

        int width = Math.Max(1, (int)Math.Ceiling(visual.ActualWidth));
        int height = Math.Max(1, (int)Math.Ceiling(visual.ActualHeight));
        RenderTargetBitmap bitmap = new(
            width,
            height,
            96,
            96,
            PixelFormats.Pbgra32);
        bitmap.Render(visual);

        PngBitmapEncoder encoder = new();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using FileStream output = File.Create(outputPath);
        encoder.Save(output);
    }

    private static void SetPrivateField(object instance, string name, object value)
    {
        FieldInfo field = instance.GetType().GetField(
            name,
            BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingFieldException(instance.GetType().FullName, name);
        field.SetValue(instance, value);
    }

    private static void InvokePrivate(object instance, string name, params object?[] arguments)
    {
        MethodInfo method = instance.GetType().GetMethod(
            name,
            BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingMethodException(instance.GetType().FullName, name);
        method.Invoke(instance, arguments);
    }

    private static void PumpDispatcher()
    {
        DispatcherFrame frame = new();
        Dispatcher.CurrentDispatcher.BeginInvoke(
            DispatcherPriority.ApplicationIdle,
            new DispatcherOperationCallback(state =>
            {
                ((DispatcherFrame)state!).Continue = false;
                return null;
            }),
            frame);
        Dispatcher.PushFrame(frame);
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null
               && !File.Exists(Path.Combine(directory.FullName, "DanteConfigEditorV3.csproj")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new DirectoryNotFoundException("Racine du dépôt Dante Config Editor introuvable.");
    }

    private sealed class TemporaryPreset : IDisposable
    {
        public TemporaryPreset(string source, UiLanguage language)
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"dce-docs-{Guid.NewGuid():N}.xml");
            XDocument document = XDocument.Load(
                source,
                LoadOptions.PreserveWhitespace);
            if (language == UiLanguage.English && document.Root is not null)
            {
                XElement? name = document.Root
                    .Elements()
                    .FirstOrDefault(element =>
                        element.Name.LocalName == "name");
                XElement? description = document.Root
                    .Elements()
                    .FirstOrDefault(element =>
                        element.Name.LocalName == "description");
                if (name is not null)
                {
                    name.Value = "Synthetic demonstration preset";
                }
                if (description is not null)
                {
                    description.Value =
                        "Representative Dante Controller structure without production data.";
                }
            }
            document.Save(Path, SaveOptions.DisableFormatting);
        }

        public string Path { get; }

        public void Dispose()
        {
            SessionRecoveryService.Delete(Path);
            File.Delete(Path);
        }
    }

    private sealed class LocalProfileFileSnapshot : IDisposable
    {
        private readonly Dictionary<string, byte[]?> _originalFiles;

        public LocalProfileFileSnapshot(string rootPath, params string[] fileNames)
        {
            _originalFiles = fileNames
                .Select(fileName => Path.Combine(rootPath, fileName))
                .ToDictionary(
                path => path,
                path => File.Exists(path) ? File.ReadAllBytes(path) : null,
                StringComparer.OrdinalIgnoreCase);
        }

        public void Dispose()
        {
            foreach ((string path, byte[]? content) in _originalFiles)
            {
                if (content is null)
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }

                    continue;
                }

                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.WriteAllBytes(path, content);
            }
        }
    }
}
