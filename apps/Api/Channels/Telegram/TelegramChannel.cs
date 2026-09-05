using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace Api.Channels.Telegram;

public sealed class TelegramChannel
{
    private readonly HttpClient _httpClient;
    private readonly TelegramOptions _options;

    public TelegramChannel(
        HttpClient httpClient,
        IOptions<TelegramOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task SendMessageAsync(
        long chatId,
        string message,
        CancellationToken cancellationToken = default)
    {
        var url =
            $"bot{_options.BotToken}/sendMessage";

        var payload = new
        {
            chat_id = chatId,
            text = message
        };

        using var response = await _httpClient.PostAsJsonAsync(
            url,
            payload,
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}