using System.Globalization;
using System.IO;
using System.Text;
using DanteConfigEditor.Models;

namespace DanteConfigEditor.Services;

internal static class SynopticPdfExportService
{
    private const double MaximumPageDimension = 7200;

    public static void Export(string path, SynopticDiagram diagram, bool english)
    {
        ArgumentNullException.ThrowIfNull(diagram);
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllBytes(path, BuildPdf(diagram, english));
    }

    private static byte[] BuildPdf(SynopticDiagram diagram, bool english)
    {
        double scale = Math.Min(1, Math.Min(
            MaximumPageDimension / Math.Max(1, diagram.Width),
            MaximumPageDimension / Math.Max(1, diagram.Height)));
        double pageWidth = diagram.Width * scale;
        double pageHeight = diagram.Height * scale;
        byte[] content = Encoding.Latin1.GetBytes(BuildPageContent(diagram, english, scale));

        List<byte[]> objects =
        [
            Ascii("<< /Type /Catalog /Pages 2 0 R >>"),
            Ascii("<< /Type /Pages /Kids [5 0 R] /Count 1 >>"),
            Ascii("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>"),
            Ascii("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>"),
            Ascii($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {Number(pageWidth)} {Number(pageHeight)}] /Resources << /Font << /F1 3 0 R /F2 4 0 R >> >> /Contents 6 0 R >>"),
            Ascii($"<< /Length {content.Length} >>\nstream\n")
                .Concat(content)
                .Concat(Ascii("\nendstream"))
                .ToArray()
        ];

        using MemoryStream stream = new();
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
        return stream.ToArray();
    }

    private static string BuildPageContent(SynopticDiagram diagram, bool english, double scale)
    {
        StringBuilder pdf = new();
        pdf.AppendLine("q");
        pdf.AppendLine($"{Number(scale)} 0 0 {Number(scale)} 0 0 cm");
        FillRectangle(pdf, 0, 0, diagram.Width, diagram.Height, "#F7F9FC", diagram.Height);
        DrawText(pdf, 34, 38, 24, diagram.Title, "#172033", bold: true, diagram.Height);
        DrawText(
            pdf,
            34,
            62,
            12,
            english ? "Grouped Dante subscriptions - offline export" : "Abonnements Dante regroupés - export hors ligne",
            "#526070",
            bold: false,
            diagram.Height);

        foreach (SynopticLocationArea location in diagram.Locations)
        {
            FillAndStrokeRectangle(pdf, location.X, location.Y, location.Width, location.Height, "#FFFFFF", location.Color, 2, diagram.Height);
            FillRectangle(pdf, location.X, location.Y, location.Width, 36, location.Color, diagram.Height);
            DrawText(pdf, location.X + 14, location.Y + 24, 14, location.Name, "#FFFFFF", bold: true, diagram.Height);
        }

        foreach (SynopticCable cable in diagram.Cables)
        {
            DrawPolyline(pdf, cable, "#FFFFFF", 8, diagram.Height);
            DrawPolyline(pdf, cable, cable.Color, 3.5, diagram.Height);
        }

        foreach (SynopticDeviceNode node in diagram.Devices)
        {
            FillAndStrokeRectangle(pdf, node.X, node.Y, node.Width, node.Height, "#FFFFFF", node.Color, 2, diagram.Height);
            FillRectangle(pdf, node.X, node.Y, 8, node.Height, node.Color, diagram.Height);
            DrawText(pdf, node.X + 20, node.Y + 29, 16, Trim(node.Name, 28), "#172033", bold: true, diagram.Height);
            if (!string.IsNullOrWhiteSpace(node.FriendlyName)
                && !string.Equals(node.FriendlyName, node.Name, StringComparison.OrdinalIgnoreCase))
            {
                DrawText(pdf, node.X + 20, node.Y + 49, 11, Trim(node.FriendlyName, 32), "#526070", bold: false, diagram.Height);
            }
            DrawText(pdf, node.X + 20, node.Y + 68, 11, $"TX {node.TxCount}   RX {node.RxCount}", "#526070", bold: false, diagram.Height);
        }

        // Les pointes sont volontairement dessinées après les cartes : sinon les
        // extrémités, placées sur leur bord, disparaissent sous leur fond blanc.
        foreach (SynopticCable cable in diagram.Cables)
        {
            DrawArrow(pdf, cable, diagram.Height);
            if (cable.IsBidirectional)
            {
                DrawArrow(pdf, cable, diagram.Height, atStart: true);
            }
        }

        if (diagram.Cables.Count <= 18)
        {
            for (int index = 0; index < diagram.Cables.Count; index++)
            {
                SynopticCable cable = diagram.Cables[index];
                FillAndStrokeCircle(pdf, cable.LabelX, cable.LabelY, 9, "#FFFFFF", cable.Color, 2, diagram.Height);
                DrawCenteredText(pdf, cable.LabelX, cable.LabelY + 3, 8, (index + 1).ToString(CultureInfo.InvariantCulture), "#172033", bold: true, diagram.Height);
            }
        }

        DrawLegend(pdf, diagram, english);

        string summary = english
            ? $"{diagram.Devices.Count} devices shown - {diagram.Cables.Count} grouped cables - {diagram.HiddenDeviceCount} hidden - {diagram.SkippedPatchCount} subscriptions outside selection"
            : $"{diagram.Devices.Count} machines affichees - {diagram.Cables.Count} cables regroupes - {diagram.HiddenDeviceCount} masquee(s) - {diagram.SkippedPatchCount} patch(s) hors selection";
        DrawText(pdf, 34, diagram.Height - 30, 11, summary, "#526070", bold: false, diagram.Height);
        DrawRightText(pdf, diagram.Width - 34, diagram.Height - 30, 10, "Dante Config Editor 2026.1 - By Mamat et ses agents  -------[]--", "#718096", bold: false, diagram.Height);
        pdf.AppendLine("Q");
        return pdf.ToString();
    }

