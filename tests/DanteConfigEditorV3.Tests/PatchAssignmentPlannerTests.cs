using DanteConfigEditor.Models;
using DanteConfigEditor.Services;

namespace DanteConfigEditorV3.Tests;

public sealed class PatchAssignmentPlannerTests
{
    [Fact]
    public void MultipleTxChannelsMapToFollowingRxChannelsInXmlOrder()
    {
        PatchSourceDescriptor[] sources =
        [
            Source(1, 1, "TX 1"),
            Source(2, 2, "TX 2"),
            Source(3, 3, "TX 3")
        ];
        PatchTargetDescriptor[] targets =
        [
            Target(10, 1, "RX 10"),
            Target(30, 2, "RX 30"),
            Target(50, 3, "RX 50"),
            Target(70, 4, "RX 70")
        ];

        SequentialPatchPlan plan = PatchAssignmentPlanner.PlanSequential(sources, targets, targets[1]);

        Assert.Equal(3, plan.Assignments.Count);
        Assert.Empty(plan.UnassignedSources);
        Assert.Equal([30, 50, 70], plan.Assignments.Select(item => item.Target.DanteId).ToArray());
        Assert.Equal(["TX 1", "TX 2", "TX 3"], plan.Assignments.Select(item => item.Source.ChannelName).ToArray());
    }

    [Fact]
    public void SourcesBeyondTheLastRxAreReportedAsUnassigned()
    {
        PatchSourceDescriptor[] sources =
        [
            Source(1, 1, "TX 1"),
            Source(2, 2, "TX 2"),
            Source(3, 3, "TX 3")
        ];
        PatchTargetDescriptor[] targets = [Target(1, 1, "RX 1"), Target(2, 2, "RX 2")];

        SequentialPatchPlan plan = PatchAssignmentPlanner.PlanSequential(sources, targets, targets[1]);

        Assert.Single(plan.Assignments);
        Assert.Equal(2, plan.UnassignedSources.Count);
        Assert.Equal("TX 2", plan.UnassignedSources[0].ChannelName);
        Assert.Equal("TX 3", plan.UnassignedSources[1].ChannelName);
    }

    [Fact]
    public void SequentialPlanAppliedAsBatchUsesExistingSafePatchWriter()
    {
        using TestDirectory directory = new();
        DanteProject project = DanteProject.Load(directory.CopyFixture("representative-preset.xml"));
        DanteDevice txDevice = Assert.IsType<DanteDevice>(project.FindDevice("DEVICE-A"));
        DanteDevice rxDevice = Assert.IsType<DanteDevice>(project.FindDevice("DEVICE-B"));
        PatchSourceDescriptor[] sources = txDevice.TxChannels.Select(channel => new PatchSourceDescriptor(
            txDevice.Name,
            channel.DanteId,
            channel.PositionIndex,
            channel.DisplayName)).ToArray();
        PatchTargetDescriptor[] targets = rxDevice.RxChannels.Select(channel => new PatchTargetDescriptor(
            rxDevice.Name,
            channel.DanteId,
            channel.PositionIndex,
            channel.DisplayName)).ToArray();
        SequentialPatchPlan plan = PatchAssignmentPlanner.PlanSequential(sources.Reverse().ToArray(), targets, targets[0]);

        project.ApplyBatch(batch =>
        {
            foreach (PlannedPatchAssignment assignment in plan.Assignments)
            {
                batch.ApplyPatch(
                    assignment.Target.DeviceName,
                    assignment.Target.DanteId,
                    assignment.Source.DeviceName,
                    assignment.Source.ChannelName);
            }
        });

        Assert.False(project.ValidateXmlChangeGuard().HasErrors);
        DanteSubscription[] subscriptions = project.PatchMatrix.Subscriptions
            .Where(subscription => subscription.RxDevice == "DEVICE-B")
            .OrderBy(subscription => subscription.RxDanteId)
            .ToArray();
        Assert.Equal(["PROGRAM R", "PROGRAM L"], subscriptions.Select(item => item.TxChannelName).ToArray());
    }

    [Fact]
    public void PlannerRejectsAStartRxOutsideTheAvailableDeviceTargets()
    {
        PatchSourceDescriptor[] sources = [Source(1, 1, "TX")];
        PatchTargetDescriptor[] targets = [Target(1, 1, "RX")];
        PatchTargetDescriptor foreignTarget = new("OTHER-RX", 1, 1, "RX");

        Assert.Throws<InvalidOperationException>(() => PatchAssignmentPlanner.PlanSequential(sources, targets, foreignTarget));
    }

