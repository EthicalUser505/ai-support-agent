namespace AgentCore.Models;

public sealed class ActionProposal
{
    public required string Name { get; init; }

    public IReadOnlyDictionary<string, object?> Parameters { get; init; }
        = new Dictionary<string, object?>();

    public double Confidence { get; init; }
}