namespace AgentCore.Models;

public enum AgentRunStatus
{
    Completed,
    ActionDenied,
    AwaitingHumanApproval,
    ActionExecuted,
    Failed
}