    [Fact]
    public void PlannerUsesTheKnownXmlPositionForTheStartingRx()
    {
        PatchSourceDescriptor[] sources = [Source(1, 1, "TX")];
        PatchTargetDescriptor[] targets =
        [
            Target(10, 1, "RX 10"),
            Target(30, 2, "RX 30")
        ];
        PatchTargetDescriptor callerCopyWithWrongPosition = new("RX-DEVICE", 30, 0, "RX 30");

        SequentialPatchPlan plan = PatchAssignmentPlanner.PlanSequential(sources, targets, callerCopyWithWrongPosition);

        Assert.Single(plan.Assignments);
        Assert.Equal(30, plan.Assignments[0].Target.DanteId);
    }

    [Fact]
    public void WorkspaceKeepsEditsPendingUntilTheyAreAppliedByTheCaller()
    {
        using TestDirectory directory = new();
        DanteProject project = DanteProject.Load(directory.CopyFixture("representative-preset.xml"));
        DanteDevice txDevice = Assert.IsType<DanteDevice>(project.FindDevice("DEVICE-A"));
        DanteDevice rxDevice = Assert.IsType<DanteDevice>(project.FindDevice("DEVICE-B"));
        PatchWorkspaceSession workspace = new(project.PatchMatrix.Subscriptions);
        PatchSourceDescriptor source = SourceFrom(txDevice.TxChannels[1]);
        PatchTargetDescriptor target = TargetFrom(rxDevice.RxChannels[0]);

        workspace.Assign(new PlannedPatchAssignment(source, target));

        Assert.True(workspace.HasChanges);
        Assert.False(project.IsModified);
        Assert.Equal("PROGRAM R", workspace.GetEffectiveAssignment(target).TxChannelName);
        Assert.Equal(
            "PROGRAM L",
            project.PatchMatrix.Subscriptions.Single(item =>
                string.Equals(item.RxDevice, target.DeviceName, StringComparison.OrdinalIgnoreCase)
                && item.RxDanteId == target.DanteId).TxChannelName);
    }

    [Fact]
    public void WorkspaceRemovesPendingEditWhenAssignmentReturnsToOriginalSource()
    {
        using TestDirectory directory = new();
        DanteProject project = DanteProject.Load(directory.CopyFixture("representative-preset.xml"));
        DanteDevice txDevice = Assert.IsType<DanteDevice>(project.FindDevice("DEVICE-A"));
        DanteDevice rxDevice = Assert.IsType<DanteDevice>(project.FindDevice("DEVICE-B"));
        PatchWorkspaceSession workspace = new(project.PatchMatrix.Subscriptions);
        PatchTargetDescriptor target = TargetFrom(rxDevice.RxChannels[0]);

        workspace.Assign(new PlannedPatchAssignment(SourceFrom(txDevice.TxChannels[1]), target));
        workspace.Assign(new PlannedPatchAssignment(SourceFrom(txDevice.TxChannels[0]), target));

        Assert.False(workspace.HasChanges);
        Assert.Empty(workspace.Edits);
    }

    [Fact]
    public void WorkspaceCanResetAllPendingVisualChanges()
    {
        using TestDirectory directory = new();
        DanteProject project = DanteProject.Load(directory.CopyFixture("representative-preset.xml"));
        DanteDevice rxDevice = Assert.IsType<DanteDevice>(project.FindDevice("DEVICE-B"));
        PatchWorkspaceSession workspace = new(project.PatchMatrix.Subscriptions);
        PatchTargetDescriptor target = TargetFrom(rxDevice.RxChannels[0]);

        workspace.Remove(target);
        Assert.Single(workspace.Edits);

        workspace.Reset();

        Assert.False(workspace.HasChanges);
        Assert.True(workspace.GetEffectiveAssignment(target).IsActive);
    }

    [Fact]
    public void WorkspaceEditsApplyAsOneSafeProjectBatch()
    {
        using TestDirectory directory = new();
        DanteProject project = DanteProject.Load(directory.CopyFixture("representative-preset.xml"));
        DanteDevice txDevice = Assert.IsType<DanteDevice>(project.FindDevice("DEVICE-A"));
        DanteDevice rxDevice = Assert.IsType<DanteDevice>(project.FindDevice("DEVICE-B"));
        PatchWorkspaceSession workspace = new(project.PatchMatrix.Subscriptions);
        PatchTargetDescriptor firstRx = TargetFrom(rxDevice.RxChannels[0]);
        PatchTargetDescriptor secondRx = TargetFrom(rxDevice.RxChannels[1]);

        workspace.Remove(firstRx);
        workspace.Assign(new PlannedPatchAssignment(SourceFrom(txDevice.TxChannels[0]), secondRx));

        project.ApplyBatch(batch =>
        {
            foreach (PatchEditRequest edit in workspace.Edits)
            {
                if (edit.IsRemoval)
                {
                    batch.RemovePatch(edit.RxDeviceName, edit.RxDanteId);
                }
                else
                {
                    batch.ApplyPatch(edit.RxDeviceName, edit.RxDanteId, edit.TxDeviceName!, edit.TxChannelName!);
                }
            }
        });

        Assert.False(project.ValidateXmlChangeGuard().HasErrors);
        DanteSubscription[] subscriptions = project.PatchMatrix.Subscriptions
            .Where(item => string.Equals(item.RxDevice, rxDevice.Name, StringComparison.OrdinalIgnoreCase))
            .OrderBy(item => item.RxPositionIndex)
            .ToArray();
        Assert.False(subscriptions[0].IsActive);
        Assert.Equal("PROGRAM L", subscriptions[1].TxChannelName);
    }

