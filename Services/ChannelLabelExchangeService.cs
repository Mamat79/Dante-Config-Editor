using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Reflection;
using DanteConfigEditor.Models;

namespace DanteConfigEditor.Services;

public static class ChannelLabelExchangeService
{
    public const string FormatName = "dante-config-editor-channel-labels";
    public const int CurrentSchemaVersion = 1;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public static ChannelLabelDocument Read(string path)
    {
        return ReadWithReport(path).Document;
    }

    public static ChannelLabelReadResult ReadWithReport(string path) =>
        ChannelLabelImportAdapterRegistry.Read(path);

    public static void Write(string path, ChannelLabelDocument document)
    {
        string extension = Path.GetExtension(path).ToLowerInvariant();
        string content = extension switch
        {
            ".json" => SerializeJson(document),
            ".csv" => SerializeCsv(document),
            _ => throw new InvalidDataException("Format d'export non pris en charge. Utilisez JSON ou CSV.")
        };

        Encoding encoding = extension == ".csv" ? new UTF8Encoding(true) : new UTF8Encoding(false);
        File.WriteAllText(path, content, encoding);
    }

    public static ChannelLabelDocument CreateFromProject(
        DanteProject project,
        IEnumerable<string> deviceNames,
        DanteChannelKind kind,
        int startChannel = 1,
        int? count = null)
    {
        ArgumentNullException.ThrowIfNull(project);
        HashSet<string> requested = new(deviceNames ?? [], StringComparer.OrdinalIgnoreCase);
        if (requested.Count == 0)
        {
            throw new InvalidOperationException("Sélectionnez au moins une machine à exporter.");
        }

        string[] missing = requested
            .Where(name => project.Devices.All(device => !string.Equals(device.Name, name, StringComparison.OrdinalIgnoreCase)))
            .ToArray();
        if (missing.Length > 0)
        {
            throw new InvalidOperationException($"Machine(s) introuvable(s) : {string.Join(", ", missing)}.");
        }

        List<ChannelLabelSet> sets = [];
        foreach (DanteDevice device in project.Devices.Where(device => requested.Contains(device.Name)))
        {
            IReadOnlyList<DanteChannel> channels = kind == DanteChannelKind.Tx ? device.TxChannels : device.RxChannels;
            IEnumerable<DanteChannel> selected = channels.Where(channel => channel.DanteId >= startChannel);
            if (count is > 0)
            {
                selected = selected.Take(count.Value);
            }

            ChannelLabelEntry[] entries = selected
                .Select(channel => new ChannelLabelEntry(channel.DanteId, channel.DisplayName, channel.DanteId))
                .ToArray();
            if (entries.Length == 0)
            {
                continue;
            }

            sets.Add(new ChannelLabelSet(
                device.Name,
                kind == DanteChannelKind.Tx ? ChannelLabelDirection.Tx : ChannelLabelDirection.Rx,
                entries));
        }

        if (sets.Count == 0)
        {
            string direction = kind == DanteChannelKind.Tx ? "TX" : "RX";
            throw new InvalidOperationException($"Aucune machine sélectionnée ne contient de canal {direction} dans la plage demandée.");
        }

        return new ChannelLabelDocument(FormatName, CurrentSchemaVersion, "Dante Config Editor", ReadApplicationVersion(), sets.ToArray());
    }

    public static string SerializeJson(ChannelLabelDocument document)
    {
        ValidateDocument(document);
        return JsonSerializer.Serialize(document, JsonOptions) + Environment.NewLine;
    }

    public static ChannelLabelDocument ParseJson(string json)
    {
        try
        {
            ChannelLabelDocument? document = JsonSerializer.Deserialize<ChannelLabelDocument>(json, JsonOptions);
            ValidateDocument(document);
            return document!;
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException("Le fichier JSON de labels est invalide.", ex);
        }
    }

