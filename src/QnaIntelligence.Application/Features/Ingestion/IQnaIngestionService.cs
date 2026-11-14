namespace QnaIntelligence.Application.Features.Ingestion;

public interface IQnaIngestionService
{
    Task<List<DiscoveredQuestionDto>> GetLatestQuestionsAsync(
        CancellationToken cancellationToken);

    Task<ImportResultDto> ImportLatestQuestionsAsync(
        CancellationToken cancellationToken);
}
