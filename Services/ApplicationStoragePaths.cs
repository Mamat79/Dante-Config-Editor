using System.IO;

namespace DanteConfigEditor.Services;

public static class ApplicationStoragePaths
{
    // La 2026.1 utilise son propre profil local. La copie des réglages V3.6 est
    // assurée par une migration non destructive au premier démarrage.
    public const string RootFolderName = "DanteConfigEditor2026.1";

    public const string LegacyV36RootFolderName = "DanteConfigEditorV3.2";

    public static string RootPath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        RootFolderName);

    public static string LegacyV36RootPath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        LegacyV36RootFolderName);

    public static string Resolve(params string[] relativeParts)
    {
        return Path.Combine([RootPath, .. relativeParts]);
    }
}
