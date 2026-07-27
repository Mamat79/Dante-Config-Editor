using DanteConfigEditor.Application;
using DanteConfigEditor.Application.Commands;
using DanteConfigEditor.DanteXml;
using DanteConfigEditor.Models;
using DanteConfigEditor.Services;

namespace DanteConfigEditorV3.Tests;

public sealed class DeviceLibraryCommandTests
{
    [Fact]
    public void DuplicateDeviceIsOneUndoableTransaction()
    {
        ProjectSession session = OpenSession();
        DanteDevice source = session.Project.FindDevice("DEVICE-A")!;
        string sourceIdentity = source.StableIdentity;

        CommandExecutionResult result = session.CommandDispatcher.Execute(
            new DuplicateDeviceCommand(
                sourceIdentity,
                new MachineCloneOptions
                {
                    NewName = "DEVICE-A-COPY"
                }));

        DanteDevice duplicate = session.Project.FindDevice("DEVICE-A-COPY")!;
        Assert.Equal("device.duplicate", result.CommandId);
        Assert.True(duplicate.IsGenericRole);
        Assert.NotEqual(sourceIdentity, duplicate.StableIdentity);
        Assert.Single(session.History);
        Assert.False(session.Project.Validate().HasErrors);

        session.CommandDispatcher.Undo();
        Assert.Null(session.Project.FindDevice("DEVICE-A-COPY"));
        Assert.NotNull(session.Project.FindDevice("DEVICE-A"));

        session.CommandDispatcher.Redo();
        Assert.NotNull(session.Project.FindDevice("DEVICE-A-COPY"));
    }

    [Fact]
    public void AddFromLibraryKeepsTemplateIndependentAndSupportsUndoRedo()
    {
        ProjectSession session = OpenSession();
        MachineTemplatePackage template = MachineTemplateService.CreateFromDevice(
            session.Project.FindDevice("DEVICE-A")!,
            session.Project.PresetVersion,
            new MachineTemplateCreateRequest
            {
                TemplateName = "Reusable device"
            });
        string before = template.TemplateDocument.ToString(
            System.Xml.Linq.SaveOptions.DisableFormatting);

        session.CommandDispatcher.Execute(new AddDeviceFromLibraryCommand(
            template,
            new MachineInstanceOptions
            {
                NewName = "LIBRARY-DEVICE",
                TxLabelPrefix = "LIB TX",
                RxLabelPrefix = "LIB RX"
            }));

        DanteDevice added = session.Project.FindDevice("LIBRARY-DEVICE")!;
        Assert.Equal(["LIB TX 1", "LIB TX 2"], added.TxChannels.Select(channel => channel.DisplayName));
        Assert.Equal(
            before,
            template.TemplateDocument.ToString(
                System.Xml.Linq.SaveOptions.DisableFormatting));
        Assert.False(session.Project.ValidateXmlChangeGuard().HasErrors);

        session.CommandDispatcher.Undo();
        Assert.Null(session.Project.FindDevice("LIBRARY-DEVICE"));
        session.CommandDispatcher.Redo();
        Assert.NotNull(session.Project.FindDevice("LIBRARY-DEVICE"));
    }

    [Fact]
    public void DuplicateNameIsRejectedBeforeProjectMutation()
    {
        ProjectSession session = OpenSession();
        DanteDevice source = session.Project.FindDevice("DEVICE-A")!;
        int deviceCount = session.Project.Devices.Count;
        DuplicateDeviceCommand command = new(
            source.StableIdentity,
            new MachineCloneOptions
            {
                NewName = "DEVICE-B"
            });

        CommandPreparation preview = session.CommandDispatcher.Preview(command);

        Assert.False(preview.CanExecute);
        Assert.Contains(preview.Errors, error => error.Code == "DuplicateDeviceName");
        Assert.Throws<InvalidOperationException>(() =>
            session.CommandDispatcher.Execute(command));
        Assert.Equal(deviceCount, session.Project.Devices.Count);
        Assert.Empty(session.History);
    }

    private static ProjectSession OpenSession()
    {
        DanteXmlOpenResult opened = new DanteXmlProjectAdapter().Open(Path.Combine(
            AppContext.BaseDirectory,
            "Fixtures",
            "representative-preset.xml"));
        ProjectSession session = new();
        session.OpenXml(opened);
        return session;
    }
}
