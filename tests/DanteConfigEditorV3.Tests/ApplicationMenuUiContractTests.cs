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

    [Fact]
    public void ProductNameStaysPrimaryAndBrandSignatureStaysInFooter()
    {
        string windowsXaml = File.ReadAllText(RepositoryFile("MainWindow.xaml"));
        string macXaml = File.ReadAllText(
            RepositoryFile("src", "DanteConfigEditor.Mac", "MainWindow.axaml"));

        foreach (string xaml in new[] { windowsXaml, macXaml })
        {
            Assert.Contains("Title=\"Dante Config Editor\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Name=\"ProductTitleTextBlock\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Text=\"Dante Config Editor\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Text=\"Dante Config Editor 2026.1.1\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Name=\"BrandSignaturePanel\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Text=\"SiLeMI/O\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Text=\"By Mamat\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Text=\"-------[]--\"", xaml, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void DarkThemeMenusOwnTheirForegroundAndBackgroundColours()
    {
        string windowsXaml = File.ReadAllText(RepositoryFile("MainWindow.xaml"));
        string windowsCode = File.ReadAllText(RepositoryFile("MainWindow.xaml.cs"));
        string macStyles = File.ReadAllText(
            RepositoryFile("src", "DanteConfigEditor.Mac", "App.axaml"));

        Assert.Contains("x:Key=\"MenuPopupBrush\"", windowsXaml, StringComparison.Ordinal);
        Assert.Contains("SystemColors.MenuBrushKey", windowsXaml, StringComparison.Ordinal);
        Assert.Contains("Property=\"IsHighlighted\"", windowsXaml, StringComparison.Ordinal);
        Assert.Contains("SetMenuBrushes(", windowsCode, StringComparison.Ordinal);
        Assert.Contains("SystemColors.MenuTextBrushKey", windowsCode, StringComparison.Ordinal);

        Assert.Contains("Style Selector=\"MenuItem\"", macStyles, StringComparison.Ordinal);
        Assert.Contains("Style Selector=\"MenuItem:pointerover\"", macStyles, StringComparison.Ordinal);
        Assert.Contains("Style Selector=\"MenuItem:disabled\"", macStyles, StringComparison.Ordinal);
        Assert.Contains("Style Selector=\"ContextMenu\"", macStyles, StringComparison.Ordinal);
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
