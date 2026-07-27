using DanteConfigEditor.Models;

namespace DanteConfigEditor.Services;

public static class MachineBankMigrationService
{
    public const int FirstSupportedBankFormatVersion = 1;

    public const int CurrentBankFormatVersion = 2;

    public const int CurrentTemplateFormatVersion = MachineTemplateMetadata.CurrentFormatVersion;

    public static void EnsureSupportedBankFormat(int formatVersion)
    {
        EnsureSupportedFormat(
            formatVersion,
            FirstSupportedBankFormatVersion,
            CurrentBankFormatVersion,
            "banque");
    }

    public static void EnsureSupportedTemplateFormat(int formatVersion)
    {
        EnsureSupportedFormat(
            formatVersion,
            1,
            CurrentTemplateFormatVersion,
            "modèle");
    }

    public static void EnsureCurrentBankFormat(int formatVersion)
    {
        EnsureCurrentFormat(formatVersion, CurrentBankFormatVersion, "banque");
    }

    public static void EnsureCurrentTemplateFormat(int formatVersion)
    {
        EnsureCurrentFormat(formatVersion, CurrentTemplateFormatVersion, "modèle");
    }

    public static bool CanMigrateBankFormat(int sourceVersion)
    {
        return sourceVersion >= FirstSupportedBankFormatVersion
               && sourceVersion <= CurrentBankFormatVersion;
    }

    public static bool CanMigrateTemplateFormat(int sourceVersion)
    {
        return sourceVersion >= 1
               && sourceVersion <= CurrentTemplateFormatVersion;
    }

    private static void EnsureCurrentFormat(int actual, int expected, string subject)
    {
        if (actual == expected)
        {
            return;
        }

        string direction = actual < expected ? "ancien" : "plus récent";
        throw new MachineBankCorruptionException(
            $"Version de {subject} {direction} non prise en charge : {actual}. "
            + $"Version attendue : {expected}. Aucune migration sûre n'est disponible ; "
            + "les fichiers sont laissés intacts.");
    }

    private static void EnsureSupportedFormat(
        int actual,
        int minimum,
        int maximum,
        string subject)
    {
        if (actual >= minimum && actual <= maximum)
        {
            return;
        }

        string direction = actual < minimum ? "ancien" : "plus récent";
        throw new MachineBankCorruptionException(
            $"Version de {subject} {direction} non prise en charge : {actual}. "
            + $"Versions lisibles : {minimum} à {maximum}. "
            + "Aucune migration sûre n'est disponible ; les fichiers sont laissés intacts.");
    }
}
