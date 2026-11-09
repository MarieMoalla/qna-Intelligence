namespace QnaIntelligence.Application.Questions.Dtos;

public sealed class CreateQuestionDto
{
    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string Technology { get; set; } = string.Empty;

    public string Tag { get; set; } = string.Empty;
}
