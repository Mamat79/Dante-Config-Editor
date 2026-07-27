using DanteConfigEditor.Domain.Projects;
using DanteConfigEditor.Models;

namespace DanteConfigEditor.Application.Commands;

public sealed class RenameChannelRangeCommand : IProjectCommand
{
    private readonly IReadOnlyDictionary<int, string> _newNames;

    public RenameChannelRangeCommand(
        string deviceStableIdentity,
        DanteChannelKind channelKind,
        IReadOnlyDictionary<int, string> newNames)
    {
        DeviceStableIdentity = deviceStableIdentity;
        ChannelKind = channelKind;
        _newNames = new Dictionary<int, string>(newNames);
    }

    public string DeviceStableIdentity { get; }

    public DanteChannelKind ChannelKind { get; }

    public string Id => ChannelKind == DanteChannelKind.Tx
        ? "channel.tx.rename-range"
        : "channel.rx.rename-range";

    public string DescriptionKey => "History.RenameChannelRange";

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
        if (_newNames.Count == 0)
        {
            errors.Add(ProjectCommandHelpers.Error("RangeEmpty", Id));
        }

        HashSet<int> available = device is null
            ? []
            : ProjectCommandHelpers.Channels(device, ChannelKind)
                .Select(channel => channel.Index)
                .ToHashSet();
        foreach ((int index, string name) in _newNames)
        {
            if (!available.Contains(index))
            {
                errors.Add(ProjectCommandHelpers.Error("ChannelNotFound", index.ToString()));
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                errors.Add(ProjectCommandHelpers.Error("NameRequired", index.ToString()));
            }
        }

        ProjectEntityReference[] affected = device is null
            ? []
            : _newNames.Keys
                .Order()
                .Select(index =>
                    ProjectCommandHelpers.ChannelReference(device, ChannelKind, index))
                .ToArray();
        return new CommandPreparation(Id, DescriptionKey, affected, [], errors);
    }

    public void Execute(ProjectSession session)
    {
        DanteDevice device = ProjectCommandHelpers.FindDevice(session, DeviceStableIdentity)
            ?? throw new InvalidOperationException("Command.Error.DeviceNotFound");
        string deviceName = device.Name;
        session.Project.ApplyBatch(project =>
        {
            foreach ((int index, string name) in _newNames.OrderBy(item => item.Key))
            {
                project.RenameChannel(deviceName, ChannelKind, index, name);
            }
        });
    }
}
