namespace DanteConfigEditor.Services;

public sealed record PatchDeviceSelectionSwapResult(
    bool Success,
    string? TxDeviceName,
    string? RxDeviceName,
    string? ErrorMessage);

public sealed record PatchDeviceSelectionPair(
    string? TxDeviceName,
    string? RxDeviceName);

public static class PatchDeviceSelectionSwapper
{
    public static PatchDeviceSelectionPair ResolveInitialPair(
        string? preferredTxDevice,
        string? preferredRxDevice,
        IEnumerable<string> txCapableDevices,
        IEnumerable<string> rxCapableDevices)
    {
        ArgumentNullException.ThrowIfNull(txCapableDevices);
        ArgumentNullException.ThrowIfNull(rxCapableDevices);

        string[] txDevices = txCapableDevices.ToArray();
        string[] rxDevices = rxCapableDevices.ToArray();
        string? selectedTx = FindName(txDevices, preferredTxDevice) ?? txDevices.FirstOrDefault();
        string? selectedRx = FindName(rxDevices, preferredRxDevice) ?? rxDevices.FirstOrDefault();

        if (!string.Equals(selectedTx, selectedRx, StringComparison.OrdinalIgnoreCase)
            || (!string.IsNullOrWhiteSpace(preferredTxDevice)
                && !string.IsNullOrWhiteSpace(preferredRxDevice)))
        {
            return new PatchDeviceSelectionPair(selectedTx, selectedRx);
        }

        // Quand un seul côté est imposé, l'autre reçoit si possible une machine
        // bidirectionnelle distincte. FLIP produit ainsi un résultat visible dès
        // l'ouverture sans remplacer une paire explicitement choisie.
        if (!string.IsNullOrWhiteSpace(preferredRxDevice)
            && string.IsNullOrWhiteSpace(preferredTxDevice))
        {
            selectedTx = FindDistinctBidirectionalName(txDevices, rxDevices, selectedRx) ?? selectedTx;
        }
        else
        {
            selectedRx = FindDistinctBidirectionalName(rxDevices, txDevices, selectedTx) ?? selectedRx;
        }

        return new PatchDeviceSelectionPair(selectedTx, selectedRx);
    }

    public static PatchDeviceSelectionSwapResult TrySwap(
        string? selectedTxDevice,
        string? selectedRxDevice,
        IEnumerable<string> txCapableDevices,
        IEnumerable<string> rxCapableDevices,
        bool rxSelectionLocked = false)
    {
        if (rxSelectionLocked)
        {
            return Failure("La machine RX est verrouillée dans cette fenêtre.");
        }

        if (string.IsNullOrWhiteSpace(selectedTxDevice) || string.IsNullOrWhiteSpace(selectedRxDevice))
        {
            return Failure("Sélectionnez une machine TX et une machine RX avant de les inverser.");
        }

        if (string.Equals(selectedTxDevice, selectedRxDevice, StringComparison.OrdinalIgnoreCase))
        {
            return Failure("Sélectionnez deux machines différentes : la même machine est affichée en TX et en RX.");
        }

        string? newTxDevice = FindName(txCapableDevices, selectedRxDevice);
        string? newRxDevice = FindName(rxCapableDevices, selectedTxDevice);
        if (newTxDevice is null || newRxDevice is null)
        {
            List<string> reasons = [];
            if (newTxDevice is null)
            {
                reasons.Add($"{selectedRxDevice} ne possède aucun canal TX");
            }
            if (newRxDevice is null)
            {
                reasons.Add($"{selectedTxDevice} ne possède aucun canal RX");
            }

            return Failure($"Inversion impossible : {string.Join(" ; ", reasons)}.");
        }

        return new PatchDeviceSelectionSwapResult(true, newTxDevice, newRxDevice, null);
    }

    private static string? FindName(IEnumerable<string> names, string? requested)
    {
        ArgumentNullException.ThrowIfNull(names);
        if (string.IsNullOrWhiteSpace(requested))
        {
            return null;
        }

        return names.FirstOrDefault(name => string.Equals(name, requested, StringComparison.OrdinalIgnoreCase));
    }

    private static string? FindDistinctBidirectionalName(
        IEnumerable<string> candidates,
        IEnumerable<string> oppositeCapableDevices,
        string? excludedName)
    {
        string[] oppositeDevices = oppositeCapableDevices.ToArray();
        return candidates.FirstOrDefault(candidate =>
            !string.Equals(candidate, excludedName, StringComparison.OrdinalIgnoreCase)
            && FindName(oppositeDevices, candidate) is not null);
    }

    private static PatchDeviceSelectionSwapResult Failure(string message)
    {
        return new PatchDeviceSelectionSwapResult(false, null, null, message);
    }
}
