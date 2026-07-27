namespace DanteConfigEditor.Models;

public enum ChannelLabelDirection
{
    Tx,
    Rx,
    ConsoleInput
}

public sealed record ChannelLabelEntry(int ChannelNumber, string Label, int? DanteId = null);

public sealed record ChannelLabelSet
{
    [System.Text.Json.Serialization.JsonConstructor]
    public ChannelLabelSet(string deviceName, ChannelLabelDirection direction, IReadOnlyList<ChannelLabelEntry> channels)
    {
        DeviceName = deviceName?.Trim() ?? string.Empty;
        Direction = direction;
        Channels = channels?.ToArray() ?? throw new ArgumentNullException(nameof(channels));
    }

    public string DeviceName { get; }

    public ChannelLabelDirection Direction { get; }

    public IReadOnlyList<ChannelLabelEntry> Channels { get; }

    public string DisplayName => $"{DeviceName} - {Direction} ({Channels.Count})";
}

public sealed record ChannelLabelDocument
{
    [System.Text.Json.Serialization.JsonConstructor]
    public ChannelLabelDocument(
        string format,
        int schemaVersion,
        string sourceApplication,
        string? sourceVersion,
        IReadOnlyList<ChannelLabelSet> sets)
    {
        Format = format;
        SchemaVersion = schemaVersion;
        SourceApplication = sourceApplication;
        SourceVersion = sourceVersion;
        Sets = sets?.ToArray() ?? throw new ArgumentNullException(nameof(sets));
    }

    public string Format { get; }

    public int SchemaVersion { get; }

    public string SourceApplication { get; }

    public string? SourceVersion { get; }

    public IReadOnlyList<ChannelLabelSet> Sets { get; }
}

public sealed record ChannelLabelReadResult(
    ChannelLabelDocument Document,
    ChannelLabelImportReport Report);

public sealed record ChannelLabelImportAdapterResult(
    ChannelLabelDocument Document,
    int IgnoredLineCount = 0,
    IReadOnlyList<string>? Warnings = null);

public sealed record ChannelLabelImportReport(
    string AdapterName,
    string SourceApplication,
    string? SourceVersion,
    int SetCount,
    int DeviceCount,
    int ChannelCount,
    int IgnoredLineCount,
    int EmptyLabelCount,
    int DuplicateLabelCount,
    IReadOnlyList<string> Transformations,
    IReadOnlyList<string> Warnings)
{
    public string ToDisplayText(bool english)
    {
        string summary = english
            ? $"{SetCount} set(s), {DeviceCount} device name(s), {ChannelCount} channel(s), {IgnoredLineCount} ignored row(s), {EmptyLabelCount} empty label(s), {DuplicateLabelCount} duplicate label(s)."
            : $"{SetCount} liste(s), {DeviceCount} nom(s) de machine, {ChannelCount} canal(aux), {IgnoredLineCount} ligne(s) ignorée(s), {EmptyLabelCount} label(s) vide(s), {DuplicateLabelCount} label(s) en doublon.";
        if (Warnings.Count == 0 && Transformations.Count == 0)
        {
            return summary;
        }

        IEnumerable<string> details = Transformations.Select(item => (english ? "Transformation: " : "Transformation : ") + item)
            .Concat(Warnings.Select(item => (english ? "Warning: " + LocalizeWarning(item) : "Avertissement : " + item)));
        return summary + Environment.NewLine + string.Join(Environment.NewLine, details);
    }

    private static string LocalizeWarning(string warning) => warning switch
    {
        "Les labels vides seront ignorés lors de l'application." => "Empty labels will be ignored when changes are applied.",
        "Structure DMT 2.14.0-RC1 reconnue par le schéma d'échange DCE version 1." => "DMT 2.14.0-RC1 structure recognized by DCE exchange schema version 1.",
        _ => warning
    };
}

public sealed record ChannelLabelAssignment(
    string DeviceName,
    DanteChannelKind Kind,
    int DanteId,
    string Label);

public enum ChannelLabelTransferStatus
{
    Ready,
    Unchanged,
    MissingSource,
    MissingTarget,
    EmptyLabel
}

public sealed record ChannelLabelTransferPreviewRow(
    string SourceDevice,
    int SourceChannel,
    string TargetDevice,
    DanteChannelKind TargetKind,
    int TargetDanteId,
    string CurrentLabel,
    string NewLabel,
    ChannelLabelTransferStatus Status)
{
    public bool CanApply => Status is ChannelLabelTransferStatus.Ready or ChannelLabelTransferStatus.Unchanged;

    public bool WillChange => Status == ChannelLabelTransferStatus.Ready;

    public string StatusDisplay => Status switch
    {
        ChannelLabelTransferStatus.Ready => "Prêt",
        ChannelLabelTransferStatus.Unchanged => "Inchangé",
        ChannelLabelTransferStatus.MissingSource => "Source absente",
        ChannelLabelTransferStatus.MissingTarget => "Canal cible absent",
        ChannelLabelTransferStatus.EmptyLabel => "Label vide ignoré",
        _ => Status.ToString()
    };

    public string StatusDisplayEnglish => Status switch
    {
        ChannelLabelTransferStatus.Ready => "Ready",
        ChannelLabelTransferStatus.Unchanged => "Unchanged",
        ChannelLabelTransferStatus.MissingSource => "Missing source",
        ChannelLabelTransferStatus.MissingTarget => "Missing target channel",
        ChannelLabelTransferStatus.EmptyLabel => "Empty label ignored",
        _ => Status.ToString()
    };
}

public sealed record DmtLabelCompatibility(bool IsCompatible, string AdaptedLabel, IReadOnlyList<string> Warnings);

public sealed record DmtWorkbookReadResult(
    string TemplateVersion,
    ChannelLabelDocument Document,
    int IgnoredRowCount = 0);

public enum ChannelLabelCaseMode
{
    Preserve,
    Lowercase,
    Uppercase,
    FirstLetterUppercase
}

public sealed record ChannelLabelTransformOptions(
    bool AsciiOnly,
    ChannelLabelCaseMode CaseMode,
    int MaximumLength,
    int StartPosition,
    bool FromEnd);

public sealed record ChannelLabelCollision(string Label, IReadOnlyList<int> Channels);

public sealed record ChannelLabelTransformResult(
    ChannelLabelSet Labels,
    IReadOnlyList<ChannelLabelCollision> Collisions);
