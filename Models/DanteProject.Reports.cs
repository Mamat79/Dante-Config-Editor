using System.IO;
using System.Text;
using DanteConfigEditor.Services;

namespace DanteConfigEditor.Models;

public sealed partial class DanteProject
{
    public string BuildSaveSummary(UiLanguage language = UiLanguage.French)
    {
        bool english = language == UiLanguage.English;
        StringBuilder builder = new();
        DanteValidationResult validation = Validate();

        builder.AppendLine(english ? "PRE-SAVE SUMMARY" : "RÉSUMÉ AVANT SAUVEGARDE");
        builder.AppendLine("=======================");
        builder.AppendLine(english
            ? $"Original file: {OriginalFilePath}"
            : $"Fichier original : {OriginalFilePath}");
        builder.AppendLine(english
            ? $"Last saved file: {LastSavedPath ?? "none"}"
            : $"Dernier fichier sauvegardé : {LastSavedPath ?? "aucun"}");
        builder.AppendLine();
        builder.AppendLine(english ? "Counts" : "Compteurs");
        builder.AppendLine("--------");
        builder.AppendLine($"Devices : {Devices.Count}");
        builder.AppendLine(english
            ? $"TX channels: {Devices.Sum(device => device.TxCount)}"
            : $"Canaux TX : {Devices.Sum(device => device.TxCount)}");
        builder.AppendLine(english
            ? $"RX channels: {Devices.Sum(device => device.RxCount)}"
            : $"Canaux RX : {Devices.Sum(device => device.RxCount)}");
        builder.AppendLine(english
            ? $"Active subscriptions: {PatchMatrix.ActivePatchCount}"
            : $"Patchs actifs : {PatchMatrix.ActivePatchCount}");
        builder.AppendLine(english
            ? $"Modified subscriptions: {_modifiedRxElements.Count}"
            : $"Patchs modifiés : {_modifiedRxElements.Count}");
        builder.AppendLine();
        AppendImportantWarnings(builder, BuildImportantWarningDetails(), language);
        builder.AppendLine();
        builder.AppendLine("Validation");
        builder.AppendLine("----------");
        builder.AppendLine(validation.ToDisplayText(language));
        builder.AppendLine();
        builder.AppendLine(DanteXmlChangeGuardService.BuildGuardReport(ValidateXmlChangeGuard(), language));
        builder.AppendLine();
        AppendChangeTable(builder, language);

        return builder.ToString();
    }

