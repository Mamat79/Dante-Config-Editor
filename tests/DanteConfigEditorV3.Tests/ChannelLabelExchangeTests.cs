using System.IO.Compression;
using System.Text;
using DanteConfigEditor.Models;
using DanteConfigEditor.Services;
using DanteConfigEditorV3.TestSupport;

namespace DanteConfigEditorV3.Tests;

public sealed class ChannelLabelExchangeTests
{
    [Fact]
    public void JsonRoundTripPreservesSeveralDevicesDirectionsAndUnicode()
    {
        ChannelLabelDocument source = new(
            ChannelLabelExchangeService.FormatName,
            ChannelLabelExchangeService.CurrentSchemaVersion,
            "Test Suite",
            "1.0",
            [
                new ChannelLabelSet("FOH", ChannelLabelDirection.Tx,
                    [new ChannelLabelEntry(1, "Départ façade", 11)]),
                new ChannelLabelSet("STAGE", ChannelLabelDirection.Rx,
                    [new ChannelLabelEntry(2, "Entrée scène", 22)])
            ]);

        string json = ChannelLabelExchangeService.SerializeJson(source);
        ChannelLabelDocument result = ChannelLabelExchangeService.ParseJson(json);

        Assert.Equal(ChannelLabelExchangeService.SerializeJson(source), ChannelLabelExchangeService.SerializeJson(result));
    }

    [Fact]
    public void CsvRoundTripHandlesQuotedLabelsAndSeveralDevices()
    {
        ChannelLabelDocument source = new(
            ChannelLabelExchangeService.FormatName,
            ChannelLabelExchangeService.CurrentSchemaVersion,
            "Test Suite",
            "1.0",
            [
                new ChannelLabelSet("FOH, MAIN", ChannelLabelDirection.Tx,
                    [new ChannelLabelEntry(1, "Lead \"Vox\"", 1)]),
                new ChannelLabelSet("STAGE", ChannelLabelDirection.Rx,
                    [new ChannelLabelEntry(3, "Ambiance, L", 3)])
            ]);

        string csv = ChannelLabelExchangeService.SerializeCsv(source);
        ChannelLabelDocument result = ChannelLabelExchangeService.ParseCsv(csv);

        Assert.Equal(ChannelLabelExchangeService.SerializeJson(source), ChannelLabelExchangeService.SerializeJson(result));
    }

    [Fact]
    public void DmtWorkbookReadsEnabledChannelNamesAndVersion()
    {
        using TemporaryDirectory temp = new();
        string template = CreateDmtWorkbook(temp.Path, [
            ("yes", 1, "KickIn"),
            ("no", 2, "SkipMe"),
            ("yes", 3, "SnTop")
        ]);

        DmtWorkbookReadResult result = DmtChannelWorkbookService.Read(template);

        Assert.Equal("14", result.TemplateVersion);
        Assert.Equal(1, result.IgnoredRowCount);
        ChannelLabelSet set = Assert.Single(result.Document.Sets);
        Assert.Equal(ChannelLabelDirection.ConsoleInput, set.Direction);
        Assert.Collection(
            set.Channels,
            channel => Assert.Equal(new ChannelLabelEntry(1, "KickIn", null), channel),
            channel => Assert.Equal(new ChannelLabelEntry(3, "SnTop", null), channel));
    }

