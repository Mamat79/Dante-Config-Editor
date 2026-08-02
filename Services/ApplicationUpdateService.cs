using System.Diagnostics;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace DanteConfigEditor.Services;

public sealed record ApplicationUpdateAsset(string Name, long Size, Uri DownloadUri);

public sealed record ApplicationUpdateRelease(
    string TagName,
    string Name,
    Version Version,
    Uri PageUri,
    DateTimeOffset? PublishedUtc,
    IReadOnlyList<ApplicationUpdateAsset> Assets);

public sealed record ApplicationUpdateDownload(
    ApplicationUpdateRelease Release,
    string PackagePath,
    string Sha256);

/// <summary>
/// Consulte la dernière Release GitHub et télécharge uniquement un installateur
/// accompagné de son empreinte SHA-256. L'interface garde la confirmation du
/// téléchargement et du lancement afin de ne jamais mettre DCE à jour en silence.
/// </summary>
public sealed class ApplicationUpdateService
{
    public const string LatestReleaseApiUrl =
        "https://api.github.com/repos/Mamat79/Dante-Config-Editor/releases/latest";

    private const long MaximumPackageBytes = 1024L * 1024 * 1024;
    private readonly HttpClient _httpClient;

    public ApplicationUpdateService(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        if (!_httpClient.DefaultRequestHeaders.UserAgent.Any())
        {
            _httpClient.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue("DanteConfigEditor", "2026.1.1"));
        }
    }

    public static Version CurrentVersion =>
        typeof(ApplicationUpdateService).Assembly.GetName().Version
        ?? new Version(2026, 1, 1);

    public async Task<ApplicationUpdateRelease> GetLatestReleaseAsync(
        CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage response = await _httpClient.GetAsync(
            LatestReleaseApiUrl,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using JsonDocument document = await JsonDocument.ParseAsync(
            stream,
            cancellationToken: cancellationToken);
        return ParseRelease(document.RootElement);
    }

    public ApplicationUpdateAsset SelectInstaller(ApplicationUpdateRelease release)
    {
        ArgumentNullException.ThrowIfNull(release);
        IEnumerable<ApplicationUpdateAsset> candidates = release.Assets.Where(asset =>
            !asset.Name.EndsWith(".sha256", StringComparison.OrdinalIgnoreCase));
        ApplicationUpdateAsset? selected;
        if (OperatingSystem.IsWindows())
        {
            selected = candidates.FirstOrDefault(asset =>
                asset.Name.EndsWith("_Installer.exe", StringComparison.OrdinalIgnoreCase));
        }
        else if (OperatingSystem.IsMacOS())
        {
            string architecture = RuntimeInformation.ProcessArchitecture == Architecture.Arm64
                ? "AppleSilicon"
                : "Intel";
            selected = candidates.FirstOrDefault(asset =>
                asset.Name.EndsWith(".dmg", StringComparison.OrdinalIgnoreCase)
                && asset.Name.Contains(architecture, StringComparison.OrdinalIgnoreCase));
        }
        else
        {
            throw new PlatformNotSupportedException("Aucun installateur DCE n'est prévu pour cette plateforme.");
        }

        return selected ?? throw new InvalidDataException(
            "La Release GitHub ne contient pas d'installateur adapté à cette plateforme.");
    }

    public async Task<ApplicationUpdateDownload> DownloadInstallerAsync(
        ApplicationUpdateRelease release,
        string? destinationDirectory = null,
        CancellationToken cancellationToken = default)
    {
        ApplicationUpdateAsset package = SelectInstaller(release);
        ApplicationUpdateAsset checksum = release.Assets.FirstOrDefault(asset =>
            string.Equals(
                asset.Name,
                package.Name + ".sha256",
                StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidDataException(
                $"L'empreinte SHA-256 de {package.Name} est absente de la Release GitHub.");
        string expectedHash = await DownloadChecksumAsync(checksum, cancellationToken);
        string directory = Path.GetFullPath(destinationDirectory ?? DefaultDownloadDirectory());
        Directory.CreateDirectory(directory);
        string destination = SafeDestination(directory, package.Name);
        string temporary = destination + $".{Guid.NewGuid():N}.part";
        try
        {
            await DownloadFileAsync(package, temporary, cancellationToken);
            string actualHash;
            await using (FileStream hashStream = File.OpenRead(temporary))
            {
                actualHash = Convert.ToHexString(
                    await SHA256.HashDataAsync(hashStream, cancellationToken));
            }
            if (!string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    "L'installateur téléchargé ne correspond pas à son empreinte SHA-256.");
            }

            File.Move(temporary, destination, overwrite: true);
            return new ApplicationUpdateDownload(release, destination, actualHash);
        }
        finally
        {
            if (File.Exists(temporary))
            {
                File.Delete(temporary);
            }
        }
    }

    public static void LaunchInstaller(string packagePath)
    {
        string fullPath = Path.GetFullPath(packagePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("L'installateur téléchargé est introuvable.", fullPath);
        }

        if (OperatingSystem.IsMacOS())
        {
            ProcessStartInfo startInfo = new("open") { UseShellExecute = false };
            startInfo.ArgumentList.Add(fullPath);
            Process.Start(startInfo);
            return;
        }

        Process.Start(new ProcessStartInfo(fullPath) { UseShellExecute = true });
    }

    internal static ApplicationUpdateRelease ParseRelease(JsonElement root)
    {
        string tag = RequiredString(root, "tag_name");
        string page = RequiredString(root, "html_url");
        if (!Uri.TryCreate(page, UriKind.Absolute, out Uri? pageUri)
            || pageUri.Scheme != Uri.UriSchemeHttps)
        {
            throw new InvalidDataException("L'adresse de la Release GitHub est invalide.");
        }

        Version version = ParseVersion(tag);
        List<ApplicationUpdateAsset> assets = [];
        foreach (JsonElement asset in root.GetProperty("assets").EnumerateArray())
        {
            string name = RequiredString(asset, "name");
            string url = RequiredString(asset, "browser_download_url");
            long size = asset.GetProperty("size").GetInt64();
            if (Path.GetFileName(name) != name
                || !Uri.TryCreate(url, UriKind.Absolute, out Uri? uri)
                || uri.Scheme != Uri.UriSchemeHttps
                || !string.Equals(uri.Host, "github.com", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException("Une ressource de la Release GitHub est invalide.");
            }

            assets.Add(new ApplicationUpdateAsset(name, size, uri));
        }

        DateTimeOffset? published = null;
        if (root.TryGetProperty("published_at", out JsonElement publishedElement)
            && DateTimeOffset.TryParse(publishedElement.GetString(), out DateTimeOffset parsed))
        {
            published = parsed;
        }

        return new ApplicationUpdateRelease(
            tag,
            root.TryGetProperty("name", out JsonElement nameElement)
                ? nameElement.GetString() ?? tag
                : tag,
            version,
            pageUri,
            published,
            assets);
    }

    internal static Version ParseVersion(string tag)
    {
        string clean = tag.Trim().TrimStart('v', 'V');
        int suffix = clean.IndexOfAny(['-', '+']);
        if (suffix >= 0)
        {
            clean = clean[..suffix];
        }

        if (!Version.TryParse(clean, out Version? parsed))
        {
            throw new InvalidDataException($"Le tag GitHub '{tag}' n'est pas un numéro de version DCE valide.");
        }

        return new Version(
            parsed.Major,
            Math.Max(0, parsed.Minor),
            Math.Max(0, parsed.Build));
    }

    private async Task<string> DownloadChecksumAsync(
        ApplicationUpdateAsset checksum,
        CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await _httpClient.GetAsync(
            checksum.DownloadUri,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        string content = await response.Content.ReadAsStringAsync(cancellationToken);
        string hash = content.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault() ?? string.Empty;
        if (hash.Length != 64 || hash.Any(character => !Uri.IsHexDigit(character)))
        {
            throw new InvalidDataException("Le fichier SHA-256 de la Release GitHub est invalide.");
        }

        return hash;
    }

    private async Task DownloadFileAsync(
        ApplicationUpdateAsset asset,
        string destination,
        CancellationToken cancellationToken)
    {
        if (asset.Size <= 0 || asset.Size > MaximumPackageBytes)
        {
            throw new InvalidDataException("La taille déclarée de l'installateur est invalide.");
        }

        using HttpResponseMessage response = await _httpClient.GetAsync(
            asset.DownloadUri,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        if (response.Content.Headers.ContentLength is long length
            && (length != asset.Size || length > MaximumPackageBytes))
        {
            throw new InvalidDataException("La taille téléchargée ne correspond pas à la Release GitHub.");
        }

        await using Stream source = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using FileStream target = new(
            destination,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            81920,
            useAsync: true);
        byte[] buffer = new byte[81920];
        long total = 0;
        while (true)
        {
            int read = await source.ReadAsync(buffer, cancellationToken);
            if (read == 0)
            {
                break;
            }

            total = checked(total + read);
            if (total > MaximumPackageBytes)
            {
                throw new InvalidDataException("L'installateur dépasse la taille autorisée.");
            }

            await target.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
        }

        if (total != asset.Size)
        {
            throw new InvalidDataException("La taille reçue ne correspond pas à la Release GitHub.");
        }
    }

    private static string DefaultDownloadDirectory()
    {
        string downloads = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Downloads");
        return Path.Combine(downloads, "Dante Config Editor Updates");
    }

    private static string SafeDestination(string directory, string fileName)
    {
        if (Path.GetFileName(fileName) != fileName)
        {
            throw new InvalidDataException("Le nom de l'installateur est invalide.");
        }

        string root = Path.GetFullPath(directory).TrimEnd(
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        string destination = Path.GetFullPath(Path.Combine(root, fileName));
        StringComparison comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        if (!destination.StartsWith(root, comparison))
        {
            throw new InvalidDataException("Le chemin de téléchargement est invalide.");
        }

        return destination;
    }

    private static string RequiredString(JsonElement element, string property)
    {
        if (!element.TryGetProperty(property, out JsonElement value)
            || string.IsNullOrWhiteSpace(value.GetString()))
        {
            throw new InvalidDataException($"La propriété GitHub '{property}' est absente.");
        }

        return value.GetString()!;
    }
}
