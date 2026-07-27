using System.IO;
using System.Reflection;
using System.Text;

namespace DanteConfigEditor.Services;

public static class ReportExportService
{
    private const int LinesPerPage = 54;
    private const int MaxLineLength = 92;

    public static void ExportText(string path, string report, bool includeSignature = true)
    {
        File.WriteAllText(path, includeSignature ? AddSignature(report) : report, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
    }

    public static void ExportPdf(string path, string title, string report)
    {
        // Générateur PDF minimal sans dépendance externe. Suffisant pour un
        // rapport texte simple et facile à embarquer dans l'application.
        report = AddSignature(report);
        List<string> lines = WrapLines(report.Replace("\r\n", "\n").Split('\n')).ToList();
        if (lines.Count == 0)
        {
            lines.Add(title);
        }

        List<List<string>> pages = lines
            .Select((line, index) => new { line, index })
            .GroupBy(item => item.index / LinesPerPage)
            .Select(group => group.Select(item => item.line).ToList())
            .ToList();

        List<byte[]> objects = [];
        List<int> pageObjectNumbers = [];

        objects.Add(Ascii("<< /Type /Catalog /Pages 2 0 R >>"));
        objects.Add([]);
        objects.Add(Ascii("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>"));

        foreach (List<string> page in pages)
        {
            int pageObjectNumber = objects.Count + 1;
            int contentObjectNumber = objects.Count + 2;
            pageObjectNumbers.Add(pageObjectNumber);

            objects.Add(Ascii($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 3 0 R >> >> /Contents {contentObjectNumber} 0 R >>"));
            byte[] content = BuildPageContent(page);
            objects.Add(Ascii($"<< /Length {content.Length} >>\nstream\n")
                .Concat(content)
                .Concat(Ascii("\nendstream"))
                .ToArray());
        }

        string kids = string.Join(" ", pageObjectNumbers.Select(number => $"{number} 0 R"));
        objects[1] = Ascii($"<< /Type /Pages /Kids [{kids}] /Count {pageObjectNumbers.Count} >>");

        using FileStream stream = File.Create(path);
        WriteAscii(stream, "%PDF-1.4\n");
        List<long> offsets = [0];

        for (int index = 0; index < objects.Count; index++)
        {
            offsets.Add(stream.Position);
            WriteAscii(stream, $"{index + 1} 0 obj\n");
            stream.Write(objects[index]);
            WriteAscii(stream, "\nendobj\n");
        }

        long xrefOffset = stream.Position;
        WriteAscii(stream, $"xref\n0 {objects.Count + 1}\n");
        WriteAscii(stream, "0000000000 65535 f \n");
        foreach (long offset in offsets.Skip(1))
        {
            WriteAscii(stream, $"{offset:0000000000} 00000 n \n");
        }

        WriteAscii(stream, $"trailer\n<< /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xrefOffset}\n%%EOF");
    }

    private static IEnumerable<string> WrapLines(IEnumerable<string> inputLines)
    {
        // Les lignes trop longues sont coupées pour rester dans la largeur A4.
        foreach (string inputLine in inputLines)
        {
            string line = inputLine.Replace("\t", "    ").TrimEnd();
            if (line.Length == 0)
            {
                yield return string.Empty;
                continue;
            }

            while (line.Length > MaxLineLength)
            {
                int split = line.LastIndexOf(' ', MaxLineLength);
                if (split < 20)
                {
                    split = MaxLineLength;
                }

                yield return line[..split].TrimEnd();
                line = line[split..].TrimStart();
            }

            yield return line;
        }
    }

    private static byte[] BuildPageContent(IEnumerable<string> lines)
    {
        StringBuilder builder = new();
        builder.AppendLine("BT");
        builder.AppendLine("/F1 10 Tf");
        builder.AppendLine("45 805 Td");
        builder.AppendLine("13 TL");

        foreach (string line in lines)
        {
            builder.Append('(');
            builder.Append(EscapePdfText(line));
            builder.AppendLine(") Tj");
            builder.AppendLine("T*");
        }

        builder.Append("ET");
        return Encoding.Latin1.GetBytes(builder.ToString());
    }

    private static string EscapePdfText(string value)
    {
        // Le PDF texte n'accepte pas directement tous les caractères Unicode.
        // Les caractères non compatibles sont remplacés pour garder un PDF lisible.
        StringBuilder builder = new();
        foreach (char character in value)
        {
            if (character is '(' or ')' or '\\')
            {
                builder.Append('\\');
                builder.Append(character);
            }
            else if (character <= 255 && !char.IsControl(character))
            {
                builder.Append(character);
            }
            else if (character == '\t')
            {
                builder.Append("    ");
            }
            else if (!char.IsControl(character))
            {
                builder.Append('?');
            }
        }

        return builder.ToString();
    }

    private static string AddSignature(string report)
    {
        string cleanReport = report.TrimEnd();
        return cleanReport
            + Environment.NewLine
            + Environment.NewLine
            + BuildSignatureLine()
            + Environment.NewLine;
    }

    private static string BuildSignatureLine()
    {
        string version = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "version inconnue";
        int metadataIndex = version.IndexOf('+', StringComparison.Ordinal);
        if (metadataIndex > 0)
        {
            version = version[..metadataIndex];
        }

        return $"Fait avec le soft Dante Config Editor V3.6 - version {version} - By Mamat et ses agents";
    }

    private static byte[] Ascii(string value)
    {
        return Encoding.ASCII.GetBytes(value);
    }

    private static void WriteAscii(Stream stream, string value)
    {
        stream.Write(Ascii(value));
    }
}
