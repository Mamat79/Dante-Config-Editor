using System.Xml.Linq;
using DanteConfigEditor.Domain.Projects;
using DanteConfigEditor.Models;

namespace DanteConfigEditor.DanteXml.Profiles;

public sealed class DanteXmlProfileDetector : IDanteXmlProfileDetector
{
    public DanteXmlProfileDescriptor Detect(DanteProject project)
    {
        ArgumentNullException.ThrowIfNull(project);

        XDocument document = project.Document;
        XElement? root = document.Root;
        if (root is null || !string.Equals(root.Name.LocalName, "preset", StringComparison.Ordinal))
        {
            return UnknownProfile(
                project.PresetVersion,
                root?.Name.NamespaceName ?? string.Empty,
                "XmlProfile.RootNotRecognized");
        }

        XElement[] devices = root
            .Elements()
            .Where(element => element.Name.LocalName == "device")
            .ToArray();
        if (devices.Length == 0)
        {
            return UnknownProfile(
                project.PresetVersion,
                root.Name.NamespaceName,
                "XmlProfile.NoDevice");
        }

        List<string> reasons = [];
        bool everyDeviceNamed = devices.All(HasVisibleName);
        bool everyChannelIdentified = devices
            .SelectMany(DeviceChannels)
            .All(ChannelHasStableTechnicalShape);
        bool complete = everyDeviceNamed && everyChannelIdentified;

        if (!everyDeviceNamed)
        {
            reasons.Add("XmlProfile.SomeDevicesHaveNoVisibleName");
        }

        if (!everyChannelIdentified)
        {
            reasons.Add("XmlProfile.SomeChannelsHaveNoDanteIdOrMediaType");
        }

        bool hasTx = project.Devices.Any(device => device.TxChannels.Count > 0);
        bool hasRx = project.Devices.Any(device => device.RxChannels.Count > 0);
        bool hasNetwork = project.Devices.Any(device => project.SupportsIpConfiguration(device.Name));
        bool hasAudio = devices.Any(device =>
            device.Elements().Any(element =>
                element.Name.LocalName is "samplerate" or "encoding" or "unicast_latency"));
        bool supportsGenericRoles = string.Equals(
            project.PresetVersion,
            "3.0.0",
            StringComparison.OrdinalIgnoreCase);

        DanteXmlCapabilities capabilities = new(
            CanReadMachines: true,
            CanEditDeviceNames: everyDeviceNamed,
            CanEditTxLabels: hasTx,
            CanEditRxLabels: hasRx,
            CanEditPatch: hasRx,
            CanEditNetwork: hasNetwork,
            CanEditAudioFormat: hasAudio,
            CanCreateDevices: supportsGenericRoles,
            CanCreateProject: supportsGenericRoles,
            CanSave: true);

        return new DanteXmlProfileDescriptor(
            complete ? "recognized-complete" : "recognized-partial",
            complete ? DanteXmlRecognitionLevel.Complete : DanteXmlRecognitionLevel.Partial,
            complete ? ProjectAccessMode.Full : ProjectAccessMode.Restricted,
            project.PresetVersion,
            root.Name.NamespaceName,
            capabilities,
            reasons);
    }

    private static DanteXmlProfileDescriptor UnknownProfile(
        string presetVersion,
        string namespaceName,
        string reason)
    {
        return new DanteXmlProfileDescriptor(
            "unknown-read-only",
            DanteXmlRecognitionLevel.Unknown,
            ProjectAccessMode.ReadOnly,
            presetVersion,
            namespaceName,
            DanteXmlCapabilities.ReadOnly,
            [reason]);
    }

    private static bool HasVisibleName(XElement device) =>
        device.Elements().Any(element =>
            element.Name.LocalName is "friendly_name" or "name"
            && !string.IsNullOrWhiteSpace(element.Value));

    private static IEnumerable<XElement> DeviceChannels(XElement device) =>
        device.Elements().Where(element =>
            element.Name.LocalName is "txchannel" or "rxchannel");

    private static bool ChannelHasStableTechnicalShape(XElement channel) =>
        channel.Attribute("danteId") is not null
        && channel.Attribute("mediaType") is not null;
}
