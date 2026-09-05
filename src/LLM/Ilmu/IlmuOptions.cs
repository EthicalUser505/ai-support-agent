namespace LLM.Ilmu;

public sealed class IlmuOptions
{
    public required string ApiKey { get; init; }

    public string BaseUrl { get; init; }
        = "https://api.ilmu.ai/v1";

    public string Model { get; init; }
        = "nemo-super";
}