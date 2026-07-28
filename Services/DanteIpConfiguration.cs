using System.Xml.Linq;

namespace DanteConfigEditor.Services;

internal static class DanteIpConfiguration
{
    public static XElement? FindPrimaryInterface(XElement deviceElement)
    {
        // Les exports Dante identifient généralement l'interface principale par
        // network="0". À défaut, la première interface reste le choix conservateur.
        XElement[] interfaces = deviceElement.Children("interface").ToArray();
        if (interfaces.Length == 0)
        {
            interfaces = deviceElement.DescendantsNamed("interface").ToArray();
        }

        return interfaces.FirstOrDefault(element =>
                string.Equals(element.Attribute("network")?.Value, "0", StringComparison.OrdinalIgnoreCase))
            ?? interfaces.FirstOrDefault();
    }

    public static XElement? FindPrimaryIpv4Address(XElement deviceElement)
    {
        XElement? primaryInterface = FindPrimaryInterface(deviceElement);
        return primaryInterface?.Child("ipv4_address")
            ?? primaryInterface?.DescendantsNamed("ipv4_address").FirstOrDefault();
    }

}
