using DanteConfigEditor.Application;
using DanteConfigEditor.Application.Commands;
using DanteConfigEditor.DanteXml;
using DanteConfigEditor.Domain.Projects;
using DanteConfigEditor.Models;

namespace DanteConfigEditorV3.Tests;

public sealed class ProjectSessionCommandTests
{
    [Fact]
    public void RenameDeviceUsesStableIdentityAndSupportsUndoRedo()
    {
        ProjectSession session = OpenSession();
        DanteDevice source = session.Project.FindDevice("DEVICE-A")!;
        string stableIdentity = source.StableIdentity;

        CommandExecutionResult result = session.CommandDispatcher.Execute(
            new RenameDeviceCommand(stableIdentity, "DEVICE-A-NEW"));

        Assert.Equal("device.rename", result.CommandId);
        Assert.Null(session.Project.FindDevice("DEVICE-A"));
        Assert.Equal(stableIdentity, session.Project.FindDevice("DEVICE-A-NEW")!.StableIdentity);
        Assert.Contains(
            session.Project.PatchMatrix.Subscriptions,
            subscription => subscription.ResolvedTxDeviceName == "DEVICE-A-NEW");
        Assert.True(session.CommandDispatcher.CanUndo);
        Assert.Single(session.History);

        Assert.Equal("device.rename", session.CommandDispatcher.Undo());
        Assert.NotNull(session.Project.FindDevice("DEVICE-A"));
        Assert.Null(session.Project.FindDevice("DEVICE-A-NEW"));
        Assert.True(session.History[0].WasUndone);

        Assert.Equal("device.rename", session.CommandDispatcher.Redo());
        Assert.NotNull(session.Project.FindDevice("DEVICE-A-NEW"));
        Assert.False(session.History[0].WasUndone);
    }

    [Fact]
    public void ChannelRangeIsOneTransactionAndOneHistoryEntry()
    {
        ProjectSession session = OpenSession();
        DanteDevice source = session.Project.FindDevice("DEVICE-A")!;
        Dictionary<int, string> names = new()
        {
            [1] = "MIC 01",
            [2] = "MIC 02"
        };

        session.CommandDispatcher.Execute(new RenameChannelRangeCommand(
            source.StableIdentity,
            DanteChannelKind.Tx,
            names));

        Assert.Equal("MIC 01", session.Project.FindDevice("DEVICE-A")!.TxChannels[0].DisplayName);
        Assert.Equal("MIC 02", session.Project.FindDevice("DEVICE-A")!.TxChannels[1].DisplayName);
        Assert.Single(session.History);
        Assert.Equal(2, session.History[0].AffectedElementCount);

        session.CommandDispatcher.Undo();
        Assert.Equal("PROGRAM L", session.Project.FindDevice("DEVICE-A")!.TxChannels[0].DisplayName);
        Assert.Equal("PROGRAM R", session.Project.FindDevice("DEVICE-A")!.TxChannels[1].DisplayName);
    }

    [Fact]
    public void PatchRangeSharesOneAtomicUndoStep()
    {
        ProjectSession session = OpenSession();
        DanteDevice tx = session.Project.FindDevice("DEVICE-A")!;
        DanteDevice rx = session.Project.FindDevice("DEVICE-C")!;

        session.CommandDispatcher.Execute(new AssignSubscriptionRangeCommand(
        [
            new SubscriptionAssignment(rx.StableIdentity, 1, tx.StableIdentity, 2)
        ]));

        DanteSubscription patched = Assert.Single(
            session.Project.PatchMatrix.Subscriptions,
            subscription => subscription.RxDevice == "DEVICE-C");
        Assert.Equal("PROGRAM R", patched.TxChannelName);

        session.CommandDispatcher.Undo();
        DanteSubscription restored = Assert.Single(
            session.Project.PatchMatrix.Subscriptions,
            subscription => subscription.RxDevice == "DEVICE-C");
        Assert.False(restored.IsActive);

        session.CommandDispatcher.Redo();
        DanteSubscription redone = Assert.Single(
            session.Project.PatchMatrix.Subscriptions,
            subscription => subscription.RxDevice == "DEVICE-C");
        Assert.Equal("PROGRAM R", redone.TxChannelName);
    }

