using System.Net;
using System.Xml.Linq;
using DanteConfigEditor.Models;

namespace DanteConfigEditor.Services;

public static class DanteProjectIntegrityValidator
{
    public static DanteValidationResult Validate(XDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        DanteValidationResult result = new();
        XElement? root = document.Root;
        if (root is null)
        {
            result.AddError(DanteIssueCategory.XmlCompatibility, "Le document XML ne contient pas de racine.");
            return result;
        }

        XElement[] devices = root.Children("device").ToArray();
        ValidateTechnicalDeviceIdentities(devices, result);
        ValidateStaticIpDuplicates(devices, result);
        foreach (XElement device in devices)
        {
            string deviceName = MachineRoleIdentityService.ReadVisibleName(device, "(machine sans nom)");
            ValidateChannels(device, "txchannel", "TX", deviceName, result);
            ValidateChannels(device, "rxchannel", "RX", deviceName, result);
            ValidateMulticastFlowReferences(device, deviceName, result);
        }

        return result;
    }

    private static void ValidateTechnicalDeviceIdentities(
        IReadOnlyList<XElement> devices,
        DanteValidationResult result)
    {
        DeviceIdentityEntry[] identities = devices
            .Select((device, position) => new DeviceIdentityEntry(
                MachineRoleIdentityService.ReadVisibleName(device, $"Device {position + 1}"),
                MachineRoleIdentityService.ReadTechnicalDeviceId(device).Trim(),
                MachineRoleIdentityService.ReadProcessId(device).Trim()))
            .Where(item => !string.IsNullOrWhiteSpace(item.DeviceId))
            .ToArray();

        foreach (IGrouping<string, DeviceIdentityEntry> group in identities.GroupBy(
                     item => $"{item.DeviceId}|{item.ProcessId}",
                     StringComparer.OrdinalIgnoreCase))
        {
            DeviceIdentityEntry[] duplicates = group.ToArray();
            if (duplicates.Length <= 1)
            {
                continue;
            }

            string names = string.Join(", ", duplicates.Select(item => (string)item.Name));
            string deviceId = duplicates[0].DeviceId;
            string processId = string.IsNullOrWhiteSpace(duplicates[0].ProcessId)
                ? "(vide)"
                : duplicates[0].ProcessId;
            result.AddError(
                DanteIssueCategory.Device,
                $"Identité technique dupliquée : device_id {deviceId}, process_id {processId}, utilisée par {names}. "
                + "Supprimez le doublon ou recréez la copie comme rôle générique sans instance_id.");
        }

        foreach (IGrouping<string, DeviceIdentityEntry> group in identities.GroupBy(
                     item => item.DeviceId,
                     StringComparer.OrdinalIgnoreCase))
        {
            string[] processIds = group
                .Select(item => item.ProcessId)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            if (group.Count() > 1 && processIds.Length > 1)
            {
                result.AddWarning(
                    DanteIssueCategory.Device,
                    $"Le device_id {group.Key} est partagé par plusieurs process_id ({string.Join(", ", processIds)}). "
                    + "Vérifiez que cette structure correspond bien au matériel d'origine.");
            }
        }

        foreach (DeviceIdentityEntry identity in identities)
        {
            if (identity.DeviceId.Length != 16
                || identity.DeviceId.Any(character => !Uri.IsHexDigit(character)))
            {
                result.AddWarning(
                    DanteIssueCategory.Device,
                    $"{identity.Name} : le device_id '{identity.DeviceId}' ne suit pas le format EUI-64 hexadécimal de 16 caractères observé. "
                    + "La valeur est conservée, mais doit être vérifiée dans Dante Controller.",
                    identity.Name);
            }
        }
    }

