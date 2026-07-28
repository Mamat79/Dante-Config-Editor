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
    public void WindowsMainWindowAlwaysStartsExpandedAndDoesNotHidePanelsForDpi()
    {
        string mainWindow = File.ReadAllText(RepositoryFile("MainWindow.xaml.cs"));

        Assert.Contains(
            "ConfigurationEditorsGrid.Visibility = Visibility.Visible",
            mainWindow,
            StringComparison.Ordinal);
        Assert.Contains(
            "SetNavigationExpanded(true)",
            mainWindow,
            StringComparison.Ordinal);
        Assert.Contains(
            "SetInspectorExpanded(true)",
            mainWindow,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "if (width < 1400 && _inspectorExpanded)",
            mainWindow,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "if (width < 1160 && _navigationExpanded)",
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
