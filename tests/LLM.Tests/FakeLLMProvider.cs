using AgentCore.LLM;

namespace AgentRuntime.Tests;

public sealed class FakeLLMProvider : ILLMProvider
{
    public LLMRequest? LastRequest { get; private set; }

    public Task<LLMResponse> GenerateAsync(
        LLMRequest request,
        CancellationToken cancellationToken = default)
    {
        LastRequest = request;

        return Task.FromResult(
            new LLMResponse
            {
                Content = """
                {
                  "intent": "general",
                  "confidence": 0.99,
                  "summary": "Customer greeted the support agent.",
                  "action": null,
                  "missing_information": [],
                  "knowledge_references": []
                }
                """
            });
    }
}