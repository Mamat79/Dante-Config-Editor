using DanteConfigEditor.Application.Commands;
using DanteConfigEditor.DanteXml;
using DanteConfigEditor.DanteXml.Profiles;
using DanteConfigEditor.Domain.History;
using DanteConfigEditor.Domain.Projects;
using DanteConfigEditor.Domain.Validation;
using DanteConfigEditor.Models;
using DanteConfigEditor.Services;

namespace DanteConfigEditor.Application;

public enum ProjectSessionChangeKind
{
    ProjectOpened,
    ProjectClosed,
    ModelChanged,
    SelectionChanged,
    FiltersChanged,
    ValidationChanged,
    Saved,
    UndoRedoChanged
}

public sealed record ProjectSessionChangedEventArgs(
    ProjectSessionChangeKind Kind,
    long Revision,
    string? CommandId = null);

public sealed class ProjectSession
{
    private readonly List<ProjectHistoryEntry> _history = [];
    private readonly Dictionary<string, string> _filters = new(StringComparer.Ordinal);
    private DanteXmlOpenResult? _openedProject;

    public ProjectSession(int undoLimit = 30)
    {
        CommandDispatcher = new ProjectCommandDispatcher(this, undoLimit);
    }

    public event EventHandler<ProjectSessionChangedEventArgs>? Changed;

    public ProjectCommandDispatcher CommandDispatcher { get; }

    public bool HasProject => _openedProject is not null;

    public DanteProject Project =>
        _openedProject?.Project
        ?? throw new InvalidOperationException("No project is open in the session.");

    public DanteXmlProfileDescriptor Profile =>
        _openedProject?.Profile
        ?? throw new InvalidOperationException("No XML profile is available.");

    public ProjectDocumentKind DocumentKind { get; private set; } = ProjectDocumentKind.DanteXml;

    public string? ProjectPackagePath { get; private set; }

    public string SourcePath => HasProject ? Project.OriginalFilePath : string.Empty;

    public string CurrentSavePath =>
        HasProject ? Project.LastSavedPath ?? Project.OriginalFilePath : string.Empty;

    public bool IsModified => HasProject && Project.IsModified;

    public long ModelRevision { get; private set; }

    public ProjectSelection Selection { get; private set; } = ProjectSelection.Empty;

    public IReadOnlyDictionary<string, string> Filters => _filters;

    public ProjectValidationState Validation { get; private set; } =
        ProjectValidationState.Empty;

    public IReadOnlyList<ProjectHistoryEntry> History => _history;

    public void OpenProject(DanteProject project)
    {
        ArgumentNullException.ThrowIfNull(project);
        DanteXmlProfileDescriptor profile = new DanteXmlProfileDetector().Detect(project);
        OpenXml(new DanteXmlOpenResult(project, profile));
    }

    public void OpenXml(DanteXmlOpenResult openedProject)
    {
        ArgumentNullException.ThrowIfNull(openedProject);

        _openedProject = openedProject;
        DocumentKind = ProjectDocumentKind.DanteXml;
        ProjectPackagePath = null;
        Selection = ProjectSelection.Empty;
        _filters.Clear();
        _history.Clear();
        CommandDispatcher.Clear();
        ModelRevision++;
        RefreshValidation();
        RaiseChanged(ProjectSessionChangeKind.ProjectOpened);
    }

    public void OpenProjectPackage(DanteXmlOpenResult openedProject, string packagePath)
    {
        if (string.IsNullOrWhiteSpace(packagePath))
        {
            throw new ArgumentException("A project package path is required.", nameof(packagePath));
        }

        OpenXml(openedProject);
        DocumentKind = ProjectDocumentKind.DceProject;
        ProjectPackagePath = packagePath;
    }

    public void Close()
    {
        _openedProject = null;
        DocumentKind = ProjectDocumentKind.DanteXml;
        ProjectPackagePath = null;
        Selection = ProjectSelection.Empty;
        Validation = ProjectValidationState.Empty;
        _filters.Clear();
        _history.Clear();
        CommandDispatcher.Clear();
        ModelRevision++;
        RaiseChanged(ProjectSessionChangeKind.ProjectClosed);
    }

    public void SetSelection(ProjectSelection selection)
    {
        Selection = selection ?? ProjectSelection.Empty;
        RaiseChanged(ProjectSessionChangeKind.SelectionChanged);
    }

    public void SetFilter(string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("A filter key is required.", nameof(key));
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            _filters.Remove(key);
        }
        else
        {
            _filters[key] = value;
        }

        RaiseChanged(ProjectSessionChangeKind.FiltersChanged);
    }

    public void ResetFilters()
    {
        if (_filters.Count == 0)
        {
            return;
        }

        _filters.Clear();
        RaiseChanged(ProjectSessionChangeKind.FiltersChanged);
    }

    public void RefreshValidation()
    {
        if (!HasProject)
        {
            Validation = ProjectValidationState.Empty;
            return;
        }

        DanteValidationResult legacyValidation = Project.Validate();
        ProjectValidationIssue[] issues = legacyValidation.Issues
            .Select((issue, index) => MapValidationIssue(issue, index))
            .ToArray();
        Validation = new ProjectValidationState(DateTimeOffset.Now, issues);
        RaiseChanged(ProjectSessionChangeKind.ValidationChanged);
    }

    internal void NotifyModelChanged(string commandId)
    {
        ModelRevision++;
        RefreshValidation();
        RaiseChanged(ProjectSessionChangeKind.ModelChanged, commandId);
    }

    internal void NotifyUndoRedo(string commandId)
    {
        ModelRevision++;
        RefreshValidation();
        RaiseChanged(ProjectSessionChangeKind.UndoRedoChanged, commandId);
    }

    internal void AddHistory(ProjectHistoryEntry entry)
    {
        _history.Add(entry);
    }

    internal void SetHistoryUndone(Guid historyEntryId, bool undone)
    {
        int index = _history.FindIndex(entry => entry.Id == historyEntryId);
        if (index >= 0)
        {
            _history[index] = _history[index] with { WasUndone = undone };
        }
    }

    public void MarkSaved()
    {
        CommandDispatcher.Clear();
        RaiseChanged(ProjectSessionChangeKind.Saved);
    }

    private ProjectValidationIssue MapValidationIssue(
        DanteValidationIssue issue,
        int index)
    {
        ProjectEntityReference? target = null;
        if (!string.IsNullOrWhiteSpace(issue.DeviceName))
        {
            DanteDevice? device = Project.FindDevice(issue.DeviceName);
            target = device is null
                ? null
                : new ProjectEntityReference(
                    ProjectEntityKind.Device,
                    device.StableIdentity,
                    device.Name);
        }

        ProjectValidationSeverity severity = issue.Severity switch
        {
            DanteIssueSeverity.Error => ProjectValidationSeverity.Error,
            DanteIssueSeverity.Warning => ProjectValidationSeverity.Warning,
            _ => ProjectValidationSeverity.Information
        };

        return new ProjectValidationIssue(
            $"legacy.{issue.Category}.{index}",
            severity,
            issue.Category.ToString(),
            $"Validation.{issue.Category}",
            issue.Message,
            target);
    }

    private void RaiseChanged(ProjectSessionChangeKind kind, string? commandId = null)
    {
        Changed?.Invoke(this, new ProjectSessionChangedEventArgs(kind, ModelRevision, commandId));
    }
}
