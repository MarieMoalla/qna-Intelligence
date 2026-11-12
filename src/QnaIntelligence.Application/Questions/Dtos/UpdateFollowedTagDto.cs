namespace QnaIntelligence.Application.Questions.Dtos;

public sealed class UpdateFollowedTagDto
{
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}