    [Fact]
    public void DmtWorkbookExportPreservesTemplateAndWritesCompatibleNames()
    {
        using TemporaryDirectory temp = new();
        string template = CreateDmtWorkbook(temp.Path, [
            ("yes", 1, "OldOne"),
            ("yes", 2, "OldTwo")
        ]);
        string originalHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(template)));
        string output = System.IO.Path.Combine(temp.Path, "export.xlsx");
        ChannelLabelSet labels = new("DANTE-A", ChannelLabelDirection.Tx, [
            new ChannelLabelEntry(1, "TrèsLongLabel", 1),
            new ChannelLabelEntry(2, "Chœur", 2)
        ]);

        DmtChannelWorkbookService.WriteCopy(template, output, labels, adaptLabels: true);

        Assert.Equal(originalHash, Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(template))));
        DmtWorkbookReadResult result = DmtChannelWorkbookService.Read(output);
        Assert.Equal("14", result.TemplateVersion);
        Assert.Equal(["TresLong", "Choeur"], result.Document.Sets.Single().Channels.Select(channel => channel.Label));
    }

    [Fact]
    public void DeviceWithoutTxCanStillExportItsRxChannels()
    {
        using TemporaryDirectory temp = new();
        string xml = System.IO.Path.Combine(temp.Path, "rx-only.xml");
        SyntheticPresetFactory.Create(xml, 1, txPerDevice: 0, rxPerDevice: 2);
        DanteProject project = DanteProject.Load(xml);

        InvalidOperationException error = Assert.Throws<InvalidOperationException>(() =>
            ChannelLabelExchangeService.CreateFromProject(project, ["DEVICE-001"], DanteChannelKind.Tx));
        ChannelLabelDocument rx = ChannelLabelExchangeService.CreateFromProject(
            project,
            ["DEVICE-001"],
            DanteChannelKind.Rx);

        Assert.Contains("TX", error.Message, StringComparison.Ordinal);
        Assert.Equal(2, rx.Sets.Single().Channels.Count);
        Assert.Equal(ChannelLabelDirection.Rx, rx.Sets.Single().Direction);
    }

    [Fact]
    public void TransferPlannerMapsOneSourceRangeToSeveralDevices()
    {
        using TemporaryDirectory temp = new();
        string xml = System.IO.Path.Combine(temp.Path, "multi.xml");
        SyntheticPresetFactory.Create(xml, 2, txPerDevice: 4, rxPerDevice: 4);
        DanteProject project = DanteProject.Load(xml);
        ChannelLabelSet source = new("dLive", ChannelLabelDirection.ConsoleInput, [
            new ChannelLabelEntry(10, "Kick", null),
            new ChannelLabelEntry(11, "Snare", null)
        ]);

        IReadOnlyList<ChannelLabelTransferPreviewRow> preview = ChannelLabelTransferPlanner.BuildPreview(
            project,
            source,
            ["DEVICE-001", "DEVICE-002"],
            DanteChannelKind.Rx,
            sourceStartChannel: 10,
            targetStartChannel: 2,
            count: 2);

        Assert.Equal(4, preview.Count);
        Assert.All(preview, row => Assert.Equal(ChannelLabelTransferStatus.Ready, row.Status));
        Assert.Equal([2, 3, 2, 3], preview.Select(row => row.TargetDanteId));
    }

    [Fact]
    public void ImportedTxSwapUpdatesSubscriptionsWithoutCascade()
    {
        using TemporaryDirectory temp = new();
        string xml = System.IO.Path.Combine(temp.Path, "swap.xml");
        SyntheticPresetFactory.Create(xml, 2, txPerDevice: 2, rxPerDevice: 2);
        DanteProject project = DanteProject.Load(xml);

        int changed = project.ApplyChannelLabels([
            new ChannelLabelAssignment("DEVICE-001", DanteChannelKind.Tx, 1, "TX-02"),
            new ChannelLabelAssignment("DEVICE-001", DanteChannelKind.Tx, 2, "TX-01")
        ]);

        Assert.Equal(2, changed);
        DanteSubscription[] subscriptions = project.PatchMatrix.Subscriptions
            .Where(item => item.RxDevice == "DEVICE-002")
            .OrderBy(item => item.RxDanteId)
            .ToArray();
        Assert.Equal("TX-02", subscriptions[0].TxChannelName);
        Assert.Equal("TX-01", subscriptions[1].TxChannelName);
        Assert.False(project.ValidateXmlChangeGuard().HasErrors);
    }

    [Theory]
    [InlineData("TrèsLongLabel", "TresLong")]
    [InlineData("Chœur", "Choeur")]
    [InlineData("Bass🎵", "Bass_")]
    public void DmtCompatibilityProducesAsciiNamesOfAtMostEightCharacters(string source, string expected)
    {
        DmtLabelCompatibility result = DmtChannelWorkbookService.CheckCompatibility(source);

        Assert.False(result.IsCompatible);
        Assert.Equal(expected, result.AdaptedLabel);
        Assert.True(Encoding.ASCII.GetByteCount(result.AdaptedLabel) <= 8);
    }

    [Fact]
    public void UnknownJsonSchemaIsRejected()
    {
        string json = """
            {"format":"dante-config-editor-channel-labels","schemaVersion":99,"sourceApplication":"test","sets":[]}
            """;

        InvalidDataException error = Assert.Throws<InvalidDataException>(() => ChannelLabelExchangeService.ParseJson(json));
        Assert.Contains("99", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("channel-list-dlive.json")]
    [InlineData("channel-list-dlive.csv")]
    public void DmtV214Rc1ExportsAreImportedThroughStableExchangeAdapters(string fileName)
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "DmtV214Rc1", fileName);

        ChannelLabelReadResult result = ChannelLabelExchangeService.ReadWithReport(path);

        Assert.Equal("2.14.0-RC1", result.Document.SourceVersion);
        Assert.Equal(3, result.Report.ChannelCount);
        Assert.Equal(0, result.Report.IgnoredLineCount);
        Assert.Contains(result.Report.Warnings, warning => warning.Contains("2.14.0-RC1", StringComparison.Ordinal));
        ChannelLabelSet set = Assert.Single(result.Document.Sets);
        Assert.Equal("dLive", set.DeviceName);
        Assert.Equal(ChannelLabelDirection.Tx, set.Direction);
        Assert.Equal([1, 2, 64], set.Channels.Select(channel => channel.ChannelNumber));
        Assert.Equal(["Kick In", "Snare Top", "Talkback"], set.Channels.Select(channel => channel.Label));
    }

    [Fact]
    public void ImportAdaptersDeclareEachSupportedExtensionOnlyOnce()
    {
        IChannelLabelImportAdapter[] adapters = ChannelLabelImportAdapterRegistry.RegisteredAdapters.ToArray();
        string[] extensions = adapters.SelectMany(adapter => adapter.Extensions).ToArray();

        Assert.Equal(5, adapters.Length);
        Assert.Equal(extensions.Length, extensions.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.Equal([".csv", ".json", ".ods", ".xlsx", ".zip"], extensions.OrderBy(value => value));
    }

    [Fact]
    public void JsonWithUnknownPropertyIsRejectedInsteadOfBeingSilentlyMisread()
    {
        string json = """
            {
              "format": "dante-config-editor-channel-labels",
              "schemaVersion": 1,
              "sourceApplication": "test",
              "sourceVersion": "1",
              "unexpected": true,
              "sets": [
                {
                  "deviceName": "DEVICE",
                  "direction": "tx",
                  "channels": [{"channelNumber":1,"label":"Kick","danteId":1}]
                }
              ]
            }
            """;

        InvalidDataException error = Assert.Throws<InvalidDataException>(() => ChannelLabelExchangeService.ParseJson(json));

        Assert.Contains("JSON", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DuplicateChannelNumberIsRejectedInsteadOfSelectingOneSilently()
    {
        ChannelLabelDocument document = new(
            ChannelLabelExchangeService.FormatName,
            ChannelLabelExchangeService.CurrentSchemaVersion,
            "test",
            "1",
            [
                new ChannelLabelSet("DEVICE", ChannelLabelDirection.Tx,
                [
                    new ChannelLabelEntry(1, "Kick", 1),
                    new ChannelLabelEntry(1, "Snare", 1)
                ])
            ]);

        InvalidDataException error = Assert.Throws<InvalidDataException>(() =>
            ChannelLabelExchangeService.SerializeJson(document));

        Assert.Contains("plusieurs fois", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CsvWithMixedSchemaVersionsIsRejected()
    {
        string csv = """
            format_version,source_app,source_version,device,direction,channel,dante_id,label
            1,test,1,DEVICE,tx,1,1,Kick
            2,test,1,DEVICE,tx,2,2,Snare
            """;

        InvalidDataException error = Assert.Throws<InvalidDataException>(() =>
            ChannelLabelExchangeService.ParseCsv(csv));

        Assert.Contains("plusieurs versions", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void XlsxAdapterReportsMissingRequiredColumns()
    {
        using TemporaryDirectory temp = new();
        string path = CreateDmtWorkbook(temp.Path, [("yes", 1, "Kick")], nameHeader: "Unexpected");

        InvalidDataException error = Assert.Throws<InvalidDataException>(() =>
            ChannelLabelExchangeService.ReadWithReport(path));

        Assert.Contains("XLSX", error.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("colonnes", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void OdsAdapterReportsMissingRequiredColumns()
    {
        using TemporaryDirectory temp = new();
        string path = CreateDmtOdsWithHeaders(temp.Path, "Channel", "Unexpected");

        InvalidDataException error = Assert.Throws<InvalidDataException>(() =>
            ChannelLabelExchangeService.ReadWithReport(path));

        Assert.Contains("ODS", error.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("colonnes", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static string CreateDmtWorkbook(
        string directory,
        IReadOnlyList<(string Enabled, int Channel, string Name)> rows,
        string nameHeader = "Name")
    {
        string path = System.IO.Path.Combine(directory, "dLiveChannelList.xlsx");
        using FileStream stream = File.Create(path);
        using ZipArchive archive = new(stream, ZipArchiveMode.Create);
        WriteEntry(archive, "[Content_Types].xml", """
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
              <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
              <Default Extension="xml" ContentType="application/xml"/>
              <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
              <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
              <Override PartName="/xl/worksheets/sheet2.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
            </Types>
            """);
        WriteEntry(archive, "_rels/.rels", """
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
              <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
            </Relationships>
            """);
        WriteEntry(archive, "xl/workbook.xml", """
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
              <sheets><sheet name="Channels" sheetId="1" r:id="rId1"/><sheet name="Misc" sheetId="2" r:id="rId2"/></sheets>
            </workbook>
            """);
        WriteEntry(archive, "xl/_rels/workbook.xml.rels", """
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
              <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
              <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet2.xml"/>
            </Relationships>
            """);
        StringBuilder sheet = new("""
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"><sheetData>
              <row r="1"><c r="A1" t="inlineStr"><is><t>Enabled</t></is></c><c r="B1" t="inlineStr"><is><t>Channel</t></is></c><c r="C1" t="inlineStr"><is><t>Name</t></is></c><c r="D1" t="inlineStr"><is><t>Color</t></is></c></row>
            """);
        sheet.Replace(">Name<", $">{nameHeader}<");
        for (int index = 0; index < rows.Count; index++)
        {
            int row = index + 2;
            (string enabled, int channel, string name) = rows[index];
            sheet.Append($"<row r=\"{row}\"><c r=\"A{row}\" t=\"inlineStr\"><is><t>{enabled}</t></is></c><c r=\"B{row}\"><v>{channel}</v></c><c r=\"C{row}\" s=\"4\" t=\"inlineStr\"><is><t>{name}</t></is></c><c r=\"D{row}\" t=\"inlineStr\"><is><t>blue</t></is></c></row>");
        }
        sheet.Append("</sheetData></worksheet>");
        WriteEntry(archive, "xl/worksheets/sheet1.xml", sheet.ToString());
        WriteEntry(archive, "xl/worksheets/sheet2.xml", """
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"><sheetData>
              <row r="1"><c r="A1" t="inlineStr"><is><t>Property</t></is></c><c r="B1" t="inlineStr"><is><t>Value</t></is></c></row>
              <row r="2"><c r="A2" t="inlineStr"><is><t>Version</t></is></c><c r="B2" t="inlineStr"><is><t>14</t></is></c></row>
            </sheetData></worksheet>
            """);
        return path;
    }

    private static string CreateDmtOdsWithHeaders(string directory, string channelHeader, string nameHeader)
    {
        string path = System.IO.Path.Combine(directory, "dLiveChannelList.ods");
        using FileStream stream = File.Create(path);
        using ZipArchive archive = new(stream, ZipArchiveMode.Create);
        string content = """
            <?xml version="1.0" encoding="UTF-8"?>
            <office:document-content
              xmlns:office="urn:oasis:names:tc:opendocument:xmlns:office:1.0"
              xmlns:table="urn:oasis:names:tc:opendocument:xmlns:table:1.0"
              xmlns:text="urn:oasis:names:tc:opendocument:xmlns:text:1.0">
              <office:body>
                <office:spreadsheet>
                  <table:table table:name="Channels">
                    <table:table-row>
                      <table:table-cell office:value-type="string"><text:p>Enabled</text:p></table:table-cell>
                      <table:table-cell office:value-type="string"><text:p>CHANNEL_HEADER</text:p></table:table-cell>
                      <table:table-cell office:value-type="string"><text:p>NAME_HEADER</text:p></table:table-cell>
                    </table:table-row>
                    <table:table-row>
                      <table:table-cell office:value-type="string"><text:p>yes</text:p></table:table-cell>
                      <table:table-cell office:value-type="string"><text:p>1</text:p></table:table-cell>
                      <table:table-cell office:value-type="string"><text:p>Kick</text:p></table:table-cell>
                    </table:table-row>
                  </table:table>
                </office:spreadsheet>
              </office:body>
            </office:document-content>
            """
            .Replace("CHANNEL_HEADER", channelHeader, StringComparison.Ordinal)
            .Replace("NAME_HEADER", nameHeader, StringComparison.Ordinal);
        WriteEntry(archive, "content.xml", content);
        return path;
    }

    private static void WriteEntry(ZipArchive archive, string name, string content)
    {
        ZipArchiveEntry entry = archive.CreateEntry(name);
        using StreamWriter writer = new(entry.Open(), new UTF8Encoding(false));
        writer.Write(content);
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "DanteConfigEditorV3.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, true);
            }
        }
    }
}
