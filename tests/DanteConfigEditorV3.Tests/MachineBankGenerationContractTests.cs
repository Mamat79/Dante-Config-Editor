using System.Text.Json;

namespace DanteConfigEditorV3.Tests;

public sealed class MachineBankGenerationContractTests
{
    [Fact]
    public void CommunityCatalogAndGeneratedBankStayUniqueAndVersioned()
    {
        using JsonDocument catalog = JsonDocument.Parse(
            File.ReadAllText(RepositoryFile(
                "machine-banks",
                "community-device-sources.json")));
        JsonElement[] profiles = catalog.RootElement
            .GetProperty("profiles")
            .EnumerateArray()
            .ToArray();

        Assert.Equal(1, catalog.RootElement.GetProperty("formatVersion").GetInt32());
        Assert.Equal(43, profiles.Length);
        Assert.Equal(
            profiles.Length,
            profiles
                .Select(profile => profile.GetProperty("key").GetString())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count());
        Assert.Equal(
            profiles.Length,
            profiles
                .Select(profile =>
                    $"{profile.GetProperty("manufacturer").GetString()}\0" +
                    profile.GetProperty("model").GetString())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count());
        Assert.All(profiles, profile =>
        {
            Assert.NotEmpty(profile.GetProperty("sourceMatchers").EnumerateArray());
            JsonElement image = profile.GetProperty("image");
            Assert.False(string.IsNullOrWhiteSpace(
                image.GetProperty("sourcePage").GetString()));
        });

        string bankRoot = RepositoryFile(
            "Resources",
            "MachineBanks",
            "Bundled",
            "DCE Community Devices 2026.1");
        using JsonDocument manifest = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(bankRoot, "bank.json")));
        Assert.Equal(2, manifest.RootElement.GetProperty("formatVersion").GetInt32());
        Assert.Equal(
            profiles.Length,
            manifest.RootElement.GetProperty("templateIds").GetArrayLength());

        string[] metadataFiles = Directory.GetFiles(
            Path.Combine(bankRoot, "machines"),
            "machine.json",
            SearchOption.AllDirectories);
        Assert.Equal(profiles.Length, metadataFiles.Length);
        Assert.All(metadataFiles, path =>
        {
            using JsonDocument metadata = JsonDocument.Parse(File.ReadAllText(path));
            Assert.Equal(2, metadata.RootElement.GetProperty("formatVersion").GetInt32());
            Assert.False(string.IsNullOrWhiteSpace(
                metadata.RootElement.GetProperty("imageSha256").GetString()));
        });
    }

    [Fact]
    public void PrivateCorpusImporterHandlesZeroOrOneChannelWithoutEmbeddingSources()
    {
        string script = File.ReadAllText(RepositoryFile(
            "tools",
            "Import-SanitizedMachineBankCandidates.ps1"));

        Assert.Contains(
            "@(Get-DirectChildren -Parent $device -LocalName \"txchannel\").Count",
            script,
            StringComparison.Ordinal);
        Assert.Contains(
            "@(Get-DirectChildren -Parent $device -LocalName \"rxchannel\").Count",
            script,
            StringComparison.Ordinal);
        Assert.DoesNotContain("source-dante-", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Radio France", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Synology", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("OneDrive", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("$LASTEXITCODE", script, StringComparison.Ordinal);
    }

    private static string RepositoryFile(params string[] relativeParts) =>
        Path.Combine([RepositoryDirectory(), .. relativeParts]);

    private static string RepositoryDirectory()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null
               && !File.Exists(Path.Combine(
                   directory.FullName,
                   "DanteConfigEditorV3.csproj")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);
        return directory!.FullName;
    }
}
