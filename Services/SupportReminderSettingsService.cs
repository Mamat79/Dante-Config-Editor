using System.IO;
using System.Reflection;
using System.Text.Json;

namespace DanteConfigEditor.Services;

public sealed class SupportReminderSettingsService
{
    public const int DefaultReminderInterval = 20;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _settingsPath;

    public SupportReminderSettingsService(string? settingsPath = null)
    {
        _settingsPath = settingsPath ?? ApplicationStoragePaths.Resolve("support-reminder.json");
    }

    public SupportReminderDecision RegisterSuccessfulLaunch(string applicationVersion)
    {
        SupportReminderState? existing = Load();
        string normalizedVersion = string.IsNullOrWhiteSpace(applicationVersion)
            ? "unknown"
            : applicationVersion.Trim();

        if (existing is null)
        {
            SupportReminderState firstLaunch = new()
            {
                LaunchCount = 1,
                NextReminderLaunch = DefaultReminderInterval,
                ApplicationVersion = normalizedVersion
            };
            Save(firstLaunch);
            return new SupportReminderDecision(false, firstLaunch);
        }

        SupportReminderState updated = existing with
        {
            LaunchCount = Math.Max(0, existing.LaunchCount) + 1,
            NextReminderLaunch = Math.Max(1, existing.NextReminderLaunch)
        };

        bool versionChanged = !string.Equals(
            existing.ApplicationVersion,
            normalizedVersion,
            StringComparison.Ordinal);
        if (versionChanged)
        {
            updated = updated with
            {
                ApplicationVersion = normalizedVersion,
                NextReminderLaunch = Math.Max(updated.NextReminderLaunch, updated.LaunchCount + 1)
            };
        }

        Save(updated);
        bool shouldShow = !versionChanged
            && !updated.NeverShowAgain
            && updated.LaunchCount >= updated.NextReminderLaunch;
        return new SupportReminderDecision(shouldShow, updated);
    }

    public void Postpone()
    {
        SupportReminderState state = Load() ?? new SupportReminderState();
        Save(state with
        {
            NextReminderLaunch = Math.Max(0, state.LaunchCount) + DefaultReminderInterval
        });
    }

    public void Suppress()
    {
        SupportReminderState state = Load() ?? new SupportReminderState();
        Save(state with { NeverShowAgain = true });
    }

    public SupportReminderState? Load()
    {
        try
        {
            if (!File.Exists(_settingsPath))
            {
                return null;
            }

            return JsonSerializer.Deserialize<SupportReminderState>(
                File.ReadAllText(_settingsPath),
                JsonOptions);
        }
        catch
        {
            // Un réglage local illisible ne doit jamais empêcher le démarrage.
            return null;
        }
    }

    public static bool IsAutomatedTestProcess()
    {
        string entryName = Assembly.GetEntryAssembly()?.GetName().Name ?? string.Empty;
        if (entryName.Contains("testhost", StringComparison.OrdinalIgnoreCase)
            || entryName.EndsWith(".Tests", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return AppDomain.CurrentDomain.GetAssemblies().Any(assembly =>
            assembly.GetName().Name?.EndsWith(".Tests", StringComparison.OrdinalIgnoreCase) == true);
    }

    private void Save(SupportReminderState state)
    {
        string? directory = Path.GetDirectoryName(_settingsPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string temporaryPath = _settingsPath + ".tmp";
        File.WriteAllText(temporaryPath, JsonSerializer.Serialize(state, JsonOptions));
        File.Move(temporaryPath, _settingsPath, overwrite: true);
    }
}

public sealed record SupportReminderState
{
    public int LaunchCount { get; init; }

    public int NextReminderLaunch { get; init; } = SupportReminderSettingsService.DefaultReminderInterval;

    public bool NeverShowAgain { get; init; }

    public string ApplicationVersion { get; init; } = string.Empty;
}

public sealed record SupportReminderDecision(
    bool ShouldShow,
    SupportReminderState State);
