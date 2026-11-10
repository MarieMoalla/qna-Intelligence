namespace QnaIntelligence.Application.Questions.Dtos;

public sealed class GetQuestionDetailsDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string Technology { get; set; } = string.Empty;

    public string Tag { get; set; } = string.Empty;

    public bool HasAnswers { get; set; }

    public int PriorityScore { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedUtc { get; set; }

    public DateTime? UpdatedUtc { get; set; }

    public QuestionAnalysisDto? Analysis { get; set; }
}
