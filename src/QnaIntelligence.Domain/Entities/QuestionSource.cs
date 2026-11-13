namespace QnaIntelligence.Domain.Entities;

public class QuestionSource
{
    public Guid Id { get; set; }

    public string SourceId { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public DateTime ImportedUtc { get; set; }

    public Guid? QuestionId { get; set; }

    public Question? Question { get; set; }
}
