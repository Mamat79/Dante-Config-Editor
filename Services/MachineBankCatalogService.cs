using System.IO;
using DanteConfigEditor.Models;

namespace DanteConfigEditor.Services;

public sealed record MachineBankCatalogSource(
    string Path,
    string Name,
    int FormatVersion,
    bool IsActive,
    IReadOnlyList<MachineTemplateMetadata> Templates);

public sealed record MachineBankCatalogEntry(
    MachineTemplateMetadata Metadata,
    string BankPath,
    string BankName,
    bool IsActiveBank);

public sealed record MachineBankCatalogIssue(
    string BankPath,
    Exception Exception);

public sealed record MachineBankCatalogSnapshot(
    IReadOnlyList<MachineBankCatalogSource> Sources,
    IReadOnlyList<MachineBankCatalogEntry> Entries,
    IReadOnlyList<MachineBankCatalogEntry> UniqueEntries,
    IReadOnlyList<MachineBankCatalogIssue> Issues);

public static class MachineBankCatalogService
{
    public static MachineBankCatalogSnapshot Load(
        string activeBankPath,
        IEnumerable<string>? includedBankPaths = null)
    {
        if (string.IsNullOrWhiteSpace(activeBankPath))
        {
            throw new ArgumentException(
                "Le répertoire de la banque active doit être renseigné.",
                nameof(activeBankPath));
        }

        string activePath = Path.GetFullPath(activeBankPath);
        IReadOnlyList<string> includedPaths = includedBankPaths?.ToArray()
            ?? MachineBankDistributionService.DiscoverIncludedBankPaths();
        string[] paths =
        [
            activePath,
            .. includedPaths
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Select(Path.GetFullPath)
        ];

        List<MachineBankCatalogSource> sources = [];
        List<MachineBankCatalogIssue> issues = [];
        foreach (string path in paths.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                MachineBankRepository repository = new(path);
                sources.Add(new MachineBankCatalogSource(
                    path,
                    BankName(path),
                    repository.GetFormatVersion(),
                    string.Equals(path, activePath, StringComparison.OrdinalIgnoreCase),
                    repository.List()));
            }
            catch (Exception exception)
            {
                issues.Add(new MachineBankCatalogIssue(path, exception));
            }
        }

        MachineBankCatalogEntry[] entries = sources
            .SelectMany(source => source.Templates.Select(metadata =>
                new MachineBankCatalogEntry(
                    metadata,
                    source.Path,
                    source.Name,
                    source.IsActive)))
            .ToArray();

        // La banque personnelle est prioritaire lorsqu'un modèle livré porte
        // le même identifiant. La vue globale reste ainsi sans doublon tout en
        // conservant les éventuelles personnalisations de l'utilisateur.
        MachineBankCatalogEntry[] uniqueEntries = entries
            .OrderByDescending(entry => entry.IsActiveBank)
            .GroupBy(entry => entry.Metadata.TemplateId)
            .Select(group => group.First())
            .OrderBy(entry => entry.Metadata.Manufacturer, StringComparer.OrdinalIgnoreCase)
            .ThenBy(entry => entry.Metadata.Model, StringComparer.OrdinalIgnoreCase)
            .ThenBy(entry => entry.Metadata.TemplateName, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new MachineBankCatalogSnapshot(
            sources,
            entries,
            uniqueEntries,
            issues);
    }

    private static string BankName(string path)
    {
        string trimmed = path.TrimEnd(
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar);
        string name = Path.GetFileName(trimmed);
        return string.IsNullOrWhiteSpace(name) ? path : name;
    }
}
