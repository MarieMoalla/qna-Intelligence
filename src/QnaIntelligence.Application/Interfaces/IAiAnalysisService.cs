using QnaIntelligence.Application.Questions.Dtos;

namespace QnaIntelligence.Application.Interfaces;

public interface IAiAnalysisService
{
    Task<QuestionAnalysisDto> AnalyzeAsync(
        Guid questionId,
        string title,
        string content,
        CancellationToken cancellationToken);
}
