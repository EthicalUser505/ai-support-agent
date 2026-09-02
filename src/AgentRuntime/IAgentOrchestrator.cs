using AgentCore.Context;
using AgentCore.Models;
using AgentRuntime.Models;

namespace AgentRuntime;

public interface IAgentOrchestrator
{
    Task<AgentRunResult> RunAsync(
        AgentRequest request,
        CancellationToken cancellationToken = default);

    Task<AgentRunResult> ResumeAsync(
        ActionResumeRequest request,
        AgentContext context,
        CancellationToken cancellationToken = default);
}