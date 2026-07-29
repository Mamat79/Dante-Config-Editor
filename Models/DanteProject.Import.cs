using System.IO;
using System.Xml.Linq;
using DanteConfigEditor.Services;

namespace DanteConfigEditor.Models;

public sealed partial class DanteProject
{
    public IReadOnlyList<string> FindDuplicateDeviceNamesInXml(string path)
    {
        DanteProject importedProject = Load(path);
        Dictionary<string, DanteDevice> existingDevicesByIdentity = BuildTechnicalDeviceIndex();
        return importedProject.Devices
            .Where(device =>
                Devices.Any(existing => string.Equals(existing.Name, device.Name, StringComparison.OrdinalIgnoreCase))
                || HasExistingTechnicalIdentity(device, existingDevicesByIdentity))
            .Select(device => device.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public IReadOnlyDictionary<string, string> BuildAutomaticDuplicateRenameMap(string path, string suffix = "Import")
    {
        DanteProject importedProject = Load(path);
        string cleanSuffix = NormalizeImportSuffix(suffix);
        Dictionary<string, DanteDevice> existingDevicesByIdentity = BuildTechnicalDeviceIndex();
        HashSet<string> usedNames = Devices.Select(device => device.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, string> renameMap = new(StringComparer.OrdinalIgnoreCase);

        foreach (DanteDevice device in importedProject.Devices)
        {
            bool nameConflict = usedNames.Contains(device.Name);
            bool identityConflict = HasExistingTechnicalIdentity(device, existingDevicesByIdentity);
            if (!nameConflict && !identityConflict)
            {
                usedNames.Add(device.Name);
                continue;
            }

            string newName = BuildUniqueImportedDeviceName(device.Name, cleanSuffix, usedNames);
            renameMap[device.Name] = newName;
            usedNames.Add(newName);
        }

        return renameMap;
    }

    public DanteMergeResult MergeDevicesFromXml(string path, IReadOnlyDictionary<string, string>? duplicateRenameMap = null)
    {
        DanteProject importedProject = Load(path);
        if (!string.Equals(importedProject.PresetVersion, PresetVersion, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Import refusé : le preset ajouté est en version {importedProject.PresetVersion}, "
                + $"le projet courant en version {PresetVersion}. "
                + "Une fusion entre versions XML différentes doit être validée avant d'être autorisée.");
        }

        if (!string.Equals(
                importedProject.Document.Root?.Name.NamespaceName,
                Document.Root?.Name.NamespaceName,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Import refusé : les deux presets n'utilisent pas le même namespace XML.");
        }

        DanteValidationResult importedValidation = importedProject.Validate();
        if (importedValidation.HasErrors)
        {
            throw new InvalidOperationException(
                "Import refusé : le fichier ajouté contient des erreurs bloquantes."
                + Environment.NewLine
                + importedValidation.ToDisplayText());
        }

        Dictionary<string, string> cleanRenameMap = NormalizeRenameMap(duplicateRenameMap);
        Dictionary<string, DanteDevice> existingDevicesByIdentity = BuildTechnicalDeviceIndex();
        HashSet<string> usedNames = Devices.Select(device => device.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        List<XElement> devicesToImport = [];
        List<string> skippedDuplicates = [];
        Dictionary<string, string> appliedRenames = new(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, string> subscriptionDeviceNameMap = new(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, string> reusedDevices = new(StringComparer.OrdinalIgnoreCase);

        foreach (DanteDevice device in importedProject.Devices)
        {
            string technicalIdentity = BuildTechnicalIdentityKey(device);
            if (!string.IsNullOrWhiteSpace(technicalIdentity)
                && existingDevicesByIdentity.TryGetValue(technicalIdentity, out DanteDevice? existingTechnicalDevice))
            {
                if (cleanRenameMap.TryGetValue(device.Name, out string? genericRoleName)
                    && !string.IsNullOrWhiteSpace(genericRoleName))
                {
                    // Un renommage demandé explicitement signifie que
                    // l'utilisateur veut conserver un second rôle. On le
                    // neutralise comme le Preset Creator au lieu d'inventer
                    // un EUI-64 ou de recopier l'identité matérielle.
                    string genericTargetName = genericRoleName;
                    if (usedNames.Contains(genericTargetName))
                    {
                        throw new InvalidOperationException($"Import refusé : le nom '{genericTargetName}' est déjà utilisé.");
                    }

                    if (ContainsProblematicCharacters(genericTargetName))
                    {
                        throw new InvalidOperationException($"Import refusé : le nom '{genericTargetName}' contient des caractères non imprimables.");
                    }

                    MachineRoleCreation genericRole = MachineRoleInstantiationService.CreateClone(
                        device.Element,
                        new MachineCloneOptions
                        {
                            NewName = genericTargetName,
                            PreserveTxLabels = true,
                            PreserveRxLabels = true,
                            PreserveDeviceSettings = true,
                            PreserveNetworkConfiguration = false,
                            PreserveSubscriptions = true,
                            PreserveMulticastFlows = false,
                            PreservePreferredMaster = false
                        });
                    devicesToImport.Add(genericRole.DeviceElement);
                    usedNames.Add(genericTargetName);
                    appliedRenames[device.Name] = genericTargetName;
                    subscriptionDeviceNameMap[device.Name] = genericTargetName;
                    continue;
                }

                // Sans renommage explicite, le rôle existant est réutilisé.
                // Les références du XML importé suivent son nom courant.
                reusedDevices[device.Name] = existingTechnicalDevice.Name;
                if (!string.Equals(device.Name, existingTechnicalDevice.Name, StringComparison.OrdinalIgnoreCase))
                {
                    subscriptionDeviceNameMap[device.Name] = existingTechnicalDevice.Name;
                }
                continue;
            }

            bool nameAlreadyUsed = usedNames.Contains(device.Name);
            XElement clone = new(device.Element);
            string targetName = device.Name;

            if (nameAlreadyUsed)
            {
                if (!cleanRenameMap.TryGetValue(device.Name, out string? renamedDeviceName) || string.IsNullOrWhiteSpace(renamedDeviceName))
                {
                    skippedDuplicates.Add(device.Name);
                    continue;
                }

                targetName = renamedDeviceName;
                RenameDeviceElement(clone, targetName);
                appliedRenames[device.Name] = targetName;
                subscriptionDeviceNameMap[device.Name] = targetName;
            }

            if (usedNames.Contains(targetName))
            {
                throw new InvalidOperationException($"Import refusé : le nom '{targetName}' est déjà utilisé.");
            }

            if (ContainsProblematicCharacters(targetName))
            {
                throw new InvalidOperationException($"Import refusé : le nom '{targetName}' contient des caractères non imprimables.");
            }

            usedNames.Add(targetName);
            devicesToImport.Add(clone);
        }

        // Les références doivent être adaptées avant la validation du
        // candidat, faute de quoi elles pourraient viser un ancien nom.
        foreach (XElement device in devicesToImport)
        {
            UpdateImportedSubscriptionDeviceNames(device, subscriptionDeviceNameMap);
        }

        EnsureStructuralCandidateIsValid(devicesToImport);
        foreach (XElement device in devicesToImport)
        {
            Document.Root!.Add(device);
            AuthorizeAddedDevice(device);
        }

        if (devicesToImport.Count > 0)
        {
            RegisterChange(
                "Import XML",
                $"{devicesToImport.Count} machine(s) ajoutée(s), "
                + $"{reusedDevices.Count} machine(s) existante(s) réutilisée(s) depuis {Path.GetFileName(path)}");
        }

        return new DanteMergeResult(
            devicesToImport.Count,
            appliedRenames.Count,
            skippedDuplicates.Count,
            skippedDuplicates,
            appliedRenames,
            reusedDevices.Count,
            reusedDevices);
    }

    private static Dictionary<string, string> NormalizeRenameMap(IReadOnlyDictionary<string, string>? renameMap)
    {
        Dictionary<string, string> clean = new(StringComparer.OrdinalIgnoreCase);
        if (renameMap is null)
        {
            return clean;
        }

        foreach (KeyValuePair<string, string> item in renameMap)
        {
            string oldName = item.Key.Trim();
            string newName = item.Value.Trim();
            if (!string.IsNullOrWhiteSpace(oldName) && !string.IsNullOrWhiteSpace(newName))
            {
                clean[oldName] = newName;
            }
        }

        return clean;
    }

    private static string NormalizeImportSuffix(string suffix)
    {
        string clean = (suffix ?? string.Empty).Trim().Trim('(', ')').Trim();
        clean = DanteNameRules.NormalizeDeviceNamePart(
            string.Join("-", clean.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)),
            string.Empty);
        if (string.IsNullOrWhiteSpace(clean))
        {
            throw new InvalidOperationException("Le suffixe de renommage ne peut pas être vide.");
        }
        return clean;
    }

    private static string BuildUniqueImportedDeviceName(string originalName, string suffix, ISet<string> usedNames)
    {
        return DanteNameRules.BuildUniqueSuffixedDeviceName(originalName, suffix, usedNames);
    }

    private Dictionary<string, DanteDevice> BuildTechnicalDeviceIndex()
    {
        Dictionary<string, DanteDevice> index = new(StringComparer.OrdinalIgnoreCase);
        foreach (DanteDevice device in Devices)
        {
            string identity = BuildTechnicalIdentityKey(device);
            if (!string.IsNullOrWhiteSpace(identity))
            {
                index.TryAdd(identity, device);
            }
        }

        return index;
    }

    private static string BuildTechnicalIdentityKey(DanteDevice device)
    {
        if (string.IsNullOrWhiteSpace(device.TechnicalDeviceId))
        {
            return string.Empty;
        }

        return $"{device.TechnicalDeviceId.Trim()}|{device.ProcessId.Trim()}";
    }

    private static bool HasExistingTechnicalIdentity(
        DanteDevice device,
        IReadOnlyDictionary<string, DanteDevice> existingDevicesByIdentity)
    {
        string identity = BuildTechnicalIdentityKey(device);
        return !string.IsNullOrWhiteSpace(identity)
            && existingDevicesByIdentity.ContainsKey(identity);
    }

    private static void RenameDeviceElement(XElement deviceElement, string newName)
    {
        SetElementValue(deviceElement, "name", newName.Trim());
        SetElementValue(deviceElement, "friendly_name", newName.Trim());
    }

    private static void UpdateImportedSubscriptionDeviceNames(XElement importedDeviceElement, IReadOnlyDictionary<string, string> renamedDevices)
    {
        if (renamedDevices.Count == 0)
        {
            return;
        }

        foreach (XElement rxChannel in importedDeviceElement.Children("rxchannel"))
        {
            XElement? subscribedDevice = FindFirstElement(rxChannel, SubscriptionDeviceElementNames);
            if (subscribedDevice is not null && renamedDevices.TryGetValue(subscribedDevice.Value.Trim(), out string? newDeviceName))
            {
                subscribedDevice.Value = newDeviceName;
            }
        }
    }
}
