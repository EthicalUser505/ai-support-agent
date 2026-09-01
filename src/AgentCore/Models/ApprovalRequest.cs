namespace AgentCore.Models;

public sealed class ApprovalRequest
{
    public required string ApprovalId { get; init; }

    public required string ConversationId { get; init; }

    public required string ActionName { get; init; }

    public required IReadOnlyDictionary<string, object?> Parameters { get; init; }

    public required string Reason { get; init; }

    public ApprovalStatus Status { get; init; } = ApprovalStatus.Pending;
}