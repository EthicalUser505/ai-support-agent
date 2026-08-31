namespace AgentCore.LLM;

public sealed class LLMRequest
{
    public required string SystemPrompt { get; init; }

    public required string UserMessage { get; init; }

    public string? Context { get; init; }

    public bool ExpectJson { get; init; }
}