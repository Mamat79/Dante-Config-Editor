using DanteConfigEditor.Services;

namespace DanteConfigEditorV3.Tests;

public sealed class InstallerContractTests
{
    [Fact]
    public void Installer2026UsesAnIndependentIdentityAndPreservesV36()
    {
        string script = File.ReadAllText(RepositoryFile("installer", "DanteConfigEditorV3.iss"));

        Assert.Contains("AppId={{C893F4F8-5ED3-4C2E-AAD8-024F9DCB4A1D}", script, StringComparison.Ordinal);
        Assert.DoesNotContain("A11FA3C8-3461-46CA-AC61-6A14316E8DBB", script, StringComparison.Ordinal);
        Assert.DoesNotContain("76E68F80-5C89-4415-A090-370CA60EB3AD", script, StringComparison.Ordinal);
        Assert.DoesNotContain("RunLegacyUninstaller", script, StringComparison.Ordinal);
        Assert.Contains("DefaultDirName={autopf}\\Dante Config Editor 2026.1", script, StringComparison.Ordinal);
        Assert.Contains("DefaultGroupName=Dante Config Editor 2026.1", script, StringComparison.Ordinal);
        Assert.Contains("OutputBaseFilename=DanteConfigEditor2026_1_1_Installer", script, StringComparison.Ordinal);
        Assert.Contains("UsePreviousAppDir=yes", script, StringComparison.Ordinal);
        Assert.DoesNotContain("{commonprograms}\\Dante Config Editor V3.6", script, StringComparison.Ordinal);
        Assert.DoesNotContain("{commondesktop}\\DCE V3.6.lnk", script, StringComparison.Ordinal);
        Assert.Contains("DetectExistingInstall", script, StringComparison.Ordinal);
        Assert.Contains("remplacer / mettre à jour", script, StringComparison.Ordinal);
        Assert.Contains("replace/update", script, StringComparison.Ordinal);
        Assert.Contains("HKLM", script, StringComparison.Ordinal);
        Assert.Contains("HKCU", script, StringComparison.Ordinal);
        Assert.Contains("IsLegacyBetaInstallPath", script, StringComparison.Ordinal);
        Assert.Contains("LegacyBetaInstallDir", script, StringComparison.Ordinal);
        Assert.Contains("DCE 2026.1 Beta.lnk", script, StringComparison.Ordinal);
        Assert.Contains("DelTree(LegacyBetaInstallDir", script, StringComparison.Ordinal);
    }

