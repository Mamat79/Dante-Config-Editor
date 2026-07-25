using System.Reflection;
using System.Xml.Linq;
using DanteConfigEditor.Models;

namespace DanteConfigEditor.Services;

public static class MachineTemplateService
{
    public static MachineTemplatePackage CreateFromDevice(
        DanteDevice source,
        string sourcePresetVersion,
        MachineTemplateCreateRequest request)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(request);

        string templateName = NormalizeRequiredText(request.TemplateName, "Le nom du modèle est obligatoire.", 120);
        XElement templateDevice = MachineRoleInstantiationService.CreateSanitizedTemplateDevice(
            source.Element,
            request.TxLabels,
            request.RxLabels);
        DateTimeOffset now = DateTimeOffset.UtcNow;
        MachineTemplateMetadata metadata = new()
        {
            TemplateId = Guid.NewGuid(),
            TemplateName = templateName,
            Manufacturer = NormalizeOptionalText(request.Manufacturer)
                ?? source.Element.ChildValue("manufacturer_name"),
            Model = NormalizeOptionalText(request.Model)
                ?? source.Element.ChildValue("model_name"),
            Description = NormalizeOptionalText(request.Description) ?? string.Empty,
            Category = NormalizeOptionalText(request.Category) ?? string.Empty,
            Tags = NormalizeTags(request.Tags),
            TxCount = templateDevice.Children("txchannel").Count(),
            RxCount = templateDevice.Children("rxchannel").Count(),
            SourcePresetVersion = sourcePresetVersion?.Trim() ?? string.Empty,
            SourceXmlNamespace = source.Element.Name.NamespaceName,
            CreatedByDceVersion = ReadDceVersion(),
            CreatedUtc = now,
            ModifiedUtc = now
        };

        return new MachineTemplatePackage(
            metadata,
            new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), templateDevice),
            NormalizeOptionalText(request.ImageSourcePath));
    }

    public static MachineTemplatePackage Update(
        MachineTemplatePackage source,
        MachineTemplateEditRequest request)
    {
        return BuildEditedPackage(source, request, duplicate: false);
    }

    public static MachineTemplatePackage Duplicate(
        MachineTemplatePackage source,
        MachineTemplateEditRequest request)
    {
        return BuildEditedPackage(source, request, duplicate: true);
    }

    private static MachineTemplatePackage BuildEditedPackage(
        MachineTemplatePackage source,
        MachineTemplateEditRequest request,
        bool duplicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(request);
        XElement templateDevice = MachineRoleInstantiationService.CreateSanitizedTemplateDevice(
            source.TemplateDocument.Root
                ?? throw new InvalidOperationException("Le modèle ne contient pas de racine <device>."),
            request.TxLabels,
            request.RxLabels);
        DateTimeOffset now = DateTimeOffset.UtcNow;
        MachineTemplateMetadata metadata = source.Metadata with
        {
            TemplateId = duplicate ? Guid.NewGuid() : source.Metadata.TemplateId,
            TemplateName = NormalizeRequiredText(request.TemplateName, "Le nom du modèle est obligatoire.", 120),
            Manufacturer = NormalizeOptionalText(request.Manufacturer) ?? string.Empty,
            Model = NormalizeOptionalText(request.Model) ?? string.Empty,
            Description = NormalizeOptionalText(request.Description) ?? string.Empty,
            Category = NormalizeOptionalText(request.Category) ?? string.Empty,
            Tags = NormalizeTags(request.Tags),
            TxCount = templateDevice.Children("txchannel").Count(),
            RxCount = templateDevice.Children("rxchannel").Count(),
            CreatedUtc = duplicate ? now : source.Metadata.CreatedUtc,
            ModifiedUtc = now,
            TemplateSha256 = string.Empty,
            ImageFileName = null
        };
        string? imageSourcePath = request.RemoveImage
            ? null
            : NormalizeOptionalText(request.ImageSourcePath) ?? source.ImagePath;
        return new MachineTemplatePackage(
            metadata,
            new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), templateDevice),
            imageSourcePath);
    }

    private static string ReadDceVersion()
    {
        return typeof(MachineTemplateService).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion
            ?? typeof(MachineTemplateService).Assembly.GetName().Version?.ToString()
            ?? "unknown";
    }

    private static string NormalizeRequiredText(string? value, string error, int maximumLength)
    {
        string clean = value?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(clean))
        {
            throw new InvalidOperationException(error);
        }

        if (clean.Length > maximumLength)
        {
            throw new InvalidOperationException($"La valeur ne doit pas dépasser {maximumLength} caractères.");
        }

        if (clean.Any(char.IsControl))
        {
            throw new InvalidOperationException("La valeur contient des caractères non imprimables.");
        }

        return clean;
    }

    private static string? NormalizeOptionalText(string? value)
    {
        string clean = value?.Trim() ?? string.Empty;
        return string.IsNullOrWhiteSpace(clean) ? null : clean;
    }

    private static IReadOnlyList<string> NormalizeTags(IEnumerable<string?> tags)
    {
        return tags
            .Select(tag => tag?.Trim() ?? string.Empty)
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(tag => tag, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
