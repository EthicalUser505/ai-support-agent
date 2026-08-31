namespace AgentCore.Models;

public sealed class PolicyDecision
{
    public bool Allowed { get; init; }

    public bool RequiresHumanApproval { get; init; }

    public IReadOnlyList<string> ValidationErrors { get; init; }
        = [];

    public IReadOnlyList<string> PolicyReferences { get; init; }
        = [];
}