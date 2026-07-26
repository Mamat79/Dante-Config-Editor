using System.Diagnostics;
using System.Security.Cryptography;
using DanteConfigEditor.Services;

namespace DanteConfigEditorV3.Tests;

public sealed class SupportDceTests
{
    [Fact]
    public void PayPalQrAssetIsTheApprovedImage()
    {
        string path = RepositoryFile("Resources", "Support", "paypal-support-qr.png");
        byte[] bytes = File.ReadAllBytes(path);

        Assert.Equal([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A], bytes[..8]);
        Assert.Equal(
            "C9205EB4CDE2EF04A9944C6696DCF6007FD512C9400DEAABFB9FAA803638495C",
            Convert.ToHexString(SHA256.HashData(bytes)));
    }

    [Fact]
    public void PayPalMeLinkIsExactTrustedAndUsesTheSystemBrowser()
    {
        Assert.Equal(
            "https://www.paypal.com/paypalme/MamatLeroy",
            SupportLinksService.PayPalMeSupportUrl);
        Assert.True(SupportLinksService.IsTrustedPayPalMeUrl(SupportLinksService.PayPalMeSupportUrl));
        Assert.False(SupportLinksService.IsTrustedPayPalMeUrl("http://www.paypal.com/paypalme/MamatLeroy"));
        Assert.False(SupportLinksService.IsTrustedPayPalMeUrl("https://example.com/paypalme/MamatLeroy"));
        Assert.False(SupportLinksService.IsTrustedPayPalMeUrl("https://www.paypal.com/qrcodes/managed/example"));

        ProcessStartInfo? captured = null;
        bool opened = SupportLinksService.TryOpenPayPalMe(
            out string? error,
            startInfo => captured = startInfo);

        Assert.True(opened);
        Assert.Null(error);
        Assert.NotNull(captured);
        Assert.Equal(SupportLinksService.PayPalMeSupportUrl, captured!.FileName);
        Assert.True(captured.UseShellExecute);
    }

    [Fact]
    public void SupportQrIsDeclaredForWindowsAndMac()
    {
        string windowsProject = File.ReadAllText(RepositoryFile("DanteConfigEditorV3.csproj"));
        string macProject = File.ReadAllText(RepositoryFile(
            "src",
            "DanteConfigEditor.Mac",
            "DanteConfigEditor.Mac.csproj"));

        Assert.Contains(@"Resources\Support\paypal-support-qr.png", windowsProject, StringComparison.Ordinal);
        Assert.Contains(@"Assets\paypal-support-qr.png", macProject, StringComparison.Ordinal);
    }

    [Fact]
    public void ReminderIsNotShownOnFirstLaunchAndAppearsOnTwentiethLaunch()
    {
        using TemporarySettings settings = new();
        SupportReminderSettingsService service = new(settings.Path);

        Assert.False(service.RegisterSuccessfulLaunch("3.6").ShouldShow);
        for (int launch = 2; launch < 20; launch++)
        {
            Assert.False(service.RegisterSuccessfulLaunch("3.6").ShouldShow);
        }

        SupportReminderDecision twentieth = service.RegisterSuccessfulLaunch("3.6");
        Assert.True(twentieth.ShouldShow);
        Assert.Equal(20, twentieth.State.LaunchCount);
    }

    [Fact]
    public void ReminderCanBePostponedForTwentyLaunches()
    {
        using TemporarySettings settings = new();
        SupportReminderSettingsService service = new(settings.Path);
        for (int launch = 1; launch <= 20; launch++)
        {
            service.RegisterSuccessfulLaunch("3.6");
        }

        service.Postpone();
        Assert.Equal(40, service.Load()!.NextReminderLaunch);
        for (int launch = 21; launch < 40; launch++)
        {
            Assert.False(service.RegisterSuccessfulLaunch("3.6").ShouldShow);
        }

        Assert.True(service.RegisterSuccessfulLaunch("3.6").ShouldShow);
    }

    [Fact]
    public void ReminderCanBePermanentlySuppressed()
    {
        using TemporarySettings settings = new();
        SupportReminderSettingsService service = new(settings.Path);
        service.RegisterSuccessfulLaunch("3.6");
        service.Suppress();

        for (int launch = 2; launch <= 50; launch++)
        {
            Assert.False(service.RegisterSuccessfulLaunch("3.6").ShouldShow);
        }

        Assert.True(service.Load()!.NeverShowAgain);
    }

    [Fact]
    public void ReminderSkipsFirstLaunchAfterAnUpdate()
    {
        using TemporarySettings settings = new();
        SupportReminderSettingsService service = new(settings.Path);
        for (int launch = 1; launch <= 20; launch++)
        {
            service.RegisterSuccessfulLaunch("3.5");
        }

        SupportReminderDecision updateLaunch = service.RegisterSuccessfulLaunch("3.6");
        Assert.False(updateLaunch.ShouldShow);
        Assert.Equal(22, updateLaunch.State.NextReminderLaunch);
        Assert.True(service.RegisterSuccessfulLaunch("3.6").ShouldShow);
    }

