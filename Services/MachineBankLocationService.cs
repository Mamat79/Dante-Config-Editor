using System.IO;
using System.Text;

namespace DanteConfigEditor.Services;

public sealed class MachineBankLocationService
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private readonly string _settingsPath;
    private readonly string _defaultBankPath;

    public MachineBankLocationService(string settingsPath, string defaultBankPath)
    {
        if (string.IsNullOrWhiteSpace(settingsPath))
        {
            throw new ArgumentException("Le chemin des réglages est obligatoire.", nameof(settingsPath));
        }

        if (string.IsNullOrWhiteSpace(defaultBankPath))
        {
            throw new ArgumentException("Le chemin par défaut de la banque est obligatoire.", nameof(defaultBankPath));
        }

        _settingsPath = Path.GetFullPath(settingsPath);
        _defaultBankPath = Path.GetFullPath(defaultBankPath);
    }

    public static MachineBankLocationService CreateDefault()
    {
        string documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        return new MachineBankLocationService(
            ApplicationStoragePaths.Resolve("machine-bank-location.txt"),
            Path.Combine(documents, "Dante Config Editor", "Machine Bank"));
    }

    public string Load()
    {
        try
        {
            if (!File.Exists(_settingsPath))
            {
                return _defaultBankPath;
            }

            string configured = File.ReadAllText(_settingsPath, Encoding.UTF8).Trim();
            return string.IsNullOrWhiteSpace(configured)
                ? _defaultBankPath
                : Path.GetFullPath(configured);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        {
            return _defaultBankPath;
        }
    }

    public void Save(string bankPath)
    {
        if (string.IsNullOrWhiteSpace(bankPath))
        {
            throw new ArgumentException("Le chemin de la banque est obligatoire.", nameof(bankPath));
        }

        string fullPath = Path.GetFullPath(bankPath);
        string settingsDirectory = Path.GetDirectoryName(_settingsPath)
            ?? throw new InvalidOperationException("Le dossier de réglages est introuvable.");
        Directory.CreateDirectory(settingsDirectory);
        string temporaryPath = Path.Combine(
            settingsDirectory,
            $".{Path.GetFileName(_settingsPath)}.{Guid.NewGuid():N}.tmp");
        try
        {
            File.WriteAllText(temporaryPath, fullPath + Environment.NewLine, Utf8WithoutBom);
            if (File.Exists(_settingsPath))
            {
                string backupPath = _settingsPath + ".bak";
                File.Replace(temporaryPath, _settingsPath, backupPath, ignoreMetadataErrors: true);
            }
            else
            {
                File.Move(temporaryPath, _settingsPath);
            }
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }
}
