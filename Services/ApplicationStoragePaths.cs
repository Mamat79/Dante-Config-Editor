using System.IO;

namespace DanteConfigEditor.Services;

public static class ApplicationStoragePaths
{
    // Les versions récentes réutilisent l'espace V3.2 afin de préserver les
    // préférences, récupérations et chemins de banque lors des mises à niveau.
    public const string RootFolderName = "DanteConfigEditorV3.2";

    public static string RootPath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        RootFolderName);

    public static string Resolve(params string[] relativeParts)
    {
        return Path.Combine([RootPath, .. relativeParts]);
    }
}
