using System.Xml.Linq;

namespace DanteConfigEditor.Services;

internal static class MachineRoleIdentityService
{
    public static string ReadVisibleName(XElement device, string fallback = "")
    {
        string[] candidates =
        [
            device.ChildValue("name"),
            device.ChildValue("friendly_name"),
            device.ChildValue("default_name")
        ];

        return candidates.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))
            ?? fallback;
    }

    public static string ReadTechnicalDeviceId(XElement device)
    {
        return device.Child("instance_id").ChildValue("device_id");
    }

    public static string ReadProcessId(XElement device)
    {
        return device.Child("instance_id").ChildValue("process_id");
    }

    public static string GetOrCreateSessionIdentity(XElement device)
    {
        DeviceRoleIdentityAnnotation? annotation = device.Annotation<DeviceRoleIdentityAnnotation>();
        if (annotation is not null)
        {
            return annotation.Value;
        }

        string value = "role:" + Guid.NewGuid().ToString("N");
        device.AddAnnotation(new DeviceRoleIdentityAnnotation(value));
        return value;
    }

    public static string? TryGetSessionIdentity(XElement device)
    {
        return device.Annotation<DeviceRoleIdentityAnnotation>()?.Value;
    }

    public static void SetSessionIdentity(XElement device, string identity)
    {
        device.RemoveAnnotations<DeviceRoleIdentityAnnotation>();
        device.AddAnnotation(new DeviceRoleIdentityAnnotation(identity));
    }

    public static void PairEquivalentDocuments(XDocument source, XDocument target)
    {
        XElement[] sourceDevices = source.Root.Children("device").ToArray();
        XElement[] targetDevices = target.Root.Children("device").ToArray();
        if (CanPairByPosition(sourceDevices, targetDevices))
        {
            for (int index = 0; index < sourceDevices.Length; index++)
            {
                SetSessionIdentity(
                    targetDevices[index],
                    GetOrCreateSessionIdentity(sourceDevices[index]));
            }

            return;
        }

        bool[] targetMatched = new bool[targetDevices.Length];
        Dictionary<string, Queue<int>> targetsByTechnicalIdentity = BuildTargetIndex(
            targetDevices,
            BuildTechnicalIdentity);
        Dictionary<string, Queue<int>> targetsByName = BuildTargetIndex(
            targetDevices,
            device => ReadVisibleName(device));

        for (int sourceIndex = 0; sourceIndex < sourceDevices.Length; sourceIndex++)
        {
            XElement sourceDevice = sourceDevices[sourceIndex];
            string identity = GetOrCreateSessionIdentity(sourceDevice);
            int targetIndex = FindEquivalentDeviceIndex(
                sourceDevice,
                targetDevices,
                targetMatched,
                targetsByTechnicalIdentity,
                targetsByName,
                sourceIndex);
            if (targetIndex < 0)
            {
                continue;
            }

            targetMatched[targetIndex] = true;
            SetSessionIdentity(targetDevices[targetIndex], identity);
        }

        foreach (XElement targetDevice in targetDevices.Where((_, index) => !targetMatched[index]))
        {
            GetOrCreateSessionIdentity(targetDevice);
        }
    }

    private static bool CanPairByPosition(
        IReadOnlyList<XElement> sourceDevices,
        IReadOnlyList<XElement> targetDevices)
    {
        if (sourceDevices.Count != targetDevices.Count)
        {
            return false;
        }

        for (int index = 0; index < sourceDevices.Count; index++)
        {
            string sourceTechnicalIdentity = BuildTechnicalIdentity(sourceDevices[index]);
            string targetTechnicalIdentity = BuildTechnicalIdentity(targetDevices[index]);
            if (!string.IsNullOrWhiteSpace(sourceTechnicalIdentity)
                || !string.IsNullOrWhiteSpace(targetTechnicalIdentity))
            {
                if (!string.Equals(
                        sourceTechnicalIdentity,
                        targetTechnicalIdentity,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                continue;
            }

            if (!string.Equals(
                    ReadVisibleName(sourceDevices[index]),
                    ReadVisibleName(targetDevices[index]),
                    StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    private static int FindEquivalentDeviceIndex(
        XElement source,
        IReadOnlyList<XElement> targets,
        IReadOnlyList<bool> matched,
        IReadOnlyDictionary<string, Queue<int>> targetsByTechnicalIdentity,
        IReadOnlyDictionary<string, Queue<int>> targetsByName,
        int sourceIndex)
    {
        string sourceTechnicalIdentity = BuildTechnicalIdentity(source);
        int matchedIndex = TryTakeIndex(
            sourceTechnicalIdentity,
            targetsByTechnicalIdentity,
            matched);
        if (matchedIndex >= 0)
        {
            return matchedIndex;
        }

        string sourceName = ReadVisibleName(source);
        matchedIndex = TryTakeIndex(sourceName, targetsByName, matched);
        if (matchedIndex >= 0)
        {
            return matchedIndex;
        }

        return sourceIndex < targets.Count && !matched[sourceIndex]
            ? sourceIndex
            : -1;
    }

    private static Dictionary<string, Queue<int>> BuildTargetIndex(
        IReadOnlyList<XElement> devices,
        Func<XElement, string> identitySelector)
    {
        Dictionary<string, Queue<int>> index = new(StringComparer.OrdinalIgnoreCase);
        for (int position = 0; position < devices.Count; position++)
        {
            string identity = identitySelector(devices[position]);
            if (string.IsNullOrWhiteSpace(identity))
            {
                continue;
            }

            if (!index.TryGetValue(identity, out Queue<int>? positions))
            {
                positions = new Queue<int>();
                index[identity] = positions;
            }

            positions.Enqueue(position);
        }

        return index;
    }

    private static int TryTakeIndex(
        string identity,
        IReadOnlyDictionary<string, Queue<int>> index,
        IReadOnlyList<bool> matched)
    {
        if (string.IsNullOrWhiteSpace(identity)
            || !index.TryGetValue(identity, out Queue<int>? positions))
        {
            return -1;
        }

        while (positions.TryDequeue(out int position))
        {
            if (!matched[position])
            {
                return position;
            }
        }

        return -1;
    }

    private static string BuildTechnicalIdentity(XElement device)
    {
        string deviceId = ReadTechnicalDeviceId(device);
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            return string.Empty;
        }

        return $"{deviceId.Trim()}|{ReadProcessId(device).Trim()}";
    }

    private sealed record DeviceRoleIdentityAnnotation(string Value);
}
