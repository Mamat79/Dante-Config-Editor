using System.Diagnostics;

namespace DanteConfigEditor.Services;

public static class SupportLinksService
{
    public const string PayPalSupportUrl = "https://www.paypal.com/qrcodes/p2pqrc/EQYCCDK8XFN5Y";

    public static bool IsTrustedPayPalUrl(string? url)
    {
        if (!string.Equals(url, PayPalSupportUrl, StringComparison.Ordinal)
            || !Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
        {
            return false;
        }

        return uri.Scheme == Uri.UriSchemeHttps
            && string.Equals(uri.Host, "www.paypal.com", StringComparison.OrdinalIgnoreCase)
            && string.IsNullOrEmpty(uri.UserInfo)
            && uri.Port == 443;
    }

    public static bool TryOpenPayPal(
        out string? error,
        Action<ProcessStartInfo>? launcher = null)
    {
        error = null;
        if (!IsTrustedPayPalUrl(PayPalSupportUrl))
        {
            error = "L'adresse de soutien PayPal n'est pas valide.";
            return false;
        }

        try
        {
            ProcessStartInfo startInfo = new(PayPalSupportUrl)
            {
                UseShellExecute = true
            };
            (launcher ?? (info => Process.Start(info)))(startInfo);
            return true;
        }
        catch (Exception exception)
        {
            error = exception.Message;
            return false;
        }
    }
}