    private static void ValidateStaticIpDuplicates(
        IReadOnlyList<XElement> devices,
        DanteValidationResult result)
    {
        var staticDevices = devices
            .Select(device => new DanteDevice(device))
            .Where(device => device.UsesStaticIp && IPAddress.TryParse(device.StaticIpAddress, out _))
            .ToArray();

        foreach (IGrouping<string, DanteDevice> group in staticDevices.GroupBy(
                     device => device.StaticIpAddress,
                     StringComparer.OrdinalIgnoreCase))
        {
            DanteDevice[] duplicates = group.ToArray();
            if (duplicates.Length <= 1)
            {
                continue;
            }

            string names = string.Join(", ", duplicates.Select(device => device.Name));
            result.AddError(
                DanteIssueCategory.Network,
                $"Adresse IPv4 fixe dupliquée : {group.Key} est utilisée par {names}. "
                + "Attribuez une adresse unique ou repassez les machines concernées en IP automatique.");
        }
    }

    private static void ValidateChannels(
        XElement device,
        string elementName,
        string kind,
        string deviceName,
        DanteValidationResult result)
    {
        XElement[] channels = device.Children(elementName).ToArray();
        HashSet<string> ids = new(StringComparer.OrdinalIgnoreCase);
        for (int position = 0; position < channels.Length; position++)
        {
            XElement channel = channels[position];
            string danteId = channel.Attribute("danteId")?.Value.Trim() ?? string.Empty;
            string mediaType = channel.Attribute("mediaType")?.Value.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(danteId))
            {
                result.AddError(
                    DanteIssueCategory.Channel,
                    $"{deviceName} {kind} position {position + 1} : attribut danteId absent. "
                    + "Restaurez l'identifiant du canal avant la sauvegarde.",
                    deviceName);
            }
            else if (!int.TryParse(danteId, out int parsedId) || parsedId <= 0)
            {
                result.AddError(
                    DanteIssueCategory.Channel,
                    $"{deviceName} {kind} position {position + 1} : danteId '{danteId}' invalide. "
                    + "Un entier strictement positif est attendu.",
                    deviceName);
            }
            else if (!ids.Add(danteId))
            {
                result.AddError(
                    DanteIssueCategory.Channel,
                    $"{deviceName} : danteId {danteId} dupliqué parmi les canaux {kind}. "
                    + "Chaque canal d'une même direction doit avoir un identifiant unique.",
                    deviceName,
                    danteId: parsedId);
            }

            if (string.IsNullOrWhiteSpace(mediaType))
            {
                result.AddError(
                    DanteIssueCategory.Channel,
                    $"{deviceName} {kind} {Blank(danteId, position + 1)} : attribut mediaType absent. "
                    + "Restaurez la valeur du XML source avant la sauvegarde.",
                    deviceName,
                    danteId: int.TryParse(danteId, out int parsedId) ? parsedId : null);
            }
        }
    }

    private static void ValidateMulticastFlowReferences(
        XElement device,
        string deviceName,
        DanteValidationResult result)
    {
        HashSet<string> txIds = device.Children("txchannel")
            .Select(channel => channel.Attribute("danteId")?.Value.Trim() ?? string.Empty)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (XElement flow in device.Children("txflow"))
        {
            string flowId = flow.Attribute("danteId")?.Value.Trim() ?? "?";
            foreach (XElement slot in flow.Children("slot"))
            {
                string channelId = slot.Attribute("channelId")?.Value.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(channelId) || !txIds.Contains(channelId))
                {
                    result.AddError(
                        DanteIssueCategory.Channel,
                        $"{deviceName} : le txflow {flowId} référence le canal TX '{Blank(channelId, 0)}', qui n'existe pas. "
                        + "Supprimez le slot invalide ou restaurez le canal TX concerné.",
                        deviceName);
                }
            }
        }
    }

    private static string Blank(string value, int fallback)
    {
        return string.IsNullOrWhiteSpace(value)
            ? fallback > 0 ? $"position {fallback}" : "(vide)"
            : value;
    }

    private sealed record DeviceIdentityEntry(
        string Name,
        string DeviceId,
        string ProcessId);
}
