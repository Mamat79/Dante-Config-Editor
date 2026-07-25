using System.IO;
using System.Text;

namespace DanteConfigEditor.Services;

public sealed class DiagnosticLogService
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private readonly object _sync = new();

    public DiagnosticLogService(string directoryPath)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            throw new ArgumentException("Le répertoire des journaux est obligatoire.", nameof(directoryPath));
        }

        DirectoryPath = Path.GetFullPath(directoryPath);
    }

    public static DiagnosticLogService Default { get; } = new(
        ApplicationStoragePaths.Resolve("Logs"));

    public string DirectoryPath { get; }

    public bool Write(string category, string message, Exception? exception = null)
    {
        try
        {
            string cleanCategory = string.IsNullOrWhiteSpace(category)
                ? "General"
                : category.Trim();
            string cleanMessage = string.IsNullOrWhiteSpace(message)
                ? "(aucun détail)"
                : message.Trim();
            StringBuilder entry = new();
            entry.Append('[')
                .Append(DateTimeOffset.Now.ToString("O"))
                .Append("] [")
                .Append(cleanCategory)
                .Append("] ")
                .AppendLine(cleanMessage);
            if (exception is not null)
            {
                entry.AppendLine(exception.ToString());
            }

            entry.AppendLine();
            lock (_sync)
            {
                Directory.CreateDirectory(DirectoryPath);
                string path = Path.Combine(
                    DirectoryPath,
                    $"diagnostic_{DateTime.Now:yyyyMMdd}.log");
                File.AppendAllText(path, entry.ToString(), Utf8WithoutBom);
            }

            return true;
        }
        catch
        {
            // Un journal indisponible ne doit jamais interrompre le travail ni
            // masquer l'erreur métier d'origine.
            return false;
        }
    }
}
