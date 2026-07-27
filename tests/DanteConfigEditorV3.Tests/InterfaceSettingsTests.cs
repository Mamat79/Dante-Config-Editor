using DanteConfigEditor.Services;

namespace DanteConfigEditorV3.Tests;

public sealed class InterfaceSettingsTests
{
    [Fact]
    public void ConfigurationEditorsAreExpandedOnFirstLaunchAndPreferencePersists()
    {
        string path = Path.Combine(Path.GetTempPath(), "DanteConfigEditorV3.Tests", Guid.NewGuid().ToString("N"), "configuration-editors.txt");

        Assert.True(InterfaceSettingsService.LoadConfigurationEditorsExpanded(path));

        InterfaceSettingsService.SaveConfigurationEditorsExpanded(false, path);
        Assert.False(InterfaceSettingsService.LoadConfigurationEditorsExpanded(path));

        InterfaceSettingsService.SaveConfigurationEditorsExpanded(true, path);
        Assert.True(InterfaceSettingsService.LoadConfigurationEditorsExpanded(path));
    }

    [Fact]
    public void WindowsMainWindowLoadsTheExpandedDefaultAndDoesNotHideItForDpi()
    {
        string mainWindow = File.ReadAllText(RepositoryFile("MainWindow.xaml.cs"));

        Assert.Contains(
            "InterfaceSettingsService.LoadConfigurationEditorsExpanded()",
            mainWindow,
            StringComparison.Ordinal);
        Assert.Contains(
            "Le facteur DPI ne doit jamais cacher les réglages au premier lancement.",
            mainWindow,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "ConfigurationEditorsGrid.Visibility = width",
            mainWindow,
            StringComparison.Ordinal);
    }

    private static string RepositoryFile(params string[] relativeParts)
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null
               && !File.Exists(Path.Combine(directory.FullName, "DanteConfigEditorV3.csproj")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);
        return Path.Combine([directory!.FullName, .. relativeParts]);
    }
}
