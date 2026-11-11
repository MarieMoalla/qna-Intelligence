using QnaIntelligence.Application.Interfaces;
using QnaIntelligence.Application.Questions.Dtos;

namespace QnaIntelligence.Application.Services;

public sealed class MockAiAnalysisService : IAiAnalysisService
{
    public Task<QuestionAnalysisDto> AnalyzeAsync(
        string title,
        string content,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(new QuestionAnalysisDto
        {
            Summary = $"Analysis for {title}",
            RootCauses = "Possible configuration issue",
            MissingInformation = "Environment details missing",
            TroubleshootingSteps = "Check logs and configuration",
            ProposedAnswer = "Verify deployment configuration",
            ConfidenceScore = 0.75
        });
    }
}
