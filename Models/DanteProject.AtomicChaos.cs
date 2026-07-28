using System.Security.Cryptography;
using System.Xml.Linq;
using DanteConfigEditor.Services;

namespace DanteConfigEditor.Models;

public sealed partial class DanteProject
{
    private static readonly string[] AtomicLatencies = ["250", "1000", "2000", "5000"];
    private static readonly string[] AtomicSamplerates = ["44100", "48000", "88200", "96000", "176400", "192000"];
    private static readonly string[] AtomicEncodings = ["16", "24", "32"];
    private static readonly string[] AtomicDeviceNames =
    [
        "APOLLON", "ATHENA", "HERMES", "ORPHEE", "HEPHAISTOS", "POSEIDON", "ARTEMIS", "HERA",
        "ZEUS", "HADES", "NYX", "HELIOS", "ATLAS", "ECHO", "MORPHEE", "ARIANE", "PEGASE",
        "PHENIX", "OLYMPE", "DELPHES", "STYX", "CIRCE", "CALYPSO", "RAVENNA", "PYRAMIX",
        "INFERNO", "PURGATORIO", "PARADISO", "BEATRICE", "VIRGILE", "CHRONOS", "DAEDALUS",
        "SONICUS", "MIXOLYDIEN", "PATCHOS", "LATENCIA", "DUPLEX", "FLOWDINI", "CLOCKOS",
        "RESONIX", "GAINOS", "PANDEMONIUM"
    ];

    public AtomicChaosResult ApplyAtomicChaos(int? seed = null) =>
        ApplyAtomicChaos(AtomicChaosOptions.All, seed);

