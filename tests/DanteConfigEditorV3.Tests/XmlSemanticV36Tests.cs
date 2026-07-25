using System.Xml.Linq;
using DanteConfigEditor.Models;
using DanteConfigEditor.Services;

namespace DanteConfigEditorV3.Tests;

public sealed class XmlSemanticV36Tests
{
    [Fact]
    public void UnchangedSaveIsSemanticallyEquivalentIncludingUnknownContent()
    {
        using TestWorkspace workspace = new();
        XDocument original = XDocument.Load(workspace.SourcePath, LoadOptions.PreserveWhitespace);
        XElement device = original.Root!.Elements().First(element => element.Name.LocalName == "device");
        device.AddFirst(new XComment("commentaire conservé"));
        device.Add(new XElement(
            device.Name.Namespace + "unknown_vendor_section",
            new XAttribute("clé", "éàç"),
            new XElement(device.Name.Namespace + "opaque", "Valeur spéciale")));
        original.Save(workspace.SourcePath, SaveOptions.DisableFormatting);
        DanteProject project = DanteProject.Load(workspace.SourcePath);
        string outputPath = Path.Combine(workspace.DirectoryPath, "unchanged-output.xml");

        project.SaveAs(outputPath);
        XDocument output = XDocument.Load(outputPath, LoadOptions.PreserveWhitespace);
        XmlSemanticComparisonResult comparison = XmlSemanticComparisonService.Compare(original, output);

        Assert.True(comparison.AreEquivalent, comparison.ToDisplayText());
        Assert.Empty(comparison.Differences);
    }

    [Fact]
    public void SemanticComparisonDetectsUnknownNodeLoss()
    {
        XDocument original = XDocument.Parse(
            """
            <preset version="3.0.0">
              <name>Test</name>
              <device><friendly_name>ONE</friendly_name><unknown value="42" /></device>
            </preset>
            """,
            LoadOptions.PreserveWhitespace);
        XDocument changed = new(original);
        changed.Descendants().Single(element => element.Name.LocalName == "unknown").Remove();

        XmlSemanticComparisonResult comparison = XmlSemanticComparisonService.Compare(original, changed);

        Assert.False(comparison.AreEquivalent);
        Assert.Contains(comparison.Differences, difference => difference.Contains("unknown", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void SemanticComparisonIgnoresFormattingButPreservesElementOrderByDefault()
    {
        XDocument compact = XDocument.Parse("<preset version=\"3.0.0\"><name>A</name><description>B</description></preset>");
        XDocument formatted = XDocument.Parse(
            """
            <preset version="3.0.0">
                <name>A</name>
                <description>B</description>
            </preset>
            """);
        XDocument reordered = XDocument.Parse("<preset version=\"3.0.0\"><description>B</description><name>A</name></preset>");

        Assert.True(XmlSemanticComparisonService.Compare(compact, formatted).AreEquivalent);
        Assert.False(XmlSemanticComparisonService.Compare(compact, reordered).AreEquivalent);
    }

    private sealed class TestWorkspace : IDisposable
    {
        public TestWorkspace()
        {
            DirectoryPath = Path.Combine(Path.GetTempPath(), "DanteConfigEditorV3.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(DirectoryPath);
            SourcePath = Path.Combine(DirectoryPath, "representative-preset.xml");
            File.Copy(
                Path.Combine(AppContext.BaseDirectory, "Fixtures", "representative-preset.xml"),
                SourcePath);
        }

        public string DirectoryPath { get; }

        public string SourcePath { get; }

        public void Dispose()
        {
            if (Directory.Exists(DirectoryPath))
            {
                Directory.Delete(DirectoryPath, true);
            }
        }
    }
}
