using AgentCore.LLM;
using AgentCore.Models;
using AgentCore.Policy;
using AgentRuntime.Decisions;
using AgentRuntime.Models;
using AgentRuntime.Prompts;

namespace AgentRuntime;

public sealed class AgentOrchestrator : IAgentOrchestrator
{
    private readonly ILLMProvider _llm;
    private readonly AgentDecisionParser _parser;
    private readonly AgentDecisionValidator _validator;
    private readonly IPolicyEngine _policy;

    public AgentOrchestrator(
        ILLMProvider llm,
        AgentDecisionParser parser,
        AgentDecisionValidator validator,
        IPolicyEngine policy)
    {
        _llm = llm;
        _parser = parser;
        _validator = validator;
        _policy = policy;
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

        var llmResponse = await _llm.GenerateAsync(
            llmRequest,
            cancellationToken);

        var decision = _parser.Parse(
            llmResponse.Content);

        _validator.Validate(decision);

        PolicyDecision? policyDecision = null;

        if (decision.Action is not null)
        {
            policyDecision = await _policy.EvaluateAsync(
                decision.Action,
                request.Context,
                cancellationToken);

            if (!policyDecision.Allowed)
            {
                return new AgentRunResult
                {
                    Status = AgentRunStatus.ActionDenied,
                    Response = "I'm unable to carry out that request.",
                    Decision = decision,
                    PolicyDecision = policyDecision,
                    LLMResponse = llmResponse
                };
            }
        }

        return new AgentRunResult
        {
            Status = AgentRunStatus.Completed,
            Response = decision.Summary
                ?? "I understand your request.",
            Decision = decision,
            PolicyDecision = policyDecision,
            LLMResponse = llmResponse
        };
    }
}