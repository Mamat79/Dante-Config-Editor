using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DanteConfigEditor.Application.Navigation;
using DanteConfigEditor.Domain.Projects;
using DanteConfigEditor.Domain.Validation;
using DanteConfigEditor.Models;
using DanteConfigEditor.Services;
using Microsoft.Win32;

namespace DanteConfigEditor;

public partial class MainWindow
{
    private sealed class ValidationCenterRow
    {
        public ValidationCenterRow(
            ProjectValidationIssue issue,
            string severityLabel,
            string categoryLabel,
            string targetLabel,
            string message,
            string suggestedAction)
        {
            Issue = issue;
            SeverityLabel = severityLabel;
            CategoryLabel = categoryLabel;
            TargetLabel = targetLabel;
            Message = message;
            SuggestedAction = suggestedAction;
        }

        public ProjectValidationIssue Issue { get; }

        public string SeverityLabel { get; }

        public string CategoryLabel { get; }

        public string TargetLabel { get; }

        public string Message { get; }

        public string TechnicalDetail => Issue.TechnicalDetail;

        public string XmlPath => Issue.XmlPath ?? string.Empty;

        public string SuggestedAction { get; }
    }

    private ValidationCenterRow CreateValidationCenterRow(
        ProjectValidationIssue issue)
    {
        string severity = issue.Severity switch
        {
            ProjectValidationSeverity.Error => T("Validation.Severity.Error"),
            ProjectValidationSeverity.Warning => T("Validation.Severity.Warning"),
            _ => T("Validation.Severity.Information")
        };
        string categoryKey = $"Validation.Category.{issue.Category}";
        string category = T(categoryKey);
        if (string.Equals(category, categoryKey, StringComparison.Ordinal))
        {
            category = issue.Category;
        }

        string message = LocalizedValidationMessage(issue);
        string action = string.IsNullOrWhiteSpace(issue.SuggestedActionKey)
            ? T("Validation.Action.None")
            : T(issue.SuggestedActionKey);
        return new ValidationCenterRow(
            issue,
            severity,
            category,
            issue.Target?.DisplayName ?? _project?.PresetName ?? T("Blank"),
            message,
            action);
    }

    private string LocalizedValidationMessage(ProjectValidationIssue issue)
    {
        string localized = T(issue.MessageKey);
        bool keyWasFound = !string.Equals(
            localized,
            issue.MessageKey,
            StringComparison.Ordinal);

        if (issue.MessageKey.StartsWith(
                "Validation.Legacy.",
                StringComparison.Ordinal))
        {
            // Les validateurs historiques produisent encore leurs détails en
            // français. En français, ce texte exact reste le meilleur message
            // humain ; en anglais, le résumé est traduit et le détail brut est
            // conservé séparément pour le diagnostic.
            return _language == UiLanguage.French
                ? issue.TechnicalDetail
                : localized;
        }

        return keyWasFound ? localized : issue.TechnicalDetail;
    }

    private static bool MatchesHealthFilter(
        ProjectValidationIssue issue,
        string filter)
    {
        return filter switch
        {
            "Filter.Info" =>
                issue.Severity == ProjectValidationSeverity.Information,
            "Filter.HealthWarnings" =>
                issue.Severity == ProjectValidationSeverity.Warning,
            "Filter.Errors" =>
                issue.Severity == ProjectValidationSeverity.Error,
            "Filter.Patches" => issue.Category == "Patch",
            "Filter.Devices" =>
                issue.Category is "Device" or "Channel",
            "Filter.Clock" => issue.Category == "Clock",
            "Filter.Network" => issue.Category == "Network",
            "Filter.XmlCompatibility" =>
                issue.Category is "XmlCompatibility"
                    or "SaveSafety"
                    or "XmlProfile"
                    or "Capabilities",
            _ => true
        };
    }

    private static bool MatchesValidationSearch(
        ProjectValidationIssue issue,
        string search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return true;
        }

