using System.IO;

namespace DanteConfigEditor.Services;

public static class ThemeSettingsService
{
    private static readonly string SettingsPath =
        ApplicationStoragePaths.Resolve("theme.txt");

    private static readonly string LegacyMacSettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "DanteConfigEditorV3",
        "theme-macos.txt");

    public static bool LoadUseLightTheme()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                return IsLight(File.ReadAllText(SettingsPath));
            }

            if (File.Exists(LegacyMacSettingsPath))
            {
                bool useLightTheme = IsLight(File.ReadAllText(LegacyMacSettingsPath));
                SaveUseLightTheme(useLightTheme);
                return useLightTheme;
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }

        // Un premier lancement doit être immédiatement lisible, y compris sur
        // un écran configuré avec une mise à l'échelle élevée.
        return true;
    }

    public static void SaveUseLightTheme(bool useLightTheme)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
            File.WriteAllText(SettingsPath, useLightTheme ? "light" : "dark");
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private static bool IsLight(string value) =>
        string.Equals(value.Trim(), "light", StringComparison.OrdinalIgnoreCase);
}
