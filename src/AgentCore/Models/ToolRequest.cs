namespace AgentCore.Models;

public sealed class ToolRequest
{
    public required string ToolName { get; init; }

    public IReadOnlyDictionary<string, object?> Parameters { get; init; }
        = new Dictionary<string, object?>();
}