using System.Xml.Linq;
using DanteConfigEditor.Services;

namespace DanteConfigEditor.Models;

public sealed partial class DanteProject
{
    private readonly List<AuthorizedDeviceAdditionState> _authorizedDeviceAdditions = [];

    public MachineCloneResult DuplicateDevice(string sourceDeviceName, MachineCloneOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        DanteDevice source = FindDevice(sourceDeviceName)
            ?? throw new InvalidOperationException("La machine source est introuvable.");
        string newName = ValidateNewRoleName(options.NewName);

        MachineRoleCreation creation = MachineRoleInstantiationService.CreateClone(
            source.Element,
            new MachineCloneOptions
            {
                NewName = newName,
                PreserveTxLabels = options.PreserveTxLabels,
                PreserveRxLabels = options.PreserveRxLabels,
                PreserveDeviceSettings = options.PreserveDeviceSettings,
                PreserveNetworkConfiguration = options.PreserveNetworkConfiguration,
                PreserveSubscriptions = options.PreserveSubscriptions,
                PreserveMulticastFlows = options.PreserveMulticastFlows,
                PreservePreferredMaster = options.PreservePreferredMaster
            });

        EnsureStructuralCandidateIsValid([creation.DeviceElement]);
        Document.Root!.Add(creation.DeviceElement);
        AuthorizeAddedDevice(creation.DeviceElement);
        RegisterChange(
            "Machine dupliquée",
            $"{source.Name} -> {newName}; identité matérielle neutralisée; "
            + $"{creation.TxCount} TX, {creation.RxCount} RX, {creation.CopiedSubscriptionCount} abonnement(s) recopié(s)");

        return new MachineCloneResult(
            source.Name,
            newName,
            creation.TxCount,
            creation.RxCount,
            creation.CopiedSubscriptionCount,
            IsGenericRole: true);
    }

    public MachineCloneResult AddDeviceFromTemplate(
        MachineTemplatePackage template,
        MachineInstanceOptions options)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(options);
        string newName = ValidateNewRoleName(options.NewName);
        EnsureTemplateVersionIsCompatible(template);

        MachineRoleCreation creation = MachineRoleInstantiationService.CreateFromTemplate(
            template.TemplateDocument.Root
                ?? throw new InvalidOperationException("Le modèle de machine ne contient pas de racine <device>."),
            new MachineInstanceOptions
            {
                NewName = newName,
                UseTemplateTxLabels = options.UseTemplateTxLabels,
                UseTemplateRxLabels = options.UseTemplateRxLabels,
                TxLabelPrefix = options.TxLabelPrefix,
                RxLabelPrefix = options.RxLabelPrefix
            });
        RebaseDanteNamespace(
            creation.DeviceElement,
            template.TemplateDocument.Root!.Name.Namespace,
            Document.Root!.Name.Namespace);

        EnsureStructuralCandidateIsValid([creation.DeviceElement]);
        Document.Root!.Add(creation.DeviceElement);
        AuthorizeAddedDevice(creation.DeviceElement);
        RegisterChange(
            "Machine ajoutée depuis la banque",
            $"{template.Metadata.TemplateName} -> {newName}; {creation.TxCount} TX, {creation.RxCount} RX");

        return new MachineCloneResult(
            template.Metadata.TemplateName,
            newName,
            creation.TxCount,
            creation.RxCount,
            creation.CopiedSubscriptionCount,
            IsGenericRole: true);
    }

    private string ValidateNewRoleName(string? proposedName)
    {
        string newName = DanteNameRules.EnsureValidDeviceName(proposedName);
        if (Devices.Any(device => string.Equals(device.Name, newName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Une machine porte déjà le nom '{newName}'.");
        }

        return newName;
    }

    private void EnsureTemplateVersionIsCompatible(MachineTemplatePackage template)
    {
        if (!string.Equals(
                template.Metadata.SourcePresetVersion,
                PresetVersion,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Le modèle provient d'un preset {Blank(template.Metadata.SourcePresetVersion)}, "
                + $"alors que le projet courant utilise {Blank(PresetVersion)}. "
                + "La migration automatique entre ces structures n'est pas encore validée.");
        }
    }

    private void EnsureStructuralCandidateIsValid(IReadOnlyList<XElement> additions)
    {
        XDocument candidate = new(Document);
        foreach (XElement addition in additions)
        {
            candidate.Root!.Add(new XElement(addition));
        }

        DanteValidationResult currentValidation = DanteProjectIntegrityValidator.Validate(Document);
        DanteValidationResult candidateValidation = DanteProjectIntegrityValidator.Validate(candidate);
        HashSet<string> existingErrors = currentValidation.Errors.ToHashSet(StringComparer.Ordinal);
        string[] introducedErrors = candidateValidation.Errors
            .Where(error => !existingErrors.Contains(error))
            .ToArray();
        if (introducedErrors.Length > 0)
        {
            throw new InvalidOperationException(
                "L'ajout de la machine est annulé car il introduirait une incohérence XML :"
                + Environment.NewLine
                + string.Join(Environment.NewLine, introducedErrors.Select(error => "- " + error)));
        }
    }

    private static void RebaseDanteNamespace(
        XElement root,
        XNamespace sourceNamespace,
        XNamespace targetNamespace)
    {
        foreach (XElement element in root.DescendantsAndSelf())
        {
            if (element.Name.Namespace == sourceNamespace)
            {
                element.Name = targetNamespace + element.Name.LocalName;
            }
        }
    }

    private void AuthorizeAddedDevice(XElement device)
    {
        string roleIdentity = MachineRoleIdentityService.GetOrCreateSessionIdentity(device);
        _authorizedDeviceAdditions.RemoveAll(item =>
            string.Equals(item.RoleIdentity, roleIdentity, StringComparison.Ordinal));
        _authorizedDeviceAdditions.Add(new AuthorizedDeviceAdditionState(
            roleIdentity,
            new XElement(device)));
    }

    private IReadOnlyList<DanteAuthorizedDeviceAddition> BuildGuardAuthorizations()
    {
        List<DanteAuthorizedDeviceAddition> authorizations = [];
        foreach (AuthorizedDeviceAdditionState state in _authorizedDeviceAdditions)
        {
            XElement? currentDevice = Document.Root.Children("device").FirstOrDefault(device =>
                string.Equals(
                    MachineRoleIdentityService.TryGetSessionIdentity(device),
                    state.RoleIdentity,
                    StringComparison.Ordinal));
            if (currentDevice is null)
            {
                continue;
            }

            authorizations.Add(new DanteAuthorizedDeviceAddition(
                MachineRoleIdentityService.ReadVisibleName(currentDevice),
                new XElement(state.BaselineDevice)));
        }

        return authorizations;
    }

    private IReadOnlyList<AuthorizedDeviceAdditionState> CaptureAuthorizedDeviceAdditions()
    {
        return _authorizedDeviceAdditions
            .Select(item => new AuthorizedDeviceAdditionState(item.RoleIdentity, new XElement(item.BaselineDevice)))
            .ToArray();
    }

    private void RestoreAuthorizedDeviceAdditions(IReadOnlyList<AuthorizedDeviceAdditionState> states)
    {
        _authorizedDeviceAdditions.Clear();
        _authorizedDeviceAdditions.AddRange(states.Select(item =>
            new AuthorizedDeviceAdditionState(item.RoleIdentity, new XElement(item.BaselineDevice))));
    }

    private sealed record AuthorizedDeviceAdditionState(
        string RoleIdentity,
        XElement BaselineDevice);
}