    public string BuildReportText(UiLanguage language = UiLanguage.French)
    {
        bool english = language == UiLanguage.English;
        StringBuilder builder = new();
        DanteValidationResult validation = Validate();

        builder.AppendLine(english ? "DANTE CONFIG EDITOR - REPORT" : "DANTE CONFIG EDITOR - RAPPORT");
        builder.AppendLine("=============================");
        builder.AppendLine($"Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        builder.AppendLine(english ? $"File: {OriginalFilePath}" : $"Fichier : {OriginalFilePath}");
        builder.AppendLine(english
            ? $"Status: {(IsModified ? "Modified - not saved" : "Unmodified")}"
            : $"Statut : {(IsModified ? "Modifié non sauvegardé" : "Non modifié")}");
        builder.AppendLine();
        builder.AppendLine(english ? "Summary" : "Synthèse");
        builder.AppendLine("--------");
        builder.AppendLine($"Devices : {Devices.Count}");
        builder.AppendLine(english
            ? $"TX channels: {Devices.Sum(device => device.TxCount)}"
            : $"Canaux TX : {Devices.Sum(device => device.TxCount)}");
        builder.AppendLine(english
            ? $"RX channels: {Devices.Sum(device => device.RxCount)}"
            : $"Canaux RX : {Devices.Sum(device => device.RxCount)}");
        builder.AppendLine(english
            ? $"Active subscriptions: {PatchMatrix.ActivePatchCount}"
            : $"Patchs actifs : {PatchMatrix.ActivePatchCount}");
        builder.AppendLine(english
            ? $"Conflicts: {PatchMatrix.ConflictCount}"
            : $"Conflits : {PatchMatrix.ConflictCount}");
        builder.AppendLine();
        AppendImportantWarnings(builder, BuildImportantWarningDetails(), language);
        builder.AppendLine();

        builder.AppendLine("Validation");
        builder.AppendLine("----------");
        builder.AppendLine(validation.ToDisplayText(language));
        builder.AppendLine();
        builder.AppendLine(BuildCompatibilityReport(language));
        builder.AppendLine();

        builder.AppendLine("Devices");
        builder.AppendLine("-------");
        AppendTableHeader(
            builder,
            "Device",
            english ? "Network" : "Réseau",
            english ? "Latency" : "Latence",
            "TX/RX");
        foreach (DanteDevice device in Devices)
        {
            string network = english
                ? device.IsRedundant ? "Redundant" : "Daisy-chain"
                : device.NetworkMode;
            AppendTableRow(builder, device.Name, network, DanteLatencyFormatter.FormatLatencyWithXmlValue(device.Latency), $"{device.TxCount}/{device.RxCount}");
        }

        builder.AppendLine();
        builder.AppendLine(english ? "Active subscriptions and conflicts" : "Patchs actifs et conflits");
        builder.AppendLine("-------------------------");
        AppendTableHeader(builder, "RX", "TX", english ? "TX channel" : "Canal TX", english ? "Status" : "État");
        foreach (DanteSubscription subscription in PatchMatrix.Subscriptions.Where(subscription => subscription.IsActive || subscription.IsConflict))
        {
            AppendTableRow(
                builder,
                $"{subscription.RxDevice} / {subscription.RxChannelName}",
                string.IsNullOrWhiteSpace(subscription.TxDevice) ? "-" : subscription.TxDevice,
                string.IsNullOrWhiteSpace(subscription.TxChannelName) ? "-" : subscription.TxChannelName,
                LocalizationService.TranslateLiteral(language, subscription.Status));
        }

        builder.AppendLine();
        AppendChangeTable(builder, language);

        return builder.ToString();
    }

    public string BuildPatchbookText(
        string scope,
        string? scopeDisplay = null,
        UiLanguage language = UiLanguage.French)
    {
        bool english = language == UiLanguage.English;
        DanteValidationResult validation = Validate();
        IEnumerable<DanteSubscription> subscriptions = PatchMatrix.Subscriptions;
        subscriptions = scope switch
        {
            "Filter.ActivePatches" => subscriptions.Where(subscription => subscription.IsActive),
            "Filter.WarningsConflicts" => subscriptions.Where(subscription => subscription.IsWarning || subscription.IsConflict),
            "Patchs actifs" => subscriptions.Where(subscription => subscription.IsActive),
            "Warnings / conflits" => subscriptions.Where(subscription => subscription.IsWarning || subscription.IsConflict),
            _ => subscriptions
        };

        DanteSubscription[] rows = subscriptions
            .OrderBy(subscription => subscription.RxDevice, StringComparer.OrdinalIgnoreCase)
            .ThenBy(subscription => subscription.RxDanteId)
            .ToArray();

        StringBuilder builder = new();
        builder.AppendLine("DANTE CONFIG EDITOR - PATCHBOOK");
        builder.AppendLine("===============================");
        builder.AppendLine($"Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        builder.AppendLine(english
            ? $"Source file name: {Path.GetFileName(OriginalFilePath)}"
            : $"Nom du fichier source : {Path.GetFileName(OriginalFilePath)}");
        builder.AppendLine(english
            ? $"Source file path: {OriginalFilePath}"
            : $"Chemin du fichier source : {OriginalFilePath}");
        builder.AppendLine($"Preset : {PresetName}");
        builder.AppendLine(english
            ? $"Preset version: {LocalizedBlank(PresetVersion, language)}"
            : $"Version preset : {LocalizedBlank(PresetVersion, language)}");
        builder.AppendLine($"Scope : {scopeDisplay ?? scope}");
        builder.AppendLine($"Devices : {Devices.Count}");
        builder.AppendLine($"TX total : {Devices.Sum(device => device.TxCount)}");
        builder.AppendLine($"RX total : {Devices.Sum(device => device.RxCount)}");
        builder.AppendLine(english
            ? $"Active subscriptions: {PatchMatrix.ActivePatchCount}"
            : $"Patchs actifs : {PatchMatrix.ActivePatchCount}");
        builder.AppendLine(english
            ? $"Free RX channels: {PatchMatrix.FreeRxCount}"
            : $"RX libres : {PatchMatrix.FreeRxCount}");
        builder.AppendLine(english
            ? $"Local subscriptions: {PatchMatrix.LocalPatchCount}"
            : $"Patchs locaux : {PatchMatrix.LocalPatchCount}");
        builder.AppendLine($"Warnings : {validation.Warnings.Count}");
        builder.AppendLine(english
            ? $"Blocking errors: {validation.Errors.Count}"
            : $"Erreurs bloquantes : {validation.Errors.Count}");
        builder.AppendLine();

        foreach (IGrouping<string, DanteSubscription> group in rows.GroupBy(subscription => subscription.RxDevice))
        {
            builder.AppendLine(group.Key);
            builder.AppendLine(new string('-', Math.Max(8, group.Key.Length)));

            foreach (DanteSubscription subscription in group)
            {
                string source = subscription.IsActive
                    ? subscription.SourceFull
                    : english ? "(free)" : "(libre)";

                builder.AppendLine(
                    $"RX {subscription.RxDanteId.ToString().PadLeft(3, '0')} | "
                    + $"{TrimForPatchbook(subscription.RxChannelName),-28} <- "
                    + $"{TrimForPatchbook(source),-48} | "
                    + LocalizationService.TranslateLiteral(language, subscription.TypeLabel));
            }

            builder.AppendLine();
        }

        if (rows.Length == 0)
        {
            builder.AppendLine(english
                ? "No row matches this export filter."
                : "Aucune ligne à exporter avec ce filtre.");
        }

        return builder.ToString();
    }

    public string BuildPatchbookCsv(
        string scope,
        UiLanguage language = UiLanguage.French)
    {
        IEnumerable<DanteSubscription> subscriptions = PatchMatrix.Subscriptions;
        subscriptions = scope switch
        {
            "Filter.ActivePatches" => subscriptions.Where(subscription => subscription.IsActive),
            "Filter.WarningsConflicts" => subscriptions.Where(subscription => subscription.IsWarning || subscription.IsConflict),
            "Patchs actifs" => subscriptions.Where(subscription => subscription.IsActive),
            "Warnings / conflits" => subscriptions.Where(subscription => subscription.IsWarning || subscription.IsConflict),
            _ => subscriptions
        };

        StringBuilder builder = new();
        builder.AppendLine("\"RxDevice\",\"Rx Dante Id\",\"RxChannel\",\"TxDevice\",\"TxChannel\",\"Type\",\"Status\"");
        foreach (DanteSubscription subscription in subscriptions.OrderBy(subscription => subscription.RxDevice, StringComparer.OrdinalIgnoreCase).ThenBy(subscription => subscription.RxDanteId))
        {
            string txDevice = subscription.IsLocalSubscription ? "LOCAL" : subscription.DisplayTxDeviceName;
            builder.AppendLine(string.Join(",",
                Csv(subscription.RxDevice),
                subscription.RxDanteId.ToString(),
                Csv(subscription.RxChannelName),
                Csv(txDevice),
                Csv(subscription.TxChannelName),
                Csv(LocalizationService.TranslateLiteral(language, subscription.TypeLabel)),
                Csv(LocalizationService.TranslateLiteral(language, subscription.Status))));
        }

        return builder.ToString();
    }

    public string BuildCompatibilityReport(UiLanguage language = UiLanguage.French)
    {
        bool english = language == UiLanguage.English;
        DanteValidationResult validation = Validate();
        DanteValidationResult compatibility = DanteXmlCompatibilityService.ValidateCompatibility(Document, _originalCompatibilityProfile);
        DanteValidationResult guard = ValidateXmlChangeGuard();

        StringBuilder builder = new();
        builder.AppendLine(english ? "XML compatibility" : "Compatibilité XML");
        builder.AppendLine("-----------------");
        builder.AppendLine(Document.Root?.Name.LocalName == "preset"
            ? english ? "OK <preset> root present" : "OK Racine <preset>"
            : english ? "ERROR <preset> root missing or modified" : "ERROR Racine <preset> absente ou modifiée");
        builder.AppendLine(!string.IsNullOrWhiteSpace(PresetVersion)
            ? english ? "OK Preset version preserved" : "OK Version du preset conservée"
            : english ? "WARNING Preset version missing" : "WARNING Version du preset absente");
        builder.AppendLine(compatibility.Errors.Any(error => error.Contains("nombre de devices", StringComparison.OrdinalIgnoreCase))
            ? english ? "ERROR Devices modified" : "ERROR Devices modifiés"
            : english ? "OK Devices preserved" : "OK Devices conservés");
        builder.AppendLine(compatibility.Errors.Any(error => error.Contains("canaux TX", StringComparison.OrdinalIgnoreCase))
            ? english ? "ERROR TX channels modified" : "ERROR TX modifiés"
            : english ? "OK TX channels preserved" : "OK TX conservés");
        builder.AppendLine(compatibility.Errors.Any(error => error.Contains("canaux RX", StringComparison.OrdinalIgnoreCase))
            ? english ? "ERROR RX channels modified" : "ERROR RX modifiés"
            : english ? "OK RX channels preserved" : "OK RX conservés");
        builder.AppendLine(HasCompatibilityError(compatibility, "dante", "TX")
            ? english ? "ERROR Missing or modified TX Dante ID" : "ERROR Dante Id TX manquant ou modifié"
            : english ? "OK All TX Dante IDs are present" : "OK Tous les Dante Id TX sont présents");
        builder.AppendLine(HasCompatibilityError(compatibility, "dante", "RX")
            ? english ? "ERROR Missing or modified RX Dante ID" : "ERROR Dante Id RX manquant ou modifié"
            : english ? "OK All RX Dante IDs are present" : "OK Tous les Dante Id RX sont présents");
        builder.AppendLine(compatibility.Errors.Any(error => error.Contains("mediaType", StringComparison.OrdinalIgnoreCase) && error.Contains("TX", StringComparison.OrdinalIgnoreCase))
            ? english ? "ERROR Missing or modified TX mediaType" : "ERROR mediaType TX manquant ou modifié"
            : english ? "OK All TX mediaType values are present" : "OK Tous les mediaType TX sont présents");
        builder.AppendLine(compatibility.Errors.Any(error => error.Contains("mediaType", StringComparison.OrdinalIgnoreCase) && error.Contains("RX", StringComparison.OrdinalIgnoreCase))
            ? english ? "ERROR Missing or modified RX mediaType" : "ERROR mediaType RX manquant ou modifié"
            : english ? "OK All RX mediaType values are present" : "OK Tous les mediaType RX sont présents");
        builder.AppendLine(compatibility.Errors.Any(error => error.Contains("Balise technique", StringComparison.OrdinalIgnoreCase))
            ? english ? "ERROR Main technical elements modified" : "ERROR Balises techniques principales modifiées"
            : english ? "OK Main technical elements preserved" : "OK Balises techniques principales conservées");
        builder.AppendLine(guard.HasErrors
            ? english ? "ERROR Forbidden change detected" : "ERROR Changement interdit détecté"
            : english ? "OK No forbidden change detected" : "OK Aucun changement interdit détecté");
        builder.AppendLine(english
            ? $"WARNING Referenced TX devices missing from the preset: {PatchMatrix.ExternalMissingDeviceCount}"
            : $"WARNING Devices TX référencés mais absents du preset : {PatchMatrix.ExternalMissingDeviceCount}");
        builder.AppendLine(english
            ? $"WARNING Referenced TX channels missing: {PatchMatrix.MissingTxChannelCount}"
            : $"WARNING Canaux TX référencés mais absents : {PatchMatrix.MissingTxChannelCount}");
        builder.AppendLine(english
            ? $"Non-blocking warnings: {validation.Warnings.Count}"
            : $"Warnings non bloquants : {validation.Warnings.Count}");
        builder.AppendLine(english
            ? $"Blocking errors: {validation.Errors.Count}"
            : $"Erreurs bloquantes : {validation.Errors.Count}");
        builder.AppendLine();
        builder.AppendLine(DanteXmlChangeGuardService.BuildGuardReport(guard, language));
        return builder.ToString();
    }

    public string BuildTopologyText(UiLanguage language = UiLanguage.French)
    {
        bool english = language == UiLanguage.English;
        DanteSubscription[] activeSubscriptions = PatchMatrix.Subscriptions
            .Where(subscription => subscription.IsActive)
            .ToArray();

        StringBuilder builder = new();
        builder.AppendLine(english ? "SIMPLE TOPOLOGY" : "TOPOLOGIE SIMPLE");
        builder.AppendLine("================");
        builder.AppendLine();
        builder.AppendLine(english ? "Most used sources" : "Sources les plus utilisées");
        builder.AppendLine("--------------------------");
        foreach (IGrouping<string, DanteSubscription> group in activeSubscriptions.GroupBy(subscription => subscription.IsLocalSubscription ? "LOCAL" : subscription.DisplayTxDeviceName).OrderByDescending(group => group.Count()).Take(20))
        {
            builder.AppendLine($"{Blank(group.Key),-30} -> {group.Count()} RX");
        }

        builder.AppendLine();
        builder.AppendLine(english ? "Most patched receivers" : "Receivers les plus patchés");
        builder.AppendLine("--------------------------");
        foreach (IGrouping<string, DanteSubscription> group in activeSubscriptions.GroupBy(subscription => subscription.RxDevice).OrderByDescending(group => group.Count()).Take(20))
        {
            builder.AppendLine(english
                ? $"{group.Key,-30} -> {group.Count()} active RX"
                : $"{group.Key,-30} -> {group.Count()} RX actifs");
        }

        builder.AppendLine();
        builder.AppendLine(english ? "TX -> RX relationships" : "Relations TX -> RX");
        builder.AppendLine("------------------");
        foreach (IGrouping<string, DanteSubscription> sourceGroup in activeSubscriptions.GroupBy(subscription => subscription.IsLocalSubscription ? "LOCAL" : subscription.DisplayTxDeviceName).OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase).Take(80))
        {
            builder.AppendLine(Blank(sourceGroup.Key));
            foreach (IGrouping<string, DanteSubscription> rxGroup in sourceGroup.GroupBy(subscription => subscription.RxDevice).OrderByDescending(group => group.Count()).Take(20))
            {
                builder.AppendLine(english
                    ? $"  -> {rxGroup.Key}: {rxGroup.Count()} subscriptions"
                    : $"  -> {rxGroup.Key} : {rxGroup.Count()} patchs");
            }
        }

        return builder.ToString();
    }

