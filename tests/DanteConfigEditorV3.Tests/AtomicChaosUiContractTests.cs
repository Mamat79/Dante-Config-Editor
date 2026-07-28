using System.Xml.Linq;

namespace DanteConfigEditorV3.Tests;

public sealed class AtomicChaosUiContractTests
{
    [Fact]
    public void AtomicPanelTurnsKeyAndOpensCoverBeforeArmLockThenFireOnWindowsAndMac()
    {
        string windowsXaml = File.ReadAllText(RepositoryFile("MainWindow.xaml"));
        string windowsCode = File.ReadAllText(RepositoryFile("MainWindow.xaml.cs"));
        string macXaml = File.ReadAllText(RepositoryFile("src", "DanteConfigEditor.Mac", "MainWindow.axaml"));
        string macStyles = File.ReadAllText(RepositoryFile("src", "DanteConfigEditor.Mac", "App.axaml"));
        string macCode = File.ReadAllText(RepositoryFile("src", "DanteConfigEditor.Mac", "MainWindow.axaml.cs"));

        Assert.Contains("x:Name=\"AtomicChaosButton\"", windowsXaml, StringComparison.Ordinal);
        Assert.DoesNotContain("AtomicChaosSidebarButton", windowsXaml, StringComparison.Ordinal);
        Assert.Contains("Header=\"Atomic Bomb\"", windowsXaml, StringComparison.Ordinal);
        Assert.Contains("Style=\"{StaticResource AtomicButtonStyle}\"", windowsXaml, StringComparison.Ordinal);
        Assert.Contains("Content=\"FIRE\"", windowsXaml, StringComparison.Ordinal);
        Assert.DoesNotContain("AtomicHazardBrush", windowsXaml, StringComparison.Ordinal);
        Assert.DoesNotContain("#FACC15", windowsXaml, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Click=\"AtomicChaosButton_Click\"", windowsXaml, StringComparison.Ordinal);
        Assert.Contains("PUPITRE DE COMMANDE", windowsXaml, StringComparison.Ordinal);
        Assert.Contains("AtomicDeviceNamesCheckBox", windowsXaml, StringComparison.Ordinal);
        Assert.Contains("AtomicTxLabelsCheckBox", windowsXaml, StringComparison.Ordinal);
        Assert.Contains("AtomicPatchesCheckBox", windowsXaml, StringComparison.Ordinal);
        Assert.Contains("AtomicIpCheckBox", windowsXaml, StringComparison.Ordinal);
        AssertSequenceControls(windowsXaml);
        AssertSimplifiedKeyVisual(windowsXaml);
        Assert.DoesNotContain("AtomicPanelResetButton", windowsXaml, StringComparison.Ordinal);
        Assert.DoesNotContain("AtomicPanelResetButton_Click", windowsCode, StringComparison.Ordinal);
        Assert.DoesNotContain("OFF · verticale", windowsXaml, StringComparison.Ordinal);
        Assert.DoesNotContain("sans dialogue supplémentaire", windowsXaml, StringComparison.Ordinal);
        Assert.Contains("SelectedAtomicChaosOptions", windowsCode, StringComparison.Ordinal);
        Assert.Contains("_atomicPanelStage != AtomicPanelStage.Locked", windowsCode, StringComparison.Ordinal);
        Assert.Contains("_atomicPanelStage = AtomicPanelStage.CoverOpen", windowsCode, StringComparison.Ordinal);
        Assert.Contains("AtomicKeyRotateTransform.Angle = coverOpen ? 90 : 0", windowsCode, StringComparison.Ordinal);
        Assert.DoesNotContain("AtomicSafetyCoverButton_Click", windowsCode, StringComparison.Ordinal);
        Assert.DoesNotContain("ConfirmAtomicChaosStage", windowsCode, StringComparison.Ordinal);
        Assert.DoesNotContain("Trois confirmations", windowsXaml, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("x:Name=\"AtomicChaosButton\"", macXaml, StringComparison.Ordinal);
        Assert.DoesNotContain("AtomicChaosSidebarButton", macXaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"AtomicTab\" Header=\"Outils avancés\"", macXaml, StringComparison.Ordinal);
        Assert.Contains("Classes=\"atomic\"", macXaml, StringComparison.Ordinal);
        Assert.Contains("Content=\"FIRE\"", macXaml, StringComparison.Ordinal);
        Assert.DoesNotContain("#FACC15", macStyles, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Click=\"AtomicChaosButton_Click\"", macXaml, StringComparison.Ordinal);
        Assert.Contains("PUPITRE DE COMMANDE", macXaml, StringComparison.Ordinal);
        Assert.Contains("AtomicDeviceNamesCheckBox", macXaml, StringComparison.Ordinal);
        Assert.Contains("AtomicTxLabelsCheckBox", macXaml, StringComparison.Ordinal);
        Assert.Contains("AtomicPatchesCheckBox", macXaml, StringComparison.Ordinal);
        Assert.Contains("AtomicIpCheckBox", macXaml, StringComparison.Ordinal);
        AssertSequenceControls(macXaml);
        AssertSimplifiedKeyVisual(macXaml);
        Assert.DoesNotContain("AtomicPanelResetButton", macXaml, StringComparison.Ordinal);
        Assert.DoesNotContain("AtomicPanelResetButton_Click", macCode, StringComparison.Ordinal);
        Assert.DoesNotContain("OFF · verticale", macXaml, StringComparison.Ordinal);
        Assert.DoesNotContain("sans dialogue supplémentaire", macXaml, StringComparison.Ordinal);
        Assert.Contains("SelectedAtomicChaosOptions", macCode, StringComparison.Ordinal);
        Assert.Contains("_atomicPanelStage != AtomicPanelStage.Locked", macCode, StringComparison.Ordinal);
        Assert.Contains("_atomicPanelStage = AtomicPanelStage.CoverOpen", macCode, StringComparison.Ordinal);
        Assert.Contains("new RotateTransform(coverOpen ? 90 : 0)", macCode, StringComparison.Ordinal);
        Assert.DoesNotContain("AtomicSafetyCoverButton_Click", macCode, StringComparison.Ordinal);
        Assert.DoesNotContain("ConfirmAtomicChaosStage", macCode, StringComparison.Ordinal);
        Assert.DoesNotContain("Trois confirmations", macXaml, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertSequenceControls(string xaml)
    {
        int key = xaml.IndexOf("x:Name=\"AtomicKeyButton\"", StringComparison.Ordinal);
        int keyVisual = xaml.IndexOf("x:Name=\"AtomicKeyVisual\"", StringComparison.Ordinal);
        int cover = xaml.IndexOf("x:Name=\"AtomicSafetyCover\"", StringComparison.Ordinal);
        int arm = xaml.IndexOf("x:Name=\"AtomicArmButton\"", StringComparison.Ordinal);
        int @lock = xaml.IndexOf("x:Name=\"AtomicLockButton\"", StringComparison.Ordinal);
        int fire = xaml.IndexOf("x:Name=\"AtomicChaosButton\"", StringComparison.Ordinal);

        Assert.True(key >= 0);
        Assert.True(keyVisual >= 0);
        Assert.True(cover >= 0);
        Assert.True(arm >= 0);
        Assert.True(@lock >= 0);
        Assert.True(fire >= 0);
        Assert.True(key < arm);
        Assert.True(arm < @lock);
        Assert.True(@lock < fire);
        Assert.True(cover > fire);
    }

    private static void AssertSimplifiedKeyVisual(string xaml)
    {
        XDocument document = XDocument.Parse(xaml);
        XNamespace xamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";
        XElement keyVisual = Assert.Single(
            document.Descendants(),
            element =>
                string.Equals(
                    element.Attribute(xamlNamespace + "Name")?.Value,
                    "AtomicKeyVisual",
                    StringComparison.Ordinal));

        Assert.Single(
            keyVisual.Descendants(),
            element => element.Name.LocalName == "Ellipse");
        Assert.Single(
            keyVisual.Descendants(),
            element => element.Name.LocalName == "Rectangle");
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
}
