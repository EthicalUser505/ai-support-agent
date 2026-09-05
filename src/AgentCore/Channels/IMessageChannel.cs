namespace AgentCore.Channels;

public interface IMessageChannel
{
    string Name { get; }

    Task SendMessageAsync(
        string conversationId,
        string message,
        CancellationToken cancellationToken = default);
}