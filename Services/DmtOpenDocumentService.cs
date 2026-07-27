using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using DanteConfigEditor.Models;

namespace DanteConfigEditor.Services;

/// <summary>
/// Lit et crée les classeurs ODS produits par DMT sans reconstruire le
/// document. Seules les cellules Enabled et Name de la feuille Channels sont
/// modifiées ; les autres feuilles, styles et paramètres restent intacts.
/// </summary>
public static class DmtOpenDocumentService
{
    private static readonly XNamespace Office = "urn:oasis:names:tc:opendocument:xmlns:office:1.0";
    private static readonly XNamespace Table = "urn:oasis:names:tc:opendocument:xmlns:table:1.0";
    private static readonly XNamespace Text = "urn:oasis:names:tc:opendocument:xmlns:text:1.0";
    private static readonly XNamespace CalcExt = "urn:org:documentfoundation:names:experimental:calc:xmlns:calcext:1.0";

    public static DmtWorkbookReadResult Read(string path)
    {
        using FileStream stream = File.OpenRead(path);
        using ZipArchive archive = new(stream, ZipArchiveMode.Read);
        XDocument content = LoadContent(archive);
        XElement channelsTable = FindTable(content, "Channels");
        XElement? miscTable = TryFindTable(content, "Misc");
        string version = miscTable is null ? string.Empty : ReadPropertyValue(miscTable, "Version");
        ChannelReadSummary channelSummary = ReadChannels(channelsTable);
        if (channelSummary.Channels.Count == 0)
        {
            throw new InvalidDataException("La feuille Channels du fichier DMT ODS ne contient aucun label actif.");
        }

        ChannelLabelDocument document = ChannelLabelExchangeService.BuildDmtDocument(
            Path.GetFileNameWithoutExtension(path),
            channelSummary.Channels,
            string.IsNullOrWhiteSpace(version) ? "ODS" : $"ODS template {version}");
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
            throw new InvalidOperationException("Le modèle DMT ODS ne peut pas être lu.");
        }

        Dictionary<int, string> namesByChannel = DmtChannelWorkbookService.PrepareNames(labels, adaptLabels);
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
                XDocument content = LoadContent(archive);
                UpdateChannelNames(FindTable(content, "Channels"), namesByChannel, replaceChannelSet);
                ReplaceContentEntry(archive, content);
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

    private static ChannelReadSummary ReadChannels(XElement table)
    {
        XElement[] rows = table.Descendants(Table + "table-row").ToArray();
        HeaderLocation header = FindHeaders(rows, ["Channel", "Name"]);
        int enabledColumn = header.Columns.TryGetValue("Enabled", out int enabled) ? enabled : -1;
        List<ChannelLabelEntry> channels = [];
        int ignoredRows = 0;
        foreach (XElement row in rows.Skip(header.RowIndex + 1))
        {
            string channelValue = ReadCell(row, header.Columns["Channel"]);
            if (!int.TryParse(channelValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out int channelNumber)
                || channelNumber <= 0)
            {
                continue;
            }

            if (enabledColumn >= 0
                && string.Equals(ReadCell(row, enabledColumn).Trim(), "no", StringComparison.OrdinalIgnoreCase))
            {
                ignoredRows++;
                continue;
            }

            string label = ReadCell(row, header.Columns["Name"]).Trim();
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
        XElement table,
        IReadOnlyDictionary<int, string> namesByChannel,
        bool replaceChannelSet)
    {
        XElement[] rows = table.Descendants(Table + "table-row").ToArray();
        HeaderLocation header = FindHeaders(rows, ["Channel", "Name"]);
        int enabledColumn = header.Columns.TryGetValue("Enabled", out int enabled) ? enabled : -1;
        HashSet<int> written = [];
        foreach (XElement row in rows.Skip(header.RowIndex + 1))
        {
            if (!int.TryParse(ReadCell(row, header.Columns["Channel"]), NumberStyles.Integer, CultureInfo.InvariantCulture, out int channelNumber))
            {
                continue;
            }

            if (namesByChannel.TryGetValue(channelNumber, out string? label))
            {
                SetStringCell(row, header.Columns["Name"], label);
                if (replaceChannelSet && enabledColumn >= 0)
                {
                    SetStringCell(row, enabledColumn, "yes");
                }
                written.Add(channelNumber);
            }
            else if (replaceChannelSet)
            {
                SetStringCell(row, header.Columns["Name"], "-");
                if (enabledColumn >= 0)
                {
                    SetStringCell(row, enabledColumn, "no");
                }
            }
        }

        int[] missing = namesByChannel.Keys.Where(channel => !written.Contains(channel)).OrderBy(channel => channel).ToArray();
        if (missing.Length > 0)
        {
            throw new InvalidOperationException($"Le modèle DMT ODS ne contient pas les canaux suivants : {string.Join(", ", missing)}.");
        }
    }

    private static string ReadPropertyValue(XElement table, string propertyName)
    {
        XElement[] rows = table.Descendants(Table + "table-row").ToArray();
        HeaderLocation header = FindHeaders(rows, ["Property", "Value"]);
        foreach (XElement row in rows.Skip(header.RowIndex + 1))
        {
            if (string.Equals(ReadCell(row, header.Columns["Property"]).Trim(), propertyName, StringComparison.OrdinalIgnoreCase))
            {
                return ReadCell(row, header.Columns["Value"]).Trim();
            }
        }
        return string.Empty;
    }

    private static HeaderLocation FindHeaders(IReadOnlyList<XElement> rows, IReadOnlyList<string> requiredHeaders)
    {
        for (int rowIndex = 0; rowIndex < Math.Min(rows.Count, 20); rowIndex++)
        {
            Dictionary<string, int> columns = [];
            int logicalColumn = 0;
            foreach (XElement cell in Cells(rows[rowIndex]))
            {
                string value = ReadCellValue(cell).Trim();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    columns.TryAdd(value, logicalColumn);
                }
                logicalColumn = checked(logicalColumn + RepeatCount(cell));
                if (logicalColumn > 512 && requiredHeaders.All(columns.ContainsKey))
                {
                    break;
                }
            }

            if (requiredHeaders.All(columns.ContainsKey))
            {
                return new HeaderLocation(rowIndex, columns);
            }
        }

        throw new InvalidDataException($"Colonnes DMT ODS introuvables : {string.Join(", ", requiredHeaders)}.");
    }

