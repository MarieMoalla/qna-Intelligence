using QnaIntelligence.Domain.Enums;

namespace QnaIntelligence.Domain.Entities;

public class Question
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string Technology { get; set; } = string.Empty;

    public string Tag { get; set; } = string.Empty;

    public int PriorityScore { get; set; }

    public bool HasAnswers { get; set; }

    public DateTime CreatedUtc { get; set; }

    public QuestionStatus Status { get; set; } = QuestionStatus.New;

    public QuestionAnalysis? Analysis { get; set; }

    public ICollection<QuestionSource> QuestionSources { get; set; } = [];
}
