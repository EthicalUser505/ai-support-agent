using AgentCore.Context;
using AgentCore.Models;
using AgentRuntime.Decisions;
using AgentRuntime.Models;


namespace AgentRuntime.Tests;

public class AgentOrchestratorTests
{
    [Fact]
    public async Task RunAsync_UsesLLMProvider()
    {
        var llm = new FakeLLMProvider();
        var parser = new AgentDecisionParser();
        var validator = new AgentDecisionValidator();
        var policy = new FakePolicyEngine();

        var orchestrator = new AgentOrchestrator(
            llm,
            parser,
            validator,
            policy);

        Assert.Null(policy.LastProposal);
Assert.Null(policy.LastContext);

        var request = new AgentRequest
        {
            Context = new AgentContext
            {
                ConversationId = "test-conversation"
            },

            Message = "Hello"
        };

        var result = await orchestrator.RunAsync(request);

        Assert.NotNull(llm.LastRequest);
        Assert.Equal("Hello", llm.LastRequest!.UserMessage);

        Assert.NotNull(result.Decision);
        Assert.Equal("general", result.Decision!.Intent);
        Assert.Equal(0.99, result.Decision.Confidence);
    }
    [Fact]
    public async Task RunAsync_DeniedAction_ReturnsActionDenied()
    {
        const string response = """
        {
        "intent": "refund_request",
        "confidence": 0.99,
        "summary": "Customer is requesting a refund.",
        "action": {
            "name": "refund_order",
            "parameters": {
            "order_id": "ORD-123"
            },
            "confidence": 0.99
        },
        "missing_information": [],
        "knowledge_references": []
        }
        """;

        var llm = new FakeLLMProvider(response);
        var parser = new AgentDecisionParser();
        var validator = new AgentDecisionValidator();
        var policy = new FakePolicyEngine(allowed: false);

        var orchestrator = new AgentOrchestrator(
            llm,
            parser,
            validator,
            policy);

        var request = new AgentRequest
        {
            Context = new AgentCore.Context.AgentContext
            {
                ConversationId = "test-conversation"
            },
            Message = "Refund my order."
        };

        var result = await orchestrator.RunAsync(request);

        Assert.Equal(
            AgentRunStatus.ActionDenied,
            result.Status);

        Assert.NotNull(result.PolicyDecision);
        Assert.False(result.PolicyDecision!.Allowed);

        Assert.NotNull(policy.LastProposal);
        Assert.Equal(
            "refund_order",
            policy.LastProposal!.Name);
    }
}