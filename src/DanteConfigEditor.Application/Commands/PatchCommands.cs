using DanteConfigEditor.Domain.Projects;
using DanteConfigEditor.Models;

namespace DanteConfigEditor.Application.Commands;

public sealed record SubscriptionAssignment(
    string RxDeviceStableIdentity,
    int RxChannelIndex,
    string TxDeviceStableIdentity,
    int TxChannelIndex);

public sealed record StableSubscriptionEdit(
    string RxDeviceStableIdentity,
    int RxChannelIndex,
    string? TxDeviceStableIdentity,
    int? TxChannelIndex)
{
    public bool IsRemoval => string.IsNullOrWhiteSpace(TxDeviceStableIdentity);
}

public sealed class ApplyPatchBatchCommand : IProjectCommand
{
    private readonly IReadOnlyList<StableSubscriptionEdit> _edits;

    public ApplyPatchBatchCommand(IEnumerable<StableSubscriptionEdit> edits)
    {
        _edits = edits.ToArray();
    }

    public string Id => "patch.apply-batch";

    public string DescriptionKey => "History.ApplyPatchBatch";

    public CommandPreparation Prepare(ProjectSession session)
    {
        List<CommandProblem> errors = [];
        List<ProjectEntityReference> affected = [];
        if (!session.Profile.Capabilities.CanEditPatch)
        {
            errors.Add(ProjectCommandHelpers.Error("CapabilityUnavailable", Id));
        }

        if (_edits.Count == 0)
        {
            errors.Add(ProjectCommandHelpers.Error("RangeEmpty", Id));
        }

        bool duplicateTarget = _edits
            .GroupBy(edit => (edit.RxDeviceStableIdentity, edit.RxChannelIndex))
            .Any(group => group.Count() > 1);
        if (duplicateTarget)
        {
            errors.Add(ProjectCommandHelpers.Error("DuplicatePatchTarget", Id));
        }

        foreach (StableSubscriptionEdit edit in _edits)
        {
            DanteDevice? rxDevice = ProjectCommandHelpers.FindDevice(
                session,
                edit.RxDeviceStableIdentity);
            if (rxDevice is null
                || rxDevice.RxChannels.All(channel => channel.Index != edit.RxChannelIndex))
            {
                errors.Add(ProjectCommandHelpers.Error(
                    "ChannelNotFound",
                    $"{edit.RxDeviceStableIdentity}:RX:{edit.RxChannelIndex}"));
                continue;
            }

            affected.Add(ProjectCommandHelpers.ChannelReference(
                rxDevice,
                DanteChannelKind.Rx,
                edit.RxChannelIndex));
            if (edit.IsRemoval)
            {
                continue;
            }

            DanteDevice? txDevice = ProjectCommandHelpers.FindDevice(
                session,
                edit.TxDeviceStableIdentity!);
            if (txDevice is null
                || edit.TxChannelIndex is not int txChannelIndex
                || txDevice.TxChannels.All(channel => channel.Index != txChannelIndex))
            {
                errors.Add(ProjectCommandHelpers.Error(
                    "ChannelNotFound",
                    $"{edit.TxDeviceStableIdentity}:TX:{edit.TxChannelIndex}"));
            }
        }

        return new CommandPreparation(
            Id,
            DescriptionKey,
            affected.Distinct().ToArray(),
            [],
            errors);
    }

    public void Execute(ProjectSession session)
    {
        session.Project.ApplyBatch(project =>
        {
            foreach (StableSubscriptionEdit edit in _edits)
            {
                DanteDevice rxDevice = project.FindDeviceByStableIdentity(
                    edit.RxDeviceStableIdentity)
                    ?? throw new InvalidOperationException("Command.Error.DeviceNotFound");
                if (edit.IsRemoval)
                {
                    project.RemovePatch(rxDevice.Name, edit.RxChannelIndex);
                    continue;
                }

                DanteDevice txDevice = project.FindDeviceByStableIdentity(
                    edit.TxDeviceStableIdentity!)
                    ?? throw new InvalidOperationException("Command.Error.DeviceNotFound");
                DanteChannel txChannel = txDevice.TxChannels.First(channel =>
                    channel.Index == edit.TxChannelIndex);
                project.ApplyPatch(
                    rxDevice.Name,
                    edit.RxChannelIndex,
                    txDevice.Name,
                    txChannel.DisplayName);
            }
        });
    }
}

public sealed class AssignSubscriptionRangeCommand : IProjectCommand
{
    private readonly IReadOnlyList<SubscriptionAssignment> _assignments;

    public AssignSubscriptionRangeCommand(IEnumerable<SubscriptionAssignment> assignments)
    {
        _assignments = assignments.ToArray();
    }

    public string Id => "patch.assign-range";

    public string DescriptionKey => "History.AssignPatchRange";

