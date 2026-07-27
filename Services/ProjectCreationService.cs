using System.Text;
using System.Xml.Linq;
using DanteConfigEditor.Models;

namespace DanteConfigEditor.Services;

public static class ProjectCreationService
{
    public const string PresetVersion = "3.0.0";
    public const int MaximumAudioChannelsPerDirection = 512;

    private static readonly string[] CaptureInfoElements =
    [
        "device_name",
        "device_samplerate",
        "device_encoding",
        "device_unicast_latency",
        "txchannel_names",
        "txflows",
        "rxchannel_names",
        "rxchannel_subscriptions",
        "rxflows"
    ];

    public static XDocument CreateDocument(NewProjectOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        string projectName = NormalizeProjectName(options.ProjectName);
        if (options.Machines.Count == 0)
        {
            throw new InvalidOperationException(
                "Un nouveau projet doit contenir au moins une machine ou un rôle de la banque.");
        }

        string[] names = options.Machines
            .Select(machine => DanteNameRules.EnsureValidDeviceName(machine.Name))
            .ToArray();
        string? duplicate = names
            .GroupBy(name => name, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1)?
            .Key;
        if (!string.IsNullOrWhiteSpace(duplicate))
        {
            throw new InvalidOperationException($"Le nom de machine '{duplicate}' est présent plusieurs fois.");
        }

        XElement root = new("preset",
            new XAttribute("version", PresetVersion),
            new XElement("name", projectName),
            new XElement(
                "description",
                string.IsNullOrWhiteSpace(options.Description)
                    ? "Dante Controller preset"
                    : options.Description.Trim()));

        for (int index = 0; index < options.Machines.Count; index++)
        {
            root.Add(CreateCustomDevice(options.Machines[index], names[index]));
        }

        return new XDocument(
            new XDeclaration("1.0", "UTF-8", "yes"),
            root);
    }

    public static XDocument CreateDocumentFromTemplate(
        string projectName,
        string? description,
        MachineTemplatePackage template,
        MachineInstanceOptions instanceOptions)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(instanceOptions);
        string cleanProjectName = NormalizeProjectName(projectName);
        MachineRoleCreation creation = MachineRoleInstantiationService.CreateFromTemplate(
            template.TemplateDocument.Root
                ?? throw new InvalidOperationException("Le modèle ne contient pas de racine <device>."),
            instanceOptions);
        if (!string.Equals(
                template.Metadata.SourcePresetVersion,
                PresetVersion,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Le modèle provient d'un preset {template.Metadata.SourcePresetVersion}, "
                + $"mais la création de projet cible {PresetVersion}. "
                + "La conversion automatique n'est pas encore validée.");
        }

        RebaseDanteNamespace(
            creation.DeviceElement,
            template.TemplateDocument.Root!.Name.Namespace,
            XNamespace.None);

        XElement root = new("preset",
            new XAttribute("version", PresetVersion),
            new XElement("name", cleanProjectName),
            new XElement(
                "description",
                string.IsNullOrWhiteSpace(description)
                    ? "Dante Controller preset"
                    : description.Trim()),
            creation.DeviceElement);
        return new XDocument(
            new XDeclaration("1.0", "UTF-8", "yes"),
            root);
    }

    private static XElement CreateCustomDevice(
        NewCustomMachineDefinition definition,
        string cleanName)
    {
        ValidateChannelCount(definition.TxCount, "TX");
        ValidateChannelCount(definition.RxCount, "RX");
        if (definition.TxCount + definition.RxCount == 0)
        {
            throw new InvalidOperationException(
                $"La machine {cleanName} doit contenir au moins un canal TX ou RX.");
        }

        if (definition.SampleRate <= 0)
        {
            throw new InvalidOperationException($"La sample rate de {cleanName} doit être positive.");
        }

        if (definition.Encoding <= 0)
        {
            throw new InvalidOperationException($"L'encodage de {cleanName} doit être positif.");
        }

        if (definition.UnicastLatency <= 0)
        {
            throw new InvalidOperationException($"La latence de {cleanName} doit être positive.");
        }

        XElement captureInfo = new("captureInfo");
        foreach (string elementName in CaptureInfoElements)
        {
            captureInfo.Add(new XElement(elementName));
        }

        XElement device = new("device",
            captureInfo,
            new XElement("friendly_name", cleanName),
            new XElement("samplerate", definition.SampleRate),
            new XElement("encoding", definition.Encoding),
            new XElement("unicast_latency", definition.UnicastLatency));
        for (int index = 1; index <= definition.TxCount; index++)
        {
            device.Add(new XElement(
                "txchannel",
                new XAttribute("danteId", index),
                new XAttribute("mediaType", "audio"),
                new XElement("label", $"Ch {index}")));
        }

        for (int index = 1; index <= definition.RxCount; index++)
        {
            device.Add(new XElement(
                "rxchannel",
                new XAttribute("danteId", index),
                new XAttribute("mediaType", "audio"),
                new XElement("name", $"Ch {index}")));
        }

        return device;
    }

    private static string NormalizeProjectName(string? value)
    {
        string clean = value?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(clean))
        {
            throw new InvalidOperationException("Le nom du projet est obligatoire.");
        }

        if (clean.Length > 120)
        {
            throw new InvalidOperationException("Le nom du projet ne doit pas dépasser 120 caractères.");
        }

        if (clean.Any(character => char.IsControl(character)
                && character is not '\r' and not '\n' and not '\t'))
        {
            throw new InvalidOperationException("Le nom du projet contient des caractères non imprimables.");
        }

        return clean;
    }

    private static void ValidateChannelCount(int count, string kind)
    {
        if (count < 0 || count > MaximumAudioChannelsPerDirection)
        {
            throw new InvalidOperationException(
                $"Le nombre de canaux {kind} doit être compris entre 0 et {MaximumAudioChannelsPerDirection}.");
        }
    }

    private static void RebaseDanteNamespace(
        XElement root,
        XNamespace sourceNamespace,
        XNamespace targetNamespace)
    {
        foreach (XElement element in root.DescendantsAndSelf())
        {
            if (element.Name.Namespace == sourceNamespace)
            {
                element.Name = targetNamespace + element.Name.LocalName;
            }
        }
    }
}
