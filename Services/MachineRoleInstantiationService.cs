using System.Xml.Linq;
using DanteConfigEditor.Models;

namespace DanteConfigEditor.Services;

internal sealed record MachineRoleCreation(
    XElement DeviceElement,
    int TxCount,
    int RxCount,
    int CopiedSubscriptionCount);

internal static class MachineRoleInstantiationService
{
    private static readonly string[] SubscriptionDeviceElementNames =
    [
        "subscribed_device",
        "subscription_device",
        "tx_device",
        "source_device"
    ];

    private static readonly string[] SubscriptionChannelElementNames =
    [
        "subscribed_channel",
        "subscribed_channel_name",
        "subscribed_channel_label",
        "subscribed_tx_channel",
        "subscribed_tx_channel_name",
        "subscribed_label",
        "source_channel",
        "source_channel_name"
    ];

    public static MachineRoleCreation CreateClone(XElement sourceDevice, MachineCloneOptions options)
    {
        ArgumentNullException.ThrowIfNull(sourceDevice);
        ArgumentNullException.ThrowIfNull(options);

        string newName = DanteNameRules.EnsureValidDeviceName(options.NewName);
        string sourceName = MachineRoleIdentityService.ReadVisibleName(sourceDevice);
        XElement clone = new(sourceDevice);
        NeutralizeHardwareIdentity(clone);
        SetGenericRoleName(clone, newName);

        if (!options.PreserveNetworkConfiguration)
        {
            RemoveChildren(clone, "interface");
        }

        if (!options.PreserveSubscriptions)
        {
            RemoveSubscriptions(clone);
        }
        else
        {
            RewriteExplicitLocalSubscriptionReferences(clone, sourceName, newName);
        }

        if (!options.PreserveMulticastFlows)
        {
            RemoveChildren(clone, "txflow");
        }

        if (!options.PreserveDeviceSettings)
        {
            SetElementValue(clone, "samplerate", "48000");
            SetElementValue(clone, "encoding", "24");
            SetElementValue(clone, "unicast_latency", "1000");
            clone.Child("redundancy")?.Remove();
        }

        if (!options.PreservePreferredMaster)
        {
            XElement? preferredMaster = clone.Child("preferred_master");
            if (preferredMaster is not null)
            {
                preferredMaster.SetAttributeValue("value", "false");
            }
        }

        if (!options.PreserveTxLabels)
        {
            ResetChannelNames(clone.Children("txchannel"), DanteChannelKind.Tx, "TX");
        }

        if (!options.PreserveRxLabels)
        {
            ResetChannelNames(clone.Children("rxchannel"), DanteChannelKind.Rx, "RX");
        }

        MachineRoleIdentityService.GetOrCreateSessionIdentity(clone);
        return new MachineRoleCreation(
            clone,
            clone.Children("txchannel").Count(),
            clone.Children("rxchannel").Count(),
            CountActiveSubscriptions(clone));
    }

    public static XElement CreateSanitizedTemplateDevice(
        XElement sourceDevice,
        IReadOnlyList<string>? txLabels = null,
        IReadOnlyList<string>? rxLabels = null)
    {
        ArgumentNullException.ThrowIfNull(sourceDevice);

        XElement template = new(sourceDevice);
        NeutralizeHardwareIdentity(template);
        SetGenericRoleName(template, "MACHINE-TEMPLATE");
        RemoveChildren(template, "interface");
        RemoveChildren(template, "txflow");
        RemoveSubscriptions(template);
        XElement? preferredMaster = template.Child("preferred_master");
        if (preferredMaster is not null)
        {
            preferredMaster.SetAttributeValue("value", "false");
        }

        ApplyExplicitLabels(template.Children("txchannel").ToArray(), DanteChannelKind.Tx, txLabels);
        ApplyExplicitLabels(template.Children("rxchannel").ToArray(), DanteChannelKind.Rx, rxLabels);
        return template;
    }

    public static MachineRoleCreation CreateFromTemplate(
        XElement templateDevice,
        MachineInstanceOptions options)
    {
        ArgumentNullException.ThrowIfNull(templateDevice);
        ArgumentNullException.ThrowIfNull(options);

        string newName = DanteNameRules.EnsureValidDeviceName(options.NewName);
        XElement instance = new(templateDevice);
        NeutralizeHardwareIdentity(instance);
        SetGenericRoleName(instance, newName);
        RemoveChildren(instance, "interface");
        RemoveSubscriptions(instance);

        XElement[] txChannels = instance.Children("txchannel").ToArray();
        XElement[] rxChannels = instance.Children("rxchannel").ToArray();
        if (!options.UseTemplateTxLabels || !string.IsNullOrWhiteSpace(options.TxLabelPrefix))
        {
            ResetChannelNames(txChannels, DanteChannelKind.Tx, CleanPrefix(options.TxLabelPrefix, "TX"));
        }

        if (!options.UseTemplateRxLabels || !string.IsNullOrWhiteSpace(options.RxLabelPrefix))
        {
            ResetChannelNames(rxChannels, DanteChannelKind.Rx, CleanPrefix(options.RxLabelPrefix, "RX"));
        }

        MachineRoleIdentityService.GetOrCreateSessionIdentity(instance);
        return new MachineRoleCreation(instance, txChannels.Length, rxChannels.Length, 0);
    }

