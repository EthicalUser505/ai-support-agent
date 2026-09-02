using AgentCore.LLM;
using AgentCore.Models;
using AgentRuntime.Decisions;
using AgentRuntime.Execution;
using AgentRuntime.Models;
using AgentRuntime.Prompts;
using AgentCore.Approval;

namespace AgentRuntime;

public sealed class AgentOrchestrator : IAgentOrchestrator
{
    private readonly ILLMProvider _llm;
    private readonly AgentDecisionParser _parser;
    private readonly AgentDecisionValidator _validator;
    private readonly ActionExecutionService _actionExecution;
    private readonly IApprovalService _approval;

    public AgentOrchestrator(
    ILLMProvider llm,
    AgentDecisionParser parser,
    AgentDecisionValidator validator,
    ActionExecutionService actionExecution,
    IApprovalService approval)
    {
            _llm = llm;
            _parser = parser;
            _validator = validator;
            _actionExecution = actionExecution;
            _approval = approval;
        }

    public async Task<AgentRunResult> RunAsync(
        AgentRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var llmRequest = new LLMRequest
        {
            SystemPrompt = AgentDecisionPrompt.SystemPrompt,
            UserMessage = request.Message,
            ExpectJson = true
        };

        // Ask the LLM to interpret the request.
        var llmResponse = await _llm.GenerateAsync(
            llmRequest,
            cancellationToken);

        // Parse the structured decision.
        var decision = _parser.Parse(
            llmResponse.Content);

        // Validate the decision before any action can occur.
        _validator.Validate(decision);

        PolicyDecision? policyDecision = null;
        ToolResult? toolResult = null;

        // Only proposed actions enter the action-execution pipeline.
        if (decision.Action is not null)
        {
            var executionResult =
                await _actionExecution.ExecuteAsync(
                    request.Context.ConversationId,
                    decision.Action,
                    request.Context,
                    cancellationToken);

            policyDecision =
                executionResult.PolicyDecision;

            toolResult =
                executionResult.ToolResult;

            // Policy denial is a hard stop.
             if (!policyDecision.Allowed)
            {
                return new AgentRunResult
                {
                    Status = AgentRunStatus.ActionDenied,
                    Response = "I'm unable to carry out that request.",
                    Decision = decision,
                    PolicyDecision = policyDecision,
                    ToolResult = null,
                    ApprovalRequest = null,
                    LLMResponse = llmResponse
                };
            }

            // Human in the loop (HITL)
            if (executionResult.ApprovalRequest is not null)
            {
                return new AgentRunResult
                {
                    Status = AgentRunStatus.AwaitingHumanApproval,
                    Response = "This request requires approval from a member of the business team.",
                    Decision = decision,
                    PolicyDecision = policyDecision,
                    ToolResult = null,
                    ApprovalRequest = executionResult.ApprovalRequest,
                    LLMResponse = llmResponse
                };
            }
        }

        // Build the final runtime result.
        var status = decision.Action is null
            ? AgentRunStatus.Completed
            : toolResult?.Success == true
                ? AgentRunStatus.ActionExecuted
                : AgentRunStatus.Failed;

        var response = decision.Action is null
            ? decision.Summary ?? "I understand your request."
            : toolResult?.Success == true
                ? "The requested action was completed."
                : "I couldn't complete the requested action.";

        return new AgentRunResult
        {
            Status = status,
            Response = response,
            Decision = decision,
            PolicyDecision = policyDecision,
            ToolResult = toolResult,
            ApprovalRequest = null,
            LLMResponse = llmResponse
        };
    }
    public async Task<AgentRunResult> ResumeAsync(
    ActionResumeRequest request,
    AgentCore.Context.AgentContext context,
    CancellationToken cancellationToken = default)
{
    ArgumentNullException.ThrowIfNull(request);
    ArgumentNullException.ThrowIfNull(context);

    var approval = await _approval.GetAsync(
        request.ApprovalId,
        cancellationToken);

    if (approval is null)
    {
        throw new KeyNotFoundException(
            $"Approval '{request.ApprovalId}' was not found.");
    }

    if (approval.Status == ApprovalStatus.Rejected)
    {
        return new AgentRunResult
        {
            Status = AgentRunStatus.ActionDenied,
            Response = "The approved action was rejected.",
            Decision = new AgentCore.Models.AgentDecision
            {
                Intent = "approved_action",
                Confidence = 1.0,
                Summary = "The approval request was rejected.",
                Action = null
            },
            PolicyDecision = null,
            ToolResult = null,
            ApprovalRequest = approval,
            LLMResponse = null
        };
    }

    if (approval.Status != ApprovalStatus.Approved)
    {
        return new AgentRunResult
        {
            Status = AgentRunStatus.AwaitingHumanApproval,
            Response = "This action is still awaiting human approval.",
            Decision = new AgentCore.Models.AgentDecision
            {
                Intent = "approval_required",
                Confidence = 1.0,
                Summary = "The action is awaiting approval.",
                Action = null
            },
            PolicyDecision = null,
            ToolResult = null,
            ApprovalRequest = approval,
            LLMResponse = null
        };
    }

    var executionResult = await _actionExecution.ResumeAsync(
        approval,
        context,
        cancellationToken);

    if (!executionResult.PolicyDecision.Allowed)
    {
        return new AgentRunResult
        {
            Status = AgentRunStatus.ActionDenied,
            Response = "The action is no longer permitted.",
            Decision = new AgentCore.Models.AgentDecision
            {
                Intent = "approved_action",
                Confidence = 1.0,
                Summary = "The previously approved action is no longer permitted.",
                Action = null
            },
            PolicyDecision = executionResult.PolicyDecision,
            ToolResult = null,
            ApprovalRequest = approval,
            LLMResponse = null
        };
    }

        if (executionResult.ToolResult is null)
        {
            return new AgentRunResult
            {
                Status = AgentRunStatus.Failed,
                Response = "The action could not be completed.",
                Decision = new AgentCore.Models.AgentDecision
                {
                    Intent = "approved_action",
                    Confidence = 1.0,
                    Summary = "The approved action did not produce a tool result.",
                    Action = null
                },
                PolicyDecision = executionResult.PolicyDecision,
                ToolResult = null,
                ApprovalRequest = approval,
                LLMResponse = null
            };
        }

        return new AgentRunResult
        {
            Status = executionResult.ToolResult.Success
                ? AgentRunStatus.ActionExecuted
                : AgentRunStatus.Failed,

            Response = executionResult.ToolResult.Success
                ? "The approved action was completed."
                : "The approved action could not be completed.",

            Decision = new AgentCore.Models.AgentDecision
            {
                Intent = "approved_action",
                Confidence = 1.0,
                Summary = "An approved action was resumed.",
                Action = null
            },

            PolicyDecision = executionResult.PolicyDecision,
            ToolResult = executionResult.ToolResult,
            ApprovalRequest = approval,
            LLMResponse = null
        };
    }
}