using Microsoft.EntityFrameworkCore;
using QnaIntelligence.Domain.Entities;

namespace QnaIntelligence.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Question> Questions => Set<Question>();

    public DbSet<QuestionAnalysis> Analyses => Set<QuestionAnalysis>();

    public DbSet<FollowedTag> FollowedTags => Set<FollowedTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FollowedTag>()
            .Property(followedTag => followedTag.Name)
            .HasMaxLength(200);

        modelBuilder.Entity<FollowedTag>()
            .HasIndex(followedTag => followedTag.Name)
            .IsUnique();
    }
}
