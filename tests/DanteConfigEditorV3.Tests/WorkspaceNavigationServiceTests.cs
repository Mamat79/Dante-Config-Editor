using DanteConfigEditor.Application.Navigation;

namespace DanteConfigEditorV3.Tests;

public sealed class WorkspaceNavigationServiceTests
{
    [Fact]
    public void StartsOnRequestedSection()
    {
        WorkspaceNavigationService navigation = new(WorkspaceSection.Overview);

        Assert.Equal(WorkspaceSection.Overview, navigation.CurrentSection);
    }

    [Fact]
    public void NavigateRaisesOneChangeWithPreviousAndCurrentSections()
    {
        WorkspaceNavigationService navigation = new();
        WorkspaceNavigationChangedEventArgs? observed = null;
        navigation.Changed += (_, change) => observed = change;

        bool changed = navigation.NavigateTo(WorkspaceSection.Machines);

        Assert.True(changed);
        Assert.Equal(WorkspaceSection.Machines, navigation.CurrentSection);
        Assert.Equal(
            new WorkspaceNavigationChangedEventArgs(
                WorkspaceSection.Home,
                WorkspaceSection.Machines),
            observed);
    }

    [Fact]
    public void NavigatingToCurrentSectionIsIdempotent()
    {
        WorkspaceNavigationService navigation = new(WorkspaceSection.Patch);
        int eventCount = 0;
        navigation.Changed += (_, _) => eventCount++;

        bool changed = navigation.NavigateTo(WorkspaceSection.Patch);

        Assert.False(changed);
        Assert.Equal(0, eventCount);
    }

    [Theory]
    [InlineData(WorkspaceSection.Home)]
    [InlineData(WorkspaceSection.Overview)]
    [InlineData(WorkspaceSection.Machines)]
    [InlineData(WorkspaceSection.Patch)]
    [InlineData(WorkspaceSection.Synoptic)]
    [InlineData(WorkspaceSection.DeviceLibrary)]
    [InlineData(WorkspaceSection.ImportExport)]
    [InlineData(WorkspaceSection.Validation)]
    [InlineData(WorkspaceSection.History)]
    [InlineData(WorkspaceSection.AdvancedTools)]
    public void EveryShellSectionCanBeSelected(WorkspaceSection section)
    {
        WorkspaceNavigationService navigation = new(WorkspaceSection.Home);

        navigation.NavigateTo(section);

        Assert.Equal(section, navigation.CurrentSection);
    }
}
