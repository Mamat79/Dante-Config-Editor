using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace DanteConfigEditor.Services;

public sealed record OnlineMachineBankEntry(
    string Id,
    string Name,
    string FileName,
    string Sha256,
    string MinimumDceVersion,
    string DescriptionFr,
    string DescriptionEn);

public sealed record MachineBankOnlineUpdateItem(
    OnlineMachineBankEntry Bank,
    string DestinationPath,
    bool IsInstalled,
    bool IsCurrent,
    bool IsCompatible);

public sealed record MachineBankOnlineUpdatePlan(
    IReadOnlyList<MachineBankOnlineUpdateItem> Items)
{
    public int PendingCount => Items.Count(item => !item.IsCurrent && item.IsCompatible);

    public int IncompatibleCount => Items.Count(item => !item.IsCompatible);
}

public sealed record MachineBankOnlineUpdateResult(
    IReadOnlyList<string> UpdatedPaths,
    IReadOnlyList<string> CurrentPaths,
    IReadOnlyList<string> BackupPaths);

/// <summary>
/// Télécharge les banques publiques DCE dans Documents. La banque personnelle
/// n'est jamais utilisée comme destination et chaque remplacement est préparé
/// dans un dossier temporaire avant de déplacer l'ancienne copie en sauvegarde.
/// </summary>
public sealed class MachineBankOnlineUpdateService
{
    public const string CatalogUrl =
        "https://raw.githubusercontent.com/Mamat79/Dante-Config-Editor/main/machine-banks/catalog.json";

    private const string ArchiveBaseUrl =
        "https://raw.githubusercontent.com/Mamat79/Dante-Config-Editor/main/machine-banks/";
    private const string MarkerFileName = ".dce-managed-bank.json";
    private const long MaximumDownloadBytes = 1024L * 1024 * 1024;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private readonly HttpClient _httpClient;

