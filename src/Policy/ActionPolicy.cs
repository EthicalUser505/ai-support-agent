using AgentCore.Context;
using AgentCore.Models;
using AgentCore.Policy;

namespace Policy;

public sealed class ActionPolicy : IPolicyEngine
{
    public Task<PolicyDecision> EvaluateAsync(
        ActionProposal proposal,
        AgentContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(proposal);
        ArgumentNullException.ThrowIfNull(context);

        var result = proposal.Name switch
        {
            "lookup_order" => new PolicyDecision
            {
                Allowed = true,
                PolicyReferences =
                [
                    "POL-ORDER-LOOKUP-001"
                ]
            },

            _ => new PolicyDecision
            {
                Allowed = false,
                ValidationErrors =
                [
                    $"Action '{proposal.Name}' is not permitted."
                ],
                PolicyReferences =
                [
                    "POL-ACTION-DENY-UNKNOWN"
                ]
            }
        };

        return Task.FromResult(result);
    }
}