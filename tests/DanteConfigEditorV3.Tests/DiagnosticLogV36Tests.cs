using DanteConfigEditor.Services;

namespace DanteConfigEditorV3.Tests;

public sealed class DiagnosticLogV36Tests
{
    [Fact]
    public void DiagnosticLogWritesCategoryMessageAndExceptionWithoutThrowing()
    {
        string directory = Path.Combine(
            Path.GetTempPath(),
            "DanteConfigEditorV3.Tests",
            Guid.NewGuid().ToString("N"));
        try
        {
            DiagnosticLogService service = new(directory);

            bool written = service.Write(
                "MachineBank",
                "Modèle illisible",
                new InvalidOperationException("hash mismatch"));

            Assert.True(written);
            string path = Assert.Single(Directory.GetFiles(directory, "diagnostic_*.log"));
            string content = File.ReadAllText(path);
            Assert.Contains("[MachineBank]", content, StringComparison.Ordinal);
            Assert.Contains("Modèle illisible", content, StringComparison.Ordinal);
            Assert.Contains("hash mismatch", content, StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }
}
