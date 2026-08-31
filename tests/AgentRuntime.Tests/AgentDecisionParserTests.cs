using AgentRuntime.Decisions;

namespace AgentRuntime.Tests;

public class AgentDecisionParserTests
{
    [Fact]
    public void Parse_ValidJson_ReturnsDecision()
    {
        var parser = new AgentDecisionParser();

        var json = """
        {
          "intent": "booking",
          "confidence": 0.94,
          "summary": "Customer wants a haircut.",
          "action": {
            "name": "create_booking",
            "parameters": {
              "service": "haircut"
            },
            "confidence": 0.94
          },
          "missing_information": [],
          "knowledge_references": []
        }
        """;

        var decision = parser.Parse(json);

        Assert.Equal("booking", decision.Intent);
        Assert.Equal(0.94, decision.Confidence);
        Assert.NotNull(decision.Action);
        Assert.Equal(
            "create_booking",
            decision.Action!.Name);
    }

    [Fact]
    public void Parse_InvalidJson_Throws()
    {
        var parser = new AgentDecisionParser();

        var invalidJson = """
        {
          "intent": "booking",
        """;

        Assert.Throws<InvalidOperationException>(
            () => parser.Parse(invalidJson));
    }
}