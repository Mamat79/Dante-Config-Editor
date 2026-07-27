using DanteConfigEditor.Domain.Projects;
using DanteConfigEditor.Models;

namespace DanteConfigEditor.Application.Commands;

public sealed class RenameDeviceCommand : IProjectCommand
{
    public RenameDeviceCommand(string deviceStableIdentity, string newName)
    {
        DeviceStableIdentity = deviceStableIdentity;
        NewName = newName;
    }

    public string DeviceStableIdentity { get; }

    public string NewName { get; }

    public string Id => "device.rename";

    public string DescriptionKey => "History.RenameDevice";

    public CommandPreparation Prepare(ProjectSession session)
    {
        DanteDevice? device = ProjectCommandHelpers.FindDevice(session, DeviceStableIdentity);
        List<CommandProblem> errors = [];
        if (!session.Profile.Capabilities.CanEditDeviceNames)
        {
            errors.Add(ProjectCommandHelpers.Error("CapabilityUnavailable", "Device name editing is disabled by the XML profile."));
        }
        if (device is null)
        {
            errors.Add(ProjectCommandHelpers.Error("DeviceNotFound", DeviceStableIdentity));
        }
        if (string.IsNullOrWhiteSpace(NewName))
        {
            errors.Add(ProjectCommandHelpers.Error("NameRequired", nameof(NewName)));
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
        session.Project.RenameDevice(device.Name, NewName);
    }
}
