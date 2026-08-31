using AgentCore.Models;

namespace AgentCore.Tools;

public interface ITool
{
    string Name { get; }

    Task<ToolResult> ExecuteAsync(
        ToolRequest request,
        CancellationToken cancellationToken = default);
}