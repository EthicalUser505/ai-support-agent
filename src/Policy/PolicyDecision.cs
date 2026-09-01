namespace Policy.Models;

public sealed class PolicyDecision
{
    public required bool Allowed { get; init; }

    public required string Reason { get; init; }
}
