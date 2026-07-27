using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using DanteConfigEditor.Models;
using DanteConfigEditor.Services;

namespace DanteConfigEditorV3.Tests;

public sealed class LocalizationConsistencyTests
{
    private static readonly HashSet<string> LanguageNeutralLiterals = new(StringComparer.Ordinal)
    {
        "#", "-", "+", "−", "↑", "↓", "↕", "↗", "0", "1", "10", "100 %", "0.0.0.0", "192.168.1", "255.255.255.0",
        "0 device - 0 TX - 0 RX", "-------[]--", "ATOMIC", "Atomic Bomb", "BOMB", "By Mamat", "et ses agents", "Dante Config Editor 2026.1 Beta",
        "Daisychain", "Dante Config Editor 2026.1 Beta - macOS", "Dante Id",
        "Device", "Easy patch", "Patchbook", "Preferred master", "Preferred Master", "RX", "TX",
        "TX device", "TX Dante Id", "TX/RX", "Type"
    };

    [Fact]
    public void FrenchAndEnglishTranslationsStaySynchronized()
    {
        IReadOnlyDictionary<string, string> french = Dictionary("French");
        IReadOnlyDictionary<string, string> english = Dictionary("English");

        Assert.Equal(french.Keys.Order(), english.Keys.Order());
        foreach (string key in french.Keys)
        {
            Assert.Equal(Placeholders(french[key]), Placeholders(english[key]));
        }

        Assert.Equal("HORRIBLE EXPERIENCE GENERATOR (BUT EDUCATIONAL)",
            LocalizationService.TranslateLiteral(UiLanguage.English, "GÉNÉRATEUR D'EXPÉRIENCE HORRIBLE (MAIS PÉDAGOGIQUE)"));
        Assert.Equal("JSON/CSV labels, DMT XLSX/ODS, A&H dLive/Avantis, and Yamaha CL/QL.",
            LocalizationService.TranslateLiteral(UiLanguage.English, "Labels JSON/CSV, DMT XLSX/ODS, A&H dLive/Avantis et Yamaha CL/QL."));
        Assert.Equal("Export a generic file or create a copy of a DMT, A&H, or Yamaha template.",
            LocalizationService.TranslateLiteral(UiLanguage.English, "Exportez en générique ou créez une copie d'un modèle DMT, A&H ou Yamaha."));
        Assert.Equal("more than 8 characters",
            LocalizationService.TranslateLiteral(UiLanguage.English, "plus de 8 caractères"));
        Assert.Equal("characters not supported by DMT/dLive",
            LocalizationService.TranslateLiteral(UiLanguage.English, "caractères non pris en charge par DMT/dLive"));
        Assert.Equal("Receiving device (Rx)",
            LocalizationService.TranslateLiteral(UiLanguage.English, "Machine réceptrice RX"));
        Assert.Equal("Transmitting device (Tx)",
            LocalizationService.TranslateLiteral(UiLanguage.English, "Machine émettrice TX"));
        Assert.Equal("All Rx channels", english["Filter.AllRx"]);
        Assert.Equal("Different bit depth", english["DeviceFilter.EncodingDifferent"]);
        Assert.Equal("Unlocked devices in current filter", english["Target.FilteredUnlocked"]);
        Assert.Equal("Unable to open file", english["Dialog.OpenFailedTitle"]);
    }

    [Fact]
    public void EnglishTranslationsDoNotContainFrenchResidue()
    {
        Regex frenchResidue = new(
            @"[àâäçéèêëîïôöùûüÿœæÀÂÄÇÉÈÊËÎÏÔÖÙÛÜŸŒÆ]|\b(?:annuler|appliquer|ajouter|aucun|banque|canaux|choisir|enregistrer|fichier|répertoire|supprimer)\b",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        foreach ((string key, string value) in Dictionary("English"))
        {
            Assert.False(
                frenchResidue.IsMatch(value),
                $"English dictionary entry '{key}' contains probable French text: {value}");
        }

        Regex localizedPair = new(
            @"(?:\bL|\bLocal)\(\s*""(?:[^""\\]|\\.)*""\s*,\s*""(?<english>(?:[^""\\]|\\.)*)""\s*\)",
            RegexOptions.CultureInvariant);
        string repository = RepositoryDirectory();
        IEnumerable<string> sourceFiles = Directory
            .EnumerateFiles(repository, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase));

        foreach (string sourceFile in sourceFiles)
        {
            string source = File.ReadAllText(sourceFile);
            foreach (Match match in localizedPair.Matches(source))
            {
                string english = match.Groups["english"].Value;
                Assert.False(
                    frenchResidue.IsMatch(english),
                    $"English translation contains probable French text in {Path.GetRelativePath(repository, sourceFile)}: {english}");
            }
        }
    }

