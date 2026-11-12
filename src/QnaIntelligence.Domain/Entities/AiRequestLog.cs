namespace QnaIntelligence.Domain.Entities;

public class AiRequestLog
{
    public Guid Id { get; set; }

    public Guid QuestionId { get; set; }

    public DateTime CreatedUtc { get; set; }

    public string Model { get; set; } = string.Empty;

    public string Prompt { get; set; } = string.Empty;

    public string Response { get; set; } = string.Empty;
}
