using DanteConfigEditor.Application;
using DanteConfigEditor.Application.Commands;
using DanteConfigEditor.Application.Patch;
using DanteConfigEditor.DanteXml;
using DanteConfigEditor.Models;
using DanteConfigEditor.Services;

namespace DanteConfigEditorV3.Tests;

public sealed class UnifiedPatchSessionTests
{
    [Fact]
    public void TwoViewsObserveTheSamePendingPatchBatch()
    {
        ProjectSession projectSession = OpenSession();
        UnifiedPatchSession patch = new(projectSession.Project);
        IPatchWorkspaceSession matrixView = patch;
        IPatchWorkspaceSession listView = patch;
        DanteDevice tx = projectSession.Project.FindDevice("DEVICE-A")!;
        DanteDevice rx = projectSession.Project.FindDevice("DEVICE-C")!;
        PlannedPatchAssignment assignment = new(
            Source(tx, tx.TxChannels[1]),
            Target(rx, rx.RxChannels[0]));

        matrixView.StagePreview(
            matrixView.BuildPreview([assignment]),
            PatchConflictResolution.Replace);

        Assert.Single(listView.PendingChanges);
        Assert.Equal("PROGRAM R", listView.PendingChanges[0].DesiredTxChannelName);
        Assert.False(projectSession.Project.IsModified);
    }

    [Fact]
    public void PendingPatchSurvivesDeviceAndChannelRenamesThroughStableIdentities()
    {
        ProjectSession projectSession = OpenSession();
        UnifiedPatchSession patch = new(projectSession.Project);
        DanteDevice tx = projectSession.Project.FindDevice("DEVICE-A")!;
        DanteDevice rx = projectSession.Project.FindDevice("DEVICE-C")!;
        patch.StagePreview(
            patch.BuildPreview([
                new PlannedPatchAssignment(
                    Source(tx, tx.TxChannels[1]),
                    Target(rx, rx.RxChannels[0]))
            ]),
            PatchConflictResolution.Replace);

        projectSession.Project.RenameDevice("DEVICE-A", "SOURCE-RENAMED");
        projectSession.Project.RenameChannel(
            "SOURCE-RENAMED",
            DanteChannelKind.Tx,
            2,
            "RIGHT-RENAMED");
        PatchRebaseResult result = patch.Rebase(projectSession.Project);

        PatchEditRequest edit = Assert.Single(patch.Edits);
        Assert.Equal(1, result.PreservedCount);
        Assert.Equal(0, result.DiscardedCount);
        Assert.Equal("SOURCE-RENAMED", edit.TxDeviceName);
        Assert.Equal("RIGHT-RENAMED", edit.TxChannelName);
        Assert.Equal(2, edit.TxDanteId);
    }

    [Fact]
    public void CommitUsesOneAtomicCommandAndSupportsUndoRedo()
    {
        ProjectSession projectSession = OpenSession();
        UnifiedPatchSession patch = new(projectSession.Project);
        DanteDevice tx = projectSession.Project.FindDevice("DEVICE-A")!;
        DanteDevice rxB = projectSession.Project.FindDevice("DEVICE-B")!;
        DanteDevice rxC = projectSession.Project.FindDevice("DEVICE-C")!;

        patch.StageEdits([
            PatchEditRequest.Remove(Target(rxB, rxB.RxChannels[0])),
            PatchEditRequest.Apply(new PlannedPatchAssignment(
                Source(tx, tx.TxChannels[1]),
                Target(rxC, rxC.RxChannels[0])))
        ]);
        CommandExecutionResult result = patch.Commit(projectSession);

        Assert.Equal("patch.apply-batch", result.CommandId);
        Assert.Single(projectSession.History);
        Assert.False(patch.HasChanges);
        Assert.False(projectSession.Project.PatchMatrix.Subscriptions.Single(
            item => item.RxDevice == "DEVICE-B" && item.RxDanteId == 1).IsActive);
        Assert.Equal("PROGRAM R", projectSession.Project.PatchMatrix.Subscriptions.Single(
            item => item.RxDevice == "DEVICE-C" && item.RxDanteId == 1).TxChannelName);

        projectSession.CommandDispatcher.Undo();
        Assert.True(projectSession.Project.PatchMatrix.Subscriptions.Single(
            item => item.RxDevice == "DEVICE-B" && item.RxDanteId == 1).IsActive);
        Assert.False(projectSession.Project.PatchMatrix.Subscriptions.Single(
            item => item.RxDevice == "DEVICE-C" && item.RxDanteId == 1).IsActive);

        projectSession.CommandDispatcher.Redo();
        Assert.False(projectSession.Project.PatchMatrix.Subscriptions.Single(
            item => item.RxDevice == "DEVICE-B" && item.RxDanteId == 1).IsActive);
        Assert.Equal("PROGRAM R", projectSession.Project.PatchMatrix.Subscriptions.Single(
            item => item.RxDevice == "DEVICE-C" && item.RxDanteId == 1).TxChannelName);
    }

    [Fact]
    public void RebaseReportsADeletedPendingTargetInsteadOfDroppingItSilently()
    {
        ProjectSession projectSession = OpenSession();
        UnifiedPatchSession patch = new(projectSession.Project);
        DanteDevice tx = projectSession.Project.FindDevice("DEVICE-A")!;
        DanteDevice rx = projectSession.Project.FindDevice("DEVICE-C")!;
        patch.StageEdits([
            PatchEditRequest.Apply(new PlannedPatchAssignment(
                Source(tx, tx.TxChannels[0]),
                Target(rx, rx.RxChannels[0])))
        ]);

        projectSession.Project.DeleteDevice("DEVICE-C");
        PatchRebaseResult result = patch.Rebase(projectSession.Project);

        Assert.Equal(0, result.PreservedCount);
        Assert.Equal(1, result.DiscardedCount);
        Assert.Single(result.Warnings);
        Assert.False(patch.HasChanges);
    }

    private static ProjectSession OpenSession()
    {
        DanteXmlOpenResult opened = new DanteXmlProjectAdapter().Open(RepositoryFile(
            "tests",
            "DanteConfigEditorV3.Tests",
            "Fixtures",
            "representative-preset.xml"));
        ProjectSession session = new();
        session.OpenXml(opened);
        return session;
    }

    private static PatchSourceDescriptor Source(DanteDevice device, DanteChannel channel) =>
        new(device.Name, channel.DanteId, channel.PositionIndex, channel.DisplayName);

    private static PatchTargetDescriptor Target(DanteDevice device, DanteChannel channel) =>
        new(device.Name, channel.DanteId, channel.PositionIndex, channel.DisplayName);

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
}
