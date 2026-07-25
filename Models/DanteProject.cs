using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Net;
using DanteConfigEditor.Services;

namespace DanteConfigEditor.Models;

public sealed partial class DanteProject
{
    public const int MaximumUndoSnapshots = 10;

    // Noms de balises reconnus pour identifier le device TX auquel un RX est abonné.
    // Plusieurs variantes sont acceptées pour rester tolérant avec les exports XML.
    private static readonly string[] SubscriptionDeviceElementNames =
    [
        "subscribed_device",
        "subscription_device",
        "tx_device",
        "source_device"
    ];

    // Noms de balises reconnus pour le canal TX de destination d'un patch.
    private static readonly string[] SubscriptionChannelElementNames =
    [
        "subscribed_channel",
        "subscribed_channel_name",
        "subscribed_channel_label",
        "subscribed_tx_channel",
        "subscribed_tx_channel_name",
        "subscribed_label",
        "source_channel",
        "source_channel_name"
    ];

    private static readonly HashSet<string> SupportedSamplerates = new(StringComparer.OrdinalIgnoreCase)
    {
        "44100",
        "48000",
        "88200",
        "96000",
        "176400",
        "192000"
    };

    private static readonly HashSet<string> SupportedEncodings = new(StringComparer.OrdinalIgnoreCase)
    {
        "16",
        "24",
        "32"
    };

    private static readonly string[] IpAddressAttributeNames =
    [
        "address",
        "ip",
        "ipv4",
        "static_address",
        "static_ip",
        "value"
    ];

    private static readonly string[] IpNetmaskAttributeNames =
    [
        "mask",
        "netmask",
        "subnet",
        "subnet_mask"
    ];

    private static readonly string[] DynamicIpClearedElementNames =
    [
        "address",
        "netmask",
        "subnet",
        "subnet_mask"
    ];

    // Les éléments RX modifiés sont gardés pour l'affichage et pour le résumé
    // avant sauvegarde. La clé reste l'élément XML exact.
    private readonly Dictionary<XElement, bool> _modifiedRxElements = [];
    private readonly List<ChangeRecord> _changes = [];
    private readonly Stack<UndoSnapshot> _undoSnapshots = [];
    private XDocument _originalDocument;
    private DanteXmlCompatibilityProfile _originalCompatibilityProfile;
    private int _batchDepth;
    private bool _reloadPending;

    private DanteProject(string originalFilePath, XDocument document, XDocument? originalDocument = null)
    {
        OriginalFilePath = originalFilePath;
        Document = document;
        _originalDocument = new XDocument(originalDocument ?? document);
        MachineRoleIdentityService.PairEquivalentDocuments(_originalDocument, Document);
        _originalCompatibilityProfile = DanteXmlCompatibilityService.CaptureProfile(_originalDocument);
        ReloadModel();
    }

    public string OriginalFilePath { get; private set; }

    public string PresetName => Document.Root.Child("name")?.Value.Trim() ?? Path.GetFileNameWithoutExtension(OriginalFilePath);

    public string PresetVersion => Document.Root?.Attribute("version")?.Value ?? string.Empty;

    public string? LastSavedPath { get; private set; }

    public XDocument Document { get; private set; }

    public IReadOnlyList<DanteDevice> Devices { get; private set; } = [];

    public DantePatchMatrix PatchMatrix { get; private set; } = new([]);

    public IReadOnlyList<ChangeRecord> Changes => _changes;

    public bool IsModified { get; private set; }

    public bool CanUndo => _undoSnapshots.Count > 0;

    public string LastUndoLabel => _undoSnapshots.TryPeek(out UndoSnapshot? snapshot) ? snapshot.Label : "Aucune action";

    public static DanteProject Load(string path)
    {
        XDocument document = LoadDocument(path);

        DanteProject project = new(path, document);
        if (project.Devices.Count == 0)
        {
            throw new InvalidOperationException("Aucun élément <device> n'a été trouvé. Ce fichier ne semble pas compatible avec cette version.");
        }

        return project;
    }

    public static DanteProject LoadRecovered(string originalPath, string recoveryPath)
    {
        XDocument originalDocument = LoadDocument(originalPath);
        XDocument recoveredDocument = LoadDocument(recoveryPath);
        DanteProject project = new(originalPath, recoveredDocument, originalDocument);
        if (project.Devices.Count == 0)
        {
            throw new InvalidOperationException("La session récupérée ne contient aucune machine Dante.");
        }

        project.IsModified = true;
        project._changes.Add(new ChangeRecord(DateTime.Now, "Récupération automatique", "Session temporaire restaurée après interruption"));
        project.MarkRecoveredPatchChanges();
        return project;
    }

    public void PushUndoSnapshot(string label)
    {
        // Copie complète du XML : plus lourd qu'une annulation ciblée, mais plus sûr
        // car les modifications peuvent toucher plusieurs balises de patch.
        TrimUndoHistory();
        XDocument snapshotDocument = new(Document);
        MachineRoleIdentityService.PairEquivalentDocuments(Document, snapshotDocument);
        _undoSnapshots.Push(new UndoSnapshot(
            label,
            snapshotDocument,
            IsModified,
            _changes.Count,
            CaptureModifiedRxReferences(),
            CaptureAuthorizedDeviceAdditions()));
    }

    public void RestoreLastUndoSnapshot()
    {
        if (_undoSnapshots.Count == 0)
        {
            return;
        }

        RestoreSnapshot(_undoSnapshots.Pop());
    }

    public string UndoLastChange()
    {
        if (_undoSnapshots.Count == 0)
        {
            throw new InvalidOperationException("Aucune action à annuler.");
        }

        UndoSnapshot snapshot = _undoSnapshots.Pop();
        RestoreSnapshot(snapshot);
        return snapshot.Label;
    }

    public DanteDevice? FindDevice(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        return Devices.FirstOrDefault(device =>
            string.Equals(
                MachineRoleIdentityService.ReadVisibleName(device.Element),
                name,
                StringComparison.OrdinalIgnoreCase));
    }

    public void ApplyBatch(Action<DanteProject> mutation)
    {
        ArgumentNullException.ThrowIfNull(mutation);
        // Les méthodes métier continuent d'enregistrer chaque changement, mais
        // le modèle de lecture n'est reconstruit qu'à la sortie du lot externe.
        _batchDepth++;
        try
        {
            mutation(this);
        }
        finally
        {
            _batchDepth--;
            if (_batchDepth == 0 && _reloadPending)
            {
                ReloadModel();
            }
        }
    }