    [Fact]
    public void InstallerPayloadIsSelfContainedAndIncludesBilingualDocumentation()
    {
        string buildScript = File.ReadAllText(RepositoryFile("installer", "build_installer.ps1"));
        string installerScript = File.ReadAllText(RepositoryFile("installer", "DanteConfigEditorV3.iss"));

        Assert.Contains("--self-contained true", buildScript, StringComparison.Ordinal);
        Assert.Contains("-p:PublishSingleFile=true", buildScript, StringComparison.Ordinal);
        Assert.Contains("if ($LASTEXITCODE -ne 0)", buildScript, StringComparison.Ordinal);
        Assert.Contains("QuickStart_DanteConfigEditorV3_FR.pdf", installerScript, StringComparison.Ordinal);
        Assert.Contains("QuickStart_DanteConfigEditorV3_EN.pdf", installerScript, StringComparison.Ordinal);
        Assert.Contains("Notice_DanteConfigEditorV3_FR.pdf", installerScript, StringComparison.Ordinal);
        Assert.Contains("Notice_DanteConfigEditorV3_EN.pdf", installerScript, StringComparison.Ordinal);
        Assert.Contains("DMT_LICENSE.txt", installerScript, StringComparison.Ordinal);
        Assert.Contains("https://github.com/Mamat79/Dante-Config-Editor", installerScript, StringComparison.Ordinal);
        Assert.Contains("SignatureLabel.Caption := 'By Mamat'", installerScript, StringComparison.Ordinal);
        Assert.Contains("SignatureAgentsLabel.Caption := 'et ses agents'", installerScript, StringComparison.Ordinal);
        string windowsWindow = File.ReadAllText(RepositoryFile("MainWindow.xaml"));
        Assert.Contains("MinHeight=\"34\"", windowsWindow, StringComparison.Ordinal);
        Assert.Contains("<ApplicationIcon>Icon3.ico</ApplicationIcon>", File.ReadAllText(RepositoryFile("DanteConfigEditorV3.csproj")), StringComparison.Ordinal);
        Assert.Contains("SetupIconFile={#SourceRoot}\\Icon3.ico", installerScript, StringComparison.Ordinal);
        Assert.Contains("IconFilename: \"{app}\\Icon3.ico\"", installerScript, StringComparison.Ordinal);
        Assert.True(File.Exists(RepositoryFile("Icon3.ico")));
        Assert.Contains("Icon=\"Icon3.ico\"", windowsWindow, StringComparison.Ordinal);
        Assert.Contains("Resources\\Branding\\silemio-logo.png", File.ReadAllText(RepositoryFile("DanteConfigEditorV3.csproj")), StringComparison.Ordinal);
        Assert.Contains("Source=\"/Resources/Branding/silemio-logo.png\"", windowsWindow, StringComparison.Ordinal);
        Assert.Contains("<TextBlock Text=\"SiLeMI/O\"", windowsWindow, StringComparison.Ordinal);
        Assert.Equal(1, windowsWindow.Split("Text=\"SiLeMI/O\"", StringSplitOptions.None).Length - 1);
        Assert.Contains("<TextBlock Text=\"By Mamat\"", windowsWindow, StringComparison.Ordinal);
        Assert.DoesNotContain("<TextBlock Text=\"et ses agents\"", windowsWindow, StringComparison.Ordinal);
        string macWindow = File.ReadAllText(RepositoryFile("src", "DanteConfigEditor.Mac", "MainWindow.axaml"));
        Assert.Contains("Source=\"avares://DanteConfigEditorV3.Mac/Assets/silemio-logo.png\"", macWindow, StringComparison.Ordinal);
        Assert.Contains("<TextBlock Text=\"SiLeMI/O\"", macWindow, StringComparison.Ordinal);
        Assert.Equal(1, macWindow.Split("Text=\"SiLeMI/O\"", StringSplitOptions.None).Length - 1);
        Assert.Contains("By Mamat", macWindow, StringComparison.Ordinal);
        Assert.DoesNotContain("<TextBlock Text=\"et ses agents\"", macWindow, StringComparison.Ordinal);
        Assert.Contains("By Mamat et ses agents", File.ReadAllText(RepositoryFile("Services", "ReportExportService.cs")), StringComparison.Ordinal);
        Assert.Contains("By Mamat et ses agents", File.ReadAllText(RepositoryFile("packaging", "macos", "Info.plist")), StringComparison.Ordinal);
        Assert.Contains("By Mamat et ses agents", File.ReadAllText(RepositoryFile("docs", "generate_guides.py")), StringComparison.Ordinal);
        Assert.Contains("README_EN.md", installerScript, StringComparison.Ordinal);
        Assert.Contains("CHANGELOG.md", installerScript, StringComparison.Ordinal);
        Assert.Contains("RELEASE_NOTES_EN.md", installerScript, StringComparison.Ordinal);
        Assert.Contains("Name: \"desktopicon\"", installerScript, StringComparison.Ordinal);
        Assert.DoesNotContain("Name: \"desktopicon\"; Description: \"{cm:CreateDesktopIcon}\"; GroupDescription: \"{cm:AdditionalIcons}\"; Flags: unchecked", installerScript, StringComparison.Ordinal);
        Assert.Contains("{autodesktop}\\{code:GetShortcutAppName}", installerScript, StringComparison.Ordinal);
        Assert.Contains("Assert-RepositoryPath", buildScript, StringComparison.Ordinal);
        Assert.Contains("Remove-GeneratedPath", buildScript, StringComparison.Ordinal);
        Assert.Contains("Get-FileHash", buildScript, StringComparison.Ordinal);
        Assert.Contains("DanteConfigEditor2026_1_1_Installer.exe", buildScript, StringComparison.Ordinal);
        Assert.Contains("Build-BundledMachineBanks.ps1", buildScript, StringComparison.Ordinal);
        Assert.Contains("CreateInputDirPage", installerScript, StringComparison.Ordinal);
        Assert.Contains("BankOptionsPage.Values[0]", installerScript, StringComparison.Ordinal);
        Assert.DoesNotContain("BankOptionsPage.Selected[", installerScript, StringComparison.Ordinal);
        Assert.Contains("machine-bank-location.txt", installerScript, StringComparison.Ordinal);
        Assert.Contains("UTF8Decode", installerScript, StringComparison.Ordinal);
        Assert.Contains("UTF8Encode", installerScript, StringComparison.Ordinal);
        Assert.Contains("CopyFile(SettingsPath, SettingsPath + '.bak', False)", installerScript, StringComparison.Ordinal);
        Assert.Contains("DCE Generic Roles 2026.1", installerScript, StringComparison.Ordinal);
        Assert.Contains("DCE Community Devices 2026.1", installerScript, StringComparison.Ordinal);
        Assert.Contains(
            "DestDir: \"{app}\\Machine Banks\\DCE Generic Roles 2026.1\"",
            installerScript,
            StringComparison.Ordinal);
        Assert.Contains(
            "DestDir: \"{app}\\Machine Banks\\DCE Community Devices 2026.1\"",
            installerScript,
            StringComparison.Ordinal);
        Assert.Contains(
            "Type: filesandordirs; Name: \"{app}\\Machine Banks\\DCE Community Devices 2026.1\"",
            installerScript,
            StringComparison.Ordinal);
        Assert.Contains("onlyifdoesntexist", installerScript, StringComparison.Ordinal);
        Assert.Contains("ShouldInstallBundledBank", installerScript, StringComparison.Ordinal);
        Assert.Contains("ShouldInstallCommunityBank", installerScript, StringComparison.Ordinal);
        Assert.Contains("GetCommunityBankDestination", installerScript, StringComparison.Ordinal);
        Assert.Contains("BankOptionsPage.Values[2]", installerScript, StringComparison.Ordinal);
        Assert.Contains("FindAvailableBankDestination", installerScript, StringComparison.Ordinal);
        Assert.Contains("while DirExists(Candidate) or FileExists(Candidate)", installerScript, StringComparison.Ordinal);
        Assert.Contains("SaveMachineBankLocation", installerScript, StringComparison.Ordinal);

        string bankBuilder = File.ReadAllText(RepositoryFile("tools", "Build-BundledMachineBanks.ps1"));
        Assert.Contains("Assert-CommunityBank", bankBuilder, StringComparison.Ordinal);
        Assert.Contains("DCE_Community_Devices_2026_1.dce-bank.zip", bankBuilder, StringComparison.Ordinal);
        Assert.Contains("dce-community-devices-2026.1", bankBuilder, StringComparison.Ordinal);
        Assert.Contains("community-device-sources.json", bankBuilder, StringComparison.Ordinal);

        string macPackaging = File.ReadAllText(RepositoryFile("packaging", "macos", "build-macos.sh"));
        Assert.Contains("Machine Banks", macPackaging, StringComparison.Ordinal);
        Assert.Contains("DCE_Generic_Roles_2026_1.dce-bank.zip", macPackaging, StringComparison.Ordinal);
        Assert.Contains("DCE_Community_Devices_2026_1.dce-bank.zip", macPackaging, StringComparison.Ordinal);
    }

