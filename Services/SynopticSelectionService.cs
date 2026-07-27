using DanteConfigEditor.Models;

namespace DanteConfigEditor.Services;

/// <summary>
/// Relie les abonnements du Patch aux câbles regroupés du synoptique.
/// Cette logique reste indépendante de WPF afin que toutes les vues utilisent
/// les mêmes identifiants et les mêmes règles de sélection.
/// </summary>
public static class SynopticSelectionService
{
    public static SynopticCable? FindCable(
        SynopticDiagram diagram,
        DanteSubscription subscription)
    {
        ArgumentNullException.ThrowIfNull(diagram);
        ArgumentNullException.ThrowIfNull(subscription);

        return diagram.Cables.FirstOrDefault(cable =>
            cable.Subscriptions.Any(item => Matches(item, subscription)));
    }

    public static SynopticCable? FindCable(
        SynopticDiagram diagram,
        string rxDeviceName,
        int rxDanteId)
    {
        ArgumentNullException.ThrowIfNull(diagram);

        return diagram.Cables.FirstOrDefault(cable =>
            cable.Subscriptions.Any(item =>
                item.RxDanteId == rxDanteId
                && string.Equals(
                    item.TargetDevice,
                    rxDeviceName,
                    StringComparison.OrdinalIgnoreCase)));
    }

    private static bool Matches(
        SynopticCableSubscription item,
        DanteSubscription subscription)
    {
        return item.RxDanteId == subscription.RxDanteId
            && string.Equals(
                item.TargetDevice,
                subscription.RxDevice,
                StringComparison.OrdinalIgnoreCase)
            && string.Equals(
                item.SourceDevice,
                subscription.ResolvedTxDeviceName,
                StringComparison.OrdinalIgnoreCase)
            && string.Equals(
                item.TxChannelName,
                subscription.TxChannelName,
                StringComparison.OrdinalIgnoreCase);
    }
}