    public AtomicChaosResult ApplyAtomicChaos(AtomicChaosOptions options, int? seed = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (Devices.Count == 0)
        {
            throw new InvalidOperationException("Atomic Bomb nécessite au moins une machine.");
        }
        if (!options.HasSelection)
        {
            throw new InvalidOperationException("Sélectionnez au moins une catégorie à modifier.");
        }

        int actualSeed = seed ?? RandomNumberGenerator.GetInt32(1, int.MaxValue);
        Random random = new(actualSeed);
        IReadOnlyList<string> deviceNames = options.DeviceNames
            ? BuildAtomicDeviceNames(Devices.Count, random)
            : Devices.Select(device => device.Name).ToArray();
        AtomicDevicePlan[] plans = Devices
            .Select((device, index) => BuildAtomicDevicePlan(device, deviceNames[index], random, options))
            .ToArray();
        List<string> documentationAddresses = BuildDocumentationAddresses(random);
        int networkOffset = random.Next(2);
        int clockOffset = random.Next(2);
        int latencyOffset = random.Next(AtomicLatencies.Length);
        int samplerateOffset = random.Next(AtomicSamplerates.Length);
        int encodingOffset = random.Next(AtomicEncodings.Length);
        int ipOffset = random.Next(2);
        int nextStaticAddress = 0;
        int staticIpCount = 0;
        int dynamicIpCount = 0;
        int redundantDeviceCount = 0;
        int preferredMasterCount = 0;
        HashSet<string> appliedLatencies = new(StringComparer.Ordinal);
        HashSet<string> appliedSamplerates = new(StringComparer.Ordinal);
        HashSet<string> appliedEncodings = new(StringComparer.Ordinal);

        // Si les patchs sont exclus, les références existantes suivent les
        // renommages. L'option "Patchs" ne doit jamais être activée en douce
        // par un simple changement de nom de machine ou de TX.
        if (!options.Subscriptions)
        {
            foreach (AtomicDevicePlan plan in plans)
            {
                if (options.TxLabels)
                {
                    UpdateSubscriptionsForRenamedTxChannels(
                        plan.Device.Name,
                        plan.TxChannels.Select(channel => (channel.OriginalName, channel.NewName)));
                }
                if (options.DeviceNames)
                {
                    UpdateSubscriptionsForRenamedDevice(plan.Device.Name, plan.NewName);
                }
            }
        }

        // Les objets DanteDevice gardent une référence vers les éléments XML.
        // On peut donc préparer tous les nouveaux noms avant de modifier le
        // document, puis tout appliquer sans reconstruire le modèle entre deux
        // machines.
        for (int planIndex = 0; planIndex < plans.Length; planIndex++)
        {
            AtomicDevicePlan plan = plans[planIndex];
            bool redundant = (planIndex + networkOffset) % 2 == 0;
            bool preferredMaster = (planIndex + clockOffset) % 2 == 0;
            string latency = AtomicLatencies[(planIndex + latencyOffset) % AtomicLatencies.Length];
            string samplerate = AtomicSamplerates[(planIndex + samplerateOffset) % AtomicSamplerates.Length];
            string encoding = AtomicEncodings[(planIndex + encodingOffset) % AtomicEncodings.Length];

            if (options.DeviceNames)
            {
                SetElementValue(plan.Device.Element, "name", plan.NewName);
                SetElementValue(plan.Device.Element, "friendly_name", plan.NewName);
            }
            if (options.NetworkMode && plan.Device.SupportsNetworkMode)
            {
                SetBooleanElementAttribute(plan.Device.Element, "redundancy", "value", redundant, afterElementName: "friendly_name");
                redundantDeviceCount += redundant ? 1 : 0;
            }
            if (options.PreferredMaster && plan.Device.SupportsPreferredMaster)
            {
                SetBooleanElementAttribute(plan.Device.Element, "preferred_master", "value", preferredMaster, afterElementName: "redundancy");
                preferredMasterCount += preferredMaster ? 1 : 0;
            }
            if (options.Latency && plan.Device.SupportsLatency)
            {
                SetExistingTechnicalElementValue(plan.Device, "unicast_latency", latency, "la latence");
                appliedLatencies.Add(latency);
            }
            if (options.SampleRate && plan.Device.SupportsSampleRate)
            {
                SetExistingTechnicalElementValue(plan.Device, "samplerate", samplerate, "la fréquence d'échantillonnage");
                appliedSamplerates.Add(samplerate);
            }
            if (options.Encoding && plan.Device.SupportsEncoding)
            {
                SetExistingTechnicalElementValue(plan.Device, "encoding", encoding, "l'encodage");
                appliedEncodings.Add(encoding);
            }

            if (options.TxLabels)
            {
                foreach (AtomicChannelPlan channel in plan.TxChannels)
                {
                    SetChannelDisplayName(channel.Channel, "label", channel.NewName);
                }
            }

            if (options.RxLabels)
            {
                foreach (AtomicChannelPlan channel in plan.RxChannels)
                {
                    SetChannelDisplayName(channel.Channel, "name", channel.NewName);
                }
            }

            if (!options.PrimaryIp || !DeviceSupportsIpConfiguration(plan.Device))
            {
                continue;
            }

            // L'alternance garantit un mélange exploitable pour un exercice,
            // tandis que la graine rend chaque scénario reproductible.
            bool useStaticAddress = (planIndex + ipOffset) % 2 == 0
                && nextStaticAddress < documentationAddresses.Count;
            if (useStaticAddress)
            {
                SetAtomicStaticIp(plan.Device, documentationAddresses[nextStaticAddress++]);
                staticIpCount++;
            }
            else
            {
                SetDeviceIpAddressesDynamic(plan.Device);
                dynamicIpCount++;
            }
        }

        AtomicTxEndpoint[] txEndpoints = options.Subscriptions
            ? plans
            .SelectMany(plan => plan.TxChannels.Select(channel => new AtomicTxEndpoint(plan.NewName, channel.NewName)))
            .ToArray()
            : [];
        int patchedRxCount = 0;
        int disconnectedRxCount = 0;
        int rxOrdinal = 0;
        int disconnectOffset = random.Next(4);

        if (options.Subscriptions)
        {
            foreach (AtomicDevicePlan plan in plans)
            {
                foreach (AtomicChannelPlan rxChannel in plan.RxChannels)
                {
                    // Environ un quart des RX restent volontairement libres.
                    // Les autres pointent vers un TX réellement présent.
                    if (txEndpoints.Length == 0 || (rxOrdinal + disconnectOffset) % 4 == 0)
                    {
                        ClearRecognizedSubscription(rxChannel.Channel.Element);
                        disconnectedRxCount++;
                    }
                    else
                    {
                        AtomicTxEndpoint source = Pick(txEndpoints, random);
                        string sourceDevice = string.Equals(source.DeviceName, plan.NewName, StringComparison.OrdinalIgnoreCase)
                            ? "."
                            : source.DeviceName;
                        SetSubscriptionElements(rxChannel.Channel.Element, sourceDevice, source.ChannelName);
                        patchedRxCount++;
                    }

                    _modifiedRxElements[rxChannel.Channel.Element] = true;
                    rxOrdinal++;
                }
            }
        }

        string categories = string.Join(", ", options.EnabledCategoryNames);
        RegisterChange(
            "Atomic Bomb",
            $"Seed {actualSeed}: {categories}; {plans.Length} machine(s), {txEndpoints.Length} TX, {patchedRxCount} RX patché(s), {disconnectedRxCount} RX libre(s)");

        return new AtomicChaosResult(
            actualSeed,
            plans.Length,
            plans.Sum(plan => plan.TxChannels.Count),
            plans.Sum(plan => plan.RxChannels.Count),
            patchedRxCount,
            disconnectedRxCount,
            staticIpCount,
            dynamicIpCount,
            redundantDeviceCount,
            preferredMasterCount,
            appliedSamplerates.Count,
            appliedEncodings.Count,
            appliedLatencies.Count);
    }

