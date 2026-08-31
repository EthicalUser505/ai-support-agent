using AgentCore.LLM;
using AgentRuntime.Decisions;
using AgentRuntime.Models;

namespace AgentRuntime;

public sealed class AgentOrchestrator : IAgentOrchestrator
{
    private readonly ILLMProvider _llm;
    private readonly AgentDecisionParser _parser;
    private readonly AgentDecisionValidator _validator;

    public AgentOrchestrator(
        ILLMProvider llm,
        AgentDecisionParser parser,
        AgentDecisionValidator validator)
    {
        _llm = llm;
        _parser = parser;
        _validator = validator;
    }

    public async Task<AgentRunResult> RunAsync(
    AgentRequest request,
    CancellationToken cancellationToken = default)
    {
        var llmRequest = new LLMRequest
        {
            SystemPrompt =
                """
                You are a customer support AI agent.

                Answer the customer's message clearly and helpfully.

                Do not invent business information.
                If you do not have enough information, say so.

                Return only valid JSON matching the agent decision schema.
                """,

            UserMessage = request.Message,
            ExpectJson = true
        };

        var llmResponse = await _llm.GenerateAsync(
            llmRequest,
            cancellationToken);

        var decision = _parser.Parse(
            llmResponse.Content);

        _validator.Validate(decision);

        return new AgentRunResult
        {
            Response = llmResponse.Content,
            Decision = decision,
            LLMResponse = llmResponse
        };
    }
}