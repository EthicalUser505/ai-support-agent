using AgentCore.Context;
using AgentCore.Models;

namespace AgentCore.Policy;

public interface IPolicyEngine
{
    Task<PolicyDecision> EvaluateAsync(
        ActionProposal proposal,
        AgentContext context,
        CancellationToken cancellationToken = default);
}