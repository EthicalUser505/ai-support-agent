using AgentCore.LLM;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace LLM.Providers.OpenAICompatible;

public sealed class OpenAICompatibleProvider : ILLMProvider
{
    private readonly OpenAICompatibleOptions _options;
    private readonly ChatClient _client;

    public OpenAICompatibleProvider(
        IOptions<OpenAICompatibleOptions> options)
    {
        _options = options.Value;

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException(
                "LLM API key has not been configured.");
        }

        if (string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            throw new InvalidOperationException(
                "LLM base URL has not been configured.");
        }

        if (string.IsNullOrWhiteSpace(_options.Model))
        {
            throw new InvalidOperationException(
                "LLM model has not been configured.");
        }

        var clientOptions = new OpenAIClientOptions
        {
            Endpoint = new Uri(_options.BaseUrl)
        };

        var client = new OpenAIClient(
            new System.ClientModel.ApiKeyCredential(_options.ApiKey),
            clientOptions);

        _client = client.GetChatClient(_options.Model);
    }

    public async Task<LLMResponse> GenerateAsync(
        LLMRequest request,
        CancellationToken cancellationToken = default)
    {
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(request.SystemPrompt)
        };

        if (!string.IsNullOrWhiteSpace(request.Context))
        {
            messages.Add(
                new UserChatMessage(
                    $"Context:\n{request.Context}"));
        }

        messages.Add(
            new UserChatMessage(request.UserMessage));

        var completion = await _client.CompleteChatAsync(
            messages,
            cancellationToken: cancellationToken);

        var content = completion.Value.Content.Count > 0
            ? completion.Value.Content[0].Text
            : string.Empty;

        return new LLMResponse
        {
            Content = content
        };
    }
}