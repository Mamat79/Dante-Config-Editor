using DanteConfigEditor.Models;
using DanteConfigEditor.Services;
using System.Reflection;

namespace DanteConfigEditorV3.Tests;

public sealed class DanteProjectTests
{
    [Fact]
    public void AssemblyMetadataUsesV36Version()
    {
        string version = typeof(DanteProject).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? string.Empty;

        Assert.StartsWith("2026.1", version, StringComparison.Ordinal);
    }

    [Fact]
    public void RepresentativePresetLoadsWithExpectedTopology()
    {
        using TestWorkspace workspace = new("representative-preset.xml");
        DanteProject project = DanteProject.Load(workspace.SourcePath);

        Assert.Equal(3, project.Devices.Count);
        Assert.Equal(3, project.Devices.Sum(device => device.TxCount));
        Assert.Equal(4, project.Devices.Sum(device => device.RxCount));
        Assert.Contains(project.PatchMatrix.Subscriptions, subscription => subscription.IsLocalSubscription);
        Assert.Contains(project.PatchMatrix.Subscriptions, subscription => subscription.IsActive && subscription.TxDanteId == 1);
        Assert.False(project.ValidateXmlChangeGuard().HasErrors);
    }

    [Fact]
    public void ImportantWarningsExposeAffectedDevices()
    {
        using TestWorkspace workspace = new("representative-preset.xml");
        DanteProject project = DanteProject.Load(workspace.SourcePath);
        IReadOnlyList<DanteImportantWarning> warnings = project.BuildImportantWarningDetails();

        Assert.Contains(warnings, warning => warning.Key == "Warning.StaticIp" && warning.DeviceNames.SequenceEqual(["DEVICE-B"]));
        Assert.Contains(warnings, warning => warning.Key == "Warning.MixedSampleRates" && warning.DeviceNames.SequenceEqual(["DEVICE-B"]));
        Assert.Contains(warnings, warning => warning.Key == "Warning.MixedEncodings" && warning.DeviceNames.SequenceEqual(["DEVICE-B"]));
        Assert.Contains(warnings, warning => warning.Key == "Warning.MixedNetworkModes" && warning.DeviceNames.SequenceEqual(["DEVICE-B"]));
        Assert.Contains(warnings, warning => warning.Key == "Warning.StaticIp" && warning.LocalizedMessage(english: true).Contains("Static IP detected", StringComparison.Ordinal));
    }

    [Fact]
    public void DeviceRenameUpdatesSubscriptionsAndCanBeUndoneAsOneAction()
    {
        using TestWorkspace workspace = new("representative-preset.xml");
        DanteProject project = DanteProject.Load(workspace.SourcePath);
        project.PushUndoSnapshot("rename");

        project.RenameDevice("DEVICE-A", "DEVICE-A-RENAMED");

        Assert.NotNull(project.FindDevice("DEVICE-A-RENAMED"));
        Assert.Contains(project.PatchMatrix.Subscriptions, subscription =>
            subscription.RxDevice == "DEVICE-B" && subscription.ResolvedTxDeviceName == "DEVICE-A-RENAMED");
        Assert.Contains(project.BuildDeviceChangeRows(), row =>
            row.DeviceName == "DEVICE-A-RENAMED" && row.Parameter == "Nom de machine" && row.Before == "DEVICE-A");
        Assert.False(project.ValidateXmlChangeGuard().HasErrors);

        project.UndoLastChange();
        Assert.NotNull(project.FindDevice("DEVICE-A"));
        Assert.False(project.IsModified);
    }

    [Fact]
    public void TxRenameUpdatesEveryRecognizedPatchReference()
    {
        using TestWorkspace workspace = new("representative-preset.xml");
        DanteProject project = DanteProject.Load(workspace.SourcePath);

        project.RenameChannel("DEVICE-A", DanteChannelKind.Tx, 1, "PROGRAM NEW");

        Assert.DoesNotContain(project.PatchMatrix.Subscriptions, subscription => subscription.TxChannelName == "PROGRAM L");
        Assert.True(project.PatchMatrix.Subscriptions.Count(subscription => subscription.TxChannelName == "PROGRAM NEW") >= 2);
        Assert.False(project.ValidateXmlChangeGuard().HasErrors);
    }

