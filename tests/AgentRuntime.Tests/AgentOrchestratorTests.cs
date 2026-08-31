using AgentCore.Context;
using AgentRuntime.Decisions;
using AgentRuntime.Models;

namespace AgentRuntime.Tests;

public class AgentOrchestratorTests
{
    [Fact]
    public async Task RunAsync_UsesLLMProvider()
    {
        var llm = new FakeLLMProvider();

        var orchestrator = new AgentOrchestrator(
            llm,
            new AgentDecisionParser(),
            new AgentDecisionValidator());

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
}