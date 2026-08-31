namespace LLM.Providers.OpenAICompatible;

public sealed class OpenAICompatibleOptions
{
    public const string SectionName = "LLM";

    public string Provider { get; set; } = "openai-compatible";

    public string ApiKey { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;
}