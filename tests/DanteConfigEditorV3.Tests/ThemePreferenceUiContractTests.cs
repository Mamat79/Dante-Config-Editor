namespace DanteConfigEditorV3.Tests;

public sealed class ThemePreferenceUiContractTests
{
    [Fact]
    public void WindowsAndMacUseTheSharedLightByDefaultThemePreference()
    {
        string service = File.ReadAllText(
            RepositoryFile("Services", "ThemeSettingsService.cs"));
        string windowsCode = File.ReadAllText(RepositoryFile("MainWindow.xaml.cs"));
        string macCode = File.ReadAllText(
            RepositoryFile("src", "DanteConfigEditor.Mac", "MainWindow.axaml.cs"));

        Assert.Contains("return true;", service, StringComparison.Ordinal);
        Assert.Contains("LoadUseLightTheme()", windowsCode, StringComparison.Ordinal);
        Assert.Contains("SaveUseLightTheme(useLightTheme: true)", windowsCode, StringComparison.Ordinal);
        Assert.Contains("SaveUseLightTheme(useLightTheme: false)", windowsCode, StringComparison.Ordinal);
        Assert.Contains("!ThemeSettingsService.LoadUseLightTheme()", macCode, StringComparison.Ordinal);
        Assert.Contains("ThemeSettingsService.SaveUseLightTheme(!_darkTheme)", macCode, StringComparison.Ordinal);
        Assert.Contains("LanguageSettingsService.Load()", windowsCode, StringComparison.Ordinal);
        Assert.Contains("LanguageSettingsService.Load()", macCode, StringComparison.Ordinal);
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
