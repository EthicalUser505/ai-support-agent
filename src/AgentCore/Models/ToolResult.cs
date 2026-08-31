namespace AgentCore.Models;

public sealed class ToolResult
{
    public bool Success { get; init; }

    public object? Data { get; init; }

    public string? Error { get; init; }
}