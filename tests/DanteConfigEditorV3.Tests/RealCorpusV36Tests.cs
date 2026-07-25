using System.Xml.Linq;
using DanteConfigEditor.Models;
using DanteConfigEditor.Services;

namespace DanteConfigEditorV3.Tests;

public sealed class RealCorpusV36Tests
{
    [Fact]
    [Trait("Category", "LocalIntegration")]
    public void OptionalRealCorpusRoundTripsSemanticallyFromTemporaryCopies()
    {
        string? corpusRoot = Environment.GetEnvironmentVariable("DANTE_REAL_XML_ROOT");
        bool required = string.Equals(
            Environment.GetEnvironmentVariable("DANTE_REAL_XML_REQUIRED"),
            "1",
            StringComparison.Ordinal);
        if (string.IsNullOrWhiteSpace(corpusRoot) || !Directory.Exists(corpusRoot))
        {
            Assert.False(
                required,
                "DANTE_REAL_XML_ROOT doit pointer vers un corpus XML existant pour ce contrôle local.");
            return;
        }

        string[] files = Directory.EnumerateFiles(corpusRoot, "*.xml", SearchOption.AllDirectories)
            .Take(100)
            .ToArray();
        Assert.NotEmpty(files);
        using TemporaryDirectory workspace = new();
        foreach (string sourceFile in files)
        {
            string caseDirectory = Path.Combine(workspace.Path, Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(caseDirectory);
            string temporarySource = Path.Combine(caseDirectory, Path.GetFileName(sourceFile));
            File.Copy(sourceFile, temporarySource);
            XDocument original = XDocument.Load(temporarySource, LoadOptions.PreserveWhitespace);
            DanteProject project = DanteProject.Load(temporarySource);
            DanteValidationResult validation = project.Validate();
            Assert.False(
                validation.HasErrors,
                $"{sourceFile}{Environment.NewLine}{validation.ToDisplayText()}");
            string outputPath = Path.Combine(caseDirectory, "roundtrip.xml");

            project.SaveAs(outputPath);
            XDocument output = XDocument.Load(outputPath, LoadOptions.PreserveWhitespace);
            XmlSemanticComparisonResult comparison = XmlSemanticComparisonService.Compare(original, output);

            Assert.True(
                comparison.AreEquivalent,
                $"{sourceFile}{Environment.NewLine}{comparison.ToDisplayText()}");
            DanteProject reloaded = DanteProject.Load(outputPath);
            Assert.Equal(project.Devices.Count, reloaded.Devices.Count);
            Assert.False(reloaded.Validate().HasErrors);
        }
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "DanteConfigEditorV3.RealCorpus",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
