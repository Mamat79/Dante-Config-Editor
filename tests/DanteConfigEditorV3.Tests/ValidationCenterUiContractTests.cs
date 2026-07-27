using System.Xml.Linq;

namespace DanteConfigEditorV3.Tests;

public sealed class ValidationCenterUiContractTests
{
    [Fact]
    public void WindowsValidationCenterExposesFiltersDetailsNavigationAndReport()
    {
        string xaml = File.ReadAllText(RepositoryFile("MainWindow.xaml"));
        XDocument document = XDocument.Parse(xaml);
        XNamespace xamlNamespace =
            "http://schemas.microsoft.com/winfx/2006/xaml";
        XElement health = document.Descendants().Single(element =>
            element.Attribute(xamlNamespace + "Name")?.Value == "HealthTab");
        string markup = health.ToString(SaveOptions.DisableFormatting);
        string code = File.ReadAllText(
            RepositoryFile("MainWindow.Validation.cs"));

        Assert.Contains("HealthSearchTextBox", markup, StringComparison.Ordinal);
        Assert.Contains("ValidationErrorCountTextBlock", markup, StringComparison.Ordinal);
        Assert.Contains("ValidationDetailTextBlock", markup, StringComparison.Ordinal);
        Assert.Contains("OpenValidationTargetButton", markup, StringComparison.Ordinal);
        Assert.Contains("ExportValidationReportButton", markup, StringComparison.Ordinal);
        Assert.Contains("HealthIssuesGrid_MouseDoubleClick", markup, StringComparison.Ordinal);
        Assert.Contains("OpenValidationSubscription", code, StringComparison.Ordinal);
        Assert.Contains("BuildValidationReport", code, StringComparison.Ordinal);
        Assert.Contains("ProjectEntityKind.Subscription", code, StringComparison.Ordinal);
    }

    [Fact]
    public void ValidationCenterUsesStructuredSessionStateInsteadOfLegacyGridRows()
    {
        string mainCode = File.ReadAllText(
            RepositoryFile("MainWindow.xaml.cs"));
        string sessionCode = File.ReadAllText(RepositoryFile(
            "src",
            "DanteConfigEditor.Application",
            "ProjectSession.cs"));

        Assert.Contains(
            "ProjectValidationState validation = _projectSession.Validation",
            mainCode,
            StringComparison.Ordinal);
        Assert.Contains(
            "_validationService.Validate(Project, Profile)",
            sessionCode,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "IEnumerable<DanteValidationIssue> issues = validation.Issues",
            mainCode,
            StringComparison.Ordinal);
    }

    private static string RepositoryFile(params string[] relativeParts) =>
        Path.Combine([RepositoryDirectory(), .. relativeParts]);

    private static string RepositoryDirectory()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null
               && !File.Exists(Path.Combine(directory.FullName, "DanteConfigEditorV3.csproj")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);
        return directory!.FullName;
    }
}
