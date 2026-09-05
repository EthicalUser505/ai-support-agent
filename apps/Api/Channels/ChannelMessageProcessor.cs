using AgentCore.Channels;
using AgentCore.Context;
using AgentRuntime;
using AgentRuntime.Models;

namespace Api.Channels;

public sealed class ChannelMessageProcessor
{
    private readonly IAgentOrchestrator _orchestrator;

    public ChannelMessageProcessor(
        IAgentOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }

    public async Task<AgentRunResult> ProcessAsync(
        ChannelMessage message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var context = new AgentContext
        {
            ConversationId = message.ConversationId,
            CustomerId = message.CustomerId
        };

        return await _orchestrator.RunAsync(
            new AgentRequest
            {
                Context = context,
                Message = message.Message
            },
            cancellationToken);
    }
}