using AgentCore.Models;

namespace AgentRuntime.Decisions;

public sealed class AgentDecisionValidator
{
    public void Validate(AgentDecision decision)
    {
        if (string.IsNullOrWhiteSpace(decision.Intent))
        {
            throw new InvalidOperationException(
                "Agent decision must contain an intent.");
        }

        if (decision.Confidence < 0 ||
            decision.Confidence > 1)
        {
            throw new InvalidOperationException(
                "Agent confidence must be between 0 and 1.");
        }

        if (decision.Action is not null)
        {
            if (string.IsNullOrWhiteSpace(
                decision.Action.Name))
            {
                throw new InvalidOperationException(
                    "Proposed action must contain a name.");
            }

            if (decision.Action.Confidence < 0 ||
                decision.Action.Confidence > 1)
            {
                throw new InvalidOperationException(
                    "Action confidence must be between 0 and 1.");
            }
        }
    }
}