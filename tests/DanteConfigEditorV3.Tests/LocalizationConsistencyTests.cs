using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using DanteConfigEditor.Services;

namespace DanteConfigEditorV3.Tests;

public sealed class LocalizationConsistencyTests
{
    private static readonly HashSet<string> LanguageNeutralLiterals = new(StringComparer.Ordinal)
    {
        "#", "-", "+", "−", "↑", "↓", "↕", "↗", "0", "1", "10", "100 %", "0.0.0.0", "192.168.1", "255.255.255.0",
        "0 device - 0 TX - 0 RX", "-------[]--", "ATOMIC", "Atomic Bomb", "BOMB", "By Mamat", "et ses agents", "Dante Config Editor V3.6",
        "Daisychain", "Dante Config Editor V3.6 - macOS", "Dante Id",
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
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "DanteConfigEditorV3.csproj")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);
        return Path.Combine([directory!.FullName, .. relativeParts]);
    }

    private static string[] Placeholders(string value) =>
        Regex.Matches(value, @"\{\d+(?::[^}]*)?\}").Select(match => match.Value).ToArray();
}