    private static void DrawLegend(StringBuilder pdf, SynopticDiagram diagram, bool english)
    {
        double topMargin = diagram.Locations.Count == 0 ? 88 : diagram.Locations.Min(location => location.Y);
        double legendHeight = diagram.Height - topMargin - 76;
        FillAndStrokeRectangle(pdf, diagram.LegendX, topMargin, diagram.LegendWidth, legendHeight, "#FFFFFF", "#CBD5E1", 1.5, diagram.Height);
        DrawText(pdf, diagram.LegendX + 18, topMargin + 29, 15, english ? "Grouped subscriptions" : "Liaisons regroupees", "#172033", bold: true, diagram.Height);

        int columns = Math.Max(1, diagram.LegendColumns);
        double gap = 10;
        double itemWidth = (diagram.LegendWidth - 24 - (columns - 1) * gap) / columns;
        double y = topMargin + 48;
        for (int rowStart = 0; rowStart < diagram.Cables.Count; rowStart += columns)
        {
            SynopticCable[] row = diagram.Cables.Skip(rowStart).Take(columns).ToArray();
            double rowHeight = row.Max(cable => Math.Max(62, 50 + cable.Labels.Count * 16));
            for (int column = 0; column < row.Length; column++)
            {
                int index = rowStart + column;
                SynopticCable cable = row[column];
                double itemX = diagram.LegendX + 12 + column * (itemWidth + gap);
                FillAndStrokeRectangle(pdf, itemX, y, itemWidth, rowHeight - 6, "#F8FAFC", "#E2E8F0", 1, diagram.Height);
                FillCircle(pdf, itemX + 19, y + 20, 11, cable.Color, diagram.Height);
                DrawCenteredText(pdf, itemX + 19, y + 24, 10, (index + 1).ToString(CultureInfo.InvariantCulture), "#FFFFFF", bold: true, diagram.Height);
                int titleLength = columns == 1 ? 52 : 38;
                DrawText(pdf, itemX + 38, y + 23, 12, Trim($"{cable.SourceDevice} {(cable.IsBidirectional ? "<->" : "->")} {cable.TargetDevice}", titleLength), "#172033", bold: true, diagram.Height);
                for (int labelIndex = 0; labelIndex < cable.Labels.Count; labelIndex++)
                {
                    int labelLength = columns == 1 ? 58 : 42;
                    DrawText(pdf, itemX + 38, y + 42 + labelIndex * 16, 10.5, Trim(cable.Labels[labelIndex], labelLength), "#526070", bold: false, diagram.Height);
                }
            }
            y += rowHeight;
        }
    }

