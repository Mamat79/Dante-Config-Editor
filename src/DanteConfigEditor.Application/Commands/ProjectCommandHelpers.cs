using DanteConfigEditor.Domain.Projects;
using DanteConfigEditor.Models;

namespace DanteConfigEditor.Application.Commands;

internal static class ProjectCommandHelpers
{
    public static DanteDevice? FindDevice(ProjectSession session, string stableIdentity) =>
        session.Project.FindDeviceByStableIdentity(stableIdentity);

    public static ProjectEntityReference DeviceReference(DanteDevice device) =>
        new(ProjectEntityKind.Device, device.StableIdentity, device.Name);

    public static ProjectEntityReference ChannelReference(
        DanteDevice device,
        DanteChannelKind channelKind,
        int channelIndex)
    {
        DanteChannel? channel = Channels(device, channelKind)
            .FirstOrDefault(item => item.Index == channelIndex);
        string displayName = channel?.DisplayName ?? channelIndex.ToString();
        return new ProjectEntityReference(
            channelKind == DanteChannelKind.Tx
                ? ProjectEntityKind.TxChannel
                : ProjectEntityKind.RxChannel,
            $"{device.StableIdentity}:{channelKind}:{channelIndex}",
            displayName,
            device.StableIdentity);
    }

    public static IReadOnlyList<DanteChannel> Channels(
        DanteDevice device,
        DanteChannelKind channelKind) =>
        channelKind == DanteChannelKind.Tx
            ? device.TxChannels
            : device.RxChannels;

    public static CommandProblem Error(string code, string detail) =>
        new(code, $"Command.Error.{code}", detail);

    public static CommandProblem Warning(string code, string detail) =>
        new(code, $"Command.Warning.{code}", detail);
}
