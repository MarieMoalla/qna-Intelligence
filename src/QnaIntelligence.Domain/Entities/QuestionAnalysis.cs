namespace QnaIntelligence.Domain.Entities;

public class QuestionAnalysis
{
    public Guid Id { get; set; }

    public Guid QuestionId { get; set; }

    public string Summary { get; set; } = string.Empty;

    public string RootCauses { get; set; } = string.Empty;

    public string MissingInformation { get; set; } = string.Empty;

    public string TroubleshootingSteps { get; set; } = string.Empty;

    public string ProposedAnswer { get; set; } = string.Empty;

    public double ConfidenceScore { get; set; }

    public Question Question { get; set; } = null!;
}
