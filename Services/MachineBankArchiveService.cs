using System.IO;
using System.IO.Compression;

namespace DanteConfigEditor.Services;

public static class MachineBankArchiveService
{
    private const int MaximumEntryCount = 20_000;
    private const long MaximumEntryBytes = 64L * 1024 * 1024;
    private const long MaximumArchiveBytes = 2L * 1024 * 1024 * 1024;

    public static void ExportBank(string bankPath, string destinationArchivePath)
    {
        string source = Path.GetFullPath(bankPath);
        string destination = ValidateNewArchivePath(destinationArchivePath);
        MachineBankRepository repository = new(source);
        foreach (var metadata in repository.List())
        {
            repository.Load(metadata.TemplateId);
        }

        string manifestPath = Path.Combine(source, "bank.json");
        string machinesPath = Path.Combine(source, "machines");
        if (!File.Exists(manifestPath) || !Directory.Exists(machinesPath))
        {
            throw new DirectoryNotFoundException(
                "La banque doit contenir bank.json et le répertoire machines.");
        }

        string temporaryPath = destination + $".{Guid.NewGuid():N}.tmp";
        try
        {
            using (FileStream stream = File.Create(temporaryPath))
            using (ZipArchive archive = new(stream, ZipArchiveMode.Create, leaveOpen: false))
            {
                AddFile(archive, manifestPath, "bank.json");
                foreach (string file in Directory.EnumerateFiles(
                             machinesPath,
                             "*",
                             SearchOption.AllDirectories))
                {
                    string relative = Path.GetRelativePath(source, file)
                        .Replace(Path.DirectorySeparatorChar, '/');
                    AddFile(archive, file, relative);
                }
            }

            File.Move(temporaryPath, destination);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    public static string RestoreBank(string archivePath, string destinationBankPath)
    {
        if (string.IsNullOrWhiteSpace(archivePath) || !File.Exists(archivePath))
        {
            throw new FileNotFoundException("L'archive de banque est introuvable.", archivePath);
        }

        string destination = Path.GetFullPath(destinationBankPath);
        EnsureDestinationIsEmpty(destination);
        string parent = Path.GetDirectoryName(destination)
            ?? throw new InvalidOperationException("Le dossier parent de la banque est introuvable.");
        Directory.CreateDirectory(parent);
        string staging = Path.Combine(parent, $".dce-bank-restore-{Guid.NewGuid():N}");
        Directory.CreateDirectory(staging);
        try
        {
            ExtractSafely(archivePath, staging);
            MachineBankRepository repository = new(staging);
            foreach (var metadata in repository.List())
            {
                repository.Load(metadata.TemplateId);
            }

            if (Directory.Exists(destination))
            {
                Directory.Delete(destination, recursive: false);
            }

            Directory.Move(staging, destination);
            return destination;
        }
        finally
        {
            if (Directory.Exists(staging))
            {
                Directory.Delete(staging, recursive: true);
            }
        }
    }

    private static string ValidateNewArchivePath(string destinationArchivePath)
    {
        if (string.IsNullOrWhiteSpace(destinationArchivePath))
        {
            throw new ArgumentException(
                "Le chemin de l'archive est obligatoire.",
                nameof(destinationArchivePath));
        }

        string destination = Path.GetFullPath(destinationArchivePath);
        if (File.Exists(destination) || Directory.Exists(destination))
        {
            throw new IOException(
                $"La destination existe déjà et ne sera pas remplacée : {destination}");
        }

        string directory = Path.GetDirectoryName(destination)
            ?? throw new InvalidOperationException("Le dossier de destination est introuvable.");
        if (!Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException(
                $"Le dossier de destination n'existe pas : {directory}");
        }

        return destination;
    }

    private static void EnsureDestinationIsEmpty(string destination)
    {
        if (File.Exists(destination))
        {
            throw new IOException(
                $"Un fichier existe déjà à l'emplacement de la banque : {destination}");
        }

        if (Directory.Exists(destination)
            && Directory.EnumerateFileSystemEntries(destination).Any())
        {
            throw new IOException(
                $"Le dossier de restauration n'est pas vide : {destination}. "
                + "Choisissez un nouveau dossier pour ne rien écraser.");
        }
    }

    private static void AddFile(ZipArchive archive, string sourcePath, string entryName)
    {
        ZipArchiveEntry entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
        using Stream destination = entry.Open();
        using FileStream source = File.OpenRead(sourcePath);
        source.CopyTo(destination);
    }

    private static void ExtractSafely(string archivePath, string destinationDirectory)
    {
        StringComparison pathComparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        string destinationRoot = Path.GetFullPath(destinationDirectory)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        using ZipArchive archive = ZipFile.OpenRead(archivePath);
        if (archive.Entries.Count > MaximumEntryCount)
        {
            throw new InvalidDataException(
                $"L'archive contient trop de fichiers ({archive.Entries.Count}).");
        }

        long totalBytes = 0;
        HashSet<string> extractedPaths = new(
            OperatingSystem.IsWindows()
                ? StringComparer.OrdinalIgnoreCase
                : StringComparer.Ordinal);
        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            if (entry.Length > MaximumEntryBytes)
            {
                throw new InvalidDataException(
                    $"Le fichier '{entry.FullName}' dépasse la taille autorisée.");
            }

            totalBytes = checked(totalBytes + entry.Length);
            if (totalBytes > MaximumArchiveBytes)
            {
                throw new InvalidDataException(
                    "La taille décompressée de la banque dépasse la limite autorisée.");
            }

            string destinationPath = Path.GetFullPath(
                Path.Combine(destinationDirectory, entry.FullName));
            if (!destinationPath.StartsWith(destinationRoot, pathComparison))
            {
                throw new InvalidDataException(
                    $"Archive refusée : chemin hors de la banque ({entry.FullName}).");
            }

            if (!extractedPaths.Add(destinationPath))
            {
                throw new InvalidDataException(
                    $"Archive refusée : chemin dupliqué ({entry.FullName}).");
            }

            if (string.IsNullOrEmpty(entry.Name))
            {
                Directory.CreateDirectory(destinationPath);
                continue;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
            entry.ExtractToFile(destinationPath, overwrite: false);
        }
    }
}
