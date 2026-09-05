using AgentCore.Channels;
using Api.Channels;
using Api.Channels.Telegram;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/webhooks/telegram")]
public sealed class TelegramWebhookController : ControllerBase
{
    private readonly ChannelMessageProcessor _processor;
    private readonly TelegramChannel _telegram;

    public TelegramWebhookController(
        ChannelMessageProcessor processor,
        TelegramChannel telegram)
    {
        _processor = processor;
        _telegram = telegram;
    }

    [HttpPost]
    public async Task<IActionResult> Receive(
        [FromBody] TelegramUpdate update,
        CancellationToken cancellationToken)
    {
        if (update.Message?.Chat is null)
            return Ok();

        if (string.IsNullOrWhiteSpace(update.Message.Text))
            return Ok();

        var chatId = update.Message.Chat.Id;

        var channelMessage = new ChannelMessage
        {
            Channel = "telegram",
            ConversationId = $"telegram:{chatId}",
            CustomerId = update.Message.From?.Id.ToString(),
            Message = update.Message.Text
        };

        var result = await _processor.ProcessAsync(
            channelMessage,
            cancellationToken);

        await HandleResultAsync(
            chatId,
            result,
            cancellationToken);

        return Ok();
    }

    private async Task HandleResultAsync(
        long chatId,
        AgentRuntime.Models.AgentRunResult result,
        CancellationToken cancellationToken)
    {
        await _telegram.SendMessageAsync(
            chatId,
            result.Response,
            cancellationToken);
    }
}