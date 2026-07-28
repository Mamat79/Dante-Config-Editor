using System.IO;

namespace DanteConfigEditor.Services;

public static class MachineBankDistributionService
{
    public const string GitHubBanksUrl =
        "https://github.com/Mamat79/Dante-Config-Editor/tree/main/machine-banks";

    public const string BundledBankFolderName = "DCE Generic Roles 2026.1";

    public const string CommunityBankFolderName = "DCE Community Devices 2026.1";

    public static string IncludedBanksRootPath()
    {
        string documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        return Path.Combine(
            documents,
            "Dante Config Editor",
            "Included Machine Banks");
    }

    public static IReadOnlyList<string> DiscoverIncludedBankPaths(string? rootPath = null)
    {
        string[] roots = string.IsNullOrWhiteSpace(rootPath)
            ?
            [
                Path.Combine(AppContext.BaseDirectory, "Machine Banks"),
                Path.GetFullPath(Path.Combine(
                    AppContext.BaseDirectory,
                    "..",
                    "Resources",
                    "Machine Banks")),
                IncludedBanksRootPath()
            ]
            : [Path.GetFullPath(rootPath)];
        List<string> banks = [];
        foreach (string root in roots.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!Directory.Exists(root))
            {
                continue;
            }

            try
            {
                banks.AddRange(Directory
                    .EnumerateDirectories(root)
                    .Where(path => File.Exists(Path.Combine(path, "bank.json"))));
            }
            catch (IOException)
            {
                // Une source indisponible ne doit pas masquer les autres banques.
            }
            catch (UnauthorizedAccessException)
            {
                // Une source indisponible ne doit pas masquer les autres banques.
            }
        }

        return banks
            .Select(Path.GetFullPath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            // La copie livrée avec l'application est prioritaire sur une
            // ancienne copie Documents portant le même nom de banque.
            .GroupBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderBy(path => Path.GetFileName(path), StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
