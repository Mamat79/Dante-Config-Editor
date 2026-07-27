using DanteConfigEditor.Domain.Projects;
using DanteConfigEditor.Domain.Validation;
using DanteConfigEditor.Models;
using DanteConfigEditor.Services;

namespace DanteConfigEditor.Application.Validation;

/// <summary>
/// Assemble les contrôles XML historiques et le profil de capacités 2026.1.
/// Le service ne modifie jamais le projet : il produit uniquement un état
/// structuré que les interfaces Windows et macOS peuvent présenter.
/// </summary>
public sealed class ProjectValidationService
{
    public ProjectValidationState Validate(
        DanteProject project,
        DanteXmlProfileDescriptor profile)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(profile);

        List<ProjectValidationIssue> issues = [];
        DanteValidationResult legacyValidation = project.Validate();
        issues.AddRange(legacyValidation.Issues.Select(
            (issue, index) => MapLegacyIssue(project, issue, index)));
        AddProfileIssues(project, profile, issues);
        AddExternalValidationScope(project, issues);

        return new ProjectValidationState(
            DateTimeOffset.Now,
            issues
                .OrderByDescending(issue => issue.Severity)
                .ThenBy(issue => issue.Category, StringComparer.Ordinal)
                .ThenBy(issue => issue.Code, StringComparer.Ordinal)
                .ToArray());
    }

    private static ProjectValidationIssue MapLegacyIssue(
        DanteProject project,
        DanteValidationIssue issue,
        int index)
    {
        ProjectValidationSeverity severity = issue.Severity switch
        {
            DanteIssueSeverity.Error => ProjectValidationSeverity.Error,
            DanteIssueSeverity.Warning => ProjectValidationSeverity.Warning,
            _ => ProjectValidationSeverity.Information
        };

        ProjectEntityReference? target = ResolveTarget(project, issue);
        return new ProjectValidationIssue(
            $"xml.{issue.Category}.{index}",
            severity,
            issue.Category.ToString(),
            severity switch
            {
                ProjectValidationSeverity.Error => "Validation.Legacy.Error",
                ProjectValidationSeverity.Warning => "Validation.Legacy.Warning",
                _ => "Validation.Legacy.Information"
            },
            issue.Message,
            target,
            BuildXmlPath(project, issue, target),
            SuggestedAction(issue.Category, target));
    }

    private static ProjectEntityReference? ResolveTarget(
        DanteProject project,
        DanteValidationIssue issue)
    {
        if (string.IsNullOrWhiteSpace(issue.DeviceName))
        {
            return null;
        }

        DanteDevice? device = project.FindDevice(issue.DeviceName);
        if (device is null)
        {
            return null;
        }

        if (issue.Category == DanteIssueCategory.Patch && issue.DanteId.HasValue)
        {
            DanteSubscription? subscription = project.PatchMatrix.Subscriptions.FirstOrDefault(
                candidate =>
                    string.Equals(
                        candidate.RxDevice,
                        device.Name,
                        StringComparison.OrdinalIgnoreCase)
                    && candidate.RxDanteId == issue.DanteId.Value);
            if (subscription is not null)
            {
                return new ProjectEntityReference(
                    ProjectEntityKind.Subscription,
                    ChannelStableId(device, DanteChannelKind.Rx, subscription.RxDanteId),
                    $"{device.Name} / RX {subscription.RxDanteId} - {subscription.RxChannelName}",
                    device.StableIdentity);
            }
        }

        if (issue.DanteId.HasValue)
        {
            DanteChannel[] channels = device.TxChannels
                .Concat(device.RxChannels)
                .Where(channel => channel.DanteId == issue.DanteId.Value)
                .Where(channel =>
                    string.IsNullOrWhiteSpace(issue.ChannelName)
                    || string.Equals(
                        channel.DisplayName,
                        issue.ChannelName,
                        StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (channels.Length == 1)
            {
                DanteChannel channel = channels[0];
                return new ProjectEntityReference(
                    channel.Kind == DanteChannelKind.Tx
                        ? ProjectEntityKind.TxChannel
                        : ProjectEntityKind.RxChannel,
                    ChannelStableId(device, channel.Kind, channel.DanteId),
                    $"{device.Name} / {channel.Kind.ToString().ToUpperInvariant()} "
                    + $"{channel.DanteId} - {channel.DisplayName}",
                    device.StableIdentity);
            }
        }

        return new ProjectEntityReference(
            ProjectEntityKind.Device,
            device.StableIdentity,
            device.Name);
    }

    private static void AddProfileIssues(
        DanteProject project,
        DanteXmlProfileDescriptor profile,
        ICollection<ProjectValidationIssue> issues)
    {
        ProjectEntityReference projectTarget = ProjectTarget(project);
        string profileDetail =
            $"Profile={profile.Id}; recognition={profile.RecognitionLevel}; "
            + $"access={profile.AccessMode}; presetVersion={profile.PresetVersion}; "
            + $"namespace={Blank(profile.NamespaceName)}";

        if (!profile.Capabilities.CanSave)
        {
            issues.Add(new ProjectValidationIssue(
                "profile.save-disabled",
                ProjectValidationSeverity.Error,
                "XmlProfile",
                "Validation.Profile.SaveDisabled",
                profileDetail,
                projectTarget,
                "/preset",
                "Validation.Action.InspectXml"));
        }
        else if (profile.AccessMode == ProjectAccessMode.Restricted)
        {
            issues.Add(new ProjectValidationIssue(
                "profile.restricted",
                ProjectValidationSeverity.Warning,
                "XmlProfile",
                "Validation.Profile.Restricted",
                profileDetail,
                projectTarget,
                "/preset",
                "Validation.Action.InspectXml"));
        }
        else
        {
            issues.Add(new ProjectValidationIssue(
                "profile.complete",
                ProjectValidationSeverity.Information,
                "XmlProfile",
                "Validation.Profile.Complete",
                profileDetail,
                projectTarget,
                "/preset"));
        }

        for (int index = 0; index < profile.Reasons.Count; index++)
        {
            string reason = profile.Reasons[index];
            issues.Add(new ProjectValidationIssue(
                $"profile.reason.{index}",
                profile.IsReadOnly
                    ? ProjectValidationSeverity.Error
                    : ProjectValidationSeverity.Warning,
                "XmlProfile",
                reason,
                profileDetail,
                projectTarget,
                "/preset",
                "Validation.Action.InspectXml"));
        }

        string[] unavailableCapabilities = DescribeUnavailableCapabilities(profile.Capabilities);
        if (unavailableCapabilities.Length > 0)
        {
            issues.Add(new ProjectValidationIssue(
                "profile.capabilities",
                profile.IsReadOnly
                    ? ProjectValidationSeverity.Warning
                    : ProjectValidationSeverity.Information,
                "Capabilities",
                "Validation.Profile.CapabilitiesLimited",
                string.Join(", ", unavailableCapabilities),
                projectTarget,
                "/preset"));
        }
    }

    private static void AddExternalValidationScope(
        DanteProject project,
        ICollection<ProjectValidationIssue> issues)
    {
        issues.Add(new ProjectValidationIssue(
            "scope.dante-controller",
            ProjectValidationSeverity.Information,
            "ExternalValidation",
            "Validation.External.DanteController",
            "DCE validates XML structure and internal consistency offline. "
            + "Hardware availability, firmware support and live-network behavior "
            + "are outside this automatic validation.",
            ProjectTarget(project),
            "/preset",
            "Validation.Action.ReviewChecklist"));
    }

    private static string? SuggestedAction(
        DanteIssueCategory category,
        ProjectEntityReference? target)
    {
        if (target?.Kind == ProjectEntityKind.Subscription
            || category == DanteIssueCategory.Patch)
        {
            return "Validation.Action.OpenPatch";
        }

        if (target?.Kind is ProjectEntityKind.Device
            or ProjectEntityKind.TxChannel
            or ProjectEntityKind.RxChannel)
        {
            return "Validation.Action.OpenMachine";
        }

        return category is DanteIssueCategory.XmlCompatibility
            or DanteIssueCategory.SaveSafety
            ? "Validation.Action.InspectXml"
            : null;
    }

    private static string? BuildXmlPath(
        DanteProject project,
        DanteValidationIssue issue,
        ProjectEntityReference? target)
    {
        if (target is null)
        {
            return issue.Category is DanteIssueCategory.XmlCompatibility
                or DanteIssueCategory.SaveSafety
                ? "/preset"
                : null;
        }

        DanteDevice? device = project.FindDevice(issue.DeviceName);
        if (device is null)
        {
            return null;
        }

        string devicePath =
            $"/preset/device[friendly_name='{EscapePathValue(device.Name)}']";
        if (!issue.DanteId.HasValue)
        {
            return devicePath;
        }

        string channelElement = target.Kind switch
        {
            ProjectEntityKind.Subscription or ProjectEntityKind.RxChannel => "rxchannel",
            ProjectEntityKind.TxChannel => "txchannel",
            _ => "*channel"
        };
        return $"{devicePath}/{channelElement}[@danteId='{issue.DanteId.Value}']";
    }

    private static ProjectEntityReference ProjectTarget(DanteProject project) =>
        new(
            ProjectEntityKind.Project,
            project.OriginalFilePath,
            string.IsNullOrWhiteSpace(project.PresetName)
                ? Path.GetFileName(project.OriginalFilePath)
                : project.PresetName);

    private static string ChannelStableId(
        DanteDevice device,
        DanteChannelKind kind,
        int danteId) =>
        $"{device.StableIdentity}:{kind}:{danteId}";

    private static string EscapePathValue(string value) =>
        value.Replace("'", "&apos;", StringComparison.Ordinal);

    private static string Blank(string value) =>
        string.IsNullOrWhiteSpace(value) ? "(none)" : value;

    private static string[] DescribeUnavailableCapabilities(
        DanteXmlCapabilities capabilities)
    {
        List<string> unavailable = [];
        AddIfDisabled(unavailable, capabilities.CanEditDeviceNames, "device names");
        AddIfDisabled(unavailable, capabilities.CanEditTxLabels, "TX labels");
        AddIfDisabled(unavailable, capabilities.CanEditRxLabels, "RX labels");
        AddIfDisabled(unavailable, capabilities.CanEditPatch, "subscriptions");
        AddIfDisabled(unavailable, capabilities.CanEditNetwork, "network");
        AddIfDisabled(unavailable, capabilities.CanEditAudioFormat, "audio format");
        AddIfDisabled(unavailable, capabilities.CanCreateDevices, "device creation");
        AddIfDisabled(unavailable, capabilities.CanCreateProject, "project creation");
        AddIfDisabled(unavailable, capabilities.CanSave, "save");
        return unavailable.ToArray();
    }

    private static void AddIfDisabled(
        ICollection<string> values,
        bool enabled,
        string label)
    {
        if (!enabled)
        {
            values.Add(label);
        }
    }
}
