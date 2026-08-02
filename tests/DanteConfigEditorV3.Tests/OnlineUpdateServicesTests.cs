using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DanteConfigEditor.Services;

namespace DanteConfigEditorV3.Tests;

public sealed class OnlineUpdateServicesTests
{
    [Fact]
    public async Task OfficialBankUpdateReplacesDocumentsCopyAndKeepsBackup()
    {
        using UpdateWorkspace workspace = new();
        byte[] archive = File.ReadAllBytes(RepositoryFile(
            "machine-banks",
            "DCE_Community_Devices_2026_1.dce-bank.zip"));
        string hash = Convert.ToHexString(SHA256.HashData(archive));
        using HttpClient client = CreateClient(Catalog(hash), archive);
        MachineBankOnlineUpdateService service = new(client);
        string existing = Path.Combine(workspace.Root, "DCE Community Devices 2026.1");
        Directory.CreateDirectory(existing);
        File.WriteAllText(Path.Combine(existing, "old.txt"), "ancienne banque");

        MachineBankOnlineUpdatePlan plan = await service.CheckAsync(workspace.Root);

        Assert.Equal(1, plan.PendingCount);
        Assert.True(plan.Items.Single().IsInstalled);

        MachineBankOnlineUpdateResult result = await service.ApplyAsync(plan);

        Assert.Single(result.UpdatedPaths);
        string backup = Assert.Single(result.BackupPaths);
        Assert.True(File.Exists(Path.Combine(backup, "old.txt")));
        Assert.Equal(
            43,
            new MachineBankRepository(existing).List().Count);
        Assert.True(File.Exists(Path.Combine(existing, ".dce-managed-bank.json")));

        MachineBankOnlineUpdatePlan secondCheck = await service.CheckAsync(workspace.Root);
        Assert.Equal(0, secondCheck.PendingCount);
        Assert.True(secondCheck.Items.Single().IsCurrent);
    }

    [Fact]
    public async Task InvalidBankHashLeavesExistingCopyUntouched()
    {
        using UpdateWorkspace workspace = new();
        byte[] archive = File.ReadAllBytes(RepositoryFile(
            "machine-banks",
            "DCE_Community_Devices_2026_1.dce-bank.zip"));
        using HttpClient client = CreateClient(Catalog(new string('0', 64)), archive);
        MachineBankOnlineUpdateService service = new(client);
        string existing = Path.Combine(workspace.Root, "DCE Community Devices 2026.1");
        Directory.CreateDirectory(existing);
        string sentinel = Path.Combine(existing, "keep.txt");
        File.WriteAllText(sentinel, "intact");
        MachineBankOnlineUpdatePlan plan = await service.CheckAsync(workspace.Root);

        await Assert.ThrowsAsync<InvalidDataException>(() => service.ApplyAsync(plan));

        Assert.Equal("intact", File.ReadAllText(sentinel));
        Assert.False(Directory.Exists(Path.Combine(workspace.Root, "Backups")));
    }

    [Fact]
    public async Task ApplicationUpdateParsesReleaseAndVerifiesInstaller()
    {
        using UpdateWorkspace workspace = new();
        byte[] installer = [0x44, 0x43, 0x45, 0x21];
        string hash = Convert.ToHexString(SHA256.HashData(installer));
        string installerName = CurrentPlatformInstallerName();
        string releaseJson = Release(installerName, installer.Length, hash);
        using HttpClient client = new(new RouteHandler(request =>
        {
            string path = request.RequestUri!.AbsolutePath;
            if (path.EndsWith("/latest", StringComparison.Ordinal))
            {
                return JsonResponse(releaseJson);
            }

            if (path.EndsWith(".sha256", StringComparison.Ordinal))
            {
                return TextResponse($"{hash}  {installerName}");
            }

            return BytesResponse(installer);
        }));
        ApplicationUpdateService service = new(client);

        ApplicationUpdateRelease release = await service.GetLatestReleaseAsync();
        ApplicationUpdateDownload download = await service.DownloadInstallerAsync(
            release,
            workspace.Root);

        Assert.Equal(new Version(2026, 1, 1), release.Version);
        Assert.Equal(installerName, Path.GetFileName(download.PackagePath));
        Assert.Equal(hash, download.Sha256);
        Assert.Equal(installer, File.ReadAllBytes(download.PackagePath));
    }

