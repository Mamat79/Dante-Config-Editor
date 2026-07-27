using DanteConfigEditor.Models;
using DanteConfigEditorV3.TestSupport;

namespace DanteConfigEditorV3.Tests;

public sealed class DanteProjectPerformanceRegressionTests
{
    [Fact]
    public void DeviceAndTxRangeRenameInSameBatchUsesCurrentDeviceIdentity()
    {
        using TemporaryDirectory directory = new();
        string source = Path.Combine(directory.Path, "synthetic-10.xml");
        SyntheticPresetFactory.Create(source, deviceCount: 10);
        DanteProject project = DanteProject.Load(source);

        project.ApplyBatch(batch =>
        {
            batch.RenameDevice("DEVICE-001", "DEVICE-001-EDITED");
            batch.BatchRenameChannels(
                "DEVICE-001-EDITED",
                DanteChannelKind.Tx,
                "PROGRAM",
                firstNumber: 1,
                startChannelIndex: 1,
                endChannelIndex: 64);
        });

        DanteSubscription[] affected = project.PatchMatrix.Subscriptions
            .Where(subscription => string.Equals(
                subscription.ResolvedTxDeviceName,
                "DEVICE-001-EDITED",
                StringComparison.OrdinalIgnoreCase))
            .ToArray();

        Assert.Equal(128, affected.Length);
        Assert.All(
            affected,
            subscription => Assert.StartsWith(
                "PROGRAM ",
                subscription.TxChannelName,
                StringComparison.Ordinal));
        Assert.False(project.ValidateXmlChangeGuard().HasErrors);
    }

    [Fact]
    public void GroupedTxRenamesUpdateSubscriptionsWithBoundedAllocations()
    {
        using TemporaryDirectory directory = new();
        string source = Path.Combine(directory.Path, "synthetic-200.xml");
        SyntheticPresetFactory.Create(source, deviceCount: 200);
        DanteProject project = DanteProject.Load(source);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();

        project.ApplyBatch(batch =>
        {
            batch.RenameDevice("DEVICE-001", "DEVICE-001-EDITED");
            for (int channel = 1; channel <= 64; channel++)
            {
                batch.RenameChannel(
                    "DEVICE-001-EDITED",
                    DanteChannelKind.Tx,
                    channel,
                    $"EDIT-TX-{channel:D2}");
            }
        });

        long allocatedBytes =
            GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        double allocatedMiB = allocatedBytes / 1024d / 1024d;

        DanteSubscription[] affected = project.PatchMatrix.Subscriptions
            .Where(subscription => string.Equals(
                subscription.ResolvedTxDeviceName,
                "DEVICE-001-EDITED",
                StringComparison.OrdinalIgnoreCase))
            .ToArray();

        Assert.Equal(128, affected.Length);
        Assert.All(
            affected,
            subscription => Assert.StartsWith(
                "EDIT-TX-",
                subscription.TxChannelName,
                StringComparison.Ordinal));
        Assert.DoesNotContain(
            project.PatchMatrix.Subscriptions,
            subscription => subscription.IsActive
                && string.Equals(
                    subscription.ResolvedTxDeviceName,
                    "DEVICE-001-EDITED",
                    StringComparison.OrdinalIgnoreCase)
                && subscription.TxChannelName.StartsWith(
                    "TX-",
                    StringComparison.Ordinal));
        Assert.True(
            allocatedMiB < 150,
            $"Le lot de renommage a alloué {allocatedMiB:F1} Mio, au-delà de la limite de 150 Mio.");
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "DanteConfigEditorV3.PerformanceRegression",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