    [Fact]
    public void ExclusivePreferredMasterKeepsOnlyTheSelectedDevice()
    {
        using TestWorkspace workspace = new("representative-preset.xml");
        DanteProject project = DanteProject.Load(workspace.SourcePath);

        project.SetExclusivePreferredMaster("DEVICE-B");

        Assert.Equal("DEVICE-B", Assert.Single(project.Devices, device => device.PreferredMaster).Name);
        Assert.False(project.ValidateXmlChangeGuard().HasErrors);
    }

    [Fact]
    public void QuickProfileChangesOnlyAllowedFieldsAndCanBeSaved()
    {
        using TestWorkspace workspace = new("representative-preset.xml");
        DanteProject project = DanteProject.Load(workspace.SourcePath);
        DeviceProfile profile = DeviceProfileCatalog.BuiltIn.Single(item => item.Key == "Profile.48k24b1msAuto");

        int changed = project.ApplyDeviceProfile(["DEVICE-B"], profile);

        DanteDevice device = Assert.Single(project.Devices, item => item.Name == "DEVICE-B");
        Assert.Equal(1, changed);
        Assert.Equal("48000", device.Samplerate);
        Assert.Equal("24", device.Encoding);
        Assert.Equal("1000", device.Latency);
        Assert.False(device.UsesStaticIp);
        Assert.Equal(["DEVICE-B"], project.GetModifiedDeviceNames());
        Assert.False(project.ValidateXmlChangeGuard().HasErrors);

        string outputPath = Path.Combine(workspace.DirectoryPath, "profile-output.xml");
        project.SaveAs(outputPath);
        Assert.False(project.IsModified);
        Assert.Empty(project.GetModifiedDeviceNames());
        DanteProject reloaded = DanteProject.Load(outputPath);
        DanteDevice reloadedDevice = Assert.Single(reloaded.Devices, item => item.Name == "DEVICE-B");
        Assert.Equal("48000", reloadedDevice.Samplerate);
        Assert.False(reloadedDevice.UsesStaticIp);
    }

    [Fact]
    public void DeleteDeviceRemovesSubscriptionsThatReferenceItsTxChannels()
    {
        using TestWorkspace workspace = new("representative-preset.xml");
        DanteProject project = DanteProject.Load(workspace.SourcePath);

        int removed = project.DeleteDevice("DEVICE-A");

        Assert.True(removed >= 2);
        Assert.Null(project.FindDevice("DEVICE-A"));
        Assert.DoesNotContain(project.PatchMatrix.Subscriptions, subscription => subscription.ResolvedTxDeviceName == "DEVICE-A");
        Assert.Contains(project.BuildDeviceChangeRows(), row => row.DeviceName == "DEVICE-A" && row.Status == "Supprimée");
        Assert.False(project.ValidateXmlChangeGuard().HasErrors);
    }

    [Fact]
    public void DeletedDeviceCanBeSavedAndReloadedWithoutDanglingSubscriptions()
    {
        using TestWorkspace workspace = new("representative-preset.xml");
        DanteProject project = DanteProject.Load(workspace.SourcePath);
        string outputPath = Path.Combine(workspace.DirectoryPath, "device-deleted.xml");

        project.DeleteDevice("DEVICE-A");
        project.SaveAs(outputPath);
        DanteProject reloaded = DanteProject.Load(outputPath);

        Assert.Null(reloaded.FindDevice("DEVICE-A"));
        Assert.DoesNotContain(
            reloaded.PatchMatrix.Subscriptions,
            subscription => string.Equals(subscription.ResolvedTxDeviceName, "DEVICE-A", StringComparison.OrdinalIgnoreCase));
        Assert.False(reloaded.Validate().HasErrors);
    }