    [Fact]
    public void MalformedReminderSettingsNeverBlockStartup()
    {
        using TemporarySettings settings = new();
        File.WriteAllText(settings.Path, "{not-json");
        SupportReminderSettingsService service = new(settings.Path);

        SupportReminderDecision decision = service.RegisterSuccessfulLaunch("3.6");

        Assert.False(decision.ShouldShow);
        Assert.Equal(1, decision.State.LaunchCount);
        Assert.NotNull(service.Load());
    }

    [Fact]
    public void GitHubFundingUsesOnlyTheApprovedPayPalMeLink()
    {
        string funding = File.ReadAllText(RepositoryFile(".github", "FUNDING.yml"));

        Assert.Contains(SupportLinksService.PayPalMeSupportUrl, funding, StringComparison.Ordinal);
        Assert.DoesNotContain("paypal.com/qrcodes", funding, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("github:", funding, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("patreon", funding, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ko_fi", funding, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SupportUiExistsOnWindowsAndMacWithoutEmbeddedPayment()
    {
        string windowsXaml = File.ReadAllText(RepositoryFile("MainWindow.xaml"));
        string windowsCode = File.ReadAllText(RepositoryFile("MainWindow.xaml.cs"));
        string windowsDialog = File.ReadAllText(RepositoryFile("SupportDceWindow.xaml"));
        string macXaml = File.ReadAllText(RepositoryFile("src", "DanteConfigEditor.Mac", "MainWindow.axaml"));
        string macCode = File.ReadAllText(RepositoryFile("src", "DanteConfigEditor.Mac", "MainWindow.axaml.cs"));
        string macDialog = File.ReadAllText(RepositoryFile("src", "DanteConfigEditor.Mac", "SupportDceDialog.axaml"));

        Assert.Contains("x:Name=\"SupportDceButton\"", windowsXaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"SupportReminderBorder\"", windowsXaml, StringComparison.Ordinal);
        Assert.Contains("SupportReminderSettingsService.IsAutomatedTestProcess()", windowsCode, StringComparison.Ordinal);
        Assert.Contains("paypal-support-qr.png", windowsDialog, StringComparison.Ordinal);
        Assert.Contains("PayPalMeButton", windowsDialog, StringComparison.Ordinal);
        Assert.DoesNotContain("WebBrowser", windowsDialog, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("x:Name=\"SupportDceButton\"", macXaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"SupportReminderBorder\"", macXaml, StringComparison.Ordinal);
        Assert.Contains("SupportReminderSettingsService.IsAutomatedTestProcess()", macCode, StringComparison.Ordinal);
        Assert.Contains("paypal-support-qr.png", macDialog, StringComparison.Ordinal);
        Assert.Contains("PayPalMeButton", macDialog, StringComparison.Ordinal);
        Assert.DoesNotContain("WebView", macDialog, StringComparison.OrdinalIgnoreCase);

        string installer = File.ReadAllText(RepositoryFile("installer", "DanteConfigEditorV3.iss"));
        string macProject = File.ReadAllText(RepositoryFile(
            "src",
            "DanteConfigEditor.Mac",
            "DanteConfigEditor.Mac.csproj"));
        Assert.Contains(@"docs\SUPPORT_DCE.md", installer, StringComparison.Ordinal);
        Assert.Contains(@"Docs\SUPPORT_DCE.md", macProject, StringComparison.Ordinal);
    }

    [Fact]
    public void ReadmesStateThatSupportIsOptionalAndTheApplicationStaysFree()
    {
        string french = File.ReadAllText(RepositoryFile("README.md"));
        string english = File.ReadAllText(RepositoryFile("README_EN.md"));

        Assert.Contains("reste entièrement gratuit", french, StringComparison.Ordinal);
        Assert.Contains("docs/SUPPORT_DCE.md", french, StringComparison.Ordinal);
        Assert.Contains(SupportLinksService.PayPalMeSupportUrl, File.ReadAllText(RepositoryFile("docs", "SUPPORT_DCE.md")), StringComparison.Ordinal);
        Assert.Contains("remains completely free", english, StringComparison.Ordinal);
        Assert.Contains("docs/SUPPORT_DCE.md", english, StringComparison.Ordinal);
    }

    private static string RepositoryFile(params string[] relativeParts)
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null
               && !File.Exists(System.IO.Path.Combine(directory.FullName, "DanteConfigEditorV3.csproj")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);
        return System.IO.Path.Combine([directory!.FullName, .. relativeParts]);
    }

    private sealed class TemporarySettings : IDisposable
    {
        private readonly string _directory = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            "DceSupportTests",
            Guid.NewGuid().ToString("N"));

        public TemporarySettings()
        {
            Directory.CreateDirectory(_directory);
            Path = System.IO.Path.Combine(_directory, "support-reminder.json");
        }

        public string Path { get; }

        public void Dispose()
        {
            Directory.Delete(_directory, recursive: true);
        }
    }
}