    [Fact]
    public async Task ApplicationUpdateRejectsInstallerWithWrongHash()
    {
        using UpdateWorkspace workspace = new();
        byte[] installer = [0x44, 0x43, 0x45, 0x21];
        string installerName = CurrentPlatformInstallerName();
        string releaseJson = Release(installerName, installer.Length, new string('0', 64));
        using HttpClient client = new(new RouteHandler(request =>
        {
            string path = request.RequestUri!.AbsolutePath;
            if (path.EndsWith("/latest", StringComparison.Ordinal))
            {
                return JsonResponse(releaseJson);
            }

            if (path.EndsWith(".sha256", StringComparison.Ordinal))
            {
                return TextResponse($"{new string('0', 64)}  {installerName}");
            }

            return BytesResponse(installer);
        }));
        ApplicationUpdateService service = new(client);
        ApplicationUpdateRelease release = await service.GetLatestReleaseAsync();

        await Assert.ThrowsAsync<InvalidDataException>(() =>
            service.DownloadInstallerAsync(release, workspace.Root));

        Assert.False(File.Exists(Path.Combine(workspace.Root, installerName)));
    }

    private static HttpClient CreateClient(string catalog, byte[] archive) =>
        new(new RouteHandler(request =>
            request.RequestUri!.AbsolutePath.EndsWith("catalog.json", StringComparison.Ordinal)
                ? JsonResponse(catalog)
                : BytesResponse(archive)));

    private static string Catalog(string hash) => JsonSerializer.Serialize(new
    {
        formatVersion = 1,
        banks = new[]
        {
            new
            {
                id = "dce-community-devices-2026.1",
                name = "DCE Community Devices 2026.1",
                file = "DCE_Community_Devices_2026_1.dce-bank.zip",
                sha256 = hash,
                minimumDceVersion = "2026.1",
                language = "fr-en",
                descriptionFr = "Banque de test",
                descriptionEn = "Test bank"
            }
        }
    });

    private static string Release(string installerName, int size, string hash) =>
        JsonSerializer.Serialize(new
        {
            tag_name = "v2026.1.1",
            name = "DCE 2026.1.1",
            html_url = "https://github.com/Mamat79/Dante-Config-Editor/releases/tag/v2026.1.1",
            published_at = "2026-08-02T12:00:00Z",
            assets = new object[]
            {
                new
                {
                    name = installerName,
                    size,
                    browser_download_url = $"https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2026.1.1/{installerName}"
                },
                new
                {
                    name = installerName + ".sha256",
                    size = 100,
                    browser_download_url = $"https://github.com/Mamat79/Dante-Config-Editor/releases/download/v2026.1.1/{installerName}.sha256"
                }
            },
            expected_hash_for_test = hash
        });

    private static string CurrentPlatformInstallerName()
    {
        if (OperatingSystem.IsWindows())
        {
            return "DanteConfigEditor2026_1_1_Installer.exe";
        }

        if (OperatingSystem.IsMacOS())
        {
            string architecture = RuntimeInformation.ProcessArchitecture == Architecture.Arm64
                ? "AppleSilicon"
                : "Intel";
            return $"DanteConfigEditor2026_1_1_macOS_{architecture}.dmg";
        }

        throw new PlatformNotSupportedException("Le test de mise à jour cible Windows et macOS.");
    }

    private static HttpResponseMessage JsonResponse(string content) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        };

    private static HttpResponseMessage TextResponse(string content) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(content, Encoding.UTF8, "text/plain")
        };

    private static HttpResponseMessage BytesResponse(byte[] content) =>
        new(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(content)
        };

    private static string RepositoryFile(params string[] parts)
    {
        string path = AppContext.BaseDirectory;
        while (!File.Exists(Path.Combine(path, "DanteConfigEditorV3.csproj")))
        {
            path = Directory.GetParent(path)?.FullName
                ?? throw new DirectoryNotFoundException("Racine du dépôt introuvable.");
        }

        return Path.Combine([path, .. parts]);
    }

    private sealed class RouteHandler(
        Func<HttpRequestMessage, HttpResponseMessage> route) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(route(request));
    }

    private sealed class UpdateWorkspace : IDisposable
    {
        public UpdateWorkspace()
        {
            Root = Path.Combine(Path.GetTempPath(), $"dce-update-tests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Root);
        }

        public string Root { get; }

        public void Dispose()
        {
            if (Directory.Exists(Root))
            {
                Directory.Delete(Root, recursive: true);
            }
        }
    }
}