    [Fact]
    public void MergeImportsUniqueDevicesEvenWhenAnotherNameIsDuplicated()
    {
        using TestWorkspace workspace = new("representative-preset.xml");
        string mergePath = workspace.CopyFixture("merge-preset.xml");
        DanteProject project = DanteProject.Load(workspace.SourcePath);

        DanteMergeResult result = project.MergeDevicesFromXml(mergePath, new Dictionary<string, string>());

        Assert.Equal(1, result.ImportedDeviceCount);
        Assert.Equal(1, result.SkippedDuplicateDeviceCount);
        Assert.NotNull(project.FindDevice("DEVICE-D"));
        Assert.Contains(project.BuildDeviceChangeRows(), row => row.DeviceName == "DEVICE-D" && row.Status == "Ajoutée");
        Assert.False(project.ValidateXmlChangeGuard().HasErrors);
    }

    [Fact]
    public void AutomaticRecoveryRoundTripPreservesUnsavedChanges()
    {
        using TestWorkspace workspace = new("representative-preset.xml");
        string recoveryDirectory = Path.Combine(workspace.DirectoryPath, "recovery");
        DanteProject project = DanteProject.Load(workspace.SourcePath);
        project.SetLatency("DEVICE-C", "5000");

        SessionRecoveryService.Save(project, recoveryDirectory);
        RecoveryCandidate candidate = Assert.IsType<RecoveryCandidate>(SessionRecoveryService.Find(workspace.SourcePath, recoveryDirectory));
        Assert.True(candidate.SourceMatches);

        DanteProject recovered = DanteProject.LoadRecovered(workspace.SourcePath, candidate.RecoveryXmlPath);
        Assert.True(recovered.IsModified);
        Assert.Equal("5000", recovered.FindDevice("DEVICE-C")?.Latency);
        Assert.Contains(recovered.BuildDeviceChangeRows(), row => row.DeviceName == "DEVICE-C" && row.Parameter == "Latence");
        Assert.False(recovered.ValidateXmlChangeGuard().HasErrors);

        SessionRecoveryService.Delete(workspace.SourcePath, recoveryDirectory);
        Assert.Null(SessionRecoveryService.Find(workspace.SourcePath, recoveryDirectory));
    }

    [Fact]
    [Trait("Category", "LocalIntegration")]
    public void OptionalRealXmlCorpusLoadsWithoutChangingTechnicalXml()
    {
        string? root = Environment.GetEnvironmentVariable("DANTE_REAL_XML_ROOT");
        bool required = string.Equals(Environment.GetEnvironmentVariable("DANTE_REAL_XML_REQUIRED"), "1", StringComparison.Ordinal);
        if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
        {
            Assert.False(required, "DANTE_REAL_XML_ROOT doit pointer vers un corpus XML existant pour ce contrôle local.");
            return;
        }

        string[] files = Directory.EnumerateFiles(root, "*.xml", SearchOption.AllDirectories).Take(100).ToArray();
        Assert.NotEmpty(files);
        Console.WriteLine($"Corpus Dante réel contrôlé : {files.Length} fichier(s).");
        foreach (string file in files)
        {
            DanteProject project = DanteProject.Load(file);
            Assert.NotEmpty(project.Devices);
            Assert.False(project.ValidateXmlChangeGuard().HasErrors);
        }
    }

    private sealed class TestWorkspace : IDisposable
    {
        public TestWorkspace(string fixtureName)
        {
            DirectoryPath = Path.Combine(Path.GetTempPath(), "DanteConfigEditorV3.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(DirectoryPath);
            SourcePath = CopyFixture(fixtureName);
        }

        public string DirectoryPath { get; }

        public string SourcePath { get; }

        public string CopyFixture(string fixtureName)
        {
            string source = Path.Combine(AppContext.BaseDirectory, "Fixtures", fixtureName);
            string destination = Path.Combine(DirectoryPath, fixtureName);
            File.Copy(source, destination, true);
            return destination;
        }

        public void Dispose()
        {
            if (Directory.Exists(DirectoryPath))
            {
                Directory.Delete(DirectoryPath, true);
            }
        }
    }
}