    private static void DrawPolyline(StringBuilder pdf, SynopticCable cable, string color, double width, double pageHeight)
    {
        IReadOnlyList<SynopticRoutePoint> points = cable.RoutePoints.Count == 0
            ? [new SynopticRoutePoint(cable.StartX, cable.StartY), new SynopticRoutePoint(cable.EndX, cable.EndY)]
            : cable.RoutePoints;
        if (points.Count < 2)
        {
            return;
        }

        SetStroke(pdf, color);
        pdf.AppendLine($"{Number(width)} w 1 J 1 j");
        pdf.AppendLine($"{Number(points[0].X)} {Number(PdfY(points[0].Y, pageHeight))} m");
        foreach (SynopticRoutePoint point in points.Skip(1))
        {
            pdf.AppendLine($"{Number(point.X)} {Number(PdfY(point.Y, pageHeight))} l");
        }
        pdf.AppendLine("S");
    }

    private static void DrawArrow(StringBuilder pdf, SynopticCable cable, double pageHeight, bool atStart = false)
    {
        IReadOnlyList<SynopticRoutePoint> points = cable.RoutePoints;
        if (points.Count < 2)
        {
            return;
        }

        SynopticRoutePoint tip = atStart ? points[0] : points[^1];
        SynopticRoutePoint previous = atStart ? points[1] : points[^2];
        double dx = tip.X - previous.X;
        double dy = tip.Y - previous.Y;
        double length = Math.Sqrt(dx * dx + dy * dy);
        if (length < 0.01)
        {
            return;
        }

        dx /= length;
        dy /= length;
        double backX = tip.X - dx * 8;
        double backY = tip.Y - dy * 8;
        double perpendicularX = -dy * 3.5;
        double perpendicularY = dx * 3.5;
        SetFill(pdf, cable.Color);
        pdf.AppendLine($"{Number(tip.X)} {Number(PdfY(tip.Y, pageHeight))} m");
        pdf.AppendLine($"{Number(backX + perpendicularX)} {Number(PdfY(backY + perpendicularY, pageHeight))} l");
        pdf.AppendLine($"{Number(backX - perpendicularX)} {Number(PdfY(backY - perpendicularY, pageHeight))} l h f");
    }

    private static void FillRectangle(StringBuilder pdf, double x, double y, double width, double height, string fill, double pageHeight)
    {
        SetFill(pdf, fill);
        pdf.AppendLine($"{Number(x)} {Number(pageHeight - y - height)} {Number(width)} {Number(height)} re f");
    }

    private static void FillAndStrokeRectangle(StringBuilder pdf, double x, double y, double width, double height, string fill, string stroke, double strokeWidth, double pageHeight)
    {
        SetFill(pdf, fill);
        SetStroke(pdf, stroke);
        pdf.AppendLine($"{Number(strokeWidth)} w {Number(x)} {Number(pageHeight - y - height)} {Number(width)} {Number(height)} re B");
    }

    private static void FillCircle(StringBuilder pdf, double centerX, double centerY, double radius, string fill, double pageHeight)
    {
        SetFill(pdf, fill);
        AppendCirclePath(pdf, centerX, centerY, radius, pageHeight);
        pdf.AppendLine("f");
    }

    private static void FillAndStrokeCircle(StringBuilder pdf, double centerX, double centerY, double radius, string fill, string stroke, double strokeWidth, double pageHeight)
    {
        SetFill(pdf, fill);
        SetStroke(pdf, stroke);
        pdf.AppendLine($"{Number(strokeWidth)} w");
        AppendCirclePath(pdf, centerX, centerY, radius, pageHeight);
        pdf.AppendLine("B");
    }