    [Fact]
    public void WindowsControlsRemainReadableWhenTheWindowIsReduced()
    {
        string xaml = File.ReadAllText(RepositoryFile("MainWindow.xaml"));

        Assert.Contains("x:Key=\"WrappingButtonContentTemplate\"", xaml, StringComparison.Ordinal);
        Assert.Contains("TextWrapping=\"Wrap\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Columns=\"2\" Margin=\"0,2,0,0\"", xaml, StringComparison.Ordinal);
        Assert.Contains("VerticalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void TestScriptsCoverBothSuitesAndARealUpgradePass()
    {
        string testScript = File.ReadAllText(RepositoryFile("tests", "run-tests.ps1"));
        string upgradeScript = File.ReadAllText(RepositoryFile("tests", "Test-InstallerUpgrade.ps1"));

        Assert.Contains("DanteConfigEditorV3.Tests.csproj", testScript, StringComparison.Ordinal);
        Assert.Contains("DanteConfigEditor.Mac.Tests.csproj", testScript, StringComparison.Ordinal);
        Assert.Contains("--no-restore", testScript, StringComparison.Ordinal);

        Assert.Contains("/VERYSILENT", upgradeScript, StringComparison.Ordinal);
        Assert.Contains("Invoke-InstallerPass", upgradeScript, StringComparison.Ordinal);
        Assert.Contains("Mise à niveau de contrôle", upgradeScript, StringComparison.Ordinal);
        Assert.Contains("TargetInstallRecords", upgradeScript, StringComparison.Ordinal);
        Assert.Contains("Get-StableSnapshot", upgradeScript, StringComparison.Ordinal);
        Assert.Contains("A11FA3C8-3461-46CA-AC61-6A14316E8DBB", upgradeScript, StringComparison.Ordinal);
        Assert.Contains("C893F4F8-5ED3-4C2E-AAD8-024F9DCB4A1D", upgradeScript, StringComparison.Ordinal);
        Assert.Contains("CommonDesktopDirectory", upgradeScript, StringComparison.Ordinal);
        Assert.Contains("raccourci Bureau manquant", upgradeScript, StringComparison.Ordinal);
        Assert.Contains("StableInstallRecords", upgradeScript, StringComparison.Ordinal);
        Assert.Contains("DanteConfigEditor2026_1_1_Installer.exe", upgradeScript, StringComparison.Ordinal);
        Assert.Contains("C893F4F8-5ED3-4C2E-AAD8-024F9DCB4A1D", upgradeScript, StringComparison.Ordinal);
    }

    [Fact]
    public void WindowsWorkflowPublishesSelfContained2026ArtifactsFromMain()
    {
        string workflow = File.ReadAllText(RepositoryFile(".github", "workflows", "windows-ci.yml"));
        string bankAudit = File.ReadAllText(RepositoryFile(".github", "workflows", "machine-bank-audit.yml"));

        Assert.Contains("- main", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("- \"2026.1\"", workflow, StringComparison.Ordinal);
        Assert.Contains("--self-contained true", workflow, StringComparison.Ordinal);
        Assert.Contains("DanteConfigEditor2026-1-1-win-x64", workflow, StringComparison.Ordinal);
        Assert.Contains("DCE-2026.1.1-Windows-Installer", workflow, StringComparison.Ordinal);
        Assert.Contains("DanteConfigEditor2026_1_1_Installer.exe", workflow, StringComparison.Ordinal);
        Assert.Contains("schedule:", bankAudit, StringComparison.Ordinal);
        Assert.Contains("Build-BundledMachineBanks.ps1", bankAudit, StringComparison.Ordinal);
        Assert.Contains("git diff --exit-code", bankAudit, StringComparison.Ordinal);
        Assert.Contains("MachineBankV36Tests", bankAudit, StringComparison.Ordinal);
        Assert.Contains("contents: read", bankAudit, StringComparison.Ordinal);
    }

    [Fact]
    public void V2026UsesAnIsolatedProfileAndKeepsTheV36MigrationSource()
    {
        Assert.Equal("DanteConfigEditor2026.1", ApplicationStoragePaths.RootFolderName);
        Assert.Equal(
            "DanteConfigEditorV3.2",
            ApplicationStoragePaths.LegacyV36RootFolderName);
        Assert.NotEqual(
            ApplicationStoragePaths.RootPath,
            ApplicationStoragePaths.LegacyV36RootPath);
    }

    [Fact]
    public void Release2026HasDedicatedMacPackagingMetadata()
    {
        string project = File.ReadAllText(RepositoryFile("src", "DanteConfigEditor.Mac", "DanteConfigEditor.Mac.csproj"));
        string plist = File.ReadAllText(RepositoryFile("packaging", "macos", "Info.plist"));
        string packaging = File.ReadAllText(RepositoryFile("packaging", "macos", "build-macos.sh"));
        string workflow = File.ReadAllText(RepositoryFile(".github", "workflows", "macos-ci.yml"));

        Assert.Contains("<InformationalVersion>2026.1.1</InformationalVersion>", project, StringComparison.Ordinal);
        Assert.Contains("<string>Dante Config Editor 2026.1</string>", plist, StringComparison.Ordinal);
        Assert.Contains("<string>fr.mamat.danteconfigeditor.y2026-1</string>", plist, StringComparison.Ordinal);
        Assert.Contains("<string>2026.1.1</string>", plist, StringComparison.Ordinal);
        Assert.Contains("Dante Config Editor 2026.1.app", packaging, StringComparison.Ordinal);
        Assert.Contains("DanteConfigEditor2026_1_1_macOS_", packaging, StringComparison.Ordinal);
        Assert.Contains("shasum -a 256 \"$DMG_NAME\"", packaging, StringComparison.Ordinal);
        Assert.Contains("branches:", workflow, StringComparison.Ordinal);
        Assert.Contains("- main", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("- \"2026.1\"", workflow, StringComparison.Ordinal);
        Assert.Contains("DanteConfigEditor2026_1_1_macOS_AppleSilicon.dmg", workflow, StringComparison.Ordinal);
        Assert.Contains("DanteConfigEditor2026_1_1_macOS_Intel.dmg", workflow, StringComparison.Ordinal);
        Assert.Contains("workflow_dispatch:", workflow, StringComparison.Ordinal);
    }

    [Fact]
    public void VersionedReleaseWorkflowPreservesPublishedHistory()
    {
        string workflow = File.ReadAllText(RepositoryFile(".github", "workflows", "versioned-release.yml"));

        Assert.Contains("refs/tags/${{ needs.prepare.outputs.tag }}", workflow, StringComparison.Ordinal);
        Assert.Contains("Release $tag already exists. Refusing to overwrite it.", workflow, StringComparison.Ordinal);
        Assert.Contains("gh release create", workflow, StringComparison.Ordinal);
        Assert.Contains("--verify-tag", workflow, StringComparison.Ordinal);
        Assert.Contains("make_latest", workflow, StringComparison.Ordinal);
        Assert.Contains("find docs -maxdepth 1 -type f -name '*.pdf'", workflow, StringComparison.Ordinal);
        Assert.Contains("find docs/media -maxdepth 1 -type f", workflow, StringComparison.Ordinal);
        Assert.Contains("dce-v${legacy_token}-presentation-*.mp4", workflow, StringComparison.Ordinal);
        Assert.Contains("dce-v${legacy_token}-presentation-*.srt", workflow, StringComparison.Ordinal);
        Assert.Contains("dce-${calendar_token}-presentation-*.mp4", workflow, StringComparison.Ordinal);
        Assert.Contains("dce-${calendar_token}-presentation-*.srt", workflow, StringComparison.Ordinal);
        Assert.Contains("RELEASE_NOTES_EN.md", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("gh release delete", workflow, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("--clobber", workflow, StringComparison.OrdinalIgnoreCase);
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
