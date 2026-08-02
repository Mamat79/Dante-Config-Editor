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
        Assert.Contains(
            "<ControlTemplate TargetType=\"{x:Type TabItem}\">",
            styles,
            StringComparison.Ordinal);
        Assert.Contains(
            "TextElement.Foreground=\"{TemplateBinding Foreground}\"",
            styles,
            StringComparison.Ordinal);
        Assert.Contains(
            "<Setter Property=\"Foreground\" Value=\"White\" />",
            styles,
            StringComparison.Ordinal);
        Assert.Contains(
            "<Style TargetType=\"{x:Type RadioButton}\">",
            styles,
            StringComparison.Ordinal);
    }

    [Fact]
    public void MachineChannelEditorsPutRxBeforeTxAndUseTheDialogTheme()
    {
        string template = File.ReadAllText(RepositoryFile("MachineTemplateEditorWindow.xaml"));
        string details = File.ReadAllText(RepositoryFile("DeviceDetailsWindow.xaml"));
        string detailsCode = File.ReadAllText(RepositoryFile("DeviceDetailsWindow.xaml.cs"));
        string macTemplate = File.ReadAllText(RepositoryFile(
            "src",
            "DanteConfigEditor.Mac",
            "MachineTemplateEditorDialog.axaml"));
        string macDetails = File.ReadAllText(RepositoryFile(
            "src",
            "DanteConfigEditor.Mac",
            "DeviceDetailsDialog.axaml"));
        string clone = File.ReadAllText(RepositoryFile("MachineCloneWindow.xaml"));
        string instance = File.ReadAllText(RepositoryFile("MachineInstanceWindow.xaml"));
        string macClone = File.ReadAllText(RepositoryFile(
            "src",
            "DanteConfigEditor.Mac",
            "MachineCloneDialog.axaml"));
        string macInstance = File.ReadAllText(RepositoryFile(
            "src",
            "DanteConfigEditor.Mac",
            "MachineInstanceDialog.axaml"));

        Assert.True(
            template.IndexOf("x:Name=\"RxTabItem\"", StringComparison.Ordinal)
            < template.IndexOf("x:Name=\"TxTabItem\"", StringComparison.Ordinal));
        Assert.True(
            details.IndexOf("x:Name=\"RxChannelsTab\"", StringComparison.Ordinal)
            < details.IndexOf("x:Name=\"TxChannelsTab\"", StringComparison.Ordinal));
        Assert.Contains("Resources/DialogStyles.xaml", details, StringComparison.Ordinal);
        Assert.Contains(
            "BasedOn=\"{StaticResource DialogLabelBaseStyle}\"",
            details,
            StringComparison.Ordinal);
        Assert.Contains(
            "BasedOn=\"{StaticResource DialogButtonBaseStyle}\"",
            details,
            StringComparison.Ordinal);
        Assert.Contains("DialogThemeService.Apply(this, useLightTheme)", detailsCode, StringComparison.Ordinal);
        Assert.True(
            macTemplate.IndexOf("x:Name=\"RxHeading\"", StringComparison.Ordinal)
            < macTemplate.IndexOf("x:Name=\"TxHeading\"", StringComparison.Ordinal));
        Assert.True(
            macDetails.IndexOf("x:Name=\"RxTab\"", StringComparison.Ordinal)
            < macDetails.IndexOf("x:Name=\"TxTab\"", StringComparison.Ordinal));
        Assert.True(
            clone.IndexOf("PreserveRxLabelsCheckBox", StringComparison.Ordinal)
            < clone.IndexOf("PreserveTxLabelsCheckBox", StringComparison.Ordinal));
        Assert.True(
            instance.IndexOf("x:Name=\"RxGroupBox\"", StringComparison.Ordinal)
            < instance.IndexOf("x:Name=\"TxGroupBox\"", StringComparison.Ordinal));
        Assert.True(
            macClone.IndexOf("PreserveRxLabelsCheckBox", StringComparison.Ordinal)
            < macClone.IndexOf("PreserveTxLabelsCheckBox", StringComparison.Ordinal));
        Assert.True(
            macInstance.IndexOf("UseRxLabelsCheckBox", StringComparison.Ordinal)
            < macInstance.IndexOf("UseTxLabelsCheckBox", StringComparison.Ordinal));
    }

    [Fact]
    public void MachineBankActionsShareOneResponsiveWrapPanel()
    {
        XDocument document = XDocument.Parse(
            File.ReadAllText(RepositoryFile("MachineBankWindow.xaml")));
        XNamespace xamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";

        XElement templateActions = NamedElement(document, xamlNamespace, "TemplateActionsPanel");
        XElement bankActions = NamedElement(document, xamlNamespace, "BankActionsPanel");
        XElement? responsiveActions = templateActions.Parent;

        Assert.Null(templateActions.Attribute("Grid.Row"));
        Assert.Null(bankActions.Attribute("Grid.Row"));
        Assert.NotNull(responsiveActions);
        Assert.Equal("WrapPanel", responsiveActions.Name.LocalName);
        Assert.Same(responsiveActions, bankActions.Parent);
        Assert.Equal("4", responsiveActions.Attribute("Grid.Row")?.Value);
    }

    [Fact]
    public void MachineBankOffersGithubExchangeOnWindowsAndMac()
    {
        string windowsXaml = File.ReadAllText(RepositoryFile("MachineBankWindow.xaml"));
        string windowsCode = File.ReadAllText(RepositoryFile("MachineBankWindow.xaml.cs"));
        string macXaml = File.ReadAllText(RepositoryFile(
            "src",
            "DanteConfigEditor.Mac",
            "MachineBankDialog.axaml"));
        string macCode = File.ReadAllText(RepositoryFile(
            "src",
            "DanteConfigEditor.Mac",
            "MachineBankDialog.axaml.cs"));

        Assert.Contains("x:Name=\"GithubBanksButton\"", windowsXaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"UpdateBanksButton\"", windowsXaml, StringComparison.Ordinal);
        Assert.Contains("Exporter la banque", windowsCode, StringComparison.Ordinal);
        Assert.Contains("Importer une banque", windowsCode, StringComparison.Ordinal);
        Assert.Contains("MachineBankDistributionService.GitHubBanksUrl", windowsCode, StringComparison.Ordinal);
        Assert.Contains("MachineBankOnlineUpdateService", windowsCode, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"GithubBanksButton\"", macXaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"UpdateBanksButton\"", macXaml, StringComparison.Ordinal);
        Assert.Contains("MachineBankDistributionService.GitHubBanksUrl", macCode, StringComparison.Ordinal);
        Assert.Contains("MachineBankOnlineUpdateService", macCode, StringComparison.Ordinal);
    }

    [Fact]
    public void MachineBankShowsAllAvailableBanksOnWindowsAndMac()
    {
        string windowsXaml = File.ReadAllText(RepositoryFile("MachineBankWindow.xaml"));
        string windowsCode = File.ReadAllText(RepositoryFile("MachineBankWindow.xaml.cs"));
        string macXaml = File.ReadAllText(RepositoryFile(
            "src",
            "DanteConfigEditor.Mac",
            "MachineBankDialog.axaml"));
        string macCode = File.ReadAllText(RepositoryFile(
            "src",
            "DanteConfigEditor.Mac",
            "MachineBankDialog.axaml.cs"));

        Assert.Contains("x:Name=\"BankSourceComboBox\"", windowsXaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"BankColumn\"", windowsXaml, StringComparison.Ordinal);
        Assert.Contains("MachineBankCatalogService.Load", windowsCode, StringComparison.Ordinal);
        Assert.Contains("_catalog.UniqueEntries", windowsCode, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"BankSourceComboBox\"", macXaml, StringComparison.Ordinal);
        Assert.Contains("Binding=\"{Binding BankName}\"", macXaml, StringComparison.Ordinal);
        Assert.Contains("MachineBankCatalogService.Load", macCode, StringComparison.Ordinal);
        Assert.Contains("_catalog.UniqueEntries", macCode, StringComparison.Ordinal);
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
