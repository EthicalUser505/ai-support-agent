using AgentCore.Context;
using AgentCore.Models;
using AgentCore.Policy;

namespace AgentRuntime.Tests;

public sealed class FakePolicyEngine : IPolicyEngine
{
    private readonly bool _allowed;

    public ActionProposal? LastProposal { get; private set; }

    public AgentContext? LastContext { get; private set; }

    public FakePolicyEngine(bool allowed = true)
    {
        _allowed = allowed;
    }

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
                Allowed = _allowed,
                RequiresHumanApproval = false,
                ValidationErrors = _allowed
                    ? []
                    : ["Action denied by test policy."]
            });
    }
}