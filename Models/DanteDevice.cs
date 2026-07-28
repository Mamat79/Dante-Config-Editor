using System.Net;
using System.Xml.Linq;
using DanteConfigEditor.Services;

namespace DanteConfigEditor.Models;

public sealed class DanteDevice
{
    internal DanteDevice(XElement element)
    {
        Element = element;
        Name = MachineRoleIdentityService.ReadVisibleName(element);
        FriendlyName = ReadElementValue(element, "friendly_name");
        Manufacturer = ReadElementValue(element, "manufacturer_name");
        Model = ReadElementValue(element, "model_name");
        string technicalDeviceId = element.Child("instance_id").ChildValue("device_id");
        TechnicalDeviceId = technicalDeviceId.Trim();
        ProcessId = element.Child("instance_id").ChildValue("process_id");
        IsGenericRole = string.IsNullOrWhiteSpace(TechnicalDeviceId);
        StableIdentity = MachineRoleIdentityService.GetOrCreateSessionIdentity(element);
        IsRedundant = string.Equals(element.Child("redundancy")?.Attribute("value")?.Value, "true", StringComparison.OrdinalIgnoreCase);
        PreferredMaster = string.Equals(element.Child("preferred_master")?.Attribute("value")?.Value, "true", StringComparison.OrdinalIgnoreCase);
        Latency = ReadElementValue(element, "unicast_latency");
        Samplerate = ReadElementValue(element, "samplerate");
        Encoding = ReadElementValue(element, "encoding");
        UsesStaticIp = DetectStaticIp(element);
        StaticIpAddress = UsesStaticIp ? ReadIpAddress(element) : string.Empty;
        StaticIpNetmask = ReadIpField(element, "netmask");
        StaticIpGateway = ReadIpField(element, "gateway");
        StaticIpDnsServer = ReadIpField(element, "dnsserver");

        // L'index est basé sur l'ordre des balises dans le XML, ce qui
        // correspond à la numérotation affichée dans l'application.
        TxChannels = element.Children("txchannel")
            .Select((channel, index) => new DanteChannel(Name, DanteChannelKind.Tx, index + 1, channel))
            .ToList();

        RxChannels = element.Children("rxchannel")
            .Select((channel, index) => new DanteChannel(Name, DanteChannelKind.Rx, index + 1, channel))
            .ToList();
    }

    public string Name { get; }

    public string FriendlyName { get; }

    // Métadonnées intrinsèques exposées sans donner à l'interface un accès
    // direct au document XML mutable.
    public string Manufacturer { get; }

    public string Model { get; }

    // Identité de session utilisée par les données annexes de l'application.
    // Elle n'est jamais écrite dans le XML et reste stable après un renommage.
    public string StableIdentity { get; }

    public string TechnicalDeviceId { get; }

    public string ProcessId { get; }

    public bool IsGenericRole { get; }

    public bool IsRedundant { get; }

    public string NetworkMode => IsRedundant ? "Redondant" : "Daisychain";

    public string Latency { get; }

    public string LatencyDisplay => DanteLatencyFormatter.FormatLatencyDisplay(Latency);

    public string Samplerate { get; }

    public string SampleRateDisplay => FormatSampleRateDisplay(Samplerate);

    public string Encoding { get; }

    public string EncodingDisplay => FormatEncodingDisplay(Encoding);

    public bool PreferredMaster { get; }

    // Une balise déjà présente est la seule preuve fiable que ce rôle Dante
    // expose le paramètre. Son absence ne doit jamais être compensée par une
    // structure XML inventée par l'application.
    public bool SupportsNetworkMode => Element.Child("redundancy") is not null;

    public bool SupportsPreferredMaster => Element.Child("preferred_master") is not null;

    public bool SupportsLatency => Element.Child("unicast_latency") is not null;

    public bool SupportsSampleRate => Element.Child("samplerate") is not null;

    public bool SupportsEncoding => Element.Child("encoding") is not null;

    public bool SupportsIpConfiguration =>
        DanteIpConfiguration.FindPrimaryIpv4Address(Element) is not null;

    public bool UsesStaticIp { get; }

    public string StaticIpAddress { get; }

    public string StaticIpNetmask { get; }

    public string StaticIpGateway { get; }

    public string StaticIpDnsServer { get; }

    public string IpModeDisplay => UsesStaticIp
        ? string.IsNullOrWhiteSpace(StaticIpAddress) ? "Fixe" : $"Fixe ({StaticIpAddress})"
        : "Auto";

    public int TxCount => TxChannels.Count;

    public int RxCount => RxChannels.Count;

    public IReadOnlyList<DanteChannel> TxChannels { get; }

    public IReadOnlyList<DanteChannel> RxChannels { get; }

    internal XElement Element { get; }

    private static string ReadElementValue(XElement element, string name)
    {
        return element.Child(name)?.Value?.Trim() ?? string.Empty;
    }

    private static string FormatSampleRateDisplay(string samplerate)
    {
        if (!int.TryParse(samplerate, out int value) || value <= 0)
        {
            return string.IsNullOrWhiteSpace(samplerate) ? "-" : samplerate;
        }

        decimal khz = value / 1000m;
        return $"{khz:0.#} kHz";
    }

