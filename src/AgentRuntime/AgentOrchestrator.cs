using AgentCore.LLM;
using AgentCore.Models;
using AgentRuntime.Decisions;
using AgentRuntime.Execution;
using AgentRuntime.Models;
using AgentRuntime.Prompts;

namespace AgentRuntime;

public sealed class AgentOrchestrator : IAgentOrchestrator
{
    private readonly ILLMProvider _llm;
    private readonly AgentDecisionParser _parser;
    private readonly AgentDecisionValidator _validator;
    private readonly ActionExecutionService _actionExecution;

    public AgentOrchestrator(
        ILLMProvider llm,
        AgentDecisionParser parser,
        AgentDecisionValidator validator,
        ActionExecutionService actionExecution)
    {
        _llm = llm;
        _parser = parser;
        _validator = validator;
        _actionExecution = actionExecution;
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
            LLMResponse = llmResponse
        };
    }
}