    [Fact]
    public void SelectionWithEqualCountsMapsOneToOneInSelectionOrder()
    {
        PatchSourceDescriptor[] sources = [Source(2, 2, "TX 2"), Source(1, 1, "TX 1")];
        PatchTargetDescriptor[] targets = [Target(20, 2, "RX 20"), Target(10, 1, "RX 10")];

        PatchAssignmentPlan plan = PatchAssignmentPlanner.PlanSelection(sources, targets);

        Assert.Equal(2, plan.Assignments.Count);
        Assert.Equal("TX 2", plan.Assignments[0].Source.ChannelName);
        Assert.Equal(20, plan.Assignments[0].Target.DanteId);
        Assert.Equal("TX 1", plan.Assignments[1].Source.ChannelName);
        Assert.Equal(10, plan.Assignments[1].Target.DanteId);
    }

    [Fact]
    public void OneTxCanFeedSeveralSelectedRxChannels()
    {
        PatchAssignmentPlan plan = PatchAssignmentPlanner.PlanSelection(
            [Source(1, 1, "TX 1")],
            [Target(10, 1, "RX 10"), Target(20, 2, "RX 20"), Target(30, 3, "RX 30")]);

        Assert.Equal(3, plan.Assignments.Count);
        Assert.All(plan.Assignments, assignment => Assert.Equal("TX 1", assignment.Source.ChannelName));
    }

