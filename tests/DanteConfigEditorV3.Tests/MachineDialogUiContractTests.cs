using System.Xml.Linq;

namespace DanteConfigEditorV3.Tests;

public sealed class MachineDialogUiContractTests
{
    [Fact]
    public void DialogThemeExplicitlyColorsItsOwnWindow()
    {
        string service = File.ReadAllText(RepositoryFile("DialogThemeService.cs"));

        Assert.Contains(
            "window.SetResourceReference(Control.BackgroundProperty, \"DialogWindowBrush\")",
            service,
            StringComparison.Ordinal);
        Assert.Contains(
            "window.SetResourceReference(Control.ForegroundProperty, \"DialogTextBrush\")",
            service,
            StringComparison.Ordinal);
    }

    [Fact]
    public void SecondaryButtonsKeepSharedSpacingAndDarkThemeListsStayReadable()
    {
        string styles = File.ReadAllText(RepositoryFile("Resources", "DialogStyles.xaml"));

        Assert.Contains(
            "BasedOn=\"{StaticResource {x:Type Button}}\"",
            styles,
            StringComparison.Ordinal);
        Assert.Contains(
            "<Style TargetType=\"{x:Type DataGridRow}\">",
            styles,
            StringComparison.Ordinal);
        Assert.Contains(
            "<Style TargetType=\"{x:Type DataGridCell}\">",
            styles,
            StringComparison.Ordinal);
        Assert.Contains(
            "<Style TargetType=\"{x:Type TabControl}\">",
            styles,
            StringComparison.Ordinal);
        Assert.Contains(
            "<Style TargetType=\"{x:Type ToolTip}\">",
            styles,
            StringComparison.Ordinal);
    }

    [Fact]
    public void MachineBankActionsUseTwoResponsiveRows()
    {
        XDocument document = XDocument.Parse(
            File.ReadAllText(RepositoryFile("MachineBankWindow.xaml")));
        XNamespace xamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";

        XElement templateActions = NamedElement(document, xamlNamespace, "TemplateActionsPanel");
        XElement bankActions = NamedElement(document, xamlNamespace, "BankActionsPanel");

        Assert.Null(templateActions.Attribute("Grid.Row"));
        Assert.Equal("1", bankActions.Attribute("Grid.Row")?.Value);
    }

    [Fact]
    public void TemplateMetadataUsesExplicitFrenchAndEnglishHelp()
    {
        string code = File.ReadAllText(RepositoryFile("MachineTemplateEditorWindow.xaml.cs"));

        Assert.Contains("Nom dans la banque", code, StringComparison.Ordinal);
        Assert.Contains("Name in the bank", code, StringComparison.Ordinal);
        Assert.Contains("Modèle matériel", code, StringComparison.Ordinal);
        Assert.Contains("Hardware model", code, StringComparison.Ordinal);
        Assert.Contains("Mots-clés", code, StringComparison.Ordinal);
        Assert.Contains("TemplateNameTextBox.ToolTip", code, StringComparison.Ordinal);
        Assert.Contains("CategoryTextBox.ToolTip", code, StringComparison.Ordinal);

        Assert.Contains(
            "PreserveNetworkCheckBox.ToolTip",
            File.ReadAllText(RepositoryFile("MachineCloneWindow.xaml.cs")),
            StringComparison.Ordinal);
        Assert.Contains(
            "UseTxLabelsCheckBox.ToolTip",
            File.ReadAllText(RepositoryFile("MachineInstanceWindow.xaml.cs")),
            StringComparison.Ordinal);
        Assert.Contains(
            "BackupBankButton.ToolTip",
            File.ReadAllText(RepositoryFile("MachineBankWindow.xaml.cs")),
            StringComparison.Ordinal);
    }

    private static XElement NamedElement(
        XDocument document,
        XNamespace xamlNamespace,
        string name)
    {
        return document.Descendants()
            .Single(element =>
                string.Equals(
                    (string?)element.Attribute(xamlNamespace + "Name"),
                    name,
                    StringComparison.Ordinal));
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
