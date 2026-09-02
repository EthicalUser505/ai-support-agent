using AgentCore.Models;
using AgentCore.Tools;

namespace AgentRuntime.Tests;

public sealed class NamedFakeTool : ITool
{
    private readonly FakeTool _inner;

    public NamedFakeTool(
        string name,
        FakeTool inner)
    {
        Name = name;
        _inner = inner;
    }

    public string Name { get; }

    public bool WasExecuted =>
        _inner.WasExecuted;

    public ToolRequest? LastRequest =>
        _inner.LastRequest;

    public Task<ToolResult> ExecuteAsync(
        ToolRequest request,
        CancellationToken cancellationToken = default)
    {
        return _inner.ExecuteAsync(
            request,
            cancellationToken);
    }
}