    public void RenameDevice(string oldName, string newName)
    {
        if (string.IsNullOrWhiteSpace(oldName) || string.IsNullOrWhiteSpace(newName))
        {
            throw new InvalidOperationException("Le nom actuel et le nouveau nom doivent être renseignés.");
        }

        string cleanNewName = DanteNameRules.EnsureValidDeviceName(newName);

        if (Devices.Any(device => string.Equals(device.Name, cleanNewName, StringComparison.OrdinalIgnoreCase) && !string.Equals(device.Name, oldName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Un autre device porte déjà ce nom.");
        }

        DanteDevice device = FindDevice(oldName) ?? throw new InvalidOperationException("Device introuvable.");
        if (device.Element.Child("name") is not null)
        {
            SetElementValue(device.Element, "name", cleanNewName);
        }
        SetElementValue(device.Element, "friendly_name", cleanNewName);

        // Si le device TX est renommé, les patchs qui pointaient vers son ancien
        // nom doivent suivre pour ne pas casser les abonnements reconnus.
        foreach (XElement rxChannel in Document.Root!.Children("device").SelectMany(deviceElement => deviceElement.Children("rxchannel")))
        {
            XElement? subscribedDevice = FindFirstElement(rxChannel, SubscriptionDeviceElementNames);
            if (subscribedDevice is not null && string.Equals(subscribedDevice.Value.Trim(), oldName, StringComparison.OrdinalIgnoreCase))
            {
                subscribedDevice.Value = cleanNewName;
                _modifiedRxElements[rxChannel] = true;
            }
        }

        RegisterChange("Nom device", $"{oldName} -> {cleanNewName}");
    }

    public void SetNetworkMode(string deviceName, bool redundant)
    {
        DanteDevice device = FindDevice(deviceName) ?? throw new InvalidOperationException("Device introuvable.");
        SetBooleanElementAttribute(device.Element, "redundancy", "value", redundant, afterElementName: "friendly_name");
        RegisterChange("Mode réseau", $"{deviceName} -> {(redundant ? "redondant" : "daisychain")}");
    }

    public void SetLatency(string deviceName, string latency)
    {
        ValidateLatency(latency);
        DanteDevice device = FindDevice(deviceName) ?? throw new InvalidOperationException("Device introuvable.");
        SetElementValue(device.Element, "unicast_latency", latency);
        RegisterChange("Latence", $"{deviceName} -> {DanteLatencyFormatter.FormatLatencyWithXmlValue(latency)}");
    }

    public void SetSamplerate(string deviceName, string samplerate)
    {
        string cleanSamplerate = ValidateSamplerate(samplerate);
        DanteDevice device = FindDevice(deviceName) ?? throw new InvalidOperationException("Device introuvable.");
        SetElementValue(device.Element, "samplerate", cleanSamplerate);
        RegisterChange("Sample rate", $"{deviceName} -> {FormatSamplerateForDisplay(cleanSamplerate)}");
    }

    public void SetEncoding(string deviceName, string encoding)
    {
        string cleanEncoding = ValidateEncoding(encoding);
        DanteDevice device = FindDevice(deviceName) ?? throw new InvalidOperationException("Device introuvable.");
        SetElementValue(device.Element, "encoding", cleanEncoding);
        RegisterChange("Bits par échantillon", $"{deviceName} -> {FormatEncodingForDisplay(cleanEncoding)}");
    }

    public bool SetIpAddressDynamic(string deviceName)
    {
        DanteDevice device = FindDevice(deviceName) ?? throw new InvalidOperationException("Device introuvable.");
        bool changed = SetDeviceIpAddressesDynamic(device);
        if (!changed)
        {
            RegisterChange("IP automatique", $"{deviceName} déjà en automatique ou sans adresse IPv4 modifiable");
            return false;
        }

        RegisterChange("IP automatique", $"{deviceName} -> dynamique");
        return true;
    }

    public void SetIpAddressStatic(string deviceName, string address, string netmask, string gateway)
    {
        DanteDevice device = FindDevice(deviceName) ?? throw new InvalidOperationException("Device introuvable.");
        string cleanAddress = ValidateIpv4Address(address, "adresse IP");
        string cleanNetmask = ValidateIpv4Address(string.IsNullOrWhiteSpace(netmask) ? "255.255.255.0" : netmask, "masque");
        string cleanGateway = ValidateIpv4Address(string.IsNullOrWhiteSpace(gateway) ? "0.0.0.0" : gateway, "passerelle");
        SetDeviceIpAddressStatic(device, cleanAddress, cleanNetmask, cleanGateway);
        RegisterChange("IP fixe", $"{deviceName} -> {cleanAddress} / {cleanNetmask} / gateway {cleanGateway}");
    }

    public bool SupportsIpConfiguration(string deviceName)
    {
        DanteDevice device = FindDevice(deviceName) ?? throw new InvalidOperationException("Device introuvable.");
        return DeviceSupportsIpConfiguration(device);
    }

    public void SetPreferredMaster(string deviceName, bool preferredMaster)
    {
        DanteDevice device = FindDevice(deviceName) ?? throw new InvalidOperationException("Device introuvable.");
        SetBooleanElementAttribute(device.Element, "preferred_master", "value", preferredMaster, afterElementName: "redundancy");
        RegisterChange("Preferred master", $"{deviceName} -> {preferredMaster}");
    }

    public int DeleteDevice(string deviceName)
    {
        DanteDevice device = FindDevice(deviceName) ?? throw new InvalidOperationException("Device introuvable.");
        if (Devices.Count <= 1)
        {
            throw new InvalidOperationException("Impossible de supprimer la dernière machine du preset.");
        }

        int removedSubscriptions = RemoveSubscriptionsReferencingDevice(device.Name);
        device.Element.Remove();
        RegisterChange("Machine supprimée", $"{deviceName} supprimé, {removedSubscriptions} patch(s) nettoyé(s)");
        return removedSubscriptions;
    }

    public int ResetDevicePatches(string deviceName)
    {
        DanteDevice device = FindDevice(deviceName) ?? throw new InvalidOperationException("Device introuvable.");
        int removedRxSubscriptions = RemoveSubscriptionsFromDeviceRxChannels(device.Name);
        int removedTxSubscriptions = RemoveSubscriptionsReferencingDevice(device.Name);
        int total = removedRxSubscriptions + removedTxSubscriptions;
        RegisterChange("Patch machine reset", $"{deviceName}: {removedRxSubscriptions} entrée(s) RX et {removedTxSubscriptions} départ(s) TX supprimé(s)");
        return total;
    }

    public int ResetDeviceRxPatches(string deviceName)
    {
        DanteDevice device = FindDevice(deviceName) ?? throw new InvalidOperationException("Device introuvable.");
        int removedRxSubscriptions = RemoveSubscriptionsFromDeviceRxChannels(device.Name);
        RegisterChange("Patch RX machine reset", $"{deviceName}: {removedRxSubscriptions} entrée(s) RX supprimée(s)");
        return removedRxSubscriptions;
    }

    public int ResetDeviceTxPatches(string deviceName)
    {
        DanteDevice device = FindDevice(deviceName) ?? throw new InvalidOperationException("Device introuvable.");
        int removedTxSubscriptions = RemoveSubscriptionsReferencingDevice(device.Name);
        RegisterChange("Patch TX machine reset", $"{deviceName}: {removedTxSubscriptions} départ(s) TX supprimé(s)");
        return removedTxSubscriptions;
    }

    public void ResetChannels(string deviceName)
    {
        DanteDevice device = FindDevice(deviceName) ?? throw new InvalidOperationException("Device introuvable.");
        ResetDeviceChannels(device);
        RegisterChange("Canaux", $"Réinitialisation des canaux de {deviceName}");
    }

    public void RenameChannel(string deviceName, DanteChannelKind channelKind, int channelIndex, string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new InvalidOperationException("Le nouveau nom de canal doit être renseigné.");
        }

        if (ContainsProblematicCharacters(newName))
        {
            throw new InvalidOperationException("Le nouveau nom de canal contient des caractères non imprimables.");
        }

        DanteDevice device = FindDevice(deviceName) ?? throw new InvalidOperationException("Device introuvable.");
        IReadOnlyList<DanteChannel> channels = channelKind == DanteChannelKind.Tx ? device.TxChannels : device.RxChannels;
        DanteChannel channel = channels.FirstOrDefault(candidate => candidate.Index == channelIndex)
            ?? throw new InvalidOperationException("Canal introuvable.");

        string oldName = channel.DisplayName;
        string trimmedNewName = DanteNameRules.EnsureValidChannelName(newName);

        if (channelKind == DanteChannelKind.Tx)
        {
            SetChannelDisplayName(channel, "label", trimmedNewName);
            // Un canal TX peut être utilisé par plusieurs RX : on met à jour
            // toutes les références reconnues dans le fichier.
            UpdateSubscriptionsForRenamedTxChannel(device.Element.ChildValue("name"), oldName, trimmedNewName);
        }
        else
        {
            SetChannelDisplayName(channel, "name", trimmedNewName);
        }

        RegisterChange("Nom canal", $"{deviceName} {channelKind} {channelIndex}: {oldName} -> {trimmedNewName}");
    }

    public void BatchRenameChannels(string deviceName, DanteChannelKind channelKind, string prefix, int firstNumber, int startChannelIndex, int endChannelIndex)
    {
        if (firstNumber < 0)
        {
            throw new InvalidOperationException("Le numéro de départ doit être positif.");
        }

        if (startChannelIndex > endChannelIndex)
        {
            throw new InvalidOperationException("La plage de canaux est invalide.");
        }

        DanteDevice device = FindDevice(deviceName) ?? throw new InvalidOperationException("Device introuvable.");
        IReadOnlyList<DanteChannel> channels = channelKind == DanteChannelKind.Tx ? device.TxChannels : device.RxChannels;
        if (channels.Count == 0)
        {
            throw new InvalidOperationException("Aucun canal à renommer pour ce device.");
        }

        DanteChannel[] selectedChannels = channels
            .Where(channel => channel.Index >= startChannelIndex && channel.Index <= endChannelIndex)
            .ToArray();

        if (selectedChannels.Length == 0)
        {
            throw new InvalidOperationException("Aucun canal trouvé dans cette plage.");
        }

        string cleanPrefix = prefix.Trim();
        int digits = Math.Max(2, (firstNumber + selectedChannels.Length - 1).ToString().Length);
        int number = firstNumber;
        List<(string OldName, string NewName)> txRenames = [];

        // On renomme seulement la plage demandée. Les autres canaux restent
        // intacts, même s'ils sont du même type TX/RX.
        foreach (DanteChannel channel in selectedChannels)
        {
            string oldName = channel.DisplayName;
            string newName = BuildBatchChannelName(cleanPrefix, number, digits, device.Name);

            if (ContainsProblematicCharacters(newName))
            {
                throw new InvalidOperationException("Le préfixe contient des caractères non imprimables.");
            }

            SetChannelDisplayName(channel, channelKind == DanteChannelKind.Tx ? "label" : "name", newName);
            if (channelKind == DanteChannelKind.Tx)
            {
                txRenames.Add((oldName, newName));
            }

            number++;
        }

        if (channelKind == DanteChannelKind.Tx)
        {
            // Mise à jour groupée après le renommage : cela évite les effets
            // de cascade si un nouveau nom ressemble à un ancien nom.
            UpdateSubscriptionsForRenamedTxChannels(device.Name, txRenames);
        }

        RegisterChange("Renommage série", $"{deviceName} {channelKind}: canaux {startChannelIndex}-{endChannelIndex}, {cleanPrefix} depuis {firstNumber}");
    }

    public int ExtendChannelNameSeries(
        string deviceName,
        DanteChannelKind channelKind,
        IReadOnlyList<int> seedChannelIndexes,
        int targetChannelIndex)
    {
        DanteDevice device = FindDevice(deviceName) ?? throw new InvalidOperationException("Device introuvable.");
        IReadOnlyList<DanteChannel> channels = channelKind == DanteChannelKind.Tx ? device.TxChannels : device.RxChannels;
        ChannelSeriesValue[] ordered = channels
            .OrderBy(channel => channel.PositionIndex)
            .Select(channel => new ChannelSeriesValue(channel.Index, channel.DisplayName))
            .ToArray();
        IReadOnlyList<ChannelSeriesValue> generated = ChannelNameSeriesService.Extend(
            ordered,
            seedChannelIndexes,
            targetChannelIndex);

        HashSet<int> generatedIndexes = generated.Select(item => item.ChannelIndex).ToHashSet();
        HashSet<string> untouchedNames = channels
            .Where(channel => !generatedIndexes.Contains(channel.Index))
            .Select(channel => channel.DisplayName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        string? duplicate = generated
            .Select(item => item.Name)
            .FirstOrDefault(name => untouchedNames.Contains(name));
        if (!string.IsNullOrWhiteSpace(duplicate))
        {
            throw new InvalidOperationException($"La série produirait un nom déjà utilisé : {duplicate}.");
        }

        List<(string OldName, string NewName)> txRenames = [];
        foreach (ChannelSeriesValue item in generated)
        {
            DanteChannel channel = channels.Single(candidate => candidate.Index == item.ChannelIndex);
            if (ContainsProblematicCharacters(item.Name))
            {
                throw new InvalidOperationException("La série contient des caractères non imprimables.");
            }

            SetChannelDisplayName(channel, channelKind == DanteChannelKind.Tx ? "label" : "name", item.Name);
            if (channelKind == DanteChannelKind.Tx)
            {
                txRenames.Add((channel.DisplayName, item.Name));
            }
        }

        if (channelKind == DanteChannelKind.Tx)
        {
            UpdateSubscriptionsForRenamedTxChannels(device.Name, txRenames);
        }

        RegisterChange(
            "Série de canaux prolongée",
            $"{deviceName} {channelKind}: {generated.Count} canal(aux) jusqu'au canal {targetChannelIndex}");
        return generated.Count;
    }

    private static string BuildBatchChannelName(string pattern, int number, int defaultDigits, string deviceName)
    {
        if (pattern.Contains('{', StringComparison.Ordinal))
        {
            string value = pattern
                .Replace("{device}", deviceName, StringComparison.OrdinalIgnoreCase)
                .Replace("{n}", number.ToString(), StringComparison.OrdinalIgnoreCase)
                .Replace("{number}", number.ToString(), StringComparison.OrdinalIgnoreCase);

            for (int digits = 1; digits <= 6; digits++)
            {
                value = value.Replace("{" + new string('0', digits) + "}", number.ToString().PadLeft(digits, '0'), StringComparison.OrdinalIgnoreCase);
            }

            return value.Trim();
        }

        return string.IsNullOrWhiteSpace(pattern)
            ? number.ToString().PadLeft(defaultDigits, '0')
            : $"{pattern} {number.ToString().PadLeft(defaultDigits, '0')}";
    }

    public void ApplyPatch(string rxDeviceName, int rxIndex, string txDeviceName, string txChannelName)
    {
        if (string.IsNullOrWhiteSpace(rxDeviceName))
        {
            throw new InvalidOperationException("Aucun device récepteur sélectionné.");
        }

        if (string.IsNullOrWhiteSpace(txDeviceName))
        {
            throw new InvalidOperationException("Aucun device émetteur sélectionné.");
        }

        DanteDevice txDevice = FindDevice(txDeviceName) ?? throw new InvalidOperationException("Le device émetteur n'existe pas dans ce fichier.");
        if (!string.IsNullOrWhiteSpace(txChannelName) && txDevice.TxChannels.Count > 0 && !ChannelExists(txDevice.TxChannels, txChannelName))
        {
            throw new InvalidOperationException("Le canal TX sélectionné n'existe pas sur le device émetteur.");
        }

        XElement rxElement = FindRxElement(rxDeviceName, rxIndex);
        string rawTxDeviceName = ShouldUseLocalSubscriptionMarker(rxElement, rxDeviceName, txDeviceName) ? "." : txDeviceName.Trim();
        // Si les balises de patch n'existent pas encore, elles sont créées avec
        // le premier nom reconnu par l'application.
        SetSubscriptionElements(rxElement, rawTxDeviceName, txChannelName.Trim());
        _modifiedRxElements[rxElement] = true;

        RegisterChange("Patch", $"{rxDeviceName} RX {rxIndex} -> {FormatDisplayTxDevice(rawTxDeviceName, txDeviceName)} {txChannelName}".Trim());
    }

    public void RemovePatch(string rxDeviceName, int rxIndex)
    {
        XElement rxElement = FindRxElement(rxDeviceName, rxIndex);
        foreach (string elementName in SubscriptionDeviceElementNames.Concat(SubscriptionChannelElementNames))
        {
            rxElement.Element(elementName)?.Remove();
        }

        _modifiedRxElements[rxElement] = true;
        RegisterChange("Patch supprimé", $"{rxDeviceName} RX {rxIndex}");
    }

    private int RemoveSubscriptionsReferencingDevice(string txDeviceName)
    {
        int count = 0;
        foreach (XElement rxChannel in Document.Root!.Children("device").SelectMany(deviceElement => deviceElement.Children("rxchannel")).ToArray())
        {
            XElement? subscribedDevice = FindFirstElement(rxChannel, SubscriptionDeviceElementNames);
            if (subscribedDevice is null || !string.Equals(subscribedDevice.Value.Trim(), txDeviceName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            foreach (string elementName in SubscriptionDeviceElementNames.Concat(SubscriptionChannelElementNames))
            {
                rxChannel.Element(elementName)?.Remove();
            }

            _modifiedRxElements[rxChannel] = true;
            count++;
        }

        return count;
    }

    private int RemoveSubscriptionsFromDeviceRxChannels(string rxDeviceName)
    {
        DanteDevice device = FindDevice(rxDeviceName) ?? throw new InvalidOperationException("Device introuvable.");
        int count = 0;
        foreach (XElement rxChannel in device.Element.Children("rxchannel").ToArray())
        {
            bool hadSubscription = SubscriptionDeviceElementNames
                .Concat(SubscriptionChannelElementNames)
                .Any(elementName => rxChannel.Element(elementName) is not null);

            if (!hadSubscription)
            {
                continue;
            }

            foreach (string elementName in SubscriptionDeviceElementNames.Concat(SubscriptionChannelElementNames))
            {
                rxChannel.Element(elementName)?.Remove();
            }

            _modifiedRxElements[rxChannel] = true;
            count++;
        }

        return count;
    }

    public DanteValidationResult Validate()
    {
        // Validation volontairement prudente : erreurs bloquantes pour les cas
        // structurels, avertissements pour les patchs non résolus.
        DanteValidationResult result = new();
        if (Document.Root is null)
        {
            result.AddError(DanteIssueCategory.XmlCompatibility, "Le document XML ne contient pas de racine.");
            return result;
        }

        if (Devices.Count == 0)
        {
            result.AddError(DanteIssueCategory.XmlCompatibility, "Aucun device Dante n'a été détecté.");
            return result;
        }

        result.Merge(DanteProjectIntegrityValidator.Validate(Document));
        result.Merge(DanteXmlCompatibilityService.ValidateCompatibility(Document, _originalCompatibilityProfile));
        result.Merge(ValidateXmlChangeGuard());

        foreach (string warning in BuildImportantWarnings())
        {
            result.AddWarning(DanteIssueCategory.Network, warning);
        }

        foreach (DanteDevice device in Devices)
        {
            if (string.IsNullOrWhiteSpace(device.Name))
            {
                result.AddError(DanteIssueCategory.Device, "Un device a un nom vide.");
            }

            if (ContainsProblematicCharacters(device.Name))
            {
                result.AddError(DanteIssueCategory.Device, $"Le device '{device.Name}' contient des caractères non imprimables.", device.Name);
            }

            if (device.TxCount == 0)
            {
                result.AddWarning(DanteIssueCategory.Device, $"{device.Name} ne contient aucun canal TX.", device.Name);
            }

            if (device.RxCount == 0)
            {
                result.AddWarning(DanteIssueCategory.Device, $"{device.Name} ne contient aucun canal RX.", device.Name);
            }

            foreach (DanteChannel channel in device.TxChannels.Concat(device.RxChannels))
            {
                if (string.IsNullOrWhiteSpace(channel.DisplayName))
                {
                    result.AddWarning(DanteIssueCategory.Channel, $"{device.Name} contient un canal {channel.Kind} sans nom lisible.", device.Name, danteId: channel.DanteId);
                }

                if (ContainsProblematicCharacters(channel.DisplayName))
                {
                    result.AddWarning(DanteIssueCategory.Channel, $"{device.Name} / {channel.DisplayName} contient des caractères non imprimables.", device.Name, channel.DisplayName, channel.DanteId);
                }
            }
        }

        AddDuplicateDanteIdIssues(result);
        AddAudioFormatIssues(result);
        AddClockIssues(result);

        foreach (IGrouping<string, DanteDevice> group in Devices.GroupBy(device => device.Name, StringComparer.OrdinalIgnoreCase))
        {
            if (!string.IsNullOrWhiteSpace(group.Key) && group.Count() > 1)
            {
                result.AddError(DanteIssueCategory.Device, $"Le nom de device '{group.Key}' est présent plusieurs fois.", group.Key);
            }
        }

        foreach (DanteSubscription subscription in PatchMatrix.Subscriptions)
        {
            if (!subscription.IsActive)
            {
                result.AddInfo(DanteIssueCategory.Patch, $"{subscription.Display} est libre.", subscription.RxDevice, subscription.RxChannelName, subscription.RxDanteId);
                continue;
            }

            if (subscription.IsLocalSubscription)
            {
                result.AddInfo(DanteIssueCategory.Patch, $"{subscription.Display} utilise une source locale '.'.", subscription.RxDevice, subscription.RxChannelName, subscription.RxDanteId);
            }

            if (subscription.IsExternalMissingDevice)
            {
                result.AddWarning(DanteIssueCategory.Patch, $"{subscription.Display} pointe vers un device TX absent du preset : {subscription.RawTxDeviceName}.", subscription.RxDevice, subscription.RxChannelName, subscription.RxDanteId);
            }
            else if (subscription.IsTxChannelMissing)
            {
                result.AddWarning(DanteIssueCategory.Patch, $"{subscription.Display} pointe vers un canal TX non retrouvé : {subscription.TxChannelName}.", subscription.RxDevice, subscription.RxChannelName, subscription.RxDanteId);
            }
            else if (subscription.IsConflict)
            {
                result.AddError(DanteIssueCategory.Patch, subscription.Status, subscription.RxDevice, subscription.RxChannelName, subscription.RxDanteId);
            }
        }

        return result;
    }

    public IReadOnlyList<string> BuildImportantWarnings()
    {
        return BuildImportantWarningDetails().Select(warning => warning.Message).ToArray();
    }

    public IReadOnlyList<DanteImportantWarning> BuildImportantWarningDetails()
    {
        List<DanteImportantWarning> warnings = [];

        DanteDevice[] redundantDevices = Devices.Where(device => device.IsRedundant).ToArray();
        DanteDevice[] daisychainDevices = Devices.Where(device => !device.IsRedundant).ToArray();
        int redundantCount = redundantDevices.Length;
        int daisychainCount = daisychainDevices.Length;
        if (redundantCount > 0 && daisychainCount > 0)
        {
            DanteDevice[] affectedDevices = redundantCount == daisychainCount
                ? Devices.ToArray()
                : redundantCount < daisychainCount ? redundantDevices : daisychainDevices;
            warnings.Add(new DanteImportantWarning(
                "Warning.MixedNetworkModes",
                $"ATTENTION : le fichier mélange {redundantCount} machine(s) en redondant et {daisychainCount} machine(s) en daisychain. Vérifiez que c'est volontaire pour ce réseau.",
                affectedDevices.Select(device => device.Name).ToArray(),
                $"WARNING: the file mixes {redundantCount} redundant device(s) and {daisychainCount} daisychain device(s). Check that this is intentional for this network."));
        }

        DanteDevice[] staticIpDevices = Devices.Where(device => device.UsesStaticIp).ToArray();
        if (staticIpDevices.Length > 0)
        {
            string devices = string.Join(", ", staticIpDevices.Take(12).Select(FormatStaticIpDevice));
            if (staticIpDevices.Length > 12)
            {
                devices += $", +{staticIpDevices.Length - 12} autre(s)";
            }

            warnings.Add(new DanteImportantWarning(
                "Warning.StaticIp",
                $"IP fixe détectée sur {staticIpDevices.Length} machine(s) : {devices}.",
                staticIpDevices.Select(device => device.Name).ToArray(),
                $"Static IP detected on {staticIpDevices.Length} device(s): {devices}."));
        }

        AddMixedValueWarningDetails(
            warnings,
            "Warning.MixedSampleRates",
            "fréquences d'échantillonnage",
            "sample rates",
            device => device.Samplerate,
            FormatSamplerateForDisplay);

        AddMixedValueWarningDetails(
            warnings,
            "Warning.MixedEncodings",
            "bits par échantillon",
            "bits per sample values",
            device => device.Encoding,
            FormatEncodingForDisplay);

        return warnings;
    }

    public IReadOnlyList<DeviceChangeRow> BuildDeviceChangeRows()
    {
        Dictionary<string, XElement> originalDevices = BuildDeviceIdentityMap(_originalDocument);
        Dictionary<string, XElement> currentDevices = BuildDeviceIdentityMap(Document);
        List<string> identities = originalDevices.Keys
            .Concat(currentDevices.Keys)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        List<DeviceChangeRow> rows = [];

        foreach (string identity in identities)
        {
            originalDevices.TryGetValue(identity, out XElement? originalElement);
            currentDevices.TryGetValue(identity, out XElement? currentElement);
            string originalName = ReadDeviceElementValue(originalElement, "name");
            string currentName = ReadDeviceElementValue(currentElement, "name");
            string displayName = !string.IsNullOrWhiteSpace(currentName) ? currentName : originalName;

            if (originalElement is null && currentElement is not null)
            {
                rows.Add(new DeviceChangeRow(displayName, "Machine", "Absente", "Ajoutée", "Ajoutée", true));
                continue;
            }

            if (currentElement is null && originalElement is not null)
            {
                rows.Add(new DeviceChangeRow(displayName, "Machine", "Présente", "Supprimée", "Supprimée", false));
                continue;
            }

            if (originalElement is null || currentElement is null)
            {
                continue;
            }

            AddDeviceFieldChanges(rows, displayName, originalElement, currentElement);
            AddChannelChanges(rows, displayName, originalElement, currentElement, DanteChannelKind.Tx);
            AddChannelChanges(rows, displayName, originalElement, currentElement, DanteChannelKind.Rx);
        }

        return rows;
    }

    public IReadOnlySet<string> GetModifiedDeviceNames()
    {
        if (!IsModified)
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        return BuildDeviceChangeRows()
            .Where(row => row.HasCurrentDevice)
            .Select(row => row.DeviceName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private void AddDuplicateDanteIdIssues(DanteValidationResult result)
    {
        foreach (DanteDevice device in Devices)
        {
            AddDuplicateDanteIdIssues(result, device, device.TxChannels, "TX");
            AddDuplicateDanteIdIssues(result, device, device.RxChannels, "RX");
        }
    }

    private static void AddDuplicateDanteIdIssues(DanteValidationResult result, DanteDevice device, IEnumerable<DanteChannel> channels, string kind)
    {
        foreach (IGrouping<int, DanteChannel> group in channels.GroupBy(channel => channel.DanteId))
        {
            if (group.Count() > 1)
            {
                result.AddError(DanteIssueCategory.Channel, $"{device.Name} contient un doublon de Dante Id {group.Key} dans les canaux {kind}.", device.Name, danteId: group.Key);
            }
        }
    }

    public DanteValidationResult ValidateXmlChangeGuard()
    {
        return DanteXmlChangeGuardService.ValidateChanges(
            _originalDocument,
            Document,
            BuildGuardAuthorizations());
    }

    private void AddAudioFormatIssues(DanteValidationResult result)
    {
        AddDistinctValueWarning(result, "samplerate", DanteIssueCategory.AudioFormat, "Plusieurs samplerates sont présents dans le preset");
        AddDistinctValueWarning(result, "encoding", DanteIssueCategory.AudioFormat, "Plusieurs encodages sont présents dans le preset");
        AddDistinctValueWarning(result, "unicast_latency", DanteIssueCategory.Network, "Plusieurs latences sont présentes dans le preset");
    }

    private void AddDistinctValueWarning(DanteValidationResult result, string elementName, DanteIssueCategory category, string message)
    {
        string[] values = Devices
            .Select(device => device.Element.Element(elementName)?.Value.Trim() ?? string.Empty)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (values.Length > 1)
        {
            result.AddWarning(category, $"{message} : {string.Join(", ", values)}.");
        }
    }

    private void AddClockIssues(DanteValidationResult result)
    {
        int preferredMasters = Devices.Count(device => device.PreferredMaster);
        if (preferredMasters == 0)
        {
            result.AddWarning(DanteIssueCategory.Clock, "Aucune machine preferred master n'est déclarée.");
        }
        else if (preferredMasters > 1)
        {
            result.AddWarning(DanteIssueCategory.Clock, $"{preferredMasters} machines sont déclarées preferred master.");
        }
    }

    private void RestoreSnapshot(UndoSnapshot snapshot)
    {
        // Restauration utilisée par Annuler action et par les erreurs pendant
        // une action utilisateur.
        Document = new XDocument(snapshot.Document);
        MachineRoleIdentityService.PairEquivalentDocuments(snapshot.Document, Document);
        IsModified = snapshot.WasModified;

        while (_changes.Count > snapshot.ChangeCount)
        {
            _changes.RemoveAt(_changes.Count - 1);
        }

        _modifiedRxElements.Clear();
        RestoreAuthorizedDeviceAdditions(snapshot.AuthorizedDeviceAdditions);
        ReloadModel();
        RestoreModifiedRxReferences(snapshot.ModifiedRxReferences);
        ReloadModel();
    }

    private IReadOnlyList<ModifiedRxReference> CaptureModifiedRxReferences()
    {
        return PatchMatrix.Subscriptions
            .Where(subscription => subscription.IsModified)
            .Select(subscription => new ModifiedRxReference(subscription.RxDevice, subscription.RxIndex))
            .ToArray();
    }

    private void RestoreModifiedRxReferences(IEnumerable<ModifiedRxReference> references)
    {
        foreach (ModifiedRxReference reference in references)
        {
            DanteDevice? rxDevice = FindDevice(reference.RxDevice);
            DanteChannel? channel = rxDevice?.RxChannels.FirstOrDefault(rx => rx.Index == reference.RxIndex);
            if (channel is not null)
            {
                _modifiedRxElements[channel.Element] = true;
            }
        }
    }

    private void AddMixedValueWarningDetails(
        List<DanteImportantWarning> warnings,
        string key,
        string label,
        string englishLabel,
        Func<DanteDevice, string> valueSelector,
        Func<string, string> formatter)
    {
        IGrouping<string, DanteDevice>[] groups = Devices
            .Where(device => !string.IsNullOrWhiteSpace(valueSelector(device)))
            .GroupBy(valueSelector, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (groups.Length <= 1)
        {
            return;
        }

        IGrouping<string, DanteDevice> majority = groups
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
            .First();
        string[] affectedNames = groups
            .Where(group => !string.Equals(group.Key, majority.Key, StringComparison.OrdinalIgnoreCase))
            .SelectMany(group => group)
            .Select(device => device.Name)
            .ToArray();
        string[] distinctValues = groups.Select(group => group.Key).ToArray();
        warnings.Add(new DanteImportantWarning(
            key,
            $"ATTENTION : plusieurs {label} sont présentes dans le preset : {string.Join(", ", distinctValues.Select(formatter))}.",
            affectedNames,
            $"WARNING: multiple {englishLabel} are present in the preset: {string.Join(", ", distinctValues.Select(formatter))}."));
    }

    private static Dictionary<string, XElement> BuildDeviceIdentityMap(XDocument document)
    {
        Dictionary<string, XElement> devices = new(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, int> occurrences = new(StringComparer.OrdinalIgnoreCase);
        foreach (XElement device in document.Root.Children("device"))
        {
            string identity = BuildDeviceIdentity(device);
            occurrences.TryGetValue(identity, out int occurrence);
            occurrence++;
            occurrences[identity] = occurrence;
            devices[$"{identity}#{occurrence}"] = device;
        }

        return devices;
    }

    private static string BuildDeviceIdentity(XElement device)
    {
        return MachineRoleIdentityService.GetOrCreateSessionIdentity(device);
    }

    private static string ReadDeviceElementValue(XElement? device, string elementName)
    {
        return device.ChildValue(elementName);
    }

    private static void AddDeviceFieldChanges(
        List<DeviceChangeRow> rows,
        string deviceName,
        XElement originalElement,
        XElement currentElement)
    {
        DanteDevice original = new(originalElement);
        DanteDevice current = new(currentElement);
        AddDeviceChange(rows, deviceName, "Nom de machine", original.Name, current.Name);
        AddDeviceChange(rows, deviceName, "Friendly name", original.FriendlyName, current.FriendlyName);
        AddDeviceChange(rows, deviceName, "Mode réseau", original.NetworkMode, current.NetworkMode);
        AddDeviceChange(rows, deviceName, "Latence", original.LatencyDisplay, current.LatencyDisplay);
        AddDeviceChange(rows, deviceName, "Sample rate", original.SampleRateDisplay, current.SampleRateDisplay);
        AddDeviceChange(rows, deviceName, "Bits par échantillon", original.EncodingDisplay, current.EncodingDisplay);
        AddDeviceChange(rows, deviceName, "Preferred master", original.PreferredMaster ? "Oui" : "Non", current.PreferredMaster ? "Oui" : "Non");
        AddDeviceChange(rows, deviceName, "Adresse IP", FormatIpForComparison(original), FormatIpForComparison(current));
    }

    private static void AddChannelChanges(
        List<DeviceChangeRow> rows,
        string deviceName,
        XElement originalElement,
        XElement currentElement,
        DanteChannelKind kind)
    {
        DanteDevice originalDevice = new(originalElement);
        DanteDevice currentDevice = new(currentElement);
        IEnumerable<DanteChannel> originalChannels = kind == DanteChannelKind.Tx ? originalDevice.TxChannels : originalDevice.RxChannels;
        IEnumerable<DanteChannel> currentChannels = kind == DanteChannelKind.Tx ? currentDevice.TxChannels : currentDevice.RxChannels;
        Dictionary<string, DanteChannel> originalMap = BuildChannelIdentityMap(originalChannels);
        Dictionary<string, DanteChannel> currentMap = BuildChannelIdentityMap(currentChannels);
        string kindLabel = kind == DanteChannelKind.Tx ? "TX" : "RX";

        foreach (string identity in originalMap.Keys.Concat(currentMap.Keys).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            originalMap.TryGetValue(identity, out DanteChannel? originalChannel);
            currentMap.TryGetValue(identity, out DanteChannel? currentChannel);
            int danteId = currentChannel?.DanteId ?? originalChannel?.DanteId ?? 0;
            string channelLabel = $"Canal {kindLabel} {danteId}";

            if (originalChannel is null && currentChannel is not null)
            {
                rows.Add(new DeviceChangeRow(deviceName, channelLabel, "Absent", currentChannel.DisplayName, "Ajouté", true));
                continue;
            }

            if (currentChannel is null && originalChannel is not null)
            {
                rows.Add(new DeviceChangeRow(deviceName, channelLabel, originalChannel.DisplayName, "Supprimé", "Supprimé", true));
                continue;
            }

            if (originalChannel is null || currentChannel is null)
            {
                continue;
            }

            AddDeviceChange(rows, deviceName, channelLabel + " - nom", originalChannel.DisplayName, currentChannel.DisplayName);
            if (kind == DanteChannelKind.Rx)
            {
                AddDeviceChange(
                    rows,
                    deviceName,
                    channelLabel + " - patch",
                    ReadSubscriptionForComparison(originalChannel.Element),
                    ReadSubscriptionForComparison(currentChannel.Element));
            }
        }
    }

    private static Dictionary<string, DanteChannel> BuildChannelIdentityMap(IEnumerable<DanteChannel> channels)
    {
        Dictionary<string, DanteChannel> result = new(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, int> occurrences = new(StringComparer.OrdinalIgnoreCase);
        foreach (DanteChannel channel in channels)
        {
            string identity = channel.HasDanteId ? $"dante-id:{channel.DanteId}" : $"position:{channel.PositionIndex}";
            occurrences.TryGetValue(identity, out int occurrence);
            occurrence++;
            occurrences[identity] = occurrence;
            result[$"{identity}#{occurrence}"] = channel;
        }

        return result;
    }

    private static void AddDeviceChange(
        ICollection<DeviceChangeRow> rows,
        string deviceName,
        string parameter,
        string before,
        string after)
    {
        if (string.Equals(before?.Trim(), after?.Trim(), StringComparison.Ordinal))
        {
            return;
        }

        rows.Add(new DeviceChangeRow(
            deviceName,
            parameter,
            string.IsNullOrWhiteSpace(before) ? "-" : before,
            string.IsNullOrWhiteSpace(after) ? "-" : after,
            "Modifié",
            true));
    }

    private static string FormatIpForComparison(DanteDevice device)
    {
        if (!device.UsesStaticIp)
        {
            return "Automatique";
        }

        string address = string.IsNullOrWhiteSpace(device.StaticIpAddress) ? "adresse inconnue" : device.StaticIpAddress;
        string netmask = string.IsNullOrWhiteSpace(device.StaticIpNetmask) ? "masque inconnu" : device.StaticIpNetmask;
        string gateway = string.IsNullOrWhiteSpace(device.StaticIpGateway) ? "passerelle inconnue" : device.StaticIpGateway;
        return $"Fixe : {address} / {netmask} / {gateway}";
    }

    private static string ReadSubscriptionForComparison(XElement rxChannel)
    {
        string device = FindFirstElement(rxChannel, SubscriptionDeviceElementNames)?.Value.Trim() ?? string.Empty;
        string channel = FindFirstElement(rxChannel, SubscriptionChannelElementNames)?.Value.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(device) && string.IsNullOrWhiteSpace(channel))
        {
            return "Libre";
        }

        string displayDevice = string.Equals(device, ".", StringComparison.Ordinal) ? "LOCAL" : device;
        return $"{Blank(displayDevice)} / {Blank(channel)}";
    }

    private static XDocument LoadDocument(string path)
    {
        try
        {
            // PreserveWhitespace évite de réécrire inutilement tout le fichier.
            XDocument document = XDocument.Load(path, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
            return document.Root is null
                ? throw new InvalidOperationException("Le fichier XML ne contient pas de racine.")
                : document;
        }
        catch (XmlException ex)
        {
            throw new InvalidOperationException($"Le fichier XML n'est pas lisible : {ex.Message}", ex);
        }
    }

    private void MarkRecoveredPatchChanges()
    {
        Dictionary<string, XElement> originalDevices = BuildDeviceIdentityMap(_originalDocument);
        Dictionary<string, XElement> currentDevices = BuildDeviceIdentityMap(Document);
        foreach (string identity in originalDevices.Keys.Intersect(currentDevices.Keys, StringComparer.OrdinalIgnoreCase))
        {
            DanteDevice originalDevice = new(originalDevices[identity]);
            DanteDevice currentDevice = new(currentDevices[identity]);
            Dictionary<string, DanteChannel> originalRx = BuildChannelIdentityMap(originalDevice.RxChannels);
            Dictionary<string, DanteChannel> currentRx = BuildChannelIdentityMap(currentDevice.RxChannels);
            foreach (string channelIdentity in originalRx.Keys.Intersect(currentRx.Keys, StringComparer.OrdinalIgnoreCase))
            {
                if (!string.Equals(
                        ReadSubscriptionForComparison(originalRx[channelIdentity].Element),
                        ReadSubscriptionForComparison(currentRx[channelIdentity].Element),
                        StringComparison.Ordinal))
                {
                    _modifiedRxElements[currentRx[channelIdentity].Element] = true;
                }
            }
        }

        ReloadModel();
    }

    private void ReloadModel()
    {
        // Après chaque modification XML, les objets de lecture sont reconstruits
        // pour refléter les nouvelles valeurs.
        Devices = Document.Root.Children("device").Select(device => new DanteDevice(device)).ToList();
        PatchMatrix = new DantePatchMatrix(BuildSubscriptions());
        _reloadPending = false;
    }

    private IReadOnlyList<DanteSubscription> BuildSubscriptions()
    {
        List<DanteSubscription> subscriptions = [];
        Dictionary<string, DanteDevice> devicesByName = Devices
            .Where(device => !string.IsNullOrWhiteSpace(device.Name))
            .GroupBy(device => device.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        foreach (DanteDevice rxDevice in Devices)
        {
            foreach (DanteChannel rxChannel in rxDevice.RxChannels)
            {
                string rawTxDeviceName = FindFirstElement(rxChannel.Element, SubscriptionDeviceElementNames)?.Value.Trim() ?? string.Empty;
                string txChannelName = FindFirstElement(rxChannel.Element, SubscriptionChannelElementNames)?.Value.Trim() ?? string.Empty;
                string resolvedTxDeviceName = string.Equals(rawTxDeviceName, ".", StringComparison.Ordinal)
                    ? rxDevice.Name
                    : rawTxDeviceName;
                string displayTxDeviceName = FormatDisplayTxDevice(rawTxDeviceName, resolvedTxDeviceName);
                int? txDanteId = null;
                string status = "Libre";
                DanteSubscriptionKind kind = DanteSubscriptionKind.Free;

                // La table Patch indique aussi les conflits simples :
                // device TX absent ou canal TX introuvable.
                if (string.IsNullOrWhiteSpace(rawTxDeviceName) != string.IsNullOrWhiteSpace(txChannelName))
                {
                    status = "Conflit - abonnement incomplet";
                    kind = DanteSubscriptionKind.Conflict;
                }
                else if (!string.IsNullOrWhiteSpace(rawTxDeviceName))
                {
                    bool isLocal = string.Equals(rawTxDeviceName, ".", StringComparison.Ordinal);
                    if (!devicesByName.TryGetValue(resolvedTxDeviceName, out DanteDevice? txDevice))
                    {
                        status = "Warning - device TX absent du preset";
                        kind = DanteSubscriptionKind.ExternalMissingDevice;
                    }
                    else if (!string.IsNullOrWhiteSpace(txChannelName) && txDevice.TxChannels.Count > 0 && !ChannelExists(txDevice.TxChannels, txChannelName))
                    {
                        status = "Warning - canal TX absent";
                        kind = DanteSubscriptionKind.MissingChannel;
                    }
                    else if (isLocal)
                    {
                        status = "Patch local";
                        kind = DanteSubscriptionKind.Local;
                    }
                    else
                    {
                        status = "Patch actif";
                        kind = DanteSubscriptionKind.Normal;
                    }

                    if (txDevice is not null)
                    {
                        txDanteId = FindReferencedChannel(txDevice.TxChannels, txChannelName)?.DanteId;
                    }
                }

                subscriptions.Add(new DanteSubscription(
                    rxDevice.Name,
                    rxChannel.DanteId,
                    rxChannel.PositionIndex,
                    rxChannel.DisplayName,
                    rxChannel.Element,
                    rawTxDeviceName,
                    resolvedTxDeviceName,
                    displayTxDeviceName,
                    txChannelName,
                    txDanteId,
                    _modifiedRxElements.ContainsKey(rxChannel.Element),
                    status,
                    kind));
            }
        }

        return subscriptions;
    }

    private XElement FindRxElement(string rxDeviceName, int rxIndex)
    {
        DanteDevice rxDevice = FindDevice(rxDeviceName) ?? throw new InvalidOperationException("Le device récepteur n'existe pas dans ce fichier.");
        DanteChannel? channel = rxDevice.RxChannels.FirstOrDefault(rx => rx.Index == rxIndex);
        return channel?.Element ?? throw new InvalidOperationException("Le canal RX sélectionné est introuvable.");
    }

    private static bool ChannelExists(IEnumerable<DanteChannel> channels, string channelName)
    {
        return channels.Any(channel =>
            string.Equals(channel.DisplayName, channelName, StringComparison.OrdinalIgnoreCase)
            || string.Equals(channel.Index.ToString(), channelName, StringComparison.OrdinalIgnoreCase));
    }

    private static DanteChannel? FindReferencedChannel(IEnumerable<DanteChannel> channels, string channelName)
    {
        DanteChannel[] matches = channels
            .Where(channel =>
                string.Equals(channel.DisplayName, channelName, StringComparison.OrdinalIgnoreCase)
                || string.Equals(channel.Index.ToString(), channelName, StringComparison.OrdinalIgnoreCase))
            .ToArray();
        return matches.Length == 1 ? matches[0] : null;
    }

    private static XElement? FindFirstElement(XElement parent, IEnumerable<string> names)
    {
        foreach (string name in names)
        {
            XElement? element = parent.Child(name);
            if (element is not null)
            {
                return element;
            }
        }

        return null;
    }

    private static void SetElementValue(XElement parent, string elementName, string value)
    {
        XElement? element = parent.Child(elementName);
        if (element is null)
        {
            parent.Add(new XElement(parent.ChildName(elementName), value));
        }
        else
        {
            element.Value = value;
        }
    }

    private static bool SetDeviceIpAddressesDynamic(DanteDevice device)
    {
        XElement? ipv4Address = DanteIpConfiguration.FindPrimaryIpv4Address(device.Element);
        if (ipv4Address is null)
        {
            return false;
        }

        bool changed = false;
        XAttribute? modeAttribute = ipv4Address.Attribute("mode");
        if (!string.Equals(modeAttribute?.Value, "dynamic", StringComparison.OrdinalIgnoreCase))
        {
            ipv4Address.SetAttributeValue("mode", "dynamic");
            changed = true;
        }

        foreach (string attributeName in IpAddressAttributeNames.Concat(IpNetmaskAttributeNames))
        {
            XAttribute? attribute = ipv4Address.Attribute(attributeName);
            if (attribute is not null)
            {
                attribute.Remove();
                changed = true;
            }
        }

        foreach (string elementName in DynamicIpClearedElementNames)
        {
            XElement? element = ipv4Address.Child(elementName);
            if (element is not null)
            {
                element.Remove();
                changed = true;
            }
        }

        if (!ipv4Address.HasElements && !string.IsNullOrWhiteSpace(ipv4Address.Value))
        {
            ipv4Address.Value = string.Empty;
            changed = true;
        }

        return changed;
    }

    private static void SetDeviceIpAddressStatic(DanteDevice device, string address, string netmask, string gateway)
    {
        XElement ipv4Address = DanteIpConfiguration.FindOrCreatePrimaryIpv4Address(device.Element);
        ipv4Address.SetAttributeValue("mode", "static");
        SetIpField(ipv4Address, IpAddressAttributeNames, "address", address);
        SetIpField(ipv4Address, IpNetmaskAttributeNames, "netmask", netmask);
        SetIpField(ipv4Address, ["gateway"], "gateway", gateway);
    }

    private static bool DeviceSupportsIpConfiguration(DanteDevice device)
    {
        return DanteIpConfiguration.FindPrimaryInterface(device.Element) is not null;
    }

    private static bool DeviceHasStaticIpConfiguration(DanteDevice device)
    {
        XElement? ipv4Address = DanteIpConfiguration.FindPrimaryIpv4Address(device.Element);
        if (ipv4Address is null)
        {
            return false;
        }

        string mode = ipv4Address.Attribute("mode")?.Value.Trim() ?? string.Empty;
        if (string.Equals(mode, "dynamic", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return device.UsesStaticIp
            || IpAddressAttributeNames.Concat(IpNetmaskAttributeNames).Any(attributeName => ipv4Address.Attribute(attributeName) is not null)
            || DynamicIpClearedElementNames.Any(elementName => ipv4Address.Child(elementName) is not null)
            || !string.IsNullOrWhiteSpace(ipv4Address.Value);
    }

    private static void SetIpField(XElement ipv4Address, IEnumerable<string> aliases, string canonicalName, string value)
    {
        foreach (string alias in aliases)
        {
            XAttribute? attribute = ipv4Address.Attribute(alias);
            if (attribute is not null)
            {
                attribute.Value = value;
                return;
            }

            XElement? element = ipv4Address.Child(alias);
            if (element is not null)
            {
                element.Value = value;
                return;
            }
        }

        SetElementValue(ipv4Address, canonicalName, value);
    }

    private static void SetChannelDisplayName(DanteChannel channel, string fallbackElementName, string value)
    {
        string cleanValue = DanteNameRules.EnsureValidChannelName(value);
        // Point important pour la compatibilité : si le nom venait d'un attribut
        // ou d'une balise précise, on écrit au même endroit.
        if (!string.IsNullOrWhiteSpace(channel.NameSource))
        {
            if (channel.NameSourceIsAttribute)
            {
                channel.Element.SetAttributeValue(channel.NameSource, cleanValue);
            }
            else
            {
                SetElementValue(channel.Element, channel.NameSource, cleanValue);
            }

            return;
        }

        SetElementValue(channel.Element, fallbackElementName, cleanValue);
    }

    private static void SetBooleanElementAttribute(XElement parent, string elementName, string attributeName, bool value, string afterElementName)
    {
        XElement? element = parent.Child(elementName);
        if (element is null)
        {
            element = new XElement(parent.ChildName(elementName), new XAttribute(attributeName, value.ToString().ToLowerInvariant()));
            XElement? previous = parent.Child(afterElementName);
            if (previous is null)
            {
                parent.Add(element);
            }
            else
            {
                previous.AddAfterSelf(element);
            }
        }
        else
        {
            element.SetAttributeValue(attributeName, value.ToString().ToLowerInvariant());
        }
    }

    private static void SetSubscriptionElements(XElement rxElement, string txDeviceName, string txChannelName)
    {
        XElement? channelElement = FindFirstElement(rxElement, SubscriptionChannelElementNames);
        XElement? deviceElement = FindFirstElement(rxElement, SubscriptionDeviceElementNames);

        if (channelElement is null)
        {
            channelElement = new XElement(rxElement.ChildName(SubscriptionChannelElementNames[0]), txChannelName);
            XElement? nameElement = rxElement.Child("name");
            if (nameElement is not null)
            {
                nameElement.AddAfterSelf(channelElement);
            }
            else
            {
                rxElement.AddFirst(channelElement);
            }
        }
        else
        {
            channelElement.Value = txChannelName;
        }

        if (deviceElement is null)
        {
            deviceElement = new XElement(rxElement.ChildName(SubscriptionDeviceElementNames[0]), txDeviceName);
            channelElement.AddAfterSelf(deviceElement);
        }
        else
        {
            deviceElement.Value = txDeviceName;
        }
    }

    private void ResetDeviceChannels(DanteDevice device)
    {
        int index = 1;
        List<(string OldName, string NewName)> txRenames = [];

        // Les TX sont traités à part pour pouvoir mettre à jour les patchs qui
        // utilisaient leurs anciens noms.
        foreach (DanteChannel channel in device.TxChannels)
        {
            string oldName = channel.DisplayName;
            string newName = index.ToString();
            SetChannelDisplayName(channel, "label", newName);
            txRenames.Add((oldName, newName));
            index++;
        }

        UpdateSubscriptionsForRenamedTxChannels(device.Name, txRenames);

        index = 1;
        foreach (DanteChannel channel in device.RxChannels)
        {
            SetChannelDisplayName(channel, "name", index.ToString());
            index++;
        }
    }

    private void UpdateSubscriptionsForRenamedTxChannel(string txDeviceName, string oldChannelName, string newChannelName)
    {
        UpdateSubscriptionsForRenamedTxChannels(txDeviceName, new[] { (OldName: oldChannelName, NewName: newChannelName) });
    }

    private void UpdateSubscriptionsForRenamedTxChannels(string txDeviceName, IEnumerable<(string OldName, string NewName)> renamedChannels)
    {
        // Dictionnaire ancien nom -> nouveau nom. Il est insensible à la casse
        // pour mieux tolérer les variations d'écriture dans les XML.
        Dictionary<string, string> renamedByOldName = new(StringComparer.OrdinalIgnoreCase);
        foreach ((string oldName, string newName) in renamedChannels)
        {
            string cleanOldName = oldName.Trim();
            string cleanNewName = newName.Trim();
            if (string.IsNullOrWhiteSpace(cleanOldName)
                || string.Equals(cleanOldName, cleanNewName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            renamedByOldName.TryAdd(cleanOldName, cleanNewName);
        }

        if (renamedByOldName.Count == 0)
        {
            return;
        }

        foreach (XElement rxChannel in Document.Root!.Children("device").SelectMany(deviceElement => deviceElement.Children("rxchannel")))
        {
            string rxDeviceName = rxChannel.Parent.ChildValue("name");
            bool sameDevice = rxChannel.Elements()
                .Where(element => SubscriptionDeviceElementNames.Contains(element.Name.LocalName))
                .Any(element =>
                {
                    string subscribedDevice = element.Value.Trim();
                    return string.Equals(subscribedDevice, txDeviceName, StringComparison.OrdinalIgnoreCase)
                        || (string.Equals(subscribedDevice, ".", StringComparison.Ordinal) && string.Equals(rxDeviceName, txDeviceName, StringComparison.OrdinalIgnoreCase));
                });

            if (!sameDevice)
            {
                continue;
            }

            foreach (XElement subscribedChannel in rxChannel.Elements().Where(element => SubscriptionChannelElementNames.Contains(element.Name.LocalName)))
            {
                if (renamedByOldName.TryGetValue(subscribedChannel.Value.Trim(), out string? newChannelName))
                {
                    subscribedChannel.Value = newChannelName;
                    _modifiedRxElements[rxChannel] = true;
                }
            }
        }
    }

    private static void ValidateLatency(string latency)
    {
        string[] allowedValues = ["250", "1000", "2000", "5000"];
        if (!allowedValues.Contains(latency))
        {
            throw new InvalidOperationException("Latence non reconnue. Valeurs autorisées : 250, 1000, 2000 ou 5000 ms.");
        }
    }

    private static string ValidateSamplerate(string samplerate)
    {
        string cleanSamplerate = samplerate.Trim();
        if (!SupportedSamplerates.Contains(cleanSamplerate))
        {
            throw new InvalidOperationException("Sample rate non reconnue. Valeurs autorisées : 44100, 48000, 88200, 96000, 176400 ou 192000.");
        }

        return cleanSamplerate;
    }

    private static string ValidateEncoding(string encoding)
    {
        string cleanEncoding = encoding.Trim();
        if (!SupportedEncodings.Contains(cleanEncoding))
        {
            throw new InvalidOperationException("Bits par échantillon non reconnus. Valeurs autorisées : 16, 24 ou 32.");
        }

        return cleanEncoding;
    }

    private static string ValidateIpv4Address(string value, string label)
    {
        string cleanValue = value.Trim();
        if (!IPAddress.TryParse(cleanValue, out IPAddress? address)
            || address.AddressFamily is not System.Net.Sockets.AddressFamily.InterNetwork)
        {
            throw new InvalidOperationException($"Le champ {label} doit être une adresse IPv4 valide.");
        }

        return address.ToString();
    }

    private static string ValidateIpv4Prefix(string value)
    {
        string cleanValue = value.Trim();
        string[] parts = cleanValue.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3)
        {
            throw new InvalidOperationException("Le préfixe IP doit contenir les trois premiers octets, par exemple 192.168.1.");
        }

        foreach (string part in parts)
        {
            if (!int.TryParse(part, out int octet) || octet < 0 || octet > 255)
            {
                throw new InvalidOperationException("Le préfixe IP contient un octet invalide.");
            }
        }

        return string.Join(".", parts.Select(part => int.Parse(part).ToString()));
    }

    private static string FormatSamplerateForDisplay(string samplerate)
    {
        if (!int.TryParse(samplerate, out int value) || value <= 0)
        {
            return Blank(samplerate);
        }

        decimal khz = value / 1000m;
        return $"{khz:0.#} kHz ({value})";
    }

    private static string FormatEncodingForDisplay(string encoding)
    {
        return string.IsNullOrWhiteSpace(encoding) ? Blank(encoding) : $"{encoding} bit";
    }

    private void RegisterChange(string action, string details)
    {
        _changes.Add(new ChangeRecord(DateTime.Now, action, details));
        IsModified = true;
        _reloadPending = true;
        if (_batchDepth == 0)
        {
            ReloadModel();
        }
    }

    private void TrimUndoHistory()
    {
        if (_undoSnapshots.Count < MaximumUndoSnapshots)
        {
            return;
        }

        // Stack énumère du plus récent au plus ancien. On conserve donc les
        // derniers états, puis on les réempile dans leur ordre d'origine.
        UndoSnapshot[] snapshotsToKeep = _undoSnapshots
            .Take(MaximumUndoSnapshots - 1)
            .Reverse()
            .ToArray();
        _undoSnapshots.Clear();
        foreach (UndoSnapshot snapshot in snapshotsToKeep)
        {
            _undoSnapshots.Push(snapshot);
        }
    }

    private static bool ContainsProblematicCharacters(string value)
    {
        return value.Any(character => char.IsControl(character));
    }

    private bool ShouldUseLocalSubscriptionMarker(XElement rxElement, string rxDeviceName, string txDeviceName)
    {
        if (!string.Equals(rxDeviceName, txDeviceName, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        string currentRawDevice = FindFirstElement(rxElement, SubscriptionDeviceElementNames)?.Value.Trim() ?? string.Empty;
        return string.Equals(currentRawDevice, ".", StringComparison.Ordinal)
            || Document.Root!.Children("device").SelectMany(deviceElement => deviceElement.Children("rxchannel"))
                .Select(channel => FindFirstElement(channel, SubscriptionDeviceElementNames)?.Value.Trim() ?? string.Empty)
                .Any(value => string.Equals(value, ".", StringComparison.Ordinal));
    }

    private static string FormatDisplayTxDevice(string rawTxDeviceName, string resolvedTxDeviceName)
    {
        if (string.IsNullOrWhiteSpace(rawTxDeviceName))
        {
            return string.Empty;
        }

        return string.Equals(rawTxDeviceName, ".", StringComparison.Ordinal)
            ? $"LOCAL / {resolvedTxDeviceName}"
            : resolvedTxDeviceName;
    }

    private sealed record UndoSnapshot(
        string Label,
        XDocument Document,
        bool WasModified,
        int ChangeCount,
        IReadOnlyList<ModifiedRxReference> ModifiedRxReferences,
        IReadOnlyList<AuthorizedDeviceAdditionState> AuthorizedDeviceAdditions);

    private sealed record ModifiedRxReference(string RxDevice, int RxIndex);
}
