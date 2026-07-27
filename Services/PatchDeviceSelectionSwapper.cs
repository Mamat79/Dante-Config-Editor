namespace DanteConfigEditor.Services;

public sealed record PatchDeviceSelectionSwapResult(
    bool Success,
    string? TxDeviceName,
    string? RxDeviceName,
    string? ErrorMessage);

public static class PatchDeviceSelectionSwapper
{
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

    private static string? FindName(IEnumerable<string> names, string requested)
    {
        ArgumentNullException.ThrowIfNull(names);
        return names.FirstOrDefault(name => string.Equals(name, requested, StringComparison.OrdinalIgnoreCase));
    }

    private static PatchDeviceSelectionSwapResult Failure(string message)
    {
        return new PatchDeviceSelectionSwapResult(false, null, null, message);
    }
}
