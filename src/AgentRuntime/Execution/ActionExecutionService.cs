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

        // Policy denial is a hard stop.
        if (!policyDecision.Allowed)
        {
            return new ActionExecutionResult
            {
                PolicyDecision = policyDecision
            };
        }

        // Human approval is required before tool execution.
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

        // Only fully authorized actions reach the tool layer.
        var toolResult = await _toolExecutor.ExecuteAsync(
            proposal,
            cancellationToken);

        return new ActionExecutionResult
        {
            PolicyDecision = policyDecision,
            ToolResult = toolResult
        };
    }

    public async Task<ActionExecutionResult> ResumeAsync(
        ApprovalRequest approvalRequest,
        AgentContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(approvalRequest);
        ArgumentNullException.ThrowIfNull(context);

        if (approvalRequest.Status != ApprovalStatus.Approved)
        {
            throw new InvalidOperationException(
                "Only approved requests can be resumed.");
        }

        var proposal = new ActionProposal
        {
            Name = approvalRequest.ActionName,
            Parameters = approvalRequest.Parameters
        };

        // Re-check authorization before execution.
        var policyDecision = await _policy.EvaluateAsync(
            proposal,
            context,
            cancellationToken);

        // A stale approval must never override a current denial.
        if (!policyDecision.Allowed)
        {
            return new ActionExecutionResult
            {
                PolicyDecision = policyDecision
            };
        }

        // The approval already happened, so we do not create
        // another approval request here.
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