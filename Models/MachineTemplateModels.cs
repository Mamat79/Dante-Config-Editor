using System.IO;
using System.Xml.Linq;

namespace DanteConfigEditor.Models;

public sealed record MachineTemplateMetadata
{
    public const int CurrentFormatVersion = 1;

    public int FormatVersion { get; init; } = CurrentFormatVersion;

    public Guid TemplateId { get; init; } = Guid.NewGuid();

    public string TemplateName { get; init; } = string.Empty;

    public string Manufacturer { get; init; } = string.Empty;

    public string Model { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string Category { get; init; } = string.Empty;

    public IReadOnlyList<string> Tags { get; init; } = [];

    public int TxCount { get; init; }

    public int RxCount { get; init; }

    public string SourcePresetVersion { get; init; } = string.Empty;

    public string SourceXmlNamespace { get; init; } = string.Empty;

    public string CreatedByDceVersion { get; init; } = string.Empty;

    public DateTimeOffset CreatedUtc { get; init; }

    public DateTimeOffset ModifiedUtc { get; init; }

    public string TemplateSha256 { get; init; } = string.Empty;

    public string? ImageFileName { get; init; }
}

public sealed class MachineTemplateCreateRequest
{
    public required string TemplateName { get; init; }

    public string? Manufacturer { get; init; }

    public string? Model { get; init; }

    public string? Description { get; init; }

    public string? Category { get; init; }

    public IReadOnlyList<string> Tags { get; init; } = [];

    public IReadOnlyList<string>? TxLabels { get; init; }

    public IReadOnlyList<string>? RxLabels { get; init; }

    public string? ImageSourcePath { get; init; }
}

public sealed class MachineTemplateEditRequest
{
    public required string TemplateName { get; init; }

    public string? Manufacturer { get; init; }

    public string? Model { get; init; }

    public string? Description { get; init; }

    public string? Category { get; init; }

    public IReadOnlyList<string> Tags { get; init; } = [];

    public IReadOnlyList<string>? TxLabels { get; init; }

    public IReadOnlyList<string>? RxLabels { get; init; }

    public string? ImageSourcePath { get; init; }

    public bool RemoveImage { get; init; }
}

public sealed class MachineTemplatePackage
{
    internal MachineTemplatePackage(
        MachineTemplateMetadata metadata,
        XDocument templateDocument,
        string? imageSourcePath = null,
        string? storedImagePath = null)
    {
        Metadata = metadata;
        TemplateDocument = new XDocument(templateDocument);
        ImageSourcePath = imageSourcePath;
        StoredImagePath = storedImagePath;
    }

    public MachineTemplateMetadata Metadata { get; }

    public XDocument TemplateDocument { get; }

    public string? ImagePath => StoredImagePath ?? ImageSourcePath;

    internal string? ImageSourcePath { get; }

    internal string? StoredImagePath { get; }
}

public sealed class MachineBankCorruptionException : IOException
{
    public MachineBankCorruptionException(string message)
        : base(message)
    {
    }

    public MachineBankCorruptionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
