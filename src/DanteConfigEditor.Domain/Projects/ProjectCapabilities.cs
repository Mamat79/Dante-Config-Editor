namespace DanteConfigEditor.Domain.Projects;

public enum ProjectDocumentKind
{
    DanteXml,
    DceProject
}

public enum DanteXmlRecognitionLevel
{
    Unknown,
    Partial,
    Complete
}

public enum ProjectAccessMode
{
    ReadOnly,
    Restricted,
    Full
}

public sealed record DanteXmlCapabilities(
    bool CanReadMachines,
    bool CanEditDeviceNames,
    bool CanEditTxLabels,
    bool CanEditRxLabels,
    bool CanEditPatch,
    bool CanEditNetwork,
    bool CanEditAudioFormat,
    bool CanCreateDevices,
    bool CanCreateProject,
    bool CanSave)
{
    public static DanteXmlCapabilities ReadOnly { get; } = new(
        CanReadMachines: false,
        CanEditDeviceNames: false,
        CanEditTxLabels: false,
        CanEditRxLabels: false,
        CanEditPatch: false,
        CanEditNetwork: false,
        CanEditAudioFormat: false,
        CanCreateDevices: false,
        CanCreateProject: false,
        CanSave: false);

    public bool AllowsEditing =>
        CanEditDeviceNames
        || CanEditTxLabels
        || CanEditRxLabels
        || CanEditPatch
        || CanEditNetwork
        || CanEditAudioFormat
        || CanCreateDevices;
}

public sealed record DanteXmlProfileDescriptor(
    string Id,
    DanteXmlRecognitionLevel RecognitionLevel,
    ProjectAccessMode AccessMode,
    string PresetVersion,
    string NamespaceName,
    DanteXmlCapabilities Capabilities,
    IReadOnlyList<string> Reasons)
{
    public bool IsReadOnly => AccessMode == ProjectAccessMode.ReadOnly;
}