    public string ListRedundantDevices(UiLanguage language = UiLanguage.French)
    {
        return BuildDeviceList(
            Devices.Where(device => device.IsRedundant),
            language == UiLanguage.English ? "No redundant device found." : "Aucune machine redondante trouvée.");
    }

    public string ListDaisychainDevices(UiLanguage language = UiLanguage.French)
    {
        return BuildDeviceList(
            Devices.Where(device => !device.IsRedundant),
            language == UiLanguage.English ? "No device in daisy-chain mode found." : "Aucune machine en daisychain trouvée.");
    }

    public string ListLatencies(UiLanguage language = UiLanguage.French)
    {
        List<string> lines = Devices
            .Where(device => !string.IsNullOrWhiteSpace(device.Latency))
            .Select(device => $"{device.Name}: {DanteLatencyFormatter.FormatLatencyWithXmlValue(device.Latency)}")
            .ToList();

        return lines.Count > 0
            ? string.Join(Environment.NewLine, lines)
            : language == UiLanguage.English ? "No latency defined." : "Aucune latence définie.";
    }

    public string ListSamplerates(UiLanguage language = UiLanguage.French)
    {
        List<string> lines = Devices
            .Where(device => !string.IsNullOrWhiteSpace(device.Samplerate))
            .Select(device => $"{device.Name}: {FormatSamplerateForDisplay(device.Samplerate)}")
            .ToList();

        return lines.Count > 0
            ? string.Join(Environment.NewLine, lines)
            : language == UiLanguage.English ? "No sample rate defined." : "Aucune sample rate définie.";
    }

