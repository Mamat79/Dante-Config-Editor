using DanteConfigEditor.Models;

namespace DanteConfigEditor.Services;

public static class MachineBankMigrationService
{
    public const int CurrentBankFormatVersion = 1;

    public const int CurrentTemplateFormatVersion = MachineTemplateMetadata.CurrentFormatVersion;

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
        // La V3.6 introduit le format 1 : il n'existe donc encore aucun format
        // historique maîtrisé à convertir. Ce point central accueillera les
        // migrations explicites et sauvegardées lors d'une évolution future.
        return sourceVersion == CurrentBankFormatVersion;
    }

    public static bool CanMigrateTemplateFormat(int sourceVersion)
    {
        return sourceVersion == CurrentTemplateFormatVersion;
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
}
