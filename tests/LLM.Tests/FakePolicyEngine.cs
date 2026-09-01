using AgentCore.Context;
using AgentCore.Models;
using AgentCore.Policy;

namespace LLM.Tests;

public sealed class FakePolicyEngine : IPolicyEngine
{
    public Task<PolicyDecision> EvaluateAsync(
        ActionProposal proposal,
        AgentContext context,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(
            new PolicyDecision
            {
                Allowed = true,
                RequiresHumanApproval = false
            });
    }
}