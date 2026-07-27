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
        "#", "-", "+", "<", "−", "↑", "↓", "↕", "↗", "0", "1", "10", "100 %", "0.0.0.0", "192.168.1", "255.255.255.0",
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
    public void TextReportsUseTheSelectedLanguage()
    {
        DanteProject project = DanteProject.Load(RepositoryFile(
            "tests",
            "DanteConfigEditorV3.Tests",
            "Fixtures",
            "representative-preset.xml"));

        string[] englishReports =
        [
            project.BuildSaveSummary(UiLanguage.English),
            project.BuildReportText(UiLanguage.English),
            project.BuildCompatibilityReport(UiLanguage.English),
            project.BuildPatchbookText("all", language: UiLanguage.English),
            project.BuildTopologyText(UiLanguage.English)
        ];

        Assert.Contains("PRE-SAVE SUMMARY", englishReports[0], StringComparison.Ordinal);
        Assert.Contains("DANTE CONFIG EDITOR - REPORT", englishReports[1], StringComparison.Ordinal);
        Assert.Contains("XML compatibility", englishReports[2], StringComparison.Ordinal);
        Assert.Contains("Active subscriptions", englishReports[3], StringComparison.Ordinal);
        Assert.Contains("SIMPLE TOPOLOGY", englishReports[4], StringComparison.Ordinal);
        Assert.All(englishReports, report =>
        {
            Assert.DoesNotContain("RÉSUMÉ", report, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Fichier original", report, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Aucune modification", report, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("canaux TX", report, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Sauvegarde refusée", report, StringComparison.OrdinalIgnoreCase);
        });
    }

    [Theory]
    [InlineData("3 machine(s) ajoutée(s) depuis second.xml", "3 device(s) added from second.xml")]
    [InlineData("2 machine(s) passée(s) en dynamique", "2 device(s) switched to automatic IP")]
    [InlineData("DEVICE-B supprimé, 4 patch(s) nettoyé(s)", "DEVICE-B deleted, 4 subscription(s) removed")]
    [InlineData("Réinitialisation des canaux de tous les devices", "Channels reset for all devices")]
    [InlineData("Fichier sauvegardé sous C:\\Temp\\preset.xml", "File saved as C:\\Temp\\preset.xml")]
    public void HistoryDetailsAreReadableInEnglish(string french, string expected)
    {
        Assert.Equal(expected, LocalizationService.TranslateHistoryDetail(UiLanguage.English, french));
        Assert.Equal(french, LocalizationService.TranslateHistoryDetail(UiLanguage.French, french));
    }

    [Theory]
    [InlineData(
        "DEVICE-B ne contient aucun canal TX.",
        "DEVICE-B has no TX channel.")]
    [InlineData(
        "DEVICE-A / LOCAL MON utilise une source locale '.'.",
        "DEVICE-A / LOCAL MON uses the local source '.'.")]
    [InlineData(
        "Plusieurs samplerates sont présents dans le preset : 48000, 96000.",
        "Multiple sample rates are present in the preset: 48000, 96000.")]
    [InlineData(
        "IP fixe détectée sur 1 machine(s) : DEVICE-B (192.168.50.20).",
        "Static IP detected on 1 device(s): DEVICE-B (192.168.50.20).")]
    [InlineData(
        "ROOM-A : nombre de canaux TX modifié (8 attendu(s), 7 trouvé(s)).",
        "ROOM-A: TX channel count changed (8 expected, 7 found).")]
    [InlineData(
        "ROOM-A RX Dante Id 4 : subscribed_channel renseigné sans subscribed_device.",
        "ROOM-A Rx Dante ID 4: subscribed_channel is set without subscribed_device.")]
    [InlineData(
        "Adresse IPv4 fixe dupliquée : 192.168.1.10 est utilisée par ROOM-A, ROOM-B. Attribuez une adresse unique ou repassez les machines concernées en IP automatique.",
        "Duplicate static IPv4 address: 192.168.1.10 is used by ROOM-A, ROOM-B. Assign a unique address or switch the affected devices back to automatic IP.")]
    [InlineData(
        "Modification technique interdite : /preset/device/instance_id : Valeur modifiée : A -> B.",
        "Forbidden technical change: /preset/device/instance_id : Value changed: A -> B.")]
    [InlineData(
        "Chemin XML non autorisé par défaut : /preset/device/inconnu : Balise ajoutée : <inconnu>.",
        "XML path not allowed by default: /preset/device/inconnu : Element added: <inconnu>.")]
    public void ValidationMessagesAreReadableInEnglish(string french, string expected)
    {
        Assert.Equal(expected, LocalizationService.TranslateValidationMessage(UiLanguage.English, french));
        Assert.Equal(french, LocalizationService.TranslateValidationMessage(UiLanguage.French, french));
    }

    [Fact]
    public void ValidationSummaryUsesTheRequestedLanguage()
    {
        DanteValidationResult result = new();
        result.AddError(
            DanteIssueCategory.XmlCompatibility,
            "Sauvegarde refusée : la racine <preset> est absente.");
        result.AddWarning(
            DanteIssueCategory.Device,
            "ROOM-A ne contient aucun canal TX.");

        string english = result.ToDisplayText(UiLanguage.English);

        Assert.Contains("Blocking errors:", english, StringComparison.Ordinal);
        Assert.Contains("Save blocked: the <preset> root element is missing.", english, StringComparison.Ordinal);
        Assert.Contains("Items to check:", english, StringComparison.Ordinal);
        Assert.Contains("ROOM-A has no TX channel.", english, StringComparison.Ordinal);
        Assert.DoesNotContain("Sauvegarde", english, StringComparison.Ordinal);
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
