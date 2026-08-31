using System.Text.Json;
using AgentCore.Models;

namespace AgentRuntime.Decisions;

public sealed class AgentDecisionParser
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AgentDecision Parse(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException(
                "The LLM returned an empty response.");
        }

        try
        {
            var decision =
                JsonSerializer.Deserialize<AgentDecision>(
                    content,
                    JsonOptions);

            if (decision is null)
            {
                throw new InvalidOperationException(
                    "The LLM returned an invalid agent decision.");
            }

            return decision;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                "The LLM returned malformed JSON.",
                ex);
        }
    }
}