    [Fact]
    public void XmlComparisonReportUsesTheSelectedLanguage()
    {
        string source = RepositoryFile(
            "tests",
            "DanteConfigEditorV3.Tests",
            "Fixtures",
            "representative-preset.xml");
        DanteProject openProject = DanteProject.Load(source);
        DanteProject comparedProject = DanteProject.Load(source);

        string english = openProject.CompareWith(comparedProject, UiLanguage.English);
        string french = openProject.CompareWith(comparedProject, UiLanguage.French);

        Assert.Contains("XML COMPARISON", english, StringComparison.Ordinal);
        Assert.Contains("No difference detected in known fields.", english, StringComparison.Ordinal);
        Assert.DoesNotContain("Aucune différence", english, StringComparison.Ordinal);
        Assert.Contains("COMPARAISON XML", french, StringComparison.Ordinal);
        Assert.Contains("Aucune différence détectée", french, StringComparison.Ordinal);
    }

    [Fact]
    public void AutomaticallyTranslatedViewsDoNotContainUnmappedVisibleLiterals()
    {
        string[][] views =
        [
            ["MainWindow.xaml"],
            ["DeviceDetailsWindow.xaml"],
            ["src", "DanteConfigEditor.Mac", "MainWindow.axaml"]
        ];
        HashSet<string> localizableAttributes = new(StringComparer.Ordinal)
        {
            "Text", "Content", "Header", "ToolTip", "Watermark", "PlaceholderText", "Title"
        };
        IReadOnlyDictionary<string, string> literalTranslations = Dictionary("LiteralFrenchToEnglish");

        foreach (string[] relativeParts in views)
        {
            XDocument document = XDocument.Load(RepositoryFile(relativeParts));
            IEnumerable<string> literals = document
                .Descendants()
                .Attributes()
                .Where(attribute => localizableAttributes.Contains(attribute.Name.LocalName)
                    || (attribute.Name.NamespaceName.Contains("System.Windows.Automation", StringComparison.Ordinal)
                        && attribute.Name.LocalName is "Name" or "HelpText"))
                .Select(attribute => attribute.Value.Trim())
                .Where(value => value.Length > 0 && !value.StartsWith('{'))
                .Distinct(StringComparer.Ordinal);

            foreach (string literal in literals)
            {
                if (LanguageNeutralLiterals.Contains(literal))
                {
                    continue;
                }

                Assert.True(literalTranslations.ContainsKey(literal),
                    $"Visible literal is not registered for translation in {string.Join('/', relativeParts)}: {literal}");
            }
        }

        string[][] automationViews = [.. views, ["PatchWorkspaceView.xaml"]];
        foreach (string[] relativeParts in automationViews)
        {
            XDocument document = XDocument.Load(RepositoryFile(relativeParts));
            IEnumerable<string> automationLiterals = document
                .Descendants()
                .Attributes()
                .Where(attribute => attribute.Name.NamespaceName.Contains("System.Windows.Automation", StringComparison.Ordinal)
                    && attribute.Name.LocalName is "Name" or "HelpText")
                .Select(attribute => attribute.Value.Trim())
                .Where(value => value.Length > 0 && !value.StartsWith('{'))
                .Distinct(StringComparer.Ordinal);

            foreach (string literal in automationLiterals)
            {
                Assert.True(literalTranslations.ContainsKey(literal),
                    $"Accessibility literal is not registered for translation in {string.Join('/', relativeParts)}: {literal}");
            }
        }
    }

    [Theory]
    [InlineData(UiLanguage.French, "nombre de canaux : valeur invalide.")]
    [InlineData(UiLanguage.English, "channel count: invalid value.")]
    public void NumericValidationMessagesUseTheSelectedLanguage(UiLanguage language, string expected)
    {
        string label = language == UiLanguage.English ? "channel count" : "nombre de canaux";

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            LocalizedNumberParser.ParsePositive("invalid", label, language));

        Assert.Equal(expected, exception.Message);
    }

    [Theory]
    [InlineData(UiLanguage.French, "Nombre de canaux invalide.")]
    [InlineData(UiLanguage.English, "Invalid channel count.")]
    public void OptionalCountValidationUsesTheSelectedLanguage(UiLanguage language, string expected)
    {
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            LocalizedNumberParser.ParseOptionalCount("-1", language));

        Assert.Equal(expected, exception.Message);
    }

    private static IReadOnlyDictionary<string, string> Dictionary(string fieldName)
    {
        FieldInfo? field = typeof(LocalizationService).GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic);
        return Assert.IsAssignableFrom<IReadOnlyDictionary<string, string>>(field?.GetValue(null));
    }

    private static string RepositoryFile(params string[] relativeParts)
    {
        return Path.Combine([RepositoryDirectory(), .. relativeParts]);
    }

    private static string RepositoryDirectory()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "DanteConfigEditorV3.csproj")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);
        return directory!.FullName;
    }

    private static string[] Placeholders(string value) =>
        Regex.Matches(value, @"\{\d+(?::[^}]*)?\}").Select(match => match.Value).ToArray();
}
