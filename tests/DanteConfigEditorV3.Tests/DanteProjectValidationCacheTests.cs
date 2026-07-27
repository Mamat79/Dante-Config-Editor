using DanteConfigEditor.Models;
using DanteConfigEditor.Services;

namespace DanteConfigEditorV3.Tests;

public sealed class DanteProjectValidationCacheTests
{
    [Fact]
    public void CachedGuardResultCannotBePoisonedByCaller()
    {
        DanteProject project = OpenFixture();
        DanteValidationResult first = project.ValidateXmlChangeGuard();
        Assert.False(first.HasErrors);

        first.AddError(
            DanteIssueCategory.SaveSafety,
            "Erreur ajoutée uniquement par le test.");

        DanteValidationResult second = project.ValidateXmlChangeGuard();
        Assert.False(second.HasErrors);
        Assert.DoesNotContain(
            second.Errors,
            message => message.Contains("uniquement par le test", StringComparison.Ordinal));
    }

    [Fact]
    public void DirectXmlMutationInvalidatesGuardCache()
    {
        DanteProject project = OpenFixture();
        Assert.False(project.ValidateXmlChangeGuard().HasErrors);

        project.Document
            .Descendants()
            .First(element => element.Name.LocalName == "device_id")
            .Value = "CHANGED-TECHNICAL-ID";

        DanteValidationResult guard = project.ValidateXmlChangeGuard();
        Assert.True(guard.HasErrors);
        Assert.Contains(
            guard.Errors,
            message => message.Contains("device_id", StringComparison.OrdinalIgnoreCase)
                || message.Contains("instance_id", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void BusinessMutationInvalidatesFullValidationCache()
    {
        DanteProject project = OpenFixture();
        Assert.DoesNotContain(
            project.Validate().Warnings,
            warning => warning.Contains(
                "Aucune machine preferred master",
                StringComparison.Ordinal));

        project.SetPreferredMaster("DEVICE-B", false);

        Assert.Contains(
            project.Validate().Warnings,
            warning => warning.Contains(
                "Aucune machine preferred master",
                StringComparison.Ordinal));
    }

    private static DanteProject OpenFixture() =>
        DanteProject.Load(Path.Combine(
            AppContext.BaseDirectory,
            "Fixtures",
            "representative-preset.xml"));
}
