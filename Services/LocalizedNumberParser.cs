namespace DanteConfigEditor.Services;

public static class LocalizedNumberParser
{
    public static int ParsePositive(string? value, string label, UiLanguage language)
    {
        if (int.TryParse(value?.Trim(), out int parsed) && parsed > 0)
        {
            return parsed;
        }

        throw new InvalidOperationException(language == UiLanguage.English
            ? $"{label}: invalid value."
            : $"{label} : valeur invalide.");
    }

    public static int? ParseOptionalCount(string? value, UiLanguage language)
    {
        if (!int.TryParse(value?.Trim(), out int parsed) || parsed < 0)
        {
            throw new InvalidOperationException(language == UiLanguage.English
                ? "Invalid channel count."
                : "Nombre de canaux invalide.");
        }

        return parsed == 0 ? null : parsed;
    }
}