    private static void AppendCirclePath(StringBuilder pdf, double centerX, double centerY, double radius, double pageHeight)
    {
        const double factor = 0.5522847498;
        double control = radius * factor;
        double y = PdfY(centerY, pageHeight);
        pdf.AppendLine($"{Number(centerX + radius)} {Number(y)} m");
        pdf.AppendLine($"{Number(centerX + radius)} {Number(y + control)} {Number(centerX + control)} {Number(y + radius)} {Number(centerX)} {Number(y + radius)} c");
        pdf.AppendLine($"{Number(centerX - control)} {Number(y + radius)} {Number(centerX - radius)} {Number(y + control)} {Number(centerX - radius)} {Number(y)} c");
        pdf.AppendLine($"{Number(centerX - radius)} {Number(y - control)} {Number(centerX - control)} {Number(y - radius)} {Number(centerX)} {Number(y - radius)} c");
        pdf.AppendLine($"{Number(centerX + control)} {Number(y - radius)} {Number(centerX + radius)} {Number(y - control)} {Number(centerX + radius)} {Number(y)} c h");
    }

    private static void DrawText(StringBuilder pdf, double x, double baselineY, double size, string text, string color, bool bold, double pageHeight)
    {
        SetFill(pdf, color);
        pdf.AppendLine($"BT /{(bold ? "F2" : "F1")} {Number(size)} Tf 1 0 0 1 {Number(x)} {Number(PdfY(baselineY, pageHeight))} Tm ({EscapeText(text)}) Tj ET");
    }

    private static void DrawCenteredText(StringBuilder pdf, double centerX, double baselineY, double size, string text, string color, bool bold, double pageHeight)
    {
        DrawText(pdf, centerX - EstimateTextWidth(text, size) / 2, baselineY, size, text, color, bold, pageHeight);
    }

    private static void DrawRightText(StringBuilder pdf, double rightX, double baselineY, double size, string text, string color, bool bold, double pageHeight)
    {
        DrawText(pdf, rightX - EstimateTextWidth(text, size), baselineY, size, text, color, bold, pageHeight);
    }

    private static double EstimateTextWidth(string value, double size) => NormalizeText(value).Length * size * 0.52;

    private static void SetFill(StringBuilder pdf, string color)
    {
        PdfColor parsed = ParseColor(color);
        pdf.AppendLine($"{Number(parsed.Red)} {Number(parsed.Green)} {Number(parsed.Blue)} rg");
    }

    private static void SetStroke(StringBuilder pdf, string color)
    {
        PdfColor parsed = ParseColor(color);
        pdf.AppendLine($"{Number(parsed.Red)} {Number(parsed.Green)} {Number(parsed.Blue)} RG");
    }

    private static PdfColor ParseColor(string value)
    {
        string hex = value.Trim().TrimStart('#');
        if (hex.Length != 6 || !int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int rgb))
        {
            return new PdfColor(0, 0, 0);
        }

        return new PdfColor(
            ((rgb >> 16) & 0xFF) / 255d,
            ((rgb >> 8) & 0xFF) / 255d,
            (rgb & 0xFF) / 255d);
    }

    private static string EscapeText(string value)
    {
        StringBuilder escaped = new();
        foreach (char character in NormalizeText(value))
        {
            if (character is '(' or ')' or '\\')
            {
                escaped.Append('\\');
            }
            escaped.Append(character);
        }
        return escaped.ToString();
    }

    private static string NormalizeText(string value)
    {
        StringBuilder normalized = new();
        foreach (char character in value)
        {
            if (character == '→')
            {
                normalized.Append("->");
            }
            else if (character == '…')
            {
                normalized.Append("...");
            }
            else if (character is '–' or '—')
            {
                normalized.Append('-');
            }
            else if (character <= 255 && !char.IsControl(character))
            {
                normalized.Append(character);
            }
            else if (!char.IsControl(character))
            {
                normalized.Append('?');
            }
        }
        return normalized.ToString();
    }

    private static string Trim(string value, int maximumLength)
    {
        string clean = string.IsNullOrWhiteSpace(value) ? "?" : value.Trim();
        return clean.Length <= maximumLength ? clean : clean[..Math.Max(1, maximumLength - 3)] + "...";
    }

    private static double PdfY(double topY, double pageHeight) => pageHeight - topY;

    private static string Number(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);

    private static byte[] Ascii(string value) => Encoding.ASCII.GetBytes(value);

    private static void WriteAscii(Stream stream, string value) => stream.Write(Ascii(value));

    private readonly record struct PdfColor(double Red, double Green, double Blue);
}
