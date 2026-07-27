namespace DanteConfigEditor.Services;

public enum DanteIssueSeverity
{
    Info,
    Warning,
    Error
}

public enum DanteIssueCategory
{
    XmlCompatibility,
    Patch,
    Device,
    Channel,
    Clock,
    Network,
    AudioFormat,
    SaveSafety
}

public sealed class DanteValidationIssue
{
    public DanteIssueSeverity Severity { get; init; }

    public DanteIssueCategory Category { get; init; }

    public string? DeviceName { get; init; }

    public string? ChannelName { get; init; }

    public int? DanteId { get; init; }

    public string Message { get; init; } = string.Empty;

    public string SeverityLabel => Severity switch
    {
        DanteIssueSeverity.Error => "Erreur",
        DanteIssueSeverity.Warning => "Avertissement",
        _ => "Info"
    };

    public string CategoryLabel => Category switch
    {
        DanteIssueCategory.XmlCompatibility => "Compatibilité XML",
        DanteIssueCategory.Patch => "Patch",
        DanteIssueCategory.Device => "Device",
        DanteIssueCategory.Channel => "Canal",
        DanteIssueCategory.Clock => "Horloge",
        DanteIssueCategory.Network => "Réseau",
        DanteIssueCategory.AudioFormat => "Format audio",
        DanteIssueCategory.SaveSafety => "Sauvegarde",
        _ => Category.ToString()
    };
}

public sealed class DanteValidationResult
{
    // Les erreurs bloquent la sauvegarde. Les avertissements signalent des points à vérifier
    // sans empêcher l'utilisateur de continuer.
    public List<string> Errors { get; } = [];

    public List<string> Warnings { get; } = [];

    public List<string> Infos { get; } = [];

    public List<DanteValidationIssue> Issues { get; } = [];

    public bool HasErrors => Errors.Count > 0;

    public bool HasWarnings => Warnings.Count > 0;

    public void AddIssue(
        DanteIssueSeverity severity,
        DanteIssueCategory category,
        string message,
        string? deviceName = null,
        string? channelName = null,
        int? danteId = null)
    {
        Issues.Add(new DanteValidationIssue
        {
            Severity = severity,
            Category = category,
            Message = message,
            DeviceName = deviceName,
            ChannelName = channelName,
            DanteId = danteId
        });

        if (severity == DanteIssueSeverity.Error)
        {
            Errors.Add(message);
        }
        else if (severity == DanteIssueSeverity.Warning)
        {
            Warnings.Add(message);
        }
        else
        {
            Infos.Add(message);
        }
    }

    public void AddError(DanteIssueCategory category, string message, string? deviceName = null, string? channelName = null, int? danteId = null)
    {
        AddIssue(DanteIssueSeverity.Error, category, message, deviceName, channelName, danteId);
    }

    public void AddWarning(DanteIssueCategory category, string message, string? deviceName = null, string? channelName = null, int? danteId = null)
    {
        AddIssue(DanteIssueSeverity.Warning, category, message, deviceName, channelName, danteId);
    }

    public void AddInfo(DanteIssueCategory category, string message, string? deviceName = null, string? channelName = null, int? danteId = null)
    {
        AddIssue(DanteIssueSeverity.Info, category, message, deviceName, channelName, danteId);
    }

    public void Merge(DanteValidationResult other)
    {
        foreach (DanteValidationIssue issue in other.Issues)
        {
            AddIssue(issue.Severity, issue.Category, issue.Message, issue.DeviceName, issue.ChannelName, issue.DanteId);
        }
    }

    public string ToDisplayText() => ToDisplayText(UiLanguage.French);

    public string ToDisplayText(UiLanguage language)
    {
        List<string> lines = [];

        if (Errors.Count == 0 && Warnings.Count == 0 && Infos.Count == 0)
        {
            return language == UiLanguage.English
                ? "No blocking issue detected."
                : "Aucune anomalie bloquante détectée.";
        }

        if (Errors.Count > 0)
        {
            lines.Add(language == UiLanguage.English ? "Blocking errors:" : "Erreurs bloquantes :");
            lines.AddRange(Errors.Select(error =>
                "- " + LocalizationService.TranslateValidationMessage(language, error)));
        }

        if (Warnings.Count > 0)
        {
            if (lines.Count > 0)
            {
                lines.Add(string.Empty);
            }

            lines.Add(language == UiLanguage.English ? "Items to check:" : "Points à vérifier :");
            lines.AddRange(Warnings.Select(warning =>
                "- " + LocalizationService.TranslateValidationMessage(language, warning)));
        }

        if (Infos.Count > 0)
        {
            if (lines.Count > 0)
            {
                lines.Add(string.Empty);
            }

            lines.Add(language == UiLanguage.English ? "Information:" : "Informations :");
            lines.AddRange(Infos.Take(20).Select(info =>
                "- " + LocalizationService.TranslateValidationMessage(language, info)));
            if (Infos.Count > 20)
            {
                lines.Add(language == UiLanguage.English
                    ? $"- {Infos.Count - 20} additional information item(s) are visible in the Validation center."
                    : $"- {Infos.Count - 20} information(s) supplémentaire(s) visibles dans l'onglet Santé du fichier.");
            }
        }

        return string.Join(Environment.NewLine, lines);
    }
}
