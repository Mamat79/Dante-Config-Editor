using DanteConfigEditor.Services;

namespace DanteConfigEditor.Models;

public sealed partial class DanteProject
{
    public int ApplyChannelLabels(IEnumerable<ChannelLabelAssignment> assignments)
    {
        ChannelLabelAssignment[] requested = (assignments ?? [])
            .Where(assignment => !string.IsNullOrWhiteSpace(assignment.DeviceName))
            .ToArray();
        if (requested.Length == 0)
        {
            return 0;
        }

        ChannelLabelAssignment[] duplicateTargets = requested
            .GroupBy(
                assignment => (Device: assignment.DeviceName.Trim().ToUpperInvariant(), assignment.Kind, assignment.DanteId))
            .Where(group => group.Select(assignment => assignment.Label.Trim()).Distinct(StringComparer.Ordinal).Count() > 1)
            .SelectMany(group => group)
            .ToArray();
        if (duplicateTargets.Length > 0)
        {
            throw new InvalidOperationException("Le transfert contient plusieurs labels différents pour un même canal cible.");
        }

        int changed = 0;
        ApplyBatch(_ =>
        {
            foreach (IGrouping<(string DeviceName, DanteChannelKind Kind), ChannelLabelAssignment> group in requested
                         .GroupBy(assignment => (assignment.DeviceName.Trim(), assignment.Kind), new DeviceKindComparer()))
            {
                DanteDevice device = FindDevice(group.Key.DeviceName)
                    ?? throw new InvalidOperationException($"Machine introuvable : {group.Key.DeviceName}.");
                IReadOnlyList<DanteChannel> channels = group.Key.Kind == DanteChannelKind.Tx ? device.TxChannels : device.RxChannels;
                Dictionary<int, DanteChannel> channelsById = channels
                    .GroupBy(channel => channel.DanteId)
                    .ToDictionary(channelGroup => channelGroup.Key, channelGroup => channelGroup.First());
                List<(string OldName, string NewName)> txRenames = [];

                foreach (ChannelLabelAssignment assignment in group
                             .GroupBy(item => item.DanteId)
                             .Select(item => item.First())
                             .OrderBy(item => item.DanteId))
                {
                    if (!channelsById.TryGetValue(assignment.DanteId, out DanteChannel? channel))
                    {
                        throw new InvalidOperationException($"Canal {group.Key.Kind} {assignment.DanteId} introuvable sur {device.Name}.");
                    }

                    string newLabel = assignment.Label.Trim();
                    if (string.IsNullOrWhiteSpace(newLabel))
                    {
                        throw new InvalidOperationException("Un label importé est vide.");
                    }

                    if (ContainsProblematicCharacters(newLabel))
                    {
                        throw new InvalidOperationException($"Le label '{newLabel}' contient des caractères non imprimables.");
                    }

                    string oldLabel = channel.DisplayName;
                    if (string.Equals(oldLabel, newLabel, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    SetChannelDisplayName(channel, group.Key.Kind == DanteChannelKind.Tx ? "label" : "name", newLabel);
                    if (group.Key.Kind == DanteChannelKind.Tx)
                    {
                        txRenames.Add((oldLabel, newLabel));
                    }

                    changed++;
                }

                if (txRenames.Count > 0)
                {
                    // Une seule passe évite les cascades lors d'une permutation A <-> B.
                    UpdateSubscriptionsForRenamedTxChannels(
                        MachineRoleIdentityService.ReadVisibleName(device.Element),
                        txRenames);
                }
            }

            if (changed > 0)
            {
                RegisterChange("Import labels", $"{changed} label(s) de canal appliqué(s)");
            }
        });

        return changed;
    }

    private sealed class DeviceKindComparer : IEqualityComparer<(string DeviceName, DanteChannelKind Kind)>
    {
        public bool Equals((string DeviceName, DanteChannelKind Kind) left, (string DeviceName, DanteChannelKind Kind) right)
        {
            return left.Kind == right.Kind
                && string.Equals(left.DeviceName, right.DeviceName, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode((string DeviceName, DanteChannelKind Kind) value)
        {
            return HashCode.Combine(StringComparer.OrdinalIgnoreCase.GetHashCode(value.DeviceName), value.Kind);
        }
    }
}
