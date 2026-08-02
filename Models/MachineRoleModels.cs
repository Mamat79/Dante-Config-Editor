namespace DanteConfigEditor.Models;

public sealed class MachineCloneOptions
{
    public required string NewName { get; init; }

    public bool PreserveTxLabels { get; init; } = true;

    public bool PreserveRxLabels { get; init; } = true;

    public bool PreserveDeviceSettings { get; init; } = true;

    public bool PreserveNetworkConfiguration { get; init; }

    public bool PreserveSubscriptions { get; init; }

    public bool PreserveMulticastFlows { get; init; }

    public bool PreservePreferredMaster { get; init; }
}

public sealed class MachineInstanceOptions
{
    public required string NewName { get; init; }

    public bool UseTemplateTxLabels { get; init; } = true;

    public bool UseTemplateRxLabels { get; init; } = true;

    public string? TxLabelPrefix { get; init; }

    public string? RxLabelPrefix { get; init; }
}

public sealed class MachineInstanceBatchRequest
{
    public const int MaximumQuantity = 100;

    public required MachineInstanceOptions Options { get; init; }

    public int Quantity { get; init; } = 1;
}

public sealed record MachineCloneResult(
    string SourceName,
    string NewName,
    int TxCount,
    int RxCount,
    int CopiedSubscriptionCount,
    bool IsGenericRole);
