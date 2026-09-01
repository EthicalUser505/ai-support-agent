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
                RequiresHumanApproval = false,
                PolicyReferences =
                [
                    "POL-ORDER-LOOKUP-001"
                ]
            },

            "refund_order" => new PolicyDecision
            {
                Allowed = true,
                RequiresHumanApproval = true,
                PolicyReferences =
                [
                    "POL-REFUND-APPROVAL-001"
                ]
            },

            _ => new PolicyDecision
            {
                Allowed = false,
                RequiresHumanApproval = false,
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