    private static IReadOnlyList<string> BuildAtomicDeviceNames(int count, Random random)
    {
        List<string> shuffledNames = AtomicDeviceNames.ToList();
        for (int index = shuffledNames.Count - 1; index > 0; index--)
        {
            int swapIndex = random.Next(index + 1);
            (shuffledNames[index], shuffledNames[swapIndex]) = (shuffledNames[swapIndex], shuffledNames[index]);
        }

        return Enumerable.Range(0, count)
            .Select(index =>
            {
                string baseName = shuffledNames[index % shuffledNames.Count];
                int series = index / shuffledNames.Count;
                return series == 0 ? baseName : $"{baseName}-{series + 1:00}";
            })
            .ToArray();
    }

    private static AtomicDevicePlan BuildAtomicDevicePlan(
        DanteDevice device,
        string newDeviceName,
        Random random,
        AtomicChaosOptions options)
    {
        AtomicChannelPlan[] txChannels = device.TxChannels
            .Select((channel, index) => new AtomicChannelPlan(
                channel,
                channel.DisplayName,
                options.TxLabels ? $"CHAOS-TX-{index + 1:000}-{random.Next(0x100):X2}" : channel.DisplayName))
            .ToArray();
        AtomicChannelPlan[] rxChannels = device.RxChannels
            .Select((channel, index) => new AtomicChannelPlan(
                channel,
                channel.DisplayName,
                options.RxLabels ? $"CHAOS-RX-{index + 1:000}-{random.Next(0x100):X2}" : channel.DisplayName))
            .ToArray();
        return new AtomicDevicePlan(device, newDeviceName, txChannels, rxChannels);
    }

    private void UpdateSubscriptionsForRenamedDevice(string oldName, string newName)
    {
        foreach (XElement rxChannel in Document.Root!.Children("device").SelectMany(device => device.Children("rxchannel")))
        {
            XElement? sourceDevice = FindFirstElement(rxChannel, SubscriptionDeviceElementNames);
            if (sourceDevice is not null
                && !string.Equals(sourceDevice.Value.Trim(), ".", StringComparison.Ordinal)
                && string.Equals(sourceDevice.Value.Trim(), oldName, StringComparison.OrdinalIgnoreCase))
            {
                sourceDevice.Value = newName;
                _modifiedRxElements[rxChannel] = true;
            }
        }
    }

