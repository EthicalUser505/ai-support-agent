namespace AgentCore.Models;

public sealed class AgentDecision
{
    public required string Intent { get; init; }

    public double Confidence { get; init; }

    public string? Summary { get; init; }

    public ActionProposal? Action { get; init; }

    public IReadOnlyList<string> MissingInformation { get; init; }
        = [];

    public IReadOnlyList<string> KnowledgeReferences { get; init; }
        = [];
}