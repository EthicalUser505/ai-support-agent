using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AgentCore.LLM;
using Microsoft.Extensions.Options;
using System.Text.Json.Serialization;

namespace LLM.Ilmu;

public sealed class IlmuProvider : ILLMProvider
{
    private readonly HttpClient _httpClient;
    private readonly IlmuOptions _options;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };


    public IlmuProvider(
        HttpClient httpClient,
        IOptions<IlmuOptions> options)
{
    _httpClient = httpClient;
    _options = options.Value;

    _httpClient.BaseAddress = new Uri(
        _options.BaseUrl.TrimEnd('/') + "/");

    _httpClient.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue(
            "Bearer",
            _options.ApiKey);
}

    public async Task<LLMResponse> GenerateAsync(
        LLMRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var messages = new List<IlmuMessage>
        {
            new()
            {
                Role = "system",
                Content = request.SystemPrompt
            }
        };

        if (!string.IsNullOrWhiteSpace(request.Context))
        {
            messages.Add(new IlmuMessage
            {
                Role = "system",
                Content = $"Additional context:\n{request.Context}"
            });
        }

        messages.Add(new IlmuMessage
        {
            Role = "user",
            Content = request.UserMessage
        });

        var payload = new IlmuChatCompletionRequest
        {
            Model = _options.Model,
            Messages = messages,
            Temperature = 0
        };

        using var response = await _httpClient.PostAsJsonAsync(
            "chat/completions",
            payload,
            JsonOptions,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(
                cancellationToken);

            throw new HttpRequestException(
                $"ILMU API request failed with status " +
                $"{(int)response.StatusCode} " +
                $"{response.ReasonPhrase}: {errorBody}");
        }

        var result =
            await response.Content.ReadFromJsonAsync<IlmuChatCompletionResponse>(
                JsonOptions,
                cancellationToken);

        if (result is null)
        {
            throw new InvalidOperationException(
                "ILMU returned an empty response.");
        }

        var content = result.Choices?
            .FirstOrDefault()?
            .Message?
            .Content;

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException(
                "ILMU returned no assistant content.");
        }

        return new LLMResponse
        {
            Content = content,
            InputTokens = result.Usage?.PromptTokens,
            OutputTokens = result.Usage?.CompletionTokens
        };
    }

    private sealed class IlmuChatCompletionRequest
    {
        [JsonPropertyName("model")]
        public required string Model { get; init; }

        [JsonPropertyName("messages")]
        public required IReadOnlyList<IlmuMessage> Messages { get; init; }

        [JsonPropertyName("temperature")]
        public double Temperature { get; init; }
    }

    private sealed class IlmuMessage
    {
        [JsonPropertyName("role")]
        public required string Role { get; init; }

        [JsonPropertyName("content")]
        public required string Content { get; init; }
    }

    private sealed class IlmuChatCompletionResponse
    {
        [JsonPropertyName("choices")]
        public List<IlmuChoice>? Choices { get; init; }

        [JsonPropertyName("usage")]
        public IlmuUsage? Usage { get; init; }
    }

    private sealed class IlmuChoice
    {
        public IlmuMessage? Message { get; init; }
    }

    private sealed class IlmuUsage
    {
        public int PromptTokens { get; init; }

        public int CompletionTokens { get; init; }

        public int TotalTokens { get; init; }
    }
}