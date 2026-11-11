namespace QnaIntelligence.Infrastructure.Options;

public sealed class AzureOpenAiOptions
{
    public string Endpoint { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;

    public string DeploymentName { get; set; } = string.Empty;
}