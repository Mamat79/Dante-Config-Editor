using System.IO;
using DanteConfigEditor.Models;

namespace DanteConfigEditor.Services;

public interface IChannelLabelImportAdapter
{
    string Name { get; }

    IReadOnlySet<string> Extensions { get; }

    ChannelLabelImportAdapterResult Read(string path);
}

public static class ChannelLabelImportAdapterRegistry
{
    private static readonly IReadOnlyList<IChannelLabelImportAdapter> Adapters =
    [
        new JsonChannelLabelImportAdapter(),
        new CsvChannelLabelImportAdapter(),
        new DmtXlsxChannelLabelImportAdapter(),
        new DmtOdsChannelLabelImportAdapter(),
        new ConsoleZipChannelLabelImportAdapter()
    ];

    public static IReadOnlyList<IChannelLabelImportAdapter> RegisteredAdapters => Adapters;

    public static ChannelLabelReadResult Read(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Le chemin du fichier de labels doit être renseigné.", nameof(path));
        }

        string extension = Path.GetExtension(path).ToLowerInvariant();
        IChannelLabelImportAdapter adapter = Adapters.FirstOrDefault(candidate => candidate.Extensions.Contains(extension))
            ?? throw new InvalidDataException(
                $"Format de labels '{extension}' non pris en charge. Utilisez JSON, CSV, XLSX/ODS DMT ou ZIP Yamaha CL/QL.");

        ChannelLabelImportAdapterResult adapterResult;
        try
        {
            adapterResult = adapter.Read(path);
        }
        catch (Exception ex) when (ex is InvalidDataException or IOException or UnauthorizedAccessException)
        {
            throw new InvalidDataException($"Import {adapter.Name} impossible : {ex.Message}", ex);
        }

        return new ChannelLabelReadResult(
            adapterResult.Document,
            BuildReport(adapter.Name, adapterResult));
    }

    private static ChannelLabelImportReport BuildReport(string adapterName, ChannelLabelImportAdapterResult adapterResult)
    {
        ChannelLabelDocument document = adapterResult.Document;
        ChannelLabelEntry[] channels = document.Sets.SelectMany(set => set.Channels).ToArray();
        int duplicateLabels = document.Sets.Sum(set => set.Channels
            .Where(channel => !string.IsNullOrWhiteSpace(channel.Label))
            .GroupBy(channel => channel.Label.Trim(), StringComparer.OrdinalIgnoreCase)
            .Count(group => group.Count() > 1));
        List<string> warnings = [.. adapterResult.Warnings ?? []];
        if (channels.Any(channel => string.IsNullOrWhiteSpace(channel.Label)))
        {
            warnings.Add("Les labels vides seront ignorés lors de l'application.");
        }

        bool dmtRc1 = string.Equals(document.SourceVersion, "2.14.0-RC1", StringComparison.OrdinalIgnoreCase);
        if (dmtRc1)
        {
            warnings.Add("Structure DMT 2.14.0-RC1 reconnue par le schéma d'échange DCE version 1.");
        }

        return new ChannelLabelImportReport(
            adapterName,
            document.SourceApplication,
            document.SourceVersion,
            document.Sets.Count,
            document.Sets.Select(set => set.DeviceName).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
            channels.Length,
            adapterResult.IgnoredLineCount,
            channels.Count(channel => string.IsNullOrWhiteSpace(channel.Label)),
            duplicateLabels,
            [],
            warnings);
    }

    private sealed class JsonChannelLabelImportAdapter : IChannelLabelImportAdapter
    {
        public string Name => "JSON DCE/DMT";

        public IReadOnlySet<string> Extensions { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".json" };

        public ChannelLabelImportAdapterResult Read(string path) => new(
            ChannelLabelExchangeService.ParseJson(File.ReadAllText(path, System.Text.Encoding.UTF8)));
    }

    private sealed class CsvChannelLabelImportAdapter : IChannelLabelImportAdapter
    {
        public string Name => "CSV DCE/DMT ou console";

        public IReadOnlySet<string> Extensions { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".csv" };

        public ChannelLabelImportAdapterResult Read(string path) => new(
            ConsoleChannelFileService.IsConsoleCsv(path)
                ? ConsoleChannelFileService.Read(path).Document
                : ChannelLabelExchangeService.ParseCsv(File.ReadAllText(path, System.Text.Encoding.UTF8)));
    }

    private sealed class DmtXlsxChannelLabelImportAdapter : IChannelLabelImportAdapter
    {
        public string Name => "Classeur DMT XLSX";

        public IReadOnlySet<string> Extensions { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".xlsx" };

        public ChannelLabelImportAdapterResult Read(string path)
        {
            DmtWorkbookReadResult result = DmtChannelWorkbookService.Read(path);
            return new ChannelLabelImportAdapterResult(result.Document, result.IgnoredRowCount);
        }
    }

    private sealed class DmtOdsChannelLabelImportAdapter : IChannelLabelImportAdapter
    {
        public string Name => "Classeur DMT ODS";

        public IReadOnlySet<string> Extensions { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".ods" };

        public ChannelLabelImportAdapterResult Read(string path)
        {
            DmtWorkbookReadResult result = DmtOpenDocumentService.Read(path);
            return new ChannelLabelImportAdapterResult(result.Document, result.IgnoredRowCount);
        }
    }

    private sealed class ConsoleZipChannelLabelImportAdapter : IChannelLabelImportAdapter
    {
        public string Name => "Archive console Yamaha";

        public IReadOnlySet<string> Extensions { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".zip" };

        public ChannelLabelImportAdapterResult Read(string path) => new(ConsoleChannelFileService.Read(path).Document);
    }
}