    public static string SerializeCsv(ChannelLabelDocument document)
    {
        ValidateDocument(document);
        StringBuilder builder = new();
        AppendCsvRow(builder, ["format_version", "source_app", "source_version", "device", "direction", "channel", "dante_id", "label"]);
        foreach (ChannelLabelSet set in document.Sets)
        {
            foreach (ChannelLabelEntry channel in set.Channels.OrderBy(channel => channel.ChannelNumber))
            {
                AppendCsvRow(builder,
                [
                    document.SchemaVersion.ToString(CultureInfo.InvariantCulture),
                    document.SourceApplication,
                    document.SourceVersion ?? string.Empty,
                    set.DeviceName,
                    DirectionToken(set.Direction),
                    channel.ChannelNumber.ToString(CultureInfo.InvariantCulture),
                    channel.DanteId?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
                    channel.Label
                ]);
            }
        }

        return builder.ToString();
    }

    public static ChannelLabelDocument ParseCsv(string csv)
    {
        List<string[]> rows = ParseCsvRows(csv);
        if (rows.Count == 0)
        {
            throw new InvalidDataException("Le fichier CSV de labels est vide.");
        }

        // Le CSV Director produit par DMT commence par [Version] puis [Channels].
        if (rows.Any(row => row.Length > 0 && string.Equals(row[0].Trim(), "[Channels]", StringComparison.OrdinalIgnoreCase)))
        {
            List<ChannelLabelEntry> channels = rows
                .Where(row => row.Length >= 3 && string.Equals(row[0].Trim(), "Input", StringComparison.OrdinalIgnoreCase))
                .Select(row => new ChannelLabelEntry(ParsePositiveNumber(row[1], "channel"), row[2].Trim(), null))
                .Where(channel => !string.IsNullOrWhiteSpace(channel.Label))
                .ToList();
            return BuildDmtDocument(Path.GetFileNameWithoutExtension("DMT-Director.csv"), channels, "Director CSV");
        }

        Dictionary<string, int> headers = rows[0]
            .Select((value, index) => (Name: NormalizeHeader(value), Index: index))
            .GroupBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First().Index, StringComparer.OrdinalIgnoreCase);

        int deviceColumn = RequireColumn(headers, "device", "machine");
        int directionColumn = RequireColumn(headers, "direction", "kind", "type");
        int channelColumn = RequireColumn(headers, "channel", "number", "index");
        int labelColumn = RequireColumn(headers, "label", "name");
        int danteIdColumn = FindColumn(headers, "dante_id", "danteid");
        int sourceAppColumn = FindColumn(headers, "source_app", "sourceapplication");
        int sourceVersionColumn = FindColumn(headers, "source_version", "sourceversion");
        int formatVersionColumn = FindColumn(headers, "format_version", "schemaversion");

        if (formatVersionColumn >= 0)
        {
            string[] declaredVersions = rows.Skip(1)
                .Select(row => ValueAt(row, formatVersionColumn).Trim())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            if (declaredVersions.Length > 1)
            {
                throw new InvalidDataException(
                    $"Le CSV déclare plusieurs versions de format : {string.Join(", ", declaredVersions)}.");
            }
        }

        List<(string Device, ChannelLabelDirection Direction, ChannelLabelEntry Entry)> entries = [];
        foreach (string[] row in rows.Skip(1).Where(row => row.Any(value => !string.IsNullOrWhiteSpace(value))))
        {
            string device = ValueAt(row, deviceColumn).Trim();
            ChannelLabelDirection direction = ParseDirection(ValueAt(row, directionColumn));
            int channel = ParsePositiveNumber(ValueAt(row, channelColumn), "channel");
            string label = ValueAt(row, labelColumn).Trim();
            int? danteId = int.TryParse(ValueAt(row, danteIdColumn), NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedId) && parsedId > 0
                ? parsedId
                : null;
            entries.Add((device, direction, new ChannelLabelEntry(channel, label, danteId)));
        }

