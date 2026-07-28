using System.Text.Json;
using DanteConfigEditor.Models;
using DanteConfigEditor.Services;

namespace DanteConfigEditorV3.Tests;

public sealed class MachineTemplateLocalizationServiceTests
{
    [Fact]
    public void EnglishUsesBundledLocalizedDescriptionAndCategory()
    {
        using JsonDocument json = JsonDocument.Parse(
            """{"descriptionEn":"Sanitized offline role."}""");
        MachineTemplateMetadata metadata = new()
        {
            Description = "Rôle hors ligne assaini.",
            Category = "Carte réseau audio",
            AdditionalData = new Dictionary<string, JsonElement>
            {
                ["descriptionEn"] =
                    json.RootElement.GetProperty("descriptionEn").Clone()
            }
        };

        Assert.Equal(
            "Sanitized offline role.",
            MachineTemplateLocalizationService.Description(
                metadata,
                UiLanguage.English));
        Assert.Equal(
            "Audio network card",
            MachineTemplateLocalizationService.Category(
                metadata,
                UiLanguage.English));
    }

    [Fact]
    public void FrenchAndLegacyBanksKeepTheirOriginalMetadata()
    {
        MachineTemplateMetadata metadata = new()
        {
            Description = "Description personnelle",
            Category = "Catégorie personnelle"
        };

        Assert.Equal(
            "Description personnelle",
            MachineTemplateLocalizationService.Description(
                metadata,
                UiLanguage.French));
        Assert.Equal(
            "Description personnelle",
            MachineTemplateLocalizationService.Description(
                metadata,
                UiLanguage.English));
        Assert.Equal(
            "Catégorie personnelle",
            MachineTemplateLocalizationService.Category(
                metadata,
                UiLanguage.English));
    }
}
