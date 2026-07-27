using System.Xml.Linq;

namespace DanteConfigEditorV3.Tests;

public sealed class ExportWorkspaceUiContractTests
{
    [Theory]
    [InlineData("MainWindow.xaml")]
    [InlineData("src/DanteConfigEditor.Mac/MainWindow.axaml")]
    public void NormalExportsAreGroupedOutsideSafetyAndLog(string relativePath)
    {
        string xaml = File.ReadAllText(RepositoryFile(relativePath));
        XDocument document = XDocument.Parse(xaml);
        XNamespace xamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";

        XElement exports = NamedElement(document, xamlNamespace, "ExportsTab");
        XElement reports = NamedElement(document, xamlNamespace, "ReportsExportTab");
        XElement synoptic = NamedElement(document, xamlNamespace, "SynopticTab");
        XElement safety = NamedElement(document, xamlNamespace, "SafetyTab");

        Assert.Equal("Import / Export", exports.Attribute("Header")?.Value);
        Assert.Equal("Rapports et patchbook", reports.Attribute("Header")?.Value);
        Assert.Equal("Synoptique", synoptic.Attribute("Header")?.Value);

        string synopticMarkup = synoptic.ToString(SaveOptions.DisableFormatting);
        Assert.Contains("ExportSynopticPdfButton", synopticMarkup, StringComparison.Ordinal);
        Assert.Contains("Exporter le synoptique PDF", synopticMarkup, StringComparison.Ordinal);
        Assert.Contains("SynopticVisibilityCheckBox_Click", synopticMarkup, StringComparison.Ordinal);
        Assert.Contains("SynopticLocation", synopticMarkup, StringComparison.Ordinal);

        string reportsMarkup = reports.ToString(SaveOptions.DisableFormatting);
        Assert.Contains("Exporter TXT", reportsMarkup, StringComparison.Ordinal);
        Assert.Contains("Exporter PDF", reportsMarkup, StringComparison.Ordinal);
        Assert.Contains("Patchbook TXT", reportsMarkup, StringComparison.Ordinal);
        Assert.Contains("Patchbook CSV", reportsMarkup, StringComparison.Ordinal);
        Assert.Contains("Topologie simple", reportsMarkup, StringComparison.Ordinal);

        string safetyMarkup = safety.ToString(SaveOptions.DisableFormatting);
        Assert.DoesNotContain("Exporter TXT", safetyMarkup, StringComparison.Ordinal);
        Assert.DoesNotContain("Exporter PDF", safetyMarkup, StringComparison.Ordinal);
        Assert.DoesNotContain("Patchbook TXT", safetyMarkup, StringComparison.Ordinal);
        Assert.DoesNotContain("Patchbook CSV", safetyMarkup, StringComparison.Ordinal);
        Assert.DoesNotContain("Topologie simple", safetyMarkup, StringComparison.Ordinal);
    }

    [Fact]
    public void LabelExchangeRemainsInItsDedicatedImportExportTab()
    {
        foreach (string relativePath in new[] { "MainWindow.xaml", "src/DanteConfigEditor.Mac/MainWindow.axaml" })
        {
            XDocument document = XDocument.Parse(File.ReadAllText(RepositoryFile(relativePath)));
            XNamespace xamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";
            XElement labels = NamedElement(document, xamlNamespace, "ChannelLabelsTab");
            XElement exports = NamedElement(document, xamlNamespace, "ExportsTab");

            Assert.Contains(labels.Ancestors(), ancestor => ReferenceEquals(ancestor, exports));
            Assert.Equal("Labels", labels.Attribute("Header")?.Value);
        }
    }

    [Fact]
    public void WindowsSynopticExposesSelectionFilteringAndPatchNavigation()
    {
        string xaml = File.ReadAllText(RepositoryFile("MainWindow.xaml"));
        string codeBehind = File.ReadAllText(
            RepositoryFile("MainWindow.Synoptic.cs"));

        Assert.Contains(
            "x:Name=\"SynopticLocationFilterComboBox\"",
            xaml,
            StringComparison.Ordinal);
        Assert.Contains(
            "x:Name=\"SynopticSelectionPanel\"",
            xaml,
            StringComparison.Ordinal);
        Assert.Contains(
            "x:Name=\"SynopticSelectionActionButton\"",
            xaml,
            StringComparison.Ordinal);
        Assert.Contains(
            "PreviewMouseDown=\"SynopticScrollViewer_PreviewMouseDown\"",
            xaml,
            StringComparison.Ordinal);
        Assert.Contains(
            "CenterSynopticSelectionButton_Click",
            xaml,
            StringComparison.Ordinal);
        Assert.Contains(
            "SelectSynopticCableForSubscription",
            codeBehind,
            StringComparison.Ordinal);
        Assert.Contains(
            "ProjectEntityKind.SynopticLink",
            codeBehind,
            StringComparison.Ordinal);
        Assert.Contains(
            "OpenSynopticCableInPatch",
            codeBehind,
            StringComparison.Ordinal);
    }

    private static XElement NamedElement(XDocument document, XNamespace xamlNamespace, string name)
    {
        return document.Descendants()
            .Single(element => string.Equals(element.Attribute(xamlNamespace + "Name")?.Value, name, StringComparison.Ordinal));
    }

    private static string RepositoryFile(string relativePath)
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "DanteConfigEditorV3.csproj")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);
        return Path.Combine(directory!.FullName, relativePath.Replace('/', Path.DirectorySeparatorChar));
    }
}
