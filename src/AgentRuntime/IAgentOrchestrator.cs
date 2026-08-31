using AgentRuntime.Models;

namespace AgentRuntime;

public interface IAgentOrchestrator
{
    Task<AgentRunResult> RunAsync(
        AgentRequest request,
        CancellationToken cancellationToken = default);
}