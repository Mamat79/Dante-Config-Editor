using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using DanteConfigEditor.Models;

namespace DanteConfigEditor.Services;

public static class DmtChannelWorkbookService
{
    private static readonly XNamespace SpreadsheetNamespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private static readonly XNamespace OfficeRelationshipNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    private static readonly XNamespace PackageRelationshipNamespace = "http://schemas.openxmlformats.org/package/2006/relationships";
    private static readonly HashSet<char> AllowedDmtCharacters = new(
        "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 !\"#%&'()*+,-./<=>?@[\\]_{|}~");

    public static DmtWorkbookReadResult Read(string path)
    {
        using FileStream stream = File.OpenRead(path);
        using ZipArchive archive = new(stream, ZipArchiveMode.Read);
        IReadOnlyList<string> sharedStrings = ReadSharedStrings(archive);
        XDocument channelsSheet = LoadWorksheet(archive, "Channels");
        XDocument? miscSheet = TryLoadWorksheet(archive, "Misc");
        string version = miscSheet is null ? string.Empty : ReadPropertyValue(miscSheet, sharedStrings, "Version");
        ChannelReadSummary channelSummary = ReadChannels(channelsSheet, sharedStrings);
        if (channelSummary.Channels.Count == 0)
        {
            throw new InvalidDataException("La feuille Channels du classeur DMT ne contient aucun label actif.");
        }

        ChannelLabelDocument document = ChannelLabelExchangeService.BuildDmtDocument(
            Path.GetFileNameWithoutExtension(path),
            channelSummary.Channels,
            string.IsNullOrWhiteSpace(version) ? "XLSX" : $"XLSX template {version}");
        return new DmtWorkbookReadResult(version, document, channelSummary.IgnoredRowCount);
    }

