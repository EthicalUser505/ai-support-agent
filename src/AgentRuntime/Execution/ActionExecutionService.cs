using AgentCore.Context;
using AgentCore.Models;
using AgentCore.Policy;
using AgentRuntime.Models;
using AgentRuntime.Tools;

namespace AgentRuntime.Execution;

public sealed class ActionExecutionService
{
    private readonly IPolicyEngine _policy;
    private readonly ToolExecutor _toolExecutor;

    public ActionExecutionService(
        IPolicyEngine policy,
        ToolExecutor toolExecutor)
    {
        _policy = policy;
        _toolExecutor = toolExecutor;
    }

    public async Task<ActionExecutionResult> ExecuteAsync(
        ActionProposal proposal,
        AgentContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(proposal);
        ArgumentNullException.ThrowIfNull(context);

        var policyDecision = await _policy.EvaluateAsync(
            proposal,
            context,
            cancellationToken);

        if (!policyDecision.Allowed)
        {
            return new ActionExecutionResult
            {
                PolicyDecision = policyDecision
            };
        }

        var toolResult = await _toolExecutor.ExecuteAsync(
            proposal,
            cancellationToken);

        return new ActionExecutionResult
        {
            PolicyDecision = policyDecision,
            ToolResult = toolResult
        };
    }
}