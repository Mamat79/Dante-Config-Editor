namespace DanteConfigEditor.Services;

/// <summary>
/// Construit les aides des réglages techniques sans dupliquer les formulations
/// françaises et anglaises dans chaque fenêtre.
/// </summary>
public static class CapabilityToolTipService
{
    public static string ForDevice(
        UiLanguage language,
        string deviceName,
        bool supported,
        string frenchSetting,
        string englishSetting,
        string xmlElement)
    {
        if (supported)
        {
            return language == UiLanguage.English
                ? $"{englishSetting} is available for {deviceName}."
                : $"{frenchSetting} est disponible pour {deviceName}.";
        }

        return language == UiLanguage.English
            ? $"Unavailable for {deviceName}: this Dante role does not expose <{xmlElement}>. DCE will not create it."
            : $"Indisponible pour {deviceName} : ce rôle Dante n'expose pas la balise <{xmlElement}>. DCE ne la créera pas.";
    }

    public static string ForTarget(
        UiLanguage language,
        bool available,
        string frenchSetting,
        string englishSetting,
        string xmlElement)
    {
        if (available)
        {
            return language == UiLanguage.English
                ? $"Applies {englishSetting} only to target devices that expose this setting."
                : $"Applique {frenchSetting} uniquement aux machines de la cible qui exposent ce paramètre.";
        }

        return language == UiLanguage.English
            ? $"Unavailable: no device in this preset exposes <{xmlElement}>. DCE will not create it."
            : $"Indisponible : aucune machine de ce preset n'expose <{xmlElement}>. DCE ne créera pas cette balise.";
    }
}
