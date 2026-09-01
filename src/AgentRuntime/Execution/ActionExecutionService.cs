using AgentCore.Approval;
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
    private readonly IApprovalService _approval;

    public ActionExecutionService(
        IPolicyEngine policy,
        ToolExecutor toolExecutor,
        IApprovalService approval)
    {
        _policy = policy;
        _toolExecutor = toolExecutor;
        _approval = approval;
    }

    public async Task<ActionExecutionResult> ExecuteAsync(
        string conversationId,
        ActionProposal proposal,
        AgentContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            conversationId);

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

        if (policyDecision.RequiresHumanApproval)
        {
            var approval = await _approval.CreateAsync(
                conversationId,
                proposal,
                "This action requires human approval.",
                cancellationToken);

            return new ActionExecutionResult
            {
                PolicyDecision = policyDecision,
                ApprovalRequest = approval
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