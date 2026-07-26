using System.Diagnostics;
using DanteConfigEditor.Services;

namespace DanteConfigEditorV3.Tests;

public sealed class SupportDceTests
{
    [Fact]
    public void PayPalSupportLinkIsExactHttpsAndRestrictedToPayPal()
    {
        Assert.Equal(
            "https://www.paypal.com/qrcodes/p2pqrc/EQYCCDK8XFN5Y",
            SupportLinksService.PayPalSupportUrl);
        Assert.True(SupportLinksService.IsTrustedPayPalUrl(SupportLinksService.PayPalSupportUrl));
        Assert.False(SupportLinksService.IsTrustedPayPalUrl("http://www.paypal.com/qrcodes/p2pqrc/EQYCCDK8XFN5Y"));
        Assert.False(SupportLinksService.IsTrustedPayPalUrl("https://example.com/qrcodes/p2pqrc/EQYCCDK8XFN5Y"));
        Assert.False(SupportLinksService.IsTrustedPayPalUrl("javascript:alert(1)"));
        Assert.False(SupportLinksService.IsTrustedPayPalUrl("file:///tmp/paypal.html"));
    }

    [Fact]
    public void PayPalSupportLinkUsesTheSystemBrowser()
    {
        ProcessStartInfo? captured = null;

        bool opened = SupportLinksService.TryOpenPayPal(
            out string? error,
            startInfo => captured = startInfo);

        Assert.True(opened);
        Assert.Null(error);
        Assert.NotNull(captured);
        Assert.Equal(SupportLinksService.PayPalSupportUrl, captured!.FileName);
        Assert.True(captured.UseShellExecute);
    }

    [Fact]
    public void PayPalSupportLinkReportsLauncherFailure()
    {
        bool opened = SupportLinksService.TryOpenPayPal(
            out string? error,
            _ => throw new InvalidOperationException("browser unavailable"));

        Assert.False(opened);
        Assert.Contains("browser unavailable", error, StringComparison.Ordinal);
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
    public void GitHubFundingUsesOnlyTheApprovedPayPalLink()
    {
        string funding = File.ReadAllText(RepositoryFile(".github", "FUNDING.yml"));

        Assert.Contains(SupportLinksService.PayPalSupportUrl, funding, StringComparison.Ordinal);
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
        Assert.Contains("x:Name=\"PayPalButton\"", windowsDialog, StringComparison.Ordinal);
        Assert.DoesNotContain("WebBrowser", windowsDialog, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("x:Name=\"SupportDceButton\"", macXaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"SupportReminderBorder\"", macXaml, StringComparison.Ordinal);
        Assert.Contains("SupportReminderSettingsService.IsAutomatedTestProcess()", macCode, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"PayPalButton\"", macDialog, StringComparison.Ordinal);
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
        Assert.Contains(SupportLinksService.PayPalSupportUrl, french, StringComparison.Ordinal);
        Assert.Contains("remains completely free", english, StringComparison.Ordinal);
        Assert.Contains(SupportLinksService.PayPalSupportUrl, english, StringComparison.Ordinal);
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
