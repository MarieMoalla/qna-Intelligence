namespace QnaIntelligence.Application.Questions.Dtos;

public sealed class QuestionDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Technology { get; set; } = string.Empty;

    public string Tag { get; set; } = string.Empty;

    public int PriorityScore { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? Source { get; set; }

    public DateTime? ImportedUtc { get; set; }
}