    public string ListEncodings(UiLanguage language = UiLanguage.French)
    {
        List<string> lines = Devices
            .Where(device => !string.IsNullOrWhiteSpace(device.Encoding))
            .Select(device => $"{device.Name}: {FormatEncodingForDisplay(device.Encoding)}")
            .ToList();

        return lines.Count > 0
            ? string.Join(Environment.NewLine, lines)
            : language == UiLanguage.English ? "No bit depth defined." : "Aucun encodage défini.";
    }

    public string ListStaticIpDevices(UiLanguage language = UiLanguage.French)
    {
        DanteDevice[] staticIpDevices = Devices.Where(device => device.UsesStaticIp).ToArray();
        if (staticIpDevices.Length == 0)
        {
            return language == UiLanguage.English ? "No static IP detected." : "Aucune IP fixe détectée.";
        }

        return string.Join(Environment.NewLine, staticIpDevices.Select(FormatStaticIpDevice));
    }

    public string ListPreferredMasters(UiLanguage language = UiLanguage.French)
    {
        return BuildDeviceList(
            Devices.Where(device => device.PreferredMaster),
            language == UiLanguage.English ? "No Preferred Master device found." : "Aucune machine preferred master trouvée.");
    }

    private static bool HasCompatibilityError(DanteValidationResult compatibility, string firstNeedle, string secondNeedle)
    {
        return compatibility.Errors.Any(error =>
            error.Contains(firstNeedle, StringComparison.OrdinalIgnoreCase)
            && error.Contains(secondNeedle, StringComparison.OrdinalIgnoreCase));
    }

