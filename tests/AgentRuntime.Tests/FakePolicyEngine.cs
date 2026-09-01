using AgentCore.Context;
using AgentCore.Models;
using AgentCore.Policy;

namespace AgentRuntime.Tests;

public sealed class FakePolicyEngine : IPolicyEngine
{
    public ActionProposal? LastProposal { get; private set; }

    public AgentContext? LastContext { get; private set; }

    public Task<PolicyDecision> EvaluateAsync(
        ActionProposal proposal,
        AgentContext context,
        CancellationToken cancellationToken = default)
    {
        LastProposal = proposal;
        LastContext = context;

        return Task.FromResult(
            new PolicyDecision
            {
                Allowed = true,
                RequiresHumanApproval = false
            });
    }
}