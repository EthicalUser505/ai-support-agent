using AgentCore.LLM;
using AgentCore.Models;

namespace AgentRuntime.Models;

public sealed class AgentRunResult
{
    public required string Response { get; init; }

    public required AgentDecision Decision { get; init; }

    public LLMResponse? LLMResponse { get; init; }
}