    private void AppendChangeTable(
        StringBuilder builder,
        UiLanguage language)
    {
        bool english = language == UiLanguage.English;
        builder.AppendLine(english ? "Changes" : "Modifications");
        builder.AppendLine("-------------");

        if (_changes.Count == 0)
        {
            builder.AppendLine(english
                ? "- No change since the file was loaded."
                : "- Aucune modification depuis le chargement.");
        }
        else
        {
            AppendTableHeader(
                builder,
                english ? "Time" : "Heure",
                "Action",
                english ? "Details" : "Détail",
                "");
            foreach (ChangeRecord change in _changes)
            {
                AppendTableRow(
                    builder,
                    change.Timestamp.ToString("HH:mm:ss"),
                    LocalizationService.TranslateLiteral(language, change.Action),
                    LocalizationService.TranslateHistoryDetail(language, change.Details),
                    "");
            }
        }
    }

    private static void AppendImportantWarnings(
        StringBuilder builder,
        IReadOnlyList<DanteImportantWarning> warnings,
        UiLanguage language)
    {
        if (warnings.Count == 0)
        {
            return;
        }

        bool english = language == UiLanguage.English;
        builder.AppendLine(english
            ? "!!! IMPORTANT ITEMS TO CHECK !!!"
            : "!!! POINTS À VÉRIFIER IMPORTANTS !!!");
        builder.AppendLine("------------------------------------");
        foreach (DanteImportantWarning warning in warnings)
        {
            builder.AppendLine("- " + warning.LocalizedMessage(english));
        }
    }

