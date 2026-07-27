using System.Xml.Linq;
using DanteConfigEditor.Models;
using DanteConfigEditor.Services;

namespace DanteConfigEditorV3.Tests;

public sealed class CompatibilityCorpus2026_1Tests
{
    public static TheoryData<string> RoundTripFixtures => new()
    {
        "representative-preset.xml",
        "merge-preset.xml",
        "official-preset-creator-custom.xml",
        "compat-partial-one-way.xml",
        "compat-subscription-edges.xml",
        "compat-namespace-unknown.xml",
        "compat-mixed-network-audio.xml"
    };

    [Theory]
    [MemberData(nameof(RoundTripFixtures))]
    public void AnonymizedFixtureRoundTripsWithoutSemanticLoss(string fixtureName)
    {
        using TemporaryDirectory directory = new();
        string source = Path.Combine(directory.Path, fixtureName);
        string destination = Path.Combine(directory.Path, "saved-" + fixtureName);
        File.Copy(Fixture(fixtureName), source);
        XDocument original = XDocument.Load(
            source,
            LoadOptions.PreserveWhitespace);

        DanteProject project = DanteProject.Load(source);
        Assert.False(
            project.Validate().HasErrors,
            project.Validate().ToDisplayText());

        project.SaveAs(destination);

        XDocument saved = XDocument.Load(
            destination,
            LoadOptions.PreserveWhitespace);
        XmlSemanticComparisonResult comparison =
            XmlSemanticComparisonService.Compare(original, saved);
        Assert.True(comparison.AreEquivalent, comparison.ToDisplayText());

        DanteProject reopened = DanteProject.Load(destination);
        Assert.Equal(project.Devices.Count, reopened.Devices.Count);
        Assert.False(
            reopened.ValidateXmlChangeGuard().HasErrors,
            reopened.ValidateXmlChangeGuard().ToDisplayText());
    }

    [Fact]
    public void NamespaceFixturePreservesUnknownExtensionAndSecondaryInterface()
    {
        DanteProject project = DanteProject.Load(
            Fixture("compat-namespace-unknown.xml"));

        Assert.Equal("urn:dce:test:preset", project.Document.Root!.Name.NamespaceName);
        Assert.Equal(
            2,
            project.Document
                .Descendants()
                .Count(element => element.Name.LocalName == "interface"));
        XElement extension = Assert.Single(
            project.Document.Descendants(),
            element => element.Name.LocalName == "opaque");
        Assert.Equal("preserve-me", extension.Elements().Single().Value);
        Assert.Contains(
            "日本語",
            Assert.Single(project.Devices).RxChannels.Single().DisplayName,
            StringComparison.Ordinal);
    }

    private static string Fixture(string name) =>
        Path.Combine(AppContext.BaseDirectory, "Fixtures", name);

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "DanteConfigEditorV3.CompatibilityCorpus",
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
