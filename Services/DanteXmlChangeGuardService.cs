using System.Xml.Linq;

namespace DanteConfigEditor.Services;

internal sealed record DanteAuthorizedDeviceAddition(
    string CurrentDeviceName,
    XElement BaselineDevice);

public static class DanteXmlChangeGuardService
{
    private static readonly HashSet<string> AllowedPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/preset/name",
        "/preset/device",
        "/preset/device/name",
        "/preset/device/friendly_name",
        "/preset/device/preferred_master/@value",
        "/preset/device/redundancy/@value",
        "/preset/device/samplerate",
        "/preset/device/encoding",
        "/preset/device/unicast_latency",
        "/preset/device/interface/ipv4_address",
        "/preset/device/interface/ipv4_address/address",
        "/preset/device/interface/ipv4_address/@address",
        "/preset/device/interface/ipv4_address/dnsserver",
        "/preset/device/interface/ipv4_address/@dnsserver",
        "/preset/device/interface/ipv4_address/gateway",
        "/preset/device/interface/ipv4_address/@gateway",
        "/preset/device/interface/ipv4_address/@ip",
        "/preset/device/interface/ipv4_address/@ipv4",
        "/preset/device/interface/ipv4_address/@mask",
        "/preset/device/interface/ipv4_address/netmask",
        "/preset/device/interface/ipv4_address/@mode",
        "/preset/device/interface/ipv4_address/@netmask",
        "/preset/device/interface/ipv4_address/@static_address",
        "/preset/device/interface/ipv4_address/@static_ip",
        "/preset/device/interface/ipv4_address/subnet",
        "/preset/device/interface/ipv4_address/@subnet",
        "/preset/device/interface/ipv4_address/subnet_mask",
        "/preset/device/interface/ipv4_address/@subnet_mask",
        "/preset/device/interface/ipv4_address/@value",
        "/preset/device/txchannel/label",
        "/preset/device/txchannel/name",
        "/preset/device/txchannel/channel_name",
        "/preset/device/txchannel/@label",
        "/preset/device/txchannel/@name",
        "/preset/device/txchannel/@channel_name",
        "/preset/device/rxchannel/name",
        "/preset/device/rxchannel/label",
        "/preset/device/rxchannel/channel_name",
        "/preset/device/rxchannel/@name",
        "/preset/device/rxchannel/@label",
        "/preset/device/rxchannel/@channel_name",
        "/preset/device/rxchannel/subscribed_channel",
        "/preset/device/rxchannel/subscribed_channel_name",
        "/preset/device/rxchannel/subscribed_channel_label",
        "/preset/device/rxchannel/subscribed_tx_channel",
        "/preset/device/rxchannel/subscribed_tx_channel_name",
        "/preset/device/rxchannel/subscribed_label",
        "/preset/device/rxchannel/source_channel",
        "/preset/device/rxchannel/source_channel_name",
        "/preset/device/rxchannel/subscribed_device",
        "/preset/device/rxchannel/subscription_device",
        "/preset/device/rxchannel/tx_device",
        "/preset/device/rxchannel/source_device"
    };

    private static readonly string[] ForbiddenExactPaths =
    [
        "/preset/@version",
        "/preset/device/instance_id",
        "/preset/device/instance_id/device_id",
        "/preset/device/instance_id/process_id",
        "/preset/device/manufacturer_id",
        "/preset/device/manufacturer_name",
        "/preset/device/model_id",
        "/preset/device/model_name",
        "/preset/device/model_version",
        "/preset/device/device_type",
        "/preset/device/device_type_string",
        "/preset/device/captureInfo",
        "/preset/device/interface",
        "/preset/device/clock",
        "/preset/device/clock_priority",
        "/preset/device/rtp",
        "/preset/device/txchannel",
        "/preset/device/rxchannel",
        "/preset/device/txchannel/@danteId",
        "/preset/device/rxchannel/@danteId",
        "/preset/device/txchannel/@mediaType",
        "/preset/device/rxchannel/@mediaType"
    ];

    public static DanteValidationResult ValidateChanges(XDocument originalDocument, XDocument currentDocument)
    {
        return ValidateChanges(originalDocument, currentDocument, []);
    }

    internal static DanteValidationResult ValidateChanges(
        XDocument originalDocument,
        XDocument currentDocument,
        IReadOnlyList<DanteAuthorizedDeviceAddition> authorizedDeviceAdditions)
    {
        DanteValidationResult result = new();

        if (originalDocument.Root is null || currentDocument.Root is null)
        {
            result.AddError(DanteIssueCategory.SaveSafety, "Sauvegarde refusée : la racine <preset> est absente.");
            return result;
        }

        if (!string.Equals(originalDocument.Root.Name.LocalName, currentDocument.Root.Name.LocalName, StringComparison.Ordinal))
        {
            result.AddError(DanteIssueCategory.SaveSafety, $"Sauvegarde refusée : racine XML modifiée ({originalDocument.Root.Name.LocalName} -> {currentDocument.Root.Name.LocalName}).");
            return result;
        }

        if (!string.Equals(originalDocument.Root.Name.NamespaceName, currentDocument.Root.Name.NamespaceName, StringComparison.Ordinal))
        {
            result.AddError(DanteIssueCategory.SaveSafety, "Sauvegarde refusée : le namespace XML de la racine a été modifié.");
            return result;
        }

        CompareElements(
            originalDocument.Root,
            currentDocument.Root,
            "/" + originalDocument.Root.Name.LocalName,
            result,
            authorizedDeviceAdditions);

        if (!result.HasErrors)
        {
            result.AddInfo(DanteIssueCategory.SaveSafety, "Compatibilité XML : aucune modification interdite détectée.");
        }

        return result;
    }

    public static string BuildGuardReport(DanteValidationResult guardResult)
    {
        List<string> lines = [];
        lines.Add("Garde-fou changements XML");
        lines.Add("------------------------");
        lines.Add(guardResult.HasErrors
            ? "ERROR Sauvegarde refusée : une modification interdite du XML Dante a été détectée."
            : "OK Compatibilité XML : aucune modification interdite détectée.");

        foreach (string warning in guardResult.Warnings.Take(80))
        {
            lines.Add("WARNING " + warning);
        }

        foreach (string error in guardResult.Errors.Take(80))
        {
            lines.Add("ERROR " + error);
        }

        if (guardResult.Warnings.Count > 80 || guardResult.Errors.Count > 80)
        {
            lines.Add("WARNING Rapport tronqué : ouvrez la page Santé du fichier pour les détails.");
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static void CompareElements(XElement original, XElement current, string path, DanteValidationResult result)
    {
        CompareElements(original, current, path, result, []);
    }

    private static void CompareElements(
        XElement original,
        XElement current,
        string path,
        DanteValidationResult result,
        IReadOnlyList<DanteAuthorizedDeviceAddition> authorizedDeviceAdditions)
    {
        // Le cas courant ne modifie qu'une petite partie du preset. Écarter
        // immédiatement un sous-arbre strictement identique évite de recréer
        // tableaux, chemins et empreintes pour tous ses descendants.
        if (XNode.DeepEquals(original, current))
        {
            return;
        }

        if (!string.Equals(original.Name.LocalName, current.Name.LocalName, StringComparison.Ordinal))
        {
            AddChangeIssue(result, path, $"Balise modifiée : <{original.Name.LocalName}> -> <{current.Name.LocalName}>.");
            return;
        }

        if (!string.Equals(original.Name.NamespaceName, current.Name.NamespaceName, StringComparison.Ordinal))
        {
            AddChangeIssue(result, path, $"Namespace modifié sur <{original.Name.LocalName}>.");
            return;
        }

        CompareAttributes(original, current, path, result);

        if (string.Equals(path, "/preset", StringComparison.OrdinalIgnoreCase))
        {
            ComparePresetChildren(original, current, result, authorizedDeviceAdditions);
            return;
        }

        XElement[] originalChildren = original.Elements().ToArray();
        XElement[] currentChildren = current.Elements().ToArray();
        if (originalChildren.Length == 0 && currentChildren.Length == 0)
        {
            string originalValue = original.Value.Trim();
            string currentValue = current.Value.Trim();
            if (!string.Equals(originalValue, currentValue, StringComparison.Ordinal))
            {
                AddChangeIssue(result, path, $"Valeur modifiée : {FormatValue(originalValue)} -> {FormatValue(currentValue)}.");
            }

            return;
        }

        CompareGroupedChildren(originalChildren, currentChildren, path, result);
    }

    private static void ComparePresetChildren(
        XElement original,
        XElement current,
        DanteValidationResult result,
        IReadOnlyList<DanteAuthorizedDeviceAddition> authorizedDeviceAdditions)
    {
        XElement[] originalNonDevices = original.Elements().Where(element => element.Name.LocalName != "device").ToArray();
        XElement[] currentNonDevices = current.Elements().Where(element => element.Name.LocalName != "device").ToArray();
        CompareGroupedChildren(originalNonDevices, currentNonDevices, "/preset", result);
        CompareDevices(
            original.Children("device").ToArray(),
            current.Children("device").ToArray(),
            authorizedDeviceAdditions,
            result);
    }

    private static void CompareGroupedChildren(
        IReadOnlyList<XElement> originalChildren,
        IReadOnlyList<XElement> currentChildren,
        string parentPath,
        DanteValidationResult result)
    {
        if (CanCompareChildrenByPosition(originalChildren, currentChildren))
        {
            for (int index = 0; index < originalChildren.Count; index++)
            {
                string childPath = NormalizePath(parentPath + "/" + originalChildren[index].Name.LocalName);
                CompareElements(originalChildren[index], currentChildren[index], childPath, result);
            }

            return;
        }

        // L'ordre des balises n'est pas une modification de contenu. Les
        // enfants sont comparés par nom puis par identité technique connue.
        string[] childNames = originalChildren.Select(element => element.Name.LocalName)
            .Concat(currentChildren.Select(element => element.Name.LocalName))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        foreach (string childName in childNames)
        {
            XElement[] originalGroup = originalChildren.Where(element => element.Name.LocalName == childName).ToArray();
            XElement[] currentGroup = currentChildren.Where(element => element.Name.LocalName == childName).ToArray();
            CompareElementGroup(originalGroup, currentGroup, NormalizePath(parentPath + "/" + childName), result);
        }
    }

    private static bool CanCompareChildrenByPosition(
        IReadOnlyList<XElement> originalChildren,
        IReadOnlyList<XElement> currentChildren)
    {
        if (originalChildren.Count != currentChildren.Count)
        {
            return false;
        }

        for (int index = 0; index < originalChildren.Count; index++)
        {
            XElement original = originalChildren[index];
            XElement current = currentChildren[index];
            if (original.Name != current.Name || !StableChildKeyMatches(original, current))
            {
                return false;
            }

            if (BuildChildMatchKey(original) is null)
            {
                for (int previous = 0; previous < index; previous++)
                {
                    if (originalChildren[previous].Name == original.Name)
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    private static bool StableChildKeyMatches(XElement original, XElement current)
    {
        string localName = original.Name.LocalName;
        if (localName is "txchannel" or "rxchannel")
        {
            return string.Equals(
                original.Attribute("danteId")?.Value,
                current.Attribute("danteId")?.Value,
                StringComparison.OrdinalIgnoreCase);
        }

        if (localName == "interface")
        {
            return string.Equals(
                original.Attribute("network")?.Value,
                current.Attribute("network")?.Value,
                StringComparison.OrdinalIgnoreCase);
        }

        return true;
    }

    private static void CompareElementGroup(
        IReadOnlyList<XElement> originalGroup,
        IReadOnlyList<XElement> currentGroup,
        string path,
        DanteValidationResult result)
    {
        List<XElement> unmatchedCurrent = currentGroup.ToList();
        List<XElement> unmatchedOriginal = [];

        foreach (XElement originalElement in originalGroup)
        {
            XElement? matched = FindMatchingChild(originalElement, unmatchedCurrent);
            if (matched is null)
            {
                unmatchedOriginal.Add(originalElement);
                continue;
            }

            unmatchedCurrent.Remove(matched);
            CompareElements(originalElement, matched, path, result);
        }

        int fallbackPairs = Math.Min(unmatchedOriginal.Count, unmatchedCurrent.Count);
        for (int index = 0; index < fallbackPairs; index++)
        {
            CompareElements(unmatchedOriginal[index], unmatchedCurrent[index], path, result);
        }

        for (int index = fallbackPairs; index < unmatchedOriginal.Count; index++)
        {
            AddChangeIssue(result, path, $"Balise supprimée : <{unmatchedOriginal[index].Name.LocalName}>.");
        }

        for (int index = fallbackPairs; index < unmatchedCurrent.Count; index++)
        {
            AddChangeIssue(result, path, $"Balise ajoutée : <{unmatchedCurrent[index].Name.LocalName}>.");
        }
    }

    private static XElement? FindMatchingChild(XElement original, IReadOnlyList<XElement> candidates)
    {
        if (candidates.Count == 0)
        {
            return null;
        }

        string? matchKey = BuildChildMatchKey(original);
        if (!string.IsNullOrWhiteSpace(matchKey))
        {
            XElement[] keyedMatches = candidates
                .Where(candidate => string.Equals(BuildChildMatchKey(candidate), matchKey, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            if (keyedMatches.Length > 0)
            {
                return keyedMatches[0];
            }
        }

        string fingerprint = BuildElementFingerprint(original);
        return candidates.FirstOrDefault(candidate =>
            string.Equals(BuildElementFingerprint(candidate), fingerprint, StringComparison.Ordinal));
    }

    private static string? BuildChildMatchKey(XElement element)
    {
        return element.Name.LocalName switch
        {
            "txchannel" or "rxchannel" when !string.IsNullOrWhiteSpace(element.Attribute("danteId")?.Value)
                => "dante-id:" + element.Attribute("danteId")!.Value.Trim(),
            "interface" when !string.IsNullOrWhiteSpace(element.Attribute("network")?.Value)
                => "network:" + element.Attribute("network")!.Value.Trim(),
            _ => null
        };
    }

    private static string BuildElementFingerprint(XElement element)
    {
        string attributes = string.Join("|", element.Attributes()
            .Where(attribute => !attribute.IsNamespaceDeclaration)
            .OrderBy(attribute => attribute.Name.ToString(), StringComparer.Ordinal)
            .Select(attribute => $"{attribute.Name}={attribute.Value}"));
        return $"{element.Name.LocalName}|{attributes}|{element.ToString(SaveOptions.DisableFormatting)}";
    }

    private static void CompareDevices(
        IReadOnlyList<XElement> originalDevices,
        IReadOnlyList<XElement> currentDevices,
        IReadOnlyList<DanteAuthorizedDeviceAddition> authorizedDeviceAdditions,
        DanteValidationResult result)
    {
        // Le nom visible peut changer. On associe d'abord les machines par
        // identité de session, puis par instance_id et par structure de rôle.
        bool[] matchedCurrentIndexes = new bool[currentDevices.Count];
        Dictionary<string, Queue<int>>?[] currentIndexesByIdentity =
            new Dictionary<string, Queue<int>>?[DeviceIdentitySelectors.Length];
        for (int originalIndex = 0; originalIndex < originalDevices.Count; originalIndex++)
        {
            XElement originalDevice = originalDevices[originalIndex];
            int? currentIndex = FindMatchingDeviceIndex(
                originalDevice,
                originalDevices.Count,
                currentDevices,
                currentIndexesByIdentity,
                matchedCurrentIndexes,
                originalIndex);
            if (!currentIndex.HasValue)
            {
                // La suppression d'une machine est une fonction métier prise
                // en charge par DanteProject, qui nettoie aussi ses patchs.
                continue;
            }

            matchedCurrentIndexes[currentIndex.Value] = true;
            CompareElements(originalDevice, currentDevices[currentIndex.Value], "/preset/device", result);
        }

        HashSet<int> usedAuthorizations = [];
        for (int currentIndex = 0; currentIndex < currentDevices.Count; currentIndex++)
        {
            if (matchedCurrentIndexes[currentIndex])
            {
                continue;
            }

            XElement currentDevice = currentDevices[currentIndex];
            string currentName = MachineRoleIdentityService.ReadVisibleName(
                currentDevice,
                $"position {currentIndex + 1}");
            int authorizationIndex = FindAuthorizationIndex(
                authorizedDeviceAdditions,
                usedAuthorizations,
                currentName);
            if (authorizationIndex < 0)
            {
                result.AddError(
                    DanteIssueCategory.SaveSafety,
                    $"Ajout de device non autorisé par défaut : {currentName}. "
                    + "Utilisez la duplication, la banque de machines ou l'import XML contrôlé.");
                continue;
            }

            usedAuthorizations.Add(authorizationIndex);
            CompareElements(
                authorizedDeviceAdditions[authorizationIndex].BaselineDevice,
                currentDevice,
                "/preset/device",
                result);
        }
    }

    private static int FindAuthorizationIndex(
        IReadOnlyList<DanteAuthorizedDeviceAddition> authorizations,
        IReadOnlySet<int> usedIndexes,
        string currentName)
    {
        for (int index = 0; index < authorizations.Count; index++)
        {
            if (!usedIndexes.Contains(index)
                && string.Equals(
                    authorizations[index].CurrentDeviceName,
                    currentName,
                    StringComparison.OrdinalIgnoreCase))
            {
                return index;
            }
        }

        return -1;
    }

    private static int? FindMatchingDeviceIndex(
        XElement originalDevice,
        int originalDeviceCount,
        IReadOnlyList<XElement> currentDevices,
        IList<Dictionary<string, Queue<int>>?> currentIndexesByIdentity,
        IReadOnlyList<bool> matchedCurrentIndexes,
        int originalIndex)
    {
        for (int selectorIndex = 0; selectorIndex < DeviceIdentitySelectors.Length; selectorIndex++)
        {
            Func<XElement, string> identitySelector = DeviceIdentitySelectors[selectorIndex];
            string originalIdentity = identitySelector(originalDevice);
            if (string.IsNullOrWhiteSpace(originalIdentity))
            {
                continue;
            }

            Dictionary<string, Queue<int>> indexes = currentIndexesByIdentity[selectorIndex]
                ??= BuildDeviceIndexes(currentDevices, identitySelector);
            if (indexes.TryGetValue(originalIdentity, out Queue<int>? candidates))
            {
                while (candidates.TryDequeue(out int candidateIndex))
                {
                    if (!matchedCurrentIndexes[candidateIndex])
                    {
                        return candidateIndex;
                    }
                }
            }
        }

        // Le repli par position n'est autorisé que si aucun device n'a été
        // ajouté ou supprimé ; il permet alors de détecter un identifiant altéré.
        if (originalDeviceCount == currentDevices.Count)
        {
            return originalIndex < currentDevices.Count && !matchedCurrentIndexes[originalIndex]
                ? originalIndex
                : Enumerable.Range(0, currentDevices.Count).FirstOrDefault(index => !matchedCurrentIndexes[index], -1) is int fallback && fallback >= 0
                    ? fallback
                    : null;
        }

        return null;
    }

    private static Dictionary<string, Queue<int>> BuildDeviceIndexes(
        IReadOnlyList<XElement> devices,
        Func<XElement, string> identitySelector)
    {
        Dictionary<string, Queue<int>> indexes = new(StringComparer.OrdinalIgnoreCase);
        for (int index = 0; index < devices.Count; index++)
        {
            string identity = identitySelector(devices[index]);
            if (string.IsNullOrWhiteSpace(identity))
            {
                continue;
            }

            if (!indexes.TryGetValue(identity, out Queue<int>? queue))
            {
                queue = new Queue<int>();
                indexes[identity] = queue;
            }

            queue.Enqueue(index);
        }

        return indexes;
    }

    private static readonly Func<XElement, string>[] DeviceIdentitySelectors =
    [
        device => MachineRoleIdentityService.TryGetSessionIdentity(device) ?? string.Empty,
        device => device.Child("instance_id").ChildValue("device_id"),
        device => device.ChildValue("default_name"),
        BuildGenericRoleIdentity
    ];

    private static string BuildGenericRoleIdentity(XElement device)
    {
        if (!string.IsNullOrWhiteSpace(MachineRoleIdentityService.ReadTechnicalDeviceId(device))
            || !string.IsNullOrWhiteSpace(device.ChildValue("default_name")))
        {
            return string.Empty;
        }

        string txSignature = string.Join(
            ",",
            device.Children("txchannel").Select(channel =>
                $"{channel.Attribute("danteId")?.Value}:{channel.Attribute("mediaType")?.Value}"));
        string rxSignature = string.Join(
            ",",
            device.Children("rxchannel").Select(channel =>
                $"{channel.Attribute("danteId")?.Value}:{channel.Attribute("mediaType")?.Value}"));
        return string.Join(
            "|",
            "generic-role",
            device.ChildValue("manufacturer_id"),
            device.ChildValue("model_id"),
            device.ChildValue("device_type"),
            txSignature,
            rxSignature);
    }

    private static string DeviceDisplayName(XElement device, int position)
    {
        return MachineRoleIdentityService.ReadVisibleName(device, $"position {position}");
    }

    private static void CompareAttributes(XElement original, XElement current, string path, DanteValidationResult result)
    {
        foreach (XAttribute originalAttribute in original.Attributes().Where(attribute => !attribute.IsNamespaceDeclaration))
        {
            XAttribute? currentAttribute = current.Attribute(originalAttribute.Name);
            string attributePath = NormalizePath(path + "/@" + originalAttribute.Name.LocalName);
            if (currentAttribute is null)
            {
                AddChangeIssue(result, attributePath, $"Attribut supprimé : @{originalAttribute.Name.LocalName}.");
            }
            else if (!string.Equals(originalAttribute.Value, currentAttribute.Value, StringComparison.Ordinal))
            {
                AddChangeIssue(result, attributePath, $"Attribut @{originalAttribute.Name.LocalName} modifié : {FormatValue(originalAttribute.Value)} -> {FormatValue(currentAttribute.Value)}.");
            }
        }

        foreach (XAttribute currentAttribute in current.Attributes().Where(attribute => !attribute.IsNamespaceDeclaration))
        {
            if (original.Attribute(currentAttribute.Name) is null)
            {
                string attributePath = NormalizePath(path + "/@" + currentAttribute.Name.LocalName);
                AddChangeIssue(result, attributePath, $"Attribut ajouté : @{currentAttribute.Name.LocalName}.");
            }
        }
    }

    private static void AddChangeIssue(DanteValidationResult result, string rawPath, string detail)
    {
        string path = NormalizePath(rawPath);
        string message = $"{ToUserPath(path)} : {detail}";

        if (IsAllowed(path))
        {
            return;
        }

        string prefix = IsForbidden(path)
            ? "Modification technique interdite : "
            : "Chemin XML non autorisé par défaut : ";
        result.AddError(DanteIssueCategory.SaveSafety, prefix + message);
    }

    private static bool IsAllowed(string path)
    {
        return AllowedPaths.Contains(path);
    }

    private static bool IsForbidden(string path)
    {
        return ForbiddenExactPaths.Any(forbidden => string.Equals(path, forbidden, StringComparison.OrdinalIgnoreCase)
            || path.StartsWith(forbidden + "/", StringComparison.OrdinalIgnoreCase));
    }

    private static string NormalizePath(string path)
    {
        string[] parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return "/" + string.Join("/", parts);
    }

    private static string ToUserPath(string path)
    {
        return path.Replace("danteId", "Dante Id", StringComparison.Ordinal);
    }

    private static string FormatValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "(vide)";
        }

        string compact = value.ReplaceLineEndings(" ").Trim();
        return compact.Length <= 60 ? compact : compact[..57] + "...";
    }
}