    private static string FormatEncodingDisplay(string encoding)
    {
        return string.IsNullOrWhiteSpace(encoding) ? "-" : $"{encoding} bit";
    }

    private static bool DetectStaticIp(XElement element)
    {
        XElement? primaryIpv4Address = DanteIpConfiguration.FindPrimaryIpv4Address(element);
        if (primaryIpv4Address is null)
        {
            return false;
        }

        string mode = primaryIpv4Address.Attribute("mode")?.Value.Trim() ?? string.Empty;
        if (string.Equals(mode, "dynamic", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        foreach (XElement candidate in primaryIpv4Address.DescendantsAndSelf())
        {
            if (IsStaticIpMarker(candidate.Name.LocalName, candidate.Value, BuildXmlPath(candidate)))
            {
                return true;
            }

            foreach (XAttribute attribute in candidate.Attributes())
            {
                if (IsStaticIpMarker(attribute.Name.LocalName, attribute.Value, BuildXmlPath(candidate)))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool IsStaticIpMarker(string name, string value, string path)
    {
        string cleanName = name.Trim().ToLowerInvariant();
        string cleanValue = value.Trim().ToLowerInvariant();
        string cleanPath = path.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(cleanValue))
        {
            return false;
        }

        bool looksNetworkRelated = cleanName.Contains("ip", StringComparison.Ordinal)
            || cleanName.Contains("address", StringComparison.Ordinal)
            || cleanName.Contains("dhcp", StringComparison.Ordinal)
            || cleanPath.Contains("network", StringComparison.Ordinal)
            || cleanPath.Contains("interface", StringComparison.Ordinal);

        if (!looksNetworkRelated)
        {
            return false;
        }

        if (cleanName.Contains("static", StringComparison.Ordinal)
            && IPAddress.TryParse(cleanValue, out IPAddress? address)
            && address.AddressFamily is System.Net.Sockets.AddressFamily.InterNetwork)
        {
            return true;
        }

        if (cleanName.Contains("dhcp", StringComparison.Ordinal)
            && (cleanValue is "false" or "0" or "off" or "no" or "disabled"))
        {
            return true;
        }

        bool staticValue = cleanValue is "static" or "manual" or "fixed" or "fixe" or "manuelle"
            || cleanValue.Contains("static", StringComparison.Ordinal)
            || cleanValue.Contains("manual", StringComparison.Ordinal)
            || cleanValue.Contains("fixed", StringComparison.Ordinal);

        if (!staticValue)
        {
            return false;
        }

        return cleanName.Contains("mode", StringComparison.Ordinal)
            || cleanName.Contains("method", StringComparison.Ordinal)
            || cleanName.Contains("config", StringComparison.Ordinal)
            || cleanName.Contains("type", StringComparison.Ordinal)
            || cleanName.Contains("static", StringComparison.Ordinal)
            || cleanPath.Contains("ip", StringComparison.Ordinal)
            || cleanPath.Contains("address", StringComparison.Ordinal);
    }

    private static string ReadIpAddress(XElement element)
    {
        XElement? primaryIpv4Address = DanteIpConfiguration.FindPrimaryIpv4Address(element);
        if (primaryIpv4Address is null)
        {
            return string.Empty;
        }

        List<(int Priority, string Value)> candidates = [];
        foreach (XElement candidate in primaryIpv4Address.DescendantsAndSelf())
        {
            AddIpCandidate(candidates, candidate.Name.LocalName, candidate.Value);

            foreach (XAttribute attribute in candidate.Attributes())
            {
                AddIpCandidate(candidates, attribute.Name.LocalName, attribute.Value);
            }
        }

        return candidates
            .OrderBy(candidate => candidate.Priority)
            .Select(candidate => candidate.Value)
            .FirstOrDefault() ?? string.Empty;
    }

    private static string ReadIpField(XElement element, string fieldName)
    {
        XElement? ipv4Address = DanteIpConfiguration.FindPrimaryIpv4Address(element);

        return ipv4Address?.Attribute(fieldName)?.Value.Trim()
            ?? ipv4Address.Child(fieldName)?.Value.Trim()
            ?? string.Empty;
    }

    private static void AddIpCandidate(List<(int Priority, string Value)> candidates, string name, string value)
    {
        string cleanName = name.Trim().ToLowerInvariant();
        string cleanValue = value.Trim();

        if (cleanName.Contains("gateway", StringComparison.Ordinal)
            || cleanName.Contains("mask", StringComparison.Ordinal)
            || cleanName.Contains("subnet", StringComparison.Ordinal))
        {
            return;
        }

        if (!IPAddress.TryParse(cleanValue, out IPAddress? address)
            || address.AddressFamily is not System.Net.Sockets.AddressFamily.InterNetwork)
        {
            return;
        }

        int priority = cleanName.Contains("static", StringComparison.Ordinal) ? 0
            : cleanName.Contains("ip", StringComparison.Ordinal) && cleanName.Contains("address", StringComparison.Ordinal) ? 1
            : cleanName is "ip" or "address" ? 2
            : 3;

        candidates.Add((priority, address.ToString()));
    }

    private static string BuildXmlPath(XElement element)
    {
        return string.Join("/", element.AncestorsAndSelf().Reverse().Select(ancestor => ancestor.Name.LocalName));
    }
}
