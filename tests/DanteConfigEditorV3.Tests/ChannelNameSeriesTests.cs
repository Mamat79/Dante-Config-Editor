using DanteConfigEditor.Models;
using DanteConfigEditor.Services;
using DanteConfigEditorV3.TestSupport;

namespace DanteConfigEditorV3.Tests;

public sealed class ChannelNameSeriesTests
{
    [Theory]
    [InlineData("Mic 4", true)]
    [InlineData("Mic 04", true)]
    [InlineData("Mic 12", true)]
    [InlineData("HF 12", true)]
    [InlineData("Ambiance3", true)]
    [InlineData("Orchestre 007", true)]
    [InlineData("Retour A 4", true)]
    [InlineData("Mic", false)]
    [InlineData("Mic gauche", false)]
    [InlineData("Mic 4 principal", false)]
    public void SeriesHandleIsAvailableOnlyForNamesEndingWithANumber(string name, bool expected)
    {
        Assert.Equal(expected, ChannelNameSeriesService.CanExtend(name));
    }

    [Fact]
    public void ExtendsExcelStyleSeriesWithoutAddingLeadingZeroes()
    {
        ChannelSeriesValue[] channels = Enumerable.Range(1, 5)
            .Select(index => new ChannelSeriesValue(index, index <= 2 ? $"Mic {index}" : index.ToString()))
            .ToArray();

        IReadOnlyList<ChannelSeriesValue> result = ChannelNameSeriesService.Extend(channels, [1, 2], 5);

        Assert.Equal(["Mic 3", "Mic 4", "Mic 5"], result.Select(item => item.Name));
    }

    [Fact]
    public void ExtendsASingleNumberedSeedWithAUnitStep()
    {
        ChannelSeriesValue[] channels = Enumerable.Range(1, 5)
            .Select(index => new ChannelSeriesValue(index, index == 1 ? "Micro 1" : index.ToString()))
            .ToArray();

        IReadOnlyList<ChannelSeriesValue> result = ChannelNameSeriesService.Extend(channels, [1], 5);

        Assert.Equal(["Micro 2", "Micro 3", "Micro 4", "Micro 5"], result.Select(item => item.Name));
    }

    [Fact]
    public void PreservesExplicitNumericPaddingAndStep()
    {
        ChannelSeriesValue[] channels =
        [
            new(1, "HF 01"),
            new(2, "HF 03"),
            new(3, "3"),
            new(4, "4")
        ];

        IReadOnlyList<ChannelSeriesValue> result = ChannelNameSeriesService.Extend(channels, [1, 2], 4);

        Assert.Equal(["HF 05", "HF 07"], result.Select(item => item.Name));
    }

    [Fact]
    public void PreservesPaddingFromASingleSeed()
    {
        ChannelSeriesValue[] channels =
        [
            new(1, "Mic 04"),
            new(2, "2"),
            new(3, "3")
        ];

        IReadOnlyList<ChannelSeriesValue> result = ChannelNameSeriesService.Extend(channels, [1], 3);

        Assert.Equal(["Mic 05", "Mic 06"], result.Select(item => item.Name));
    }

    [Fact]
    public void RejectsNonConsecutiveSeeds()
    {
        ChannelSeriesValue[] channels =
        [
            new(1, "Mic 1"),
            new(2, "Other"),
            new(3, "Mic 2"),
            new(4, "4")
        ];

        Assert.Throws<InvalidOperationException>(() => ChannelNameSeriesService.Extend(channels, [1, 3], 4));
    }

    [Fact]
    public void ProjectSeriesRenameUpdatesTxSubscriptions()
    {
        using TestDirectory directory = new();
        string source = directory.PathFor("series.xml");
        SyntheticPresetFactory.Create(source, deviceCount: 2, txPerDevice: 4, rxPerDevice: 4);
        DanteProject project = DanteProject.Load(source);
        project.RenameChannel("DEVICE-001", DanteChannelKind.Tx, 1, "Mic 1");
        project.RenameChannel("DEVICE-001", DanteChannelKind.Tx, 2, "Mic 2");

        project.ExtendChannelNameSeries("DEVICE-001", DanteChannelKind.Tx, [1, 2], 4);

        DanteDevice device = Assert.Single(project.Devices, item => item.Name == "DEVICE-001");
        Assert.Equal(["Mic 1", "Mic 2", "Mic 3", "Mic 4"], device.TxChannels.Take(4).Select(channel => channel.DisplayName));
        Assert.Contains(project.PatchMatrix.Subscriptions, subscription => subscription.TxChannelName == "Mic 3");
        Assert.Contains(project.PatchMatrix.Subscriptions, subscription => subscription.TxChannelName == "Mic 4");
        Assert.False(project.ValidateXmlChangeGuard().HasErrors);
    }

    [Fact]
    public void PaddedTxSeriesCanBeUndoneAndSavedWithoutBreakingSubscriptions()
    {
        using TestDirectory directory = new();
        string source = directory.PathFor("series-padded.xml");
        string destination = directory.PathFor("series-padded-saved.xml");
        SyntheticPresetFactory.Create(source, deviceCount: 2, txPerDevice: 4, rxPerDevice: 4);
        DanteProject project = DanteProject.Load(source);
        project.RenameChannel("DEVICE-001", DanteChannelKind.Tx, 1, "Mic 01");
        project.PushUndoSnapshot("padded series");

        project.ExtendChannelNameSeries("DEVICE-001", DanteChannelKind.Tx, [1], 3);

        DanteDevice renamed = Assert.Single(project.Devices, item => item.Name == "DEVICE-001");
        Assert.Equal(["Mic 01", "Mic 02", "Mic 03"], renamed.TxChannels.Take(3).Select(channel => channel.DisplayName));
        Assert.Contains(project.PatchMatrix.Subscriptions, subscription => subscription.TxChannelName == "Mic 02");
        Assert.Contains(project.PatchMatrix.Subscriptions, subscription => subscription.TxChannelName == "Mic 03");
        Assert.False(project.ValidateXmlChangeGuard().HasErrors);

        project.UndoLastChange();
        DanteDevice restored = Assert.Single(project.Devices, item => item.Name == "DEVICE-001");
        Assert.Equal("Mic 01", restored.TxChannels[0].DisplayName);
        Assert.NotEqual("Mic 02", restored.TxChannels[1].DisplayName);
        Assert.False(project.ValidateXmlChangeGuard().HasErrors);

        project.PushUndoSnapshot("padded series");
        project.ExtendChannelNameSeries("DEVICE-001", DanteChannelKind.Tx, [1], 3);
        project.SaveAs(destination);
        DanteProject reloaded = DanteProject.Load(destination);
        DanteDevice saved = Assert.Single(reloaded.Devices, item => item.Name == "DEVICE-001");
        Assert.Equal(["Mic 01", "Mic 02", "Mic 03"], saved.TxChannels.Take(3).Select(channel => channel.DisplayName));
    }

    private sealed class TestDirectory : IDisposable
    {
        public TestDirectory()
        {
            Root = Path.Combine(Path.GetTempPath(), "DanteConfigEditorV3.SeriesTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Root);
        }

        private string Root { get; }

        public string PathFor(string fileName) => Path.Combine(Root, fileName);

        public void Dispose()
        {
            if (Directory.Exists(Root))
            {
                Directory.Delete(Root, recursive: true);
            }
        }
    }
}
