namespace AgentCore.LLM;

public sealed class LLMResponse
{
    public required string Content { get; init; }

    public int? InputTokens { get; init; }

    public int? OutputTokens { get; init; }
}