        return Contains(issue.Category, search)
            || Contains(issue.MessageKey, search)
            || Contains(issue.TechnicalDetail, search)
            || Contains(issue.Target?.DisplayName ?? string.Empty, search)
            || Contains(issue.XmlPath ?? string.Empty, search);
    }

    private void HealthSearchTextBox_TextChanged(
        object sender,
        TextChangedEventArgs e)
    {
        if (!_refreshingUi)
        {
            RefreshHealthPage();
        }
    }

    private void HealthIssuesGrid_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (HealthIssuesGrid.SelectedItem is not ValidationCenterRow row)
        {
            ValidationDetailTitleTextBlock.Text = LocalizeLiteral(
                "Détail du résultat");
            ValidationDetailTextBlock.Text = T("Validation.Center.NoSelection");
            ValidationSuggestedActionTextBlock.Text = string.Empty;
            OpenValidationTargetButton.IsEnabled = false;
            return;
        }

        ValidationDetailTitleTextBlock.Text =
            $"{row.SeverityLabel} · {row.CategoryLabel}";
        StringBuilder detail = new();
        detail.AppendLine(row.Message);
        if (!string.Equals(
                row.Message,
                row.TechnicalDetail,
                StringComparison.Ordinal))
        {
            detail.AppendLine();
            detail.AppendLine(row.TechnicalDetail);
        }

        if (!string.IsNullOrWhiteSpace(row.XmlPath))
        {
            detail.AppendLine();
            detail.Append("XML : ");
            detail.Append(row.XmlPath);
        }

        ValidationDetailTextBlock.Text = detail.ToString();
        ValidationSuggestedActionTextBlock.Text = row.SuggestedAction;
        OpenValidationTargetButton.IsEnabled = CanNavigateToValidationTarget(
            row.Issue.Target);

        if (_projectSession.HasProject && row.Issue.Target is not null)
        {
            _projectSession.SetSelection(new ProjectSelection(
                [row.Issue.Target]));
        }
    }

    private void HealthIssuesGrid_MouseDoubleClick(
        object sender,
        MouseButtonEventArgs e)
    {
        OpenSelectedValidationTarget();
    }

    private void OpenValidationTargetButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        OpenSelectedValidationTarget();
    }

    private void OpenSelectedValidationTarget()
    {
        if (_project is null
            || HealthIssuesGrid.SelectedItem is not ValidationCenterRow row
            || row.Issue.Target is not ProjectEntityReference target
            || !CanNavigateToValidationTarget(target))
        {
            return;
        }

        DanteDevice? device = target.Kind == ProjectEntityKind.Device
            ? _project.FindDeviceByStableIdentity(target.StableId)
            : _project.FindDeviceByStableIdentity(target.ParentStableId);
        if (device is null)
        {
            SetStatus(_language == UiLanguage.English
                ? "The affected device is no longer present."
                : "La machine concernée n’est plus présente.");
            return;
        }

        if (target.Kind == ProjectEntityKind.Subscription)
        {
            OpenValidationSubscription(device, target);
            return;
        }

        _workspaceNavigation.NavigateTo(WorkspaceSection.Machines);
        DeviceComboBox.SelectedItem = device.Name;
        SelectDeviceInQuickList(device.Name);

        if (target.Kind is ProjectEntityKind.TxChannel
            or ProjectEntityKind.RxChannel
            && TryReadTargetDanteId(target.StableId, out int danteId))
        {
            ChannelKindComboBox.SelectedItem =
                target.Kind == ProjectEntityKind.TxChannel ? "TX" : "RX";
            RefreshChannelSelector();
            ChannelComboBox.SelectedItem =
                (ChannelComboBox.ItemsSource as IEnumerable<ChannelChoice>)
                ?.FirstOrDefault(channel => channel.Index == danteId);
            ChannelComboBox.Focus();
        }
    }

    private void OpenValidationSubscription(
        DanteDevice device,
        ProjectEntityReference target)
    {
        if (!TryReadTargetDanteId(target.StableId, out int rxDanteId))
        {
            return;
        }

        _workspaceNavigation.NavigateTo(WorkspaceSection.Patch);
        ShowPatchWorkspaceMode(PatchWorkspaceDisplayMode.List);
        DeviceComboBox.SelectedItem = device.Name;
        SelectDeviceInQuickList(device.Name);
        PatchListModeButton.IsChecked = true;
        PatchSearchTextBox.Text = string.Empty;
        ReceiverDeviceList.SelectedItem = device.Name;
        PatchStateFilterComboBox.SelectedItem =
            (PatchStateFilterComboBox.ItemsSource as IEnumerable<LocalizedOption>)
            ?.FirstOrDefault(option => option.Key == "Filter.AllRx");
        RefreshPatchRows();

        DanteSubscription? subscription = _patchRows.FirstOrDefault(
            candidate =>
                string.Equals(
                    candidate.RxDevice,
                    device.Name,
                    StringComparison.OrdinalIgnoreCase)
                && candidate.RxDanteId == rxDanteId);
        if (subscription is not null)
        {
            PatchGrid.SelectedItem = subscription;
            PatchGrid.ScrollIntoView(subscription);
            PatchGrid.Focus();
        }
    }

    private void SelectDeviceInQuickList(string deviceName)
    {
        DeviceFilterComboBox.SelectedItem =
            (DeviceFilterComboBox.ItemsSource as IEnumerable<LocalizedOption>)
            ?.FirstOrDefault(option => option.Key == "DeviceFilter.All");
        DeviceRow? row = _deviceRows.FirstOrDefault(candidate =>
            string.Equals(
                candidate.Name,
                deviceName,
                StringComparison.OrdinalIgnoreCase));
        if (row is null)
        {
            return;
        }

        DeviceGrid.SelectedItem = row;
        DeviceGrid.ScrollIntoView(row);
    }

    private static bool CanNavigateToValidationTarget(
        ProjectEntityReference? target) =>
        target?.Kind is ProjectEntityKind.Device
            or ProjectEntityKind.TxChannel
            or ProjectEntityKind.RxChannel
            or ProjectEntityKind.Subscription;

    private static bool TryReadTargetDanteId(
        string stableId,
        out int danteId)
    {
        danteId = 0;
        int separator = stableId.LastIndexOf(':');
        return separator >= 0
            && int.TryParse(stableId[(separator + 1)..], out danteId);
    }

    private void ExportValidationReportButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (!EnsureProjectLoaded())
        {
            return;
        }

        SaveFileDialog dialog = new()
        {
            Filter = T("Dialog.TxtFilter"),
            DefaultExt = ".txt",
            AddExtension = true,
            FileName =
                $"{Path.GetFileNameWithoutExtension(_project!.OriginalFilePath)}"
                + "_validation-DCE.txt",
            InitialDirectory = Path.GetDirectoryName(_project.OriginalFilePath),
            Title = _language == UiLanguage.English
                ? "Export validation report"
                : "Exporter le rapport de validation"
        };
        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        try
        {
            ReportExportService.ExportText(
                dialog.FileName,
                BuildValidationReport());
            AddLog($"{T("Validation.Report.Exported")} {dialog.FileName}");
            SetStatus(T("Validation.Report.Exported"));
        }
        catch (Exception ex)
        {
            ShowError(
                _language == UiLanguage.English
                    ? "Validation report unavailable"
                    : "Rapport de validation impossible",
                ex);
        }
    }

    private string BuildValidationReport()
    {
        if (_project is null || !_projectSession.HasProject)
        {
            return T("Status.NoFileLoaded");
        }

        ProjectValidationState validation = _projectSession.Validation;
        StringBuilder report = new();
        report.AppendLine(T("Validation.Report.Title"));
        report.AppendLine(new string('=', T("Validation.Report.Title").Length));
        report.AppendLine();
        report.AppendLine($"{(_language == UiLanguage.English ? "File" : "Fichier")} : {_project.OriginalFilePath}");
        report.AppendLine($"{(_language == UiLanguage.English ? "Date" : "Date")} : {validation.ValidatedAt.ToLocalTime():G}");
        report.AppendLine(Tf(
            "Validation.Center.Profile",
            _projectSession.Profile.Id,
            LocalizedRecognitionLevel(_projectSession.Profile.RecognitionLevel),
            LocalizedAccessMode(_projectSession.Profile.AccessMode),
            Blank(_projectSession.Profile.PresetVersion)));
        report.AppendLine(Tf(
            "Validation.Center.Summary",
            validation.Issues.Count,
            validation.ErrorCount,
            validation.WarningCount,
            validation.InformationCount));
        report.AppendLine();
        report.AppendLine(T("Validation.Report.Internal"));
        report.AppendLine(new string('-', T("Validation.Report.Internal").Length));

        foreach (ProjectValidationIssue issue in validation.Issues)
        {
            ValidationCenterRow row = CreateValidationCenterRow(issue);
            report.AppendLine(
                $"[{row.SeverityLabel}] {row.CategoryLabel} - {row.TargetLabel}");
            report.AppendLine(row.Message);
            if (!string.Equals(
                    row.Message,
                    row.TechnicalDetail,
                    StringComparison.Ordinal))
            {
                report.AppendLine(
                    $"{(_language == UiLanguage.English ? "Technical detail" : "Détail technique")} : "
                    + row.TechnicalDetail);
            }

            if (!string.IsNullOrWhiteSpace(row.XmlPath))
            {
                report.AppendLine($"XML : {row.XmlPath}");
            }

            report.AppendLine(
                $"{(_language == UiLanguage.English ? "Suggested action" : "Action suggérée")} : "
                + row.SuggestedAction);
            report.AppendLine();
        }

        report.AppendLine(T("Validation.Report.External"));
        report.AppendLine(new string('-', T("Validation.Report.External").Length));
        report.AppendLine(T("Validation.Report.ExternalDetail"));
        return report.ToString();
    }

    private string LocalizedRecognitionLevel(
        DanteXmlRecognitionLevel recognitionLevel) =>
        T($"Validation.Recognition.{recognitionLevel}");

    private string LocalizedAccessMode(ProjectAccessMode accessMode) =>
        T($"Validation.Access.{accessMode}");
}
