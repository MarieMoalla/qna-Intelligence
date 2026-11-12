namespace QnaIntelligence.Domain.Entities;

public class FollowedTag
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime CreatedUtc { get; set; }
}
