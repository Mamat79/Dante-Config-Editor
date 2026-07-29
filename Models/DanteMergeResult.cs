namespace DanteConfigEditor.Models;

public sealed record DanteMergeResult(
    int ImportedDeviceCount,
    int RenamedDeviceCount,
    int SkippedDuplicateDeviceCount,
    IReadOnlyList<string> SkippedDuplicateDeviceNames,
    IReadOnlyDictionary<string, string> RenamedDevices,
    int ReusedDeviceCount,
    IReadOnlyDictionary<string, string> ReusedDevices);
