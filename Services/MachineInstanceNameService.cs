namespace DanteConfigEditor.Services;

public static class MachineInstanceNameService
{
    public static IReadOnlyList<string> BuildNames(
        string? requestedName,
        int quantity,
        IEnumerable<string> existingNames)
    {
        if (quantity is < 1 or > Models.MachineInstanceBatchRequest.MaximumQuantity)
        {
            throw new InvalidOperationException(
                $"Le nombre de machines doit être compris entre 1 et {Models.MachineInstanceBatchRequest.MaximumQuantity}.");
        }

        string baseName = DanteNameRules.EnsureValidDeviceName(requestedName);
        HashSet<string> usedNames = existingNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
        List<string> names = new(quantity);

        for (int index = 1; index <= quantity; index++)
        {
            string suffix = index == 1 ? string.Empty : $"-{index}";
            int maximumBaseLength = DanteNameRules.MaximumNameLength - suffix.Length;
            string truncatedBase = baseName[..Math.Min(baseName.Length, maximumBaseLength)].TrimEnd('-');
            if (string.IsNullOrWhiteSpace(truncatedBase))
            {
                throw new InvalidOperationException("Le nom de base est trop court pour générer la série demandée.");
            }

            string candidate = DanteNameRules.EnsureValidDeviceName(truncatedBase + suffix);
            if (!usedNames.Add(candidate))
            {
                throw new InvalidOperationException(
                    $"Une machine porte déjà le nom '{candidate}'. Choisissez un autre nom de base.");
            }

            names.Add(candidate);
        }

        return names;
    }
}
