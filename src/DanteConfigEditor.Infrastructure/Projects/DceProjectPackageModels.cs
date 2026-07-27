using System.Text.Json;
using System.Text.Json.Serialization;
using DanteConfigEditor.DanteXml;
using DanteConfigEditor.Domain.History;
using DanteConfigEditor.Domain.Validation;
using DanteConfigEditor.Domain.Workspace;
using DanteConfigEditor.Models;

namespace DanteConfigEditor.Infrastructure.Projects;

public sealed class DceProjectManifest
{
    public const string CurrentSchemaVersion = "1.0";

    public string SchemaVersion { get; set; } = CurrentSchemaVersion;

    public string CreatedWithVersion { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset ModifiedAt { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string DanteXmlEntry { get; set; } = DceProjectPackageEntries.DanteXml;

    public Dictionary<string, string> ContentSha256 { get; set; } =
        new(StringComparer.Ordinal);

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}

public static class DceProjectPackageEntries
{
    public const string Manifest = "manifest.json";
    public const string DanteXml = "dante/project.xml";
    public const string Metadata = "workspace/metadata.json";
    public const string Layout = "workspace/layout.json";
    public const string Annotations = "workspace/annotations.json";
    public const string History = "workspace/history.json";
    public const string Settings = "workspace/settings.json";
    public const string Validation = "reports/last-validation.json";

    public static IReadOnlySet<string> Required { get; } = new HashSet<string>(
        [
            Manifest,
            DanteXml,
            Metadata,
            Layout,
            Annotations,
            History,
            Settings
        ],
        StringComparer.Ordinal);
}

public sealed record DceProjectPackageLimits(
    int MaximumEntryCount = 200,
    long MaximumTotalUncompressedBytes = 100 * 1024 * 1024,
    long MaximumXmlBytes = 25 * 1024 * 1024,
    long MaximumJsonBytes = 5 * 1024 * 1024,
    long MaximumAssetBytes = 10 * 1024 * 1024);

public sealed record DceProjectWriteRequest(
    DanteProject Project,
    ProjectWorkspaceData Workspace,
    IReadOnlyList<ProjectHistoryEntry> History,
    ProjectValidationState Validation,
    string ApplicationVersion,
    DceProjectManifest? ExistingManifest = null,
    IReadOnlyDictionary<string, byte[]>? Assets = null);

public sealed record DceProjectPackage(
    string PackagePath,
    DceProjectManifest Manifest,
    DanteXmlOpenResult OpenedXml,
    ProjectWorkspaceData Workspace,
    IReadOnlyList<ProjectHistoryEntry> History,
    ProjectValidationState Validation,
    IReadOnlyDictionary<string, byte[]> Assets);

public sealed record DceProjectSaveResult(
    string DestinationPath,
    string BackupPath,
    string PackageSha256);

public enum DceProjectSaveStage
{
    AfterTemporaryPackageCreated,
    BeforeDestinationCommit
}

public sealed record DceAnnotationsPayload(
    IReadOnlyList<string> Annotations,
    IReadOnlyList<string> HiddenDeviceIds);

public sealed record DceWorkspaceMetadataPayload(
    string Name,
    string Description,
    DateTimeOffset CreatedAt,
    DateTimeOffset ModifiedAt,
    string CreatedWithVersion,
    IReadOnlyDictionary<string, string>? MetadataExtensions,
    IReadOnlyDictionary<string, string>? WorkspaceExtensions)
{
    public static DceWorkspaceMetadataPayload FromWorkspace(ProjectWorkspaceData workspace) =>
        new(
            workspace.Metadata.Name,
            workspace.Metadata.Description,
            workspace.Metadata.CreatedAt,
            workspace.Metadata.ModifiedAt,
            workspace.Metadata.CreatedWithVersion,
            workspace.Metadata.Extensions,
            workspace.Extensions);

    public ProjectMetadata ToMetadata() =>
        new(
            Name,
            Description,
            CreatedAt,
            ModifiedAt,
            CreatedWithVersion,
            MetadataExtensions);
}
