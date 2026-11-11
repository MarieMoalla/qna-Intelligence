using QnaIntelligence.Application.Questions.Dtos;

namespace QnaIntelligence.Application.Interfaces;

public interface IAiAnalysisService
{
    Task<QuestionAnalysisDto> AnalyzeAsync(
        string title,
        string content,
        CancellationToken cancellationToken);
}
