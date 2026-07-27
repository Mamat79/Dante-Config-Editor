using DanteConfigEditor.Application;
using DanteConfigEditor.DanteXml;
using DanteConfigEditor.Domain.Projects;
using DanteConfigEditor.Domain.Validation;

namespace DanteConfigEditorV3.Tests;

public sealed class ProjectValidationServiceTests
{
    [Fact]
    public void CompleteProfileAddsCapabilitiesAndExternalValidationScope()
    {
        ProjectSession session = OpenRepresentativeSession();

        Assert.Contains(
            session.Validation.Issues,
            issue => issue.Code == "profile.complete"
                     && issue.Severity == ProjectValidationSeverity.Information);
        Assert.Contains(
            session.Validation.Issues,
            issue => issue.Code == "scope.dante-controller"
                     && issue.Category == "ExternalValidation");
        Assert.True(session.Validation.InformationCount > 0);
    }

    [Fact]
    public void LocalSubscriptionIssueTargetsTheExactRxAndExposesAnXmlPath()
    {
        ProjectSession session = OpenRepresentativeSession();

        ProjectValidationIssue issue = Assert.Single(
            session.Validation.Issues,
            candidate =>
                candidate.Category == "Patch"
                && candidate.TechnicalDetail.Contains(
                    "source locale",
                    StringComparison.OrdinalIgnoreCase));

        Assert.Equal(ProjectEntityKind.Subscription, issue.Target?.Kind);
        Assert.Equal("DEVICE-A / RX 1 - LOCAL MON", issue.Target?.DisplayName);
        Assert.Contains("rxchannel[@danteId='1']", issue.XmlPath);
        Assert.Equal("Validation.Action.OpenPatch", issue.SuggestedActionKey);
    }

    [Fact]
    public void PartialProfileIsReportedAsRestrictedWithoutBlockingSave()
    {
        string path = WriteTemporaryPreset(
            """
            <?xml version="1.0" encoding="UTF-8"?>
            <preset version="3.0.0">
              <name>Partial validation</name>
              <device>
                <friendly_name>PARTIAL-DEVICE</friendly_name>
                <txchannel><label>TX 1</label></txchannel>
              </device>
            </preset>
            """);

        try
        {
            ProjectSession session = new();
            session.OpenProject(DanteConfigEditor.Models.DanteProject.Load(path));

            Assert.Contains(
                session.Validation.Issues,
                issue => issue.Code == "profile.restricted"
                         && issue.Severity == ProjectValidationSeverity.Warning);
            Assert.DoesNotContain(
                session.Validation.Issues,
                issue => issue.Code == "profile.save-disabled");
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void UnknownProfileReportsSaveAsBlocking()
    {
        string path = WriteTemporaryPreset(
            """
            <?xml version="1.0" encoding="UTF-8"?>
            <unknown version="3.0.0">
              <device>
                <friendly_name>UNKNOWN-DEVICE</friendly_name>
              </device>
            </unknown>
            """);

        try
        {
            ProjectSession session = new();
            session.OpenXml(new DanteXmlProjectAdapter().Open(path));

            Assert.Contains(
                session.Validation.Issues,
                issue => issue.Code == "profile.save-disabled"
                         && issue.Severity == ProjectValidationSeverity.Error);
            Assert.False(session.Profile.Capabilities.CanSave);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static ProjectSession OpenRepresentativeSession()
    {
        ProjectSession session = new();
        session.OpenXml(new DanteXmlProjectAdapter().Open(RepositoryFile(
            "tests",
            "DanteConfigEditorV3.Tests",
            "Fixtures",
            "representative-preset.xml")));
        return session;
    }

    private static string WriteTemporaryPreset(string xml)
    {
        string path = Path.Combine(
            Path.GetTempPath(),
            $"dce-validation-{Guid.NewGuid():N}.xml");
        File.WriteAllText(path, xml);
        return path;
    }

    private static string RepositoryFile(params string[] relativeParts) =>
        Path.Combine([RepositoryDirectory(), .. relativeParts]);

    private static string RepositoryDirectory()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null
               && !File.Exists(Path.Combine(directory.FullName, "DanteConfigEditorV3.csproj")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);
        return directory!.FullName;
    }
}
