using DanteConfigEditor.Models;

namespace DanteConfigEditor.Services;

public enum PatchNavigationStatus
{
    Found,
    Free,
    MissingDevice,
    MissingChannel,
    AmbiguousChannel,
    NoDestinations
}

public sealed record PatchSourceNavigationResult(
    PatchNavigationStatus Status,
    PatchSourceDescriptor? Source,
    string? RequestedDeviceName,
    string? RequestedChannelName)
{
    public bool CanNavigate => Status == PatchNavigationStatus.Found && Source is not null;
}

public sealed record PatchDestinationNavigationResult(
    PatchNavigationStatus Status,
    IReadOnlyList<PatchTargetDescriptor> Destinations,
    int AmbiguousReferenceCount)
{
    public bool CanNavigate =>
        Status == PatchNavigationStatus.Found && Destinations.Count > 0;
}

/// <summary>
/// Index de navigation en lecture seule entre les RX et les TX. Il s'appuie
/// sur l'affectation effective de la session sans modifier le projet ni le lot.
/// </summary>
public sealed class PatchNavigationService
{
    private readonly DanteProject _project;
    private readonly IPatchWorkspaceSession _session;
    private readonly IReadOnlyList<PatchTargetDescriptor> _targets;

    public PatchNavigationService(
        DanteProject project,
        IPatchWorkspaceSession session)
    {
        _project = project ?? throw new ArgumentNullException(nameof(project));
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _targets = project.Devices
            .SelectMany(device => device.RxChannels.Select(channel =>
                new PatchTargetDescriptor(
                    device.Name,
                    channel.DanteId,
                    channel.PositionIndex,
                    channel.DisplayName)))
            .ToArray();
    }

    public PatchSourceNavigationResult FindSource(PatchTargetDescriptor target)
    {
        ArgumentNullException.ThrowIfNull(target);
        EffectivePatchAssignment assignment = _session.GetEffectiveAssignment(target);
        if (!assignment.IsActive)
        {
            return new PatchSourceNavigationResult(
                PatchNavigationStatus.Free,
                null,
                null,
                null);
        }

        string deviceName = assignment.TxDeviceName!;
        string channelName = assignment.TxChannelName ?? string.Empty;
        DanteDevice? device = _project.FindDevice(deviceName);
        if (device is null)
        {
            return new PatchSourceNavigationResult(
                PatchNavigationStatus.MissingDevice,
                null,
                deviceName,
                channelName);
        }

        DanteChannel[] matches = ResolveChannels(device, channelName);
        if (matches.Length == 0)
        {
            return new PatchSourceNavigationResult(
                PatchNavigationStatus.MissingChannel,
                null,
                device.Name,
                channelName);
        }

        if (matches.Length > 1)
        {
            return new PatchSourceNavigationResult(
                PatchNavigationStatus.AmbiguousChannel,
                null,
                device.Name,
                channelName);
        }

        DanteChannel channel = matches[0];
        return new PatchSourceNavigationResult(
            PatchNavigationStatus.Found,
            new PatchSourceDescriptor(
                device.Name,
                channel.DanteId,
                channel.PositionIndex,
                channel.DisplayName),
            device.Name,
            channelName);
    }

    public PatchDestinationNavigationResult FindDestinations(
        PatchSourceDescriptor source)
    {
        ArgumentNullException.ThrowIfNull(source);
        DanteDevice? sourceDevice = _project.FindDevice(source.DeviceName);
        if (sourceDevice is null)
        {
            return new PatchDestinationNavigationResult(
                PatchNavigationStatus.MissingDevice,
                [],
                0);
        }

        DanteChannel? requestedChannel = sourceDevice.TxChannels.FirstOrDefault(channel =>
            channel.DanteId == source.DanteId);
        if (requestedChannel is null)
        {
            return new PatchDestinationNavigationResult(
                PatchNavigationStatus.MissingChannel,
                [],
                0);
        }

        List<PatchTargetDescriptor> destinations = [];
        int ambiguousReferenceCount = 0;
        foreach (PatchTargetDescriptor target in _targets)
        {
            EffectivePatchAssignment assignment = _session.GetEffectiveAssignment(target);
            if (!assignment.IsActive
                || !string.Equals(
                    assignment.TxDeviceName,
                    sourceDevice.Name,
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            DanteChannel[] matches = ResolveChannels(
                sourceDevice,
                assignment.TxChannelName ?? string.Empty);
            if (matches.Length > 1)
            {
                ambiguousReferenceCount++;
                continue;
            }

            if (matches.Length == 1 && matches[0].DanteId == requestedChannel.DanteId)
            {
                destinations.Add(target);
            }
        }

        PatchTargetDescriptor[] ordered = destinations
            .OrderBy(target => target.DeviceName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(target => target.PositionIndex)
            .ThenBy(target => target.DanteId)
            .ToArray();
        PatchNavigationStatus status = ordered.Length > 0
            ? PatchNavigationStatus.Found
            : ambiguousReferenceCount > 0
                ? PatchNavigationStatus.AmbiguousChannel
                : PatchNavigationStatus.NoDestinations;
        return new PatchDestinationNavigationResult(
            status,
            ordered,
            ambiguousReferenceCount);
    }

    private static DanteChannel[] ResolveChannels(
        DanteDevice device,
        string referencedName)
    {
        string name = referencedName.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return [];
        }

        return device.TxChannels
            .Where(channel =>
                string.Equals(
                    channel.DisplayName,
                    name,
                    StringComparison.OrdinalIgnoreCase)
                || string.Equals(
                    channel.DanteId.ToString(),
                    name,
                    StringComparison.OrdinalIgnoreCase))
            .DistinctBy(channel => channel.DanteId)
            .ToArray();
    }
}