    public CommandPreparation Prepare(ProjectSession session)
    {
        List<CommandProblem> errors = [];
        List<ProjectEntityReference> affected = [];
        if (!session.Profile.Capabilities.CanEditPatch)
        {
            errors.Add(ProjectCommandHelpers.Error("CapabilityUnavailable", Id));
        }
        if (_assignments.Count == 0)
        {
            errors.Add(ProjectCommandHelpers.Error("RangeEmpty", Id));
        }

        foreach (SubscriptionAssignment assignment in _assignments)
        {
            DanteDevice? rxDevice = ProjectCommandHelpers.FindDevice(
                session,
                assignment.RxDeviceStableIdentity);
            DanteDevice? txDevice = ProjectCommandHelpers.FindDevice(
                session,
                assignment.TxDeviceStableIdentity);
            if (rxDevice is null || txDevice is null)
            {
                errors.Add(ProjectCommandHelpers.Error(
                    "DeviceNotFound",
                    rxDevice is null
                        ? assignment.RxDeviceStableIdentity
                        : assignment.TxDeviceStableIdentity));
                continue;
            }

            if (rxDevice.RxChannels.All(channel => channel.Index != assignment.RxChannelIndex))
            {
                errors.Add(ProjectCommandHelpers.Error(
                    "ChannelNotFound",
                    $"{rxDevice.Name}:RX:{assignment.RxChannelIndex}"));
            }
            if (txDevice.TxChannels.All(channel => channel.Index != assignment.TxChannelIndex))
            {
                errors.Add(ProjectCommandHelpers.Error(
                    "ChannelNotFound",
                    $"{txDevice.Name}:TX:{assignment.TxChannelIndex}"));
            }

            affected.Add(ProjectCommandHelpers.ChannelReference(
                rxDevice,
                DanteChannelKind.Rx,
                assignment.RxChannelIndex));
        }

        return new CommandPreparation(
            Id,
            DescriptionKey,
            affected.Distinct().ToArray(),
            [],
            errors);
    }

    public void Execute(ProjectSession session)
    {
        session.Project.ApplyBatch(project =>
        {
            foreach (SubscriptionAssignment assignment in _assignments)
            {
                DanteDevice rxDevice = project.FindDeviceByStableIdentity(
                    assignment.RxDeviceStableIdentity)
                    ?? throw new InvalidOperationException("Command.Error.DeviceNotFound");
                DanteDevice txDevice = project.FindDeviceByStableIdentity(
                    assignment.TxDeviceStableIdentity)
                    ?? throw new InvalidOperationException("Command.Error.DeviceNotFound");
                DanteChannel txChannel = txDevice.TxChannels
                    .First(channel => channel.Index == assignment.TxChannelIndex);

                project.ApplyPatch(
                    rxDevice.Name,
                    assignment.RxChannelIndex,
                    txDevice.Name,
                    txChannel.DisplayName);
            }
        });
    }
}

public sealed class ClearSubscriptionsCommand : IProjectCommand
{
    private readonly IReadOnlyList<(string DeviceStableIdentity, int RxChannelIndex)> _targets;

    public ClearSubscriptionsCommand(
        IEnumerable<(string DeviceStableIdentity, int RxChannelIndex)> targets)
    {
        _targets = targets.ToArray();
    }

    public string Id => "patch.clear-range";

    public string DescriptionKey => "History.ClearPatchRange";

    public CommandPreparation Prepare(ProjectSession session)
    {
        List<CommandProblem> errors = [];
        List<ProjectEntityReference> affected = [];
        if (!session.Profile.Capabilities.CanEditPatch)
        {
            errors.Add(ProjectCommandHelpers.Error("CapabilityUnavailable", Id));
        }
        foreach ((string stableIdentity, int rxIndex) in _targets)
        {
            DanteDevice? device = ProjectCommandHelpers.FindDevice(session, stableIdentity);
            if (device is null || device.RxChannels.All(channel => channel.Index != rxIndex))
            {
                errors.Add(ProjectCommandHelpers.Error(
                    "ChannelNotFound",
                    $"{stableIdentity}:RX:{rxIndex}"));
                continue;
            }

            affected.Add(ProjectCommandHelpers.ChannelReference(
                device,
                DanteChannelKind.Rx,
                rxIndex));
        }

        return new CommandPreparation(Id, DescriptionKey, affected, [], errors);
    }

    public void Execute(ProjectSession session)
    {
        session.Project.ApplyBatch(project =>
        {
            foreach ((string stableIdentity, int rxIndex) in _targets)
            {
                DanteDevice device = project.FindDeviceByStableIdentity(stableIdentity)
                    ?? throw new InvalidOperationException("Command.Error.DeviceNotFound");
                project.RemovePatch(device.Name, rxIndex);
            }
        });
    }
}
