namespace QnaIntelligence.Application.Features.Ingestion;

public interface IQnaIngestionService
{
    Task<List<DiscoveredQuestionDto>> GetLatestQuestionsAsync(
        CancellationToken cancellationToken);

    Task<QuestionImportResultDto> ImportLatestQuestionsAsync(
        CancellationToken cancellationToken);
}
