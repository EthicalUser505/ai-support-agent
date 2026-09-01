using AgentCore.Models;

namespace AgentRuntime.Decisions;

public sealed class AgentDecisionValidator
{
    public void Validate(AgentDecision decision)
    {
        ArgumentNullException.ThrowIfNull(decision);

        if (string.IsNullOrWhiteSpace(decision.Intent))
        {
            throw new InvalidOperationException(
                "Agent decision must contain an intent.");
        }

        if (string.IsNullOrWhiteSpace(decision.Summary))
        {
            throw new InvalidOperationException(
                "Agent decision must contain a summary.");
        }

        ValidateConfidence(
            decision.Confidence,
            "Agent confidence");

        if (decision.Action is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(decision.Action.Name))
        {
            throw new InvalidOperationException(
                "Proposed action must contain a name.");
        }

        ValidateConfidence(
            decision.Action.Confidence,
            "Action confidence");

        if (decision.Action.Parameters is null)
        {
            throw new InvalidOperationException(
                "Proposed action parameters must not be null.");
        }
    }

    private static void ValidateConfidence(
        double confidence,
        string fieldName)
    {
        if (confidence < 0 || confidence > 1)
        {
            throw new InvalidOperationException(
                $"{fieldName} must be between 0 and 1.");
        }
    }
}