    [Fact]
    public void FailedCommandRestoresTheWholeProjectAndDoesNotPolluteHistory()
    {
        ProjectSession session = OpenSession();
        DanteDevice source = session.Project.FindDevice("DEVICE-A")!;

        Assert.Throws<InvalidOperationException>(() =>
            session.CommandDispatcher.Execute(new FailingCommand(source.StableIdentity)));

        Assert.NotNull(session.Project.FindDevice("DEVICE-A"));
        Assert.Null(session.Project.FindDevice("BROKEN-NAME"));
        Assert.Empty(session.History);
        Assert.False(session.CommandDispatcher.CanUndo);
    }

    [Fact]
    public void UndoStackIsBoundedWithoutDiscardingReadableHistory()
    {
        ProjectSession session = OpenSession(undoLimit: 2);
        string stableIdentity = session.Project.FindDevice("DEVICE-A")!.StableIdentity;

        session.CommandDispatcher.Execute(new RenameDeviceCommand(stableIdentity, "DEVICE-A-1"));
        session.CommandDispatcher.Execute(new RenameDeviceCommand(stableIdentity, "DEVICE-A-2"));
        session.CommandDispatcher.Execute(new RenameDeviceCommand(stableIdentity, "DEVICE-A-3"));

        Assert.Equal(3, session.History.Count);
        session.CommandDispatcher.Undo();
        session.CommandDispatcher.Undo();
        Assert.Throws<InvalidOperationException>(() => session.CommandDispatcher.Undo());
        Assert.Equal("DEVICE-A-1", session.Project.FindDeviceByStableIdentity(stableIdentity)!.Name);
    }

    [Fact]
    public void SessionOwnsSelectionFiltersAndValidationState()
    {
        ProjectSession session = OpenSession();
        DanteDevice device = session.Project.FindDevice("DEVICE-B")!;
        ProjectEntityReference selection = new(
            ProjectEntityKind.Device,
            device.StableIdentity,
            device.Name);

        session.SetSelection(new ProjectSelection([selection]));
        session.SetFilter("machines.search", "DEVICE");

        Assert.Equal(selection, session.Selection.Primary);
        Assert.Equal("DEVICE", session.Filters["machines.search"]);
        Assert.True(session.Validation.ValidatedAt > DateTimeOffset.MinValue);
    }

    private static ProjectSession OpenSession(int undoLimit = 30)
    {
        DanteXmlOpenResult opened = new DanteXmlProjectAdapter().Open(RepositoryFile(
            "tests",
            "DanteConfigEditorV3.Tests",
            "Fixtures",
            "representative-preset.xml"));
        ProjectSession session = new(undoLimit);
        session.OpenXml(opened);
        return session;
    }

    private static string RepositoryFile(params string[] relativeParts) =>
        Path.Combine([RepositoryDirectory(), .. relativeParts]);

    private static string RepositoryDirectory()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null
               && !File.Exists(Path.Combine(directory.FullName, "DanteConfigEditorV3.csproj")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);
        return directory!.FullName;
    }

    private sealed class FailingCommand : IProjectCommand
    {
        private readonly string _stableIdentity;

        public FailingCommand(string stableIdentity)
        {
            _stableIdentity = stableIdentity;
        }

        public string Id => "test.fail";

        public string DescriptionKey => "Test.Fail";

        public CommandPreparation Prepare(ProjectSession session)
        {
            DanteDevice device = session.Project.FindDeviceByStableIdentity(_stableIdentity)!;
            return new CommandPreparation(
                Id,
                DescriptionKey,
                [new ProjectEntityReference(ProjectEntityKind.Device, device.StableIdentity, device.Name)],
                [],
                []);
        }

        public void Execute(ProjectSession session)
        {
            DanteDevice device = session.Project.FindDeviceByStableIdentity(_stableIdentity)!;
            session.Project.RenameDevice(device.Name, "BROKEN-NAME");
            throw new InvalidOperationException("Simulated failure");
        }
    }
}
