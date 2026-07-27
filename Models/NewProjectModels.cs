namespace DanteConfigEditor.Models;

public sealed class NewProjectOptions
{
    public required string ProjectName { get; init; }

    public string? Description { get; init; }

    public IReadOnlyList<NewCustomMachineDefinition> Machines { get; init; } = [];
}

public sealed class NewCustomMachineDefinition
{
    public required string Name { get; init; }

    public int TxCount { get; init; }

    public int RxCount { get; init; }

    public int SampleRate { get; init; } = 48000;

    public int Encoding { get; init; } = 24;

    public int UnicastLatency { get; init; } = 1000;
}
