using DanteConfigEditor.Services;

namespace DanteConfigEditorV3.Tests;

public sealed class MachineBankLocationV36Tests
{
    [Fact]
    public void MissingSettingUsesTheVisibleDocumentsDefault()
    {
        using TemporaryDirectory workspace = new();
        string defaultPath = Path.Combine(workspace.Path, "Documents", "Dante Config Editor", "Machine Bank");
        MachineBankLocationService service = new(
            Path.Combine(workspace.Path, "Settings", "machine-bank-location.txt"),
            defaultPath);

        Assert.Equal(Path.GetFullPath(defaultPath), service.Load());
    }

    [Fact]
    public void SavingANewLocationIsAtomicAndKeepsThePreviousSettingAsBackup()
    {
        using TemporaryDirectory workspace = new();
        string settingsPath = Path.Combine(workspace.Path, "Settings", "machine-bank-location.txt");
        MachineBankLocationService service = new(
            settingsPath,
            Path.Combine(workspace.Path, "DefaultBank"));
        string firstBank = Path.Combine(workspace.Path, "Shared", "First");
        string secondBank = Path.Combine(workspace.Path, "Shared", "Second");

        service.Save(firstBank);
        service.Save(secondBank);

        Assert.Equal(Path.GetFullPath(secondBank), service.Load());
        Assert.True(File.Exists(settingsPath + ".bak"));
        Assert.Equal(Path.GetFullPath(firstBank), File.ReadAllText(settingsPath + ".bak").Trim());
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "DanteConfigEditorV3.Tests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
