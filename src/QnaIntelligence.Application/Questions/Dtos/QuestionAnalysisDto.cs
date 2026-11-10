namespace QnaIntelligence.Application.Questions.Dtos;

public sealed class QuestionAnalysisDto
{
    public string Summary { get; set; } = string.Empty;

    public string RootCauses { get; set; } = string.Empty;

    public string MissingInformation { get; set; } = string.Empty;

    public string TroubleshootingSteps { get; set; } = string.Empty;

    public string ProposedAnswer { get; set; } = string.Empty;

    public double ConfidenceScore { get; set; }
}
