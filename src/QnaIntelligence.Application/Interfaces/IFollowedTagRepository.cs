using QnaIntelligence.Domain.Entities;

namespace QnaIntelligence.Application.Interfaces;

public interface IFollowedTagRepository
{
    Task<List<FollowedTag>> GetAllAsync(CancellationToken cancellationToken);

    Task<FollowedTag?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> NameExistsAsync(string name, Guid? excludingId, CancellationToken cancellationToken);

    Task AddAsync(FollowedTag followedTag, CancellationToken cancellationToken);

    Task UpdateAsync(FollowedTag followedTag, CancellationToken cancellationToken);
}
