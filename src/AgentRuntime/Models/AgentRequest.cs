using AgentCore.Context;

namespace AgentRuntime.Models;

public sealed class AgentRequest
{
    public required AgentContext Context { get; init; }

    public required string Message { get; init; }
}