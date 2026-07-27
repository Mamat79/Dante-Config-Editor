using DanteConfigEditor.Domain.Projects;
using DanteConfigEditor.Models;
using DanteConfigEditor.Services;

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

public sealed class DuplicateDeviceCommand : IProjectCommand
{
    public DuplicateDeviceCommand(
        string sourceDeviceStableIdentity,
        MachineCloneOptions options)
    {
        SourceDeviceStableIdentity = sourceDeviceStableIdentity;
        Options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public string SourceDeviceStableIdentity { get; }

    public MachineCloneOptions Options { get; }

    public string Id => "device.duplicate";

    public string DescriptionKey => "History.DuplicateDevice";

    public CommandPreparation Prepare(ProjectSession session)
    {
        DanteDevice? source = ProjectCommandHelpers.FindDevice(
            session,
            SourceDeviceStableIdentity);
        List<CommandProblem> errors = [];
        List<CommandProblem> warnings = [];
        ValidateCreationRequest(session, Options.NewName, errors);
        if (source is null)
        {
            errors.Add(ProjectCommandHelpers.Error(
                "DeviceNotFound",
                SourceDeviceStableIdentity));
        }
        if (Options.PreserveNetworkConfiguration)
        {
            warnings.Add(ProjectCommandHelpers.Warning(
                "NetworkConfigurationCopied",
                Options.NewName));
        }
        if (Options.PreserveSubscriptions || Options.PreserveMulticastFlows)
        {
            warnings.Add(ProjectCommandHelpers.Warning(
                "ProjectReferencesCopied",
                Options.NewName));
        }

        List<ProjectEntityReference> affected = [];
        if (source is not null)
        {
            affected.Add(ProjectCommandHelpers.DeviceReference(source));
        }
        affected.Add(PendingDeviceReference(Options.NewName));
        return new CommandPreparation(Id, DescriptionKey, affected, warnings, errors);
    }

    public void Execute(ProjectSession session)
    {
        DanteDevice source = ProjectCommandHelpers.FindDevice(
            session,
            SourceDeviceStableIdentity)
            ?? throw new InvalidOperationException("Command.Error.DeviceNotFound");
        session.Project.DuplicateDevice(source.Name, Options);
    }

    private static void ValidateCreationRequest(
        ProjectSession session,
        string? newName,
        ICollection<CommandProblem> errors)
    {
        if (!session.Profile.Capabilities.CanCreateDevices)
        {
            errors.Add(ProjectCommandHelpers.Error(
                "CapabilityUnavailable",
                "CanCreateDevices"));
        }

        string? nameError = DanteNameRules.ValidateDeviceName(newName);
        if (nameError is not null)
        {
            errors.Add(ProjectCommandHelpers.Error("InvalidDeviceName", nameError));
        }
        else if (session.Project.Devices.Any(device => string.Equals(
                     device.Name,
                     newName?.Trim(),
                     StringComparison.OrdinalIgnoreCase)))
        {
            errors.Add(ProjectCommandHelpers.Error(
                "DuplicateDeviceName",
                newName ?? string.Empty));
        }
    }

    private static ProjectEntityReference PendingDeviceReference(string name) =>
        new(
            ProjectEntityKind.Device,
            $"pending-device:{name.Trim()}",
            name.Trim());
}

public sealed class AddDeviceFromLibraryCommand : IProjectCommand
{
    public AddDeviceFromLibraryCommand(
        MachineTemplatePackage template,
        MachineInstanceOptions options)
    {
        Template = template ?? throw new ArgumentNullException(nameof(template));
        Options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public MachineTemplatePackage Template { get; }

    public MachineInstanceOptions Options { get; }

    public string Id => "device.add-from-library";

    public string DescriptionKey => "History.AddDeviceFromLibrary";

    public CommandPreparation Prepare(ProjectSession session)
    {
        List<CommandProblem> errors = [];
        if (!session.Profile.Capabilities.CanCreateDevices)
        {
            errors.Add(ProjectCommandHelpers.Error(
                "CapabilityUnavailable",
                "CanCreateDevices"));
        }

        string? nameError = DanteNameRules.ValidateDeviceName(Options.NewName);
        if (nameError is not null)
        {
            errors.Add(ProjectCommandHelpers.Error("InvalidDeviceName", nameError));
        }
        else if (session.Project.Devices.Any(device => string.Equals(
                     device.Name,
                     Options.NewName.Trim(),
                     StringComparison.OrdinalIgnoreCase)))
        {
            errors.Add(ProjectCommandHelpers.Error(
                "DuplicateDeviceName",
                Options.NewName));
        }
        if (!string.Equals(
                Template.Metadata.SourcePresetVersion,
                session.Project.PresetVersion,
                StringComparison.OrdinalIgnoreCase))
        {
            errors.Add(ProjectCommandHelpers.Error(
                "TemplateProfileMismatch",
                $"{Template.Metadata.SourcePresetVersion} != {session.Project.PresetVersion}"));
        }

        ProjectEntityReference affected = new(
            ProjectEntityKind.Device,
            $"pending-device:{Options.NewName.Trim()}",
            Options.NewName.Trim());
        return new CommandPreparation(Id, DescriptionKey, [affected], [], errors);
    }

    public void Execute(ProjectSession session)
    {
        session.Project.AddDeviceFromTemplate(Template, Options);
    }
}
