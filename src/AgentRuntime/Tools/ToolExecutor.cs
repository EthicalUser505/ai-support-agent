using AgentCore.Models;
using AgentCore.Tools;

namespace AgentRuntime.Tools;

public sealed class ToolExecutor
{
    private readonly IToolRegistry _registry;

    public ToolExecutor(IToolRegistry registry)
    {
        _registry = registry;
    }

    public async Task<ToolResult> ExecuteAsync(
        ActionProposal proposal,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(proposal);

        var tool = _registry.GetRequired(proposal.Name);

        var request = new ToolRequest
        {
            ToolName = proposal.Name,
            Parameters = proposal.Parameters
        };

        return await tool.ExecuteAsync(
            request,
            cancellationToken);
    }
}