namespace AgentCore.Context;

public sealed class AgentContext
{
    public required string ConversationId { get; init; }

    public string? CustomerId { get; init; }

    public IReadOnlyDictionary<string, object?> Metadata { get; init; }
        = new Dictionary<string, object?>();
}