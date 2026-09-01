using AgentCore.LLM;
using AgentCore.Models;

namespace AgentRuntime.Models;

public sealed class AgentRunResult
{
    public required AgentRunStatus Status { get; init; }

    public required string Response { get; init; }

    public required AgentDecision Decision { get; init; }

    public PolicyDecision? PolicyDecision { get; init; }

    public ToolResult? ToolResult { get; init; }

    public LLMResponse? LLMResponse { get; init; }

    public ApprovalRequest? ApprovalRequest { get; init; }
}