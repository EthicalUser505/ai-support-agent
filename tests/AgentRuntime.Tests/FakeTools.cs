using AgentCore.Models;
using AgentCore.Tools;

namespace AgentRuntime.Tests;

public sealed class FakeTool : ITool
{
    public string Name => "fake_tool";

    public bool WasExecuted { get; private set; }

    public ToolRequest? LastRequest { get; private set; }

    public Task<ToolResult> ExecuteAsync(
        ToolRequest request,
        CancellationToken cancellationToken = default)
    {
        WasExecuted = true;
        LastRequest = request;

        return Task.FromResult(
            new ToolResult
            {
                Success = true,
                Data = new
                {
                    message = "Tool executed successfully."
                }
            });
    }
}