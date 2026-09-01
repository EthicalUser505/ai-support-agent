using AgentCore.Context;
using AgentCore.Models;
using AgentCore.Policy;

namespace AgentRuntime.Tests;

public sealed class FakePolicyEngine : IPolicyEngine
{
    private readonly bool _allowed;
    private readonly bool _requiresHumanApproval;

    public ActionProposal? LastProposal { get; private set; }

    public AgentContext? LastContext { get; private set; }

    public FakePolicyEngine(
        bool allowed = true,
        bool requiresHumanApproval = false)
    {
        _allowed = allowed;
        _requiresHumanApproval = requiresHumanApproval;
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
                RequiresHumanApproval = _requiresHumanApproval,
                ValidationErrors = _allowed
                    ? []
                    : ["Action denied by test policy."]
            });
    }
}