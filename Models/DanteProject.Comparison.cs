using System.Text;
using DanteConfigEditor.Services;

namespace DanteConfigEditor.Models;

public sealed partial class DanteProject
{
    public string CompareWith(
        DanteProject other,
        UiLanguage language = UiLanguage.French)
    {
        bool english = language == UiLanguage.English;
        StringBuilder builder = new();
        builder.AppendLine(english ? "XML COMPARISON" : "COMPARAISON XML");
        builder.AppendLine("==============");
        builder.AppendLine(english
            ? $"Open file: {OriginalFilePath}"
            : $"Fichier ouvert : {OriginalFilePath}");
        builder.AppendLine(english
            ? $"Compared file: {other.OriginalFilePath}"
            : $"Fichier comparé : {other.OriginalFilePath}");
        builder.AppendLine();

        Dictionary<string, DanteDevice> currentDevices = Devices
            .Where(device => !string.IsNullOrWhiteSpace(device.Name))
            .ToDictionary(device => device.Name, StringComparer.OrdinalIgnoreCase);
        Dictionary<string, DanteDevice> otherDevices = other.Devices
            .Where(device => !string.IsNullOrWhiteSpace(device.Name))
            .ToDictionary(device => device.Name, StringComparer.OrdinalIgnoreCase);

        List<string> differences = [];

        foreach (string deviceName in currentDevices.Keys.Except(otherDevices.Keys, StringComparer.OrdinalIgnoreCase))
        {
            differences.Add(english
                ? $"Device only in the open file: {deviceName}"
                : $"Device seulement dans le fichier ouvert : {deviceName}");
        }

        foreach (string deviceName in otherDevices.Keys.Except(currentDevices.Keys, StringComparer.OrdinalIgnoreCase))
        {
            differences.Add(english
                ? $"Device only in the compared file: {deviceName}"
                : $"Device seulement dans le fichier comparé : {deviceName}");
        }

        foreach (string deviceName in currentDevices.Keys.Intersect(otherDevices.Keys, StringComparer.OrdinalIgnoreCase))
        {
            DanteDevice current = currentDevices[deviceName];
            DanteDevice compared = otherDevices[deviceName];
            CompareValue(differences, $"{deviceName} / {(english ? "network mode" : "mode réseau")}", current.NetworkMode, compared.NetworkMode);
            CompareValue(differences, $"{deviceName} / {(english ? "latency" : "latence")}", DanteLatencyFormatter.FormatLatencyDisplay(current.Latency), DanteLatencyFormatter.FormatLatencyDisplay(compared.Latency));
            CompareValue(differences, $"{deviceName} / Preferred Master", current.PreferredMaster.ToString(), compared.PreferredMaster.ToString());
            CompareValue(differences, $"{deviceName} / sample rate", current.Element.Child("samplerate")?.Value.Trim() ?? string.Empty, compared.Element.Child("samplerate")?.Value.Trim() ?? string.Empty);
            CompareValue(differences, $"{deviceName} / encoding", current.Element.Child("encoding")?.Value.Trim() ?? string.Empty, compared.Element.Child("encoding")?.Value.Trim() ?? string.Empty);
            CompareChannels(differences, deviceName, "TX", current.TxChannels, compared.TxChannels, english);
            CompareChannels(differences, deviceName, "RX", current.RxChannels, compared.RxChannels, english);
        }

        Dictionary<string, DanteSubscription> currentPatches = PatchMatrix.Subscriptions
            .GroupBy(BuildPatchKey, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
        Dictionary<string, DanteSubscription> otherPatches = other.PatchMatrix.Subscriptions
            .GroupBy(BuildPatchKey, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        foreach (string patchKey in currentPatches.Keys.Except(otherPatches.Keys, StringComparer.OrdinalIgnoreCase))
        {
            differences.Add(english
                ? $"{patchKey}: subscription only in the open file ({FormatPatchForComparison(currentPatches[patchKey], english)})"
                : $"{patchKey} : patch seulement dans le fichier ouvert ({FormatPatchForComparison(currentPatches[patchKey], english)})");
        }

        foreach (string patchKey in otherPatches.Keys.Except(currentPatches.Keys, StringComparer.OrdinalIgnoreCase))
        {
            differences.Add(english
                ? $"{patchKey}: subscription only in the compared file ({FormatPatchForComparison(otherPatches[patchKey], english)})"
                : $"{patchKey} : patch seulement dans le fichier comparé ({FormatPatchForComparison(otherPatches[patchKey], english)})");
        }

        foreach (string patchKey in currentPatches.Keys.Intersect(otherPatches.Keys, StringComparer.OrdinalIgnoreCase))
        {
            DanteSubscription current = currentPatches[patchKey];
            DanteSubscription compared = otherPatches[patchKey];
            string currentPatch = FormatPatchForComparison(current, english);
            string comparedPatch = FormatPatchForComparison(compared, english);
            if (!string.Equals(currentPatch, comparedPatch, StringComparison.OrdinalIgnoreCase))
            {
                differences.Add(english
                    ? $"{patchKey}: open file = {currentPatch} | compared file = {comparedPatch}"
                    : $"{patchKey} : fichier ouvert = {currentPatch} | fichier comparé = {comparedPatch}");
            }
        }

        if (differences.Count == 0)
        {
            builder.AppendLine(english
                ? "No difference detected in known fields."
                : "Aucune différence détectée dans les champs connus.");
        }
        else
        {
            foreach (string difference in differences.Take(250))
            {
                builder.AppendLine("- " + difference);
            }

            if (differences.Count > 250)
            {
                builder.AppendLine(english
                    ? $"- {differences.Count - 250} additional difference(s) not displayed."
                    : $"- {differences.Count - 250} différence(s) supplémentaire(s) non affichée(s).");
            }
        }

        return builder.ToString();
    }

    private static void CompareValue(List<string> differences, string label, string current, string compared)
    {
        if (!string.Equals(current, compared, StringComparison.OrdinalIgnoreCase))
        {
            differences.Add($"{label}: {Blank(current)} -> {Blank(compared)}");
        }
    }

    private static void CompareChannels(
        List<string> differences,
        string deviceName,
        string kind,
        IReadOnlyList<DanteChannel> currentChannels,
        IReadOnlyList<DanteChannel> comparedChannels,
        bool english)
    {
        Dictionary<int, DanteChannel> currentById = currentChannels
            .GroupBy(channel => channel.DanteId)
            .ToDictionary(group => group.Key, group => group.First());
        Dictionary<int, DanteChannel> comparedById = comparedChannels
            .GroupBy(channel => channel.DanteId)
            .ToDictionary(group => group.Key, group => group.First());

        foreach (int danteId in currentById.Keys.Except(comparedById.Keys).OrderBy(id => id))
        {
            differences.Add(english
                ? $"{deviceName} / {kind} Dante ID {danteId}: only in the open file ({currentById[danteId].DisplayName})"
                : $"{deviceName} / {kind} Dante Id {danteId}: seulement dans le fichier ouvert ({currentById[danteId].DisplayName})");
        }

        foreach (int danteId in comparedById.Keys.Except(currentById.Keys).OrderBy(id => id))
        {
            differences.Add(english
                ? $"{deviceName} / {kind} Dante ID {danteId}: only in the compared file ({comparedById[danteId].DisplayName})"
                : $"{deviceName} / {kind} Dante Id {danteId}: seulement dans le fichier comparé ({comparedById[danteId].DisplayName})");
        }

        foreach (int danteId in currentById.Keys.Intersect(comparedById.Keys).OrderBy(id => id))
        {
            DanteChannel current = currentById[danteId];
            DanteChannel compared = comparedById[danteId];
            if (!string.Equals(current.DisplayName, compared.DisplayName, StringComparison.OrdinalIgnoreCase))
            {
                differences.Add($"{deviceName} / {kind} {(english ? "Dante ID" : "Dante Id")} {danteId}: {current.DisplayName} -> {compared.DisplayName}");
            }
        }
    }

    private static string BuildPatchKey(DanteSubscription subscription)
    {
        return $"{subscription.RxDevice} / RX Dante Id {subscription.RxDanteId}";
    }

    private static string FormatPatchForComparison(
        DanteSubscription subscription,
        bool english)
    {
        if (!subscription.IsActive)
        {
            return english ? "(free)" : "(libre)";
        }

        string sourceDevice = subscription.IsLocalSubscription
            ? $"LOCAL / {subscription.ResolvedTxDeviceName}"
            : Blank(subscription.DisplayTxDeviceName);

        return $"{sourceDevice} / {Blank(subscription.TxChannelName)} [{subscription.TypeLabel}]";
    }
}