    private static void NeutralizeHardwareIdentity(XElement device)
    {
        // Un rôle de preset hors ligne n'est pas une identité matérielle. Le
        // Preset Creator officiel omet lui aussi instance_id pour un device
        // personnalisé, au lieu d'inventer un EUI-64 susceptible de collision.
        device.Child("instance_id")?.Remove();
        device.Child("default_name")?.Remove();
    }

    private static void SetGenericRoleName(XElement device, string name)
    {
        device.Child("name")?.Remove();
        SetElementValue(device, "friendly_name", name);
    }

    private static void RemoveSubscriptions(XElement device)
    {
        foreach (XElement rxChannel in device.Children("rxchannel"))
        {
            foreach (string elementName in SubscriptionDeviceElementNames.Concat(SubscriptionChannelElementNames))
            {
                rxChannel.Child(elementName)?.Remove();
            }
        }
    }

    private static int CountActiveSubscriptions(XElement device)
    {
        return device.Children("rxchannel").Count(rxChannel =>
            SubscriptionDeviceElementNames.Any(name => !string.IsNullOrWhiteSpace(rxChannel.ChildValue(name)))
            && SubscriptionChannelElementNames.Any(name => !string.IsNullOrWhiteSpace(rxChannel.ChildValue(name))));
    }

    private static void RewriteExplicitLocalSubscriptionReferences(
        XElement device,
        string sourceName,
        string newName)
    {
        if (string.IsNullOrWhiteSpace(sourceName))
        {
            return;
        }

        foreach (XElement rxChannel in device.Children("rxchannel"))
        {
            foreach (string elementName in SubscriptionDeviceElementNames)
            {
                XElement? subscribedDevice = rxChannel.Child(elementName);
                if (subscribedDevice is not null
                    && string.Equals(
                        subscribedDevice.Value.Trim(),
                        sourceName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    subscribedDevice.Value = newName;
                }
            }
        }
    }

    private static void RemoveChildren(XElement parent, string localName)
    {
        foreach (XElement child in parent.Children(localName).ToArray())
        {
            child.Remove();
        }
    }

    private static void ResetChannelNames(
        IEnumerable<XElement> channelElements,
        DanteChannelKind kind,
        string prefix)
    {
        int position = 1;
        foreach (XElement channel in channelElements)
        {
            SetChannelName(
                channel,
                kind,
                DanteNameRules.EnsureValidChannelName($"{prefix} {position}"));
            position++;
        }
    }

    private static void ApplyExplicitLabels(
        IReadOnlyList<XElement> channels,
        DanteChannelKind kind,
        IReadOnlyList<string>? labels)
    {
        if (labels is null)
        {
            return;
        }

        if (labels.Count != channels.Count)
        {
            throw new InvalidOperationException(
                $"Le nombre de labels {kind} ({labels.Count}) ne correspond pas au nombre de canaux ({channels.Count}).");
        }

        for (int index = 0; index < channels.Count; index++)
        {
            SetChannelName(channels[index], kind, DanteNameRules.EnsureValidChannelName(labels[index]));
        }
    }

    private static void SetChannelName(XElement channel, DanteChannelKind kind, string value)
    {
        string[] preferredNames = kind == DanteChannelKind.Tx
            ? ["label", "name", "channel_name"]
            : ["name", "label", "channel_name"];

        foreach (string candidate in preferredNames)
        {
            XElement? element = channel.Child(candidate);
            if (element is not null)
            {
                element.Value = value;
                return;
            }

            XAttribute? attribute = channel.Attribute(candidate);
            if (attribute is not null)
            {
                attribute.Value = value;
                return;
            }
        }

        SetElementValue(channel, preferredNames[0], value);
    }

    private static void SetElementValue(XElement parent, string localName, string value)
    {
        XElement? element = parent.Child(localName);
        if (element is null)
        {
            parent.Add(new XElement(parent.ChildName(localName), value));
        }
        else
        {
            element.Value = value;
        }
    }

    private static string CleanPrefix(string? value, string fallback)
    {
        string clean = value?.Trim() ?? string.Empty;
        return string.IsNullOrWhiteSpace(clean) ? fallback : clean;
    }
}
