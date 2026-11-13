namespace QnaIntelligence.Application.Features.Ingestion;

public sealed class DiscoveredQuestionDto
{
    public string Title { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string Technology { get; set; } = string.Empty;

    public DateTime? DiscoveredUtc { get; set; }
}
