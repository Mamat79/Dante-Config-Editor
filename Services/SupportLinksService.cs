using System.Diagnostics;

namespace DanteConfigEditor.Services;

public static class SupportLinksService
{
    public const string PayPalMeSupportUrl = "https://www.paypal.com/paypalme/MamatLeroy";

    public static bool IsTrustedPayPalMeUrl(string? url)
    {
        if (!string.Equals(url, PayPalMeSupportUrl, StringComparison.Ordinal)
            || !Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
        {
            return false;
        }

        return uri.Scheme == Uri.UriSchemeHttps
            && string.Equals(uri.Host, "www.paypal.com", StringComparison.OrdinalIgnoreCase)
            && string.Equals(uri.AbsolutePath, "/paypalme/MamatLeroy", StringComparison.Ordinal)
            && string.IsNullOrEmpty(uri.UserInfo)
            && uri.Port == 443;
    }

    public static bool TryOpenPayPalMe(
        out string? error,
        Action<ProcessStartInfo>? launcher = null)
    {
        error = null;
        if (!IsTrustedPayPalMeUrl(PayPalMeSupportUrl))
        {
            error = "L'adresse PayPal.Me de soutien n'est pas valide.";
            return false;
        }

        try
        {
            ProcessStartInfo startInfo = new(PayPalMeSupportUrl)
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