        int schemaVersion = formatVersionColumn >= 0
            ? ParsePositiveNumber(rows.Skip(1).Select(row => ValueAt(row, formatVersionColumn)).FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? "1", "format_version")
            : CurrentSchemaVersion;
        string sourceApplication = sourceAppColumn >= 0
            ? rows.Skip(1).Select(row => ValueAt(row, sourceAppColumn)).FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? "CSV"
            : "CSV";
        string? sourceVersion = sourceVersionColumn >= 0
            ? rows.Skip(1).Select(row => ValueAt(row, sourceVersionColumn)).FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))
            : null;

        ChannelLabelDocument document = new(
            FormatName,
            schemaVersion,
            sourceApplication,
            sourceVersion,
            entries.GroupBy(item => (item.Device, item.Direction))
                .Select(group => new ChannelLabelSet(group.Key.Device, group.Key.Direction, group.Select(item => item.Entry).ToArray()))
                .ToArray());
        ValidateDocument(document);
        return document;
    }

    internal static ChannelLabelDocument BuildDmtDocument(string name, IEnumerable<ChannelLabelEntry> channels, string sourceVersion)
    {
        return new ChannelLabelDocument(
            FormatName,
            CurrentSchemaVersion,
            "dlive-midi-tools",
            sourceVersion,
            [new ChannelLabelSet(name, ChannelLabelDirection.ConsoleInput, channels.ToArray())]);
    }

    internal static ChannelLabelDocument BuildConsoleDocument(
        string name,
        IEnumerable<ChannelLabelEntry> channels,
        string sourceApplication,
        string sourceVersion)
    {
        return new ChannelLabelDocument(
            FormatName,
            CurrentSchemaVersion,
            sourceApplication,
            sourceVersion,
            [new ChannelLabelSet(name, ChannelLabelDirection.ConsoleInput, channels.ToArray())]);
    }

    private static void ValidateDocument(ChannelLabelDocument? document)
    {
        if (document is null)
        {
            throw new InvalidDataException("Le document de labels est vide.");
        }

        if (!string.Equals(document.Format, FormatName, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException($"Format de labels inconnu : '{document.Format}'.");
        }

        if (document.SchemaVersion != CurrentSchemaVersion)
        {
            throw new InvalidDataException($"Version de format non prise en charge : {document.SchemaVersion}.");
        }

        if (document.Sets.Count == 0)
        {
            throw new InvalidDataException("Le document ne contient aucune liste de labels.");
        }

        foreach (ChannelLabelSet set in document.Sets)
        {
            if (string.IsNullOrWhiteSpace(set.DeviceName))
            {
                throw new InvalidDataException("Une liste de labels ne contient pas de nom de machine.");
            }

            if (set.Channels.Count == 0)
            {
                throw new InvalidDataException($"La liste '{set.DeviceName}' ne contient aucun canal.");
            }

            if (set.Channels.Any(channel => channel.ChannelNumber <= 0))
            {
                throw new InvalidDataException($"La liste '{set.DeviceName}' contient un numéro de canal invalide.");
            }

            int[] duplicateChannels = set.Channels
                .GroupBy(channel => channel.ChannelNumber)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .OrderBy(value => value)
                .ToArray();
            if (duplicateChannels.Length > 0)
            {
                throw new InvalidDataException(
                    $"La liste '{set.DeviceName}' contient plusieurs fois le(s) canal(aux) {string.Join(", ", duplicateChannels)}.");
            }

            if (set.Channels.Any(channel => channel.DanteId is <= 0))
            {
                throw new InvalidDataException($"La liste '{set.DeviceName}' contient un Dante ID invalide.");
            }

            if (set.Channels.Any(channel => channel.Label.Any(char.IsControl)))
            {
                throw new InvalidDataException($"La liste '{set.DeviceName}' contient un label avec des caractères non imprimables.");
            }
        }
    }

    private static string ReadApplicationVersion()
    {
        string? informationalVersion = typeof(ChannelLabelExchangeService).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;
        return string.IsNullOrWhiteSpace(informationalVersion) ? "3.6" : informationalVersion;
    }

    private static string DirectionToken(ChannelLabelDirection direction) => direction switch
    {
        ChannelLabelDirection.Tx => "tx",
        ChannelLabelDirection.Rx => "rx",
        ChannelLabelDirection.ConsoleInput => "console-input",
        _ => throw new InvalidDataException($"Direction inconnue : {direction}.")
    };

    private static ChannelLabelDirection ParseDirection(string value)
    {
        return value.Trim().ToLowerInvariant() switch
        {
            "tx" or "transmit" or "output" => ChannelLabelDirection.Tx,
            "rx" or "receive" or "input-dante" => ChannelLabelDirection.Rx,
            "console-input" or "console input" or "input" or "channel" => ChannelLabelDirection.ConsoleInput,
            _ => throw new InvalidDataException($"Direction de canal inconnue : '{value}'.")
        };
    }

    private static void AppendCsvRow(StringBuilder builder, IEnumerable<string?> values)
    {
        builder.AppendLine(string.Join(",", values.Select(EscapeCsv)));
    }

    private static string EscapeCsv(string? value)
    {
        string clean = value ?? string.Empty;
        return clean.IndexOfAny([',', '"', '\r', '\n']) >= 0
            ? $"\"{clean.Replace("\"", "\"\"")}\""
            : clean;
    }

    private static List<string[]> ParseCsvRows(string csv)
    {
        List<string[]> rows = [];
        List<string> row = [];
        StringBuilder field = new();
        bool quoted = false;
        for (int index = 0; index < csv.Length; index++)
        {
            char current = csv[index];
            if (quoted)
            {
                if (current == '"' && index + 1 < csv.Length && csv[index + 1] == '"')
                {
                    field.Append('"');
                    index++;
                }
                else if (current == '"')
                {
                    quoted = false;
                }
                else
                {
                    field.Append(current);
                }
            }
            else if (current == '"')
            {
                quoted = true;
            }
            else if (current == ',')
            {
                row.Add(field.ToString());
                field.Clear();
            }
            else if (current is '\r' or '\n')
            {
                if (current == '\r' && index + 1 < csv.Length && csv[index + 1] == '\n')
                {
                    index++;
                }

                row.Add(field.ToString());
                field.Clear();
                rows.Add(row.ToArray());
                row.Clear();
            }
            else
            {
                field.Append(current);
            }
        }

        if (quoted)
        {
            throw new InvalidDataException("Le CSV contient un champ entre guillemets non terminé.");
        }

        if (field.Length > 0 || row.Count > 0)
        {
            row.Add(field.ToString());
            rows.Add(row.ToArray());
        }

        return rows.Where(candidate => candidate.Any(value => !string.IsNullOrWhiteSpace(value))).ToList();
    }

    private static string NormalizeHeader(string value) => value.Trim().TrimStart('\uFEFF').Replace(" ", "_").ToLowerInvariant();

    private static int RequireColumn(IReadOnlyDictionary<string, int> headers, params string[] aliases)
    {
        int column = FindColumn(headers, aliases);
        return column >= 0 ? column : throw new InvalidDataException($"Colonne obligatoire absente : {aliases[0]}.");
    }

    private static int FindColumn(IReadOnlyDictionary<string, int> headers, params string[] aliases)
    {
        foreach (string alias in aliases)
        {
            if (headers.TryGetValue(NormalizeHeader(alias), out int index))
            {
                return index;
            }
        }

        return -1;
    }

    private static string ValueAt(IReadOnlyList<string> row, int index) => index >= 0 && index < row.Count ? row[index] : string.Empty;

    private static int ParsePositiveNumber(string value, string field)
    {
        if (!int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed) || parsed <= 0)
        {
            throw new InvalidDataException($"Valeur invalide pour {field} : '{value}'.");
        }

        return parsed;
    }
}
