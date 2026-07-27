using System.Xml.Linq;
using DanteConfigEditor.Domain.Projects;

namespace DanteConfigEditorV3.Tests;

public sealed class ArchitectureBoundaryTests
{
    [Fact]
    public void WindowsAndMacShareTheSameCoreAssembly()
    {
        XDocument windowsProject = XDocument.Load(RepositoryFile("DanteConfigEditorV3.csproj"));
        XDocument macProject = XDocument.Load(
            RepositoryFile("src", "DanteConfigEditor.Mac", "DanteConfigEditor.Mac.csproj"));

        string[] windowsReferences = windowsProject
            .Descendants("ProjectReference")
            .Select(element => element.Attribute("Include")?.Value ?? string.Empty)
            .ToArray();
        string[] macReferences = macProject
            .Descendants("ProjectReference")
            .Select(element => element.Attribute("Include")?.Value ?? string.Empty)
            .ToArray();

        Assert.Contains(@"src\DanteConfigEditor.Core\DanteConfigEditor.Core.csproj", windowsReferences);
        Assert.Contains(@"..\DanteConfigEditor.Core\DanteConfigEditor.Core.csproj", macReferences);

        string[] removedSources = windowsProject
            .Descendants("Compile")
            .Select(element => element.Attribute("Remove")?.Value ?? string.Empty)
            .ToArray();
        Assert.Contains(@"Models\**\*.cs", removedSources);
        Assert.Contains(@"Services\**\*.cs", removedSources);
    }

    [Fact]
    public void DesktopViewsDoNotManipulateLinqToXmlDirectly()
    {
        string repository = RepositoryDirectory();
        IEnumerable<string> desktopSources = Directory
            .EnumerateFiles(repository, "*.cs", SearchOption.TopDirectoryOnly)
            .Where(path => path.EndsWith(".xaml.cs", StringComparison.OrdinalIgnoreCase))
            .Concat(Directory.EnumerateFiles(
                Path.Combine(repository, "src", "DanteConfigEditor.Mac"),
                "*.axaml.cs",
                SearchOption.TopDirectoryOnly));

        string[] forbiddenTokens =
        [
            "using System.Xml.Linq",
            "XDocument",
            "XElement",
            "XAttribute",
            "XNamespace"
        ];

        foreach (string sourcePath in desktopSources)
        {
            string source = File.ReadAllText(sourcePath);
            foreach (string token in forbiddenTokens)
            {
                Assert.False(
                    source.Contains(token, StringComparison.Ordinal),
                    $"XML access '{token}' found in {Path.GetRelativePath(repository, sourcePath)}");
            }
        }
    }

    [Fact]
    public void SharedCoreDoesNotReferenceDesktopUiFrameworks()
    {
        string project = File.ReadAllText(
            RepositoryFile("src", "DanteConfigEditor.Core", "DanteConfigEditor.Core.csproj"));
        Assert.DoesNotContain("<UseWPF>", project, StringComparison.Ordinal);
        Assert.DoesNotContain("Avalonia", project, StringComparison.Ordinal);

        string repository = RepositoryDirectory();
        IEnumerable<string> coreSources = Directory
            .EnumerateFiles(Path.Combine(repository, "Models"), "*.cs", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(
                Path.Combine(repository, "Services"),
                "*.cs",
                SearchOption.AllDirectories));

        foreach (string sourcePath in coreSources)
        {
            string source = File.ReadAllText(sourcePath);
            Assert.DoesNotContain("using System.Windows", source, StringComparison.Ordinal);
            Assert.DoesNotContain("using Avalonia", source, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void DomainAssemblyDoesNotReferenceXmlUiOrFileInfrastructure()
    {
        string[] referencedAssemblies = typeof(DanteXmlCapabilities).Assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .ToArray();

        Assert.DoesNotContain("System.Xml.XDocument", referencedAssemblies);
        Assert.DoesNotContain("PresentationFramework", referencedAssemblies);
        Assert.DoesNotContain("Avalonia", referencedAssemblies);

        string domainRoot = RepositoryFile("src", "DanteConfigEditor.Domain");
        IEnumerable<string> domainSources = Directory
            .EnumerateFiles(domainRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains(
                $"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}",
                StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains(
                $"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}",
                StringComparison.OrdinalIgnoreCase));

        foreach (string sourcePath in domainSources)
        {
            string source = File.ReadAllText(sourcePath);
            Assert.DoesNotContain("System.Xml", source, StringComparison.Ordinal);
            Assert.DoesNotContain("System.IO", source, StringComparison.Ordinal);
            Assert.DoesNotContain("System.Windows", source, StringComparison.Ordinal);
            Assert.DoesNotContain("Avalonia", source, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void ApplicationLayerDoesNotManipulateXmlOrFilesDirectly()
    {
        string applicationRoot = RepositoryFile("src", "DanteConfigEditor.Application");
        IEnumerable<string> applicationSources = Directory
            .EnumerateFiles(applicationRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains(
                $"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}",
                StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains(
                $"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}",
                StringComparison.OrdinalIgnoreCase));
        string[] forbiddenTokens =
        [
            "System.Xml",
            "XDocument",
            "XElement",
            "XAttribute",
            "File.",
            "Directory.",
            "FileStream",
            "System.Windows",
            "Avalonia"
        ];

        foreach (string sourcePath in applicationSources)
        {
            string source = File.ReadAllText(sourcePath);
            foreach (string token in forbiddenTokens)
            {
                Assert.False(
                    source.Contains(token, StringComparison.Ordinal),
                    $"Application access '{token}' found in {Path.GetRelativePath(applicationRoot, sourcePath)}");
            }
        }
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
