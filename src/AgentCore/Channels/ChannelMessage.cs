namespace AgentCore.Channels;

public sealed class ChannelMessage
{
    public required string Channel { get; init; }
    
    public required string ConversationId { get; init; }
    
    public string? CustomerId { get; init; }
    
    public required string Message { get; init; }
}