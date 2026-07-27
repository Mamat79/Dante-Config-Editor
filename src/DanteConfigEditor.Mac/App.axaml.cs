using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace DanteConfigEditor.Mac;

public partial class App : Avalonia.Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            MainWindow window = new();
            desktop.MainWindow = window;

            // macOS transmet le fichier ouvert depuis Finder dans les arguments.
            string? startupFile = desktop.Args?.FirstOrDefault(File.Exists);
            if (startupFile is not null)
            {
                window.Opened += async (_, _) => await window.OpenStartupFileAsync(startupFile);
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}