    private static void AppendTableHeader(StringBuilder builder, string first, string second, string third, string fourth)
    {
        builder.AppendLine($"{TrimForColumn(first),-18} | {TrimForColumn(second),-22} | {TrimForColumn(third),-36} | {TrimForColumn(fourth),-18}");
        builder.AppendLine(new string('-', 103));
    }

    private static void AppendTableRow(StringBuilder builder, string first, string second, string third, string fourth)
    {
        builder.AppendLine($"{TrimForColumn(first),-18} | {TrimForColumn(second),-22} | {TrimForColumn(third),-36} | {TrimForColumn(fourth),-18}");
    }

    private static string TrimForColumn(string value)
    {
        string cleanValue = value.ReplaceLineEndings(" ").Trim();
        return cleanValue.Length <= 34 ? cleanValue : cleanValue[..31] + "...";
    }

    private static string TrimForPatchbook(string value)
    {
        string cleanValue = value.ReplaceLineEndings(" ").Trim();
        return cleanValue.Length <= 46 ? cleanValue : cleanValue[..43] + "...";
    }

    private static string Csv(string value)
    {
        string cleanValue = value.Replace("\"", "\"\"", StringComparison.Ordinal);
        return $"\"{cleanValue}\"";
    }

    private static string Blank(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "(vide)" : value;
    }

    private static string LocalizedBlank(string value, UiLanguage language)
    {
        return string.IsNullOrWhiteSpace(value)
            ? language == UiLanguage.English ? "(empty)" : "(vide)"
            : value;
    }

    private static string FormatStaticIpDevice(DanteDevice device)
    {
        return string.IsNullOrWhiteSpace(device.StaticIpAddress)
            ? device.Name
            : $"{device.Name} ({device.StaticIpAddress})";
    }

    private static string BuildDeviceList(IEnumerable<DanteDevice> devices, string emptyMessage)
    {
        List<string> names = devices.Select(device => device.Name).Where(name => !string.IsNullOrWhiteSpace(name)).ToList();
        return names.Count > 0 ? string.Join(Environment.NewLine, names) : emptyMessage;
    }
}
