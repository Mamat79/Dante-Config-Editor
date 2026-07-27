using System.IO;
using DanteConfigEditor.Services;

namespace DanteConfigEditor.Models;

public sealed partial class DanteProject
{
    public static DanteProject CreateNew(string intendedPath, NewProjectOptions options)
    {
        string fullPath = ValidateIntendedPath(intendedPath);
        DanteProject project = new(fullPath, ProjectCreationService.CreateDocument(options));
        project.IsModified = true;
        project._changes.Add(new ChangeRecord(
            DateTime.Now,
            "Nouveau projet expérimental",
            $"{options.Machines.Count} machine(s), format preset {ProjectCreationService.PresetVersion}"));
        return project;
    }

    public static DanteProject CreateNewFromTemplate(
        string intendedPath,
        string projectName,
        string? description,
        MachineTemplatePackage template,
        MachineInstanceOptions instanceOptions)
    {
        string fullPath = ValidateIntendedPath(intendedPath);
        DanteProject project = new(
            fullPath,
            ProjectCreationService.CreateDocumentFromTemplate(
                projectName,
                description,
                template,
                instanceOptions));
        project.IsModified = true;
        project._changes.Add(new ChangeRecord(
            DateTime.Now,
            "Nouveau projet expérimental",
            $"Créé depuis le modèle {template.Metadata.TemplateName}, format preset {ProjectCreationService.PresetVersion}"));
        return project;
    }

    private static string ValidateIntendedPath(string intendedPath)
    {
        if (string.IsNullOrWhiteSpace(intendedPath))
        {
            throw new ArgumentException("Le chemin prévu pour le nouveau projet doit être renseigné.", nameof(intendedPath));
        }

        string fullPath = Path.GetFullPath(intendedPath);
        if (File.Exists(fullPath))
        {
            throw new IOException(
                $"Le fichier existe déjà. Choisissez un nouveau nom pour éviter tout écrasement : {fullPath}");
        }

        return fullPath;
    }
}
