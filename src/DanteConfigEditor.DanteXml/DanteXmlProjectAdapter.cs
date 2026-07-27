using DanteConfigEditor.Domain.Projects;
using DanteConfigEditor.DanteXml.Profiles;
using DanteConfigEditor.Models;
using DanteConfigEditor.Services;

namespace DanteConfigEditor.DanteXml;

public sealed record DanteXmlOpenResult(
    DanteProject Project,
    DanteXmlProfileDescriptor Profile);

public sealed record DanteXmlSaveResult(
    string DestinationPath,
    string BackupPath,
    DanteValidationResult Validation);

public interface IDanteXmlProjectAdapter
{
    DanteXmlOpenResult Open(string path);

    DanteXmlSaveResult SaveAs(DanteXmlOpenResult openProject, string destinationPath);
}

public sealed class DanteXmlProjectAdapter : IDanteXmlProjectAdapter
{
    private readonly IDanteXmlProfileDetector _profileDetector;

    public DanteXmlProjectAdapter(IDanteXmlProfileDetector? profileDetector = null)
    {
        _profileDetector = profileDetector ?? new DanteXmlProfileDetector();
    }

    public DanteXmlOpenResult Open(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("The XML path is required.", nameof(path));
        }

        DanteProject project = DanteProject.Load(path);
        return new DanteXmlOpenResult(project, _profileDetector.Detect(project));
    }

    public DanteXmlSaveResult SaveAs(DanteXmlOpenResult openProject, string destinationPath)
    {
        ArgumentNullException.ThrowIfNull(openProject);
        if (!openProject.Profile.Capabilities.CanSave)
        {
            throw new InvalidOperationException(
                $"The XML profile '{openProject.Profile.Id}' does not allow saving.");
        }

        DanteValidationResult validation = openProject.Project.Validate();
        if (validation.HasErrors)
        {
            throw new InvalidOperationException(validation.ToDisplayText());
        }

        string backupPath = openProject.Project.SaveAs(destinationPath);
        return new DanteXmlSaveResult(
            Path.GetFullPath(destinationPath),
            backupPath,
            validation);
    }
}
