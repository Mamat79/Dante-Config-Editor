namespace DanteConfigEditorV3.Tests;

public sealed class ApplicationMenuUiContractTests
{
    [Fact]
    public void WindowsAndMacExposeTheSameTopLevelApplicationMenus()
    {
        string windowsXaml = File.ReadAllText(RepositoryFile("MainWindow.xaml"));
        string macXaml = File.ReadAllText(
            RepositoryFile("src", "DanteConfigEditor.Mac", "MainWindow.axaml"));

        foreach (string header in new[]
                 {
                     "Fichier",
                     "Édition",
                     "Machines",
                     "Affichage",
                     "Outils",
                     "Aide"
                 })
        {
            Assert.Contains($"Header=\"{header}\"", windowsXaml, StringComparison.Ordinal);
            Assert.Contains($"Header=\"{header}\"", macXaml, StringComparison.Ordinal);
        }

        Assert.Contains("x:Name=\"InspectorMenuItem\"", windowsXaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"InspectorRevealButton\"", windowsXaml, StringComparison.Ordinal);
        Assert.Contains("Header=\"À propos de DCE\"", windowsXaml, StringComparison.Ordinal);
        Assert.Contains("Header=\"À propos de DCE\"", macXaml, StringComparison.Ordinal);
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
