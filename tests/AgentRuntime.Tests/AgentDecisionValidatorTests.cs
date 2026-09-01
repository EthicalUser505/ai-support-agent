using AgentCore.Models;
using AgentRuntime.Decisions;

namespace AgentRuntime.Tests;

public class AgentDecisionValidatorTests
{
    private readonly AgentDecisionValidator _validator = new();

    [Fact]
    public void Validate_ValidDecision_DoesNotThrow()
    {
        var decision = new AgentDecision
        {
            Intent = "booking",
            Confidence = 0.95,
            Summary = "Customer wants to make a booking."
        };

        _validator.Validate(decision);
    }

    [Fact]
    public void Validate_InvalidConfidence_Throws()
    {
        var decision = new AgentDecision
        {
            Intent = "booking",
            Confidence = 1.5,
            Summary = "Test summary"
        };

        Assert.Throws<InvalidOperationException>(
            () => _validator.Validate(decision));
    }

    [Fact]
    public void Validate_MissingIntent_Throws()
    {
        var decision = new AgentDecision
        {
            Intent = "",
            Confidence = 0.9,
            Summary = "Test summary"
        };

        Assert.Throws<InvalidOperationException>(
            () => _validator.Validate(decision));
    }

}