using DanteConfigEditor.Domain.Projects;
using DanteConfigEditor.Models;

namespace DanteConfigEditor.Application.Commands;

public sealed class RenameChannelCommand : IProjectCommand
{
    public RenameChannelCommand(
        string deviceStableIdentity,
        DanteChannelKind channelKind,
        int channelIndex,
        string newName)
    {
        DeviceStableIdentity = deviceStableIdentity;
        ChannelKind = channelKind;
        ChannelIndex = channelIndex;
        NewName = newName;
    }

    public string DeviceStableIdentity { get; }

    public DanteChannelKind ChannelKind { get; }

    public int ChannelIndex { get; }

    public string NewName { get; }

    public string Id => ChannelKind == DanteChannelKind.Tx
        ? "channel.tx.rename"
        : "channel.rx.rename";

    public string DescriptionKey => ChannelKind == DanteChannelKind.Tx
        ? "History.RenameTx"
        : "History.RenameRx";

    public CommandPreparation Prepare(ProjectSession session)
    {
        DanteDevice? device = ProjectCommandHelpers.FindDevice(session, DeviceStableIdentity);
        List<CommandProblem> errors = [];
        bool capability = ChannelKind == DanteChannelKind.Tx
            ? session.Profile.Capabilities.CanEditTxLabels
            : session.Profile.Capabilities.CanEditRxLabels;
        if (!capability)
        {
            errors.Add(ProjectCommandHelpers.Error("CapabilityUnavailable", Id));
        }
        if (device is null)
        {
            errors.Add(ProjectCommandHelpers.Error("DeviceNotFound", DeviceStableIdentity));
        }
        else if (ProjectCommandHelpers.Channels(device, ChannelKind)
                 .All(channel => channel.Index != ChannelIndex))
        {
            errors.Add(ProjectCommandHelpers.Error("ChannelNotFound", ChannelIndex.ToString()));
        }
        if (string.IsNullOrWhiteSpace(NewName))
        {
            errors.Add(ProjectCommandHelpers.Error("NameRequired", nameof(NewName)));
        }

        IReadOnlyList<ProjectEntityReference> affected = device is null
            ? []
            : [ProjectCommandHelpers.ChannelReference(device, ChannelKind, ChannelIndex)];
        return new CommandPreparation(Id, DescriptionKey, affected, [], errors);
    }

    public void Execute(ProjectSession session)
    {
        DanteDevice device = ProjectCommandHelpers.FindDevice(session, DeviceStableIdentity)
            ?? throw new InvalidOperationException("Command.Error.DeviceNotFound");
        session.Project.RenameChannel(device.Name, ChannelKind, ChannelIndex, NewName);
    }
}
