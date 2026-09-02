namespace AgentCore.Channels;

public interface IMessageChannel
{
    string Name { get; }

    Task SendMessageAsync(
        ChannelMessage message,
        CancellationToken cancellationToken = default);
}