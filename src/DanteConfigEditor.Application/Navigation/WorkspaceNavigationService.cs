namespace DanteConfigEditor.Application.Navigation;

public enum WorkspaceSection
{
    Home,
    Overview,
    Machines,
    Patch,
    Synoptic,
    DeviceLibrary,
    ImportExport,
    Validation,
    History,
    AdvancedTools
}

public sealed record WorkspaceNavigationChangedEventArgs(
    WorkspaceSection Previous,
    WorkspaceSection Current);

/// <summary>
/// Conserve la destination fonctionnelle du shell sans dépendre de WPF.
/// Les vues Windows et macOS peuvent ainsi traduire la même section vers
/// leurs propres contrôles sans dupliquer les règles de navigation.
/// </summary>
public sealed class WorkspaceNavigationService
{
    public WorkspaceNavigationService(WorkspaceSection initialSection = WorkspaceSection.Home)
    {
        CurrentSection = initialSection;
    }

    public event EventHandler<WorkspaceNavigationChangedEventArgs>? Changed;

    public WorkspaceSection CurrentSection { get; private set; }

    public bool NavigateTo(WorkspaceSection section)
    {
        if (section == CurrentSection)
        {
            return false;
        }

        WorkspaceSection previous = CurrentSection;
        CurrentSection = section;
        Changed?.Invoke(this, new WorkspaceNavigationChangedEventArgs(previous, section));
        return true;
    }
}