    public MachineBankOnlineUpdateService(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        if (!_httpClient.DefaultRequestHeaders.UserAgent.Any())
        {
            _httpClient.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue("DanteConfigEditor", "2026.1.1"));
        }
    }

    public async Task<MachineBankOnlineUpdatePlan> CheckAsync(
        string? includedBanksRoot = null,
        CancellationToken cancellationToken = default)
    {
        string root = Path.GetFullPath(
            includedBanksRoot ?? MachineBankDistributionService.IncludedBanksRootPath());
        OnlineMachineBankEntry[] banks = await DownloadCatalogAsync(cancellationToken);
        List<MachineBankOnlineUpdateItem> items = [];
        foreach (OnlineMachineBankEntry bank in banks)
        {
            string destination = SafeDestination(root, bank.Name);
            bool installed = Directory.Exists(destination);
            string? installedHash = ReadInstalledHash(destination, bank.Id);
            bool compatible = IsCompatible(bank.MinimumDceVersion);
            items.Add(new MachineBankOnlineUpdateItem(
                bank,
                destination,
                installed,
                string.Equals(installedHash, bank.Sha256, StringComparison.OrdinalIgnoreCase),
                compatible));
        }

        return new MachineBankOnlineUpdatePlan(items);
    }

    public async Task<MachineBankOnlineUpdateResult> ApplyAsync(
        MachineBankOnlineUpdatePlan plan,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(plan);
        List<string> updated = [];
        List<string> current = [];
        List<string> backups = [];

        foreach (MachineBankOnlineUpdateItem item in plan.Items)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!item.IsCompatible)
            {
                continue;
            }

            if (item.IsCurrent)
            {
                current.Add(item.DestinationPath);
                continue;
            }

            progress?.Report(item.Bank.Name);
            string? backup = await DownloadAndReplaceAsync(item, cancellationToken);
            updated.Add(item.DestinationPath);
            if (!string.IsNullOrWhiteSpace(backup))
            {
                backups.Add(backup);
            }
        }

        return new MachineBankOnlineUpdateResult(updated, current, backups);
    }

    private async Task<OnlineMachineBankEntry[]> DownloadCatalogAsync(
        CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await _httpClient.GetAsync(
            CatalogUrl,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        CatalogDocument? document = await JsonSerializer.DeserializeAsync<CatalogDocument>(
            stream,
            JsonOptions,
            cancellationToken);
        if (document is null || document.FormatVersion != 1 || document.Banks.Count == 0)
        {
            throw new InvalidDataException("Le catalogue GitHub des banques est vide ou incompatible.");
        }

        OnlineMachineBankEntry[] banks = document.Banks.Select(bank =>
        {
            ValidateCatalogBank(bank);
            return new OnlineMachineBankEntry(
                bank.Id,
                bank.Name,
                bank.File,
                bank.Sha256.ToLowerInvariant(),
                bank.MinimumDceVersion,
                bank.DescriptionFr,
                bank.DescriptionEn);
        }).ToArray();
        if (banks.Select(bank => bank.Id).Distinct(StringComparer.OrdinalIgnoreCase).Count()
            != banks.Length)
        {
            throw new InvalidDataException("Le catalogue GitHub contient des identifiants de banque dupliqués.");
        }

        return banks;
    }

    private async Task<string?> DownloadAndReplaceAsync(
        MachineBankOnlineUpdateItem item,
        CancellationToken cancellationToken)
    {
        string destination = Path.GetFullPath(item.DestinationPath);
        string root = Path.GetDirectoryName(destination)
            ?? throw new InvalidOperationException("Le dossier des banques fournies est introuvable.");
        Directory.CreateDirectory(root);
        string archivePath = Path.Combine(root, $".dce-download-{Guid.NewGuid():N}.zip");
        string incomingPath = Path.Combine(root, $".dce-incoming-{Guid.NewGuid():N}");
        string? backupPath = null;

        try
        {
            Uri archiveUri = new(new Uri(ArchiveBaseUrl), Uri.EscapeDataString(item.Bank.FileName));
            await DownloadFileAsync(archiveUri, archivePath, cancellationToken);
            string actualHash;
            await using (FileStream hashStream = File.OpenRead(archivePath))
            {
                actualHash = Convert.ToHexString(
                    await SHA256.HashDataAsync(hashStream, cancellationToken));
            }
            if (!string.Equals(actualHash, item.Bank.Sha256, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    $"L'empreinte SHA-256 de la banque {item.Bank.Name} ne correspond pas au catalogue.");
            }

            MachineBankArchiveService.RestoreBank(archivePath, incomingPath);
            WriteMarker(incomingPath, item.Bank);

            if (Directory.Exists(destination))
            {
                FileAttributes attributes = File.GetAttributes(destination);
                if (attributes.HasFlag(FileAttributes.ReparsePoint))
                {
                    throw new IOException("La banque à remplacer est un lien et ne peut pas être mise à jour automatiquement.");
                }

                string backupRoot = Path.Combine(root, "Backups");
                Directory.CreateDirectory(backupRoot);
                backupPath = Path.Combine(
                    backupRoot,
                    $"{SafeFolderName(item.Bank.Name)}_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}");
                Directory.Move(destination, backupPath);
            }

            try
            {
                Directory.Move(incomingPath, destination);
            }
            catch
            {
                if (!Directory.Exists(destination)
                    && !string.IsNullOrWhiteSpace(backupPath)
                    && Directory.Exists(backupPath))
                {
                    Directory.Move(backupPath, destination);
                    backupPath = null;
                }

                throw;
            }

            return backupPath;
        }
        finally
        {
            if (File.Exists(archivePath))
            {
                File.Delete(archivePath);
            }

            if (Directory.Exists(incomingPath))
            {
                Directory.Delete(incomingPath, recursive: true);
            }
        }
    }

    private async Task DownloadFileAsync(
        Uri uri,
        string destination,
        CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await _httpClient.GetAsync(
            uri,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        if (response.Content.Headers.ContentLength is > MaximumDownloadBytes)
        {
            throw new InvalidDataException("La banque distante dépasse la taille autorisée.");
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
            if (total > MaximumDownloadBytes)
            {
                throw new InvalidDataException("La banque distante dépasse la taille autorisée.");
            }

            await target.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
        }
    }

    private static void ValidateCatalogBank(CatalogBank bank)
    {
        if (string.IsNullOrWhiteSpace(bank.Id)
            || string.IsNullOrWhiteSpace(bank.Name)
            || string.IsNullOrWhiteSpace(bank.File)
            || Path.GetFileName(bank.File) != bank.File
            || !bank.File.EndsWith(".dce-bank.zip", StringComparison.OrdinalIgnoreCase)
            || bank.Sha256.Length != 64
            || bank.Sha256.Any(character => !Uri.IsHexDigit(character)))
        {
            throw new InvalidDataException("Une entrée du catalogue GitHub est invalide.");
        }
    }

    private static string SafeDestination(string root, string bankName)
    {
        string destination = Path.GetFullPath(Path.Combine(root, SafeFolderName(bankName)));
        string prefix = Path.GetFullPath(root).TrimEnd(
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        StringComparison comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        if (!destination.StartsWith(prefix, comparison))
        {
            throw new InvalidDataException("Le nom de la banque produit un chemin non autorisé.");
        }

        return destination;
    }

    private static string SafeFolderName(string value)
    {
        HashSet<char> invalid = new(Path.GetInvalidFileNameChars());
        string safe = new(value.Trim()
            .Select(character => invalid.Contains(character) ? '_' : character)
            .ToArray());
        safe = safe.Trim(' ', '.');
        return string.IsNullOrWhiteSpace(safe) ? "DCE Bank" : safe;
    }

    private static string? ReadInstalledHash(string destination, string bankId)
    {
        try
        {
            string markerPath = Path.Combine(destination, MarkerFileName);
            if (!File.Exists(markerPath))
            {
                return null;
            }

            ManagedBankMarker? marker = JsonSerializer.Deserialize<ManagedBankMarker>(
                File.ReadAllText(markerPath, Encoding.UTF8),
                JsonOptions);
            return string.Equals(marker?.BankId, bankId, StringComparison.OrdinalIgnoreCase)
                ? marker?.Sha256
                : null;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException)
        {
            return null;
        }
    }

    private static bool IsCompatible(string minimumVersion)
    {
        try
        {
            return string.IsNullOrWhiteSpace(minimumVersion)
                || ApplicationUpdateService.ParseVersion(minimumVersion)
                    <= ApplicationUpdateService.CurrentVersion;
        }
        catch (InvalidDataException)
        {
            return false;
        }
    }

    private static void WriteMarker(string bankPath, OnlineMachineBankEntry bank)
    {
        ManagedBankMarker marker = new()
        {
            FormatVersion = 1,
            BankId = bank.Id,
            Sha256 = bank.Sha256,
            InstalledUtc = DateTimeOffset.UtcNow
        };
        File.WriteAllText(
            Path.Combine(bankPath, MarkerFileName),
            JsonSerializer.Serialize(marker, JsonOptions),
            new UTF8Encoding(false));
    }

    private sealed class CatalogDocument
    {
        public int FormatVersion { get; set; }

        public List<CatalogBank> Banks { get; set; } = [];
    }

    private sealed class CatalogBank
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string File { get; set; } = string.Empty;

        public string Sha256 { get; set; } = string.Empty;

        public string MinimumDceVersion { get; set; } = string.Empty;

        public string DescriptionFr { get; set; } = string.Empty;

        public string DescriptionEn { get; set; } = string.Empty;
    }

    private sealed class ManagedBankMarker
    {
        public int FormatVersion { get; set; }

        public string BankId { get; set; } = string.Empty;

        public string Sha256 { get; set; } = string.Empty;

        public DateTimeOffset InstalledUtc { get; set; }
    }
}
