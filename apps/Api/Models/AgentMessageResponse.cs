using AgentCore.Models;

namespace Api.Models;

public sealed class AgentMessageResponse
{
    public required AgentRunStatus Status { get; init; }
    
    public required string Response { get; init; }
    
    public required AgentDecision Decision { get; init; }
    
    public PolicyDecision? PolicyDecision { get; init; }
    
    public ToolResult? ToolResult { get; init; }
    
    public ApprovalRequest? ApprovalRequest { get; init; }
}