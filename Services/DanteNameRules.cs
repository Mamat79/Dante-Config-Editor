using System.Text.RegularExpressions;

namespace DanteConfigEditor.Services;

public static partial class DanteNameRules
{
    public const int MaximumNameLength = 31;

    public static string? ValidateDeviceName(string? value)
    {
        string clean = value?.Trim() ?? string.Empty;
        if (clean.Length == 0)
        {
            return "Le nom de machine est obligatoire.";
        }

        if (clean.Length > MaximumNameLength)
        {
            return $"Le nom de machine ne doit pas dépasser {MaximumNameLength} caractères.";
        }

        if (!IsPrintableAscii(clean))
        {
            return "Le nom de machine doit contenir uniquement des caractères ASCII imprimables.";
        }

        if (!DeviceNamePattern().IsMatch(clean))
        {
            return "Le nom de machine accepte uniquement les lettres, les chiffres et le tiret.";
        }

        if (clean.StartsWith("-", StringComparison.Ordinal))
        {
            return "Le nom de machine ne doit pas commencer par un tiret.";
        }

        if (clean.EndsWith("-", StringComparison.Ordinal))
        {
            return "Le nom de machine ne doit pas se terminer par un tiret.";
        }

        return null;
    }

    public static string? ValidateChannelName(string? value)
    {
        string clean = value?.Trim() ?? string.Empty;
        if (clean.Length == 0)
        {
            return "Le nom de canal est obligatoire.";
        }

        if (clean.Length > MaximumNameLength)
        {
            return $"Le nom de canal ne doit pas dépasser {MaximumNameLength} caractères.";
        }

        if (!IsPrintableAscii(clean))
        {
            return "Le nom de canal doit contenir uniquement des caractères ASCII imprimables.";
        }

        if (clean.IndexOfAny(['=', '.', '@']) >= 0)
        {
            return "Le nom de canal ne doit pas contenir les caractères '=', '.' ou '@'.";
        }

        return null;
    }

    public static string EnsureValidDeviceName(string? value)
    {
        string clean = value?.Trim() ?? string.Empty;
        string? error = ValidateDeviceName(clean);
        if (error is not null)
        {
            throw new InvalidOperationException(error);
        }

        return clean;
    }

    public static string EnsureValidChannelName(string? value)
    {
        string clean = value?.Trim() ?? string.Empty;
        string? error = ValidateChannelName(clean);
        if (error is not null)
        {
            throw new InvalidOperationException(error);
        }

        return clean;
    }

    public static string BuildUniqueSuffixedDeviceName(
        string? originalName,
        string? suffix,
        ISet<string> usedNames)
    {
        ArgumentNullException.ThrowIfNull(usedNames);
        string cleanBase = NormalizeDeviceNamePart(originalName, "Imported");
        string cleanSuffix = NormalizeDeviceNamePart(suffix, string.Empty);
        if (string.IsNullOrWhiteSpace(cleanSuffix))
        {
            throw new InvalidOperationException("Le suffixe de renommage ne peut pas être vide.");
        }

        for (int index = 1; index < int.MaxValue; index++)
        {
            string ending = index == 1
                ? "-" + cleanSuffix
                : $"-{cleanSuffix}-{index}";
            int maximumBaseLength = MaximumNameLength - ending.Length;
            if (maximumBaseLength < 1)
            {
                throw new InvalidOperationException(
                    $"Le suffixe est trop long pour produire un nom de machine de {MaximumNameLength} caractères.");
            }

            string truncatedBase = cleanBase[..Math.Min(cleanBase.Length, maximumBaseLength)]
                .Trim('-');
            if (string.IsNullOrWhiteSpace(truncatedBase))
            {
                truncatedBase = "D";
            }

            string candidate = truncatedBase + ending;
            if (!usedNames.Contains(candidate))
            {
                return EnsureValidDeviceName(candidate);
            }
        }

        throw new InvalidOperationException("Impossible de générer un nom de machine unique.");
    }

    public static string NormalizeDeviceNamePart(string? value, string fallback)
    {
        string input = value?.Trim() ?? string.Empty;
        char[] normalized = input
            .Select(character => character is >= 'A' and <= 'Z'
                    or >= 'a' and <= 'z'
                    or >= '0' and <= '9'
                    or '-'
                ? character
                : '-')
            .ToArray();
        string clean = CollapseHyphens(new string(normalized)).Trim('-');
        return string.IsNullOrWhiteSpace(clean) ? fallback : clean;
    }

    private static bool IsPrintableAscii(string value)
    {
        return value.All(character => character is >= '\x20' and <= '\x7E');
    }

    private static string CollapseHyphens(string value)
    {
        while (value.Contains("--", StringComparison.Ordinal))
        {
            value = value.Replace("--", "-", StringComparison.Ordinal);
        }

        return value;
    }

    [GeneratedRegex("^[A-Za-z0-9-]+$", RegexOptions.CultureInvariant)]
    private static partial Regex DeviceNamePattern();
}