    private static string ReadCell(XElement row, int columnIndex)
    {
        (XElement? Cell, _, _) = LocateCell(row, columnIndex);
        return Cell is null ? string.Empty : ReadCellValue(Cell);
    }

    private static string ReadCellValue(XElement cell)
    {
        string text = string.Concat(cell.Descendants(Text + "p").Select(paragraph => paragraph.Value));
        if (!string.IsNullOrEmpty(text))
        {
            return text;
        }
        return cell.Attribute(Office + "string-value")?.Value
            ?? cell.Attribute(Office + "value")?.Value
            ?? string.Empty;
    }

    private static void SetStringCell(XElement row, int columnIndex, string value)
    {
        XElement cell = EnsureSingleCell(row, columnIndex);
        cell.SetAttributeValue(Office + "value-type", "string");
        if (cell.Attribute(CalcExt + "value-type") is not null)
        {
            cell.SetAttributeValue(CalcExt + "value-type", "string");
        }
        cell.SetAttributeValue(Office + "value", null);
        cell.SetAttributeValue(Office + "string-value", null);
        cell.SetAttributeValue(Office + "boolean-value", null);
        cell.SetAttributeValue(Table + "formula", null);
        cell.Elements().Remove();
        cell.Add(new XElement(Text + "p", value));
    }

    private static XElement EnsureSingleCell(XElement row, int columnIndex)
    {
        (XElement? cell, int start, int repeat) = LocateCell(row, columnIndex);
        if (cell is null)
        {
            while (start < columnIndex)
            {
                row.Add(new XElement(Table + "table-cell"));
                start++;
            }
            XElement appended = new(Table + "table-cell");
            row.Add(appended);
            return appended;
        }

        if (repeat == 1)
        {
            return cell;
        }

        int beforeCount = columnIndex - start;
        int afterCount = repeat - beforeCount - 1;
        List<XElement> replacements = [];
        if (beforeCount > 0)
        {
            XElement before = new(cell);
            SetRepeat(before, beforeCount);
            replacements.Add(before);
        }
        XElement target = new(cell);
        target.SetAttributeValue(Table + "number-columns-repeated", null);
        replacements.Add(target);
        if (afterCount > 0)
        {
            XElement after = new(cell);
            SetRepeat(after, afterCount);
            replacements.Add(after);
        }
        cell.ReplaceWith(replacements);
        return target;
    }

    private static (XElement? Cell, int Start, int Repeat) LocateCell(XElement row, int columnIndex)
    {
        int current = 0;
        foreach (XElement cell in Cells(row))
        {
            int repeat = RepeatCount(cell);
            if (columnIndex >= current && columnIndex < current + repeat)
            {
                return (cell, current, repeat);
            }
            current = checked(current + repeat);
            if (current > columnIndex)
            {
                break;
            }
        }
        return (null, current, 0);
    }

    private static IEnumerable<XElement> Cells(XElement row) =>
        row.Elements().Where(element => element.Name == Table + "table-cell" || element.Name == Table + "covered-table-cell");

    private static int RepeatCount(XElement cell) =>
        int.TryParse(cell.Attribute(Table + "number-columns-repeated")?.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int repeat)
            && repeat > 0
            ? repeat
            : 1;

    private static void SetRepeat(XElement cell, int count) =>
        cell.SetAttributeValue(Table + "number-columns-repeated", count > 1 ? count : null);

    private static XElement FindTable(XDocument document, string name) =>
        TryFindTable(document, name)
        ?? throw new InvalidDataException($"Le fichier DMT ODS ne contient pas la feuille '{name}'.");

    private static XElement? TryFindTable(XDocument document, string name) =>
        document.Descendants(Table + "table")
            .FirstOrDefault(table => string.Equals(table.Attribute(Table + "name")?.Value, name, StringComparison.OrdinalIgnoreCase));

    private static XDocument LoadContent(ZipArchive archive)
    {
        ZipArchiveEntry entry = archive.GetEntry("content.xml")
            ?? throw new InvalidDataException("Le fichier ODS ne contient pas content.xml.");
        using Stream stream = entry.Open();
        return XDocument.Load(stream, LoadOptions.PreserveWhitespace);
    }

    private static void ReplaceContentEntry(ZipArchive archive, XDocument document)
    {
        archive.GetEntry("content.xml")?.Delete();
        ZipArchiveEntry replacement = archive.CreateEntry("content.xml", CompressionLevel.Optimal);
        using Stream stream = replacement.Open();
        using XmlWriter writer = XmlWriter.Create(stream, new XmlWriterSettings
        {
            Encoding = new UTF8Encoding(false),
            Indent = false,
            CloseOutput = false
        });
        document.Save(writer);
    }

    private sealed record HeaderLocation(int RowIndex, IReadOnlyDictionary<string, int> Columns);

    private sealed record ChannelReadSummary(
        IReadOnlyList<ChannelLabelEntry> Channels,
        int IgnoredRowCount);
}