    [Fact]
    public void SeveralTxToOneRxIsBlockedWithoutPlan()
    {
        InvalidOperationException error = Assert.Throws<InvalidOperationException>(() =>
            PatchAssignmentPlanner.PlanSelection(
                [Source(1, 1, "TX 1"), Source(2, 2, "TX 2")],
                [Target(10, 1, "RX 10")]));

        Assert.Contains("Plusieurs TX", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void UnequalMultipleSelectionsAreBlockedWithoutPartialPlan()
    {
        Assert.Throws<InvalidOperationException>(() =>
            PatchAssignmentPlanner.PlanSelection(
                [Source(1, 1, "TX 1"), Source(2, 2, "TX 2")],
                [Target(10, 1, "RX 10"), Target(20, 2, "RX 20"), Target(30, 3, "RX 30")]));
    }

    [Fact]
    public void RangePlanIsStrictAndNeverReturnsAPartialResult()
    {
        PatchSourceDescriptor[] sources = [Source(1, 1, "TX 1"), Source(2, 2, "TX 2")];
        PatchTargetDescriptor[] targets = [Target(10, 1, "RX 10"), Target(20, 2, "RX 20")];

        PatchAssignmentPlan valid = PatchAssignmentPlanner.PlanOneToOne(sources, sources[0], targets, targets[0], 2);
        Assert.Equal(2, valid.Assignments.Count);

        Assert.Throws<InvalidOperationException>(() =>
            PatchAssignmentPlanner.PlanOneToOne(sources, sources[0], targets, targets[0], 3));
    }

    [Fact]
    public void OneToOneCapacityReportsEachSideAndTheStrictMaximum()
    {
        PatchSourceDescriptor[] sources =
        [
            Source(1, 0, "TX 1"),
            Source(2, 1, "TX 2"),
            Source(3, 2, "TX 3"),
            Source(4, 3, "TX 4")
        ];
        PatchTargetDescriptor[] targets =
        [
            Target(10, 0, "RX 10"),
            Target(20, 1, "RX 20"),
            Target(30, 2, "RX 30")
        ];

        PatchRangeCapacity capacity = PatchAssignmentPlanner.GetRangeCapacity(
            sources,
            sources[1],
            targets,
            targets[1]);

        Assert.Equal(3, capacity.TxAvailable);
        Assert.Equal(2, capacity.RxAvailable);
        Assert.Equal(2, capacity.MaximumCount);
    }

    [Fact]
    public void OneToOneRejectsForeignStartChannelsWithoutStagingAPartialPlan()
    {
        PatchSourceDescriptor[] sources = [Source(1, 0, "TX 1")];
        PatchTargetDescriptor[] targets = [Target(10, 0, "RX 10")];

        Assert.Throws<InvalidOperationException>(() =>
            PatchAssignmentPlanner.PlanOneToOne(
                sources,
                new PatchSourceDescriptor("OTHER-TX", 1, 0, "TX 1"),
                targets,
                targets[0],
                1));
        Assert.Throws<InvalidOperationException>(() =>
            PatchAssignmentPlanner.PlanOneToOne(
                sources,
                sources[0],
                targets,
                new PatchTargetDescriptor("OTHER-RX", 10, 0, "RX 10"),
                1));
    }

    [Fact]
    public void DeviceSelectionSwapOnlyChangesOrientationWhenBothDevicesSupportIt()
    {
        PatchDeviceSelectionSwapResult result = PatchDeviceSelectionSwapper.TrySwap(
            "CONSOLE",
            "STAGEBOX",
            ["CONSOLE", "STAGEBOX"],
            ["CONSOLE", "STAGEBOX"]);

        Assert.True(result.Success);
        Assert.Equal("STAGEBOX", result.TxDeviceName);
        Assert.Equal("CONSOLE", result.RxDeviceName);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void DeviceSelectionSwapIsRejectedWithoutChangingEitherSelection()
    {
        PatchDeviceSelectionSwapResult result = PatchDeviceSelectionSwapper.TrySwap(
            "TX-ONLY",
            "RX-ONLY",
            ["TX-ONLY"],
            ["RX-ONLY"]);

        Assert.False(result.Success);
        Assert.Null(result.TxDeviceName);
        Assert.Null(result.RxDeviceName);
        Assert.Contains("Inversion impossible", result.ErrorMessage, StringComparison.Ordinal);
    }

    [Fact]
    public void DeviceSelectionSwapRejectsTheSameDeviceOnBothSides()
    {
        PatchDeviceSelectionSwapResult result = PatchDeviceSelectionSwapper.TrySwap(
            "CONSOLE",
            "CONSOLE",
            ["CONSOLE", "STAGEBOX"],
            ["CONSOLE", "STAGEBOX"]);

        Assert.False(result.Success);
        Assert.Contains("deux machines différentes", result.ErrorMessage, StringComparison.Ordinal);
    }

    [Fact]
    public void InitialPatchSelectionUsesADistinctReversiblePairWhenAvailable()
    {
        PatchDeviceSelectionPair pair = PatchDeviceSelectionSwapper.ResolveInitialPair(
            preferredTxDevice: "CONSOLE",
            preferredRxDevice: null,
            txCapableDevices: ["CONSOLE", "STAGEBOX", "TX-ONLY"],
            rxCapableDevices: ["CONSOLE", "STAGEBOX", "RX-ONLY"]);

        Assert.Equal("CONSOLE", pair.TxDeviceName);
        Assert.Equal("STAGEBOX", pair.RxDeviceName);
        Assert.True(PatchDeviceSelectionSwapper.TrySwap(
            pair.TxDeviceName,
            pair.RxDeviceName,
            ["CONSOLE", "STAGEBOX", "TX-ONLY"],
            ["CONSOLE", "STAGEBOX", "RX-ONLY"]).Success);
    }

    [Fact]
    public void InitialPatchSelectionPreservesAnExplicitSameDevicePair()
    {
        PatchDeviceSelectionPair pair = PatchDeviceSelectionSwapper.ResolveInitialPair(
            preferredTxDevice: "CONSOLE",
            preferredRxDevice: "CONSOLE",
            txCapableDevices: ["CONSOLE", "STAGEBOX"],
            rxCapableDevices: ["CONSOLE", "STAGEBOX"]);

        Assert.Equal("CONSOLE", pair.TxDeviceName);
        Assert.Equal("CONSOLE", pair.RxDeviceName);
    }

    [Fact]
    public void DeviceSelectionSwapRespectsALockedRxSelector()
    {
        PatchDeviceSelectionSwapResult result = PatchDeviceSelectionSwapper.TrySwap(
            "DEVICE-A",
            "DEVICE-B",
            ["DEVICE-A", "DEVICE-B"],
            ["DEVICE-A", "DEVICE-B"],
            rxSelectionLocked: true);

        Assert.False(result.Success);
        Assert.Contains("verrouillée", result.ErrorMessage, StringComparison.Ordinal);
    }

    [Fact]
    public void DeviceSelectionSwapDoesNotAlterPendingPatchChanges()
    {
        using TestDirectory directory = new();
        DanteProject project = DanteProject.Load(directory.CopyFixture("representative-preset.xml"));
        DanteDevice txDevice = Assert.IsType<DanteDevice>(project.FindDevice("DEVICE-A"));
        DanteDevice rxDevice = Assert.IsType<DanteDevice>(project.FindDevice("DEVICE-B"));
        PatchWorkspaceSession workspace = new(project.PatchMatrix.Subscriptions);
        PatchTargetDescriptor target = TargetFrom(rxDevice.RxChannels[0]);
        workspace.Assign(new PlannedPatchAssignment(SourceFrom(txDevice.TxChannels[1]), target));
        PatchEditRequest before = Assert.Single(workspace.Edits);

        PatchDeviceSelectionSwapResult result = PatchDeviceSelectionSwapper.TrySwap(
            txDevice.Name,
            rxDevice.Name,
            [txDevice.Name, rxDevice.Name],
            [txDevice.Name, rxDevice.Name]);

        Assert.True(result.Success);
        Assert.Equal(before, Assert.Single(workspace.Edits));
        Assert.False(project.IsModified);
    }

    [Fact]
    public void HorizontalMatrixGestureBuildsASequentialTxRxRange()
    {
        PatchSourceDescriptor[] sources =
        [
            Source(1, 0, "TX 1"),
            Source(2, 1, "TX 2"),
            Source(3, 2, "TX 3")
        ];
        PatchTargetDescriptor[] targets =
        [
            Target(10, 0, "RX 10"),
            Target(20, 1, "RX 20"),
            Target(30, 2, "RX 30")
        ];

        PatchAssignmentPlan plan = PatchAssignmentPlanner.PlanMatrixGesture(
            sources,
            targets,
            startSourceIndex: 0,
            startTargetIndex: 0,
            endSourceIndex: 2,
            endTargetIndex: 0);

        Assert.Equal([1, 2, 3], plan.Assignments.Select(item => item.Source.DanteId).ToArray());
        Assert.Equal([10, 20, 30], plan.Assignments.Select(item => item.Target.DanteId).ToArray());
    }

    [Fact]
    public void VerticalMatrixGestureBroadcastsOneTxToSeveralRxChannels()
    {
        PatchSourceDescriptor[] sources = [Source(1, 0, "TX 1"), Source(2, 1, "TX 2")];
        PatchTargetDescriptor[] targets =
        [
            Target(10, 0, "RX 10"),
            Target(20, 1, "RX 20"),
            Target(30, 2, "RX 30")
        ];

        PatchAssignmentPlan plan = PatchAssignmentPlanner.PlanMatrixGesture(
            sources,
            targets,
            startSourceIndex: 1,
            startTargetIndex: 0,
            endSourceIndex: 1,
            endTargetIndex: 2);

        Assert.Equal(3, plan.Assignments.Count);
        Assert.All(plan.Assignments, item => Assert.Equal(2, item.Source.DanteId));
        Assert.Equal([10, 20, 30], plan.Assignments.Select(item => item.Target.DanteId).ToArray());
    }

    [Fact]
    public void DiagonalMatrixGestureBuildsOneToOneAssignments()
    {
        PatchSourceDescriptor[] sources =
        [
            Source(1, 0, "TX 1"),
            Source(2, 1, "TX 2"),
            Source(3, 2, "TX 3")
        ];
        PatchTargetDescriptor[] targets =
        [
            Target(10, 0, "RX 10"),
            Target(20, 1, "RX 20"),
            Target(30, 2, "RX 30")
        ];

        PatchAssignmentPlan plan = PatchAssignmentPlanner.PlanMatrixGesture(
            sources,
            targets,
            startSourceIndex: 0,
            startTargetIndex: 0,
            endSourceIndex: 2,
            endTargetIndex: 2);

        Assert.Equal(3, plan.Assignments.Count);
        Assert.Equal([1, 2, 3], plan.Assignments.Select(item => item.Source.DanteId).ToArray());
        Assert.Equal([10, 20, 30], plan.Assignments.Select(item => item.Target.DanteId).ToArray());
    }

    [Fact]
    public void MatrixGestureRejectsAnAmbiguousDiagonalWithoutPartialPlan()
    {
        PatchSourceDescriptor[] sources =
        [
            Source(1, 0, "TX 1"),
            Source(2, 1, "TX 2"),
            Source(3, 2, "TX 3")
        ];
        PatchTargetDescriptor[] targets =
        [
            Target(10, 0, "RX 10"),
            Target(20, 1, "RX 20"),
            Target(30, 2, "RX 30")
        ];

        Assert.Throws<InvalidOperationException>(() => PatchAssignmentPlanner.PlanMatrixGesture(
            sources,
            targets,
            startSourceIndex: 0,
            startTargetIndex: 0,
            endSourceIndex: 2,
            endTargetIndex: 1));
    }

    [Fact]
    public void PreviewClassifiesCreateReplaceAndUnchangedBeforeStaging()
    {
        using TestDirectory directory = new();
        DanteProject project = DanteProject.Load(directory.CopyFixture("representative-preset.xml"));
        DanteDevice txDevice = Assert.IsType<DanteDevice>(project.FindDevice("DEVICE-A"));
        DanteDevice rxB = Assert.IsType<DanteDevice>(project.FindDevice("DEVICE-B"));
        DanteDevice rxC = Assert.IsType<DanteDevice>(project.FindDevice("DEVICE-C"));
        PatchWorkspaceSession workspace = new(project.PatchMatrix.Subscriptions);

        PatchBatchPreview preview = workspace.BuildPreview(
        [
            new PlannedPatchAssignment(SourceFrom(txDevice.TxChannels[0]), TargetFrom(rxB.RxChannels[0])),
            new PlannedPatchAssignment(SourceFrom(txDevice.TxChannels[0]), TargetFrom(rxB.RxChannels[1])),
            new PlannedPatchAssignment(SourceFrom(txDevice.TxChannels[0]), TargetFrom(rxC.RxChannels[0]))
        ]);

        Assert.Equal(1, preview.UnchangedCount);
        Assert.Equal(1, preview.ReplaceCount);
        Assert.Equal(1, preview.CreateCount);
        Assert.False(workspace.HasChanges);
    }

    [Fact]
    public void CommittedPreviewStillDetectsAConflictHiddenByAPendingRemoval()
    {
        using TestDirectory directory = new();
        DanteProject project = DanteProject.Load(directory.CopyFixture("representative-preset.xml"));
        DanteDevice txDevice = Assert.IsType<DanteDevice>(project.FindDevice("DEVICE-A"));
        DanteDevice rxDevice = Assert.IsType<DanteDevice>(project.FindDevice("DEVICE-B"));
        PatchWorkspaceSession workspace = new(project.PatchMatrix.Subscriptions);
        PatchTargetDescriptor target = TargetFrom(rxDevice.RxChannels[0]);
        PlannedPatchAssignment replacement = new(
            SourceFrom(txDevice.TxChannels[1]),
            target);

        workspace.Remove(target);

        Assert.Equal(1, workspace.BuildPreview([replacement]).CreateCount);
        Assert.Equal(1, workspace.BuildCommittedPreview([replacement]).ReplaceCount);
    }

    [Theory]
    [InlineData(PatchConflictResolution.Cancel, 0, 0, true)]
    [InlineData(PatchConflictResolution.Skip, 1, 1, false)]
    [InlineData(PatchConflictResolution.Replace, 2, 0, false)]
    public void ConflictResolutionIsExplicitAndAtomic(
        PatchConflictResolution resolution,
        int expectedStaged,
        int expectedSkipped,
        bool expectedCancelled)
    {
        using TestDirectory directory = new();
        DanteProject project = DanteProject.Load(directory.CopyFixture("representative-preset.xml"));
        DanteDevice txDevice = Assert.IsType<DanteDevice>(project.FindDevice("DEVICE-A"));
        DanteDevice rxB = Assert.IsType<DanteDevice>(project.FindDevice("DEVICE-B"));
        DanteDevice rxC = Assert.IsType<DanteDevice>(project.FindDevice("DEVICE-C"));
        PatchWorkspaceSession workspace = new(project.PatchMatrix.Subscriptions);
        PatchBatchPreview preview = workspace.BuildPreview(
        [
            new PlannedPatchAssignment(SourceFrom(txDevice.TxChannels[1]), TargetFrom(rxB.RxChannels[0])),
            new PlannedPatchAssignment(SourceFrom(txDevice.TxChannels[1]), TargetFrom(rxC.RxChannels[0]))
        ]);

        PatchStageResult result = workspace.StagePreview(preview, resolution);

        Assert.Equal(expectedStaged, result.StagedCount);
        Assert.Equal(expectedSkipped, result.SkippedConflictCount);
        Assert.Equal(expectedCancelled, result.IsCancelled);
        Assert.Equal(expectedStaged, workspace.PendingCount);
    }

    [Fact]
    public void SeveralPreviewsAccumulateWithoutChangingTheProjectBeforeFinalApply()
    {
        using TestDirectory directory = new();
        DanteProject project = DanteProject.Load(directory.CopyFixture("representative-preset.xml"));
        DanteDevice txDevice = Assert.IsType<DanteDevice>(project.FindDevice("DEVICE-A"));
        DanteDevice freeRxDevice = Assert.IsType<DanteDevice>(project.FindDevice("DEVICE-C"));
        DanteDevice patchedRxDevice = Assert.IsType<DanteDevice>(project.FindDevice("DEVICE-B"));
        PatchWorkspaceSession workspace = new(project.PatchMatrix.Subscriptions);
        PatchTargetDescriptor firstTarget = TargetFrom(freeRxDevice.RxChannels[0]);
        PatchTargetDescriptor secondTarget = TargetFrom(patchedRxDevice.RxChannels[1]);

        workspace.StagePreview(
            workspace.BuildPreview([new PlannedPatchAssignment(SourceFrom(txDevice.TxChannels[0]), firstTarget)]),
            PatchConflictResolution.Replace);
        workspace.StagePreview(
            workspace.BuildPreview([new PlannedPatchAssignment(SourceFrom(txDevice.TxChannels[0]), secondTarget)]),
            PatchConflictResolution.Replace);

        Assert.Equal(2, workspace.PendingCount);
        Assert.Equal(2, workspace.PendingChanges.Count);
        Assert.False(project.IsModified);
        Assert.False(project.PatchMatrix.Subscriptions.Single(item => item.RxDevice == freeRxDevice.Name).IsActive);
        Assert.Equal(
            "PROGRAM R",
            project.PatchMatrix.Subscriptions.Single(item =>
                item.RxDevice == patchedRxDevice.Name && item.RxDanteId == secondTarget.DanteId).TxChannelName);

        ApplyEdits(project, workspace.Edits);

        Assert.True(project.IsModified);
        Assert.Equal(
            "PROGRAM L",
            project.PatchMatrix.Subscriptions.Single(item => item.RxDevice == freeRxDevice.Name).TxChannelName);
        Assert.Equal(
            "PROGRAM L",
            project.PatchMatrix.Subscriptions.Single(item =>
                item.RxDevice == patchedRxDevice.Name && item.RxDanteId == secondTarget.DanteId).TxChannelName);
    }

    [Fact]
    public void InvalidStalePreviewLeavesExistingPendingEditsIntact()
    {
        using TestDirectory directory = new();
        DanteProject project = DanteProject.Load(directory.CopyFixture("representative-preset.xml"));
        DanteDevice rxDevice = Assert.IsType<DanteDevice>(project.FindDevice("DEVICE-B"));
        PatchWorkspaceSession workspace = new(project.PatchMatrix.Subscriptions);
        PatchTargetDescriptor existingTarget = TargetFrom(rxDevice.RxChannels[0]);
        workspace.Remove(existingTarget);
        PatchBatchPreview invalidPreview = new(
        [
            new PatchPreviewItem(
                new PlannedPatchAssignment(Source(1, 1, "TX"), new PatchTargetDescriptor("UNKNOWN", 999, 0, "RX")),
                new EffectivePatchAssignment("UNKNOWN", 999, null, null, false),
                PatchPreviewAction.Create)
        ]);

        Assert.Throws<InvalidOperationException>(() =>
            workspace.StagePreview(invalidPreview, PatchConflictResolution.Replace));
        PatchEditRequest remaining = Assert.Single(workspace.Edits);
        Assert.Equal(existingTarget.DanteId, remaining.RxDanteId);
        Assert.True(remaining.IsRemoval);
    }

    [Fact]
    public void SeveralSelectedRxChannelsCanBeDisconnectedTogether()
    {
        using TestDirectory directory = new();
        DanteProject project = DanteProject.Load(directory.CopyFixture("representative-preset.xml"));
        DanteDevice rxDevice = Assert.IsType<DanteDevice>(project.FindDevice("DEVICE-B"));
        PatchWorkspaceSession workspace = new(project.PatchMatrix.Subscriptions);

        int removed = workspace.RemoveMany(rxDevice.RxChannels.Select(TargetFrom));

        Assert.Equal(2, removed);
        Assert.Equal(2, workspace.PendingCount);
        Assert.All(workspace.Edits, edit => Assert.True(edit.IsRemoval));
    }

    [Fact]
    public void PendingEditsCanBeReopenedAndPersistedAfterSave()
    {
        using TestDirectory directory = new();
        string sourcePath = directory.CopyFixture("representative-preset.xml");
        DanteProject project = DanteProject.Load(sourcePath);
        DanteDevice txDevice = Assert.IsType<DanteDevice>(project.FindDevice("DEVICE-A"));
        DanteDevice rxDevice = Assert.IsType<DanteDevice>(project.FindDevice("DEVICE-C"));
        PatchTargetDescriptor target = TargetFrom(rxDevice.RxChannels[0]);
        PatchEditRequest pending = new(
            target.DeviceName,
            target.DanteId,
            txDevice.Name,
            txDevice.TxChannels[1].DisplayName);
        PatchWorkspaceSession reopened = new(project.PatchMatrix.Subscriptions, [pending]);

        Assert.Equal(pending, Assert.Single(reopened.Edits));
        ApplyEdits(project, reopened.Edits);
        string destination = Path.Combine(directory.Root, "patched.xml");
        project.SaveAs(destination);

        DanteProject reloaded = DanteProject.Load(destination);
        DanteSubscription saved = Assert.Single(
            reloaded.PatchMatrix.Subscriptions,
            subscription => subscription.RxDevice == target.DeviceName && subscription.RxDanteId == target.DanteId);
        Assert.Equal("DEVICE-A", saved.ResolvedTxDeviceName);
        Assert.Equal("PROGRAM R", saved.TxChannelName);
        Assert.False(reloaded.ValidateXmlChangeGuard().HasErrors);
    }

    [Fact]
    public void ReopenedPendingEditsCanBeResetToAnEmptyResult()
    {
        using TestDirectory directory = new();
        DanteProject project = DanteProject.Load(directory.CopyFixture("representative-preset.xml"));
        PatchEditRequest pending = new("DEVICE-C", 1, "DEVICE-A", "PROGRAM L");
        PatchWorkspaceSession reopened = new(project.PatchMatrix.Subscriptions, [pending]);

        Assert.Single(reopened.Edits);
        reopened.Reset();

        Assert.False(reopened.HasChanges);
        Assert.Empty(reopened.Edits);
    }

    [Fact]
    public void DeviceDetailBatchPatchesBeforeRenamesAndUndoesAsOneAction()
    {
        using TestDirectory directory = new();
        DanteProject project = DanteProject.Load(directory.CopyFixture("representative-preset.xml"));
        DanteChannel sourceChannel = Assert.IsType<DanteDevice>(project.FindDevice("DEVICE-A")).TxChannels[0];
        project.PushUndoSnapshot("device details");

        project.ApplyBatch(batch =>
        {
            batch.ApplyPatch("DEVICE-C", 1, "DEVICE-A", sourceChannel.DisplayName);
            batch.RenameDevice("DEVICE-A", "DEVICE-A-RENAMED");
            batch.RenameChannel("DEVICE-A-RENAMED", DanteChannelKind.Tx, sourceChannel.Index, "PROGRAM NEW");
        });

        DanteSubscription changed = Assert.Single(
            project.PatchMatrix.Subscriptions,
            subscription => subscription.RxDevice == "DEVICE-C" && subscription.RxDanteId == 1);
        Assert.Equal("DEVICE-A-RENAMED", changed.ResolvedTxDeviceName);
        Assert.Equal("PROGRAM NEW", changed.TxChannelName);
        Assert.False(project.ValidateXmlChangeGuard().HasErrors);

        project.UndoLastChange();

        DanteSubscription restored = Assert.Single(
            project.PatchMatrix.Subscriptions,
            subscription => subscription.RxDevice == "DEVICE-C" && subscription.RxDanteId == 1);
        Assert.False(restored.IsActive);
        Assert.NotNull(project.FindDevice("DEVICE-A"));
        Assert.Null(project.FindDevice("DEVICE-A-RENAMED"));
    }

    private static void ApplyEdits(DanteProject project, IEnumerable<PatchEditRequest> edits)
    {
        project.ApplyBatch(batch =>
        {
            foreach (PatchEditRequest edit in edits)
            {
                if (edit.IsRemoval)
                {
                    batch.RemovePatch(edit.RxDeviceName, edit.RxDanteId);
                }
                else
                {
                    batch.ApplyPatch(edit.RxDeviceName, edit.RxDanteId, edit.TxDeviceName!, edit.TxChannelName!);
                }
            }
        });
    }

    private static PatchSourceDescriptor Source(int danteId, int position, string name)
    {
        return new PatchSourceDescriptor("TX-DEVICE", danteId, position, name);
    }

    private static PatchTargetDescriptor Target(int danteId, int position, string name)
    {
        return new PatchTargetDescriptor("RX-DEVICE", danteId, position, name);
    }

    private static PatchSourceDescriptor SourceFrom(DanteChannel channel)
    {
        return new PatchSourceDescriptor(channel.DeviceName, channel.DanteId, channel.PositionIndex, channel.DisplayName);
    }

    private static PatchTargetDescriptor TargetFrom(DanteChannel channel)
    {
        return new PatchTargetDescriptor(channel.DeviceName, channel.DanteId, channel.PositionIndex, channel.DisplayName);
    }

    private sealed class TestDirectory : IDisposable
    {
        public TestDirectory()
        {
            Root = Path.Combine(Path.GetTempPath(), "DanteConfigEditorV3.PatchPlannerTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Root);
        }

        public string Root { get; }

        public string CopyFixture(string name)
        {
            string destination = Path.Combine(Root, name);
            File.Copy(Path.Combine(AppContext.BaseDirectory, "Fixtures", name), destination, true);
            return destination;
        }

        public void Dispose()
        {
            if (Directory.Exists(Root))
            {
                Directory.Delete(Root, true);
            }
        }
    }
}
