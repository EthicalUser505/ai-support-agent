using AgentCore.LLM;

namespace AgentRuntime.Tests;

public sealed class FakeLLMProvider : ILLMProvider
{
    private readonly string _response;

    public LLMRequest? LastRequest { get; private set; }

    public FakeLLMProvider(
        string response = """
        {
          "intent": "general",
          "confidence": 0.99,
          "summary": "Customer greeted the support agent.",
          "action": null,
          "missing_information": [],
          "knowledge_references": []
        }
        """)
    {
        _response = response;
    }

    public Task<LLMResponse> GenerateAsync(
        LLMRequest request,
        CancellationToken cancellationToken = default)
    {
        LastRequest = request;

        return Task.FromResult(
            new LLMResponse
            {
                Content = _response
            });
    }
}