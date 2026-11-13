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

    public DbSet<AiRequestLog> AiRequestLogs => Set<AiRequestLog>();

    public DbSet<QuestionSource> QuestionSources => Set<QuestionSource>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FollowedTag>()
            .Property(followedTag => followedTag.Name)
            .HasMaxLength(200);

        modelBuilder.Entity<FollowedTag>()
            .HasIndex(followedTag => followedTag.Name)
            .IsUnique();

        modelBuilder.Entity<QuestionSource>()
            .Property(questionSource => questionSource.SourceId)
            .HasMaxLength(200);

        modelBuilder.Entity<QuestionSource>()
            .HasIndex(questionSource => questionSource.SourceId)
            .IsUnique();

        modelBuilder.Entity<QuestionSource>()
            .HasOne(questionSource => questionSource.Question)
            .WithMany(question => question.QuestionSources)
            .HasForeignKey(questionSource => questionSource.QuestionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