    public static void WriteCopy(string templatePath, string outputPath, ChannelLabelSet labels, bool adaptLabels)
    {
        string sourceFullPath = Path.GetFullPath(templatePath);
        string outputFullPath = Path.GetFullPath(outputPath);
        if (string.Equals(sourceFullPath, outputFullPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Le fichier de sortie doit être différent du modèle DMT original.");
        }

        using FileStream source = File.OpenRead(sourceFullPath);
        WriteFromTemplate(source, outputFullPath, labels, adaptLabels);
    }

    public static void WriteFromTemplate(
        Stream templateStream,
        string outputPath,
        ChannelLabelSet labels,
        bool adaptLabels,
        bool replaceChannelSet = false)
    {
        ArgumentNullException.ThrowIfNull(templateStream);
        ArgumentNullException.ThrowIfNull(labels);
        if (!templateStream.CanRead)
        {
            throw new InvalidOperationException("Le modèle DMT ne peut pas être lu.");
        }

        Dictionary<int, string> namesByChannel = PrepareNames(labels, adaptLabels);
        string outputFullPath = Path.GetFullPath(outputPath);
        string directory = Path.GetDirectoryName(outputFullPath) ?? Environment.CurrentDirectory;
        Directory.CreateDirectory(directory);
        string temporaryPath = Path.Combine(directory, $".{Path.GetFileName(outputFullPath)}.{Guid.NewGuid():N}.tmp");
        try
        {
            using (FileStream temporary = new(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                templateStream.CopyTo(temporary);
            }

            using (FileStream stream = new(temporaryPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            using (ZipArchive archive = new(stream, ZipArchiveMode.Update))
            {
                string worksheetPath = ResolveWorksheetPath(archive, "Channels");
                ZipArchiveEntry sheetEntry = archive.GetEntry(worksheetPath)
                    ?? throw new InvalidDataException("La feuille Channels du modèle DMT est introuvable.");
                XDocument sheet = LoadXml(sheetEntry);
                IReadOnlyList<string> sharedStrings = ReadSharedStrings(archive);
                UpdateChannelNames(sheet, sharedStrings, namesByChannel, replaceChannelSet);
                SaveXml(sheetEntry, sheet);
            }
            File.Move(temporaryPath, outputFullPath, true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    internal static Dictionary<int, string> PrepareNames(ChannelLabelSet labels, bool adaptLabels)
    {
        Dictionary<int, string> namesByChannel = [];
        foreach (ChannelLabelEntry channel in labels.Channels)
        {
            if (channel.ChannelNumber <= 0)
            {
                throw new InvalidOperationException("Un numéro de canal DMT est invalide.");
            }

            string label = channel.Label.Trim();
            if (string.IsNullOrWhiteSpace(label))
            {
                continue;
            }

            DmtLabelCompatibility compatibility = CheckCompatibility(label);
            if (!compatibility.IsCompatible && !adaptLabels)
            {
                throw new InvalidOperationException(
                    $"Le label '{label}' n'est pas compatible DMT : {string.Join("; ", compatibility.Warnings)}. Activez l'adaptation DMT ou utilisez JSON/CSV.");
            }

            if (!namesByChannel.TryAdd(channel.ChannelNumber, adaptLabels ? compatibility.AdaptedLabel : label))
            {
                throw new InvalidOperationException($"Le canal DMT {channel.ChannelNumber} est présent plusieurs fois.");
            }
        }

        if (namesByChannel.Count == 0)
        {
            throw new InvalidOperationException("Aucun label non vide à exporter vers DMT.");
        }
        return namesByChannel;
    }

    public static DmtLabelCompatibility CheckCompatibility(string label)
    {
        string clean = label?.Trim() ?? string.Empty;
        List<string> warnings = [];
        if (clean.Length > 8)
        {
            warnings.Add("plus de 8 caractères");
        }

        if (clean.Any(character => character > 127 || !AllowedDmtCharacters.Contains(character)))
        {
            warnings.Add("caractères non pris en charge par DMT/dLive");
        }

        string adapted = AdaptLabel(clean);
        return new DmtLabelCompatibility(
            string.Equals(clean, adapted, StringComparison.Ordinal) && warnings.Count == 0,
            adapted,
            warnings);
    }

    private static string AdaptLabel(string label)
    {
        string expanded = label
            .Replace("œ", "oe", StringComparison.Ordinal)
            .Replace("Œ", "OE", StringComparison.Ordinal)
            .Replace("æ", "ae", StringComparison.Ordinal)
            .Replace("Æ", "AE", StringComparison.Ordinal)
            .Replace("ß", "ss", StringComparison.Ordinal)
            .Replace("ø", "o", StringComparison.Ordinal)
            .Replace("Ø", "O", StringComparison.Ordinal)
            .Replace("ł", "l", StringComparison.Ordinal)
            .Replace("Ł", "L", StringComparison.Ordinal);
        string decomposed = expanded.Normalize(NormalizationForm.FormD);
        StringBuilder builder = new();
        foreach (Rune rune in decomposed.EnumerateRunes())
        {
            UnicodeCategory category = Rune.GetUnicodeCategory(rune);
            if (category == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (rune.IsAscii && AllowedDmtCharacters.Contains((char)rune.Value))
            {
                builder.Append((char)rune.Value);
            }
            else
            {
                builder.Append('_');
            }

            if (builder.Length >= 8)
            {
                break;
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    private static ChannelReadSummary ReadChannels(XDocument sheet, IReadOnlyList<string> sharedStrings)
    {
        XElement[] rows = sheet.Descendants(SpreadsheetNamespace + "row").ToArray();
        HeaderLocation header = FindHeaders(rows, sharedStrings, ["Channel", "Name"]);
        int enabledColumn = header.Columns.TryGetValue("Enabled", out int enabled) ? enabled : -1;
        List<ChannelLabelEntry> channels = [];
        int ignoredRows = 0;
        foreach (XElement row in rows.Skip(header.RowIndex + 1))
        {
            Dictionary<int, string> values = ReadRow(row, sharedStrings);
            if (!values.TryGetValue(header.Columns["Channel"], out string? channelValue)
                || !int.TryParse(channelValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out int channelNumber)
                || channelNumber <= 0)
            {
                continue;
            }

            if (enabledColumn >= 0
                && values.TryGetValue(enabledColumn, out string? enabledValue)
                && string.Equals(enabledValue.Trim(), "no", StringComparison.OrdinalIgnoreCase))
            {
                ignoredRows++;
                continue;
            }

            string label = values.TryGetValue(header.Columns["Name"], out string? name) ? name.Trim() : string.Empty;
            if (!string.IsNullOrWhiteSpace(label) && label is not "-" && !string.Equals(label, "byp", StringComparison.OrdinalIgnoreCase))
            {
                channels.Add(new ChannelLabelEntry(channelNumber, label, null));
            }
            else
            {
                ignoredRows++;
            }
        }

        return new ChannelReadSummary(channels, ignoredRows);
    }

    private static void UpdateChannelNames(
        XDocument sheet,
        IReadOnlyList<string> sharedStrings,
        IReadOnlyDictionary<int, string> namesByChannel,
        bool replaceChannelSet)
    {
        XElement[] rows = sheet.Descendants(SpreadsheetNamespace + "row").ToArray();
        HeaderLocation header = FindHeaders(rows, sharedStrings, ["Channel", "Name"]);
        int enabledColumn = header.Columns.TryGetValue("Enabled", out int enabled) ? enabled : -1;
        HashSet<int> written = [];
        foreach (XElement row in rows.Skip(header.RowIndex + 1))
        {
            Dictionary<int, string> values = ReadRow(row, sharedStrings);
            if (!values.TryGetValue(header.Columns["Channel"], out string? channelValue)
                || !int.TryParse(channelValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out int channelNumber))
            {
                continue;
            }

            if (namesByChannel.TryGetValue(channelNumber, out string? label))
            {
                SetInlineStringCell(row, header.Columns["Name"], label);
                if (replaceChannelSet && enabledColumn >= 0)
                {
                    SetInlineStringCell(row, enabledColumn, "yes");
                }
                written.Add(channelNumber);
            }
            else if (replaceChannelSet)
            {
                SetInlineStringCell(row, header.Columns["Name"], "-");
                if (enabledColumn >= 0)
                {
                    SetInlineStringCell(row, enabledColumn, "no");
                }
            }
        }

        int[] missing = namesByChannel.Keys.Where(channel => !written.Contains(channel)).OrderBy(channel => channel).ToArray();
        if (missing.Length > 0)
        {
            throw new InvalidOperationException($"Le modèle DMT ne contient pas les canaux suivants : {string.Join(", ", missing)}.");
        }
    }

    private static string ReadPropertyValue(XDocument sheet, IReadOnlyList<string> sharedStrings, string propertyName)
    {
        XElement[] rows = sheet.Descendants(SpreadsheetNamespace + "row").ToArray();
        HeaderLocation header = FindHeaders(rows, sharedStrings, ["Property", "Value"]);
        foreach (XElement row in rows.Skip(header.RowIndex + 1))
        {
            Dictionary<int, string> values = ReadRow(row, sharedStrings);
            if (values.TryGetValue(header.Columns["Property"], out string? property)
                && string.Equals(property.Trim(), propertyName, StringComparison.OrdinalIgnoreCase))
            {
                return values.TryGetValue(header.Columns["Value"], out string? value) ? value.Trim() : string.Empty;
            }
        }

        return string.Empty;
    }

    private static HeaderLocation FindHeaders(
        IReadOnlyList<XElement> rows,
        IReadOnlyList<string> sharedStrings,
        IReadOnlyList<string> requiredHeaders)
    {
        for (int rowIndex = 0; rowIndex < Math.Min(rows.Count, 20); rowIndex++)
        {
            Dictionary<int, string> values = ReadRow(rows[rowIndex], sharedStrings);
            Dictionary<string, int> columns = values
                .Where(item => !string.IsNullOrWhiteSpace(item.Value))
                .GroupBy(item => item.Value.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First().Key, StringComparer.OrdinalIgnoreCase);
            if (requiredHeaders.All(columns.ContainsKey))
            {
                return new HeaderLocation(rowIndex, columns);
            }
        }

        throw new InvalidDataException($"Colonnes DMT introuvables : {string.Join(", ", requiredHeaders)}.");
    }

    private static Dictionary<int, string> ReadRow(XElement row, IReadOnlyList<string> sharedStrings)
    {
        Dictionary<int, string> values = [];
        foreach (XElement cell in row.Elements(SpreadsheetNamespace + "c"))
        {
            string reference = cell.Attribute("r")?.Value ?? string.Empty;
            int column = ColumnIndex(reference);
            if (column >= 0)
            {
                values[column] = ReadCellValue(cell, sharedStrings);
            }
        }

        return values;
    }

    private static string ReadCellValue(XElement cell, IReadOnlyList<string> sharedStrings)
    {
        string type = cell.Attribute("t")?.Value ?? string.Empty;
        if (string.Equals(type, "inlineStr", StringComparison.OrdinalIgnoreCase))
        {
            return string.Concat(cell.Descendants(SpreadsheetNamespace + "t").Select(text => text.Value));
        }

        string value = cell.Element(SpreadsheetNamespace + "v")?.Value ?? string.Empty;
        if (string.Equals(type, "s", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int sharedIndex)
            && sharedIndex >= 0
            && sharedIndex < sharedStrings.Count)
        {
            return sharedStrings[sharedIndex];
        }

        return value;
    }

    private static void SetInlineStringCell(XElement row, int columnIndex, string value)
    {
        int rowNumber = int.TryParse(row.Attribute("r")?.Value, out int parsedRow) ? parsedRow : 1;
        string reference = $"{ColumnName(columnIndex)}{rowNumber}";
        XElement? cell = row.Elements(SpreadsheetNamespace + "c")
            .FirstOrDefault(candidate => string.Equals(candidate.Attribute("r")?.Value, reference, StringComparison.OrdinalIgnoreCase));
        if (cell is null)
        {
            cell = new XElement(SpreadsheetNamespace + "c", new XAttribute("r", reference));
            XElement? following = row.Elements(SpreadsheetNamespace + "c")
                .FirstOrDefault(candidate => ColumnIndex(candidate.Attribute("r")?.Value ?? string.Empty) > columnIndex);
            if (following is null)
            {
                row.Add(cell);
            }
            else
            {
                following.AddBeforeSelf(cell);
            }
        }

        cell.SetAttributeValue("t", "inlineStr");
        cell.Elements().Remove();
        XAttribute? space = value.Length != value.Trim().Length
            ? new XAttribute(XNamespace.Xml + "space", "preserve")
            : null;
        cell.Add(new XElement(SpreadsheetNamespace + "is", new XElement(SpreadsheetNamespace + "t", space, value)));
    }

    private static IReadOnlyList<string> ReadSharedStrings(ZipArchive archive)
    {
        ZipArchiveEntry? entry = archive.GetEntry("xl/sharedStrings.xml");
        if (entry is null)
        {
            return [];
        }

        XDocument document = LoadXml(entry);
        return document.Root?.Elements(SpreadsheetNamespace + "si")
            .Select(item => string.Concat(item.Descendants(SpreadsheetNamespace + "t").Select(text => text.Value)))
            .ToArray() ?? [];
    }

    private static XDocument LoadWorksheet(ZipArchive archive, string sheetName)
    {
        return TryLoadWorksheet(archive, sheetName)
            ?? throw new InvalidDataException($"Le classeur ne contient pas la feuille '{sheetName}'.");
    }

    private static XDocument? TryLoadWorksheet(ZipArchive archive, string sheetName)
    {
        try
        {
            string path = ResolveWorksheetPath(archive, sheetName);
            ZipArchiveEntry? entry = archive.GetEntry(path);
            return entry is null ? null : LoadXml(entry);
        }
        catch (InvalidDataException)
        {
            return null;
        }
    }

    private static string ResolveWorksheetPath(ZipArchive archive, string sheetName)
    {
        ZipArchiveEntry workbookEntry = archive.GetEntry("xl/workbook.xml")
            ?? throw new InvalidDataException("Le classeur XLSX ne contient pas xl/workbook.xml.");
        ZipArchiveEntry relationshipsEntry = archive.GetEntry("xl/_rels/workbook.xml.rels")
            ?? throw new InvalidDataException("Les relations du classeur XLSX sont absentes.");
        XDocument workbook = LoadXml(workbookEntry);
        XElement sheet = workbook.Descendants(SpreadsheetNamespace + "sheet")
            .FirstOrDefault(candidate => string.Equals(candidate.Attribute("name")?.Value, sheetName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidDataException($"Le classeur ne contient pas la feuille '{sheetName}'.");
        string relationshipId = sheet.Attribute(OfficeRelationshipNamespace + "id")?.Value
            ?? throw new InvalidDataException($"La feuille '{sheetName}' ne possède pas de relation XLSX.");
        XDocument relationships = LoadXml(relationshipsEntry);
        string target = relationships.Descendants(PackageRelationshipNamespace + "Relationship")
            .FirstOrDefault(candidate => string.Equals(candidate.Attribute("Id")?.Value, relationshipId, StringComparison.Ordinal))
            ?.Attribute("Target")?.Value
            ?? throw new InvalidDataException($"La relation de la feuille '{sheetName}' est introuvable.");
        string normalized = target.Replace('\\', '/').TrimStart('/');
        return normalized.StartsWith("xl/", StringComparison.OrdinalIgnoreCase) ? normalized : $"xl/{normalized}";
    }

    private static XDocument LoadXml(ZipArchiveEntry entry)
    {
        using Stream stream = entry.Open();
        return XDocument.Load(stream, LoadOptions.PreserveWhitespace);
    }

    private static void SaveXml(ZipArchiveEntry entry, XDocument document)
    {
        using Stream stream = entry.Open();
        stream.SetLength(0);
        using XmlWriter writer = XmlWriter.Create(stream, new XmlWriterSettings
        {
            Encoding = new UTF8Encoding(false),
            Indent = false,
            CloseOutput = false
        });
        document.Save(writer);
    }

    private static int ColumnIndex(string cellReference)
    {
        int result = 0;
        int letters = 0;
        foreach (char character in cellReference)
        {
            if (!char.IsLetter(character))
            {
                break;
            }

            result = checked(result * 26 + char.ToUpperInvariant(character) - 'A' + 1);
            letters++;
        }

        return letters == 0 ? -1 : result - 1;
    }

    private static string ColumnName(int index)
    {
        StringBuilder builder = new();
        int value = index + 1;
        while (value > 0)
        {
            value--;
            builder.Insert(0, (char)('A' + value % 26));
            value /= 26;
        }

        return builder.ToString();
    }

    private sealed record HeaderLocation(int RowIndex, IReadOnlyDictionary<string, int> Columns);

    private sealed record ChannelReadSummary(
        IReadOnlyList<ChannelLabelEntry> Channels,
        int IgnoredRowCount);
}
