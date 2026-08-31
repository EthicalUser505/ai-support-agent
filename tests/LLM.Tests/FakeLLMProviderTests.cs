using AgentCore.Context;
using AgentRuntime;
using AgentRuntime.Decisions;
using AgentRuntime.Models;

namespace AgentRuntime.Tests;

public class AgentOrchestratorTests
{
    [Fact]
    public async Task RunAsync_RequestsStructuredOutput()
    {
        var llm = new FakeLLMProvider();
        var parser = new AgentDecisionParser();
        var validator = new AgentDecisionValidator();

        var orchestrator =
            new AgentOrchestrator(
                llm,
                parser,
                validator);

        var request = new AgentRequest
        {
            Context = new AgentContext
            {
                ConversationId = "test-conversation"
            },

            Message = "Hello"
        };

        await orchestrator.RunAsync(request);

        Assert.NotNull(llm.LastRequest);
        Assert.True(llm.LastRequest!.ExpectJson);
    }
}