    private static List<string> BuildDocumentationAddresses(Random random)
    {
        // RFC 5737 réserve ces trois réseaux aux exemples et à la documentation.
        // Ils évitent de générer par hasard une adresse privée réellement utilisée.
        string[] prefixes = ["192.0.2", "198.51.100", "203.0.113"];
        List<string> addresses = prefixes
            .SelectMany(prefix => Enumerable.Range(1, 254).Select(host => $"{prefix}.{host}"))
            .ToList();

        for (int index = addresses.Count - 1; index > 0; index--)
        {
            int swapIndex = random.Next(index + 1);
            (addresses[index], addresses[swapIndex]) = (addresses[swapIndex], addresses[index]);
        }

        return addresses;
    }

    private static void SetAtomicStaticIp(DanteDevice device, string address)
    {
        XElement ipv4Address = DanteIpConfiguration.FindPrimaryIpv4Address(device.Element)
            ?? throw UnsupportedTechnicalSetting(device, "IPv4", "ipv4_address");
        ipv4Address.SetAttributeValue("mode", "static");
        SetIpField(ipv4Address, IpAddressAttributeNames, "address", address);
        SetIpField(ipv4Address, IpNetmaskAttributeNames, "netmask", "255.255.255.0");
        // La passerelle et le DNS ne sont jamais modifiés implicitement.
    }

    private static void ClearRecognizedSubscription(XElement rxChannel)
    {
        HashSet<string> recognizedNames = SubscriptionDeviceElementNames
            .Concat(SubscriptionChannelElementNames)
            .ToHashSet(StringComparer.Ordinal);
        foreach (XElement element in rxChannel.Elements().Where(element => recognizedNames.Contains(element.Name.LocalName)).ToArray())
        {
            element.Remove();
        }
    }

    private static T Pick<T>(IReadOnlyList<T> values, Random random)
    {
        return values[random.Next(values.Count)];
    }

    private sealed record AtomicDevicePlan(
        DanteDevice Device,
        string NewName,
        IReadOnlyList<AtomicChannelPlan> TxChannels,
        IReadOnlyList<AtomicChannelPlan> RxChannels);

    private sealed record AtomicChannelPlan(DanteChannel Channel, string OriginalName, string NewName);

    private sealed record AtomicTxEndpoint(string DeviceName, string ChannelName);
}

public sealed record AtomicChaosOptions(
    bool DeviceNames,
    bool TxLabels,
    bool RxLabels,
    bool Subscriptions,
    bool NetworkMode,
    bool PreferredMaster,
    bool Latency,
    bool SampleRate,
    bool Encoding,
    bool PrimaryIp)
{
    public static AtomicChaosOptions All { get; } = new(true, true, true, true, true, true, true, true, true, true);

    public bool HasSelection => DeviceNames || TxLabels || RxLabels || Subscriptions || NetworkMode
        || PreferredMaster || Latency || SampleRate || Encoding || PrimaryIp;

    public IReadOnlyList<string> EnabledCategoryNames => new[]
    {
        (DeviceNames, "noms machines"),
        (TxLabels, "labels TX"),
        (RxLabels, "labels RX"),
        (Subscriptions, "patchs"),
        (NetworkMode, "modes réseau"),
        (PreferredMaster, "Preferred Master"),
        (Latency, "latences"),
        (SampleRate, "fréquences"),
        (Encoding, "bits"),
        (PrimaryIp, "IP principales")
    }.Where(item => item.Item1).Select(item => item.Item2).ToArray();
}

public sealed record AtomicChaosResult(
    int Seed,
    int DeviceCount,
    int TxChannelCount,
    int RxChannelCount,
    int PatchedRxCount,
    int DisconnectedRxCount,
    int StaticIpCount,
    int DynamicIpCount,
    int RedundantDeviceCount,
    int PreferredMasterCount,
    int SampleRateValueCount,
    int EncodingValueCount,
    int LatencyValueCount);
