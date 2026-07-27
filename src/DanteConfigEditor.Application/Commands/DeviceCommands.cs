using DanteConfigEditor.Domain.Projects;
using DanteConfigEditor.Models;

namespace DanteConfigEditor.Application.Commands;

public sealed class DeleteDeviceCommand : IProjectCommand
{
    public DeleteDeviceCommand(string deviceStableIdentity)
    {
        DeviceStableIdentity = deviceStableIdentity;
    }

    public string DeviceStableIdentity { get; }

    public string Id => "device.delete";

    public string DescriptionKey => "History.DeleteDevice";

    public CommandPreparation Prepare(ProjectSession session)
    {
        DanteDevice? device = ProjectCommandHelpers.FindDevice(session, DeviceStableIdentity);
        List<CommandProblem> errors = [];
        if (device is null)
        {
            errors.Add(ProjectCommandHelpers.Error("DeviceNotFound", DeviceStableIdentity));
        }

        IReadOnlyList<ProjectEntityReference> affected = device is null
            ? []
            : [ProjectCommandHelpers.DeviceReference(device)];
        return new CommandPreparation(Id, DescriptionKey, affected, [], errors);
    }

    public void Execute(ProjectSession session)
    {
        DanteDevice device = ProjectCommandHelpers.FindDevice(session, DeviceStableIdentity)
            ?? throw new InvalidOperationException("Command.Error.DeviceNotFound");
        session.Project.DeleteDevice(device.Name);
    }
}

public sealed class ChangeAudioFormatCommand : IProjectCommand
{
    public ChangeAudioFormatCommand(
        string deviceStableIdentity,
        string? sampleRate = null,
        string? encoding = null,
        string? latency = null)
    {
        DeviceStableIdentity = deviceStableIdentity;
        SampleRate = sampleRate;
        Encoding = encoding;
        Latency = latency;
    }

    public string DeviceStableIdentity { get; }

    public string? SampleRate { get; }

    public string? Encoding { get; }

    public string? Latency { get; }

    public string Id => "device.change-audio-format";

    public string DescriptionKey => "History.ChangeAudioFormat";

    public CommandPreparation Prepare(ProjectSession session)
    {
        DanteDevice? device = ProjectCommandHelpers.FindDevice(session, DeviceStableIdentity);
        List<CommandProblem> errors = [];
        if (!session.Profile.Capabilities.CanEditAudioFormat)
        {
            errors.Add(ProjectCommandHelpers.Error("CapabilityUnavailable", Id));
        }
        if (device is null)
        {
            errors.Add(ProjectCommandHelpers.Error("DeviceNotFound", DeviceStableIdentity));
        }
        if (SampleRate is null && Encoding is null && Latency is null)
        {
            errors.Add(ProjectCommandHelpers.Error("NoChangeRequested", Id));
        }

        IReadOnlyList<ProjectEntityReference> affected = device is null
            ? []
            : [ProjectCommandHelpers.DeviceReference(device)];
        return new CommandPreparation(Id, DescriptionKey, affected, [], errors);
    }

    public void Execute(ProjectSession session)
    {
        DanteDevice device = ProjectCommandHelpers.FindDevice(session, DeviceStableIdentity)
            ?? throw new InvalidOperationException("Command.Error.DeviceNotFound");
        string deviceName = device.Name;
        session.Project.ApplyBatch(project =>
        {
            if (SampleRate is not null)
            {
                project.SetSamplerate(deviceName, SampleRate);
            }
            if (Encoding is not null)
            {
                project.SetEncoding(deviceName, Encoding);
            }
            if (Latency is not null)
            {
                project.SetLatency(deviceName, Latency);
            }
        });
    }
}
