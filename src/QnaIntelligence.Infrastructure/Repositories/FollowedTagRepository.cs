using Microsoft.EntityFrameworkCore;
using QnaIntelligence.Application.Interfaces;
using QnaIntelligence.Domain.Entities;
using QnaIntelligence.Infrastructure.Persistence;

namespace QnaIntelligence.Infrastructure.Repositories;

public sealed class FollowedTagRepository(ApplicationDbContext dbContext) : IFollowedTagRepository
{
    public Task<List<FollowedTag>> GetAllAsync(CancellationToken cancellationToken)
    {
        return dbContext.FollowedTags
            .OrderBy(followedTag => followedTag.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<FollowedTag?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.FollowedTags.FirstOrDefaultAsync(
            followedTag => followedTag.Id == id,
            cancellationToken);
    }

    public Task<bool> NameExistsAsync(
        string name,
        Guid? excludingId,
        CancellationToken cancellationToken)
    {
        return dbContext.FollowedTags.AnyAsync(
            followedTag => followedTag.Name == name
                && (excludingId == null || followedTag.Id != excludingId.Value),
            cancellationToken);
    }

    public async Task AddAsync(FollowedTag followedTag, CancellationToken cancellationToken)
    {
        await dbContext.FollowedTags.AddAsync(followedTag, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(FollowedTag followedTag, CancellationToken cancellationToken)
    {
        dbContext.FollowedTags.Update(followedTag);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
