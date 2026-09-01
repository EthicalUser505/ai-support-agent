using AgentCore.Models;

namespace AgentRuntime.Models;

public sealed class ActionExecutionResult
{
    public required PolicyDecision PolicyDecision { get; init; }

    public ToolResult? ToolResult { get; init; }
}