using System.Xml.Linq;
using DanteConfigEditor.Services;

namespace DanteConfigEditor.Models;

public enum DanteSubscriptionKind
{
    Free,
    Normal,
    Local,
    ExternalMissingDevice,
    MissingChannel,
    Conflict
}

public sealed class DanteSubscription
{
    internal DanteSubscription(
        string rxDevice,
        int rxDanteId,
        int rxPositionIndex,
        string rxChannelName,
        XElement rxElement,
        string rawTxDeviceName,
        string resolvedTxDeviceName,
        string displayTxDeviceName,
        string txChannelName,
        int? txDanteId,
        bool isModified,
        string status,
        DanteSubscriptionKind kind)
    {
        RxDevice = rxDevice;
        RxIndex = rxDanteId;
        RxDanteId = rxDanteId;
        RxPositionIndex = rxPositionIndex;
        RxChannelName = rxChannelName;
        RxElement = rxElement;
        RawTxDeviceName = rawTxDeviceName;
        ResolvedTxDeviceName = resolvedTxDeviceName;
        DisplayTxDeviceName = displayTxDeviceName;
        TxDevice = displayTxDeviceName;
        TxChannelName = txChannelName;
        TxDanteId = txDanteId;
        IsModified = isModified;
        Status = status;
        Kind = kind;
    }

    public string RxDevice { get; }

    // Ancien nom conservé pour les appels existants. Il correspond maintenant au danteId RX.
    public int RxIndex { get; }

    public int RxDanteId { get; }

    public int RxPositionIndex { get; }

    public string RxChannelName { get; }

    public string RawTxDeviceName { get; }

    public string ResolvedTxDeviceName { get; }

    public string DisplayTxDeviceName { get; }

    // Ancien nom conservé pour les bindings existants.
    public string TxDevice { get; }

    public string TxChannelName { get; }

    public int? TxDanteId { get; }

    public string TxDanteIdDisplay => TxDanteId?.ToString() ?? "-";

    public bool CanExtendRxNameSeries => ChannelNameSeriesService.CanExtend(RxChannelName);

    public bool CanExtendTxNameSeries => ChannelNameSeriesService.CanExtend(TxChannelName);

    // Un patch est considéré actif dès qu'un device TX est renseigné.
    // Le canal peut rester vide selon certains formats XML.
    public bool IsActive => !string.IsNullOrWhiteSpace(RawTxDeviceName) || !string.IsNullOrWhiteSpace(TxChannelName);

    public bool IsModified { get; }

    public DanteSubscriptionKind Kind { get; }

    public bool IsLocalSubscription => Kind == DanteSubscriptionKind.Local;

    public bool IsExternalMissingDevice => Kind == DanteSubscriptionKind.ExternalMissingDevice;

    public bool IsTxChannelMissing => Kind == DanteSubscriptionKind.MissingChannel;

    public bool IsConflict => Kind == DanteSubscriptionKind.Conflict;

    public bool IsWarning => Kind is DanteSubscriptionKind.ExternalMissingDevice or DanteSubscriptionKind.MissingChannel;

    public string TypeLabel => Kind switch
    {
        DanteSubscriptionKind.Free => "Libre",
        DanteSubscriptionKind.Normal => "Normal",
        DanteSubscriptionKind.Local => "Local",
        DanteSubscriptionKind.ExternalMissingDevice => "Device externe absent",
        DanteSubscriptionKind.MissingChannel => "Canal TX absent",
        DanteSubscriptionKind.Conflict => "Conflit",
        _ => Kind.ToString()
    };

    public string SourceFull
    {
        get
        {
            if (!IsActive)
            {
                return "Aucune source";
            }

            string device = IsLocalSubscription ? "LOCAL" : DisplayTxDeviceName;
            if (string.IsNullOrWhiteSpace(TxChannelName))
            {
                return string.IsNullOrWhiteSpace(device) ? "Aucune source" : device;
            }

            return $"{device} / {TxChannelName}";
        }
    }

    // Utilisé par le XAML pour colorer les lignes : conflit, modifié,
    // actif ou libre.
    public string RowState => IsConflict ? "Conflict" : IsWarning ? "Warning" : IsModified ? "Modified" : IsLocalSubscription ? "Local" : IsActive ? "Active" : "Free";

    public string Status { get; }

    public string Display => $"{RxDevice} / {RxChannelName}";

    internal XElement RxElement { get; }
}
