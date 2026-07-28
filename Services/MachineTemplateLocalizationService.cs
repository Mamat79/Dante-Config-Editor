using System.Text.Json;
using DanteConfigEditor.Models;

namespace DanteConfigEditor.Services;

/// <summary>
/// Présente les métadonnées localisées lorsqu'un modèle les fournit, tout en
/// conservant la compatibilité avec les banques personnelles plus anciennes.
/// </summary>
public static class MachineTemplateLocalizationService
{
    private static readonly IReadOnlyDictionary<string, string> EnglishCategories =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Adaptateur"] = "Adapter",
            ["Amplificateur"] = "Amplifier",
            ["Carte réseau audio"] = "Audio network card",
            ["Console"] = "Mixing console",
            ["Intercom"] = "Intercom",
            ["Interface audio"] = "Audio interface",
            ["Interface logicielle"] = "Software interface",
            ["Moniteur"] = "Monitor",
            ["Pont réseau audio"] = "Audio network bridge",
            ["Processeur"] = "Processor",
            ["Récepteur sans fil"] = "Wireless receiver",
            ["Stagebox"] = "Stagebox",
            ["Système sans fil"] = "Wireless system"
        };

    public static string Description(
        MachineTemplateMetadata metadata,
        UiLanguage language)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        return language == UiLanguage.English
            ? AdditionalString(metadata, "descriptionEn") ?? metadata.Description
            : metadata.Description;
    }

    public static string Category(
        MachineTemplateMetadata metadata,
        UiLanguage language)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        if (language != UiLanguage.English)
        {
            return metadata.Category;
        }

        string? explicitCategory = AdditionalString(metadata, "categoryEn");
        if (!string.IsNullOrWhiteSpace(explicitCategory))
        {
            return explicitCategory;
        }

        return EnglishCategories.TryGetValue(metadata.Category, out string? translated)
            ? translated
            : metadata.Category;
    }

    private static string? AdditionalString(
        MachineTemplateMetadata metadata,
        string key)
    {
        if (metadata.AdditionalData is null
            || !metadata.AdditionalData.TryGetValue(key, out JsonElement value)
            || value.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        string? text = value.GetString()?.Trim();
        return string.IsNullOrWhiteSpace(text) ? null : text;
    }
}
