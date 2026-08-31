namespace AgentCore.LLM;

public interface ILLMProvider
{
    Task<LLMResponse> GenerateAsync(
        LLMRequest request,
        CancellationToken cancellationToken = default);
}