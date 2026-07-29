using System.Reflection;
using System.Xml.Linq;
using DanteConfigEditor.Services;

namespace DanteConfigEditorV3.Tests;

public sealed class ToolTipUiContractTests
{
    [Fact]
    public void StaticToolTipsAreRegisteredForFrenchAndEnglish()
    {
        IReadOnlyDictionary<string, string> translations = LiteralTranslations();
        string repository = RepositoryDirectory();
        List<string> missing = [];
        string[] files =
        [
            .. Directory.EnumerateFiles(repository, "*.xaml", SearchOption.AllDirectories),
            .. Directory.EnumerateFiles(repository, "*.axaml", SearchOption.AllDirectories)
        ];

        foreach (string file in files
                     .Where(path => !IsGeneratedPath(path))
                     .Order(StringComparer.OrdinalIgnoreCase))
        {
            XDocument document = XDocument.Load(file);
            IEnumerable<string> toolTips = document
                .Descendants()
                .Attributes()
                .Where(attribute => attribute.Name.LocalName is "ToolTip" or "ToolTip.Tip")
                .Select(attribute => attribute.Value.Trim())
                .Where(value => value.Length > 0 && !value.StartsWith('{'))
                .Distinct(StringComparer.Ordinal);

            foreach (string french in toolTips)
            {
                if (!translations.TryGetValue(french, out string? english)
                    || string.IsNullOrWhiteSpace(english))
                {
                    missing.Add($"{Path.GetRelativePath(repository, file)}: {french}");
                }
            }
        }

        Assert.True(
            missing.Count == 0,
            "Tooltips missing an English translation:" + Environment.NewLine
            + string.Join(Environment.NewLine, missing));
    }

    [Fact]
    public void WindowsAndMacToolTipStylesWrapLongText()
    {
        string designSystem = File.ReadAllText(RepositoryFile("Resources", "DesignSystem2026.xaml"));
        string mainWindow = File.ReadAllText(RepositoryFile("MainWindow.xaml"));
        string dialogStyles = File.ReadAllText(RepositoryFile("Resources", "DialogStyles.xaml"));
        string patchWorkspace = File.ReadAllText(RepositoryFile("PatchWorkspaceView.xaml"));
        string macStyles = File.ReadAllText(RepositoryFile("src", "DanteConfigEditor.Mac", "App.axaml"));

        Assert.Contains("ShellWrappingToolTipContentTemplate", designSystem, StringComparison.Ordinal);
        Assert.Contains("TextWrapping=\"Wrap\"", designSystem, StringComparison.Ordinal);
        Assert.Contains("WrappingToolTipContentTemplate", mainWindow, StringComparison.Ordinal);
        Assert.Contains("ToolTipService.ShowDuration=\"30000\"", mainWindow, StringComparison.Ordinal);
        Assert.Contains("DialogWrappingToolTipContentTemplate", dialogStyles, StringComparison.Ordinal);
        Assert.Contains("PatchWrappingToolTipContentTemplate", patchWorkspace, StringComparison.Ordinal);
        Assert.Contains("<Style Selector=\"ToolTip\">", macStyles, StringComparison.Ordinal);
        Assert.Contains("<Style Selector=\"ToolTip TextBlock\">", macStyles, StringComparison.Ordinal);
        Assert.Contains("<Setter Property=\"TextWrapping\" Value=\"Wrap\" />", macStyles, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(UiLanguage.French, true, "le mode réseau Redondant est disponible pour DEVICE-A.")]
    [InlineData(UiLanguage.English, true, "Redundant network mode is available for DEVICE-A.")]
    [InlineData(UiLanguage.French, false, "Indisponible pour DEVICE-A : ce rôle Dante n'expose pas la balise <redundancy>. DCE ne la créera pas.")]
    [InlineData(UiLanguage.English, false, "Unavailable for DEVICE-A: this Dante role does not expose <redundancy>. DCE will not create it.")]
    public void DeviceCapabilityHelpIsCompleteInBothLanguages(
        UiLanguage language,
        bool supported,
        string expected)
    {
        string result = CapabilityToolTipService.ForDevice(
            language,
            "DEVICE-A",
            supported,
            "le mode réseau Redondant",
            "Redundant network mode",
            "redundancy");

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(UiLanguage.French, true, "Applique le mode Daisychain uniquement aux machines de la cible qui exposent ce paramètre.")]
    [InlineData(UiLanguage.English, true, "Applies Daisychain mode only to target devices that expose this setting.")]
    [InlineData(UiLanguage.French, false, "Indisponible : aucune machine de ce preset n'expose <redundancy>. DCE ne créera pas cette balise.")]
    [InlineData(UiLanguage.English, false, "Unavailable: no device in this preset exposes <redundancy>. DCE will not create it.")]
    public void GlobalCapabilityHelpIsCompleteInBothLanguages(
        UiLanguage language,
        bool available,
        string expected)
    {
        string result = CapabilityToolTipService.ForTarget(
            language,
            available,
            "le mode Daisychain",
            "Daisychain mode",
            "redundancy");

        Assert.Equal(expected, result);
    }

    private static IReadOnlyDictionary<string, string> LiteralTranslations()
    {
        FieldInfo? field = typeof(LocalizationService).GetField(
            "LiteralFrenchToEnglish",
            BindingFlags.Static | BindingFlags.NonPublic);
        return Assert.IsAssignableFrom<IReadOnlyDictionary<string, string>>(field?.GetValue(null));
    }

    private static bool IsGeneratedPath(string path) =>
        path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
        || path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
        || path.Contains($"{Path.DirectorySeparatorChar}dist{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
        || path.Contains($"{Path.DirectorySeparatorChar}publish{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);

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
