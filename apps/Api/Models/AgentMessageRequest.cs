namespace Api.Models;

public sealed class AgentMessageRequest
{
    public required string ConversationId { get; init; }

    public string? CustomerId { get; init; }

    public required string Message { get; init; }
}